using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SOS.Core;
using SOS.Core.Jobs;
using SOS.Core.Models.Observations;
using SOS.Core.Repositories;

namespace SOS.Hangfire.UI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobTestController : ControllerBase
    {
        private const string MongoUrl = "mongodb://localhost";
        private const string DatabaseName = "diff_sample";
        private readonly IBackgroundJobClient _client;

        public JobTestController(IBackgroundJobClient client)
        {
            _client = client;
        }

        [HttpPost("AddVerbatimTestDataProviderHarvestJob")]
        public ActionResult<IEnumerable<string>> AddVerbatimTestDataProviderHarvestJobRunOnce(int nrObservations)
        {
            BackgroundJob.Enqueue<VerbatimTestDataHarvestJob>(job => job.Run(nrObservations));
            return Ok("VerbatimTestDataProvider observation harvest job added");
        }

        [HttpPost("AddVerbatimTestDataProviderProcessJob")]
        public ActionResult<IEnumerable<string>> AddVerbatimTestDataProviderProcessJob()
        {
            BackgroundJob.Enqueue<VerbatimTestDataProcessJob>(job => job.Run());
            return Ok("VerbatimTestDataProvider observation process job added");
        }


        [HttpPost("AddVerbatimTestDataProviderHarvestJobAndContinueWithProcessing")]
        public ActionResult<IEnumerable<string>> AddVerbatimTestDataProviderHarvestJobAndContinueWithProcessing(int nrObservations)
        {
            string jobId = BackgroundJob.Enqueue<VerbatimTestDataHarvestJob>(job => job.Run(nrObservations));
            BackgroundJob.ContinueJobWith<VerbatimTestDataProcessJob>(
                jobId,
                job => job.Run());

            return Ok("VerbatimTestDataProvider observation harvest job added with processing continuation");
        }

        [HttpPost("DropProcessedObservationCollection")]
        public ActionResult<string> DropProcessedObservationCollection()
        {
            MongoDbContext observationsDbContext = new MongoDbContext(MongoUrl, DatabaseName, Constants.ObservationCollectionName);
            var observationRepository = new VersionedObservationRepository<ProcessedDwcObservation>(observationsDbContext);
            observationRepository.DropObservationCollectionAsync().Wait();
            return Ok("Processed observation collection was deleted");
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