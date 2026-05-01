using HotelBooking.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Endpoints
{
    public static class DatabaseEndpoints
    {
        public static void MapDatabaseEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/database");

            group.MapPost("/clear", async (HotelBookingContext db) =>
            {
                await db.Bookings.ExecuteDeleteAsync();
                await db.RoomAvailabilities.ExecuteDeleteAsync();
                await db.Rooms.ExecuteDeleteAsync();
                await db.Hotels.ExecuteDeleteAsync();
                await db.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('dbo.Bookings', RESEED, 0);");
                await db.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('dbo.RoomAvailabilities', RESEED, 0);");
                await db.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('dbo.Rooms', RESEED, 0);");
                await db.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('dbo.Hotels', RESEED, 0);");

                return Results.Ok();
            })
            .WithName("ClearDatabase")
            .WithSummary("Clear the database")
            .WithDescription("Drops and recreates the database schema. All data is lost. For development use only.")
            .WithTags("Database")
            .Produces(StatusCodes.Status200OK);

            group.MapPost("/seed", async (HotelBookingContext db) =>
            {
                if (await db.Hotels.AnyAsync())
                    return Results.BadRequest("Database already seeded.");

                var hotelNames = new[] { "Hotel ABC", "Hotel DEF", "Hotel GHI" };
                var hotels = hotelNames.Select(name => new Models.Hotel
                {
                    Name = name,
                    Rooms = new List<Models.Room>
                    {
                        new Models.Room { TypeId = 1 },
                        new Models.Room { TypeId = 1 },
                        new Models.Room { TypeId = 2 },
                        new Models.Room { TypeId = 2 },
                        new Models.Room { TypeId = 3 },
                        new Models.Room { TypeId = 3 }
                    }
                }).ToList();

                db.Hotels.AddRange(hotels);
                await db.SaveChangesAsync();

                var today = DateOnly.FromDateTime(DateTime.Today);
                var latestDate = new DateOnly(9999, 12, 31);

                foreach (var hotel in hotels)
                foreach (var room in hotel.Rooms)
                {
                    db.RoomAvailabilities.Add(new Models.RoomAvailability
                    {
                        RoomId = room.RoomId,
                        FromDate = today,
                        ToDate = latestDate
                    });
                }

                await db.SaveChangesAsync();
                return Results.Ok();
            })
            .WithName("SeedDatabase")
            .WithSummary("Seed the database")
            .WithDescription("Inserts sample hotels, rooms, and availability data. Returns 400 if data already exists.")
            .WithTags("Database")
            .Produces(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest);
        }
    }
}
