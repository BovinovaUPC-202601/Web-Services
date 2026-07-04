using VacApp_Bovinova_Platform.IAM.Domain.Model.Aggregates;
using VacApp_Bovinova_Platform.Shared.Domain.Repositories;

namespace VacApp_Bovinova_Platform.IAM.Domain.Repositories
{
    public interface IPasswordResetTokenRepository : IBaseRepository<PasswordResetToken>
    {
        /// <summary>Latest still-usable token for the user (not used, not expired), or null.</summary>
        Task<PasswordResetToken?> FindActiveByUserIdAsync(int userId);

        /// <summary>Burns every outstanding token of the user, so only the newest code works.</summary>
        Task InvalidateAllForUserAsync(int userId);
    }
}
