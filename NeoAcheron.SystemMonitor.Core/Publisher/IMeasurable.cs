using System;
using System.Collections.Generic;
using System.Text;

namespace NeoAcheron.SystemMonitor.Core
{
    public interface IMeasurable
    {
        Measurement[] AllMeasurements { get; }
    }
}
