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
        public ConcurrentDictionary<string, string> SensorNames { get; set; } = new ConcurrentDictionary<string, string>();
        public ConcurrentDictionary<string, bool> SensorHidden { get; set; } = new ConcurrentDictionary<string, bool>();
    }

    public static class SensorConfigHelper
    {
        public static void AddOrUpdateName(this SensorConfig config, string path, string name)
        {
            config.SensorNames.AddOrUpdate(path, name, (key, oldval) => { return name; });
        }

        public static string GetName(this SensorConfig config, string path, string name)
        {
            return config.SensorNames.GetOrAdd(path, name);
        }

        public static void AddOrUpdateHidden(this SensorConfig config, string path, bool hidden)
        {
            config.SensorHidden.AddOrUpdate(path, hidden, (key, oldval) => { return hidden; });
        }

        public static bool IsHidden(this SensorConfig config, string path, bool hidden)
        {
            return config.SensorHidden.GetOrAdd(path, hidden);
        }

        public static void AddOrUpdatePrimary(this SensorConfig config, string path, bool primary)
        {
            config.PrimarySensors.AddOrUpdate(path, primary, (key, oldval) => { return primary; });
        }

        public static bool IsPrimary(this SensorConfig config, string path, bool primary)
        {
            return config.PrimarySensors.GetOrAdd(path, primary);
        }
    }
}
