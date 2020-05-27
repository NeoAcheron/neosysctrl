using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Extensions.ManagedClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utf8Json;

namespace NeoAcheron.SystemMonitor.Core
{
    public class Subscriber : IMqttApplicationMessageReceivedHandler
    {
        private readonly IApplicationMessageReceiver mqttReceiver;
        private readonly Dictionary<string, Setting> registeredSettings;

        public Subscriber(IApplicationMessageReceiver mqttReceiver)
        {
            this.mqttReceiver = mqttReceiver;
            this.registeredSettings = new Dictionary<string, Setting>();
            
            mqttReceiver.ApplicationMessageReceivedHandler = this;
        }

        public Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
        {
            if (arg.ClientId == null) return null;
            return Task.Run(() =>
            {
                try
                {
                    if (registeredSettings.ContainsKey(arg.ApplicationMessage.Topic))
                    {
                        Setting setting = registeredSettings[arg.ApplicationMessage.Topic];
                        if (setting != null)
                        {
                            dynamic data = JsonSerializer.Deserialize<dynamic>(arg.ApplicationMessage.Payload);
                            setting.Value = data;
                            arg.ProcessingFailed = false;
                        }
                    }
                }
                catch (NullReferenceException ex)
                {

                }
            });
        }

        public void Register(ISettable settable)
        {
            if (settable != null)
            {
                foreach (var setting in settable.AllSettings)
                {
                    registeredSettings.Add(setting.Path, setting);
                }
            }
        }

        public void DeregisterAll()
        {
            registeredSettings.Clear();
            mqttReceiver.ApplicationMessageReceivedHandler = null;
        }


        public void Dispose()
        {
            DeregisterAll();
        }
    }
}
