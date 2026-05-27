using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Commands;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Repositories;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Services;
using VacApp_Bovinova_Platform.Shared.Domain.Repositories;

namespace VacApp_Bovinova_Platform.IoTMonitoring.Application.Internal.CommandServices;

public class BovineHealthRecordCommandService(
    IBovineHealthRecordRepository repository,
    IUnitOfWork unitOfWork)
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
        return record;
    }
}
