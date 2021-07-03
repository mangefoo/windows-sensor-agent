using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using WindowsSensorAgent;

namespace WindowsSensorAgent
{
    class Program
    {
        static void Main(string[] args)
        {
            CultureInfo cultureInfo = CultureInfo.GetCultureInfo("en-US"); // To get the correct formatting when serializing to JSON
            Thread.CurrentThread.CurrentCulture = cultureInfo;

            SensorProvider sensorProvider = new SensorProvider();
            SensorReporter sensorReporter = new SensorReporter("sensor-relay.int.mindphaser.se", "windows-sensor-agent", "sensors");

            while (true)
            {
                Dictionary<String, String> sensorValues = sensorProvider.GetSensorValues();
                sensorReporter.report(sensorValues);

                Thread.Sleep(1000);
            }

            Console.ReadLine();
        }
    }
}
