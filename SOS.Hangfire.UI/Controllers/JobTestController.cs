using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SOS.Core.Jobs;

namespace SOS.Hangfire.UI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobTestController : ControllerBase
    {
        private readonly IBackgroundJobClient _client;

        public JobTestController(IBackgroundJobClient client)
        {
            _client = client;
        }

        [HttpPost("AddSharkRecurringJobOnceEveryNight")]
        public ActionResult<IEnumerable<string>> AddSharkRecurringJobOnceEveryNight()
        {
            RecurringJob.AddOrUpdate<SharkHarvestJob>(nameof(SharkHarvestJob), job => job.Run(), "0 4 * * *", TimeZoneInfo.Local);
            return Ok("Shark recurring job successfully added");
        }

        [HttpPost("AddSharkRecurringJobEveryMinute")]
        public ActionResult<IEnumerable<string>> AddSharkRecurringJobEveryMinute()
        {
            RecurringJob.AddOrUpdate<SharkHarvestJob>(nameof(SharkHarvestJob), job => job.Run(), Cron.Minutely, TimeZoneInfo.Local);
            return Ok("Shark recurring job successfully added");
        }

        [HttpPost("AddMvmRecurringJobEveryHour")]
        public ActionResult<IEnumerable<string>> AddMvmRecurringJobEveryHour()
        {
            RecurringJob.AddOrUpdate<MvmHarvestJob>(nameof(MvmHarvestJob), job => job.Run(), Cron.Hourly, TimeZoneInfo.Local);
            return Ok("MVM recurring job successfully added");
        }
    }
}