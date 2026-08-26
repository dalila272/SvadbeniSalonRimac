using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SvadbeniSalon.Services.Database;

namespace SvadbeniSalon.Services;

public static class RacunPdfGenerator
{
    static RacunPdfGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public static byte[] Generate(
        Racun racun,
        Svadba svadba,
        IReadOnlyList<Rata> uplate)
    {
        var klijent = svadba.User != null
            ? $"{svadba.User.FirstName} {svadba.User.LastName}".Trim()
            : "—";
        var email = svadba.User?.Email ?? "—";
        var paket = svadba.Ponuda?.Naziv ?? "—";
        var preostalo = racun.UkupanIznos - racun.UplaceniIznos;

        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(column =>
                {
                    column.Item().Text("Svadbeni Salon Rimac")
                        .FontSize(22).Bold().FontColor(Colors.Red.Medium);
                    column.Item().Text("Račun za usluge organizacije svadbe")
                        .FontSize(12).FontColor(Colors.Grey.Darken2);
                    column.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingVertical(16).Column(column =>
                {
                    column.Spacing(12);

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Column(left =>
                        {
                            left.Item().Text($"Broj računa: {racun.BrojRacuna}").Bold();
                            left.Item().Text(
                                $"Datum izdavanja: {racun.DatumIzdavanja:dd.MM.yyyy.}");
                            left.Item().Text(
                                $"Datum svadbe: {svadba.DatumSvadbe:dd.MM.yyyy.} u {svadba.Vrijeme:hh\\:mm}");
                        });

                        row.RelativeItem().Column(right =>
                        {
                            right.Item().Text("Klijent").Bold();
                            right.Item().Text(klijent);
                            right.Item().Text(email);
                            right.Item().Text($"Broj gostiju: {svadba.BrojGostiju}");
                        });
                    });

                    column.Item().PaddingTop(8).Text($"Paket: {paket}").Bold();

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(6)
                                .Text("Stavka").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(6)
                                .AlignRight().Text("Iznos (KM)").Bold();
                        });

                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(6).Text($"Usluga — {paket}");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(6).AlignRight().Text(FormatAmount(racun.UkupanIznos));
                    });

                    if (uplate.Count > 0)
                    {
                        column.Item().PaddingTop(8).Text("Evidentirane uplate").Bold();

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(6)
                                    .Text("Datum uplate").Bold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(6)
                                    .AlignRight().Text("Iznos (KM)").Bold();
                            });

                            foreach (var uplata in uplate.OrderBy(u => u.DatumUplate))
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                    .Padding(6).Text(uplata.DatumUplate.ToString("dd.MM.yyyy."));
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                    .Padding(6).AlignRight().Text(FormatAmount(uplata.Iznos));
                            }
                        });
                    }

                    column.Item().PaddingTop(12).AlignRight().Column(summary =>
                    {
                        summary.Item().Text($"Ukupno: {FormatAmount(racun.UkupanIznos)} KM").Bold();
                        summary.Item().Text($"Uplaćeno: {FormatAmount(racun.UplaceniIznos)} KM")
                            .FontColor(Colors.Green.Darken2);
                        summary.Item().Text($"Preostalo: {FormatAmount(preostalo)} KM")
                            .Bold();
                    });
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Hvala na povjerenju — ");
                    text.Span("Svadbeni Salon Rimac").Bold();
                });
            });
        }).GeneratePdf();
    }

    private static string FormatAmount(decimal value) => value.ToString("N2");
}
