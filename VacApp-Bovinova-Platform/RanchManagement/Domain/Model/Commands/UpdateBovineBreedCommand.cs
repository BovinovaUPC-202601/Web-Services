namespace VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Commands;

public record UpdateBovineBreedCommand(
    int Id,
    string Name,
    double MinTemperature,
    double MaxTemperature,
    int MinHeartRate,
    int MaxHeartRate
);
