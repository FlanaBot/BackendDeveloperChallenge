using HotelBooking.Data;
using HotelBooking.DTOs;
using HotelBooking.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Endpoints
{
    public static class BookingEndpoints
    {
        public static void MapBookingEndpoints(this WebApplication app)
        {
            app.MapGet("/bookings/{id}", async (int id, HotelBookingContext db) =>
            {
                var booking = await db.Bookings
                    .Where(b => b.BookingId == id)
                    .Select(b => new BookingDto
                    {
                        BookingId = b.BookingId,
                        HotelId = b.HotelId,
                        RoomId = b.RoomId,
                        FromDate = b.FromDate,
                        ToDate = b.ToDate,
                        GuestCount = b.GuestCount
                    })
                    .FirstOrDefaultAsync();

                return booking is null ? Results.NotFound() : Results.Ok(booking);
            })
            .WithName("GetBooking")
            .WithSummary("Get booking by ID")
            .WithDescription("Returns the booking with the specified ID.")
            .WithTags("Bookings")
            .Produces<BookingDto>()
            .Produces(StatusCodes.Status404NotFound);

            app.MapPost("/hotels/{hotelId}/rooms/{roomId}/bookings",
                async (int hotelId, int roomId, CreateBookingRequest request, HotelBookingContext db) =>
                {
                    var today = DateOnly.FromDateTime(DateTime.Today);

                    if (request.FromDate < today)
                        return Results.BadRequest("FromDate cannot be in the past.");

                    if (request.FromDate >= request.ToDate)
                        return Results.BadRequest("FromDate must be before ToDate.");

                    if (request.GuestCount < 1)
                        return Results.BadRequest("GuestCount must be at least 1.");

                    var room = await db.Rooms
                        .Include(r => r.RoomType)
                        .Include(r => r.Hotel)
                        .FirstOrDefaultAsync(r => r.RoomId == roomId);

                    if (room is null)
                        return Results.NotFound();

                    if (request.GuestCount > room.RoomType!.Capacity)
                        return Results.BadRequest(
                            $"GuestCount {request.GuestCount} exceeds room capacity of {room.RoomType.Capacity}.");

                    await using var transaction = await db.Database.BeginTransactionAsync();

                    var availability = await db.RoomAvailabilities
                        .FirstOrDefaultAsync(ra => ra.RoomId == roomId
                            && ra.FromDate <= request.FromDate
                            && ra.ToDate >= request.ToDate);

                    if (availability is null)
                        return Results.UnprocessableEntity("Room is not available for the requested dates.");

                    if (availability.FromDate == request.FromDate && availability.ToDate == request.ToDate)
                    {
                        db.RoomAvailabilities.Remove(availability);
                    }
                    else if (availability.FromDate == request.FromDate)
                    {
                        availability.FromDate = request.ToDate;
                    }
                    else if (availability.ToDate == request.ToDate)
                    {
                        availability.ToDate = request.FromDate;
                    }
                    else
                    {
                        db.RoomAvailabilities.Add(new RoomAvailability
                        {
                            RoomId = roomId,
                            FromDate = request.ToDate,
                            ToDate = availability.ToDate
                        });
                        availability.ToDate = request.FromDate;
                    }

                    var booking = new Booking
                    {
                        HotelId = hotelId,
                        Hotel = room.Hotel!,
                        RoomId = roomId,
                        Room = room,
                        FromDate = request.FromDate,
                        ToDate = request.ToDate,
                        GuestCount = (byte)request.GuestCount
                    };
                    db.Bookings.Add(booking);

                    await db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Results.Created($"/bookings/{booking.BookingId}", new BookingDto
                    {
                        BookingId = booking.BookingId,
                        HotelId = booking.HotelId,
                        RoomId = booking.RoomId,
                        FromDate = booking.FromDate,
                        ToDate = booking.ToDate,
                        GuestCount = booking.GuestCount
                    });
                })
            .WithName("CreateBooking")
            .WithSummary("Create a booking")
            .WithDescription("Books a room for the specified hotel and date range. Validates guest count against room capacity and ensures availability.")
            .WithTags("Bookings")
            .Produces<BookingDto>(StatusCodes.Status201Created)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<string>(StatusCodes.Status422UnprocessableEntity);
        }
    }
}
