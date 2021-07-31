using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using katarabbitmq.bdd.tests.Helpers;
using RemoteControlledProcess;
using TechTalk.SpecFlow;
using Xunit;
using Xunit.Abstractions;

namespace katarabbitmq.bdd.tests.Steps
{
    [Binding]
    public class SharedStepDefinitions : IDisposable
    {
        private const string ExpectedMessageAfterRabbitMqConnected = "Established connection to RabbitMQ";

        public static List<TestProcessWrapper> Clients { get;  } = new();
        private readonly ITestOutputHelper _testOutputHelper;
        public static TestProcessWrapper Robot { get; private set; }

        private bool _isDisposed;

        public SharedStepDefinitions(ITestOutputHelper testOutputHelper) => _testOutputHelper = testOutputHelper;

        [Given(@"the robot and (.*) client are running")]
        [Given(@"the robot and (.*) clients are running")]
        public void GivenTheRobotAndClientsAreRunning(int numberOfClients)
        {
            var isCoverletEnabled = true;
            for (var clientIndex = 0; clientIndex < numberOfClients; clientIndex++)
            {
                var client = new TestProcessWrapper("kata-rabbitmq.client.app", isCoverletEnabled);
                client.TestOutputHelper = _testOutputHelper;

                client.AddEnvironmentVariable("RabbitMq__HostName", RabbitMq.Hostname);
                client.AddEnvironmentVariable("RabbitMq__Port",
                    RabbitMq.Port.ToString(CultureInfo.InvariantCulture));
                client.AddEnvironmentVariable("RabbitMq__UserName", RabbitMq.Username);
                client.AddEnvironmentVariable("RabbitMq__Password", RabbitMq.Password);

                client.AddReadinessCheck(output => output.Contains(ExpectedMessageAfterRabbitMqConnected));

                client.Start();

                Clients.Add(client);

                isCoverletEnabled = false;
            }

            Assert.True(Clients.All(c => c.IsRunning));

            Robot = new TestProcessWrapper("kata-rabbitmq.robot.app", true);
            Robot.TestOutputHelper = _testOutputHelper;

            Robot.AddEnvironmentVariable("RabbitMq__HostName", RabbitMq.Hostname);
            Robot.AddEnvironmentVariable("RabbitMq__Port",
                RabbitMq.Port.ToString(CultureInfo.InvariantCulture));
            Robot.AddEnvironmentVariable("RabbitMq__UserName", RabbitMq.Username);
            Robot.AddEnvironmentVariable("RabbitMq__Password", RabbitMq.Password);

            Robot.AddReadinessCheck(output => output.Contains(ExpectedMessageAfterRabbitMqConnected));

            Robot.Start();

            Assert.True(Robot.IsRunning);
        }

        public static void ShutdownProcessesGracefully()
        {
            SharedStepDefinitions.Robot.ShutdownGracefully();

            foreach (var client in SharedStepDefinitions.Clients)
            {
                client.ShutdownGracefully();
            }
        }

        [AfterScenario]
        public static void ForceProcessTermination()
        {
            Robot.ForceTermination();
            Robot.Dispose();
            Robot = null;

            foreach (var client in Clients)
            {
                client.ForceTermination();
                client.Dispose();
            }

            Clients.Clear();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~SharedStepDefinitions()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                Robot?.Dispose();
                foreach (var client in Clients)
                {
                    client?.Dispose();
                }
            }

            _isDisposed = true;
        }
    }
}