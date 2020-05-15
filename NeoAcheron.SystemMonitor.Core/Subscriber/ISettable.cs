using System;
using System.Collections.Generic;
using System.Text;

namespace NeoAcheron.SystemMonitor.Core
{
    public interface ISettable
    {
        string Name { get; }
        Setting[] AllSettings { get; }

        void SettingUpdate(object source, Setting setting);
    }
}
