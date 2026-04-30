using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Models
{
    public class RoomType
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(32)]
        public required string Name { get; set; }

        [Required]
        public byte Capacity { get; set; }
    }
}