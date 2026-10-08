namespace SvadbeniSalon.Model.Requests;

public class RecenzijaInsertRequest
{
    public int PonudaId { get; set; }

    /// <summary>
    /// Optional. When set, the wedding must be completed and owned by the user.
    /// When null, the rating is a pre-wedding package review.
    /// </summary>
    public int? SvadbaId { get; set; }

    public int Ocjena { get; set; }
    public string? Komentar { get; set; }
}

public class RecenzijaUpdateRequest
{
    public int Ocjena { get; set; }
    public string? Komentar { get; set; }
}
