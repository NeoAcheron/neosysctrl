using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Utf8Json;

namespace NeoAcheron.SystemMonitor.Core.Controllers
{
    public class FixedAdjuster : IAdjuster
    {
        private Setting _setting = null;
        public float FixedTarget { get; set; } = 100;
        public string SettingPath { get; set; }

        public string[] WatchedMeasurementPaths => new string[] { };
        public string[] ControlledSettingPaths => new string[] { SettingPath };

        [IgnoreDataMember]
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

        public string Type { get; set; }

        private void Setting_OnChange(Setting e)
        {
            if (e != null)
            {
                e.Value = FixedTarget;
            }
        }

        public bool Start(SystemTraverser systemTraverser)
        {
            Setting = systemTraverser.AllSettings.FirstOrDefault(s => s.Path.Equals(SettingPath));
            Setting_OnChange(Setting);

            return _setting != null;
        }

        public bool Stop()
        {
            Setting = null;

            return true;
        }
    }
}
