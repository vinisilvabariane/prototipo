using ClosedXML.Excel;
using IrisWeb.Application.Interfaces.Stations;

namespace IrisWeb.Infrastructure.Export;

public sealed class ClosedXmlStationExportWriter : IStationExportWriter
{
    private static readonly string[] Headers =
    [
        "Tipo", "Linha", "Posto", "Matrícula", "Colaborador", "Nível de treinamento",
        "Alocado no Sync", "Início", "Fim", "Duração", "Duração (s)", "Em aberto",
        "Candidato (rosto desconhecido)", "Distância do candidato",
    ];

    public Stream Write(IReadOnlyList<StationExportRow> rows, DateTime from, DateTime to, bool truncated)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Registros");

        for (var column = 0; column < Headers.Length; column++)
            sheet.Cell(1, column + 1).Value = Headers[column];

        var header = sheet.Range(1, 1, 1, Headers.Length);
        header.Style.Font.Bold = true;
        header.Style.Font.FontColor = XLColor.White;
        header.Style.Fill.BackgroundColor = XLColor.FromHtml("#1F2937");

        for (var index = 0; index < rows.Count; index++)
            WriteRow(sheet, index + 2, rows[index]);

        var lastRow = Math.Max(1, rows.Count + 1);

        sheet.Column(8).Style.DateFormat.Format = "dd/mm/yyyy hh:mm:ss";
        sheet.Column(9).Style.DateFormat.Format = "dd/mm/yyyy hh:mm:ss";
        sheet.Column(10).Style.DateFormat.Format = "[h]:mm:ss";
        sheet.Column(14).Style.NumberFormat.Format = "0.000";

        sheet.SheetView.FreezeRows(1);
        sheet.Range(1, 1, lastRow, Headers.Length).SetAutoFilter();
        sheet.Columns().AdjustToContents(1, Math.Min(lastRow, 500));

        if (truncated)
        {
            var note = sheet.Cell(lastRow + 2, 1);
            note.Value = $"Exportação limitada a 5.000 registros por tipo no período {from:dd/MM/yyyy HH:mm} a {to:dd/MM/yyyy HH:mm}. Reduza o período para ver o restante.";
            note.Style.Font.Italic = true;
        }

        var output = new MemoryStream();
        workbook.SaveAs(output);
        output.Position = 0;

        return output;
    }

    private static void WriteRow(IXLWorksheet sheet, int row, StationExportRow item)
    {
        sheet.Cell(row, 1).Value = KindLabel(item.Kind);
        sheet.Cell(row, 2).Value = item.Line;
        sheet.Cell(row, 3).Value = item.Workstation;
        sheet.Cell(row, 4).SetValue(item.Registration ?? string.Empty);
        sheet.Cell(row, 5).Value = item.CollaboratorName ?? string.Empty;
        sheet.Cell(row, 6).Value = LevelLabel(item);
        sheet.Cell(row, 7).SetValue(item.AllocatedRegistration ?? string.Empty);
        sheet.Cell(row, 8).Value = item.StartedAt;

        if (item.EndedAt is { } endedAt)
            sheet.Cell(row, 9).Value = endedAt;

        sheet.Cell(row, 10).Value = TimeSpan.FromSeconds(item.DurationSeconds);
        sheet.Cell(row, 11).Value = item.DurationSeconds;
        sheet.Cell(row, 12).Value = item.EndedAt is null ? "Sim" : "Não";
        sheet.Cell(row, 13).SetValue(item.CandidateRegistration ?? string.Empty);

        if (item.CandidateDistance is { } distance)
            sheet.Cell(row, 14).Value = distance;
    }

    private static string KindLabel(string kind) => kind switch
    {
        "presence" => "Presença",
        "vacancy" => "Posto vazio",
        "unknown" => "Rosto desconhecido",
        _ => kind,
    };

    private static string LevelLabel(StationExportRow item)
    {
        if (!string.IsNullOrWhiteSpace(item.TrainingLevel)) return item.TrainingLevel;

        return item.Kind == "presence" ? "Não registrado" : string.Empty;
    }
}
