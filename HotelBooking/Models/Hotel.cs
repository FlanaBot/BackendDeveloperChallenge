using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelBooking.Models
{
    public class Hotel
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(128)]
        public required string Name { get; set; }

        public ICollection<Room>? Rooms { get; set; }
    }
}