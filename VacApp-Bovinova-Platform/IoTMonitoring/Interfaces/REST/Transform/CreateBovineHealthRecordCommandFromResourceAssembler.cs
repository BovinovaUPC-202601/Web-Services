using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Commands;
using VacApp_Bovinova_Platform.IoTMonitoring.Interfaces.REST.Resources;

namespace VacApp_Bovinova_Platform.IoTMonitoring.Interfaces.REST.Transform;

public static class CreateBovineHealthRecordCommandFromResourceAssembler
{
    public static CreateBovineHealthRecordCommand ToCommandFromResource(
        CreateBovineHealthRecordResource resource)
        => new(resource.BovineId, resource.UserId, resource.DeviceId,
               resource.Temperature, resource.HeartRate);
}
