namespace PetWise_API.Contracts.HealthEvent
{
    public class CreateHealthEvent
    {
        public int pet_id { get; set; }

        public string event_name { get; set; }

        public DateTime event_date { get; set; }

        public string type { get; set; } // e.g., "vaccination", "illness", "checkup"

        

    }
}
