namespace PetWise_API.Contracts.User
{
    public class UserResponse
    {
        public string user_id { get; set; }
        
        public string first_name { get; set; }
        
        public string last_name { get; set; }
        
        public string email { get; set; }
        
        public DateTime created_at { get; set; }
    }
}
