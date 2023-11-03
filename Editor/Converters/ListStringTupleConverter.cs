using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Editor.Converters;

public class ListStringTupleConverter : JsonConverter<List<(string, string)>>
{
    public override List<(string, string)> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var list = new List<(string, string)>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return list;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException($"Unable to parse token {reader.TokenType} as a repository name string");

            var repo = reader.GetString();
            if (repo == null)
                throw new JsonException("Repository name can't be null");

            reader.Read();
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException($"Unable to parse token {reader.TokenType} as a license string");

            var license = reader.GetString();
            if (license == null)
                throw new JsonException("License can't be null");

            list.Add((repo, license));
        }

        return list;
    }

    public override void Write(Utf8JsonWriter writer, List<(string, string)> value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
