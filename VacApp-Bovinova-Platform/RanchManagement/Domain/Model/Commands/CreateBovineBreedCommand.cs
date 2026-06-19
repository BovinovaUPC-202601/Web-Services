namespace VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Commands;

public record CreateBovineBreedCommand(
    string Name,
    double MinTemperature,
    double MaxTemperature,
    int MinHeartRate,
    int MaxHeartRate,
    int? UserId
);
