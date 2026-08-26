using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SvadbeniSalon.Model.Enums;
using SvadbeniSalon.Services.Database;

namespace SvadbeniSalon.Services;

public static class IzvjestajPdfGenerator
{
    static IzvjestajPdfGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public static byte[] GenerateSvadbeReport(
        DateTime datumOd,
        DateTime datumDo,
        int? statusFilter,
        IReadOnlyList<Svadba> svadbe)
    {
        var statusLabel = FormatStatusFilter(statusFilter);
        var ukupno = svadbe.Count;
        var potvrđene = svadbe.Count(s => s.Status == TerminStatus.Confirmed);
        var završene = svadbe.Count(s => s.Status == TerminStatus.Completed);

        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(32);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(column =>
                {
                    column.Item().Text("Svadbeni Salon Rimac")
                        .FontSize(20).Bold().FontColor(Colors.Red.Medium);
                    column.Item().Text("Izvještaj — Pregled svadbi u periodu")
                        .FontSize(12).FontColor(Colors.Grey.Darken2);
                    column.Item().PaddingTop(6).Text(
                        $"Period: {datumOd:dd.MM.yyyy.} – {datumDo:dd.MM.yyyy.}   |   Status: {statusLabel}");
                    column.Item().Text(
                        $"Ukupno: {ukupno}   |   Potvrđene: {potvrđene}   |   Završene: {završene}");
                    column.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingVertical(12).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(70);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.ConstantColumn(70);
                        columns.ConstantColumn(50);
                        columns.ConstantColumn(75);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        foreach (var title in new[]
                                 {
                                     "Datum", "Klijent", "Paket", "Vrijeme", "Gosti", "Cijena", "Status",
                                 })
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5)
                                .Text(title).Bold();
                        }
                    });

                    foreach (var s in svadbe.OrderBy(x => x.DatumSvadbe))
                    {
                        var klijent = s.User != null
                            ? $"{s.User.FirstName} {s.User.LastName}".Trim()
                            : "—";
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(s.DatumSvadbe.ToString("dd.MM.yyyy."));
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(klijent);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(s.Ponuda?.Naziv ?? "—");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(s.Vrijeme.ToString(@"hh\:mm"));
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .AlignRight().Text(s.BrojGostiju.ToString());
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .AlignRight().Text(FormatAmount(s.Ponuda?.Cijena ?? 0));
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(FormatStatus(s.Status));
                    }

                    if (svadbe.Count == 0)
                    {
                        table.Cell().ColumnSpan(7).Padding(12)
                            .Text("Nema svadbi za odabrane kriterije.");
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Generisano: ");
                    text.Span($"{DateTime.UtcNow:dd.MM.yyyy. HH:mm} UTC").Bold();
                });
            });
        }).GeneratePdf();
    }

    public static byte[] GenerateUplateReport(
        DateTime datumOd,
        DateTime datumDo,
        IReadOnlyList<Rata> uplate)
    {
        var ukupno = uplate.Sum(u => u.Iznos);

        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(32);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(column =>
                {
                    column.Item().Text("Svadbeni Salon Rimac")
                        .FontSize(20).Bold().FontColor(Colors.Red.Medium);
                    column.Item().Text("Izvještaj — Pregled uplata u periodu")
                        .FontSize(12).FontColor(Colors.Grey.Darken2);
                    column.Item().PaddingTop(6).Text(
                        $"Period: {datumOd:dd.MM.yyyy.} – {datumDo:dd.MM.yyyy.}");
                    column.Item().Text(
                        $"Broj uplata: {uplate.Count}   |   Ukupan iznos: {FormatAmount(ukupno)} KM");
                    column.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingVertical(12).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(75);
                        columns.RelativeColumn(2);
                        columns.ConstantColumn(75);
                        columns.RelativeColumn(2);
                        columns.ConstantColumn(80);
                    });

                    table.Header(header =>
                    {
                        foreach (var title in new[]
                                 {
                                     "Datum uplate", "Klijent", "Datum svadbe", "Paket", "Iznos (KM)",
                                 })
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5)
                                .Text(title).Bold();
                        }
                    });

                    foreach (var u in uplate.OrderBy(x => x.DatumUplate))
                    {
                        var svadba = u.Svadba;
                        var klijent = svadba?.User != null
                            ? $"{svadba.User.FirstName} {svadba.User.LastName}".Trim()
                            : "—";
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(u.DatumUplate.ToString("dd.MM.yyyy."));
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(klijent);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(svadba?.DatumSvadbe.ToString("dd.MM.yyyy.") ?? "—");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(svadba?.Ponuda?.Naziv ?? "—");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .AlignRight().Text(FormatAmount(u.Iznos));
                    }

                    if (uplate.Count == 0)
                    {
                        table.Cell().ColumnSpan(5).Padding(12)
                            .Text("Nema uplata za odabrani period.");
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Generisano: ");
                    text.Span($"{DateTime.UtcNow:dd.MM.yyyy. HH:mm} UTC").Bold();
                });
            });
        }).GeneratePdf();
    }

    private static string FormatAmount(decimal value) => value.ToString("N2");

    private static string FormatStatusFilter(int? status) =>
        status switch
        {
            0 => "Na čekanju",
            1 => "Potvrđene",
            2 => "Otkazane",
            3 => "Završene",
            _ => "Svi statusi",
        };

    private static string FormatStatus(TerminStatus status) =>
        status switch
        {
            TerminStatus.Pending => "Na čekanju",
            TerminStatus.Confirmed => "Potvrđena",
            TerminStatus.Cancelled => "Otkazana",
            TerminStatus.Completed => "Završena",
            _ => status.ToString(),
        };
}
