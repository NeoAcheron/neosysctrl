using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Utf8Json;
using Utf8Json.Internal;
using Utf8Json.Resolvers;

namespace NeoAcheron.SystemMonitor.Core.Controllers
{
    [JsonFormatter(typeof(AdjusterFormatter))]
    public interface IAdjuster
    {
        [DataMember]
        string Type { get; set; }
        string[] WatchedMeasurementPaths { get; }
        string[] ControlledSettingPaths { get; }

        bool Start(SystemTraverser systemTraverser);
        bool Stop();
    }

    public sealed class AdjusterTypeContainer
    {
        private static Dictionary<string, Type> supportedTypes = new Dictionary<string, Type>() {
            { nameof(DefaultAdjuster), typeof(DefaultAdjuster) },
            { nameof(FixedAdjuster), typeof(FixedAdjuster) },
            { nameof(LinearAdjuster), typeof(LinearAdjuster) },
        };

        public string Type { get; set; }

        public Type GetAdjusterType()
        {
            return supportedTypes[Type];
        }
    }

    public class AdjusterFormatter : IJsonFormatter<IAdjuster>
    {
        public IAdjuster Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            var savedReader = reader;
            AdjusterTypeContainer data = formatterResolver.GetFormatter<AdjusterTypeContainer>().Deserialize(ref reader, formatterResolver);
            Type type = data.GetAdjusterType();
            if (type != null)
            {
                IAdjuster adjuster = (IAdjuster)JsonSerializer.NonGeneric.Deserialize(type, ref savedReader);
                return adjuster;
            }
            else
            {
                return null;
            }
        }

        public void Serialize(ref JsonWriter writer, IAdjuster value, IJsonFormatterResolver formatterResolver)
        {
            JsonSerializer.NonGeneric.Serialize(value.GetType(), ref writer, value, formatterResolver);
        }

        /*
        public override Adjuster Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            JsonNamingPolicy namingPolicy = options.PropertyNamingPolicy;
            if (namingPolicy == null)
            {
                namingPolicy = JsonNamingPolicy.CamelCase;
            }

            Adjuster adjuster = null;
            var jsonDocument = JsonDocument.ParseValue(ref reader);
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
        }*/

        /*
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
        }*/
    }

}
