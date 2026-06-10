using Microsoft.EntityFrameworkCore;
using VacApp_Bovinova_Platform.RanchManagement.Application.Internal.CommandServices;
using VacApp_Bovinova_Platform.RanchManagement.Application.Internal.QueryServices;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Repositories;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Services;
using VacApp_Bovinova_Platform.RanchManagement.Infrastructure.Persistence.EFC.Repositories;
using VacApp_Bovinova_Platform.Shared.Domain.Repositories;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Interfaces.ASAP.Configuration;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Configuration;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Repositories;
using VacApp_Bovinova_Platform.StaffAdministration.Application.Internal.CommandServices;
using VacApp_Bovinova_Platform.StaffAdministration.Application.Internal.QueryServices;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Repositories;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Services;
using VacApp_Bovinova_Platform.StaffAdministration.Infrastructure.Persistence.EFC.Repositories;
using VacApp_Bovinova_Platform.CampaignManagement.Application.Internal.CommandServices;
using VacApp_Bovinova_Platform.CampaignManagement.Application.Internal.QueryServices;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Repositories;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Services;
using VacApp_Bovinova_Platform.CampaignManagement.Infrastructure.Repositories;
using VacApp_Bovinova_Platform.IAM.Application.OutBoundServices;
using VacApp_Bovinova_Platform.IAM.Domain.Repositories;
using VacApp_Bovinova_Platform.IAM.Domain.Services;
using VacApp_Bovinova_Platform.IAM.Infrastructure.Repositories;
using VacApp_Bovinova_Platform.IAM.Application.CommandServices;
using VacApp_Bovinova_Platform.IAM.Infrastructure.Hashing.BCrypt.Services;
using VacApp_Bovinova_Platform.IAM.Infrastructure.Tokens.JWT.Services;
using VacApp_Bovinova_Platform.IAM.Infrastructure.Tokens.JWT.Configuration;
using Microsoft.OpenApi.Models;
using VacApp_Bovinova_Platform.IAM.Application.QueryServices;
using dotenv.net;
using VacApp_Bovinova_Platform.Shared.Application.OutboundServices;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Media.Cloudinary;
using VacApp_Bovinova_Platform.AlertManagement.Application.Internal.CommandServices;
using VacApp_Bovinova_Platform.AlertManagement.Application.Internal.QueryServices;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Repositories;
using VacApp_Bovinova_Platform.AlertManagement.Domain.Services;
using VacApp_Bovinova_Platform.AlertManagement.Infrastructure.Persistence.EFC.Repositories;
using VacApp_Bovinova_Platform.IAM.Infrastructure.Pipeline.Middleware.Extensions;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Pipeline.Middleware.Extensions;
using VacApp_Bovinova_Platform.IoTMonitoring.Application.Internal.CommandServices;
using VacApp_Bovinova_Platform.IoTMonitoring.Application.Internal.QueryServices;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Repositories;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Services;
using VacApp_Bovinova_Platform.IoTMonitoring.Infrastructure.Persistence.EFC.Repositories;
using VacApp_Bovinova_Platform.IoTMonitoring.Infrastructure.Messaging;
using VacApp_Bovinova_Platform.IoTMonitoring.Application.ACL;
using VacApp_Bovinova_Platform.IoTMonitoring.Infrastructure.ACL;
using VacApp_Bovinova_Platform.SubscriptionManagement.Application.Internal.CommandServices;
using VacApp_Bovinova_Platform.SubscriptionManagement.Application.Internal.QueryServices;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Repositories;
using VacApp_Bovinova_Platform.SubscriptionManagement.Domain.Services;
using VacApp_Bovinova_Platform.SubscriptionManagement.Infrastructure.Persistence.EFC.Repositories;
using VacApp_Bovinova_Platform.SubscriptionManagement.Application.ACL;
using VacApp_Bovinova_Platform.SubscriptionManagement.Infrastructure.ACL;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Media.Local;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Repositories;
using VacApp_Bovinova_Platform.AIAssistant.Domain.Services;
using VacApp_Bovinova_Platform.AIAssistant.Application.ACL;
using VacApp_Bovinova_Platform.AIAssistant.Application.Internal.CommandServices;
using VacApp_Bovinova_Platform.AIAssistant.Application.Internal.QueryServices;
using VacApp_Bovinova_Platform.AIAssistant.Infrastructure.ACL;
using VacApp_Bovinova_Platform.AIAssistant.Infrastructure.AI.Clients;
using VacApp_Bovinova_Platform.AIAssistant.Infrastructure.AI.Configuration;
using VacApp_Bovinova_Platform.AIAssistant.Infrastructure.AI.Services;
using VacApp_Bovinova_Platform.AIAssistant.Infrastructure.Persistence.EFC.Repositories;

