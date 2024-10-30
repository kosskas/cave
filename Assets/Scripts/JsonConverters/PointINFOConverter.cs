using System;
using UnityEngine;
using Newtonsoft.Json;

public class PointINFOConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        PointINFO pointINFO = (PointINFO)value;

        writer.WriteStartObject();
        writer.WritePropertyName("Label");
        writer.WriteValue(pointINFO.Label);
        writer.WritePropertyName("FullLabel");
        writer.WriteValue(pointINFO.FullLabel);
        writer.WritePropertyName("WallNumber");
        writer.WriteValue(pointINFO.WallInfo.number);
        writer.WritePropertyName("GridPoint");
        writer.WriteValue(pointINFO.GridPoint.name);
        writer.WriteEndObject();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        // Implement deserialization logic if needed
        throw new NotImplementedException("Deserialization for PointINFO is not implemented.");
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(PointINFO);
    }
}
