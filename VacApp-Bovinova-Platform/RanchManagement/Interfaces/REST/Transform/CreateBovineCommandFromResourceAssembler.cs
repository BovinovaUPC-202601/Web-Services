using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.RanchManagement.Interfaces.REST.Resources;

public static class CreateBovineCommandFromResourceAssembler
{
    public static CreateBovineCommand ToCommandFromResource(CreateBovineResource resource, int userId)
    {
        return new CreateBovineCommand(
            resource.Name,
            resource.Gender,
            resource.BirthDate,
            resource.Breed,
            resource.StableId,
            string.Empty,
            userId,
            resource.FileData.OpenReadStream(),
            resource.MinTemperature,
            resource.MaxTemperature,
            resource.MinHeartRate,
            resource.MaxHeartRate
        );
    }
}