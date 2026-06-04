using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.RanchManagement.Interfaces.REST.Resources;

namespace VacApp_Bovinova_Platform.RanchManagement.Interfaces.REST.Transform;

public static class ProductResourceFromEntityAssembler
{
    public static ProductResource ToResourceFromEntity(Product entity)
    {
        return new ProductResource(
            entity.Id,
            entity.Name,
            entity.CategoryId,
            entity.Quantity,
            entity.UserId,
            entity.ExpirationDate,
            entity.Unit
        );
    }
}