using System;
using Newtonsoft.Json;

namespace IIIF.LegacyInclusions.Serialisation
{
    public class MetaDataValueSerialiser : WriteOnlyConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var metaDataValue = value as MetaDataValue;
            if (metaDataValue == null)
            {
                throw new ArgumentException(
                    "MetaDataValueSerialiser cannot serialise a " + value.GetType().Name, "value");
            }

            if (metaDataValue.LanguageValues.Length == 0)
            {
                throw new ArgumentException(
                    "MetaDataValueSerialiser cannot serialise an empty array " + value.GetType().Name, "value");
            }

            if (metaDataValue.LanguageValues.Length > 1)
            {
                writer.WriteStartArray();
            }

            foreach (var lv in metaDataValue.LanguageValues)
            {
                if (String.IsNullOrWhiteSpace(lv.Language))
                {
                    writer.WriteValue(lv.Value);
                }
                else
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("@value");
                    writer.WriteValue(lv.Value);
                    writer.WriteEndObject();
                }
            }

            if (metaDataValue.LanguageValues.Length > 1)
            {
                writer.WriteEndArray();
            }
        }
    }
}