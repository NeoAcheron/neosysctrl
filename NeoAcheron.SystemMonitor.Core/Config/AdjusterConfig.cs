using NeoAcheron.SystemMonitor.Core.Controllers;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoAcheron.SystemMonitor.Core.Config
{
    public class AdjusterConfig : ConfigurationLoader<AdjusterConfig>
    {
        public List<Adjuster> Adjusters { get; set; } = new List<Adjuster>();
    }
}
