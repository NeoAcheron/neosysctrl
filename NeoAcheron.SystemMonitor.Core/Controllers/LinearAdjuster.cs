using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace NeoAcheron.SystemMonitor.Core.Controllers
{
    [JsonConverter(typeof(AdjusterConverter))]
    public class LinearAdjuster : Adjuster
    {
        private Measurement _measurement = null;
        private Setting _setting = null;

        public float LowerValue { get; set; } = 45;
        public float UpperValue { get; set; } = 70;
        public float LowerTarget { get; set; } = 0;
        public float UpperTarget { get; set; } = 100;

        public string MeasurementPath { get; set; }
        public string SettingPath { get; set; }

        public override string[] WatchedMeasurementPaths => new string[] { MeasurementPath };
        public override string[] ControlledSettingPaths => new string[] { SettingPath };

        public LinearAdjuster()
        {
        }

        public LinearAdjuster(Measurement measurement, Setting setting)
        {
            this.Measurement = measurement;
            this.Setting = setting;
        }

        private void Measurement_OnChange(object sender, Measurement e)
        {
            if (e?.Value != null && _setting != null)
            {
                float value = (float)e.Value;
                float target = linear(value, LowerValue, UpperValue, LowerTarget, UpperTarget);

                _setting.UpdateValue(this, target);
            }
        }

        public override bool Start(SystemTraverser systemTraverser)
        {
            if (String.IsNullOrEmpty(MeasurementPath) || string.IsNullOrEmpty(SettingPath))
                return false;

            Measurement = systemTraverser.AllMeasurements.FirstOrDefault(m => m.Path.Equals(MeasurementPath));
            Setting = systemTraverser.AllSettings.FirstOrDefault(s => s.Path.Equals(SettingPath));
            Measurement_OnChange(this, Measurement);

            return _measurement != null && _setting != null;
        }

        public override bool Stop()
        {
            Measurement = null;
            Setting = null;

            return true;
        }


        [JsonIgnore]
        public Measurement Measurement
        {
            get
            {
                return _measurement;
            }
            set
            {
                if (_measurement != null)
                    _measurement.OnChange -= Measurement_OnChange;
                _measurement = value;
                MeasurementPath = _measurement?.Path;
                if (_measurement != null)
                    _measurement.OnChange += Measurement_OnChange;
            }
        }

        [JsonIgnore]
        public Setting Setting
        {
            get
            {
                return _setting;
            }
            set
            {
                _setting = value;
                SettingPath = _setting?.Path;
            }
        }


        static public float linear(float x, float x0, float x1, float y0, float y1)
        {
            if (x < x0) return y0;
            if (x > x1) return y1;
            if ((x1 - x0) == 0)
            {
                return (y0 + y1) / 2;
            }
            return y0 + (x - x0) * (y1 - y0) / (x1 - x0);
        }

    }
}
