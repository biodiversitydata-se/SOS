using SOS.Lib.Enums;

namespace SOS.Lib.Models.Search.Result;
public class TimeSeriesHistogramResult
{
    public TimeSeriesType Type { get; set; }
    public int Period { get; set; }
    public int Observations { get; set; }
    public int Quantity { get; set; }
    public int Taxa { get; set; }
}
