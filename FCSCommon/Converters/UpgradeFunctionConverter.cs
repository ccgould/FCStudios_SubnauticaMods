using System;
using FCSCommon.Objects;
using Oculus.Newtonsoft.Json;

namespace FCSCommon.Converters
{
    internal class UpgradeFunctionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(UpgradeFunction));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize(reader, typeof(UpgradeFunction));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value, typeof(UpgradeFunction));
        }
    }
}
