using System.ComponentModel.DataAnnotations;

namespace VacApp_Bovinova_Platform.RanchManagement.Interfaces.REST.Resources;

public record CreateProductResource(
    [Required] string Name,
    int CategoryId,
    int Quantity,
    DateOnly? ExpirationDate,
    string? Unit = null);