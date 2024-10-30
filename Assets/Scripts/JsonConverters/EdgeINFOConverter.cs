using System;
using UnityEngine;
using Newtonsoft.Json;

public class EdgeINFOConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        EdgeINFO edgeINFO = (EdgeINFO)value;

        writer.WriteStartObject();
        writer.WritePropertyName("EdgeName");
        writer.WriteValue(edgeINFO.EdgeObj.name);
        writer.WriteEndObject();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        // Implement deserialization logic if needed
        throw new NotImplementedException("Deserialization for EdgeINFO is not implemented.");
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(EdgeINFO);
    }
}
