namespace PetWise_API.Contracts.Auth
{
    public class ChangePasswordRequest
    {
        public string email { get; set; }          
        public string current_password { get; set; }
        public string new_password { get; set; }
    }
}
