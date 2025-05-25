using cakeshop_api.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace cakeshop_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhooksController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly OrderService _orderService;
        private readonly string? _webhookSecret;

        public WebhooksController(
            IConfiguration configuration,
            OrderService orderService)
        {
            _configuration = configuration;
            _orderService = orderService;
            _webhookSecret = _configuration["Stripe:WebhookSecret"];
        }

        [HttpPost]
        public async Task<IActionResult> HandleWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _webhookSecret,
                    throwOnApiVersionMismatch: false
                );

                // Handle the checkout.session.completed event
                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Session;

                    if (session == null)
                    {
                        return BadRequest("Session is null");
                    }

                    // Fulfill the purchase
                    await _orderService.FulfillOrder(
                        session.Metadata["PendingOrderId"],
                        session.PaymentIntentId,
                        session.CustomerId
                    );
                }

                return Ok();
            }
            catch (StripeException e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}