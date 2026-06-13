using System.Text;
using System.Text.Json;
using MQTTnet;
using MQTTnet.Client;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Commands;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Repositories;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Services;

namespace VacApp_Bovinova_Platform.IoTMonitoring.Infrastructure.Messaging;

/// <summary>
/// Background service that subscribes to the MQTT telemetry topic and feeds every
/// incoming reading into the IoTMonitoring command pipeline — the same one the old
/// HTTP endpoint used. This is the single ingestion path for collar telemetry
/// (constraint CON2: communication with the collar is exclusively over MQTT).
///
/// Flow: collar publishes -> broker -> this consumer -> BovineHealthRecordCommandService
///       -> aggregate evaluates -> (if abnormal) AbnormalTelemetryDetectedEvent
///       -> AlertManagement creates the alert. This consumer also publishes the
///       result back to a device-specific response topic so the collar can drive its LED.
/// </summary>
public class MqttTelemetryConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly MqttSettings _settings;
    private readonly ILogger<MqttTelemetryConsumer> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private IMqttClient? _client;

    public MqttTelemetryConsumer(
        IServiceScopeFactory scopeFactory,
        MqttSettings settings,
        ILogger<MqttTelemetryConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = settings;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new MqttFactory();
        _client = factory.CreateMqttClient();

        var options = BuildOptions();

        _client.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;

        // Keep trying to (re)connect until the application stops.
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!_client.IsConnected)
                {
                    _logger.LogInformation("Connecting to MQTT broker {Host}:{Port}…", _settings.Host, _settings.Port);
                    await _client.ConnectAsync(options, stoppingToken);

                    await _client.SubscribeAsync(
                        factory.CreateSubscribeOptionsBuilder()
                            .WithTopicFilter(f => f.WithTopic(_settings.TelemetryTopic))
                            .Build(),
                        stoppingToken);

                    _logger.LogInformation("Subscribed to topic '{Topic}'.", _settings.TelemetryTopic);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MQTT connection failed. Retrying in 5s…");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private MqttClientOptions BuildOptions()
    {
        var builder = new MqttClientOptionsBuilder()
            .WithTcpServer(_settings.Host, _settings.Port)
            .WithClientId($"{_settings.ClientId}-{Guid.NewGuid():N}")
            .WithCleanSession();

        if (_settings.HasCredentials)
            builder = builder.WithCredentials(_settings.Username, _settings.Password);

        if (_settings.UseTls)
        {
            builder = builder.WithTlsOptions(o =>
            {
                o.UseTls(true);
                if (_settings.AllowUntrustedCertificates)
                {
                    o.WithAllowUntrustedCertificates(true);
                    o.WithIgnoreCertificateChainErrors(true);
                    o.WithIgnoreCertificateRevocationErrors(true);
                }
            });
        }

        return builder.Build();
    }

    private async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
    {
        var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
        _logger.LogInformation("Telemetry received: {Payload}", payload);

        TelemetryMessage? message;
        try
        {
            message = JsonSerializer.Deserialize<TelemetryMessage>(payload, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Discarding malformed telemetry payload.");
            return;
        }

        if (message is null || string.IsNullOrWhiteSpace(message.DeviceId))
        {
            _logger.LogWarning("Discarding telemetry without a deviceId.");
            return;
        }

        // The command pipeline is scoped (DbContext, repositories), so open a scope per message.
        using var scope = _scopeFactory.CreateScope();

        // Resolve bovine/owner from the collar registered for this device — the device
        // only sends its deviceId, so the binding lives server-side (single source of truth).
        var collarRepository = scope.ServiceProvider.GetRequiredService<ICollarRepository>();
        var collar = await collarRepository.FindByDeviceIdAsync(message.DeviceId);

        if (collar is null)
        {
            _logger.LogWarning(
                "Discarding telemetry from unregistered device {DeviceId}. Register the collar in the app first.",
                message.DeviceId);
            return;
        }

        if (!collar.IsActive)
        {
            _logger.LogWarning(
                "Discarding telemetry from device {DeviceId}: collar is {Status}, not Active.",
                message.DeviceId, collar.LifecycleStatus);
            return;
        }

        var commandService = scope.ServiceProvider.GetRequiredService<IBovineHealthRecordCommandService>();

        var record = await commandService.Handle(new CreateBovineHealthRecordCommand(
            BovineId:     collar.BovineId,
            UserId:       collar.UserId,
            DeviceId:     message.DeviceId,
            Temperature:  message.Temperature,
            HeartRate:    message.HeartRate,
            BatteryLevel: message.BatteryLevel));

        if (record is null)
        {
            _logger.LogError("Could not persist telemetry from device {DeviceId}.", message.DeviceId);
            return;
        }

        await PublishResponseAsync(message.DeviceId, new TelemetryResponse
        {
            Id      = record.Id,
            IsAlert = record.IsAlert,
            Message = record.IsAlert
                ? "ALERT: vital signs outside normal bovine range."
                : "OK: vital signs within normal range."
        });
    }

    private async Task PublishResponseAsync(string deviceId, TelemetryResponse response)
    {
        if (_client is null || !_client.IsConnected) return;

        var topic = $"{_settings.ResponseTopicPrefix}/{deviceId}";
        var payload = JsonSerializer.Serialize(response, JsonOptions);

        await _client.PublishAsync(new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .Build());

        _logger.LogInformation("Response published to '{Topic}': {Payload}", topic, payload);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_client is { IsConnected: true })
            await _client.DisconnectAsync();

        _client?.Dispose();
        await base.StopAsync(cancellationToken);
    }
}
