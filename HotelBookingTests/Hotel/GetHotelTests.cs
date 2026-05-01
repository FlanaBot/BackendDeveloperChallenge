using System.Net;
using System.Net.Http.Json;
using HotelBooking.DTOs;
using HotelBookingTests.Common;
using Xunit;

namespace HotelBooking.Tests.Hotel
{
    [Collection("Integration")]
    public class GetHotelTests : IClassFixture<SeededDatabaseFixture>
    {
        private readonly HttpClient _client;

        public GetHotelTests(SeededDatabaseFixture fixture)
        {
            _client = fixture.Factory.CreateClient();
        }

        [Fact]
        public async Task GetHotel_ByExistingName_ReturnsHotels()
        {
            var response = await _client.GetAsync("/hotels?name=Hotel ABC");
            response.EnsureSuccessStatusCode();
            var hotels = await response.Content.ReadFromJsonAsync<List<HotelDto>>();
            Assert.NotEmpty(hotels!);
            Assert.All(hotels!, h => Assert.Equal("Hotel ABC", h.Name));
        }

        [Fact]
        public async Task GetHotel_UnknownName_ReturnsEmptyList()
        {
            var response = await _client.GetAsync("/hotels?name=Nonexistent Hotel");
            response.EnsureSuccessStatusCode();
            var hotels = await response.Content.ReadFromJsonAsync<List<HotelDto>>();
            Assert.Empty(hotels!);
        }
    }
}
