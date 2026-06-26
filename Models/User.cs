using Postgrest.Attributes;
using Postgrest.Models;

namespace PetWise_API.Models
{
    [Table("User")]
    public class User : BaseModel
    {
        [PrimaryKey("user_id", false)]
        public Guid user_id { get; set; }

        [Column("first_name")]
        public string? first_name { get; set; }

        [Column("last_name")]
        public string? last_name { get; set; }

        [Column("email")]
        public string? email { get; set; }

        [Column("image_url")]
        public string? image_url { get; set; }

        [Column("nickname")]
        public string? nickname { get; set; }

        [Column("created_at")]
        public DateTime created_at { get; set; }

        [Column("has_completed_setup")]
        public bool has_completed_setup { get; set; }
    }
}
