using MediatR;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Events;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Commands;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Repositories;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Services;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Queries;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Services;
using VacApp_Bovinova_Platform.Shared.Domain.Model;
using VacApp_Bovinova_Platform.Shared.Domain.Repositories;

namespace VacApp_Bovinova_Platform.IoTMonitoring.Application.Internal.CommandServices;

public class BovineHealthRecordCommandService(
    IBovineHealthRecordRepository repository,
    IUnitOfWork unitOfWork,
    IMediator mediator,
    IBovineQueryService bovineQueryService)
    : IBovineHealthRecordCommandService
{
    public async Task<BovineHealthRecord?> Handle(CreateBovineHealthRecordCommand command)
    {
        var record = new BovineHealthRecord(command);

        // Evaluate the reading against THIS bovine's configured thresholds
        // (ACL into RanchManagement). Missing/unset values fall back to the
        // shared defaults so legacy bovines still raise sensible alerts.
        var bovine  = await bovineQueryService.Handle(new GetBovinesByIdQuery(command.BovineId));
        var minTemp = BovineVitalRanges.Resolve(bovine?.MinTemperature ?? 0, BovineVitalRanges.MinTemperature);
        var maxTemp = BovineVitalRanges.Resolve(bovine?.MaxTemperature ?? 0, BovineVitalRanges.MaxTemperature);
        var minHr   = BovineVitalRanges.Resolve(bovine?.MinHeartRate   ?? 0, BovineVitalRanges.MinHeartRate);
        var maxHr   = BovineVitalRanges.Resolve(bovine?.MaxHeartRate   ?? 0, BovineVitalRanges.MaxHeartRate);
        record.EvaluateAlert(minTemp, maxTemp, minHr, maxHr);

        try
        {
            await repository.AddAsync(record);
            await unitOfWork.CompleteAsync();
        }
        catch (Exception)
        {
            return null;
        }

        // Publish domain event — AlertManagement will react to this. The
        // thresholds travel with the event so the alert wording/urgency match
        // the bovine's own range.
        if (record.IsAlert)
        {
            await mediator.Publish(new AbnormalTelemetryDetectedEvent(
                BovineId:       record.BovineId,
                UserId:         command.UserId,
                Temperature:    record.Temperature,
                HeartRate:      record.HeartRate,
                DeviceId:       record.DeviceId,
                MinTemperature: minTemp,
                MaxTemperature: maxTemp,
                MinHeartRate:   minHr,
                MaxHeartRate:   maxHr
            ));
        }

        return record;
    }
}
