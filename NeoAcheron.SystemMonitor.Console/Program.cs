using ColorMine.ColorSpaces;
using HidSharp;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using NeoAcheron.SystemMonitor.Core;
using NeoAcheron.SystemMonitor.Core.Controllers;
using NeoAcheron.SystemMonitor.NZXT;
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
        static List<HidDeviceBuilder> supported_hid_devices = new List<HidDeviceBuilder>()
        {
            new HidDeviceBuilder<NZXT.NzxtKrakenX3>(0x1e71, 0x2007),
            new HidDeviceBuilder<Corsair.PowerSupplies.SeriesRMi>(0x1b1c, 0x1c0d),
        };

        static Publisher publisher;
        static Subscriber subscriber;

        static NZXT.NzxtKrakenX3 nzxtKrakenX3;

        static int hue = 240;

        static void Main(string[] args)
        {
            var options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(1))
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithTcpServer("192.168.178.3", 1883)
                    .Build())
                .Build();

            IManagedMqttClient mqttClient = new MqttFactory().CreateManagedMqttClient();
            mqttClient.StartAsync(options).Wait();

            publisher = new Publisher("SystemMonitor", mqttClient);
            subscriber = new Subscriber("SystemControl", mqttClient);

            var list = DeviceList.Local;
            list.Changed += List_Changed;
            list.RaiseChanged();

            SystemTraverser systemTraverser = new SystemTraverser();
            publisher.Register(systemTraverser);

            Measurement cpuTemp = systemTraverser.AllMeasurements.Where(m => m.MeasurementName.Equals("amdcpu/0/temperature/6")).FirstOrDefault();

            Measurement radiatorFan1 = systemTraverser.AllMeasurements.Where(m => m.MeasurementName.Equals("lpc/it8688e/control/4")).FirstOrDefault();

            Measurement[] loadStats = systemTraverser.AllMeasurements.Where(m => m.MeasurementName.StartsWith("amdcpu/0/load")).ToArray();
            
            cpuTemp.OnChange += CpuTemp_OnChange;
            var controller = new LinearController(cpuTemp, nzxtKrakenX3.PumpTargetSetting);

           // publisher.Halted = true;
            uint j = 0;
            while (true)
            {
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
                nzxtKrakenX3.RgbControl.UpdateValue(nzxtKrakenX3, values);

                j = ++j % 8;
            }
        }

        private static void CpuTemp_OnChange(object sender, Measurement e)
        {
            float value = (float)e.MeasurementValue;

            float min = 45;
            float max = 70;
            float range = max - min;

            value = Math.Clamp(value, min, max);
            value -= min;
            value /= range;

            hue = (int)(value * 120) + 240;
        }

        private static void List_Changed(object sender, DeviceListChangedEventArgs e)
        {
            var list = sender as DeviceList;
            foreach (var device in list.GetHidDevices())
            {
                HidBase instance = supported_hid_devices
                    .Where(builder => builder.VendorId == device.VendorID && builder.ProductId == device.ProductID)
                    .Select(builder => builder.BuildInstance(device))
                    .FirstOrDefault();

                if (instance != null)
                {
                    publisher.Register(instance as IMeasurable);
                    subscriber.Register(instance as ISettable);
                }

                if (instance is NzxtKrakenX3)
                {
                    nzxtKrakenX3 = instance as NzxtKrakenX3;
                }
            }
        }
    }
}
