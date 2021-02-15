using System;
using Newtonsoft.Json;

namespace IIIF.Presentation.V2.Serialisation
{
    public abstract class WriteOnlyConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => true;
        public override bool CanRead => false;

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}