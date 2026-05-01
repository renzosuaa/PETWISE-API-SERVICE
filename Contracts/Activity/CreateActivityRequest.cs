namespace PetWise_API.Contracts.Activity
{
    public class CreateActivityRequest
    {
        public int pet_id { get; set; }
        
        public string title { get; set; }
 
        public string description { get; set; }
       
        public TimeOnly time_scheduled { get; set; }

        public string recurrence { get; set; }
    }
}
