namespace PetWise_API.Contracts.User
{
    public class UpdateUserRequest
    {
        public string? first_name { get; set; } 
        public string? last_name { get; set; }

        public string? nickname { get; set; }
    }
}
