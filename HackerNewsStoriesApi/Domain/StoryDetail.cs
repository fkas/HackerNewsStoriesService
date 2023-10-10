using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace HackerNewsStoriesApi.Domain;

public record StoryDetail
{
    [property:JsonPropertyName("title")]
    public string? Title { get; set; }
   
    [property:JsonPropertyName("url")]
    public string? Url { get; set; }
    
    [property:JsonPropertyName("by")]
    public string? By { get; set; }
    
    [property:JsonPropertyName("time")]
    public ulong Time { get; set; }
    [property:JsonPropertyName("score")]
    public required uint Score { get; set; }
    
    [property:JsonPropertyName("descendants")]
    public uint Descendants { get; set; }    
}