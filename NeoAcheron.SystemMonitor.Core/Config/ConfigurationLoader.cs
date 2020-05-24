using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace NeoAcheron.SystemMonitor.Core.Config
{
    public abstract class ConfigurationLoader<T> : IDisposable where T : new()
    {
        private readonly string filePath;
        private FileStream fileStream = null;
        private readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        protected ConfigurationLoader()
        {
            this.filePath = typeof(T).Name + ".json";
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
                string json = JsonSerializer.Serialize(config, SerializerOptions);
                File.WriteAllText(filePath, json);
            }
        }

        public void Load()
        {
            lock (filePath)
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    T config = JsonSerializer.Deserialize<T>(json, SerializerOptions);
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
