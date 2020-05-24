using MQTTnet;
using MQTTnet.Diagnostics;
using MQTTnet.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Utf8Json;

namespace NeoAcheron.SystemMonitor.Web.Utils
{
    public class RetainedMessagesManager : IMqttRetainedMessagesManager
    {
        private Dictionary<string, MqttApplicationMessage> retainedMessages = new Dictionary<string, MqttApplicationMessage>();

        public Task ClearMessagesAsync()
        {
            return Task.Run(() =>
            {
                lock (retainedMessages)
                {
                    retainedMessages.Clear();
                }
            });
        }

        public Task<IList<MqttApplicationMessage>> GetMessagesAsync()
        {
            return Task.Run(() =>
            {
                IList<MqttApplicationMessage> list;
                lock (retainedMessages)
                {
                    list = retainedMessages.Values.ToList();
                }
                return list;
            });
        }

        public Task<IList<MqttApplicationMessage>> GetSubscribedMessagesAsync(ICollection<MqttTopicFilter> topicFilters)
        {
            return Task.Run(() =>
            {
                IList<MqttApplicationMessage> list;
                lock (retainedMessages)
                {
                    IEnumerable<MqttApplicationMessage> filteredList = new List<MqttApplicationMessage>();
                    foreach (var topicFilter in topicFilters)
                    {
                        var mqttTopicFilter = topicFilter.Topic;
                        IEnumerable<MqttApplicationMessage> includedMeasurements;
                        if (mqttTopicFilter.EndsWith("#"))
                        {
                            includedMeasurements = retainedMessages.Where(m => m.Key.StartsWith(mqttTopicFilter.TrimEnd('#'))).Select(m => m.Value);
                        }
                        else if (mqttTopicFilter.Contains("+"))
                        {
                            string pattern = mqttTopicFilter.Replace("+", @"[\s\d_]+");
                            Regex rx = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

                            includedMeasurements = retainedMessages.Where(m => rx.IsMatch(m.Key)).Select(m => m.Value);
                        }
                        else
                        {
                            includedMeasurements = retainedMessages.Where(m => m.Key == mqttTopicFilter).Select(m => m.Value);
                        }
                        filteredList = filteredList.Concat(includedMeasurements);
                    }
                    list = filteredList.ToList();
                }
                return list;
            });
        }

        public Task HandleMessageAsync(string clientId, MqttApplicationMessage applicationMessage)
        {
            return Task.Run(() =>
            {
                lock (retainedMessages)
                {
                    if (retainedMessages.ContainsKey(applicationMessage.Topic))
                    {
                        retainedMessages[applicationMessage.Topic] = applicationMessage;
                    }
                    else
                    {
                        retainedMessages.Add(applicationMessage.Topic, applicationMessage);
                    }
                }
            });
        }

        public Task LoadMessagesAsync()
        {
            return Task.CompletedTask;
        }

        public Task Start(IMqttServerOptions options, IMqttNetLogger logger)
        {
            return Task.CompletedTask;
        }
    }
}
