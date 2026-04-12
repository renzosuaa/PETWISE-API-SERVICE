namespace PetWise_API.Contracts.Pet
{
    public class CreatePetRequest
    {
        public string name { get; set; }
       
        public string species { get; set; }
        
        public int user_id { get; set; }
        
        public DateTime birthday { get; set; }
       
        public string sex { get; set; }
    }
}
