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
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
                attachUserToContext(context, token);

            await this.next(context);
        }

        private void attachUserToContext(HttpContext context, string token)
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
                Id = long.Parse(jwtToken.Claims.First(x => x.Type == "uid").Value),
                Email = jwtToken.Claims.First(x => x.Type == "email").Value,

            };
            context.Items["Roles"] = jwtToken.Claims.Where(x => x.Type == "roles").Select(x => x.Value);
        }

    }

}