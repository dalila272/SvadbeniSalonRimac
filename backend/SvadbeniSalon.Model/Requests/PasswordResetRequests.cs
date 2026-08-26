namespace SvadbeniSalon.Model.Requests;

public class ForgotPasswordRequest
{
    public string EmailOrUsername { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    public string EmailOrUsername { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
