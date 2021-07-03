using System;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;
using System.Net.Http;
using OpenHardwareMonitor.Hardware;
using System.Text.Json;
using System.IO.MemoryMappedFiles;

namespace Get_CPU_Temp5
{
    public class SensorReport
    {
        public String reporter { get; set; }
        public String topic { get; set; }
        public Dictionary<String, String> sensors { get; set; }
    }

    class FPSProvider {
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

    class Program
    {
        static HttpClient client = new HttpClient();

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
        static void GetSystemInfo()
        {
            UpdateVisitor updateVisitor = new UpdateVisitor();
            Computer computer = new Computer();
            computer.Open();
            computer.CPUEnabled = true;
            computer.GPUEnabled = true;
            computer.MainboardEnabled = true;
            computer.RAMEnabled = true;
            computer.FanControllerEnabled = true;
            computer.HDDEnabled = true;
            computer.Accept(updateVisitor);

            Dictionary<String, String> sensorValues = new Dictionary<string, string>();

            /*
            sensor_values.insert("cpu_utilization".to_string(), rng.gen_range(0..100).to_string());
            sensor_values.insert("cpu_die_temp".to_string(), rng.gen_range(29..100).to_string());
            sensor_values.insert("cpu_package_temp".to_string(), rng.gen_range(29..100).to_string());
            sensor_values.insert("cpu_power".to_string(), rng.gen_range(19.0..250.0).to_string());
            sensor_values.insert("cpu_voltage".to_string(), rng.gen_range(0.0..2.5).to_string());
            sensor_values.insert("cpu_frequency".to_string(), rng.gen_range(-1..4900).to_string());
            */
            sensorValues.Add("cpu_voltage", "0");
/*            sensorValues.Add("gpu_utilization", "50");
            sensorValues.Add("gpu_die_temp", "50");
            sensorValues.Add("gpu_package_temp", "60");
            sensorValues.Add("gpu_power", "100");
            sensorValues.Add("gpu_voltage", "1.5");
            sensorValues.Add("gpu_frequency", "2000");*/

            for (int i = 0; i < computer.Hardware.Length; i++)
            {
                //if (computer.Hardware[i].HardwareType == HardwareType.CPU || computer.Hardware[i].HardwareType == HardwareType.GpuAti || computer.Hardware[i].HardwareType == HardwareType.Mainboard || computer.Hardware[i].HardwareType == HardwareType.RAM)
                {
                    for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                    {
                        //if (computer.Hardware[i].Sensors[j].SensorType == SensorType.Temperature)
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

                        Console.WriteLine(sensor.SensorType + " -- " + sensor.Name + ":" + sensor.Value.ToString() + "\r");
                        //Console.WriteLine("Hardware: " + sensor.Hardware.Name);
                    }
                }
            }
            computer.Close();

            SensorReport report = new SensorReport
            {
                reporter = "windows-agent",
                topic = "sensors",
                sensors = sensorValues
            };

            string jsonString = JsonSerializer.Serialize(report);

            Console.WriteLine("Sending request: " + jsonString);

            //client.BaseAddress = new Uri("http://sensor-relay.int.mindphaser.se/");

            StringContent content = new StringContent(jsonString);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue( "application/json");

            HttpResponseMessage res = client.PostAsync("http://sensor-relay.int.mindphaser.se/publish", content).Result;
            Console.WriteLine("Got result " + res.StatusCode);
        }
        static void Main(string[] args)
        {
            //            while (true)
            //            {
            CultureInfo cultureInfo = CultureInfo.GetCultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            FPSProvider fpsProvider = new FPSProvider();
            while (true)
            {
                GetSystemInfo();
                Thread.Sleep(1000);
            }
            Console.ReadLine();
//            }
        }
    }
}
