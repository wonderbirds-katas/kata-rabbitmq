using System;
using System.Threading.Tasks;
using kata_rabbitmq.robot.app;
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
        private Task _mainTask;

        public LightSensorReadingsStepDefinitions(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            Environment.SetEnvironmentVariable("RABBITMQ_HOSTNAME", RabbitMq.Container.Hostname);
            Environment.SetEnvironmentVariable("RABBITMQ_PORT", RabbitMq.Container.Port.ToString());
            Environment.SetEnvironmentVariable("RABBITMQ_USERNAME", RabbitMq.Container.Username);
            Environment.SetEnvironmentVariable("RABBITMQ_PASSWORD", RabbitMq.Container.Password);

            _testOutputHelper.WriteLine("Running the Program.Main function ...");
            _mainTask = Program.Main(null);
        }

        [AfterScenario]
        public void AfterScenario()
        {
            _testOutputHelper.WriteLine("Requesting Program.Exit ...");
            Program.Exit();
            _mainTask.Wait();
            _testOutputHelper.WriteLine("Program.Main has exited");
        }

        [Given("the robot app is started")]
        public void GivenTheRobotAppIsStarted()
        {
        }
        
        [When("the sensor queue is checked")]
        public void WhenTheSensorQueueIsChecked()
        {
            try
            {
                _testOutputHelper.WriteLine("Testing whether robot:sensors exists ...");
                RabbitMq.Channel.ExchangeDeclarePassive("robot");
                RabbitMq.Channel.QueueDeclarePassive("sensors");
                _isSensorQueuePresent = true;
                _testOutputHelper.WriteLine("robot:sensors exists");
            }
            catch (Exception e)
            {
                // If one of the passive *Declare* functions fails, then the exchange
                // or channel does not exist.
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
