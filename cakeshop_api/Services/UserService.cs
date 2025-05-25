using MongoDB.Driver;
using cakeshop_api.Models;
using BC = BCrypt.Net.BCrypt;

namespace cakeshop_api.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;

        public UserService(IMongoDatabase database)
        {
            _users = database.GetCollection<User>("Users");
        }

        public async Task<User> CreateUser(string username, string email, string password, string role, string phone, string address)
        {
            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = BC.HashPassword(password),
                Role = role,
                PhoneNumber = phone,
                Address = address
            };

            await _users.InsertOneAsync(user);
            return user;
        }

        public async Task<List<User>> GetAllUsers()
        {
            return await _users.Find(user => true).ToListAsync();
        }

        public async Task<User> GetUserByEmail(string email)
        {
            return await _users.Find(user => user.Email == email).FirstOrDefaultAsync();
        }

        public Task<bool> ValidatePassword(User user, string password)
        {
            return Task.FromResult(BC.Verify(password, user.PasswordHash));
        }

        public async Task<User?> UpdateUserAsync(string id, UpdateUser updatedUser)
        {
            var user = await _users.Find(u => u.Id == id).FirstOrDefaultAsync();

            if (user == null) return null;

            // Only update if new values are provided
            if (!string.IsNullOrEmpty(updatedUser.Username))
                user.Username = updatedUser.Username;

            if (!string.IsNullOrEmpty(updatedUser.PhoneNumber))
                user.PhoneNumber = updatedUser.PhoneNumber;

            if (!string.IsNullOrEmpty(updatedUser.Address))
                user.Address = updatedUser.Address;

            if (!string.IsNullOrEmpty(updatedUser.PasswordHash))
                user.PasswordHash = BC.HashPassword(updatedUser.PasswordHash);

            await _users.ReplaceOneAsync(u => u.Id == id, user);

            return user;
        }

    }
}
