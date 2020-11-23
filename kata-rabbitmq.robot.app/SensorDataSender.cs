using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace kata_rabbitmq.robot.app
{
    public class SensorDataSender : BackgroundService
    {
        private readonly ILogger<SensorDataSender> _logger;

        public SensorDataSender(ILogger<SensorDataSender> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Waiting for cancellation request");
            stoppingToken.Register(() => _logger.LogInformation("STOP request received"));
            stoppingToken.ThrowIfCancellationRequested();

            IModel channel = null;
            IConnection connection = null;

            try
            {
                while (true)
                {
                    if (channel == null)
                    {
                        _logger.LogDebug("Connecting to RabbitMQ ...");
                        try
                        {
                            var connectionFactory = new ConnectionFactory();
                            connectionFactory.HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOSTNAME");
                            var portString = Environment.GetEnvironmentVariable("RABBITMQ_PORT");
                            if (portString != null) connectionFactory.Port = int.Parse(portString);
                            connectionFactory.UserName = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME");
                            connectionFactory.Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD");
                            connectionFactory.VirtualHost = "/";
                            connectionFactory.ClientProvidedName = "app:robot";
                            
                            _logger.LogDebug($"RabbitMQ HostName: {connectionFactory.HostName}");
                            _logger.LogDebug($"RabbitMQ Port: {connectionFactory.Port}");
                            _logger.LogDebug($"RabbitMQ UserName: {connectionFactory.UserName}");

                            connection = connectionFactory.CreateConnection();
                            channel = connection.CreateModel();

                            channel.ExchangeDeclare("robot", ExchangeType.Direct, durable: false, autoDelete: true,
                                arguments: null);
                            channel.QueueDeclare("sensors", durable: false, exclusive: false, autoDelete: true,
                                arguments: null);

                            _logger.LogInformation("Established connection to RabbitMQ");
                        }
                        catch (Exception e)
                        {
                            _logger.LogDebug(e.Message);
                            channel = null;
                            connection = null;
                        }
                    }

                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // This exception is desired, when shutdown is requested. No action is necessary.
            }
            catch (Exception e)
            {
                _logger.LogCritical(e.ToString());
            }
            finally
            {
                _logger.LogInformation("Shutting down ...");

                channel?.Close();
                connection?.Close();
                
                _logger.LogDebug("Shutdown complete.");
            } 
        }
    }
}