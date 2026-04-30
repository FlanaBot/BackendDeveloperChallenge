using HotelBooking.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HotelBooking.Tests
{
    [Collection("Integration")]
    public class SeedDatabaseTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public SeedDatabaseTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task SeedDatabase_CreatesHotelAndRooms()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsync("/database/seed", null);
            response.EnsureSuccessStatusCode();

            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<HotelBookingContext>();

            Assert.Equal(3, context.Hotels.Count());
            Assert.Equal(3 * 6, context.Rooms.Count());
            Assert.Equal(3 * 6, context.RoomAvailabilities.Count());
        }
    }
}
