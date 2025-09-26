using System.Globalization;
using System.IO;
using Microsoft.Extensions.Hosting;
using Parking.Application.Dtos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Parking.Api.Services;

public sealed class TicketPdfExporter : ITicketPdfExporter
{
    private static readonly CultureInfo Culture = new("pt-BR");
    private readonly string _backgroundImagePath;
    private readonly string _emptyStateImagePath;

    public TicketPdfExporter(IHostEnvironment hostEnvironment)
    {
        if (hostEnvironment is null)
        {
            throw new ArgumentNullException(nameof(hostEnvironment));
        }

        var assetsDirectory = Path.Combine(hostEnvironment.ContentRootPath, "Assets");
        _backgroundImagePath = Path.Combine(assetsDirectory, "export-background.png");
        _emptyStateImagePath = Path.Combine(assetsDirectory, "no-data.png");
    }

    public byte[] Generate(IReadOnlyCollection<ParkingTicketDto> tickets)
    {
        var ticketList = (tickets ?? Array.Empty<ParkingTicketDto>())
            .OrderByDescending(ticket => ticket.EntryAt)
            .ToList();

        EnsureAssetExists(_backgroundImagePath, "export-background.png");
        EnsureAssetExists(_emptyStateImagePath, "no-data.png");

        var backgroundImage = ImageSource.FromFile(_backgroundImagePath);
        var emptyStateImage = ImageSource.FromFile(_emptyStateImagePath);

        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(60);
                page.MarginTop(160);
                page.MarginBottom(160);
                page.DefaultTextStyle(TextStyle.Default.FontSize(11));
                page.Background().Image(backgroundImage, ImageScaling.FitArea);
                page.Content().PaddingHorizontal(15).Element(container =>
                {
                    if (ticketList.Count == 0)
                    {
                        BuildEmptyState(container, emptyStateImage);
                        return;
                    }

                    container.Column(column =>
                    {
                        column.Spacing(14);
                        column.Item().Text("Relatório de Tickets de Estacionamento")
                            .FontSize(18)
                            .SemiBold()
                            .FontColor(Colors.Blue.Darken3);

                        column.Item().Text($"Atualizado em {DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm", Culture)}")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken2);

                        column.Item().Element(content => BuildTable(content, ticketList));
                    });
                });

                page.Footer().PaddingTop(12).Row(row =>
                {
                    row.RelativeItem().Text(text =>
                    {
                        text.Span("Cuidado ao compartilhar conteúdo sensível da empresa, descarte o documento após uso conforme as normas da empresa.")
                            .FontSize(9)
                            .FontColor(Colors.Red.Medium)
                            .WrapAnywhere();
                    });

                    row.ConstantItem(110).AlignRight().Text(text =>
                    {
                        text.Span("Página ");
                        text.CurrentPageNumber();
                        text.Span(" de ");
                        text.TotalPages();
                    }).FontSize(9).FontColor(Colors.Grey.Darken3);
                });
            });
        }).GeneratePdf();
    }

    private static void EnsureAssetExists(string assetPath, string assetFileName)
    {
        if (File.Exists(assetPath))
        {
            return;
        }

        throw new FileNotFoundException(
            $"O arquivo '{assetFileName}' não foi encontrado no diretório de assets da API. Anexe o arquivo manualmente antes de gerar o PDF.",
            assetPath);
    }

    private static void BuildTable(IContainer container, IReadOnlyList<ParkingTicketDto> tickets)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2); // Placa
                columns.RelativeColumn(2); // Entrada
                columns.RelativeColumn(2); // Saída
                columns.RelativeColumn(1.5f); // Duração
                columns.RelativeColumn(1.5f); // Valor
                columns.RelativeColumn(1.5f); // Status
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCell).Text("Placa");
                header.Cell().Element(HeaderCell).Text("Entrada");
                header.Cell().Element(HeaderCell).Text("Saída");
                header.Cell().Element(HeaderCell).Text("Duração");
                header.Cell().Element(HeaderCell).Text("Valor");
                header.Cell().Element(HeaderCell).Text("Status");
            });

            foreach (var ticket in tickets)
            {
                table.Cell().Element(ContentCell).Text(ticket.Plate).WrapAnywhere();
                table.Cell().Element(ContentCell).Text(FormatDate(ticket.EntryAt));
                table.Cell().Element(ContentCell).Text(FormatNullableDate(ticket.ExitAt));
                table.Cell().Element(ContentCell).Text(FormatDuration(ticket.DurationInMinutes));
                table.Cell().Element(ContentCell).Text(FormatAmount(ticket.TotalAmount));
                table.Cell().Element(ContentCell).Text(ticket.ExitAt.HasValue ? "Finalizado" : "Ativo");
            }
        });
    }

    private static void BuildEmptyState(IContainer container, ImageSource emptyStateImage)
    {
        container.Column(column =>
        {
            column.Spacing(25);
            column.Item().AlignCenter().Element(imageContainer =>
            {
                imageContainer
                    .Width(320)
                    .Height(320)
                    .Image(emptyStateImage, ImageScaling.FitArea);
            });
            column.Item().AlignCenter().Text("Sem dados para exportar")
                .FontSize(18)
                .SemiBold()
                .FontColor(Colors.Blue.Darken3);

            column.Item().AlignCenter().Text("Cadastre tickets para gerar um relatório em PDF.")
                .FontSize(12)
                .FontColor(Colors.Grey.Darken2);
        });
    }

    private static IContainer HeaderCell(IContainer container)
    {
        return container.Background(Colors.Blue.Medium)
            .PaddingVertical(6)
            .PaddingHorizontal(8);
    }

    private static IContainer ContentCell(IContainer container)
    {
        return container
            .PaddingVertical(6)
            .PaddingHorizontal(8)
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten2);
    }

    private static string FormatDate(DateTimeOffset date)
        => date.ToOffset(TimeSpan.Zero).ToString("dd/MM/yyyy HH:mm", Culture);

    private static string FormatNullableDate(DateTimeOffset? date)
        => date.HasValue ? FormatDate(date.Value) : "-";

    private static string FormatDuration(double? durationInMinutes)
    {
        if (!durationInMinutes.HasValue)
        {
            return "-";
        }

        var totalMinutes = Math.Max(0, durationInMinutes.Value);
        var time = TimeSpan.FromMinutes(totalMinutes);
        return $"{(int)time.TotalHours:D2}h {time.Minutes:D2}m";
    }

    private static string FormatAmount(decimal? amount)
        => amount.HasValue ? amount.Value.ToString("C", Culture) : "-";
}
