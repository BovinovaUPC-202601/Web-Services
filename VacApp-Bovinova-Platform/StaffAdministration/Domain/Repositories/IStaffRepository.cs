using VacApp_Bovinova_Platform.Shared.Domain.Repositories;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Model.ValueObjects;

namespace VacApp_Bovinova_Platform.StaffAdministration.Domain.Repositories;

public interface IStaffRepository : IBaseRepository<Staff>
{
    Task<Staff?> FindByNameAsync(string name);
    Task<IEnumerable<Staff>> FindByEmployeeStatusAsync(int employeeStatus);

    /// <summary>Finds all staff of an owner. The userId parameter is the owner/rancher id.</summary>
    Task<IEnumerable<Staff>> FindByUserIdAsync(int userId);

    /// <summary>Finds the staff record a signed-in user is linked to (active records first).</summary>
    Task<Staff?> FindByLinkedUserIdAsync(int linkedUserId);

    Task<Staff?> FindByOwnerUserIdAndLinkedUserIdAsync(int ownerUserId, int linkedUserId);
    Task<Staff?> FindByOwnerUserIdAndEmailAsync(int ownerUserId, string email);
    Task<IEnumerable<Staff>> FindByOwnerUserIdAsync(int ownerUserId);
}