DotEnv.Load();

// Force invariant culture so decimal model binding (e.g. "39.3") is parsed with a
// dot separator regardless of the host OS locale (es_ES treats "." as thousands,
// turning 39.3 into 393 and breaking [Range] validation on temperatures).
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//configure Lower Case URLs
builder.Services.AddRouting(options => options.LowercaseUrls = true);

// Configure Kebab Case Route Naming Convention
builder.Services.AddControllers(options => options.Conventions.Add(new KebabCaseRouteNamingConvention()));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(
    c =>
    {
        c.EnableAnnotations();

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Id = "Bearer",
                        Type = ReferenceType.SecurityScheme
                    }
                },
                Array.Empty<string>()
            }
        });
    });

/////////////////////////Begin Database Configuration/////////////////////////

var connectionString = $"Server={Environment.GetEnvironmentVariable("DB_HOST")};" +
                       $"Port={Environment.GetEnvironmentVariable("DB_PORT")};" +
                       $"Database={Environment.GetEnvironmentVariable("DB_NAME")};" +
                       $"User={Environment.GetEnvironmentVariable("DB_USER")};" +
                       $"Password={Environment.GetEnvironmentVariable("DB_PASS")};" +
                       $"SslMode={Environment.GetEnvironmentVariable("DB_SSL_MODE") ?? "none"};" +
                       $"AllowPublicKeyRetrieval={Environment.GetEnvironmentVariable("DB_ALLOW_PUBLIC_KEY_RETRIEVAL") ?? "false"};";

// Verify Database Connection string
if (string.IsNullOrEmpty(connectionString))
    throw new Exception("Database connection string is not set in .env");

// Configure Database Context and Logging Levels
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseMySQL(connectionString)
               .EnableSensitiveDataLogging()
               .EnableDetailedErrors()
    );
}
else if (builder.Environment.IsProduction())
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseMySQL(connectionString)
               .LogTo(Console.WriteLine, LogLevel.Error)
               .EnableDetailedErrors()
    );
}

// Add CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllPolicy",
        policy => policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Configure Dependency Injection

// Shared Bounded Context Injection Configuration
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IMediaStorageService>(_ =>
{
    var cloudinaryUrl = Environment.GetEnvironmentVariable("CLOUDINARY_URL");
    var useLocalMediaStorage = string.IsNullOrWhiteSpace(cloudinaryUrl) ||
                               cloudinaryUrl.Contains("dummy", StringComparison.OrdinalIgnoreCase);

    return useLocalMediaStorage ? new LocalMediaStorageService() : new CloudinaryService();
});

//IAM
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserCommandService, UserCommandService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();
builder.Services.AddScoped<IHashingService, HashingService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection("TokenSettings"));

//Ranch Management BC
builder.Services.AddScoped<IBovineBreedRepository, BovineBreedRepository>();

builder.Services.AddScoped<IBovineRepository, BovineRepository>();
builder.Services.AddScoped<IBovineQueryService, BovineQueryService>();
builder.Services.AddScoped<IBovineCommandService, BovineCommandService>();

builder.Services.AddScoped<IStableRepository, StableRepository>();
builder.Services.AddScoped<IStableQueryService, StableQueryService>();
builder.Services.AddScoped<IStableCommandService, StableCommandService>();

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryQueryService, CategoryQueryService>();
builder.Services.AddScoped<ICategoryCommandService, CategoryCommandService>();

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductQueryService, ProductQueryService>();
builder.Services.AddScoped<IProductCommandService, ProductCommandService>();

//Staff Administration BC
builder.Services.AddScoped<IStaffRepository, StaffRepository>();
builder.Services.AddScoped<IStaffQueryService, StaffQueryService>();
builder.Services.AddScoped<IStaffCommandService, StaffCommandService>();

//Campaign Management BC
builder.Services.AddScoped<ICampaignRepository, CampaignRepository>();
builder.Services.AddScoped<ICampaignCommandService, CampaignCommandService>();
builder.Services.AddScoped<ICampaignQueryService, CampaignQueryService>();

//IoT Monitoring BC
builder.Services.AddScoped<IBovineHealthRecordRepository, BovineHealthRecordRepository>();
builder.Services.AddScoped<IBovineHealthRecordCommandService, BovineHealthRecordCommandService>();
builder.Services.AddScoped<IBovineHealthRecordQueryService, BovineHealthRecordQueryService>();
builder.Services.AddScoped<ICollarRepository, CollarRepository>();
builder.Services.AddScoped<ICollarCommandService, CollarCommandService>();
builder.Services.AddScoped<ICollarQueryService, CollarQueryService>();
builder.Services.AddScoped<ISubscriptionContextFacade, SubscriptionContextFacade>();

