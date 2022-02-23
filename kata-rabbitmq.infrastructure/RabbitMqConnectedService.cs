using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace katarabbitmq.infrastructure
{
    public abstract class RabbitMqConnectedService : BackgroundService
    {
        protected RabbitMqConnectedService(IRabbitMqConnection rabbit, ILogger<RabbitMqConnectedService> logger)
        {
            Rabbit = rabbit;
            Logger = logger;
        }

        protected IRabbitMqConnection Rabbit { get; }

        protected ILogger<RabbitMqConnectedService> Logger { get; }

        protected TimeSpan DelayAfterEachLoop { get; init; } = TimeSpan.FromMilliseconds(50.0);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                RegisterCancellationRequest(stoppingToken);

                while (true)
                {
                    await ExecuteSensorLoopBody(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // This exception is desired, when shutdown is requested. No action is necessary.
            }
            catch (Exception e)
            {
                // TODO: Fix static analysis warnings about deprecated logging mechanisms
#pragma warning disable CA1848
#pragma warning disable CA2254
                Logger.LogCritical(e.ToString());
#pragma warning restore CA2254
#pragma warning restore CA1848
            }
            finally
            {
                ShutdownService();
            }
        }

        private void RegisterCancellationRequest(CancellationToken stoppingToken)
        {
            // TODO: Fix static analysis warnings about deprecated logging mechanisms
#pragma warning disable CA1848
            Logger.LogInformation("Waiting for cancellation request");
            stoppingToken.Register(() => Logger.LogInformation("STOP request received"));
#pragma warning restore CA1848
            stoppingToken.ThrowIfCancellationRequested();
        }

        protected virtual async Task ExecuteSensorLoopBody(CancellationToken stoppingToken)
        {
            if (!Rabbit.IsConnected)
            {
                Rabbit.TryConnect();
            }

            await Task.Delay(DelayAfterEachLoop, stoppingToken);
        }

        private void ShutdownService()
        {
            // TODO: Fix static analysis warnings about deprecated logging mechanisms
#pragma warning disable CA1848
            Logger.LogInformation("Shutting down ...");

            Rabbit.Disconnect();
            OnShutdownService();

            Logger.LogDebug("Shutdown complete.");
#pragma warning restore CA1848
        }

        protected virtual void OnShutdownService()
        {
        }
    }
}