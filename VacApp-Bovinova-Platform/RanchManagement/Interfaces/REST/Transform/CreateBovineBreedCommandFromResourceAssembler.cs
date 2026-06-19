using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.RanchManagement.Interfaces.REST.Resources;

namespace VacApp_Bovinova_Platform.RanchManagement.Interfaces.REST.Transform;

public static class CreateBovineBreedCommandFromResourceAssembler
{
    public static CreateBovineBreedCommand ToCommandFromResource(CreateBovineBreedResource resource, int? userId)
    {
        return new CreateBovineBreedCommand(
            resource.Name,
            resource.MinTemperature,
            resource.MaxTemperature,
            resource.MinHeartRate,
            resource.MaxHeartRate,
            userId
        );
    }
}
