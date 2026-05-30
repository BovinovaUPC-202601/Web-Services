using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.RanchManagement.Interfaces.REST.Resources;

namespace VacApp_Bovinova_Platform.RanchManagement.Interfaces.REST.Transform;

public static class UpdateCategoryCommandFromResourceAssembler
{
    public static UpdateCategoryCommand ToCommandFromResource(int id, UpdateCategoryResource resource)
    {
        return new UpdateCategoryCommand(id, resource.Name);
    }
}
