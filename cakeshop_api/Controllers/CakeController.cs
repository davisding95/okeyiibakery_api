
using Newtonsoft.Json;
using cakeshop_api.Models;
using cakeshop_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace cakeshop_api.Controllers
{

    [ApiController]
    [Route("api/[Controller]")]
    public class CakeController : ControllerBase
    {
        private readonly CakeService _cakeService;
        private readonly CategoryService _categoryService;
        private readonly CloudflareR2Service _cloudflareR2Service;

        // Constructor to inject CakeService
        public CakeController(CakeService cakeService, CategoryService categoryService, CloudflareR2Service cloudflareR2Service)
        {
            _cakeService = cakeService;
            _categoryService = categoryService;
            _cloudflareR2Service = cloudflareR2Service;
        }

        // Get all availabel cakes

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableCakes()
        {
            var availableCakes = await _cakeService.GetAvailableCakes();
            var categories = await _categoryService.GetAllCategories();

            return Ok(new { cakes = availableCakes, categories = categories });
        }

        // GET /api/cake
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllCakes()
        {
            var cakes = await _cakeService.GetAllCakes();
            var categories = await _categoryService.GetAllCategories();

            return Ok(new { cakes = cakes, categories = categories });
        }

        // POST /api/cake
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateCake([FromForm] CreateCakeRequest request)
        {

            var cakeOptions = JsonConvert.DeserializeObject<List<CakeOption>>(request.CakeOptions) ?? new List<CakeOption>();

            if (cakeOptions is null)
            {
                return BadRequest("Cake options are required.");
            }

            var cake = new Cake
            {
                CakeName = request.CakeName,
                CategoryId = request.CategoryId,
                CakeDescription = request.CakeDescription,
                CakeOptions = cakeOptions,
                UserId = request.UserId,
                IsPromoted = request.IsPromoted,
                IsAvailable = request.IsAvailable
            };

            if (request.CakeImages is not null)
            {
                var imageUrls = new List<string>();

                foreach (var file in request.CakeImages)
                {
                    using var stream = file.OpenReadStream();
                    string fileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{DateTime.Now.Ticks}{Path.GetExtension(file.FileName)}";

                    string imageUrl = await _cloudflareR2Service.UploadFileAsync(stream, fileName, file.ContentType);
                    imageUrls.Add(imageUrl);
                }
                cake.CakeImages = imageUrls;
            }

            var createdCake = await _cakeService.CreateCake(cake);

            return Ok(createdCake);
        }

        // PUT /api/cake/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateCake(string id, [FromForm] UpdateCakeRequest request)
        {
            // Print the request body
            Console.WriteLine(JsonConvert.SerializeObject(request));

            var cakeOptions = JsonConvert.DeserializeObject<List<CakeOption>>(request.CakeOptions) ?? new List<CakeOption>();

            var existingCake = await _cakeService.GetCakeById(id);
            request.Id = existingCake.Id;

            var cake = new Cake
            {
                Id = request.Id,
                CakeName = request.CakeName,
                CategoryId = request.CategoryId,
                CakeDescription = request.CakeDescription,
                CakeOptions = cakeOptions,
                UserId = request.UserId,
                IsPromoted = request.IsPromoted,
                IsAvailable = request.IsAvailable
            };

            if (existingCake is null)
            {
                return NotFound("Cake not found.");
            }

            if (request.CakeImages is not null)
            {
                var imageUrls = new List<string>();

                foreach (var file in request.CakeImages)
                {
                    using var stream = file.OpenReadStream();
                    string fileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{DateTime.Now.Ticks}{Path.GetExtension(file.FileName)}";

                    string imageUrl = await _cloudflareR2Service.UploadFileAsync(stream, fileName, file.ContentType);
                    imageUrls.Add(imageUrl);
                }

                if (request.ExistingCakeImages is not null)
                {
                    imageUrls.AddRange(request.ExistingCakeImages);
                }

                cake.CakeImages = imageUrls;
            }
            else
            {
                cake.CakeImages = request.ExistingCakeImages ?? new List<string>();
            }

            var updatedCake = await _cakeService.UpdateCake(id, cake);

            return Ok(updatedCake);
        }

        // DELETE /api/cake/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteCake(string id)
        {
            var existingCake = await _cakeService.GetCakeById(id);

            if (existingCake is null)
            {
                return NotFound("Cake not found.");
            }

            // Delete cake images from Cloudflare R2
            foreach (var imageUrl in existingCake.CakeImages)
            {
                await _cloudflareR2Service.DeleteFileAsync(imageUrl);
            }

            await _cakeService.DeleteCake(id);

            return Ok("Cake deleted successfully.");
        }
    }
}