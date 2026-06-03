namespace VacApp_Bovinova_Platform.RanchManagement.Interfaces.REST.Resources;

public record ProductResource(
    int Id,
    string Name,
    int CategoryId,
    int Quantity,
    int UserId,
    DateOnly? ExpirationDate,
    string? Unit = null);