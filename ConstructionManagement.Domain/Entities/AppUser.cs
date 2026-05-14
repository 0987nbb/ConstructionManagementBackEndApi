namespace ConstructionManagement.Domain.Entities
{
    public class AppUser
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public bool MustChangePassword { get; set; } = false;
        public bool IsFirstLogin { get; set; } = false;
        public string? PasswordSetupTokenHash { get; set; }
        public DateTime? PasswordSetupTokenExpiresAtUtc { get; set; }
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LastFailedLoginAtUtc { get; set; }
        public DateTime? LockoutEndUtc { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
