using Microsoft.AspNetCore.Http;
using CriminalCodeSystem.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CriminalCodeSystem.Response;

namespace CriminalCodeSystem.Middlewares
{
    public class AuthorizationMiddleware
    {
        private readonly RequestDelegate nextStep;

        public AuthorizationMiddleware(RequestDelegate next)
        {
            nextStep = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                using (var requestBodyCopy = new System.IO.MemoryStream())
                {
                    await context.Request.Body.CopyToAsync(requestBodyCopy);
                    requestBodyCopy.Position = 0; // rewind

                    string requestBody = await (new System.IO.StreamReader(requestBodyCopy)).ReadToEndAsync();

                    var jsonSettings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                    };

                    var body = JsonConvert.DeserializeObject<AuthorizationRequestBodyPayload>(requestBody, jsonSettings);
                    if (body == null) {
                        body = new AuthorizationRequestBodyPayload();
                    }

                    if (string.IsNullOrEmpty(body.AccessToken))
                    {
                        string token = context.Request.Headers["Authorization"];
                        if (string.IsNullOrEmpty(token)) {
                            if (!string.IsNullOrEmpty(context.Request.Query["accessToken"])) {
                                body.AccessToken = context.Request.Query["accessToken"];
                            }
                            else {
                                throw new Exception("User token is not provided in the request body or in the header!");
                            }
                        }
                        else {
                            body.AccessToken = token.Replace("Bearer ", "");
                        }
                    }

                    if (!JwtManager.ValidateToken(body.AccessToken))
                    {
                        throw new Exception("Invalid provided access token!");
                    }

                    requestBodyCopy.Position = 0; // rewind again
                    context.Request.Body = requestBodyCopy; // put back in place for downstream handlers

                    await nextStep(context);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                var data = new AuthorizationResponseDataPayload
                {
                    Status = -1,
                    Message = "TOKEN AUTHENTICATION FAILED",
                };
                var payload = JsonConvert.SerializeObject(data);
                context.Response.Headers.Add("Content-Type", "application/json");
                await context.Response.WriteAsync(payload);
            }
        }
    }

    public class AuthorizationRequestBodyPayload
    {
        public string AccessToken { get; set; } = "";
    }

    public class AuthorizationResponseDataPayload
    {
        public int Status { get; set; }
        public string Message { get; set; } = "";
    }
}
