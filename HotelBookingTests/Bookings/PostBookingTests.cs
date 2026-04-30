using System.Net;
using System.Net.Http.Json;
using HotelBooking.Data;
using HotelBooking.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HotelBooking.Tests
{
    [Collection("Integration")]
    public class PostBookingTests : IClassFixture<BookingFixture>
    {
        private readonly HttpClient _client;
        private readonly BookingFixture _fixture;

        public PostBookingTests(BookingFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Factory.CreateClient();
        }

        [Fact]
        public async Task Book_PastFromDate_Returns400()
        {
            var request = new CreateBookingRequest
            {
                GuestCount = 1,
                FromDate = new DateOnly(2020, 1, 1),
                ToDate = new DateOnly(2070, 1, 5)
            };

            var response = await _client.PostAsJsonAsync(
                $"/hotels/{_fixture.HotelId}/rooms/{_fixture.SingleRoomId}/bookings", request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Book_FromDateNotBeforeOrEqualToToDate_Returns400()
        {
            var request = new CreateBookingRequest
            {
                GuestCount = 1,
                FromDate = new DateOnly(2035, 1, 5),
                ToDate = new DateOnly(2035, 1, 5)
            };

            var response = await _client.PostAsJsonAsync(
                $"/hotels/{_fixture.HotelId}/rooms/{_fixture.SingleRoomId}/bookings", request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Book_GuestCountExceedsCapacity_Returns400()
        {
            var request = new CreateBookingRequest
            {
                GuestCount = 2,
                FromDate = new DateOnly(2035, 2, 1),
                ToDate = new DateOnly(2035, 2, 5)
            };

            // Single room capacity = 1; sending guestCount = 2
            var response = await _client.PostAsJsonAsync(
                $"/hotels/{_fixture.HotelId}/rooms/{_fixture.SingleRoomId}/bookings", request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Book_UnknownRoom_Returns404()
        {
            var request = new CreateBookingRequest
            {
                GuestCount = 1,
                FromDate = new DateOnly(2035, 3, 1),
                ToDate = new DateOnly(2035, 3, 5)
            };

            var response = await _client.PostAsJsonAsync(
                $"/hotels/{_fixture.HotelId}/rooms/99999/bookings", request);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Book_Success_Returns201WithBookingDetails()
        {
            var request = new CreateBookingRequest
            {
                GuestCount = 2,
                FromDate = new DateOnly(2035, 4, 1),
                ToDate = new DateOnly(2035, 4, 5)
            };

            var response = await _client.PostAsJsonAsync(
                $"/hotels/{_fixture.HotelId}/rooms/{_fixture.DoubleRoomId}/bookings", request);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var booking = await response.Content.ReadFromJsonAsync<BookingDto>();
            Assert.NotNull(booking);
            Assert.Equal(_fixture.HotelId, booking.HotelId);
            Assert.Equal(_fixture.DoubleRoomId, booking.RoomId);
            Assert.Equal(new DateOnly(2035, 4, 1), booking.FromDate);
            Assert.Equal(new DateOnly(2035, 4, 5), booking.ToDate);
            Assert.Equal(2, booking.GuestCount);
        }

        [Fact]
        public async Task Book_OverlappingExistingBooking_Returns422()
        {
            // First booking on Hotel2 single room
            var first = new CreateBookingRequest
            {
                GuestCount = 1,
                FromDate = new DateOnly(2035, 6, 1),
                ToDate = new DateOnly(2035, 6, 10)
            };
            var firstResponse = await _client.PostAsJsonAsync(
                $"/hotels/{_fixture.Hotel2Id}/rooms/{_fixture.Hotel2SingleRoomId}/bookings", first);
            Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

            // Overlapping booking on same room
            var overlapping = new CreateBookingRequest
            {
                GuestCount = 1,
                FromDate = new DateOnly(2035, 6, 5),
                ToDate = new DateOnly(2035, 6, 15)
            };
            var overlapResponse = await _client.PostAsJsonAsync(
                $"/hotels/{_fixture.Hotel2Id}/rooms/{_fixture.Hotel2SingleRoomId}/bookings", overlapping);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, overlapResponse.StatusCode);
        }

        [Fact]
        public async Task Book_Success_SplitsAvailabilityInterval()
        {
            // Book Deluxe room for a mid-range window; original availability is [today, 9999-12-31)
            var from = new DateOnly(2036, 1, 10);
            var to = new DateOnly(2036, 1, 20);
            var request = new CreateBookingRequest { GuestCount = 1, FromDate = from, ToDate = to };

            var response = await _client.PostAsJsonAsync(
                $"/hotels/{_fixture.HotelId}/rooms/{_fixture.DeluxeRoomId}/bookings", request);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            using var scope = _fixture.Factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<HotelBookingContext>();

            var availabilities = await context.RoomAvailabilities
                .Where(ra => ra.RoomId == _fixture.DeluxeRoomId)
                .OrderBy(ra => ra.FromDate)
                .ToListAsync();

            Assert.Equal(2, availabilities.Count);

            var today = DateOnly.FromDateTime(DateTime.Today);
            Assert.Equal(today, availabilities[0].FromDate);
            Assert.Equal(from, availabilities[0].ToDate);

            Assert.Equal(to, availabilities[1].FromDate);
            Assert.Equal(new DateOnly(9999, 12, 31), availabilities[1].ToDate);
        }

        [Fact]
        public async Task Book_Success_PersistedAndRetrievableByGetBooking()
        {
            var request = new CreateBookingRequest
            {
                GuestCount = 1,
                FromDate = new DateOnly(2037, 3, 1),
                ToDate = new DateOnly(2037, 3, 10)
            };

            var createResponse = await _client.PostAsJsonAsync(
                $"/hotels/{_fixture.Hotel2Id}/rooms/{_fixture.Hotel2DoubleRoomId}/bookings", request);
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var created = await createResponse.Content.ReadFromJsonAsync<BookingDto>();
            Assert.NotNull(created);

            var getResponse = await _client.GetAsync($"/bookings/{created.BookingId}");
            getResponse.EnsureSuccessStatusCode();

            var fetched = await getResponse.Content.ReadFromJsonAsync<BookingDto>();
            Assert.Equal(created.BookingId, fetched!.BookingId);
            Assert.Equal(new DateOnly(2037, 3, 1), fetched.FromDate);
            Assert.Equal(new DateOnly(2037, 3, 10), fetched.ToDate);
        }
    }
}
