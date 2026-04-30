namespace HotelBooking.DTOs
{
    public class BookingDto
    {
        public int BookingId { get; set; }
        public int HotelId { get; set; }
        public int RoomId { get; set; }
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public byte GuestCount { get; set; }
    }
}
