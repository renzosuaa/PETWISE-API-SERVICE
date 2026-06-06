using System.Text.Json.Serialization;

namespace PetWise_API.Contracts.Analytics
{
    public class PetActivityHealthAnalyticsResponse
    {
        [JsonPropertyName("totalScheduledActivities")]
        public int TotalScheduledActivities { get; set; }

        [JsonPropertyName("totalActiveRoutines")]
        public int TotalActiveRoutines { get; set; }

        [JsonPropertyName("totalHealthEvents")]
        public int TotalHealthEvents { get; set; }

        [JsonPropertyName("medicalComplianceRate")]
        public double MedicalComplianceRate { get; set; }

        [JsonPropertyName("activityRecurrenceDistribution")]
        public Dictionary<string, int> ActivityRecurrenceDistribution { get; set; } = new();

        [JsonPropertyName("healthEventTypeDistribution")]
        public Dictionary<string, int> HealthEventTypeDistribution { get; set; } = new();

        [JsonPropertyName("activityTimeline")]
        public List<HourlyActivityMetric> ActivityTimeline { get; set; } = new();
    }

    public class HourlyActivityMetric
    {
        [JsonPropertyName("timeSlot")]
        public string TimeSlot { get; set; } = string.Empty;

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }
}
