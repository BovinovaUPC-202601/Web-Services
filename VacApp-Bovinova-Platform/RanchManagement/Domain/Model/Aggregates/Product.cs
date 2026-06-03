using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Commands;

namespace VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Aggregates;

public class Product
{
    [Key]
    public int Id { get; private set; }

    [Required]
    [StringLength(100)]
    public string Name { get; private set; }

    [Required]
    public int CategoryId { get; private set; }

    [ForeignKey(nameof(CategoryId))]
    public Category? Category { get; private set; }

    [Required]
    public int Quantity { get; private set; }

    [Required]
    public int UserId { get; private set; }

    public DateOnly? ExpirationDate { get; private set; }
    public string? Unit { get; private set; }

    private Product() { Name = string.Empty; }

    public Product(string name, int categoryId, int quantity, int userId, DateOnly? expirationDate = null, string? unit = null)
    {
        Name = name;
        CategoryId = categoryId;
        Quantity = quantity;
        UserId = userId;
        ExpirationDate = expirationDate;
        Unit = unit;
    }

    public Product(CreateProductCommand command)
    {
        Name = command.Name;
        CategoryId = command.CategoryId;
        Quantity = command.Quantity;
        UserId = command.UserId;
        ExpirationDate = command.ExpirationDate;
        Unit = command.Unit;
    }

    public void Update(UpdateProductCommand command)
    {
        Name = command.Name;
        CategoryId = command.CategoryId;
        Quantity = command.Quantity;
        ExpirationDate = command.ExpirationDate;
        Unit = command.Unit;
    }
}
