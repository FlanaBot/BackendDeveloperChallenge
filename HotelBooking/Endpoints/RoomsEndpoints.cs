using HotelBooking.Data;
using HotelBooking.DTOs;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Endpoints
{
    public static class RoomsEndpoints
    {
        public static void MapRoomsEndpoints(this WebApplication app)
        {
            app.MapGet("/rooms", async (DateOnly fromDate, DateOnly toDate, int guestCount, HotelBookingContext db) =>
            {
                var rooms = await db.Rooms
                    .Where(r =>
                        r.RoomType!.Capacity >= guestCount &&
                        db.RoomAvailabilities.Any(a =>
                            a.RoomId == r.RoomId &&
                            a.FromDate <= fromDate &&
                            a.ToDate >= toDate))
                    .Select(r => new RoomDto
                    {
                        RoomId = r.RoomId,
                        HotelId = r.HotelId,
                        HotelName = r.Hotel!.Name,
                        RoomTypeName = r.RoomType!.Name,
                        Capacity = r.RoomType!.Capacity
                    })
                    .ToListAsync();

                return Results.Ok(rooms);
            })
            .WithName("GetAvailableRooms")
            .WithSummary("Search available rooms")
            .WithDescription("Returns rooms with sufficient capacity that have an availability window covering the requested date range.")
            .WithTags("Rooms")
            .Produces<List<RoomDto>>();
        }
    }
}
