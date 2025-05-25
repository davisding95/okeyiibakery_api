using cakeshop_api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace cakeshop_api.Services
{
    public class CartService
    {
        private readonly IMongoCollection<Cart> _carts;
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Cake> _cakes;

        public CartService(IMongoDatabase database)
        {
            _carts = database.GetCollection<Cart>("Carts");
            _users = database.GetCollection<User>("Users");
            _cakes = database.GetCollection<Cake>("Cakes");
        }

        public async Task<Cart> CreateCartAsync(Cart cart)
        {
            // Before creating a cart, check if there is the same cart for the same user and same cake
            var existingCart = await _carts.Find(c => c.UserId == cart.UserId && c.CakeId == cart.CakeId && c.OptionId == cart.OptionId).FirstOrDefaultAsync();

            // If so, update the quantity
            if (existingCart is not null)
            {
                existingCart.Quantity += cart.Quantity;
                await _carts.ReplaceOneAsync(c => c.Id == existingCart.Id, existingCart);
                return existingCart;
            }

            // If not, create a new cart
            await _carts.InsertOneAsync(cart);
            return cart;
        }

        public async Task<List<Cart>> GetCartsByUserIdAsync(string userId)
        {
            return await _carts.Find(c => c.UserId == userId).ToListAsync();
        }

        public async Task<Cart?> UpdateCartAsync(Cart cart)
        {
            await _carts.ReplaceOneAsync(c => c.Id == cart.Id, cart);
            return cart;
        }

        public async Task<bool> DeleteCartAsync(string id)
        {
            var result = await _carts.DeleteOneAsync(c => c.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<bool> DeleteAllCartsByUserIdAsync(string userId)
        {
            var result = await _carts.DeleteManyAsync(c => c.UserId == userId);
            return result.DeletedCount > 0;
        }

    }
}
