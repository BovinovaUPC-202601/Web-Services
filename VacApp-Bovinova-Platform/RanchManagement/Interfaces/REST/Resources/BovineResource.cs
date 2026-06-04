namespace VacApp_Bovinova_Platform.RanchManagement.Interfaces.REST.Resources;

public record BovineResource(
    int Id,
    string Name,
    string Gender,
    DateOnly BirthDate,
    string Breed,
    string BovineImg,
    int StableId,
    double MinTemperature,
    double MaxTemperature,
    int MinHeartRate,
    int MaxHeartRate);