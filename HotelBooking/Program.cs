using HotelBooking.Data;
using HotelBooking.Endpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Info.Title = "Hotel Booking API";
        document.Info.Description = "Search hotels and available rooms, and create bookings.";
        document.Info.Version = "v1";
        return Task.CompletedTask;
    });
});
builder.Services.AddDbContext<HotelBookingContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.MapOpenApi("/swagger/v1/swagger.json");
app.UseHttpsRedirection();

app.MapHotelEndpoints();
app.MapRoomsEndpoints();
app.MapBookingEndpoints();
app.MapDatabaseEndpoints();

app.Run();
