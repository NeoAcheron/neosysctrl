using MQTTnet.Implementations;
using MQTTnet.Server;
using NeoAcheron.SystemMonitor.Core;
using NeoAcheron.SystemMonitor.Core.Config;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Threading.Tasks;

namespace NeoAcheron.SystemMonitor.Web.Utils
{
    public class ConnectionManager : IMqttServerClientSubscribedTopicHandler, IMqttServerClientUnsubscribedTopicHandler, IMqttServerClientConnectedHandler, IMqttServerClientDisconnectedHandler
    {
        ConcurrentDictionary<string, Publisher> publishers = new ConcurrentDictionary<string, Publisher>();

        private readonly IMqttServer mqttServer;
        private readonly IMeasurable[] measurables;

        internal ConnectionManager(IMqttServer mqttServer, IMeasurable[] measurables)
        {
            this.mqttServer = mqttServer;
            this.measurables = measurables;
        }

        public Task HandleClientSubscribedTopicAsync(MqttServerClientSubscribedTopicEventArgs eventArgs)
        {
            Publisher publisher;
            if (publishers.TryGetValue(eventArgs.ClientId, out publisher))
            {
                publisher.AddPublishFilter(eventArgs.ClientId, eventArgs.TopicFilter.Topic);
            }
            else
            {
                throw new Exception($"Could not subscribe {eventArgs.ClientId} to topic {eventArgs.TopicFilter}!");
            }
            return Task.CompletedTask;
        }

        public async Task HandleClientUnsubscribedTopicAsync(MqttServerClientUnsubscribedTopicEventArgs eventArgs)
        {
            Publisher publisher;
            if (publishers.TryGetValue(eventArgs.ClientId, out publisher))
            {
                publisher.RemovePublishFilter(eventArgs.ClientId, eventArgs.TopicFilter);
            }
            else
            {
                throw new Exception($"Could not unsubscribe {eventArgs.ClientId} from topic {eventArgs.TopicFilter}!");
            }
        }

        public async Task HandleClientDisconnectedAsync(MqttServerClientDisconnectedEventArgs eventArgs)
        {
            var clientStatus = await mqttServer.GetClientStatusAsync();
            var client = clientStatus.Where(c => c.ClientId == eventArgs.ClientId).FirstOrDefault();
            if (client == null)
            {
                Publisher publisher;
                if (publishers.Remove(eventArgs.ClientId, out publisher))
                {
                    publisher.DeregisterAll();
                }
            }
        }

        public async Task HandleClientConnectedAsync(MqttServerClientConnectedEventArgs eventArgs)
        {
            if (!publishers.ContainsKey(eventArgs.ClientId))
            {
                Publisher publisher = new Publisher(mqttServer);
                publishers.TryAdd(eventArgs.ClientId, publisher);

                foreach (var measurable in measurables)
                    publisher.Register(measurable);
            }
        }
    }
}
