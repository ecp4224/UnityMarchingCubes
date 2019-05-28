using System;
using Newtonsoft.Json;

public class ChunkPointConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var point = (World.ChunkPoint) value;
        
        writer.WriteStartObject();
        writer.WritePropertyName("X");
        serializer.Serialize(writer, point.X);
        writer.WritePropertyName("Z");
        serializer.Serialize(writer, point.Z);
        writer.WriteEndObject();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        int x = 0;
        int z = 0;

        while (reader.Read())
        {
            if (reader.TokenType != JsonToken.PropertyName)
                break;

            var propName = (string) reader.Value;
            if (!reader.Read())
                continue;

            if (propName == "X")
            {
                x = serializer.Deserialize<int>(reader);
            }

            if (propName == "Z")
            {
                z = serializer.Deserialize<int>(reader);
            }
        }

        return new World.ChunkPoint(x, z);
    }

    public override bool CanConvert(Type objectType)
    {
        return typeof(World.ChunkPoint) == objectType;
    }
}