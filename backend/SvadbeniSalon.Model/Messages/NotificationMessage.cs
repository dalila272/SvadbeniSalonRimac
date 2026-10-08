namespace SvadbeniSalon.Model.Messages;

public class NotificationMessage
{
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? RecipientEmail { get; set; }
    public string? Kind { get; set; }
    public string? AdminBody { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsCritical =>
        string.Equals(Kind, "PasswordReset", StringComparison.OrdinalIgnoreCase);
}
