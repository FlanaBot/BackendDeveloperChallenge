using HotelBooking.Data;
using HotelBooking.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using Xunit;

namespace HotelBooking.Tests
{
    public class GetRoomsFixture : IAsyncLifetime
    {
        public WebApplicationFactory<Program> Factory { get; } = new();
        public int HotelId { get; private set; }
        public int BookedSingleRoomId { get; private set; }
        public int OtherSingleRoomId { get; private set; }

        // The booking carved out of the seeded availability
        public DateOnly BookingFrom { get; } = new DateOnly(2035, 5, 10);
        public DateOnly BookingTo { get; } = new DateOnly(2035, 5, 20);

        public async Task InitializeAsync()
        {
            var client = Factory.CreateClient();
            await client.PostAsync("/database/clear", null);
            await client.PostAsync("/database/seed", null);

            using var scope = Factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<HotelBookingContext>();

            var hotel = await context.Hotels.OrderBy(h => h.Id).FirstAsync();
            HotelId = hotel.Id;

            var singles = await context.Rooms
                .Include(r => r.RoomType)
                .Where(r => r.HotelId == HotelId && r.RoomType!.Name == "Single")
                .OrderBy(r => r.RoomId)
                .ToListAsync();

            BookedSingleRoomId = singles[0].RoomId;
            OtherSingleRoomId = singles[1].RoomId;

            // Establish a known booking on the first single room
            await client.PostAsJsonAsync(
                $"/hotels/{HotelId}/rooms/{BookedSingleRoomId}/bookings",
                new CreateBookingRequest { GuestCount = 1, FromDate = BookingFrom, ToDate = BookingTo });
        }

        public async Task DisposeAsync()
        {
            var client = Factory.CreateClient();
            await client.PostAsync("/database/clear", null);
            await Factory.DisposeAsync();
        }
    }
}
