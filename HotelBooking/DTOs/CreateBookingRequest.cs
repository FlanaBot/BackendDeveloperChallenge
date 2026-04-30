using HotelBooking.Models;

namespace HotelBooking.DTOs
{
    public class CreateBookingRequest
    {
        public int GuestCount { get; set; }
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }

    }
}
