using ClosedXML.Excel;
using IrisWeb.Application.Interfaces.Stations;
using IrisWeb.Infrastructure.Export;

namespace IrisWeb.Tests.Infrastructure;

public sealed class ClosedXmlStationExportWriterTests
{
    private static readonly DateTime Start = new(2026, 9, 25, 8, 0, 0);

    private static IXLWorksheet Open(Stream stream) => new XLWorkbook(stream).Worksheet("Registros");

    [Fact]
    public void Write_ProducesOneSheetWithHeaderAndTreatedRows()
    {
        var rows = new[]
        {
            new StationExportRow(
                "presence", "N111", "#1 INSPEÇÃO", "15834", "FULANO", "Treinado", "15834",
                Start, Start.AddSeconds(90), 90, null, null),
            new StationExportRow(
                "vacancy", "N111", "#1 INSPEÇÃO", null, null, null, null,
                Start.AddMinutes(5), null, 30, null, null),
            new StationExportRow(
                "unknown", "N111", "#1 INSPEÇÃO", null, null, null, null,
                Start.AddMinutes(9), Start.AddMinutes(10), 60, "99999", 0.9f),
        };

        using var stream = new ClosedXmlStationExportWriter().Write(rows, Start, Start.AddDays(1), truncated: false);
        var sheet = Open(stream);

        Assert.Equal("Tipo", sheet.Cell(1, 1).GetString());
        Assert.Equal("Presença", sheet.Cell(2, 1).GetString());
        Assert.Equal("Posto vazio", sheet.Cell(3, 1).GetString());
        Assert.Equal("Rosto desconhecido", sheet.Cell(4, 1).GetString());
        Assert.Equal("15834", sheet.Cell(2, 4).GetString());
        Assert.Equal(Start, sheet.Cell(2, 8).GetDateTime());
        Assert.Equal(TimeSpan.FromSeconds(90), sheet.Cell(2, 10).GetTimeSpan());
        Assert.Equal("Não", sheet.Cell(2, 12).GetString());
        Assert.True(sheet.Cell(3, 9).IsEmpty());
        Assert.Equal("Sim", sheet.Cell(3, 12).GetString());
        Assert.Equal("99999", sheet.Cell(4, 13).GetString());
        Assert.Equal(0.9, sheet.Cell(4, 14).GetDouble(), 3);
    }

    [Fact]
    public void Write_PresenceWithoutLevel_SaysNotRegistered()
    {
        var rows = new[]
        {
            new StationExportRow(
                "presence", "N111", "#1 POSTO", "1", null, null, null, Start, Start.AddSeconds(5), 5, null, null),
        };

        using var stream = new ClosedXmlStationExportWriter().Write(rows, Start, Start.AddDays(1), truncated: false);

        Assert.Equal("Não registrado", Open(stream).Cell(2, 6).GetString());
    }

    [Fact]
    public void Write_Truncated_AddsANoteBelowTheData()
    {
        var rows = new[]
        {
            new StationExportRow(
                "presence", "N111", "#1 POSTO", "1", null, null, null, Start, Start.AddSeconds(5), 5, null, null),
        };

        using var stream = new ClosedXmlStationExportWriter().Write(rows, Start, Start.AddDays(1), truncated: true);

        Assert.Contains("limitada", Open(stream).Cell(4, 1).GetString());
    }

    [Fact]
    public void Write_NoRows_StillHasTheHeader()
    {
        using var stream = new ClosedXmlStationExportWriter().Write([], Start, Start.AddDays(1), truncated: false);

        Assert.Equal("Tipo", Open(stream).Cell(1, 1).GetString());
    }
}
