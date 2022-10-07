using SOS.DataStewardship.Api.Models;

namespace SOS.DataStewardship.Api.Managers.Interfaces;

public interface IDataStewardshipManager
{
    Task<Dataset> GetDatasetByIdAsync(string id);

}
