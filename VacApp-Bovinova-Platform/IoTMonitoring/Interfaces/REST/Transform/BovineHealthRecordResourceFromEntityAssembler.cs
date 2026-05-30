using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IoTMonitoring.Interfaces.REST.Resources;

namespace VacApp_Bovinova_Platform.IoTMonitoring.Interfaces.REST.Transform;

public static class BovineHealthRecordResourceFromEntityAssembler
{
    public static BovineHealthRecordResource ToResourceFromEntity(BovineHealthRecord entity)
        => new(entity.Id, entity.BovineId, entity.DeviceId,
               entity.Temperature, entity.HeartRate,
               entity.IsAlert, entity.RecordedAt);
}
