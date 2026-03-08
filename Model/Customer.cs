using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CustomerManagement
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        public string? CustomerName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Website { get; set; }
        public string? Industry { get; set; }
        public string? CompanySize { get; set; }
        public string? Classification { get; set; }
        public string? Type { get; set; }

        public int? SegmentId { get; set; }
        public int? ParentCustomerId { get; set; }

        public decimal AccountValue { get; set; }
        public int HealthScore { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        public bool IsDeleted { get; set; }

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}