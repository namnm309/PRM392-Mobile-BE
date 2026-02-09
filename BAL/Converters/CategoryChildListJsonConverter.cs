using System.Text.Json;
using System.Text.Json.Serialization;
using BAL.DTOs.Category;

namespace BAL.Converters
{
    /// <summary>
    /// JsonConverter for List of CreateCategoryChildDto.
    /// Supports backward compatibility: ["Child1", "Child2"] or [{"name":"Child1","imageUrl":null,"displayOrder":0,"isHot":false}]
    /// </summary>
    public class CategoryChildListJsonConverter : JsonConverter<List<CreateCategoryChildDto>>
    {
        public override List<CreateCategoryChildDto>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException("Expected start of array");

            var list = new List<CreateCategoryChildDto>();
            reader.Read();

            while (reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    var name = reader.GetString();
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        list.Add(new CreateCategoryChildDto
                        {
                            Name = name.Trim(),
                            DisplayOrder = list.Count
                        });
                    }
                    reader.Read();
                }
                else if (reader.TokenType == JsonTokenType.StartObject)
                {
                    var child = JsonSerializer.Deserialize<CreateCategoryChildDto>(ref reader, options);
                    if (child != null && !string.IsNullOrWhiteSpace(child.Name))
                        list.Add(child);
                }
                else
                {
                    reader.Read();
                }
            }

            return list;
        }

        public override void Write(Utf8JsonWriter writer, List<CreateCategoryChildDto> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}
