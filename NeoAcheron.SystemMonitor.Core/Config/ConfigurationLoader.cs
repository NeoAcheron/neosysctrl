using System;
using System.Collections.Generic;
using System.IO;
using Utf8Json;

namespace NeoAcheron.SystemMonitor.Core.Config
{
    public abstract class ConfigurationLoader<T> : IDisposable where T : new()
    {
        private readonly string filePath;
        private FileStream fileStream = null;      

        protected ConfigurationLoader()
        {
            this.filePath = AppContext.BaseDirectory + "/" + typeof(T).Name + ".json";
        }

        public void Save()
        {
            lock (filePath)
            {
                T config = new T();

                var properties = typeof(T).GetProperties();
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(this);
                    prop.SetValue(config, value);
                }
                byte[] json = JsonSerializer.Serialize(config);
                File.WriteAllBytes(filePath, json);
            }
        }

        public void Load()
        {
            lock (filePath)
            {
                if (File.Exists(filePath))
                {
                    byte[] json = File.ReadAllBytes(filePath);
                    T config = JsonSerializer.Deserialize<T>(json);
                    if (config != null)
                    {
                        var properties = typeof(T).GetProperties();
                        foreach (var prop in properties)
                        {
                            var value = prop.GetValue(config);
                            prop.SetValue(this, value);
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            if (fileStream != null)
            {
                fileStream.Close();
            }
        }
    }
}
