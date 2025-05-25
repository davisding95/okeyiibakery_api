namespace cakeshop_api.Models
{
    public class UserLogin
    {
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
    }
}