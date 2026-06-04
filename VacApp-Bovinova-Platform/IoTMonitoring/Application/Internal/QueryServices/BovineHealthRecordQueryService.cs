using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Queries;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Repositories;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Services;

namespace VacApp_Bovinova_Platform.IoTMonitoring.Application.Internal.QueryServices;

public class BovineHealthRecordQueryService(IBovineHealthRecordRepository repository)
    : IBovineHealthRecordQueryService
{
    public async Task<IEnumerable<BovineHealthRecord>> Handle(GetHealthRecordsByBovineIdQuery query)
        => await repository.FindByBovineIdAsync(query.BovineId, query.UserId);

    public async Task<BovineHealthRecord?> Handle(GetLatestHealthRecordByBovineIdQuery query)
        => await repository.FindLatestByBovineIdAsync(query.BovineId, query.UserId);
}
