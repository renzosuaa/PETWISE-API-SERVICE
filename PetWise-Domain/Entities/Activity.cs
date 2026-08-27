using System;
using System.Collections.Generic;
using System.Text;

namespace PetWise_Domain.Entities
{
    public class Activity
    {
        public int Id { get; set; }
        public int PetId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TimeOnly? TimeScheduled { get; set; }
        public string? Recurrence { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
