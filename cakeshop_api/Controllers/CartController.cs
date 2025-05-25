using Microsoft.AspNetCore.Mvc;
using cakeshop_api.Services;
using Microsoft.AspNetCore.Authorization;
using cakeshop_api.Models;
using System.Security.Claims;

namespace cakeshop_api.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class CartController : ControllerBase
    {
        private readonly CartService _cartService;
        public CartController(CartService cartService)
        {
            _cartService = cartService;
        }

        [HttpPost]
        [Authorize(Roles = "user, admin")]
        public async Task<IActionResult> CreateCartAsync([FromBody] Cart cart)
        {
            var result = await _cartService.CreateCartAsync(cart);

            if (result is null)
                return BadRequest("Failed to add to cart.");

            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "user, admin")]
        public async Task<IActionResult> GetCartByUserAsync()
        {
            // Get the current user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId is null)
                return BadRequest("Failed to get user.");

            var result = await _cartService.GetCartsByUserIdAsync(userId);

            if (result is null)
                return BadRequest("Failed to get cart.");

            return Ok(result);
        }

        [HttpPut]
        [Authorize(Roles = "user, admin")]
        public async Task<IActionResult> UpdateCartAsync([FromBody] Cart cart)
        {
            var result = await _cartService.UpdateCartAsync(cart);

            if (result is null)
                return BadRequest("Failed to update cart.");

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "user, admin")]
        public async Task<IActionResult> DeleteCartAsync(string id)
        {
            var result = await _cartService.DeleteCartAsync(id);

            if (result is false)
                return BadRequest("Failed to delete cart.");

            return Ok(result);
        }

        [HttpDelete("clear/{userId}")]
        [Authorize(Roles = "user, admin")]
        public async Task<IActionResult> DeleteAllCartsByUserIdAsync(string userId)
        {
            var result = await _cartService.DeleteAllCartsByUserIdAsync(userId);

            if (result is false)
                return BadRequest("Failed to delete carts.");

            return Ok(result);
        }
    }
}