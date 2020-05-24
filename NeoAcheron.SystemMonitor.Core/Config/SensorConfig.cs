using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace NeoAcheron.SystemMonitor.Core.Config
{
    public class SensorConfig : ConfigurationLoader<SensorConfig>
    {
        public ConcurrentDictionary<string, bool> PrimarySensors { get; set; } = new ConcurrentDictionary<string, bool>();
        public ConcurrentDictionary<string, object> SensorSettings { get; set; } = new ConcurrentDictionary<string, object>();

        public bool Contains(string name)
        {
            return SensorSettings.ContainsKey(name);
        }

        public T GetValue<T>(string name, T value)
        {
            return (T)SensorSettings.GetOrAdd(name, value);
        }

        public void Remove(string name)
        {
            SensorSettings.TryRemove(name, out _);
        }

        public void SetValue(string name, object value)
        {
            SensorSettings.AddOrUpdate(name, value, (key, oldval) => { return value; });
        }
    }
}
