using ColorMine.ColorSpaces;
using HidSharp;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Server;
using NeoAcheron.SystemMonitor.Core;
using NeoAcheron.SystemMonitor.Core.Controllers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NeoAcheron.SystemMonitor.Console
{
    class Program
    {
        static IMqttServer mqttServer;
        static Publisher publisher;
        static Subscriber subscriber;
        public static bool Running { get; set; }

        static int hue = 240;

        static void Main(string[] args)
        {
            var optionsBuilder = new MqttServerOptionsBuilder()
               .WithConnectionBacklog(2)
               .WithDefaultEndpointPort(1883)
               .WithClientId("SystemMonitor");


            mqttServer = new MqttFactory().CreateMqttServer();
            mqttServer.StartAsync(optionsBuilder.Build());

            Running = true;

            publisher = new Publisher(mqttServer);
            subscriber = new Subscriber(mqttServer);

            //var list = DeviceList.Local;
            //list.Changed += List_Changed;
            //list.RaiseChanged();

            SystemTraverser systemTraverser = new SystemTraverser();
            publisher.Register(systemTraverser);
            subscriber.Register(systemTraverser);

            Measurement cpuTemp = systemTraverser.AllMeasurements.Where(m => m.Path.Equals("amdcpu/0/temperature/6")).FirstOrDefault();
            Measurement gpuTemp = systemTraverser.AllMeasurements.Where(m => m.Path.Equals("gpu/0/temperature/0")).FirstOrDefault();
            Measurement liquidTemp = systemTraverser.AllMeasurements.Where(m => m.Path.Equals("nzxt/krakenx3/1A/temperature/0")).FirstOrDefault();

            Setting radiatorFans = systemTraverser.AllSettings.Where(s => s.Path.Equals("lpc/it8688e/control/0/control")).FirstOrDefault();
            Setting caseFans = systemTraverser.AllSettings.Where(s => s.Path.Equals("lpc/it8688e/control/2/control")).FirstOrDefault();
            Setting pumpControl = systemTraverser.AllSettings.Where(s => s.Path.Equals("nzxt/krakenx3/1A/control/0/control")).FirstOrDefault();

            Measurement[] loadStats = systemTraverser.AllMeasurements.Where(m => m.Path.StartsWith("amdcpu/0/load")).ToArray();

            var radiatorFanController = new LinearAdjuster(liquidTemp, radiatorFans)
            {
                LowerValue = 35,
                UpperValue = 50,
                LowerTarget = 0,
                UpperTarget = 100
            };
            var pumpController = new LinearAdjuster(cpuTemp, pumpControl)
            {
                LowerValue = 50,
                UpperValue = 70,
                LowerTarget = 30,
                UpperTarget = 100
            };
            var caseFanController = new LinearAdjuster(gpuTemp, caseFans)
            {
                LowerValue = 60,
                UpperValue = 80,
                LowerTarget = 0,
                UpperTarget = 50
            };

            // caseFans.UpdateValue(pumpController, 100.0f);
            while (Running)
            {
                /*
                    ColorSpace[] values = new ColorSpace[16];

                    int sleeptime = (int)(loadStats.Select(m => (float)m.MeasurementValue).Average() * 2);

                    Thread.Sleep(205 - sleeptime);

                    for (int i = 0; i < 8; i++)
                    {
                        //  hue = (hue + 1) % 360;                    

                        var lead_color = new Hsl { H = hue, S = 1, L = 0.7 };
                        var mid_color = new Hsl { H = hue, S = 1, L = 0.5 };
                        var trail_color = new Hsl { H = hue, S = 1, L = 0.25 };

                        values[(j - 1) % 8] = trail_color;
                        values[j] = mid_color;
                        values[(j + 1) % 8] = lead_color;
                    }
                   // nzxtKrakenX3.RgbControl.UpdateValue(nzxtKrakenX3, values);

                    j = ++j % 8;
                    */
                Thread.Yield();
                //caseFans.UpdateValue(pumpController, 100.0f);
                Thread.Sleep(1000);
                // caseFans.UpdateValue(pumpController, 1.0f);
                //Thread.Sleep(1000);
            }
        }
    }
}
