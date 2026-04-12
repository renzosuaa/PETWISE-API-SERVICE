namespace PetWise_API.Contracts.Auth
{
    public class SignUpRequest
    {
        public string first_name { get; set; }   // maps to User.first_name
        public string last_name { get; set; }    // maps to User.last_name
        public string email { get; set; }       
        public string password { get; set; }    
    }
}
