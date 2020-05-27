using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using System;
using System.Collections.Concurrent;
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
        private readonly IApplicationMessagePublisher mqttPublisher;

        private readonly ConcurrentDictionary<Measurement, MqttApplicationMessage> registeredMeasurements = new ConcurrentDictionary<Measurement, MqttApplicationMessage>();
        private readonly CancellationTokenSource cancellationTokenSource;

        public Publisher(IApplicationMessagePublisher mqttPublisher)
        {
            this.mqttPublisher = mqttPublisher;
            this.cancellationTokenSource = new CancellationTokenSource();
        }

        private IEnumerable<Measurement> TopicFilterToMeasurements(string mqttTopicFilter, bool publishing)
        {
            IEnumerable<Measurement> includedMeasurements = registeredMeasurements.Where(m => (m.Value != null) == publishing).Select(m => m.Key);
            if (mqttTopicFilter.EndsWith("#"))
            {
                includedMeasurements = includedMeasurements.Where(m => m.Path.StartsWith(mqttTopicFilter.TrimEnd('#')));
            }
            else if (mqttTopicFilter.Contains("+"))
            {
                string pattern = mqttTopicFilter.Replace("+", @"[\s\d_]+");
                Regex rx = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

                includedMeasurements = includedMeasurements.Where(m => rx.IsMatch(m.Path));
            }
            else
            {
                includedMeasurements = includedMeasurements.Where(m => m.Path == mqttTopicFilter);
            }

            return includedMeasurements.ToArray();
        }

        public void AddPublishFilter(string clientId, string mqttTopicFilter)
        {
            var measurements = TopicFilterToMeasurements(mqttTopicFilter, false);
            foreach (var measurement in measurements)
            {
                MqttApplicationMessage message = new MqttApplicationMessageBuilder()
                    .WithRetainFlag()
                    .WithTopic(measurement.Path)
                    .Build();

                if (registeredMeasurements.TryUpdate(measurement, message, null))
                {
                    measurement.OnChange += PublishUpdate;
                    PublishUpdate(measurement);
                }
            }
        }

        public void RemovePublishFilter(string clientId, string mqttTopicFilter)
        {
            var measurements = TopicFilterToMeasurements(mqttTopicFilter, true);
            foreach (var measurement in measurements)
            {
                MqttApplicationMessage message;
                if (registeredMeasurements.TryGetValue(measurement, out message))
                {
                    measurement.OnChange -= PublishUpdate;
                    registeredMeasurements.TryUpdate(measurement, null, message);
                }
            }
        }

        public void Register(IMeasurable measurable)
        {
            if (measurable != null)
            {
                foreach (var measurement in measurable.AllMeasurements)
                {
                    registeredMeasurements.TryAdd(measurement, null);
                }
            }
        }

        public void PublishUpdate(Measurement measurement)
        {
            MqttApplicationMessage message;
            if (measurement?.Value != null && registeredMeasurements.TryGetValue(measurement, out message))
            {
                message.Payload = JsonSerializer.Serialize(measurement.Value);
                _ = mqttPublisher.PublishAsync(message, cancellationTokenSource.Token);
            }
        }

        public void DeregisterAll()
        {
            var publishingMeasurments = registeredMeasurements.Where(m => m.Value != null).Select(m => m.Key).ToArray();
            foreach (var measurement in publishingMeasurments)
            {
                measurement.OnChange -= PublishUpdate;
                registeredMeasurements.TryRemove(measurement, out _);
            }
        }

        public void Dispose()
        {
            DeregisterAll();
            cancellationTokenSource.Cancel();
        }
    }
}
