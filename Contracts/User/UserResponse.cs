namespace PetWise_API.Contracts.User
{
    public class UserResponse
    {
        public Guid user_id { get; set; }
        
        public string first_name { get; set; }
        
        public string last_name { get; set; }
        
        public string email { get; set; }
        public string nickname { get; set; }


        public DateTime created_at { get; set; }
    }
}
