using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Queries;

namespace VacApp_Bovinova_Platform.IoTMonitoring.Domain.Services;

public interface IBovineHealthRecordQueryService
{
    Task<IEnumerable<BovineHealthRecord>> Handle(GetHealthRecordsByBovineIdQuery query);
    Task<BovineHealthRecord?>             Handle(GetLatestHealthRecordByBovineIdQuery query);
}
