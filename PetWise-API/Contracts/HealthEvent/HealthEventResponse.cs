using Postgrest.Attributes;

namespace PetWise_API.Contracts.HealthEvent
{
    public class HealthEventResponse
    {
        public int event_id { get; set; }
      
        public int pet_id { get; set; }
       
        public string event_name { get; set; }
       
        public DateTime event_date { get; set; }
       
        public string type { get; set; } // e.g., "vaccination", "illness", "checkup"

        public bool is_completed { get; set; }
       
        public DateTime created_at { get; set; }
    }
}
