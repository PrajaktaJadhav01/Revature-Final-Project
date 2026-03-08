using System.ComponentModel.DataAnnotations;

namespace CustomerManagement
{
    public class CustomerAddress
    {
        [Key]
        public int AddressId { get; set; }

        public int CustomerId { get; set; }

        public string? AddressType { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }

        public bool IsDeleted { get; set; }
    }
}