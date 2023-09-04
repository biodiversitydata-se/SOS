namespace SOS.Observations.Api.IntegrationTests.Helpers;
internal static class CsvHelper
{
    public static List<Dictionary<string, string>> ReadCsvFile(byte[] csvFileBytes)
    {
        var items = new List<Dictionary<string, string>>();
        using var readMemoryStream = new MemoryStream(csvFileBytes);
        using var streamRdr = new StreamReader(readMemoryStream);
        var csvReader = new NReco.Csv.CsvReader(streamRdr, "\t");
        var columnIdByHeader = new Dictionary<string, int>();
        var headerByColumnId = new Dictionary<int, string>();

        // Read header
        csvReader.Read();
        for (int i = 0; i < csvReader.FieldsCount; i++)
        {
            string val = csvReader[i];
            columnIdByHeader.Add(val, i);
            headerByColumnId.Add(i, val);
        }

        // Read data
        while (csvReader.Read())
        {
            var item = new Dictionary<string, string>();
            for (int i = 0; i < csvReader.FieldsCount; i++)
            {
                string val = csvReader[i];
                item.Add(headerByColumnId[i], val);
            }

            items.Add(item);
        }

        return items;
    }
}
