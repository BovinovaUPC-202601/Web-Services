namespace VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Commands;

public record CreateProductCommand(string Name, int CategoryId, int Quantity, int UserId, DateOnly? ExpirationDate, string? Unit = null);
