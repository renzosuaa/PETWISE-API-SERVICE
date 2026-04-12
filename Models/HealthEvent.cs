
using Postgrest.Attributes;
using Postgrest.Models;

namespace PetWise_API.Models
{
    [Table("HealthEvent")]
    public class HealthEvent : BaseModel
    {
        [PrimaryKey("event_id", false)]
        public int event_id { get; set; }

        [Column("pet_id")]
        public int pet_id { get; set; }

        [Column("event_name")]
        public string event_name { get; set; }

        [Column("event_date")]
        public DateTime event_date { get; set; }

        [Column("type")]
        public string type { get; set; } // e.g., "vaccination", "illness", "checkup"

        [Column("is_completed")]
        public bool is_completed { get; set; }

        [Column("created_at")]
        public DateTime created_at { get; set; }
    }
}
