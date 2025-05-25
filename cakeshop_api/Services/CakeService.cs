using cakeshop_api.Models;
using MongoDB.Driver;

namespace cakeshop_api.Services
{
    public class CakeService
    {
        private readonly IMongoCollection<Cake> _cakes;

        public CakeService(IMongoDatabase database)
        {
            _cakes = database.GetCollection<Cake>("Cakes");
        }

        public async Task<Cake> CreateCake(Cake cake)
        {
            await _cakes.InsertOneAsync(cake);
            return cake;
        }

        public async Task<Cake> UpdateCake(string id, Cake cake)
        {
            await _cakes.ReplaceOneAsync(c => c.Id == id, cake);
            return cake;
        }

        public async Task DeleteCake(string id)
        {
            await _cakes.DeleteOneAsync(cake => cake.Id == id);
        }

        public async Task<List<Cake>> GetAllCakes()
        {
            return await _cakes.Find(Cake => true).ToListAsync();
        }

        public async Task<List<Cake>> GetAvailableCakes()
        {
            return await _cakes.Find(cake => cake.IsAvailable == true).ToListAsync();
        }

        public async Task<Cake> GetCakeById(string id)
        {
            return await _cakes.Find(cake => cake.Id == id).FirstOrDefaultAsync();
        }
    }
}