using System.Text.Json.Serialization;

namespace Competitions.Models
{
    public class Document
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("_rid")]
        public string ResourceId { get; set; }
        [JsonPropertyName("_self")]
        public string SelfLink { get; set; }
        [JsonPropertyName("_ts")]
        public int Timestamp { get; set; }
        [JsonPropertyName("_etag")]
        public string ETag { get; set; }
    }
}