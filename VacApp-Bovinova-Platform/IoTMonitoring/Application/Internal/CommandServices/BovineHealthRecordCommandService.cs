using MediatR;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Events;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Commands;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Repositories;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Services;
using VacApp_Bovinova_Platform.Shared.Domain.Repositories;

namespace VacApp_Bovinova_Platform.IoTMonitoring.Application.Internal.CommandServices;

public class BovineHealthRecordCommandService(
    IBovineHealthRecordRepository repository,
    IUnitOfWork unitOfWork,
    IMediator mediator)
    : IBovineHealthRecordCommandService
{
    public async Task<BovineHealthRecord?> Handle(CreateBovineHealthRecordCommand command)
    {
        var record = new BovineHealthRecord(command);
        try
        {
            await repository.AddAsync(record);
            await unitOfWork.CompleteAsync();
        }
        catch (Exception)
        {
            return null;
        }

        // Publish domain event — AlertManagement will react to this
        if (record.IsAlert)
        {
            await mediator.Publish(new AbnormalTelemetryDetectedEvent(
                BovineId:    record.BovineId,
                UserId:      command.UserId,
                Temperature: record.Temperature,
                HeartRate:   record.HeartRate,
                DeviceId:    record.DeviceId
            ));
        }

        return record;
    }
}
