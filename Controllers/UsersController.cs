using CdnFreelancerApi.Data;
using CdnFreelancerApi.Models;
using CdnFreelancerApi.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using System.Text;

namespace CdnFreelancerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly MemoryCacheEntryOptions _cacheOptions;

        public UsersController(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;

            _cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
        }

        // GET: api/users?page=1&pageSize=10
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers(int page = 1, int pageSize = 10)
        {
            string cacheKey = $"Users_Page{page}_Size{pageSize}";

            if (!_cache.TryGetValue(cacheKey, out List<UserDto>? users))
            {
                users = await _context.Users
                    .Where(u => !u.IsDeleted)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        Username = u.Username,
                        Email = u.Email,
                        PhoneNumber = u.PhoneNumber,
                        Skillsets = u.Skillsets,
                        Hobby = u.Hobby,
                        CreatedAt = u.CreatedAt,
                        PasswordChangedAt = u.PasswordChangedAt
                    })
                    .ToListAsync();

                _cache.Set(cacheKey, users, _cacheOptions);
            }

            return Ok(users);
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            string cacheKey = $"User_{id}";

            if (!_cache.TryGetValue(cacheKey, out UserDto? userDto))
            {
                var user = await _context.Users
                    .Where(u => u.Id == id && !u.IsDeleted)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound();
                }
            
                userDto = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Skillsets = user.Skillsets,
                    Hobby = user.Hobby,
                    CreatedAt = user.CreatedAt,
                    PasswordChangedAt = user.PasswordChangedAt
                };

                _cache.Set(cacheKey, userDto, _cacheOptions);
            }
            return Ok(userDto);
        }

        // POST: api/users
        [HttpPost]
        public async Task<ActionResult<User>> RegisterUser(RegisterUserDto userDto)
        {
            if (string.IsNullOrWhiteSpace(userDto.Username) || string.IsNullOrWhiteSpace(userDto.Email) || string.IsNullOrWhiteSpace(userDto.Password))
            {
                return BadRequest("Username, email, and password are required.");
            }

            userDto.Email = userDto.Email.ToLower();
            var existingUser = await _context.Users.AnyAsync(u => u.Email == userDto.Email);
            if (existingUser)
            {
                return Conflict(new { message = "Email is already in use." });
            }

            var user = new User
            {
                Username = userDto.Username,
                Email = userDto.Email,
                PhoneNumber = userDto.PhoneNumber,
                Skillsets = userDto.Skillsets,
                Hobby = userDto.Hobby,
            };

            user.SetPassword(userDto.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _cache.Remove("Users_Page1_Size10");

            var userResponse = new
            {
                id = user.Id,
                username = user.Username,
                email = user.Email,
                phoneNumber = user.PhoneNumber,
                skillsets = user.Skillsets,
                hobby = user.Hobby,
                createdAt = user.CreatedAt
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, userResponse);
        }

        // PUT: api/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDto updateUserDto)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound(new { message = "User not found." });
            }
            
            if (string.IsNullOrWhiteSpace(updateUserDto.Username))
            {
                return BadRequest(new { message = "Username is required." });
            }

            // If email can be change by user
            //if (!string.IsNullOrWhiteSpace(updateUserDto.Email))
            //{
            //    var emailExists = await _context.Users.AnyAsync(u => u.Email == updateUserDto.Email && u.Id != id);
            //    if (emailExists)
            //    {
            //        return Conflict(new { message = "Email is already in use by another user." });
            //    }
            //    existingUser.Email = updateUserDto.Email.ToLower();
            //}

            existingUser.Username = updateUserDto.Username;
            existingUser.PhoneNumber = updateUserDto.PhoneNumber;
            existingUser.Skillsets = updateUserDto.Skillsets;
            existingUser.Hobby = updateUserDto.Hobby;

            try
            {
                await _context.SaveChangesAsync();
                _cache.Remove($"User_{id}");
                _cache.Remove("Users_Page1_Size10");

                return Ok(new
                {
                    id = existingUser.Id,
                    username = existingUser.Username,
                    phoneNumber = existingUser.PhoneNumber,
                    skillsets = existingUser.Skillsets,
                    hobby = existingUser.Hobby
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new { message = "The user was updated by another process. Please refresh and try again." });
            }
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsDeleted = true;
            await _context.SaveChangesAsync();
            _cache.Remove($"User_{id}");
            _cache.Remove("Users_Page1_Size10");
            return Ok(new { message = "User deleted successfully" });
        }

        [HttpPatch("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(int id, ChangePasswordDto changePasswordDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (!user.VerifyPassword(changePasswordDto.CurrentPassword))
            {
                return BadRequest("Current password is incorrect.");
            }

            if (changePasswordDto.NewPassword.Length < 6)
                return BadRequest("New password must be at least 6 characters long.");

            user.SetPassword(changePasswordDto.NewPassword);
            user.PasswordChangedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Password changed successfully" });
        }

    }
}
