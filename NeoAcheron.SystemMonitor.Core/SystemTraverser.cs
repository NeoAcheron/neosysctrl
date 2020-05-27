using NeoAcheron.SystemMonitor.Core;
using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Utf8Json.Internal;
using System.Threading;
using System.Security.AccessControl;
using NeoAcheron.SystemMonitor.Core.Controllers;
using NeoAcheron.SystemMonitor.Core.Config;

namespace NeoAcheron.SystemMonitor.Core
{
    public class SystemTraverser : IDisposable, IVisitor, IMeasurable, ISettable
    {
        private Dictionary<ISensor, Measurement> sensors = new Dictionary<ISensor, Measurement>();

        private Dictionary<Setting, IControl> controls = new Dictionary<Setting, IControl>();

        public Measurement[] AllMeasurements => sensors.Values.ToArray();
        public Setting[] AllSettings => controls.Keys.ToArray();

        public readonly Computer computer;
        private readonly Thread thread;
        private readonly SensorConfig config;

        public SystemTraverser(SensorConfig config = null)
        {
            this.config = config ?? new SensorConfig();

            computer = new Computer();
            computer.HardwareAdded += HardwareAdded;
            computer.HardwareRemoved += HardwareRemoved;

            computer.IsControllerEnabled = true;
            computer.IsCpuEnabled = true;
            computer.IsGpuEnabled = true;
            computer.IsMemoryEnabled = false;
            computer.IsMotherboardEnabled = true;
            computer.IsStorageEnabled = false;
            computer.IsNetworkEnabled = false;

            computer.Open();
            computer.Traverse(this);

            thread = new Thread(PollStatus);
            thread.Start();
        }

        private void PollStatus()
        {
            while (true)
            {
                try
                {
                    computer.Traverse(this);
                }
                catch (Exception ex)
                {
                    // Meh   
                }
                finally
                {
                    Thread.Sleep(500);
                }
            }
        }

        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }

        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();
            hardware.Traverse(this);
        }

        public void VisitSensor(ISensor sensor)
        {
            Measurement measurement;
            if (sensors.TryGetValue(sensor, out measurement))
            {
                measurement.Value = sensor.Value;
            }
        }

        public void VisitParameter(IParameter parameter) { }

        public void Dispose()
        {
            computer.Close();
        }

        private Measurement GetMeasurement(ISensor sensor)
        {
            Measurement measurement;
            bool success = sensors.TryGetValue(sensor, out measurement);

            if (!success)
            {
            }
            return measurement;
        }

        public void SettingUpdate(Setting setting)
        {
            if (setting.Value != null)
            {
                float value = (float)setting.Value;
                controls[setting].SetSoftware(value);
            }
        }

        private void HardwareRemoved(IHardware hardware)
        {
            hardware.SensorAdded -= SensorAdded;
            hardware.SensorRemoved -= SensorRemoved;

            foreach (var sensor in hardware.Sensors)
            {
                SensorRemoved(sensor);
            }

            foreach (var subHardware in hardware.SubHardware)
            {
                HardwareRemoved(subHardware);
            }
        }

        private void HardwareAdded(IHardware hardware)
        {
            hardware.SensorAdded += SensorAdded;
            hardware.SensorRemoved += SensorRemoved;

            foreach (var sensor in hardware.Sensors)
            {
                SensorAdded(sensor);
            }

            foreach (var subHardware in hardware.SubHardware)
            {
                HardwareAdded(subHardware);
            }
        }

        private void SensorRemoved(ISensor sensor)
        {
            if (sensors.ContainsKey(sensor))
                sensors.Remove(sensor);
        }

        private void SensorAdded(ISensor sensor)
        {
            sensor.ValuesTimeWindow = TimeSpan.Zero;

            var path = sensor.Identifier.ToString().Trim('/');
            path = path.Replace('#', '$').Replace('+', '_');

            Measurement measurement = new Measurement(path);
            sensors.Add(sensor, measurement);

            var control = sensor.Control;
            if (control != null)
            {
                path = control.Identifier.ToString().Trim('/');
                path = path.Replace('#', '$').Replace('+', '_');
                Setting setting = new Setting(path);

                controls.Add(setting, control);
                setting.OnChange += SettingUpdate;
            }
        }

        public void Shutdown()
        {
            foreach (var control in controls.Values)
            {
                control.SetDefault();
            }
        }
    }
}
