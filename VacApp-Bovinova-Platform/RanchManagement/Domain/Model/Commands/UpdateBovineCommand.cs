namespace VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Commands;

public record UpdateBovineCommand(
    int Id,
    string? Name = null,
    string? Gender = null,
    DateOnly? BirthDate = null,
    string? Breed = null,
    int? UserId = null,
    int? StableId = null,
    double? MinTemperature = null,
    double? MaxTemperature = null,
    int? MinHeartRate = null,
    int? MaxHeartRate = null
);
