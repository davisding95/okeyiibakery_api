using cakeshop_api.Models;
using MongoDB.Driver;

namespace cakeshop_api.Services
{
    public class CategoryService
    {
        private readonly IMongoCollection<Category> _categories;

        public CategoryService(IMongoDatabase database)
        {
            _categories = database.GetCollection<Category>("Categories");
        }

        // Fetch all categories
        public async Task<List<Category>> GetAllCategories()
        {
            return await _categories.Find(category => true).ToListAsync();
        }
    }
}