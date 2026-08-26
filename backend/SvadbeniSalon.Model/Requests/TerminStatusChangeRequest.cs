using SvadbeniSalon.Model.Enums;

namespace SvadbeniSalon.Model.Requests;

public class TerminStatusChangeRequest
{
    public TerminStatus Status { get; set; }
    public string? Razlog { get; set; }
}
