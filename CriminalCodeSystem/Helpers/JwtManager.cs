using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace CriminalCodeSystem.Helpers;

public class JwtManager
{
    private static string Secret = "";
    private const int ExpireMinutes = 1 * 60 * 24 * 3; // 3 days to expire by default

    static JwtManager()
    {
        Secret = UserConfig.Credentials["MASTER_JWT_SECRET"];
    }

    public static string GenerateToken(string userName, int expireMinutes = ExpireMinutes)
    {
        var symmetricKey = Convert.FromBase64String(Secret);
        var tokenHandler = new JwtSecurityTokenHandler();

        var now = DateTime.UtcNow;
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] {
                new Claim("user_name", userName),
                new Claim("self_id", System.Guid.NewGuid().ToString()), // force JWT token to be different even if issued at same time
            }),
            IssuedAt = now,
            NotBefore = now,
            Expires = now.AddMinutes(expireMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(symmetricKey), SecurityAlgorithms.HmacSha256Signature)
        };

        SecurityToken securityToken = tokenHandler.CreateToken(tokenDescriptor);
        var token = tokenHandler.WriteToken(securityToken);

        return token;
    }

    public static string GenerateToken(string userName, DateTime notBefore, DateTime expirationPoint)
    {
        var symmetricKey = Convert.FromBase64String(Secret);
        var tokenHandler = new JwtSecurityTokenHandler();

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] {
                new Claim("user_name", userName),
                new Claim("self_id", System.Guid.NewGuid().ToString()), // force JWT token to be different even if issued at same time
            }),
            IssuedAt = notBefore,
            NotBefore = notBefore,
            Expires = expirationPoint,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(symmetricKey), SecurityAlgorithms.HmacSha256Signature)
        };

        SecurityToken securityToken = tokenHandler.CreateToken(tokenDescriptor);
        var token = tokenHandler.WriteToken(securityToken);

        return token;
    }

    public static bool ValidateToken(string token, string userName)
    {
        try
        {
            DateTime tokenExpiration = JwtUtils.GetExpiryTimestampFromJwt(token);
            if (DateTime.UtcNow >= tokenExpiration) {
                return false;
            }

            var tokenHandler = new JwtSecurityTokenHandler();

            if (tokenHandler.ReadToken(token) == null)
                return false;

            var symmetricKey = Convert.FromBase64String(Secret);

            var validationParameters = new TokenValidationParameters()
            {
                RequireExpirationTime = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                IssuerSigningKey = new SymmetricSecurityKey(symmetricKey)
            };

            tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            var validatedJwtToken = (JwtSecurityToken) validatedToken;
            var tokenUserName = validatedJwtToken.Claims.First(x => x.Type == "user_name").Value;

            return (string.IsNullOrEmpty(userName) || tokenUserName.ToLower() == userName.ToLower());
        }
        catch
        {
            return false;
        }
    }

    public static bool ValidateToken(string token)
    {
        try
        {
            DateTime tokenExpiration = JwtUtils.GetExpiryTimestampFromJwt(token);
            if (DateTime.UtcNow >= tokenExpiration) {
                return false;
            }

            var tokenHandler = new JwtSecurityTokenHandler();

            if (tokenHandler.ReadToken(token) == null)
                return false;

            var symmetricKey = Convert.FromBase64String(Secret);

            var validationParameters = new TokenValidationParameters()
            {
                RequireExpirationTime = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                IssuerSigningKey = new SymmetricSecurityKey(symmetricKey)
            };

            tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            return true;
        }
        catch
        {
            return false;
        }
    }
}