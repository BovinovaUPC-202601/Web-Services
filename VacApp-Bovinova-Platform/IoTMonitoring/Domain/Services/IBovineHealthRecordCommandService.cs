using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Commands;

namespace VacApp_Bovinova_Platform.IoTMonitoring.Domain.Services;

public interface IBovineHealthRecordCommandService
{
    Task<BovineHealthRecord?> Handle(CreateBovineHealthRecordCommand command);
}
