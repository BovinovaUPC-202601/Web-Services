using VacApp_Bovinova_Platform.IAM.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IAM.Domain.Repositories;
using VacApp_Bovinova_Platform.Shared.Domain.Model.Exceptions;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Model.ValueObjects;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Repositories;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Services;

namespace VacApp_Bovinova_Platform.StaffAdministration.Application.Internal.Services;

public class StaffAccessService(
    IStaffRepository staffRepository,
    IUserRepository userRepository) : IStaffAccessService
{
    public async Task<Staff?> GetActiveStaffAccessAsync(User user)
    {
        var staff = await staffRepository.FindByLinkedUserIdAsync(user.Id);
        if (staff is null) return null;

        if (!staff.IsActive)
            throw new ForbiddenRequestException(
                "Your staff access is inactive. Contact the ranch owner.");

        return staff;
    }

    public async Task<int> GetEffectiveUserIdAsync(User user)
    {
        var staff = await GetActiveStaffAccessAsync(user);
        return staff?.UserId ?? user.Id;
    }

    public async Task<bool> IsStaffAsync(User user)
        => await GetActiveStaffAccessAsync(user) is not null;

    public async Task<bool> IsOwnerAsync(User user)
        => await GetActiveStaffAccessAsync(user) is null;

    public async Task<bool> CanReadAsync(User user)
    {
        // Throws when the staff access is inactive; owners and any active staff can read.
        await GetActiveStaffAccessAsync(user);
        return true;
    }

    public async Task<bool> CanEditAsync(User user)
    {
        var staff = await GetActiveStaffAccessAsync(user);
        return staff is null || staff.AccessLevel >= StaffAccessLevel.Editor;
    }

    public async Task<bool> CanManageStaffAsync(User user)
    {
        var staff = await GetActiveStaffAccessAsync(user);
        return staff is null || staff.AccessLevel == StaffAccessLevel.Manager;
    }

    public async Task<User> GetEffectiveOwnerAsync(User user)
    {
        var staff = await GetActiveStaffAccessAsync(user);
        if (staff is null) return user;

        var owner = await userRepository.FindByIdAsync(staff.UserId);
        return owner ?? user;
    }
}
