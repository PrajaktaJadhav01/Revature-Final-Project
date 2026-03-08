using System;
using System.ComponentModel.DataAnnotations;

namespace CustomerManagement
{
    public class CustomerAudit
    {
        [Key]
        public int AuditId { get; set; }

        public int CustomerId { get; set; }

        public string? ChangedField { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }

        public DateTime ChangedDate { get; set; }
    }
}