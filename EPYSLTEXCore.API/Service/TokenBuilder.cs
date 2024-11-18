using EPYSLTEX.Core.Entities.Gmt;
using EPYSLTEXCore.Application.Entities;
using EPYSLTEXCore.Infrastructure.Static;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EPYSLTEX.Web.Services
{
    public class TokenBuilder : ITokenBuilder
    {
        private readonly IConfiguration _configuration;

        public TokenBuilder(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string BuildToken(LoginUser user, DateTime expiresAtUtc)
        {
            var role = user.IsSuperUser ? UserRoles.SUPER_USER : user.IsAdmin ? UserRoles.ADMIN : UserRoles.GENERAL;
            var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, role)  // Add roles or any other claims you need
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),  // Set token expiry time
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public interface ITokenBuilder
    {
        string BuildToken(LoginUser user, DateTime expiresAtUtc);
    }
}
