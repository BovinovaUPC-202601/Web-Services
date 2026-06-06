using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IoTMonitoring.Domain.Model.Commands;

namespace VacApp_Bovinova_Platform.IoTMonitoring.Domain.Services;

public interface ICollarCommandService
{
    Task<Collar> Handle(RegisterCollarCommand command);

    /// <summary>Deletes (unassigns) a collar owned by the user, freeing capacity.</summary>
    Task Handle(DeleteCollarCommand command);

    /// <summary>Reassigns a user's collar to a different bovine.</summary>
    Task<Collar> Handle(ReassignCollarCommand command);

    /// <summary>Suspends all of a user's collars (e.g. subscription suspended/cancelled).</summary>
    Task SuspendUserCollarsAsync(int userId);

    /// <summary>Reactivates a user's suspended collars (e.g. subscription reactivated).</summary>
    Task ReactivateUserCollarsAsync(int userId);
}
