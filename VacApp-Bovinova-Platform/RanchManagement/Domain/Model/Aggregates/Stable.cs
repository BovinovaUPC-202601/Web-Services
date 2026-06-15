using System.ComponentModel.DataAnnotations;
using VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Commands;
using ValidationException = VacApp_Bovinova_Platform.Shared.Domain.Model.Exceptions.ValidationException;

namespace VacApp_Bovinova_Platform.RanchManagement.Domain.Model.Aggregates;

public class Stable
{
    [Required]
    public int Id { get; private set; }

    [Required]
    [StringLength(50)]
    public string Name { get; private set; }

    [Required]
    public int Limit { get; private set; }

    public int UserId { get; set; }

    // Default constructor for EF Core
    private Stable()
    {
        Name = "Stable A";
    }

    // Constructor with parameters
    public Stable(CreateStableCommand command)
    {
        if (command.Limit <= 0)
            throw new ValidationException("La capacidad debe ser mayor a 0.");

        if (string.IsNullOrEmpty(command.Name))
            throw new ValidationException("El nombre no puede estar vacío.");

        Limit = command.Limit;
        Name = command.Name;
        UserId = command.UserId;
    }

    public void Update(UpdateStableCommand command)
    {
        if (command.Limit <= 0)
            throw new ValidationException("La capacidad debe ser mayor a 0.");

        if (string.IsNullOrEmpty(command.Name))
            throw new ValidationException("El nombre no puede estar vacío.");

        Limit = command.Limit;
        Name = command.Name;
    }
}