using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace WindowsSensorAgent
{
    class NetworkInfoProvider
    {
        static Dictionary<string, long> previousValues = new Dictionary<string, long>();

        public static Dictionary<String, String> GetNetworkInfo()
        {
            List<string> interestingDevices = new List<string>() {"ethernet", "wi-fi"};

            Dictionary<string, string> values = new Dictionary<string, string>();
            if (!NetworkInterface.GetIsNetworkAvailable())
                return values;

            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            int interfaceIndex = 1;
            foreach (NetworkInterface ni in interfaces.Where(ni => interestingDevices.Contains(ni.Name.ToLower()))) {
                string interfaceName = ni.Name.ToLower();
                IPInterfaceStatistics ipStatistics = ni.GetIPStatistics();
                long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                if (previousValues.ContainsKey(interfaceName + "_time"))
                {
                    long previousTime = previousValues[interfaceName + "_time"];
                    long previousSent = previousValues[interfaceName + "_sent"];
                    long previousReceived = previousValues[interfaceName + "_received"];
                    long sentBytesPerSecond = (ipStatistics.BytesSent - previousSent) / (now - previousTime) * 1000;
                    long receivedBytesPerSecond = (ipStatistics.BytesReceived - previousReceived) * 1000 /  (now - previousTime);

                    values.Add("network_name_" + interfaceIndex, interfaceName);
                    values.Add("network_sent_bytes_" + interfaceIndex, sentBytesPerSecond.ToString());
                    values.Add("network_received_bytes_" + interfaceIndex, receivedBytesPerSecond.ToString());

                    //Console.WriteLine("{0}: {1} - {2} {3}", interfaceName, sentBytesPerSecond, receivedBytesPerSecond, receivedBytesPerSecond * 8);

                    interfaceIndex++;
                }
                previousValues[interfaceName + "_time"] = now;
                previousValues[interfaceName + "_sent"] = ipStatistics.BytesSent;
                previousValues[interfaceName + "_received"] = ipStatistics.BytesReceived;
            }

            return values;
        }
    }
}
