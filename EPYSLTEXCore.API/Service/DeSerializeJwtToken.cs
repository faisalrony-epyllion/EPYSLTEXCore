using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EPYSLTEX.Web.Services
{
    public class DeSerializeJwtToken : IDeSerializeJwtToken
    {
        public IEnumerable<Claim> GetClaims(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var securityToken = handler.ReadToken(token) as JwtSecurityToken;

            return securityToken.Claims;
        }
    }

    public interface IDeSerializeJwtToken
    {
        IEnumerable<Claim> GetClaims(string token);
    }
}