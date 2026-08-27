using System;
using Postgrest.Attributes;
using Postgrest.Models;

namespace PetWise.Infrastructure.Persistence.Models;

[Table("HealthEvent")]
public class HealthEventModel : BaseModel
{
    [PrimaryKey("event_id", true)]
    public int event_id { get; set; }

    [Column("pet_id")]
    public int pet_id { get; set; }

    [Column("type")]
    public string type { get; set; } = string.Empty;

    [Column("event_name")]
    public string event_name { get; set; } = string.Empty;

    [Column("event_date")]
    public DateTime event_date { get; set; }

    [Column("is_completed")]
    public bool is_completed { get; set; }

    [Column("created_at")]
    public DateTime created_at { get; set; }
}