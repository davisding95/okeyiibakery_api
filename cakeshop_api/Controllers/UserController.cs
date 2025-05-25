using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using cakeshop_api.Models;
using cakeshop_api.Services;
using cakeshop_api.Utils;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace cakeshop_api.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly JwtHelper _jwtHelper;

        public UserController(UserService userService, JwtHelper jwtHelper)
        {
            _userService = userService;
            _jwtHelper = jwtHelper;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            var existingUser = await _userService.GetUserByEmail(user.Email);
            if (existingUser is not null)
            {
                return BadRequest(new { message = "User with this email already exists." });
            }

            var address = user.Address ?? string.Empty;
            var createdUser = await _userService.CreateUser(user.Username, user.Email, user.PasswordHash, user.Role, user.PhoneNumber, address);

            var token = _jwtHelper.GenerateJwtToken(createdUser);

            return Ok(new { User = createdUser, Token = token });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLogin user)
        {
            var existingUser = await _userService.GetUserByEmail(user.Email);
            if (existingUser is null)
            {
                return BadRequest(new { message = "Invalid Email or Password! Please try again." });
            }

            var isPasswordValid = await _userService.ValidatePassword(existingUser, user.PasswordHash);
            if (!isPasswordValid)
            {
                return BadRequest(new { message = "Invalid Email or Password! Please try again." });
            }

            var token = _jwtHelper.GenerateJwtToken(existingUser);

            // Check if the user is an admin
            var isAdmin = existingUser.Role == "admin";

            if (isAdmin)
            {
                var users = await _userService.GetAllUsers();
                return Ok(new { User = existingUser, Users = users, Token = token });
            }


            return Ok(new { User = existingUser, Token = token });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "user, admin")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUser updatedUser)
        {
            var user = await _userService.UpdateUserAsync(id, updatedUser);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            return Ok(new { message = "User updated successfully", User = user });
        }


        [HttpGet("me")]
        [Authorize(Roles = "user, admin")]
        public async Task<IActionResult> GetCurrentUserByToken()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (email is null) return BadRequest(new { message = "Invalid token." });

            var user = await _userService.GetUserByEmail(email);

            var isAdmin = User.IsInRole("admin");
            if (isAdmin)
            {
                var users = await _userService.GetAllUsers();
                return Ok(new { User = user, Users = users });
            }

            return Ok(new { User = user });
        }
    }
}