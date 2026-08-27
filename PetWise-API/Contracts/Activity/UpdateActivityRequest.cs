using Postgrest.Attributes;

namespace PetWise_API.Contracts.Activity
{
    public class UpdateActivityRequest
    {
        public string? title { get; set; }

        public string? description { get; set; }

        public TimeOnly? time_scheduled { get; set; }

        public string? recurrence { get; set; }

        
        public bool? is_active { get; set; }
    }
}