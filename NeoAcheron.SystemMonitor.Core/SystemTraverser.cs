using NeoAcheron.SystemMonitor.Core;
using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Timers;

namespace NeoAcheron.SystemMonitor.Core
{
    public class SystemTraverser : IDisposable, IVisitor, IMeasurable, ISettable
    {
        public string Name => "System";
        private Dictionary<Identifier, Measurement> sensors = new Dictionary<Identifier, Measurement>();
        private Dictionary<Identifier, Setting> controls = new Dictionary<Identifier, Setting>();

        public Measurement[] AllMeasurements => sensors.Values.ToArray();
        public Setting[] AllSettings => controls.Values.ToArray();


        private readonly Computer computer = new Computer();

        public SystemTraverser()
        {
            computer.Open();
            computer.IsControllerEnabled = true;
            computer.IsCpuEnabled = true;
            computer.IsGpuEnabled = true;
            computer.IsMemoryEnabled = true;
            computer.IsMotherboardEnabled = true;
            computer.IsStorageEnabled = true;
            computer.IsNetworkEnabled = false;

            computer.Accept(this);

            System.Timers.Timer timer = new System.Timers.Timer(1000);
            timer.AutoReset = true;
            timer.Elapsed += PollStatus;
            timer.Enabled = true;
        }

        private void PollStatus(object sender, ElapsedEventArgs e)
        {
            computer.Accept(this);
        }

        public void VisitComputer(IComputer computer)
        {
            foreach (var hardware in computer.Hardware)
            {
                VisitHardware(hardware);
            }
        }

        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();

            Measurement measurement = GetMeasurement(hardware.Identifier);
            measurement.UpdateValue(this, hardware.Name);

            if (hardware.HardwareType == HardwareType.Motherboard)
            {
                hardware.GetType();
            }

            foreach (var sensor in hardware.Sensors)
            {
                VisitSensor(sensor);
            }
            foreach (IHardware subHardware in hardware.SubHardware)
            {
                VisitHardware(subHardware);
            }
        }

        public void VisitSensor(ISensor sensor)
        {

            Measurement measurement = GetMeasurement(sensor.Identifier);
            measurement.UpdateValue(this, sensor.Value);

            var control = sensor.Control;
            if(control != null){
                Setting setting = GetSetting(sensor.Identifier);

            }

        }

        public void VisitParameter(IParameter parameter) { }

        public void Dispose()
        {
            computer.Close();
        }

        private Measurement GetMeasurement(Identifier identifier)
        {
            Measurement measurement;
            if (sensors.ContainsKey(identifier))
            {
                measurement = sensors[identifier];
            }
            else
            {
                var name = identifier.ToString().Trim('/');
                measurement = new Measurement(name);
                sensors.Add(identifier, measurement);
            }
            return measurement;
        }

        private Setting GetSetting(Identifier identifier)
        {
            Setting setting;
            if (controls.ContainsKey(identifier))
            {
                setting = controls[identifier];
            }
            else
            {
                var name = identifier.ToString().Trim('/');
                setting = new Setting(name);
                controls.Add(identifier, setting);
            }
            return setting;
        }

        public void SettingUpdate(object source, Setting setting)
        {
            Identifier id = new Identifier("/" + setting.SettingName);

        }
    }
}
