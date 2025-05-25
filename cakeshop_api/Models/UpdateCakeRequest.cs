namespace cakeshop_api.Models
{
    public class UpdateCakeRequest
    {
        public required string Id { get; set; }
        public required string CakeName { get; set; }
        public required string CategoryId { get; set; }
        public required string CakeDescription { get; set; }
        public List<IFormFile>? CakeImages { get; set; }
        public required string CakeOptions { get; set; }
        public required string UserId { get; set; }
        public List<string>? ExistingCakeImages { get; set; }

        public required bool IsPromoted { get; set; }
        public required bool IsAvailable { get; set; }
    }
}