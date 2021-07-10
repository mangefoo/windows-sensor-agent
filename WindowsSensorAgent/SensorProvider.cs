using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenHardwareMonitor.Hardware;

namespace WindowsSensorAgent
{
    public class SensorProvider
    {
        private Computer computer;
        private FPSProvider fpsProvider;
        public SensorProvider(StreamWriter writer)
        {
            writer.WriteLine("Creating Computer");
            writer.Flush();
            computer = new Computer();
            computer.Open();
            computer.CPUEnabled = true;
            computer.GPUEnabled = true;
            computer.MainboardEnabled = true;
            computer.RAMEnabled = true;
            computer.FanControllerEnabled = true;
            computer.HDDEnabled = true;

            writer.WriteLine("Creating FPSProvider");
            writer.Flush();
            try
            {
                fpsProvider = new FPSProviderSharedMemory(writer);
                writer.WriteLine("Created FPSProvider");
                writer.Flush();
            } catch (Exception ex)
            {
                writer.WriteLine("Failed to create FPS provider: {0}", ex);
                writer.Flush();
            }
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

            var sensorMap = new Dictionary<SensorType, Dictionary<string, string>>()
            {
                { 
                    SensorType.Load, new Dictionary<string, string>() {
                        { "CPU Total", "cpu_utilization" },
                        { "GPU Core", "gpu_utilization" },
                        { "CPU Core #1", "cpu_core_load_1" },
                        { "CPU Core #2", "cpu_core_load_2" },
                        { "CPU Core #3", "cpu_core_load_3" },
                        { "CPU Core #4", "cpu_core_load_4" },
                        { "CPU Core #5", "cpu_core_load_5" },
                        { "CPU Core #6", "cpu_core_load_6" },
                        { "CPU Core #7", "cpu_core_load_7" },
                        { "CPU Core #8", "cpu_core_load_8" },
                        { "CPU Core #9", "cpu_core_load_9" },
                        { "CPU Core #10", "cpu_core_load_10" },
                        { "CPU Core #11", "cpu_core_load_11" },
                        { "CPU Core #12", "cpu_core_load_12" },
                        { "CPU Core #13", "cpu_core_load_13" },
                        { "CPU Core #14", "cpu_core_load_14" },
                        { "CPU Core #15", "cpu_core_load_15" },
                        { "CPU Core #16", "cpu_core_load_16" }
                    } 
                },
                {
                    SensorType.Temperature, new Dictionary<string, string>()
                    {
                        { "CPU CCD Average", "cpu_die_temp" },
                        { "CPU Package", "cpu_package_temp" },
                        { "GPU Core", "gpu_die_temp" },
                        { "GPU Hot Spot", "gpu_package_temp" }
                    }
                },
                {
                    SensorType.Power, new Dictionary<string, string>()
                    {
                        { "CPU Cores", "cpu_power" },
                        { "GPU Total", "gpu_power" },
                        { "CPU Core #1", "cpu_core_power_1" },
                        { "CPU Core #2", "cpu_core_power_2" },
                        { "CPU Core #3", "cpu_core_power_3" },
                        { "CPU Core #4", "cpu_core_power_4" },
                        { "CPU Core #5", "cpu_core_power_5" },
                        { "CPU Core #6", "cpu_core_power_6" },
                        { "CPU Core #7", "cpu_core_power_7" },
                        { "CPU Core #8", "cpu_core_power_8" },
                        { "CPU Core #9", "cpu_core_power_9" },
                        { "CPU Core #10", "cpu_core_power_10" },
                        { "CPU Core #11", "cpu_core_power_11" },
                        { "CPU Core #12", "cpu_core_power_12" },
                        { "CPU Core #13", "cpu_core_power_13" },
                        { "CPU Core #14", "cpu_core_power_14" },
                        { "CPU Core #15", "cpu_core_power_15" },
                        { "CPU Core #16", "cpu_core_power_16" }
                    }
                },
                {
                    SensorType.Clock, new Dictionary<string, string>()
                    {
                        { "GPU Core", "gpu_frequency" },
                        { "CPU Core #1", "cpu_core_frequency_1" },
                        { "CPU Core #2", "cpu_core_frequency_2" },
                        { "CPU Core #3", "cpu_core_frequency_3" },
                        { "CPU Core #4", "cpu_core_frequency_4" },
                        { "CPU Core #5", "cpu_core_frequency_5" },
                        { "CPU Core #6", "cpu_core_frequency_6" },
                        { "CPU Core #7", "cpu_core_frequency_7" },
                        { "CPU Core #8", "cpu_core_frequency_8" },
                        { "CPU Core #9", "cpu_core_frequency_9" },
                        { "CPU Core #10", "cpu_core_frequency_10" },
                        { "CPU Core #11", "cpu_core_frequency_11" },
                        { "CPU Core #12", "cpu_core_frequency_12" },
                        { "CPU Core #13", "cpu_core_frequency_13" },
                        { "CPU Core #14", "cpu_core_frequency_14" },
                        { "CPU Core #15", "cpu_core_frequency_15" },
                        { "CPU Core #16", "cpu_core_frequency_16" }
                    }
                },
                {
                    SensorType.Voltage, new Dictionary<string, string>()
                    {
                        { "GPU Core", "gpu_voltage" }
                    }
                },
                {
                    SensorType.Data, new Dictionary<string, string>()
                    {
                        { "Used Memory", "mem_used" },
                        { "Available Memory", "mem_available" }
                    }
                }
            };

            for (int i = 0; i < computer.Hardware.Length; i++)
            {
                {
                    for (int j = 0; j < computer.Hardware[i].Sensors.Length; j++)
                    {
                        ISensor sensor = computer.Hardware[i].Sensors[j];

                        if (sensorMap.ContainsKey(sensor.SensorType) && sensorMap[sensor.SensorType].ContainsKey(sensor.Name))
                        {
                            sensorValues.Add(sensorMap[sensor.SensorType][sensor.Name], sensor.Value.ToString());
                        }

                        Console.WriteLine(sensor.SensorType + " -- " + sensor.Name + " - " + sensor.Identifier + ":" + sensor.Value.ToString() + "\r");
                    }
                }
            }

            if (fpsProvider != null)
            {
                Console.WriteLine("Getting FPS");
                int fps = fpsProvider.GetFPS();
                sensorValues.Add("gpu_fps", fps.ToString());
                Console.WriteLine("Got FPS {0}", fps);
            }

            return sensorValues;
        }
    }
}
