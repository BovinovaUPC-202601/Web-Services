using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Entities;

namespace VacApp_Bovinova_Platform.RanchManagement.Domain.Services;

public interface IBovineBreedCommandService
{
    Task<BovineBreed?> Handle(CreateBovineBreedCommand command);
    Task<BovineBreed?> Handle(UpdateBovineBreedCommand command);
    Task<BovineBreed?> Handle(DeleteBovineBreedCommand command);
}
