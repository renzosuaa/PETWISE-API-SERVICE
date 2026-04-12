using Postgrest.Attributes;
using Postgrest.Models;

namespace PetWise_API.Models
{
    [Table("User")]
    public class User : BaseModel
    {
        [PrimaryKey("user_id", false)]
        public int user_id { get; set; }

        [Column("first_name")]
        public string first_name { get; set; }

        [Column("last_name")]
        public string last_name { get; set; }

        [Column("email")]
        public string email { get; set; }

        [Column("password")]
        public string password { get; set; }

        [Column("supabase_user_id")]
        public string supabase_user_id { get; set; }

        [Column("created_at")]
        public DateTime created_at { get; set; }  

    }
}
