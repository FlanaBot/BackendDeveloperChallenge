using System.Net;
using System.Net.Http.Json;
using HotelBooking.DTOs;
using HotelBookingTests.Common;
using Xunit;

namespace HotelBookingTests.Bookings
{
    [Collection("Integration")]
    public class GetBookingTests : IClassFixture<SeededDatabaseFixture>
    {
        private readonly HttpClient _client;
        private readonly int _existingBookingId;

        public GetBookingTests(SeededDatabaseFixture fixture)
        {
            _client = fixture.Factory.CreateClient();
            _existingBookingId = fixture.ExistingBookingId;
        }

        [Fact]
        public async Task GetBooking_UnknownId_Returns404()
        {
            var response = await _client.GetAsync("/bookings/99999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetBooking_ExistingId_ReturnsBooking()
        {
            var response = await _client.GetAsync($"/bookings/{_existingBookingId}");
            response.EnsureSuccessStatusCode();
            var booking = await response.Content.ReadFromJsonAsync<BookingDto>();
            Assert.Equal(_existingBookingId, booking!.BookingId);
        }
    }
}
