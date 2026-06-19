using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Entities;
using VacApp_Bovinova_Platform.RanchManagement.Interfaces.REST.Resources;

namespace VacApp_Bovinova_Platform.RanchManagement.Interfaces.REST.Transform
{
    public class BovineBreedResourceFromEntityAssembler
    {
        static public BovineBreedResource ToResourceFromEntity(BovineBreed breed)
        {
            return new BovineBreedResource(
                breed.Id,
                breed.Name,
                breed.MinTemperature,
                breed.MaxTemperature,
                breed.MinHeartRate,
                breed.MaxHeartRate,
                breed.UserId
            );
        }
    }
}