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
using Application.Interfaces.Repositories;

namespace Application.Features.Account.Commands.AuthenticateUser
{
    public class AuthenticateUser : IRequest<Response<AuthenticationUserViewModel>>
    {
        public string email { get; set; }
        public string password { get; set; }
        public string ipaddress { get; set; }

    }

    public class AuthenticateUserHandler : IRequestHandler<AuthenticateUser, Response<AuthenticationUserViewModel>>
    {

        private const string ERRORTITLE = "Account Error";

        private readonly IAccountRepositoryAsync accountRepository;
        private readonly JWTokenSettings jwtSettings;
        private readonly IPasswordHasher<User> passwordHasher;

        public AuthenticateUserHandler(
            IAccountRepositoryAsync accountRepository,
            IPasswordHasher<User> passwordHasher,
            IOptions<JWTokenSettings> jwtSettings)
        {
            this.accountRepository = accountRepository;
            this.jwtSettings = jwtSettings.Value;
            this.passwordHasher = passwordHasher;

        }
        public async Task<Response<AuthenticationUserViewModel>> Handle(AuthenticateUser request, CancellationToken cancellationToken)
        {
            var user = await this.accountRepository.FindByEmailAsync(request.email, cancellationToken);
            if (user == null)
            {
                throw new ApiException(ERRORTITLE, $"No Accounts Registered with {request.email}.");
            }

            var result = this.passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.password);
            if (result != PasswordVerificationResult.Success)
            {
                throw new ApiException(ERRORTITLE, $"Invalid Credentials for '{request.email}'.");
            }

            JwtSecurityToken jwtSecurityToken = GetJWToken(user, request.ipaddress);
            AuthenticationUserViewModel response = new AuthenticationUserViewModel();
            response.JWToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            var refreshToken = GetRefreshToken(request.ipaddress);
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

        private JwtSecurityToken GetJWToken(User user, string ipAddress)
        {
            var roleClaims = new List<Claim>();
            for (int i = 0; i < user.Roles.Count; i++)
            {
                roleClaims.Add(new Claim("roles", user.Roles[i].Name));
            }
            var groupClaims = new List<Claim>();
            for (int i = 0; i < user.Groups.Count; i++)
            {
                groupClaims.Add(new Claim("groups", user.Groups[i].Name));
            }

            ipAddress = ipAddress ?? NetworkHelper.GetIpAddress();

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("first_name", user.FirstName.ToString()),
                new Claim("last_name", user.LastName.ToString()),
                new Claim("uid", user.Id.ToString()),
                new Claim("ip", ipAddress)
            }
            .Union(roleClaims)
            .Union(groupClaims);

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