namespace PetWise_API.Contracts.Pet
{
    public class CreatePetRequest
    {
        public string name { get; set; }
       
        public string species { get; set; }
        
        public Guid user_id { get; set; }
        
        public DateTime birthday { get; set; }
       
        public string sex { get; set; } 

        public string breed { get; set; }

        public string image_url { get; set; }

        public float weight { get; set; }
    }
}
