using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using SportWear.Api.Models;
using SportWear.Api.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SportWear.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        public AuthController(
            UserManager<ApplicationUser> userManager, 
            IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(
                user, model.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            var roleResult = await _userManager.AddToRoleAsync(
                user, "Customer");

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return BadRequest(roleResult.Errors);
            }

            var token = await GenerateJwtToken(user);

            var response = new AuthResponseDto
            {
                Token = token,
                Email = user.Email!,
                UserName = user.UserName!,
                Role = "Customer"
            };
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Unauthorized("Неверный Email или пароль");
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!isPasswordValid)
            {
                return Unauthorized("Неверный Email или пароль");
            }

            var roles = await _userManager.GetRolesAsync(user);

            if (!roles.Any())
            {
                return Unauthorized("У пользователя не назначена роль");
            }

            var token = await GenerateJwtToken(user);

            var response = new AuthResponseDto
            {
                Token = token,
                Email = user.Email!,
                UserName = user.UserName!,
                Role = roles.First()
            };

            return Ok(response);
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!)
            };
   
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var secretkey = _configuration["Jwt:SecretKey"];

            if (string.IsNullOrWhiteSpace(secretkey))
            {
                throw new InvalidOperationException(
                    "JWT SecretKey не настроен");
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretkey));

            var credentials = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
