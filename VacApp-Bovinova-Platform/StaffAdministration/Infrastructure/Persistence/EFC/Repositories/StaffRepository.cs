using Microsoft.EntityFrameworkCore;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Configuration;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Repositories;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Model.ValueObjects;
using VacApp_Bovinova_Platform.StaffAdministration.Domain.Repositories;

namespace VacApp_Bovinova_Platform.StaffAdministration.Infrastructure.Persistence.EFC.Repositories;

public class StaffRepository(AppDbContext ctx)
    : BaseRepository<Staff>(ctx), IStaffRepository
{
    public async Task<Staff?> FindByNameAsync(string name)
    {
        return await Context.Set<Staff>().FirstOrDefaultAsync(f => f.Name == name);
    }

    public async Task<IEnumerable<Staff>> FindByEmployeeStatusAsync(int employeeStatus)
    {
        return await Context.Set<Staff>().Where(f => f.EmployeeStatus.Value == employeeStatus).ToListAsync();
    }

    public async Task<IEnumerable<Staff>> FindByUserIdAsync(int userId)
    {
        return await Context.Set<Staff>().Where(f => f.UserId == userId).ToListAsync();
    }

    public async Task<Staff?> FindByLinkedUserIdAsync(int linkedUserId)
    {
        // Prefer an active record so a user who is inactive staff for one owner
        // but active staff for another resolves to the access that still works.
        var records = await Context.Set<Staff>()
            .Where(f => f.LinkedUserId == linkedUserId)
            .ToListAsync();
        return records
            .OrderBy(f => f.EmployeeStatus.Value == 1 ? 0 : 1)
            .FirstOrDefault();
    }

    public async Task<Staff?> FindByOwnerUserIdAndLinkedUserIdAsync(int ownerUserId, int linkedUserId)
    {
        return await Context.Set<Staff>()
            .FirstOrDefaultAsync(f => f.UserId == ownerUserId && f.LinkedUserId == linkedUserId);
    }

    public async Task<Staff?> FindByOwnerUserIdAndEmailAsync(int ownerUserId, string email)
    {
        return await Context.Set<Staff>()
            .FirstOrDefaultAsync(f => f.UserId == ownerUserId && f.Email == email);
    }

    public async Task<IEnumerable<Staff>> FindByOwnerUserIdAsync(int ownerUserId)
    {
        return await Context.Set<Staff>().Where(f => f.UserId == ownerUserId).ToListAsync();
    }
}
