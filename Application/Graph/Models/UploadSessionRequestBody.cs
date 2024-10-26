using System.Text.Json;
using System.Text.Json.Serialization;

namespace Application.Graph.Models;

public class UploadSessionRequestBody
{
    [JsonPropertyName("item"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ItemContents? Item { get; init; }
    
    public class ItemContents
    {
        [JsonPropertyName("@microsoft.graph.conflictBehavior"), JsonConverter(typeof(ConflictBehaviourConverter)), JsonInclude]
        public ConflictBehavior ConflictBehavior { get; init; } = ConflictBehavior.Rename;

        [JsonPropertyName("@odata.type"), JsonInclude]
        public string ODataType = "microsoft.graph.driveItemUploadableProperties";
        //
        // [JsonPropertyName("name"), JsonInclude]
        // public string? Name { get; init; }
        
    }

    public UploadSessionRequestBody()
    {
        
    }

    public UploadSessionRequestBody(ConflictBehavior conflictBehavior)
    {
        Item = new ItemContents
        {
            ConflictBehavior = conflictBehavior
        };
    }
}



























public enum ConflictBehavior
{
    Rename,
    Fail,
    Replace
}

public class ConflictBehaviourConverter : JsonConverter<ConflictBehavior>
{
    public override ConflictBehavior Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, ConflictBehavior value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString().ToLower());
        
        
    }
}