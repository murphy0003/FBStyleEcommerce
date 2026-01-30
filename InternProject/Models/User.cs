namespace InternProject.Models
{
    public class User
    {
        public Guid UserId { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public string? Password { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string? GoogleId {  get; set; }
        public AccountType Type { get; set; }
        public AccountStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsEmailVerified { get; set; } = false;
        public bool IsRegistrationCompleted { get; set; }
        public DateTime? RegistrationExpiresAt { get; set; }
        public string? VerificationOtp { get; set; }
        public DateTime? OtpExpiry { get; set; }
        public int VerificationAttempts { get; set; } = 0;
        public DateTime? LastOtpSentAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public int AccessFailedCount { get; set; } = 0;
        public DateTime? LockoutEnd { get; set; }
        public bool IsPasswordResetVerified { get; set; }

        public ICollection<Post> Posts { get; set; }

    }
}
