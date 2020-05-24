using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Utf8Json;

namespace NeoAcheron.SystemMonitor.Core
{
    public class Publisher : IDisposable
    {
        private static readonly object _lock = new object();
        private readonly IApplicationMessagePublisher mqttPublisher;

        private readonly Dictionary<Measurement, bool> registeredMeasurements = new Dictionary<Measurement, bool>();
        private readonly CancellationTokenSource cancellationTokenSource;

        public Publisher(IApplicationMessagePublisher mqttPublisher)
        {
            this.mqttPublisher = mqttPublisher;
            this.cancellationTokenSource = new CancellationTokenSource();
        }

        private IEnumerable<Measurement> TopicFilterToMeasurements(string mqttTopicFilter, bool publishing)
        {
            IEnumerable<Measurement> includedMeasurements;
            if (mqttTopicFilter.EndsWith("#"))
            {
                includedMeasurements = registeredMeasurements.Where(m => m.Value == publishing).Where(m => m.Key.Path.StartsWith(mqttTopicFilter.TrimEnd('#'))).Select(m => m.Key);
            }
            else if (mqttTopicFilter.Contains("+"))
            {
                string pattern = mqttTopicFilter.Replace("+", @"[\s\d_]+");
                Regex rx = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

                includedMeasurements = registeredMeasurements.Where(m => m.Value == publishing).Where(m => rx.IsMatch(m.Key.Path)).Select(m => m.Key);
            }
            else
            {
                includedMeasurements = registeredMeasurements.Where(m => m.Value == publishing).Where(m => m.Key.Path == mqttTopicFilter).Select(m => m.Key);
            }

            return includedMeasurements.ToArray();
        }

        public void AddPublishFilter(string clientId, string mqttTopicFilter)
        {
            lock (_lock)
            {
                var measurements = TopicFilterToMeasurements(mqttTopicFilter, false);
                foreach (var measurement in measurements)
                {
                    registeredMeasurements[measurement] = true;
                    measurement.OnChange += PublishUpdate;
                    PublishUpdate(this, measurement);
                }
            }
        }

        public void RemovePublishFilter(string clientId, string mqttTopicFilter)
        {
            lock (_lock)
            {
                var measurements = TopicFilterToMeasurements(mqttTopicFilter, true);
                foreach (var measurement in measurements)
                {
                    measurement.OnChange -= PublishUpdate;
                    registeredMeasurements[measurement] = false;
                }
            }
        }

        public void Register(IMeasurable measurable)
        {
            if (measurable != null)
            {
                foreach (var measurement in measurable.AllMeasurements)
                {
                    if (!registeredMeasurements.ContainsKey(measurement))
                    {
                        registeredMeasurements.Add(measurement, false);
                    }
                }
            }
        }

        public void PublishUpdate(object sender, Measurement measurement)
        {
            if (measurement?.Value != null)
            {
                var topic = measurement.Path;
                byte[] payload = JsonSerializer.Serialize(measurement.Value);

                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(payload)
                    .WithRetainFlag()
                    .Build();

                mqttPublisher.PublishAsync(message, cancellationTokenSource.Token).Wait();
            }
        }

        public void DeregisterAll()
        {
            lock (_lock)
            {
                var publishingMeasurments = registeredMeasurements.Where(m => m.Value).Select(m => m.Key).ToArray();
                foreach (var measurement in publishingMeasurments)
                {
                    measurement.OnChange -= PublishUpdate;
                    registeredMeasurements[measurement] = false;
                }
            }
        }

        public void Dispose()
        {
            DeregisterAll();
        }
    }
}
