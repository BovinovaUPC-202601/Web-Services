namespace VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Commands;

public record UpdateProductCommand(int Id, string Name, int CategoryId, int Quantity, DateOnly? ExpirationDate, string? Unit = null);
