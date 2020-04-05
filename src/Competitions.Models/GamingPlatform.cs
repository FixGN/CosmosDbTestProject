using System.Text.Json.Serialization;

namespace Competitions.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum GamingPlatform
    {
        PC,
        PS3,
        PS4,
        XBoxOne
    }
}