
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace SOS.Administration.Api.Controllers.Interfaces;

public interface IIptController
{
    /// <summary>
    /// Get all resources
    /// </summary>
    /// <returns></returns>
    Task<IActionResult> GetResourcesAsync();
}
