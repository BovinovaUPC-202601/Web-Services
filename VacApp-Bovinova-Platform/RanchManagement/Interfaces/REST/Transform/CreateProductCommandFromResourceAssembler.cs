using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.RanchManagement.Interfaces.REST.Resources;

namespace VacApp_Bovinova_Platform.RanchManagement.Interfaces.REST.Transform;

public static class CreateProductCommandFromResourceAssembler
{
    public static CreateProductCommand ToCommandFromResource(CreateProductResource resource, int userId)
    {
        return new CreateProductCommand(
            resource.Name,
            resource.CategoryId,
            resource.Quantity,
            userId,
            resource.ExpirationDate,
            resource.Unit
        );
    }
}