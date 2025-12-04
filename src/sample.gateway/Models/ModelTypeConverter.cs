namespace sample.gateway.Models
{
    using Newtonsoft.Json;
    using System.Reflection;

    public class ModelTypeConverter<IdType> : JsonConverter<IdType?> where IdType : struct
    {
        delegate bool TryParseDelegate(string input, out IdType result);

        private static readonly TryParseDelegate _tryParse;

        static ModelTypeConverter()
        {
            var tryParseMethodInfo = typeof(IdType).GetMethod(
                "TryParse",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new[] { typeof(string), typeof(IdType).MakeByRefType() },
                null
            );
            if (tryParseMethodInfo == null || tryParseMethodInfo.ReturnType != typeof(bool))
            {
                throw new Exception($"Type {typeof(IdType)} does not have a public static bool TryParse(System.String, out {typeof(IdType).FullName}) method");
            }
            _tryParse = (TryParseDelegate)tryParseMethodInfo.CreateDelegate(typeof(TryParseDelegate));
        }

        public override void WriteJson(JsonWriter writer, IdType? value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override IdType? ReadJson(JsonReader reader, Type objectType, IdType? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
            {
                return default(IdType);
            }
            if (reader.Value is string str)
            {
                if (string.IsNullOrWhiteSpace(str))
                {
                    return default(IdType);
                }
                if (_tryParse(str, out IdType id))
                {
                    return id;
                }
            }
            return default(IdType);
        }
    }
}