using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using System;
using System.Collections.Generic;
using Utf8Json;

namespace NeoAcheron.SystemMonitor.Core
{
    public class Publisher : IDisposable
    {
        private readonly List<IMeasurable> measurables = new List<IMeasurable>();
        private readonly IManagedMqttClient mqttClient;

        private readonly string PublisherName;

        private readonly Dictionary<Measurement, string> topicCache = new Dictionary<Measurement, string>();

        public bool Halted { get; set; } = false;

        public Publisher(string publisherName, IManagedMqttClient mqttClient)
        {
            this.PublisherName = publisherName;
            this.mqttClient = mqttClient;
        }

        public void Register(IMeasurable measurable)
        {
            if (measurable != null)
            {
                measurables.Add(measurable);

                foreach (var measurement in measurable.AllMeasurements)
                {
                    topicCache.Add(measurement, $"{PublisherName}/{measurable?.Name}/{measurement.MeasurementName}");
                    measurement.OnChange += PublishUpdate;
                    PublishUpdate(measurable, measurement);
                }
            }
        }

        private async void PublishUpdate(object sender, Measurement measurement)
        {
            if (!Halted)
            {
                if (measurement?.MeasurementValue != null)
                {
                    var topic = topicCache[measurement];
                    var payload = JsonSerializer.ToJsonString(measurement.MeasurementValue);

                    await mqttClient.PublishAsync(topic, payload, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce, true);
                }
            }
        }

        public void Dispose()
        {
            foreach (var emitter in measurables)
            {
                foreach (var measurement in emitter.AllMeasurements)
                {
                    measurement.OnChange -= PublishUpdate;
                }
            }
            measurables.Clear();
        }
    }
}
