using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using cakeshop_api.Services;
using cakeshop_api.Models;
using Newtonsoft.Json;

namespace cakeshop_api.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;
        public OrderController(OrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        [Authorize(Roles = "user, admin")]
        public async Task<IActionResult> GetOrdersByUserAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var isAdmin = userRole == "admin";

            if (userId is null)
                return BadRequest("Failed to get user.");

            if (isAdmin)
            {
                var Res = await _orderService.GetAllOrdersAsync();

                if (Res is null)
                    return BadRequest("Failed to get orders.");

                return Ok(Res);
            }

            var Results = await _orderService.GetOrdersByUserAsync(userId);

            if (Results is null)
                return BadRequest("Failed to get orders.");

            return Ok(Results);
        }

        [HttpPost]
        [Authorize(Roles = "user, admin")]
        public async Task<IActionResult> CreateOrderAsync([FromBody] Order order)
        {
            var result = await _orderService.CreatePendingOrder(order);

            if (result is null)
                return BadRequest("Failed to create order.");

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "user, admin")]
        public async Task<IActionResult> DeleteOrderAsync(string id)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userRole is null)
                return BadRequest("Failed to get user role.");

            var result = await _orderService.DeleteOrderAsync(id, userRole);

            if (!result)
                return BadRequest(new { message = "Failed to cancel order. Your order is confirmed or completed. Please contact our customer service." });

            return Ok("Order deleted.");
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "user, admin")]
        public async Task<IActionResult> UpdateOrderAsync(string id, [FromBody] Order order)
        {
            Console.WriteLine("Request body: " + JsonConvert.SerializeObject(order));
            var result = await _orderService.UpdateOrderAsync(order);

            if (!result)
                return BadRequest("Failed to update order.");

            return Ok("Order updated.");
        }

        [HttpGet("payment-intent/{paymentIntentId}")]
        [Authorize(Roles = "user, admin")]
        public async Task<IActionResult> GetPaymentIntent(string paymentIntentId)
        {
            var result = await _orderService.GetPaymentIntent(paymentIntentId);

            if (result is null)
                return BadRequest("Failed to get order.");
            return Ok(result);
        }
    }
}