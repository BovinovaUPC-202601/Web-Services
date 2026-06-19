using Microsoft.EntityFrameworkCore;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Model.Entities;
using VacApp_Bovinova_Platform.CampaignManagement.Domain.Repositories;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Configuration;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace VacApp_Bovinova_Platform.CampaignManagement.Infrastructure.Repositories;

public class CampaignRepository(AppDbContext context) : BaseRepository<Campaign>(context), ICampaignRepository
{
    public async Task<Campaign?> FindByNameAsync(string name)
    {
        return await Context.Set<Campaign>()
            .Include(c => c.CampaignStables)
            .Include(c => c.CampaignBovines)
            .FirstOrDefaultAsync(c => c.Name == name);
    }

    public async Task<IEnumerable<Campaign>> FindByUserIdAsync(int userId)
    {
        return await Context.Set<Campaign>()
            .Where(f => f.UserId == userId)
            .Include(c => c.CampaignStables)
            .Include(c => c.CampaignBovines)
            .ToListAsync();
    }

    public new async Task<Campaign?> FindByIdAsync(int id)
    {
        return await Context.Set<Campaign>()
            .Include(c => c.CampaignStables)
            .Include(c => c.CampaignBovines)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}