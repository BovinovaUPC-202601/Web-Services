using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace VacApp_Bovinova_Platform.RanchManagement.Interfaces.REST.Resources;

public record CreateBovineResource(
    string Name,
    string Gender,
    DateOnly BirthDate,
    string Breed,
    [Required]
    IFormFile FileData,
    int StableId);