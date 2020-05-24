using System;
using System.Collections.Generic;
using System.Text;

namespace NeoAcheron.SystemMonitor.Core
{
    public class Setting
    {
        public readonly string Path;

        public dynamic SettingValue => _settingValue;

        private dynamic _settingValue = null;

        public event EventHandler<Setting> OnChange;

        public Setting(string path, dynamic defaults = null)
        {
            Path = path;
            _settingValue = defaults;
        }

        public void UpdateValue(object changeSource, dynamic settingValue)
        {
            if (!Equals(settingValue, _settingValue))
            {
                _settingValue = settingValue;
                OnChange?.Invoke(changeSource, this);
            }
        }
    }
}
