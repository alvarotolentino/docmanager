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
    public class RegisterAccountCommand : IRequest<Response<long>>
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }
        public string UserName { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }
    }

    public class RegisterAccountCommandHandler : IRequestHandler<RegisterAccountCommand, Response<long>>
    {
        private const string ERRORTITLE = "Account Error";

        private readonly IEmailService emailService;
        private readonly IAccountRepositoryAsync accountRepository;
        IPasswordHasher<User> passwordHasher;

        public RegisterAccountCommandHandler(
            IAccountRepositoryAsync accountRepository,
            IPasswordHasher<User> passwordHasher,
            IEmailService emailService)
        {
            this.accountRepository = accountRepository;
            this.emailService = emailService;
            this.passwordHasher = passwordHasher;
        }
        public async Task<Response<long>> Handle(RegisterAccountCommand command, CancellationToken cancellationToken)
        {
            var user = new User()
            {
                FirstName = command.FirstName,
                LastName = command.LastName,
                UserName = command.UserName,
                Email = command.Email
            };

            user.PasswordHash = passwordHasher.HashPassword(user, command.Password);
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
                return new Response<long>(user.Id, message: $"User Registered");
            }
            else
            {
                var errors = Utf8Json.JsonSerializer.ToJsonString(result.Errors);
                throw new ApiException(ERRORTITLE, $"{errors}");
            }
        }
    }
}