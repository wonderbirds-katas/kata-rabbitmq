using System.Text;
using System.Threading;
using System.Threading.Tasks;
using katarabbitmq.infrastructure;
using katarabbitmq.model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace katarabbitmq.robot.app
{
    public class SensorDataSender : RabbitMqConnectedService
    {
        private readonly ILogger<SensorDataSender> _logger;
        private int _numberOfMeasurements;

        public SensorDataSender(IRabbitMqConnection rabbit, ILogger<SensorDataSender> logger)
            : base(rabbit, logger) =>
            _logger = logger;

        protected override async Task ExecuteSensorLoopBody(CancellationToken stoppingToken)
        {
            await base.ExecuteSensorLoopBody(stoppingToken);

            if (!Rabbit.IsConnected)
            {
                return;
            }

            SendMeasurement();
        }

        private void SendMeasurement()
        {
            ++_numberOfMeasurements;

            var measurement = new LightSensorValue { ambient = 7, sequenceNumber = _numberOfMeasurements };
            var message = JsonConvert.SerializeObject(measurement, Formatting.None);
            var body = Encoding.UTF8.GetBytes(message);

            Rabbit.Channel.BasicPublish("robot", "", null, body);

            // TODO: Fix static analysis warnings about deprecated logging mechanisms
#pragma warning disable CA1848
#pragma warning disable CA2254
            _logger.LogInformation($"Sensor data: '{message}'");
#pragma warning restore CA2254
#pragma warning restore CA1848
        }

        protected override void OnShutdownService()
        {
            base.OnShutdownService();
            // TODO: Fix static analysis warnings about deprecated logging mechanisms
#pragma warning disable CA1848
#pragma warning disable CA2254
            _logger.LogInformation($"Sent {_numberOfMeasurements} sensor values.");
#pragma warning restore CA2254
#pragma warning restore CA1848
        }
    }
}