using Core.Interfaces;
using Core.Types;
using Domain;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core.DTOs;

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
            var existingUser = await _context._users.AnyAsync(u => u.Email == request.Email || u.Username == request.Username, cancellationToken);
            if (existingUser)
            {
                return ApiResponse<bool>.FailureResponse("User already exists");
            }
            var confirmPasswordCheck = request.Password == request.ConfirmPassword ? true : false;
            if (!confirmPasswordCheck)
            {
                return ApiResponse<bool>.FailureResponse("Passwords do not match");
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
            var user = await _context._users.FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);
            if (user == null || !BCrypt.Net.BCrypt.EnhancedVerify(request.Password, user.Password))
            {
                return ApiResponse<LoginResponse>.FailureResponse("Invalid credentials");
            }
            var token = GenerateJwtToken(user);
            return ApiResponse<LoginResponse>.SuccessResponse(new LoginResponse(
                user.Id,
                user.Username,
                user.FirstName,
                user.LastName,
                user.Age,
                user.Email,
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
