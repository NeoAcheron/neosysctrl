using System;
using System.Collections.Generic;
using System.Text;

namespace NeoAcheron.SystemMonitor.Core
{
    public interface ISettable
    {
        Setting[] AllSettings { get; }

        void SettingUpdate(object source, Setting setting);
    }
}
