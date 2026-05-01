using Postgrest.Attributes;
using Postgrest.Models;

namespace PetWise_API.Models
{
    [Table("Activity")]
    public class Activity : BaseModel
    {
        [PrimaryKey("activity_id", false)]
        public int activity_id { get; set; }

        [Column("pet_id")]
        public int pet_id { get; set; }

        [Column("title")]
        public string title { get; set; }

        [Column("description")]
        public string description { get; set; }

        [Column("time_scheduled")]
        public TimeOnly time_scheduled { get; set; }

        [Column("recurrence")]
        public string recurrence { get; set; }

        [Column("is_active")]
        public bool is_active { get; set; }

        [Column("created_at")]
        public DateTime created_at { get; set; }

    }
}
