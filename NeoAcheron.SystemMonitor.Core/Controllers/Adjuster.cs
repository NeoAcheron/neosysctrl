using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Utf8Json.Internal;

namespace NeoAcheron.SystemMonitor.Core.Controllers
{
    [JsonConverter(typeof(AdjusterConverter))]
    public abstract class Adjuster
    {
        internal string Type { get; set; }

        public abstract string[] WatchedMeasurementPaths { get; }
        public abstract string[] ControlledSettingPaths { get; }

        public abstract bool Start(SystemTraverser systemTraverser);
        public abstract bool Stop();
    }

    public class AdjusterConverter : JsonConverter<Adjuster>
    {
        public override bool CanConvert(Type typeToConvert) => typeof(Adjuster).IsAssignableFrom(typeToConvert);
        public override Adjuster Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            JsonNamingPolicy namingPolicy = options.PropertyNamingPolicy;
            if (namingPolicy == null)
            {
                namingPolicy = JsonNamingPolicy.CamelCase;
            }

            Adjuster adjuster = null;
            using var jsonDocument = JsonDocument.ParseValue(ref reader);
            var jsonObject = jsonDocument.RootElement;
            var typeString = jsonObject.GetProperty(namingPolicy.ConvertName("Type")).GetString();

            Type type = Type.GetType("NeoAcheron.SystemMonitor.Core.Controllers." + typeString);

            adjuster = (Adjuster)Activator.CreateInstance(type);
            adjuster.Type = typeString;

            IEnumerable<PropertyInfo> properties = type.GetProperties()
                .Where(p => !Attribute.IsDefined(p, typeof(JsonIgnoreAttribute)))
                .Where(p => p.CanWrite);

            foreach (PropertyInfo propertyInfo in properties)
            {
                string name = namingPolicy.ConvertName(propertyInfo.Name);

                JsonElement element;
                if (!jsonObject.TryGetProperty(name, out element) && !jsonObject.TryGetProperty(propertyInfo.Name, out element))
                {
                    continue;
                }

                object value;
                switch (element.ValueKind)
                {
                    case JsonValueKind.Undefined:
                    case JsonValueKind.Null:
                        continue;
                    case JsonValueKind.String:
                        value = element.GetString();
                        break;
                    case JsonValueKind.False:
                        value = false;
                        break;
                    case JsonValueKind.True:
                        value = true;
                        break;
                    default:
                        value = JsonSerializer.Deserialize(element.ToString(), propertyInfo.PropertyType, options);
                        break;
                }
                propertyInfo.SetValue(adjuster, value);
            }

            return adjuster;
        }

        public override void Write(Utf8JsonWriter writer, Adjuster adjuster, JsonSerializerOptions options)
        {
            JsonNamingPolicy namingPolicy = options.PropertyNamingPolicy;
            if (namingPolicy == null)
            {
                namingPolicy = JsonNamingPolicy.CamelCase;
            }

            writer.WriteStartObject();
            IEnumerable<PropertyInfo> properties = adjuster.GetType().GetProperties().Where(p => !Attribute.IsDefined(p, typeof(JsonIgnoreAttribute)));
            adjuster.Type = adjuster.GetType().Name;

            foreach (PropertyInfo propertyInfo in properties)
            {
                var name = namingPolicy.ConvertName(propertyInfo.Name);
                var value = propertyInfo.GetValue(adjuster);

                writer.WritePropertyName(name);
                JsonSerializer.Serialize(writer, value, propertyInfo.PropertyType, options);
            }
            writer.WriteString(namingPolicy.ConvertName("Type"), adjuster.Type);
            writer.WriteEndObject();
        }
    }

}
