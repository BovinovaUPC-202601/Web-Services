using Microsoft.EntityFrameworkCore;
using VacApp_Bovinova_Platform.IAM.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.IAM.Domain.Repositories;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Configuration;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace VacApp_Bovinova_Platform.IAM.Infrastructure.Repositories
{
    public class PasswordResetTokenRepository(AppDbContext context)
        : BaseRepository<PasswordResetToken>(context), IPasswordResetTokenRepository
    {
        public async Task<PasswordResetToken?> FindActiveByUserIdAsync(int userId)
        {
            var now = DateTime.UtcNow;
            return await context.Set<PasswordResetToken>()
                .Where(t => t.UserId == userId && t.UsedAt == null && t.ExpiresAt > now)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task InvalidateAllForUserAsync(int userId)
        {
            var tokens = await context.Set<PasswordResetToken>()
                .Where(t => t.UserId == userId && t.UsedAt == null)
                .ToListAsync();
            foreach (var token in tokens)
                token.MarkUsed();
        }
    }
}
