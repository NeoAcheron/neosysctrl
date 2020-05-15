using System;
using System.Collections.Generic;
using System.Text;

namespace NeoAcheron.SystemMonitor.Core
{
    public class Setting
    {
        public readonly string SettingName;

        public dynamic SettingValue => _settingValue;
        private dynamic _settingValue;

        public event EventHandler<Setting> OnChange;

        public Setting(string settingName, dynamic defaults = null)
        {
            SettingName = settingName;
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
