using System.Net.Http.Json;
using HotelBooking.DTOs;
using Xunit;

namespace HotelBooking.Tests
{
    [Collection("Integration")]
    public class GetRoomsTests : IClassFixture<GetRoomsFixture>
    {
        private readonly HttpClient _client;
        private readonly GetRoomsFixture _fixture;

        public GetRoomsTests(GetRoomsFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Factory.CreateClient();
        }

        private async Task<List<RoomDto>> SearchRooms(DateOnly from, DateOnly to, int guestCount)
        {
            var response = await _client.GetAsync(
                $"/rooms?fromDate={from:yyyy-MM-dd}&toDate={to:yyyy-MM-dd}&guestCount={guestCount}");
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<List<RoomDto>>())!;
        }

        [Fact]
        public async Task Search_ExactBookingDates_ExcludesBookedRoom()
        {
            var rooms = await SearchRooms(_fixture.BookingFrom, _fixture.BookingTo, guestCount: 1);

            Assert.DoesNotContain(rooms, r => r.RoomId == _fixture.BookedSingleRoomId);
            Assert.Contains(rooms, r => r.RoomId == _fixture.OtherSingleRoomId);
        }

        [Fact]
        public async Task Search_DatesBeforeBooking_IncludesBookedRoom()
        {
            // [2035-05-01, 2035-05-09) is entirely before booking starts
            var rooms = await SearchRooms(
                new DateOnly(2035, 5, 1), new DateOnly(2035, 5, 9), guestCount: 1);

            Assert.Contains(rooms, r => r.RoomId == _fixture.BookedSingleRoomId);
        }

        [Fact]
        public async Task Search_DatesAfterBooking_IncludesBookedRoom()
        {
            // [2035-05-21, 2035-05-30) is entirely after booking ends
            var rooms = await SearchRooms(
                new DateOnly(2035, 5, 21), new DateOnly(2035, 5, 30), guestCount: 1);

            Assert.Contains(rooms, r => r.RoomId == _fixture.BookedSingleRoomId);
        }

        [Fact]
        public async Task Search_OverlapsBookingStart_ExcludesBookedRoom()
        {
            // [2035-05-05, 2035-05-15) crosses into the booking
            var rooms = await SearchRooms(
                new DateOnly(2035, 5, 5), new DateOnly(2035, 5, 15), guestCount: 1);

            Assert.DoesNotContain(rooms, r => r.RoomId == _fixture.BookedSingleRoomId);
        }

        [Fact]
        public async Task Search_OverlapsBookingEnd_ExcludesBookedRoom()
        {
            // [2035-05-15, 2035-05-25) crosses out of the booking
            var rooms = await SearchRooms(
                new DateOnly(2035, 5, 15), new DateOnly(2035, 5, 25), guestCount: 1);

            Assert.DoesNotContain(rooms, r => r.RoomId == _fixture.BookedSingleRoomId);
        }

        [Fact]
        public async Task Search_AdjacentBeforeBooking_IncludesBookedRoom()
        {
            // [2035-05-01, 2035-05-10): ToDate == booking FromDate; no overlap (exclusive)
            var rooms = await SearchRooms(
                new DateOnly(2035, 5, 1), _fixture.BookingFrom, guestCount: 1);

            Assert.Contains(rooms, r => r.RoomId == _fixture.BookedSingleRoomId);
        }

        [Fact]
        public async Task Search_AdjacentAfterBooking_IncludesBookedRoom()
        {
            // [2035-05-20, 2035-05-25): FromDate == booking ToDate; no overlap (exclusive)
            var rooms = await SearchRooms(
                _fixture.BookingTo, new DateOnly(2035, 5, 25), guestCount: 1);

            Assert.Contains(rooms, r => r.RoomId == _fixture.BookedSingleRoomId);
        }

        [Fact]
        public async Task Search_WithinBookingDates_ExcludesBookedRoom()
        {
            // [2035-05-12, 2035-05-15) is entirely within the booked period
            var rooms = await SearchRooms(
                new DateOnly(2035, 5, 12), new DateOnly(2035, 5, 15), guestCount: 1);

            Assert.DoesNotContain(rooms, r => r.RoomId == _fixture.BookedSingleRoomId);
        }

        [Fact]
        public async Task Search_GuestCount3_ReturnsOnlyDeluxeRooms()
        {
            var rooms = await SearchRooms(
                new DateOnly(2035, 7, 1), new DateOnly(2035, 7, 10), guestCount: 3);

            Assert.NotEmpty(rooms);
            Assert.All(rooms, r => Assert.True(r.Capacity >= 3));
        }

        [Fact]
        public async Task Search_AllRooms_ReturnsResults()
        {
            // Unbooked date range — all rooms should be available
            var rooms = await SearchRooms(
                new DateOnly(2040, 1, 1), new DateOnly(2040, 1, 5), guestCount: 1);

            Assert.NotEmpty(rooms);
        }
    }
}
