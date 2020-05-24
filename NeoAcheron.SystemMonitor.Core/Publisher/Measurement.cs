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
        public virtual object Value => _measurementValue;
        public string Name { get; set; }

        protected object _measurementValue;

        public event EventHandler<Measurement> OnChange;

        public Measurement(string path)
        {
            Path = path;
            _measurementValue = null;
        }

        public void UpdateValue(IMeasurable changeSource, object measurementValue)
        {
            if (!Equals(measurementValue, _measurementValue))
            {
                _measurementValue = measurementValue;

                if (OnChange != null)
                {
                    OnChange.Invoke(changeSource, this);
#if DEBUG
                    Console.WriteLine($"Counting {OnChange.GetInvocationList().Length} delegations on {Path}");
#endif
                }
            }
        }

        internal void RemoveChangeHandler(object source)
        {
            if (OnChange != null)
            {
                foreach (var d in OnChange.GetInvocationList())
                {
                    if (d.Target == source)
                    {
                        OnChange -= (d as EventHandler<Measurement>);
                    }
                }
            }
        }
    }
}
