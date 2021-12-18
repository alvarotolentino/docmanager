using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DocManager.Api.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate next;

        private readonly JWTokenSettings jwtSettings;


        public JwtMiddleware(RequestDelegate next,
        IOptions<JWTokenSettings> jwtSettings)
        {
            this.next = next;
            this.jwtSettings = jwtSettings.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            if (string.IsNullOrWhiteSpace(context.Request.Headers["Authorization"]))
                await this.next(context);

            var bearerToken = context.Request.Headers["Authorization"][0]?.Split(" ");
            if (bearerToken != null)
                attachUserToContext(context, bearerToken[1]);

            await this.next(context);
        }

        private void attachUserToContext(HttpContext context, string token)
        {

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = this.jwtSettings.Issuer,
                    ValidAudience = this.jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.jwtSettings.Key))
                }, out SecurityToken validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;

                context.Items["User"] = new User
                {
                    Id = int.Parse(jwtToken.Claims.First(x => x.Type == "uid").Value),
                    Email = jwtToken.Claims.First(x => x.Type == "email").Value,

                };
                context.Items["Roles"] = jwtToken.Claims.Where(x => x.Type == "roles").Select(x => x.Value);
            }
            catch (Microsoft.IdentityModel.Tokens.SecurityTokenExpiredException)
            {
                throw new SecurityTokenExpiredException("The token is expired");
            }
            catch (Exception exception)
            {
                throw exception;
            }

        }

    }

}