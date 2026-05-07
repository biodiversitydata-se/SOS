using OfficeOpenXml;
using OfficeOpenXml.Style;
using SOS.Harvest.Models;
using System.Drawing;

namespace SOS.Harvest.Factories.Validation;

/// <summary>
///     Creates an Excel file summarising per-observation geographic coordinate resolution
///     for Virtual Herbarium validation reports.
/// </summary>
internal static class VirtualHerbariumGeoExcelWriter
{
    private static readonly Color HeaderBackground = Color.FromArgb(79, 129, 189);
    private static readonly Color HeaderForeground = Color.FromArgb(255, 255, 255);

    private static readonly string[] Headers =
    [
        "Province (verbatim)",
        "District (verbatim)",
        "Location (verbatim)",
        "Match",
        "CoordinateUncertaintyInMeters",
        "Province",
        "Municipality",
        "Parish",
        "Count"
    ];

    /// <summary>
    ///     Builds an Excel workbook from the supplied rows and returns it as a byte array.
    ///     Rows with identical field values are grouped and a Count column is added.
    /// </summary>
    public static byte[] CreateExcelBytes(IReadOnlyList<VirtualHerbariumGeoRow> rows)
    {
        ExcelPackage.License.SetNonCommercialOrganization("SLU");
        using var package = new ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add("Geo Mapping");

        var grouped = rows
            .GroupBy(r => (r.VerbatimProvince, r.VerbatimDistrict, r.VerbatimLocation, r.Match,
                           r.CoordinateUncertaintyInMeters, r.Province, r.Municipality, r.Parish))
            .Select(g => (Row: g.First(), Count: g.Count()))
            .OrderByDescending(x => x.Count)
            .ToList();

        WriteHeader(sheet);
        WriteRows(sheet, grouped);

        return package.GetAsByteArray();
    }

    private static void WriteHeader(ExcelWorksheet sheet)
    {
        for (int col = 1; col <= Headers.Length; col++)
        {
            var cell = sheet.Cells[1, col];
            cell.Value = Headers[col - 1];
            cell.Style.Font.Bold = true;
            cell.Style.Font.Color.SetColor(HeaderForeground);
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(HeaderBackground);
            sheet.Column(col).AutoFit(10, 60);
        }
    }

    private static void WriteRows(ExcelWorksheet sheet, List<(VirtualHerbariumGeoRow Row, int Count)> grouped)
    {
        for (int i = 0; i < grouped.Count; i++)
        {
            int row = i + 2;
            var r = grouped[i].Row;
            sheet.Cells[row, 1].Value = r.VerbatimProvince;
            sheet.Cells[row, 2].Value = r.VerbatimDistrict;
            sheet.Cells[row, 3].Value = r.VerbatimLocation;
            sheet.Cells[row, 4].Value = r.Match;
            sheet.Cells[row, 5].Value = r.CoordinateUncertaintyInMeters;
            sheet.Cells[row, 6].Value = r.Province;
            sheet.Cells[row, 7].Value = r.Municipality;
            sheet.Cells[row, 8].Value = r.Parish;
            sheet.Cells[row, 9].Value = grouped[i].Count;
        }

        if (grouped.Count > 0)
        {
            sheet.Cells[2, 5, grouped.Count + 1, 5].Style.Numberformat.Format = "0";
            sheet.Cells[2, 9, grouped.Count + 1, 9].Style.Numberformat.Format = "0";
        }
    }
}

