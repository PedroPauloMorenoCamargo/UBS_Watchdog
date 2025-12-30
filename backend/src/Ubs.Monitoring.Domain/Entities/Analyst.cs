namespace Ubs.Monitoring.Domain.Entities;

public class Analyst
{
    private Analyst() { }

    public Analyst(
        string corporateEmail,
        string passwordHash,
        string fullName,
        string? phoneNumber = null)
    {
        if (string.IsNullOrWhiteSpace(corporateEmail))
            throw new ArgumentException("Corporate email is required", nameof(corporateEmail));
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required", nameof(passwordHash));
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name is required", nameof(fullName));

        Id = Guid.NewGuid();
        CorporateEmail = corporateEmail.ToLowerInvariant();
        PasswordHash = passwordHash;
        FullName = fullName;
        PhoneNumber = phoneNumber;
        ProfilePictureBase64 = null;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string CorporateEmail { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public string? PhoneNumber { get; private set; }
    public string? ProfilePictureBase64 { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public ICollection<Case> Cases { get; set; } = new List<Case>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public void UpdateProfilePicture(string? profilePictureBase64)
    {
        ProfilePictureBase64 = profilePictureBase64;
    }
}
