using System;
using System.ComponentModel.DataAnnotations;

namespace CustomerManagement
{
    public class CustomerInteraction
    {
        [Key]
        public int InteractionId { get; set; }

        public int CustomerId { get; set; }

        public string? InteractionType { get; set; }
        public string? Subject { get; set; }
        public string? Details { get; set; }

        public DateTime InteractionDate { get; set; }
    }
}