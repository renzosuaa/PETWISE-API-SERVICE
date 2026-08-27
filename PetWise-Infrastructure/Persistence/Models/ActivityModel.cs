using Postgrest.Attributes;
using Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PetWise_Infrastructure.Persistence.Models
{
    [Table("Activity")]
    public class ActivityModel : BaseModel
    {
        [PrimaryKey("activity_id", true)]
        public int activity_id { get; set; }

        [Column("pet_id")]
        public int pet_id { get; set; }

        [Column("title")]
        public string title { get; set; } = string.Empty;

        [Column("description")]
        public string? description { get; set; }

        [Column("time_scheduled")]
        public TimeOnly? time_scheduled { get; set; }

        [Column("recurrence")]
        public string? recurrence { get; set; }

        [Column("is_active")]
        public bool is_active { get; set; }

        [Column("created_at")]
        public DateTime created_at { get; set; }
    }
}
