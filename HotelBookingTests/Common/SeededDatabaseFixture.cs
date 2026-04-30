using HotelBooking.Data;
using HotelBooking.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HotelBookingTests.Common
{
    public class SeededDatabaseFixture : IAsyncLifetime
    {
        public WebApplicationFactory<Program> Factory { get; }
        public int ExistingBookingId { get; private set; }

        public SeededDatabaseFixture()
        {
            Factory = new WebApplicationFactory<Program>();
        }

        public async Task InitializeAsync()
        {
            var client = Factory.CreateClient();
            await client.PostAsync("/database/clear", null);
            await client.PostAsync("/database/seed", null);

            using var scope = Factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<HotelBookingContext>();
            var hotel = await context.Hotels.FirstAsync();
            var room = await context.Rooms.FirstAsync(r => r.HotelId == hotel.Id);

            var booking = new Booking
            {
                HotelId = hotel.Id,
                Hotel = hotel,
                RoomId = room.RoomId,
                Room = room,
                FromDate = new DateOnly(2030, 6, 1),
                ToDate = new DateOnly(2030, 6, 5),
                GuestCount = 1
            };
            context.Bookings.Add(booking);
            await context.SaveChangesAsync();
            ExistingBookingId = booking.BookingId;
        }

        public async Task DisposeAsync()
        {
            var client = Factory.CreateClient();
            await client.PostAsync("/database/clear", null);
            await Factory.DisposeAsync();
        }
    }
}
