using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NeoAcheron.SystemMonitor.Core
{

    public class Measurement
    {
        public readonly string Path;

        protected object _measurementValue;

        public event Action<Measurement> OnChange;

        public Measurement(string path)
        {
            Path = path;
            _measurementValue = null;
        }

        public object Value
        {
            get
            {
                return _measurementValue;
            }
            set
            {
                if (!Equals(value, _measurementValue))
                {
                    _measurementValue = value;
                    OnChange?.Invoke(this);
                }
            }
        }
    }
}
