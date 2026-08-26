namespace SvadbeniSalon.Model.Requests;

public class AdminSetPasswordRequest
{
    public string NewPassword { get; set; } = null!;
    public string ConfirmNewPassword { get; set; } = null!;
}
