namespace VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Commands;

public record CreateBovineCommand(
    string Name,
    string Gender,
    DateOnly BirthDate,
    string Breed,
    int StableId,
    string BovineImg,
    int UserId,
    Stream FileData,
    double MinTemperature = 0,
    double MaxTemperature = 0,
    int MinHeartRate = 0,
    int MaxHeartRate = 0
    );