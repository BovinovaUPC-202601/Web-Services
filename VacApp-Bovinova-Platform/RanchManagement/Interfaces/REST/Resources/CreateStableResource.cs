using System.ComponentModel.DataAnnotations;

namespace VacApp_Bovinova_Platform.RanchManagement.Interfaces.REST.Resources;

public record CreateStableResource(
    [Required] string Name,
    int Limit/*,
    List<BovineResource> Bovines*/);