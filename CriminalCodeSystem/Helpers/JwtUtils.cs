using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JWT;
using JWT.Algorithms;
using JWT.Exceptions;
using JWT.Serializers;

namespace CriminalCodeSystem.Helpers
{
    public class JwtUtils
    {
        private static JwtToken? GetDataFromJwt(string accessToken)
        {
            IJsonSerializer serializer = new JsonNetSerializer();
            IDateTimeProvider provider = new UtcDateTimeProvider();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();

            IJwtValidator validator = new JwtValidator(serializer, provider);
            IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, algorithm);
            return decoder.DecodeToObject<JwtToken>(accessToken);
        }

        public static DateTime GetExpiryTimestampFromJwt(string accessToken)
        {
            try
            {
                var token = GetDataFromJwt(accessToken);
                if (token == null) {
                    return DateTime.MinValue;
                }
                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(token.exp);
                return dateTimeOffset.UtcDateTime;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        public static string GetUserNameFromJwt(string accessToken)
        {
            try
            {
                var token = GetDataFromJwt(accessToken);
                return token?.user_name ?? "";
            }
            catch
            {
                return "";
            }
        }
    }

    public class JwtToken
    {
        public long exp { get; set; }
        public string user_name { get; set; } = "";
    }
}
