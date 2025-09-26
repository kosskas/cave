using System;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.JsonConverters
{
    public class Vector3Converter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            // handle Vector3 and Nullable<Vector3>
            var t = Nullable.GetUnderlyingType(objectType) ?? objectType;
            return t == typeof(Vector3);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var v = (Vector3)value;
            writer.WriteStartObject();
            writer.WritePropertyName("x"); writer.WriteValue(v.x);
            writer.WritePropertyName("y"); writer.WriteValue(v.y);
            writer.WritePropertyName("z"); writer.WriteValue(v.z);
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Accept either { "x":..., "y":..., "z":... } or null
            switch (reader.TokenType)
            {
                case JsonToken.Null:
                    // If target type is nullable -> return null
                    if (Nullable.GetUnderlyingType(objectType) != null) return null;

                    // Non-nullable Vector3 -> return Vector3.zero (or throw if you prefer strictness)
                    return Vector3.zero;

                case JsonToken.StartObject:
                    // Object form: { "x": ..., "y": ..., "z": ... }
                    var jo = JObject.Load(reader);

                    // Missing fields default to 0
                    var x = jo["x"]?.ToObject<float>() ?? 0f;
                    var y = jo["y"]?.ToObject<float>() ?? 0f;
                    var z = jo["z"]?.ToObject<float>() ?? 0f;

                    return new Vector3(x, y, z);

                default:
                    // Any other token is unsupported for Vector3
                    throw new JsonSerializationException(
                        $"Unsupported token for Vector3: {reader.TokenType}. Expected StartObject or Null.");
            }
        }

    }
}
