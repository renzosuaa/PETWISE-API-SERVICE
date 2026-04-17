namespace PetWise_API.Contracts.User
{
    public class CreateUserRequest
    {
        public string first_name { get; set; }

        public string last_name { get; set; }
        
        public string email { get; set; }

        public string nickname { get; set; }

        public string password { get; set; }
    }
}
