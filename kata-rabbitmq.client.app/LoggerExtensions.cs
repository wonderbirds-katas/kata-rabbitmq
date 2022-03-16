using System;
using katarabbitmq.model;
using Microsoft.Extensions.Logging;

namespace katarabbitmq.client.app;

public static class LoggerExtensions
{
    private static readonly Action<ILogger, LightSensorValue, Exception> SensorDataAction;

    static LoggerExtensions() =>
        SensorDataAction = LoggerMessage.Define<LightSensorValue>(LogLevel.Information, new EventId(1, nameof(SensorData)),
            "Sensor data: {Measurement}");

    public static void SensorData(this ILogger logger, LightSensorValue measurement) => SensorDataAction(logger, measurement, null);
}