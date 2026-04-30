using HotelBooking.Data;
using HotelBooking.DTOs;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Endpoints
{
    public static class HotelEndpoints
    {
        public static void MapHotelEndpoints(this WebApplication app)
        {
            app.MapGet("/hotels", async (string name, HotelBookingContext db) =>
            {
                var hotels = await db.Hotels
                    .Where(h => h.Name == name)
                    .Select(h => new HotelDto { Id = h.Id, Name = h.Name })
                    .ToListAsync();

                return Results.Ok(hotels);
            })
            .WithName("GetHotels")
            .WithSummary("Search hotels by name")
            .WithDescription("Returns all hotels whose name exactly matches the provided query parameter.")
            .WithTags("Hotels")
            .Produces<List<HotelDto>>();
        }
    }
}
