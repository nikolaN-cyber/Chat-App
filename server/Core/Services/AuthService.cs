using Core.Interfaces;
using Core.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core.DTOs.Auth;
using Domain.Entities;
using Infrastructure.Contexts;
using Core.DTOs.UserStatus;

namespace Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _config = configuration;
        }

        public async Task<ApiResponse<bool>> RegisterAsync(RegisterData request, CancellationToken cancellationToken)
        {
            var confirmPasswordCheck = request.Password == request.ConfirmPassword ? true : false;
            if (!confirmPasswordCheck)
            {
                throw new ArgumentException("Passwords do not match.");
            }

            var existingUser = await _context._users.AnyAsync(u => u.Email == request.Email || u.Username == request.Username, cancellationToken);
            if (existingUser)
            {
                throw new ArgumentException("User with this email or username already exists.");
            }
            string hashedPassword = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password);
            var newUser = new User
            {
                Username = request.Username,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Age = request.Age,
                Email = request.Email,
                Password = hashedPassword
            };
            _context._users.Add(newUser);
            await _context.SaveChangesAsync(cancellationToken);
            return ApiResponse<bool>.SuccessResponse(true);
        }

        public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginData request, CancellationToken cancellationToken)
        {
            var user = await _context._users.Include(u => u.UserStatus).FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);
            if (user == null || !BCrypt.Net.BCrypt.EnhancedVerify(request.Password, user.Password))
            {
                throw new ArgumentException("Invalid credentials");
            }
            var token = GenerateJwtToken(user);
            return ApiResponse<LoginResponse>.SuccessResponse(new LoginResponse(
                user.Id,
                user.Username,
                user.FirstName,
                user.LastName,
                user.Age,
                user.Email,
                user.PhotoUrl,
                token
             ), "User successfully loged in");
        }

        private string GenerateJwtToken(User user)
        {
            var key = Encoding.UTF8.GetBytes(_config["jwt:key"]!);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                new Claim("id", user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }
    }
}
