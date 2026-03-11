using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;   // ADD THIS
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CustomerManagement
{
    public class Customer
    {
        // MongoDB ID (for MongoDB documents)
        [NotMapped]   // ADD THIS
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        // SQL Server Primary Key (existing)
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

        public virtual Segment? Segment { get; set; }
    }
}