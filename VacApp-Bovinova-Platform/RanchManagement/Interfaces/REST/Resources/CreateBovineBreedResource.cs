using System.ComponentModel.DataAnnotations;

namespace VacApp_Bovinova_Platform.RanchManagement.Interfaces.REST.Resources;

public record CreateBovineBreedResource(
    [Required] string Name,
    [Range(30.0, 45.0)] double MinTemperature,
    [Range(30.0, 45.0)] double MaxTemperature,
    [Range(10, 150)] int MinHeartRate,
    [Range(10, 150)] int MaxHeartRate
);
