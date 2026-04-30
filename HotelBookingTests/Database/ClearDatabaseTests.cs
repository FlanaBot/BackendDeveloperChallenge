using HotelBooking.Data;
using HotelBooking.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HotelBooking.Tests
{
    [Collection("Integration")]
    public class ClearDatabaseTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ClearDatabaseTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task ClearDatabase_RemovesAllData()
        {
            var client = _factory.CreateClient();

            await client.PostAsync("/database/seed", null);

            using var scope = _factory.Services.CreateScope();
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

            var clearResponse = await client.PostAsync("/database/clear", null);
            clearResponse.EnsureSuccessStatusCode();

            Assert.Empty(context.Hotels);
            Assert.Empty(context.Rooms);
            Assert.Empty(context.RoomAvailabilities);
            Assert.Empty(context.Bookings);
        }
    }
}
