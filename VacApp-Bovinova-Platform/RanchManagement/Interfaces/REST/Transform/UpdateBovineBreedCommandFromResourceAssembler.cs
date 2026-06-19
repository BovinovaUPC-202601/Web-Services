using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.RanchManagement.Interfaces.REST.Resources;

namespace VacApp_Bovinova_Platform.RanchManagement.Interfaces.REST.Transform;

public static class UpdateBovineBreedCommandFromResourceAssembler
{
    public static UpdateBovineBreedCommand ToCommandFromResource(int id, UpdateBovineBreedResource resource)
    {
        return new UpdateBovineBreedCommand(
            id,
            resource.Name,
            resource.MinTemperature,
            resource.MaxTemperature,
            resource.MinHeartRate,
            resource.MaxHeartRate
        );
    }
}
