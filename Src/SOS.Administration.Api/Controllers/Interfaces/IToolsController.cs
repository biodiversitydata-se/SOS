using Microsoft.AspNetCore.Mvc;

namespace SOS.Administration.Api.Controllers.Interfaces;

/// <summary>
///     Cache controller
/// </summary>
public interface IToolsController
{
    /// <summary>
    /// Schedule cleanup job
    /// </summary>
    /// <param name="runIntervalInMinutes"></param>
    /// <returns></returns>
    IActionResult ScheduleCleanUpJob(byte runIntervalInMinutes);
}