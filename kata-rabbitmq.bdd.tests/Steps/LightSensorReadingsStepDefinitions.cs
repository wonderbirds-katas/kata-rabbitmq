using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using kata_rabbitmq.robot.app;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions;
using TechTalk.SpecFlow;
using Xunit;
using Xunit.Abstractions;

namespace kata_rabbitmq.bdd.tests.Steps
{
    [Binding]
    public class LightSensorReadingsStepDefinitions
    {
        private bool _isSensorQueuePresent;
        private ITestOutputHelper _testOutputHelper;
        private Process _robotProcess;

        public LightSensorReadingsStepDefinitions(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [AfterScenario("LightSensorReadings")]
        public void AfterScenario()
        {
            _testOutputHelper.WriteLine("Stopping robot application ...");
            _robotProcess.StandardInput.WriteLine("123");
            _robotProcess.WaitForExit(2000);
            _testOutputHelper.WriteLine("OK");
        }

        [Given("the robot app is started")]
        public void GivenTheRobotAppIsStarted()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var robotAppRelativeDir = Path.Combine(currentDirectory, "..", "..", "..", "..", "kata-rabbitmq.robot.app", "bin", "Debug", "netcoreapp3.1");
            var robotAppFullDir = Path.GetFullPath(robotAppRelativeDir);
            var robotAppFullPath = Path.Combine(robotAppFullDir, "kata-rabbitmq.robot.app.dll");
            
            var robotProcessStartInfo = new ProcessStartInfo("dotnet");
            robotProcessStartInfo.UseShellExecute = false;
            robotProcessStartInfo.RedirectStandardInput = true;
            robotProcessStartInfo.Arguments = $"run \"{robotAppFullPath}\"";
            robotProcessStartInfo.WorkingDirectory = robotAppFullDir;

            robotProcessStartInfo.AddEnvironmentVariable("RABBITMQ_HOSTNAME", RabbitMq.Container.Hostname);
            robotProcessStartInfo.AddEnvironmentVariable("RABBITMQ_PORT", RabbitMq.Container.Port.ToString());
            robotProcessStartInfo.AddEnvironmentVariable("RABBITMQ_USERNAME", RabbitMq.Container.Username);
            robotProcessStartInfo.AddEnvironmentVariable("RABBITMQ_PASSWORD", RabbitMq.Container.Password);
            
            _robotProcess = Process.Start(robotProcessStartInfo);
            Thread.Sleep(1000);
        }
        
        [When("the sensor queue is checked")]
        public void WhenTheSensorQueueIsChecked()
        {
            try
            {
                _testOutputHelper.WriteLine("Testing whether robot:sensors exists ...");
                RabbitMq.Channel.ExchangeDeclarePassive("robot");
                RabbitMq.Channel.QueueDeclarePassive("sensors");

                _testOutputHelper.WriteLine("robot:sensors exists");
                _isSensorQueuePresent = true;
            }
            catch (Exception e)
            {
                _testOutputHelper.WriteLine($"robot:sensors does not exist. Exception: {e.Message}");
                _isSensorQueuePresent = false;
            }
        }

        [Then("the sensor queue exists")]
        public void ThenTheSensorsQueueExists()
        {
            Assert.True(_isSensorQueuePresent);
        }
    }
}
