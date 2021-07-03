using System;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;
using System.Net.Http;
using OpenHardwareMonitor.Hardware;
using System.Text.Json;
using System.IO.MemoryMappedFiles;
using System.Net;

namespace Get_CPU_Temp5
{
    public class SensorReport
    {
        public String reporter { get; set; }
        public String topic { get; set; }
        public Dictionary<String, String> sensors { get; set; }
    }

    class FPSProvider
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        private MemoryMappedFile mmf;
        private MemoryMappedViewAccessor accessor;

        public FPSProvider()
        {
            mmf = MemoryMappedFile.OpenExisting("RTSSSharedMemoryV2");
            accessor = mmf.CreateViewAccessor();
        }

        public int GetFPS()
        {
            UInt32 version = accessor.ReadUInt32(4);
            uint size = accessor.ReadUInt32(8);
            uint offset = accessor.ReadUInt32(12);

            for (int i = 0; i < 256; i++)
            {
                long current_offset = offset + i * size;

                uint process_id = accessor.ReadUInt32(current_offset);

                IntPtr hWnd = GetForegroundWindow();
                IntPtr processIdPtr = new IntPtr(sizeof(UInt32));
                int ProcessIdTest;
                GetWindowThreadProcessId(hWnd, out ProcessIdTest);

                if (ProcessIdTest == process_id)
                {
                    int fps = RoundToClosest10((int)accessor.ReadUInt32(current_offset + 5024)) / 10;
                    return fps;
                }
            }

            return 0;
        }
        private int RoundToClosest10(int value)
        {
            int lower = (value / 10) * 10;
            int upper = (value / 10 + 1) * 10;

            int result = (value - lower < upper - value) ? lower : upper;

            return result;
        }
    }

    class SensorProvider
    {
        private Computer computer;
        public SensorProvider()
        {
            computer = new Computer();
            computer.Open();
            computer.CPUEnabled = true;
            computer.GPUEnabled = true;
            computer.MainboardEnabled = true;
            computer.RAMEnabled = true;
            computer.FanControllerEnabled = true;
            computer.HDDEnabled = true;
            // XXX - remember to close Computer()
        }

        public class UpdateVisitor : IVisitor
        {
            public void VisitComputer(IComputer computer)
            {
                computer.Traverse(this);
            }
            public void VisitHardware(IHardware hardware)
            {
                hardware.Update();
                foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
            }
            public void VisitSensor(ISensor sensor) { }
            public void VisitParameter(IParameter parameter) { }
        }
        public Dictionary<String, String> GetSensorValues()
        {
            UpdateVisitor updateVisitor = new UpdateVisitor();
            computer.Accept(updateVisitor);

            Dictionary<String, String> sensorValues = new Dictionary<string, string>();

            sensorValues.Add("cpu_voltage", "0");

            for (int i = 0; i < computer.Hardware.Length; i++)
            {
                {
                    for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                    {
                        ISensor sensor = computer.Hardware[i].Sensors[j];
                        if (sensor.SensorType == SensorType.Load && sensor.Name == "CPU Total")
                        {
                            sensorValues.Add("cpu_utilization", sensor.Value.ToString());

                        }
                        if (sensor.SensorType == SensorType.Temperature && sensor.Name == "CPU CCD Average")
                        {
                            sensorValues.Add("cpu_die_temp", sensor.Value.ToString());
                        }
                        if (sensor.SensorType == SensorType.Temperature && sensor.Name == "CPU Package")
                        {
                            sensorValues.Add("cpu_package_temp", sensor.Value.ToString());
                        }
                        if (sensor.SensorType == SensorType.Power && sensor.Name == "CPU Cores")
                        {
                            sensorValues.Add("cpu_power", sensor.Value.ToString());
                        }
                        if (sensor.SensorType == SensorType.Clock && sensor.Name == "CPU Core #1")
                        {
                            sensorValues.Add("cpu_frequency", sensor.Value.ToString());
                        }

                        if (sensor.SensorType == SensorType.Load && sensor.Name == "GPU Core")
                        {
                            sensorValues.Add("gpu_utilization", sensor.Value.ToString());
                        }
                        if (sensor.SensorType == SensorType.Temperature && sensor.Name == "GPU Core")
                        {
                            sensorValues.Add("gpu_die_temp", sensor.Value.ToString());
                        }
                        if (sensor.SensorType == SensorType.Temperature && sensor.Name == "GPU Hot Spot")
                        {
                            sensorValues.Add("gpu_package_temp", sensor.Value.ToString());
                        }
                        if (sensor.SensorType == SensorType.Power && sensor.Name == "GPU Total")
                        {
                            sensorValues.Add("gpu_power", sensor.Value.ToString());
                        }
                        if (sensor.SensorType == SensorType.Voltage && sensor.Name == "GPU Core")
                        {
                            sensorValues.Add("gpu_voltage", sensor.Value.ToString());
                        }
                        if (sensor.SensorType == SensorType.Clock && sensor.Name == "GPU Core")
                        {
                            sensorValues.Add("gpu_frequency", sensor.Value.ToString());
                        }

                        //Console.WriteLine(sensor.SensorType + " -- " + sensor.Name + ":" + sensor.Value.ToString() + "\r");
                    }
                }
            }

            return sensorValues;
        }

        class SensorReporter
        {
            private String relayHost;
            private HttpClient httpClient;

            public SensorReporter(String relayHost)
            {
                this.relayHost = relayHost;
                this.httpClient = new HttpClient();
            }

            public Boolean report(Dictionary<String, String> sensorValues)
            {
                SensorReport report = new SensorReport
                {
                    reporter = "windows-agent",
                    topic = "sensors",
                    sensors = sensorValues
                };

                string jsonString = JsonSerializer.Serialize(report);

                Console.WriteLine("Sending report: " + jsonString);

                StringContent content = new StringContent(jsonString);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                HttpResponseMessage res = httpClient.PostAsync(getRelayUrl("/publish"), content).Result;
                
                Console.WriteLine("Got result " + res.StatusCode);

                return res.StatusCode == HttpStatusCode.OK;
            }

            private String getRelayUrl(String path)
            {
                return String.Format("http://{0}{1}", relayHost, path);
            }
        }

        class Program
        {
            


            static void Main(string[] args)
            {
                CultureInfo cultureInfo = CultureInfo.GetCultureInfo("en-US");
                Thread.CurrentThread.CurrentCulture = cultureInfo;
                FPSProvider fpsProvider = new FPSProvider();
                SensorProvider sensorProvider = new SensorProvider();
                SensorReporter sensorReporter = new SensorReporter("sensor-relay.int.mindphaser.se");
                while (true)
                {
                    Dictionary<String, String> sensorValues = sensorProvider.GetSensorValues();
                    int fps = fpsProvider.GetFPS();
                    sensorValues.Add("gpu_fps", fps.ToString());

                    sensorReporter.report(sensorValues);

                    Thread.Sleep(1000);
                }
                Console.ReadLine();
            }
        }
    }
}
