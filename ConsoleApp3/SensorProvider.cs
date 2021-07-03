using System;
using System.Collections.Generic;
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

            int fps = fpsProvider.GetFPS();
            sensorValues.Add("gpu_fps", fps.ToString());

            return sensorValues;
        }
    }
}
