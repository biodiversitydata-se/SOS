using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using SOS.Administration.Gui.Controllers;
using SOS.Administration.Gui.Models;
using SOS.Administration.Gui.Services;
using SOS.Lib.Configuration.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SOS.Administration.Gui
{
    public class ApTestOptionsMonitor : IOptionsMonitor<ApiTestConfiguration>
    {
        public ApTestOptionsMonitor(ApiTestConfiguration currentValue)
        {
            CurrentValue = currentValue;
        }

        public ApiTestConfiguration Get(string name)
        {
            return CurrentValue;
        }

        public IDisposable OnChange(Action<ApiTestConfiguration, string> listener)
        {
            throw new NotImplementedException();
        }

        public ApiTestConfiguration CurrentValue { get; }
    }
    
    public class PerformanceLoggerHostedService : IHostedService, IDisposable
    {
        private int executionCount = 0;
        private readonly ILogger<PerformanceLoggerHostedService> _logger;
        private readonly TestService _service;
        private readonly ElasticClient _elasticClient;
        private Timer _timer;
        private string _indexName = "";

        public PerformanceLoggerHostedService(ILogger<PerformanceLoggerHostedService> logger, IOptionsMonitor<ApiTestConfiguration> apitestconfiguration, IOptionsMonitor<ElasticSearchConfiguration> elasticConfiguration)
        {
            _logger = logger;
            _service = new TestService(apitestconfiguration.CurrentValue);
            _indexName = elasticConfiguration.CurrentValue.IndexPrefix + "-performance-data";
            _elasticClient = elasticConfiguration.CurrentValue.GetClient();            
            if(!_elasticClient.Indices.Exists(_indexName).Exists)
            {
                _elasticClient.Indices.Create(_indexName);
            }
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service running.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromMinutes(1));

            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            var count = Interlocked.Increment(ref executionCount);
            long totalTimeTakenMs = 0;
            foreach (var test in _service.GetTests())
            {
                PerformanceResult result = new PerformanceResult()
                {
                    TestId = test.Id,
                    Timestamp = DateTime.Now
                };
                var testResult = await test.RunTest();                
                result.TimeTakenMs = testResult.TimeTakenMs;
                totalTimeTakenMs += result.TimeTakenMs;
                var indexResult = await _elasticClient.IndexAsync<PerformanceResult>(result, p=>p.Index(_indexName));
            }
            PerformanceResult totalResult = new PerformanceResult()
            {
                TestId = -1,
                Timestamp = DateTime.Now,
                TimeTakenMs = totalTimeTakenMs
            };
            var indexResult2 = await _elasticClient.IndexAsync<PerformanceResult>(totalResult, p=>p.Index(_indexName));
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
