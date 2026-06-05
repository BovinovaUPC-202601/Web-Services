using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IoTMonitoring.Interfaces.REST.Resources;

namespace VacApp_Bovinova_Platform.IoTMonitoring.Interfaces.REST.Transform;

public static class CollarResourceFromEntityAssembler
{
    public static CollarResource ToResourceFromEntity(Collar collar)
        => new(collar.Id, collar.DeviceId, collar.BovineId, collar.RegisteredAt, collar.IsActive);
}
