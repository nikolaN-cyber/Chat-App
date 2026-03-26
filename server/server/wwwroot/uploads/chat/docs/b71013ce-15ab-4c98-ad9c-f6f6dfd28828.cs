using Application.Users.DTOs;
using Domain;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.Users.Queries
{
    public class LoginHandler : IRequestHandler<LoginQuery, LoginResponse?>
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public LoginHandler(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }
        public async Task<LoginResponse?> Handle(LoginQuery request, CancellationToken cancellationToken)
        {
            var user = await _context._users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.HashedPassword))
            {
                return null;
            }
            var token = GenerateJwtToken(user);
            var loginResponse = new LoginResponse
                (
                    user.Username,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.ProfileImage,
                    token
                );
            return loginResponse;
        }

        private string GenerateJwtToken(User User)
        {
            var key = Encoding.UTF8.GetBytes(_config["jwt:key"]!);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                new Claim("id", User.Id.ToString()),
                new Claim(ClaimTypes.Name, User.Username)
            }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }
    }
}
