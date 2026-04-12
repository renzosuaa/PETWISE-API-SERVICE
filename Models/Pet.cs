using Postgrest.Attributes;
using Postgrest.Models;

namespace PetWise_API.Models
{
    [Postgrest.Attributes.Table("Pet")]
    public class Pet: BaseModel
    {
        [PrimaryKey("pet_id", false)]
        public int pet_id { get; set; }

        [Column("name")]
        public string name { get; set; }

        [Column("species")]
        public string species { get; set; }

        [Column("user_id")]
        public int user_id { get; set; }

        [Column("birthday")]
        public DateTime birthday { get; set; }

        [Column("sex")]
        public string sex { get; set; }

        [Column("created_at")]
        public DateTime created_at { get; set; }
    }
}
