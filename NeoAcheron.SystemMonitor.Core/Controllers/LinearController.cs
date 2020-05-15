using System;
using System.Collections.Generic;
using System.Text;

namespace NeoAcheron.SystemMonitor.Core.Controllers
{
    public class LinearController
    {
        private readonly Measurement measurement;
        private readonly Setting target;

        public LinearController(Measurement measurement, Setting target){
            this.measurement = measurement;
            this.target = target;

            measurement.OnChange += Measurement_OnChange;
        }

        private void Measurement_OnChange(object sender, Measurement e)
        {
            float value = (float)e.MeasurementValue;

            float min = 45;
            float max = 70;
            float range = max - min;

            value = Math.Clamp(value, min, max);
            value -= min;
            value /= range;

            target.UpdateValue(this, (byte)(value * 100));
        }
    }
}
