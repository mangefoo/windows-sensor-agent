using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsSensorAgent;

namespace WindowsSensorAgentForm
{
    static class Program
    {
        static Thread sensorPublisherThread;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            sensorPublisherThread = new Thread(SensorPublisher);
            sensorPublisherThread.Start();

            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

            Console.WriteLine("Terminating thread");
            sensorPublisherThread.Abort();
            sensorPublisherThread.Join();
            Console.WriteLine("Terminated");
        }

        static void SensorPublisher()
        {
            StreamWriter writer = new StreamWriter(Console.OpenStandardOutput());
            
            try
            {
                CultureInfo cultureInfo = CultureInfo.GetCultureInfo("en-US"); // To get the correct formatting when serializing to JSON
                Thread.CurrentThread.CurrentCulture = cultureInfo;

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
            }
            catch (Exception ex)
            {
                writer.WriteLine("Got exception: {0}", ex);
                writer.Flush();
            }
            writer.WriteLine("Closing");
            writer.Flush();
            writer.Close();
        }
    }
}
