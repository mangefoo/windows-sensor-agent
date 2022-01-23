using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace WindowsSensorAgentService
{
    public partial class SensorAgentService : ServiceBase
    {
        private int processId;
        private string filePath;
        public SensorAgentService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            var location = new Uri(Assembly.GetEntryAssembly().CodeBase).LocalPath;
            var path = Path.GetDirectoryName(location);
            var serverPath = "C:\\Users\\eriks\\source\\repos\\WindowsSensorAgent\\Output\\WindowsSensorAgent.exe";
            Process cmd = new Process();
            cmd.StartInfo.FileName = serverPath;
            cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            using (var f = File.Create(Path.Combine(path, "TestFile.txt")))
            {
                filePath = f.Name;
            }

            cmd.StartInfo.Arguments = filePath;
            cmd.Start();
            processId = cmd.Id;
        }

        protected override void OnStop()
        {
            Process process = null;
            try
            {
                process = Process.GetProcessById((int)processId);
            }
            catch
            {
            }
            finally
            {
                if (process != null)
                {
                    process.Kill();
                    process.WaitForExit();
                    process.Dispose();
                }

                File.Delete(filePath);
            }
        }
    }
}
