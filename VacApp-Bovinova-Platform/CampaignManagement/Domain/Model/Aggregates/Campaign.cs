using VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Commands;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Entities;
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

    public ICollection<CampaignStable> CampaignStables { get; private set; } = new List<CampaignStable>();
    public ICollection<CampaignBovine> CampaignBovines { get; private set; } = new List<CampaignBovine>();

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

    private static void ValidateTargets(List<int> stableIds, List<int> bovineIds)
    {
        if (stableIds.Count == 0 && bovineIds.Count == 0)
            throw new ValidationException("Debe seleccionar al menos un establo o un bovino.");
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
        ValidateTargets(command.StableIds, command.BovineIds);

        Name = command.Name;
        Description = command.Description;
        StartDate = command.StartDate;
        EndDate = command.EndDate;
        UserId = command.UserId;
        foreach (var stableId in command.StableIds)
            CampaignStables.Add(new CampaignStable(stableId));
        foreach (var bovineId in command.BovineIds)
            CampaignBovines.Add(new CampaignBovine(bovineId));
    }

    public void Update(UpdateCampaignCommand command)
    {
        ValidateDates(command.StartDate, command.EndDate);
        ValidateTargets(command.StableIds, command.BovineIds);

        Name = command.Name;
        Description = command.Description;
        StartDate = command.StartDate;
        EndDate = command.EndDate;
        CampaignStables.Clear();
        foreach (var stableId in command.StableIds)
            CampaignStables.Add(new CampaignStable(stableId));
        CampaignBovines.Clear();
        foreach (var bovineId in command.BovineIds)
            CampaignBovines.Add(new CampaignBovine(bovineId));
    }
}