// Subscription Management BC
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddScoped<IAdditionalCollarRequestRepository, AdditionalCollarRequestRepository>();
builder.Services.AddScoped<ISubscriptionCommandService, SubscriptionCommandService>();
builder.Services.AddScoped<ISubscriptionQueryService, SubscriptionQueryService>();
builder.Services.AddScoped<ICollarLifecycleFacade, CollarLifecycleFacade>();

// MQTT telemetry ingestion (CON2: collar communicates exclusively over MQTT)
builder.Services.AddSingleton(new MqttSettings
{
    Host = Environment.GetEnvironmentVariable("MQTT_HOST") ?? "localhost",
    Port = int.TryParse(Environment.GetEnvironmentVariable("MQTT_PORT"), out var mqttPort) ? mqttPort : 1883,
    Username = Environment.GetEnvironmentVariable("MQTT_USERNAME") ?? string.Empty,
    Password = Environment.GetEnvironmentVariable("MQTT_PASSWORD") ?? string.Empty,
    ClientId = Environment.GetEnvironmentVariable("MQTT_CLIENT_ID") ?? "vacapp-backend",
    TelemetryTopic = Environment.GetEnvironmentVariable("MQTT_TELEMETRY_TOPIC") ?? "vacapp/telemetry",
    ResponseTopicPrefix = Environment.GetEnvironmentVariable("MQTT_RESPONSE_TOPIC_PREFIX") ?? "vacapp/telemetry/response",
    UseTls = bool.TryParse(Environment.GetEnvironmentVariable("MQTT_USE_TLS"), out var mqttTls) && mqttTls,
    AllowUntrustedCertificates = bool.TryParse(Environment.GetEnvironmentVariable("MQTT_TLS_ALLOW_UNTRUSTED"), out var mqttUntrusted) && mqttUntrusted
});
builder.Services.AddHostedService<MqttTelemetryConsumer>();

//Alert Management BC
builder.Services.AddScoped<IAlertRepository, AlertRepository>();
builder.Services.AddScoped<IAlertCommandService, AlertCommandService>();
builder.Services.AddScoped<IAlertQueryService, AlertQueryService>();

//MediatR — scans all handlers in this assembly
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

//AI Assistant BC
builder.Services.Configure<LocalModelSettings>(options =>
{
    var configuredSettings = builder.Configuration.GetSection("LocalModelSettings").Get<LocalModelSettings>() ??
                             new LocalModelSettings();
    options.BaseUrl = Environment.GetEnvironmentVariable("LM_STUDIO_BASE_URL") ?? configuredSettings.BaseUrl;
    options.Model = Environment.GetEnvironmentVariable("LM_STUDIO_MODEL") ?? configuredSettings.Model;
    options.ApiKey = Environment.GetEnvironmentVariable("LM_STUDIO_API_KEY") ?? configuredSettings.ApiKey;
    options.Temperature = configuredSettings.Temperature;
});
builder.Services.AddHttpClient<ILocalModelClient, OpenAICompatibleModelClient>();
builder.Services.AddScoped<IAIChatService, LmStudioChatService>();
builder.Services.AddScoped<IAIVisionService, LmStudioVisionService>();
builder.Services.AddScoped<IAISessionRepository, AISessionRepository>();
builder.Services.AddScoped<IBovineAnalysisRepository, BovineAnalysisRepository>();
builder.Services.AddScoped<IRanchContextFacade, RanchContextFacade>();
builder.Services.AddScoped<IAlertContextFacade, AlertContextFacade>();
builder.Services.AddScoped<IIoTContextFacade, IoTContextFacade>();
builder.Services.AddScoped<IAIAssistantCommandService, AIAssistantCommandService>();
builder.Services.AddScoped<IAIAssistantQueryService, AIAssistantQueryService>();


/////////////////////////End Database Configuration/////////////////////////
var app = builder.Build();

// Verify Database Objects are created
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAllPolicy");
// Must sit after CORS so error responses keep their CORS headers, and before the
// auth middleware so its 401s are mapped instead of bubbling up as 500s.
app.UseRequestExceptionHandling();
app.UseRequestAuthorization();
app.UseHttpsRedirection();
//app.UseAuthentication();
//app.UseAuthorization();
app.MapControllers();
app.Run();
