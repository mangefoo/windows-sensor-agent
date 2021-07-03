using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace WindowsSensorAgent
{
    class SensorReport
    {
        public String reporter { get; set; }
        public String topic { get; set; }
        public Dictionary<String, String> sensors { get; set; }
    }
    public class SensorReporter
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
}
