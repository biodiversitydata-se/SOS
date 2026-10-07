using Newtonsoft.Json;
using OfficeOpenXml;
using SOS.Api.Contracts.Lookups.Responses;
using SOS.Api.Contracts.Exports.Requests;
using OfficeOpenXml.Style;
using SOS.Lib.Models;
using SOS.Lib.Models.DataValidation;
using SOS.Lib.Models.Processed.Observation;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace SOS.Lib.Helpers;

/// <summary>
/// Creates an Excel file from valid observations in a data validation report.
/// Mirrors the style and field resolution of <see cref="SOS.Lib.IO.Excel.ExcelFileWriter"/>.
/// </summary>
public static class ValidObservationsExcelWriter
{
    private const string RawDataColumnHeader = "RawData";

    /// <summary>
    /// Creates an Excel file containing all valid observations.
    /// The first column is "RawData" with the serialized verbatim observation.
    /// The remaining columns are all standard observation export fields.
    /// </summary>
    public static async Task<byte[]> CreateExcelFileAsync(
        IEnumerable<ValidObservationTuple<object, Observation>> validObservations,
        PropertyLabelType propertyLabelType = PropertyLabelType.English)
    {
        ExcelPackage.License.SetNonCommercialOrganization("SLU");
        using var memoryStream = new MemoryStream();
        using var package = new ExcelPackage(memoryStream);
        var sheet = package.Workbook.Worksheets.Add("Valid observations");

        var propertyFields = ObservationPropertyFieldDescriptionHelper
            .GetExportFieldsFromOutputFields(outputFields: null);

        WriteHeader(sheet, propertyFields, propertyLabelType);        
        WriteData(sheet, propertyFields, validObservations);

        await package.SaveAsync();
        return memoryStream.ToArray();
    }

    private static void WriteHeader(
        ExcelWorksheet sheet,
        IList<PropertyFieldDescription> propertyFields,
        PropertyLabelType propertyLabelType)
    {
        // Column 1: RawData
        sheet.Cells[1, 1].Value = RawDataColumnHeader;
        ApplyHeaderCellStyle(sheet.Cells[1, 1]);
        sheet.Column(1).AutoFit(10, 70);

        // Columns 2+: standard observation fields
        for (int i = 0; i < propertyFields.Count; i++)
        {
            int col = i + 2;
            string title = ObservationPropertyFieldDescriptionHelper.GetPropertyLabel(propertyFields[i], propertyLabelType);
            sheet.Cells[1, col].Value = title;
            ApplyHeaderCellStyle(sheet.Cells[1, col]);
            sheet.Column(col).AutoFit(10, 70);
        }
    }

    private static void ApplyHeaderCellStyle(ExcelRange cell)
    {
        cell.Style.Font.Bold = true;
        cell.Style.Font.Color.SetColor(Color.FromArgb(255, 255, 255));
        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
        cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));
    }  

    private static void WriteData(
        ExcelWorksheet sheet,
        IList<PropertyFieldDescription> propertyFields,
        IEnumerable<ValidObservationTuple<object, Observation>> validObservations)
    {
        int rowIndex = 2;
        foreach (var tuple in validObservations)
        {
            if (tuple.ProcessedObservation == null) continue;

            // Column 1: serialized verbatim (raw) observation
            sheet.Cells[rowIndex, 1].Value = tuple.VerbatimObservation != null
                ? JsonConvert.SerializeObject(tuple.VerbatimObservation)
                : null;

            // Columns 2+: processed observation fields
            var flatObservation = new FlatObservation(tuple.ProcessedObservation);
            for (int i = 0; i < propertyFields.Count; i++)
            {
                int col = i + 2;
                var propertyField = propertyFields[i];
                var value = flatObservation.GetValue(propertyField);
                object cellValue = value == null ? null : propertyField.DataTypeEnum switch
                {
                    PropertyFieldDataType.Boolean => ((bool?)value)?.ToString(CultureInfo.InvariantCulture),
                    PropertyFieldDataType.DateTime => ((DateTime?)value)?.ToShortDateString(),
                    _ => value
                };

                sheet.Cells[rowIndex, col].Value = cellValue;
            }

            rowIndex++;
        }
    }
}
