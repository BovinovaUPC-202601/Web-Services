using VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.Shared.Domain.Model.Exceptions;

namespace VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Aggregates;

public class Campaign
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public int UserId { get; private set; }

    protected Campaign()
    {
        Name = string.Empty;
        Description = string.Empty;
        StartDate = DateOnly.FromDateTime(DateTime.Now);
        EndDate = DateOnly.FromDateTime(DateTime.Now);
    }

    private static void ValidateDates(DateOnly startDate, DateOnly endDate)
    {
        if (startDate > endDate)
            throw new ValidationException("La fecha de inicio no puede ser posterior a la fecha de fin.");
    }

    public Campaign(string name, string description, DateOnly startDate, DateOnly endDate, int userId)
    {
        ValidateDates(startDate, endDate);

        Name = name;
        Description = description;
        StartDate = startDate;
        EndDate = endDate;
        UserId = userId;
    }

    public Campaign(CreateCampaignCommand command)
    {
        ValidateDates(command.StartDate, command.EndDate);

        Name = command.Name;
        Description = command.Description;
        StartDate = command.StartDate;
        EndDate = command.EndDate;
        UserId = command.UserId;
    }

    // Actualiza los campos editables de la campaña.
    public void Update(UpdateCampaignCommand command)
    {
        ValidateDates(command.StartDate, command.EndDate);

        Name = command.Name;
        Description = command.Description;
        StartDate = command.StartDate;
        EndDate = command.EndDate;
    }
}