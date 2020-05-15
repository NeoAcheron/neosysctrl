using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utf8Json;

namespace NeoAcheron.SystemMonitor.Core
{
    public class Subscriber
    {
        private readonly IManagedMqttClient mqttClient;
        private readonly Dictionary<string, Setting> registeredSettings = new Dictionary<string, Setting>();

        private readonly string SubscriberName;

        public Subscriber(string subscriberName, IManagedMqttClient mqttClient)
        {
            this.SubscriberName = subscriberName;
            this.mqttClient = mqttClient;

            mqttClient.UseApplicationMessageReceivedHandler(this.OnReceive);
        }

        private void OnReceive(MqttApplicationMessageReceivedEventArgs arg)
        {
            Setting setting;
            registeredSettings.TryGetValue(arg.ApplicationMessage.Topic, out setting);

            if (setting != null)
            {
                var format = arg.ApplicationMessage.PayloadFormatIndicator.GetValueOrDefault();

                dynamic data = JsonSerializer.Deserialize<dynamic>(arg.ApplicationMessage.Payload);
                setting.UpdateValue(this, data);
            }
            arg.ProcessingFailed = false;
        }

        public void Register(ISettable settable)
        {
            if (settable != null)
            {
                foreach (var setting in settable.AllSettings)
                {
                    setting.OnChange += settable.SettingUpdate;

                    string topic = $"{SubscriberName}/{settable.Name}/{setting.SettingName}";
                    registeredSettings.Add(topic, setting);

                    byte[] payload = JsonSerializer.Serialize<object>(setting.SettingValue);

                    var message = new MqttApplicationMessageBuilder()
                        .WithTopic(topic)
                        .WithPayload(payload)
                        .WithExactlyOnceQoS()
                        .WithRetainFlag()
                        .Build();

                    mqttClient.PublishAsync(message);

                    mqttClient.SubscribeAsync(topic, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);

                    Console.WriteLine($"Subscribed to {topic} for updates");
                }
            }
        }


        public void Dispose()
        {
        }
    }
}
