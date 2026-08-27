using Postgrest.Attributes;
using Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PetWise_Infrastructure.Persistence.Models
{
    [Table("Pet")]
    public class PetModel : BaseModel
    {
        [PrimaryKey("pet_id", true)]
        public int pet_id { get; set; }

        [Column("name")]
        public string name { get; set; } = string.Empty;

        [Column("species")]
        public string species { get; set; } = string.Empty;

        [Column("breed")]
        public string? breed { get; set; }

        [Column("image_url")]
        public string? image_url { get; set; }

        [Column("weight")]
        public double weight { get; set; }

        [Column("sex")]
        public string? sex { get; set; }

        [Column("birthday")]
        public DateTime? birthday { get; set; }

        [Column("created_at")]
        public DateTime created_at { get; set; }

        [Column("user_id")]
        public Guid user_id { get; set; }

        [Column("is_deleted")]
        public bool is_deleted { get; set; }
    }
}
