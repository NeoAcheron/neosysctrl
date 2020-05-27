using System.Linq;
using Utf8Json;

namespace NeoAcheron.SystemMonitor.Core.Controllers
{
    public class DefaultAdjuster : IAdjuster
    {
        public string[] WatchedMeasurementPaths => new string[0];
        public string[] ControlledSettingPaths => new string[] { SettingPath };

        public string Type { get; set; }

        public string SettingPath { get; set; }

        public bool Start(SystemTraverser systemTraverser)
        {
            var Setting = systemTraverser.AllSettings.FirstOrDefault(s => s.Path.Equals(SettingPath));
            Setting.Value = null;

            return Setting != null;
        }

        public bool Stop()
        {
            return true;
        }
    }
}
