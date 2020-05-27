using System;
using System.Collections.Generic;
using System.Text;

namespace NeoAcheron.SystemMonitor.Core
{
    public class Setting
    {
        public readonly string Path;

        public dynamic Value
        {
            get
            {
                return _settingValue;
            }
            set
            {
                if (!Equals(value, _settingValue))
                {
                    _settingValue = value;
                    OnChange?.Invoke(this);
                }
            }
        }

        private dynamic _settingValue = null;

        public event Action<Setting> OnChange;

        public Setting(string path, dynamic defaults = null)
        {
            Path = path;
            _settingValue = defaults;
        }
    }
}
