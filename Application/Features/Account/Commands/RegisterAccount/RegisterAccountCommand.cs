using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Account.Commands.RegisterAccount
{
    public class RegisterAccount : IRequest<Response<RegisterAccountViewModel>>
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }
        public string UserName { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }
    }

    public class RegisterAccountHandler : IRequestHandler<RegisterAccount, Response<RegisterAccountViewModel>>
    {
        private const string ERRORTITLE = "Account Error";

        private readonly IEmailService emailService;
        private readonly IAccountRepositoryAsync accountRepository;
        private readonly IPasswordHasher<User> passwordHasher;

        public RegisterAccountHandler(
            IAccountRepositoryAsync accountRepository,
            IPasswordHasher<User> passwordHasher,
            IEmailService emailService)
        {
            this.accountRepository = accountRepository;
            this.emailService = emailService;
            this.passwordHasher = passwordHasher;
        }
        public async Task<Response<RegisterAccountViewModel>> Handle(RegisterAccount request, CancellationToken cancellationToken)
        {
            var user = new User()
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.UserName,
                Email = request.Email
            };

            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
            var result = await this.accountRepository.CreateAsync(user, cancellationToken);
            if (result.Succeeded)
            {
                await this.emailService.SendAsync(
                    new Application.DTOs.Email.EmailRequest()
                    {
                        From = "mail@docmanager.com",
                        To = user.Email,
                        Body = $"Your account was create in DocManager App",
                        Subject = "Confirm Registration"
                    });
                return new Response<RegisterAccountViewModel>(new RegisterAccountViewModel { Id = user.Id, Email = user.Email }, message: $"User Registered");
            }
            else
            {
                throw new ApiException(ERRORTITLE, result.Errors.FirstOrDefault().Description);
            }
        }
    }
}