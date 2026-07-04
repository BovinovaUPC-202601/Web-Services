namespace VacApp_Bovinova_Platform.IAM.Domain.Model.Aggregates
{
    /// <summary>
    /// A single-use password-recovery token (RF-03). We store only the BCrypt hash of the
    /// 6-digit code — never the code itself — so a database leak can't be used to reset
    /// passwords. The code is short-lived and rate-limited to make online guessing useless.
    /// </summary>
    public class PasswordResetToken
    {
        /// <summary>Max verification attempts before the token is burned (anti-brute-force).</summary>
        public const int MaxAttempts = 5;

        public int Id { get; private set; }
        public int UserId { get; private set; }
        public string CodeHash { get; private set; }
        public DateTime ExpiresAt { get; private set; }
        public DateTime? UsedAt { get; private set; }
        public int Attempts { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private PasswordResetToken() { }

        public PasswordResetToken(int userId, string codeHash, DateTime expiresAt)
        {
            UserId = userId;
            CodeHash = codeHash;
            ExpiresAt = expiresAt;
            Attempts = 0;
            CreatedAt = DateTime.UtcNow;
        }

        public bool IsUsed => UsedAt is not null;
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool ExceededAttempts => Attempts >= MaxAttempts;
        public bool IsActive => !IsUsed && !IsExpired && !ExceededAttempts;

        public void MarkUsed() => UsedAt = DateTime.UtcNow;
        public void RegisterFailedAttempt() => Attempts++;
    }
}
