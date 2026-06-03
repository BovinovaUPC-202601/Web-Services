using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.RanchManagement.Interfaces.REST.Resources;

namespace VacApp_Bovinova_Platform.RanchManagement.Interfaces.REST.Transform;

public static class UpdateProductCommandFromResourceAssembler
{
    public static UpdateProductCommand ToCommandFromResource(int id, UpdateProductResource resource)
    {
        return new UpdateProductCommand(
            id,
            resource.Name,
            resource.CategoryId,
            resource.Quantity,
            resource.ExpirationDate,
            resource.Unit
        );
    }
}
