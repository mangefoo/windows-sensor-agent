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
        private String reporterId;
        private String topic;
        private HttpClient httpClient;

        public SensorReporter(String relayHost, String reporterId, String topic)
        {
            this.relayHost = relayHost;
            this.reporterId = reporterId;
            this.topic = topic;
            this.httpClient = new HttpClient();
        }

        public Boolean report(Dictionary<String, String> sensorValues)
        {
            SensorReport report = new SensorReport
            {
                reporter = reporterId,
                topic = topic,
                sensors = sensorValues
            };

            string jsonString = JsonSerializer.Serialize(report);

            StringContent content = new StringContent(jsonString);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            try
            {
                HttpResponseMessage res = httpClient.PostAsync(getRelayUrl("/publish"), content).Result;

                return res.StatusCode == HttpStatusCode.OK;
            } catch (Exception ex)
            {
                Console.WriteLine("Failed to send sensor report: " + ex);
            }

            return false;
        }

        private String getRelayUrl(String path)
        {
            return String.Format("http://{0}{1}", relayHost, path);
        }
    }
}
