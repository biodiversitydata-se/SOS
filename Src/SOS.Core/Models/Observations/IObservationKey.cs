namespace SOS.Core.Models.Observations
{
    public interface IObservationKey
    {
        int DataProviderId { get; set; }
        string CatalogNumber { get; set; }
    }
}
