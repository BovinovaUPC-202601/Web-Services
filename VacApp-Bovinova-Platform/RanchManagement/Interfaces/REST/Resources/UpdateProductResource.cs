namespace VacApp_Bovinova_Platform.RanchManagement.Interfaces.REST.Resources;

public record UpdateProductResource(
    string Name,
    int CategoryId,
    int Quantity,
    DateOnly? ExpirationDate,
    string? Unit = null);
