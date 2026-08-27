using System;
using System.Collections.Generic;
using System.Text;

namespace PetWise_Domain.Entities
{
    public class HealthEvent
    {
        public int Id { get; set; }
        public int PetId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
