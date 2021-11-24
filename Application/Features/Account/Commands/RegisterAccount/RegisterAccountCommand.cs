using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Exceptions;
using Application.Interfaces.Services;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Account.Commands.RegisterAccount
{
    public class RegisterAccountCommand : IRequest<Response<long>>
    {
        public string firstname { get; set; }

        public string lastname { get; set; }

        public string email { get; set; }
        public string username { get; set; }

        public string password { get; set; }

        public string confirmpassword { get; set; }
    }

    public class RegisterAccountCommandHandler : IRequestHandler<RegisterAccountCommand, Response<long>>
    {
        private const string ERRORTITLE = "Account Error";

        private readonly UserManager<User> userManager;
        private readonly IEmailService emailService;

        public RegisterAccountCommandHandler(UserManager<User> userManager,
         IEmailService emailService)
        {
            this.userManager = userManager;
            this.emailService = emailService;


        }
        public async Task<Response<long>> Handle(RegisterAccountCommand request, CancellationToken cancellationToken)
        {
            var userWithSameUserName = await this.userManager.FindByNameAsync(request.username);
            if (userWithSameUserName != null)
            {
                throw new ApiException(ERRORTITLE, $"Username '{request.username}' is already taken.");
            }
            var user = new User
            {
                Email = request.email,
                FirstName = request.firstname,
                LastName = request.lastname,
                UserName = request.username
            };
            var userWithSameEmail = await this.userManager.FindByEmailAsync(request.email);
            if (userWithSameEmail == null)
            {
                var result = await this.userManager.CreateAsync(user, request.password);
                if (result.Succeeded)
                {
                    await this.userManager.AddToRoleAsync(user, Application.Enums.UserRoles.Basic.ToString());
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
                    throw new ApiException(ERRORTITLE, $"{result.Errors}");
                }
            }
            else
            {
                throw new ApiException(ERRORTITLE, $"Email {request.email } is already registered.");
            }
        }
    }
}