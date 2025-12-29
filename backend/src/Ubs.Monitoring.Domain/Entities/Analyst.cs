namespace Ubs.Monitoring.Domain.Entities;

public class Analyst
{
    public Guid Id { get; private set; }
    public string CorporateEmail { get; private set; } = null!; 
    public string PasswordHash { get; private set; } = null!; 
    public string FullName { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? ProfilePictureBase64 { get; set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public ICollection<Case> Cases { get; set; } = new List<Case>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
