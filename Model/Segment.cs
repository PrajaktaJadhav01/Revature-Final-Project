using System.ComponentModel.DataAnnotations;

namespace CustomerManagement
{
    public class Segment
    {
        [Key]
        public int SegmentId { get; set; }

        public string? SegmentName { get; set; }
        public string? Description { get; set; }
    }
}