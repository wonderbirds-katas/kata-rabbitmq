using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace katarabbitmq.infrastructure
{
    public class LogApplicationInfoService : BackgroundService
    {
        private readonly ILogger<LogApplicationInfoService> _logger;

        public LogApplicationInfoService(ILogger<LogApplicationInfoService> logger) => _logger = logger;

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var processId = Environment.ProcessId;
            // TODO: Fix static analysis warnings about deprecated logging mechanisms
#pragma warning disable CA1848
#pragma warning disable CA2254
            _logger.LogInformation($"Process ID {processId}");
#pragma warning restore CA2254
#pragma warning restore CA1848

            return Task.CompletedTask;
        }
    }
}