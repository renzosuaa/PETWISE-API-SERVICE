namespace PetWise_API.Contracts.Activity
{
    public class CreateActivityRequest
    {
        public int pet_id { get; set; }
        
        public string title { get; set; }
 
        public string description { get; set; }
       
        public DateTime scheduled_date { get; set; }

        public string recurrence { get; set; }
    }
}
