using VacApp_Bovinova_Platform.IAM.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Model.Aggregates;

namespace VacApp_Bovinova_Platform.StaffAdministration.Domain.Services;

/// <summary>
/// Resolves who the authenticated user really operates as.
/// Owners operate on their own data; active staff operate on their owner's data
/// (effectiveUserId = Staff.UserId) with permissions taken from Staff.AccessLevel.
/// Permissions are always resolved from the database — never from the JWT — so
/// access changes apply immediately without waiting for a token refresh.
/// All methods throw <see cref="Shared.Domain.Model.Exceptions.ForbiddenRequestException"/>
/// (HTTP 403) when the user is linked as staff but the access is inactive.
/// </summary>
public interface IStaffAccessService
{
    /// <summary>Owner id whose data must be read/written: Staff.UserId for active staff, user.Id otherwise.</summary>
    Task<int> GetEffectiveUserIdAsync(User user);

    Task<bool> IsStaffAsync(User user);
    Task<bool> CanReadAsync(User user);
    Task<bool> CanEditAsync(User user);
    Task<bool> CanManageStaffAsync(User user);
    Task<bool> IsOwnerAsync(User user);

    /// <summary>Active staff record of the user, or null when the user is an owner (not staff).</summary>
    Task<Staff?> GetActiveStaffAccessAsync(User user);

    /// <summary>The owner User aggregate the user operates as (itself for owners). Used to read the effective subscription plan.</summary>
    Task<User> GetEffectiveOwnerAsync(User user);
}
