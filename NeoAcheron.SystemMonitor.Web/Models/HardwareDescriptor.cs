using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NeoAcheron.SystemMonitor.Web.Models
{
    public class HardwareDescriptor
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Type { get; set; }
        public IList<HardwareDescriptor> Children { get; } = new List<HardwareDescriptor>();
        public Dictionary<string, List<SensorDescriptor>> Sensors { get; } = new Dictionary<string, List<SensorDescriptor>>();
        public IEnumerable<string> SensorTypes => Sensors.Keys;

        public override string ToString()
        {
            return Path;
        }
    }

    public class SensorDescriptor
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Type { get; set; }
        public bool Primary { get; set; }
        public string ControlPath { get; set; }
        public bool Hidden { get; set; }
    }
}
