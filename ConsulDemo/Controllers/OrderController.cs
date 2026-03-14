using Consul;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/order")]
    public class OrderController : ControllerBase
    {
        private readonly IConsulClient _consul;
        private readonly IHttpClientFactory _httpClientFactory;

        public OrderController(IConsulClient consul, IHttpClientFactory httpClientFactory)
        {
            _consul = consul;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("{customerId}")]
        public async Task<IActionResult> GetOrder(int customerId)
        {
            var services = await _consul.Health.Service("customer-service", "", true);
            var service = services.Response.FirstOrDefault()?.Service;
            if (service is null)
                return StatusCode(503, "Customer service not available.");

            var address = $"http://{service.Address}:{service.Port}/api/v1/customer/{customerId}";

            var http = _httpClientFactory.CreateClient();

            var customer = await http.GetFromJsonAsync<object>(address);
            if (customer is null)
                return NotFound($"Customer {customerId} not found.");

            return Ok(new
            {
                OrderId = 1001,
                Product = "Laptop",
                Customer = customer
            });
        }
    }
}