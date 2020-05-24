using MQTTnet.Server;
using NeoAcheron.SystemMonitor.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NeoAcheron.SystemMonitor.Web.Utils
{
    public static class ExtensionMethods
    {
        public static void UseConnectionManager(this IMqttServer mqttServer, params IMeasurable[] measurables)
        {
            var connectionManager = new ConnectionManager(mqttServer, measurables);
            mqttServer.ClientConnectedHandler = connectionManager;
            mqttServer.ClientDisconnectedHandler = connectionManager;
            mqttServer.ClientSubscribedTopicHandler = connectionManager;
            mqttServer.ClientUnsubscribedTopicHandler = connectionManager;
        }

    }
}
