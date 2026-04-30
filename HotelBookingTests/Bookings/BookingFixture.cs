using HotelBooking.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HotelBooking.Tests
{
    public class BookingFixture : IAsyncLifetime
    {
        public WebApplicationFactory<Program> Factory { get; } = new();
        public int HotelId { get; private set; }
        public int Hotel2Id { get; private set; }
        public int SingleRoomId { get; private set; }
        public int DoubleRoomId { get; private set; }
        public int DeluxeRoomId { get; private set; }
        public int Hotel2SingleRoomId { get; private set; }
        public int Hotel2DoubleRoomId { get; private set; }

        public async Task InitializeAsync()
        {
            var client = Factory.CreateClient();
            await client.PostAsync("/database/clear", null);
            await client.PostAsync("/database/seed", null);

            using var scope = Factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<HotelBookingContext>();

            var hotels = await context.Hotels.OrderBy(h => h.Id).ToListAsync();
            HotelId = hotels[0].Id;
            Hotel2Id = hotels[1].Id;

            var hotel1Rooms = await context.Rooms
                .Include(r => r.RoomType)
                .Where(r => r.HotelId == HotelId)
                .ToListAsync();

            SingleRoomId = hotel1Rooms.First(r => r.RoomType!.Name == "Single").RoomId;
            DoubleRoomId = hotel1Rooms.First(r => r.RoomType!.Name == "Double").RoomId;
            DeluxeRoomId = hotel1Rooms.First(r => r.RoomType!.Name == "Deluxe").RoomId;

            var hotel2Rooms = await context.Rooms
                .Include(r => r.RoomType)
                .Where(r => r.HotelId == Hotel2Id)
                .ToListAsync();

            Hotel2SingleRoomId = hotel2Rooms.First(r => r.RoomType!.Name == "Single").RoomId;
            Hotel2DoubleRoomId = hotel2Rooms.First(r => r.RoomType!.Name == "Double").RoomId;
        }

        public async Task DisposeAsync()
        {
            var client = Factory.CreateClient();
            await client.PostAsync("/database/clear", null);
            await Factory.DisposeAsync();
        }
    }
}
