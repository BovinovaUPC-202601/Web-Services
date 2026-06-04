using System.ComponentModel.DataAnnotations;

namespace VacApp_Bovinova_Platform.RanchManagement.Interfaces.REST.Resources;

public class UpdateBovineResource
{
    /*
    string Name,
    string Gender,
    DateTime? BirthDate,
    string? Breed,
    string? Location,
    string? BovineImg,
    int? StableId
     */

    public string Name { get; set; }
    public string Gender { get; set; }
    public DateOnly BirthDate { get; set; }
    public string Breed { get; set; }
    public int StableId { get; set; }
    [Range(30.0, 45.0, ErrorMessage = "La temperatura debe estar entre 30°C y 45°C")]
    public double? MinTemperature { get; set; }

    [Range(30.0, 45.0, ErrorMessage = "La temperatura debe estar entre 30°C y 45°C")]
    public double? MaxTemperature { get; set; }

    [Range(10, 150, ErrorMessage = "El ritmo cardíaco debe ser entre 10 y 150 BPM")]
    public int? MinHeartRate { get; set; }

    [Range(10, 150, ErrorMessage = "El ritmo cardíaco debe ser entre 10 y 150 BPM")]
    public int? MaxHeartRate { get; set; }
}