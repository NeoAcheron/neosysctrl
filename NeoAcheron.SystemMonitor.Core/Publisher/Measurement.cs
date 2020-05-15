using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NeoAcheron.SystemMonitor.Core
{

    public class Measurement : EventArgs
    {
        public readonly string MeasurementName;

        public virtual object MeasurementValue => _measurementValue;

        protected object _measurementValue;

        public event EventHandler<Measurement> OnChange;

        public Measurement(string measurementName)
        {
            MeasurementName = measurementName;
            _measurementValue = null;
        }

        public void UpdateValue(IMeasurable changeSource, object measurementValue)
        {
            if (!Equals(measurementValue, _measurementValue))
            {
                _measurementValue = measurementValue;
                if (OnChange != null)
                    OnChange.Invoke(changeSource, this);
            }
        }
    }
}
