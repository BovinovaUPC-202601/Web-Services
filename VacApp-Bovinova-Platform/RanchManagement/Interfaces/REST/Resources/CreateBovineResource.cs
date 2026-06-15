using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace VacApp_Bovinova_Platform.RanchManagement.Interfaces.REST.Resources;

public record CreateBovineResource(
    [Required] string Name,
    [Required] string Gender,
    DateOnly BirthDate,
    [Required] string Breed,
    [Required]
    IFormFile FileData,
    int StableId,
    [Range(30.0, 45.0, ErrorMessage = "La temperatura debe estar entre 30°C y 45°C")]
    double MinTemperature = 38.0,

    [Range(30.0, 45.0, ErrorMessage = "La temperatura debe estar entre 30°C y 45°C")]
    double MaxTemperature = 39.3,

    [Range(10, 150, ErrorMessage = "El ritmo cardíaco debe ser entre 10 y 150 BPM")]
    int MinHeartRate = 40,

    [Range(10, 150, ErrorMessage = "El ritmo cardíaco debe ser entre 10 y 150 BPM")]
    int MaxHeartRate = 80
);