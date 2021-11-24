using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Application.Common;
using Application.Exceptions;
using Domain.Settings;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Application.Helpers;
using System.Linq;
using Domain.Entities;
using Microsoft.Extensions.Options;

namespace Application.Features.Account.Commands.AuthenticateUser
{
    public class AuthenticateUserCommand : IRequest<Response<AuthenticationUserViewModel>>
    {
        public string email { get; set; }
        public string password { get; set; }
        public string ipaddress { get; set; }

    }

    public class AuthenticateUserCommandHandler : IRequestHandler<AuthenticateUserCommand, Response<AuthenticationUserViewModel>>
    {

        private const string ERRORTITLE = "Account Error";

        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly JWTokenSettings jwtSettings;

        public AuthenticateUserCommandHandler(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IOptions<JWTokenSettings> jwtSettings)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.jwtSettings = jwtSettings.Value;
        }
        public async Task<Response<AuthenticationUserViewModel>> Handle(AuthenticateUserCommand command, CancellationToken cancellationToken)
        {
            var user = await this.userManager.FindByEmailAsync(command.email);
            if (user == null)
            {
                throw new ApiException(ERRORTITLE, $"No Accounts Registered with {command.email}.");
            }
            
            var result = await this.signInManager.PasswordSignInAsync(user.UserName, command.password, false, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                throw new ApiException(ERRORTITLE, $"Invalid Credentials for '{command.email}'.");
            }

            JwtSecurityToken jwtSecurityToken = await GetJWToken(user, command.ipaddress);
            AuthenticationUserViewModel response = new AuthenticationUserViewModel();
            response.JWToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            var rolesList = await this.userManager.GetRolesAsync(user).ConfigureAwait(false);
            var refreshToken = GetRefreshToken(command.ipaddress);
            response.RefreshToken = refreshToken.Token;
            return new Response<AuthenticationUserViewModel>(response, $"Authenticated {user.UserName}");

        }

        private string GetRandomToken()
        {
            using (var cryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                var randomBytes = new byte[40];
                cryptoServiceProvider.GetBytes(randomBytes);
                return BitConverter.ToString(randomBytes).Replace("-", "");
            }
        }

        private RefreshToken GetRefreshToken(string ipAddress)
        {
            return new RefreshToken
            {
                Token = GetRandomToken(),
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };
        }

        private async Task<JwtSecurityToken> GetJWToken(User user, string ipAddress)
        {
            var userClaims = await this.userManager.GetClaimsAsync(user);
            var roles = await this.userManager.GetRolesAsync(user);

            var roleClaims = new List<Claim>();

            for (int i = 0; i < roles.Count; i++)
            {
                roleClaims.Add(new Claim("roles", roles[i]));
            }

            ipAddress = ipAddress ?? NetworkHelper.GetIpAddress();

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id.ToString()),
                new Claim("ip", ipAddress)
            }
            .Union(userClaims)
            .Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.jwtSettings.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: this.jwtSettings.Issuer,
                audience: this.jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(this.jwtSettings.Expiration),
                signingCredentials: signingCredentials);
            return jwtSecurityToken;
        }

    }
}