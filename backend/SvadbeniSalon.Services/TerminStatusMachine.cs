using SvadbeniSalon.Model.Enums;

namespace SvadbeniSalon.Services;

public static class TerminStatusMachine
{
    public static bool CanTransition(TerminStatus from, TerminStatus to, bool isStaff)
    {
        if (from == to)
        {
            return false;
        }

        if (from is TerminStatus.Cancelled or TerminStatus.Completed)
        {
            return false;
        }

        if (!isStaff)
        {
            return from == TerminStatus.Pending && to == TerminStatus.Cancelled;
        }

        return (from, to) switch
        {
            (TerminStatus.Pending, TerminStatus.Confirmed) => true,
            (TerminStatus.Pending, TerminStatus.Cancelled) => true,
            (TerminStatus.Confirmed, TerminStatus.Completed) => true,
            (TerminStatus.Confirmed, TerminStatus.Cancelled) => true,
            _ => false
        };
    }

    public static bool RequiresReason(TerminStatus to) =>
        to == TerminStatus.Cancelled;

    public static bool IsActive(TerminStatus status) =>
        status is TerminStatus.Pending or TerminStatus.Confirmed;

    public static readonly TerminStatus[] ActiveStatuses =
        [TerminStatus.Pending, TerminStatus.Confirmed];

    public static string StatusLabel(TerminStatus status) => status switch
    {
        TerminStatus.Pending => "na čekanju",
        TerminStatus.Confirmed => "potvrđen",
        TerminStatus.Cancelled => "otkazan",
        TerminStatus.Completed => "završen",
        _ => status.ToString()
    };
}
