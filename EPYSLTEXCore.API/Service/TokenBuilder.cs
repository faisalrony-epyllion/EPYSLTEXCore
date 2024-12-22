using EPYSLTEXCore.Infrastructure.Entities;
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

            List<Claim> claims = new List<Claim>
        {
            new Claim(JwtTokenStorage.UserID, user.UserCode.ToString()),
            new Claim(JwtTokenStorage.CompanyId, user.CompanyId.ToString()),

        };

            string[] audiences = _configuration.GetSection("JwtSettings:Audiences").Get<string[]>();


            foreach (var audience in audiences)
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Aud, audience));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],

                claims: claims,
                expires: DateTime.Now.AddDays(1),  // Set token expiry time
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
