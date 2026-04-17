namespace PetWise_API.Contracts.Auth
{
    public class AuthResponse
    {
        public Guid user_id { get; set; }
        public string email { get; set; }
        public string access_token { get; set; }   // JWT from Supabase Auth
    }
}

