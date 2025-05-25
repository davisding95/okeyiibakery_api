using System.Security.Claims;
using cakeshop_api.Models;
using cakeshop_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace cakeshop_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]

    public class PaymentController : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly UserService _userService;
        private readonly IConfiguration _configuration;
        private readonly string? _stripeSecretKey;

        public PaymentController(OrderService orderService, UserService userService, IConfiguration configuration)
        {
            _orderService = orderService;
            _userService = userService;
            _configuration = configuration;
            _stripeSecretKey = _configuration["Stripe:SecretKey"];
            StripeConfiguration.ApiKey = _stripeSecretKey;
        }

        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] Order request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated.");
                }

                // Create line items for Stripe
                var lineItems = new List<SessionLineItemOptions>();
                foreach (var item in request.OrderItems)
                {
                    lineItems.Add(new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.CakePrice * 100), // Convert to cents
                            Currency = "nzd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.CakeName,
                                Images = new List<string> { item.CakeImage },
                                Description = $"Size: {item.CakeSize}"
                            }
                        },
                        Quantity = item.Quantity
                    });
                }

                // Store temporary order data
                var pendingOrderId = await _orderService.CreatePendingOrder(request);

                // Create Stripe Checkout session
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = lineItems,
                    Mode = "payment",
                    SuccessUrl = $"{_configuration["Frontend:Url"]}/payment/success?session_id={{CHECKOUT_SESSION_ID}}",
                    CancelUrl = $"{_configuration["Frontend:Url"]}/orders/{pendingOrderId}",
                    Metadata = new Dictionary<string, string>
                    {
                        {"PendingOrderId", pendingOrderId.ToString()},
                        {"UserId", userId}
                    }
                };

                var service = new SessionService();
                var session = await service.CreateAsync(options);

                return Ok(new { url = session.Url });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("verify/{sessionId}")]
        public async Task<IActionResult> VerifyPayment(string sessionId)
        {
            try
            {
                // Get user ID from the token
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var service = new SessionService();
                var session = await service.GetAsync(sessionId);

                if (session.PaymentStatus != "paid")
                {
                    return BadRequest(new { message = "Payment has not been completed" });
                }

                // Get the pending order from metadata
                var pendingOrderId = session.Metadata["PendingOrderId"];
                var pendingOrder = await _orderService.GetOrderById(pendingOrderId);

                if (pendingOrder == null)
                {
                    return NotFound(new { message = "Order information not found" });
                }

                return Ok(new
                {
                    paymentId = session.PaymentIntentId,
                    customerId = session.CustomerId,
                    items = pendingOrder.OrderItems,
                    success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}