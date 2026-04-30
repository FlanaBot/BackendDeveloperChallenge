using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace HotelBookingTests.Swagger
{
    [Collection("Integration")]
    public class SwaggerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public SwaggerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task SwaggerJson_IsAvailable()
        {
            var response = await _client.GetAsync("/swagger/v1/swagger.json");
            response.EnsureSuccessStatusCode();
        }
    }
}
