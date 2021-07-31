using System.Globalization;
using RemoteControlledProcess;

namespace katarabbitmq.bdd.tests.Helpers
{
    public static class RabbitMqExtensions
    {
        private const string ExpectedMessageAfterRabbitMqConnected = "Established connection to RabbitMQ";

        public static void ConfigureRabbitMq(this TestProcessWrapper testProcess)
        {
            testProcess.AddEnvironmentVariable("RabbitMq__HostName", RabbitMq.Hostname);
            testProcess.AddEnvironmentVariable("RabbitMq__Port",
                RabbitMq.Port.ToString(CultureInfo.InvariantCulture));
            testProcess.AddEnvironmentVariable("RabbitMq__UserName", RabbitMq.Username);
            testProcess.AddEnvironmentVariable("RabbitMq__Password", RabbitMq.Password);

            testProcess.AddReadinessCheck(output => output.Contains(ExpectedMessageAfterRabbitMqConnected));

        }
    }
}