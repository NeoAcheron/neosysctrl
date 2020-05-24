using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace NeoAcheron.SystemMonitor.Core.Controllers
{
    [JsonConverter(typeof(AdjusterConverter))]
    public class FixedAdjuster : Adjuster
    {
        private Setting _setting = null;
        public float FixedTarget { get; set; } = 100;
        public string SettingPath { get; set; }

        public override string[] WatchedMeasurementPaths => new string[] { };
        public override string[] ControlledSettingPaths => new string[] { SettingPath };

        [JsonIgnore]
        public Setting Setting
        {
            get
            {
                return _setting;
            }
            set
            {
                if (_setting != null)
                    _setting.OnChange -= Setting_OnChange;
                _setting = value;
                SettingPath = _setting?.Path;
                if (_setting != null)
                    _setting.OnChange += Setting_OnChange;
            }
        }


        private void Setting_OnChange(object sender, Setting e)
        {
            if (e != null)
            {
                e.UpdateValue(this, FixedTarget);
            }
        }

        public override bool Start(SystemTraverser systemTraverser)
        {
            Setting = systemTraverser.AllSettings.FirstOrDefault(s => s.Path.Equals(SettingPath));
            Setting_OnChange(this, Setting);

            return _setting != null;
        }

        public override bool Stop()
        {
            Setting = null;

            return true;
        }
    }
}
