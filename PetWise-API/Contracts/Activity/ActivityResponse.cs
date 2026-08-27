using Postgrest.Attributes;

namespace PetWise_API.Contracts.Activity
{
    public class ActivityResponse
    {
        
        public int activity_id { get; set; }
        
        public int pet_id { get; set; }
        
        public string title { get; set; } = string.Empty;
        
        public string description { get; set; } = string.Empty;

        public TimeOnly? time_scheduled { get; set; }
       
        public string recurrence { get; set; } = string.Empty;

        public bool is_active { get; set; }
        public DateTime created_at { get; set; }
    }
}
