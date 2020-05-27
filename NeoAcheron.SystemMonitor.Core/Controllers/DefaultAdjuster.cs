using Utf8Json;

namespace NeoAcheron.SystemMonitor.Core.Controllers
{
    public class DefaultAdjuster : IAdjuster
    {
        public string[] WatchedMeasurementPaths => new string[0];
        public string[] ControlledSettingPaths => new string[] { settingPath };

        public string Type { get; set; }

        public string settingPath;

        public bool Start(SystemTraverser systemTraverser)
        {
            return true;
        }

        public bool Stop()
        {
            return true;
        }
    }
}
