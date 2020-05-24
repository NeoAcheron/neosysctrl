using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace NeoAcheron.SystemMonitor.Core.Controllers
{
    [JsonConverter(typeof(AdjusterConverter))]
    public class DefaultAdjuster : Adjuster
    {
        public override string[] WatchedMeasurementPaths => new string[0];
        public override string[] ControlledSettingPaths => new string[] { settingPath };

        public string settingPath;

        public override bool Start(SystemTraverser systemTraverser)
        {
            return true;
        }

        public override bool Stop()
        {
            return true;
        }
    }
}
