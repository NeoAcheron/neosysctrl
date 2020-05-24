using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NeoAcheron.SystemMonitor.Web.Models
{
    public class MeasurementDescriptor
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public bool Controllable { get; set; }

        public SensorType Type { get; set; }
        public string Parent { get; set; }
    }
}
