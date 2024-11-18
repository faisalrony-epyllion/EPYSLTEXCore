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
        public string BuildToken(LoginUser user, DateTime expiresAtUtc)
        {
            var role = user.IsSuperUser ? UserRoles.SUPER_USER : user.IsAdmin ? UserRoles.ADMIN : UserRoles.GENERAL;
            var claims = new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserCode.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, role)
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppConstants.SYMMETRIC_SECURITY_KEY));
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            
            var jwt = new JwtSecurityToken(claims: claims, signingCredentials: signingCredentials, expires: expiresAtUtc);
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }
    }

    public interface ITokenBuilder
    {
        string BuildToken(LoginUser user, DateTime expiresAtUtc);
    }
}
