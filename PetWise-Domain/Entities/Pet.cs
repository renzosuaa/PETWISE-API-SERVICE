using System;
using System.Collections.Generic;
using System.Text;

namespace PetWise_Domain.Entities
{
    public class Pet
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Species { get; set; } = string.Empty;
        public string Breed { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public double Weight { get; set; }
        public string? Sex { get; set; }
        public DateTime? Birthday { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid UserId { get; set; }
        public bool IsDeleted { get; set; }
    }
}
