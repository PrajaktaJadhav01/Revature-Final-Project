using System.ComponentModel.DataAnnotations;

namespace CustomerManagement
{
    public class ContactPerson
    {
        [Key]
        public int ContactPersonId { get; set; }

        public int CustomerId { get; set; }

        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Title { get; set; }

        public bool IsPrimary { get; set; }
        public bool IsDeleted { get; set; }
    }
}