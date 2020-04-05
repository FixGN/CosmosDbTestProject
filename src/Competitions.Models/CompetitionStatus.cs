using System.Text.Json.Serialization;

namespace Competitions.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CompetitionStatus
    {
        Scheduled,
        Finished,
        Canceled
    }
}