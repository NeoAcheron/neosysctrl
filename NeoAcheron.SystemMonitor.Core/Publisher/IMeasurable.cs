using System;
using System.Collections.Generic;
using System.Text;

namespace NeoAcheron.SystemMonitor.Core
{
    public interface IMeasurable
    {
        string Name { get; }
        Measurement[] AllMeasurements { get; }
    }
}
