using Postgrest.Attributes;

namespace PetWise_API.Contracts.Activity
{
    public class ActivityResponse
    {
        
        public int activity_id { get; set; }
        
        public int pet_id { get; set; }
        
        public string title { get; set; }
        
        public string description { get; set; }
        
        public DateTime scheduled_date { get; set; }
       
        public string recurrence { get; set; }

        public bool is_completed { get; set; }

        public DateTime created_at { get; set; }
    }
}
