using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelBooking.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [ForeignKey("Hotel")]
        public int HotelId { get; set; }

        public required Hotel Hotel { get; set; }

        [ForeignKey("Room")]
        public int RoomId { get; set; }

        public required Room Room { get; set; }

        [Required]
        public DateOnly FromDate { get; set; }

        [Required]
        public DateOnly ToDate { get; set; }

        [Required]
        public byte GuestCount { get; set; }
    }
}