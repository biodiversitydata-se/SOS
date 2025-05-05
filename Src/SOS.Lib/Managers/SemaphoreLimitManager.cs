using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SOS.Lib.Managers;
public class SemaphoreLimitManager
{
    private readonly SemaphoreLimitsConfiguration _semaphoreLimitsConfiguration;
    private readonly ILogger<SemaphoreLimitManager> _logger;
    private readonly Dictionary<ApiUserType, SemaphoreSlim> _observationSemaphores = new();
    private readonly Dictionary<ApiUserType, SemaphoreSlim> _aggregationSemaphores = new ();
    public readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(DefaultTimeoutSeconds);
    public const int DefaultTimeoutSeconds = 50;

    public SemaphoreLimitManager()
    {
        _observationSemaphores = new Dictionary<ApiUserType, SemaphoreSlim>
        {
            { ApiUserType.Unknown, new SemaphoreSlim(5) }
        };
        _aggregationSemaphores = new Dictionary<ApiUserType, SemaphoreSlim>
        {
            { ApiUserType.Unknown, new SemaphoreSlim(5) }
        };
    }

    public SemaphoreLimitManager(
        SemaphoreLimitsConfiguration semaphoreLimitsConfiguration,
        ILogger<SemaphoreLimitManager> logger)
    {
        _semaphoreLimitsConfiguration = semaphoreLimitsConfiguration ?? throw new ArgumentNullException(nameof(semaphoreLimitsConfiguration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        foreach (var pair in _semaphoreLimitsConfiguration.Observation)
        {
            _observationSemaphores.Add(pair.Key, new SemaphoreSlim(pair.Value));
        }
        foreach (var pair in _semaphoreLimitsConfiguration.Aggregation)
        {
            _aggregationSemaphores.Add(pair.Key, new SemaphoreSlim(pair.Value));
        }
    }

    public async Task<SemaphoreResult> GetSemaphoreAsync(SemaphoreType semaphoreType, ApiUserType userType, string endpointName)
    {
        var semaphoreResult = new SemaphoreResult();        
        if (semaphoreType == SemaphoreType.Aggregation)
        {
            semaphoreResult.Semaphore = GetAggregationSemaphore(userType);
        }
        else
        {
            semaphoreResult.Semaphore = GetObservationSemaphore(userType);
        }

        var semaphoreTime = Stopwatch.StartNew();
        if (semaphoreResult.Semaphore.CurrentCount == 0)
        {
            _logger.LogWarning("All semaphore slots are occupied. Request will be queued. Endpoint={endpoint}, UserType={@userType}", endpointName, userType);
            semaphoreResult.SemaphoreStatus = "Wait";
        }

        if (!await semaphoreResult.Semaphore.WaitAsync(DefaultTimeout).ConfigureAwait(true))
        {
            _logger.LogError("Too many requests. Semaphore limit reached. Endpoint={endpoint}, UserType={@userType}", endpointName, userType);
            semaphoreResult.SemaphoreStatus = "Timeout";
            semaphoreTime.Stop();
            semaphoreResult.SemaphoreWaitTime = semaphoreTime.Elapsed;
            semaphoreResult.Semaphore = null;
            return semaphoreResult;
        }

        semaphoreTime.Stop();
        semaphoreResult.SemaphoreWaitTime = semaphoreTime.Elapsed;
        return semaphoreResult;
    }

    private SemaphoreSlim GetObservationSemaphore(ApiUserType userType)
    {
        if (_observationSemaphores.TryGetValue(userType, out var semaphore))
        {
            return semaphore;
        }
        else
        {
            _logger.LogWarning($"No observation semaphore found for user type {userType}");
            if (_observationSemaphores.TryGetValue(ApiUserType.Unknown, out var unknownSemaphore))
            {
                return unknownSemaphore;
            }
            else
            {
                throw new KeyNotFoundException($"No observation semaphore found for user type {userType}");
            }
        }
    }

    private SemaphoreSlim GetAggregationSemaphore(ApiUserType userType)
    {
        if (_aggregationSemaphores.TryGetValue(userType, out var semaphore))
        {
            return semaphore;
        }
        else
        {
            _logger.LogWarning($"No aggregation semaphore found for user type {userType}");
            if (_aggregationSemaphores.TryGetValue(ApiUserType.Unknown, out var unknownSemaphore))
            {
                return unknownSemaphore;
            }
            else
            {
                throw new KeyNotFoundException($"No aggregation semaphore found for user type {userType}");
            }
        }
    }
}
