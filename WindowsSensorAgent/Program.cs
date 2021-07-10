using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading;
using WindowsSensorAgent;

namespace WindowsSensorAgent
{
    class Program
    {
        [HandleProcessCorruptedStateExceptions]
        static void Main(string[] args)
        {
            StreamWriter writer = null;

            if (args.Length > 0)
            {
                writer = new StreamWriter(args[0]);
                writer.WriteLine("Starting SensorAgent");
                writer.Flush();
            } else
            {
                writer = new StreamWriter(Console.OpenStandardOutput());
            }

            try
            {
                CultureInfo cultureInfo = CultureInfo.GetCultureInfo("en-US"); // To get the correct formatting when serializing to JSON
                Thread.CurrentThread.CurrentCulture = cultureInfo;

                writer.WriteLine("Creating SensorProvider");
                writer.Flush();
                SensorProvider sensorProvider = new SensorProvider(writer);
                writer.WriteLine("Creating SensorReporter");
                writer.Flush();
                SensorReporter sensorReporter = new SensorReporter("sensor-relay.int.mindphaser.se", "windows-sensor-agent", "sensors");

                writer.WriteLine("Starting sensor publisher loop");
                while (true)
                {
                    writer.WriteLine("Getting sensor values");
                    writer.Flush();
                    Dictionary<String, String> sensorValues = sensorProvider.GetSensorValues();
                    writer.WriteLine("Publishing sensor values");
                    writer.Flush();
                    sensorReporter.report(sensorValues);

                    Thread.Sleep(1000);
                }
            } catch (Exception ex)
            {
                writer.WriteLine("Got exception: {0}", ex);
                writer.Flush();
            }
            writer.WriteLine("Closing");
            writer.Flush();
            writer.Close();
            Console.ReadLine();
        }
    }
}
