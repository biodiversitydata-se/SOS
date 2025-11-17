using Microsoft.AspNetCore.Mvc;
using SOS.Administration.Api.Models;
using System.Threading.Tasks;

namespace SOS.Administration.Api.Controllers.Interfaces;

/// <summary>
///     Interface for validation controller
/// </summary>
public interface IValidationController
{
    /// <summary>
    ///     Create data validation report for DwC-A files.
    /// </summary>
    /// <returns></returns>
    Task<IActionResult> RunDwcaDataValidationJob([FromForm] CreateDwcaDataValidationReportDto model);
}