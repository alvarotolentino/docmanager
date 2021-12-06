using System;
using System.Text;
using System.Threading.Tasks;
using Application.Enums;
using Application.Features.Account.Commands.AuthenticateUser;
using Application.Features.Account.Commands.RegisterAccount;
using Application.Interfaces.Services;
using DocManager.Api.Services;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace DocManagerTest.AplicationLayerTest
{
    public sealed class AccountTests : BaseTest
    {
        [Fact]
        public async Task Login_With_Valid_Account()
        {
            ServiceCollection.AddScoped<IAuthenticatedUserService, AuthenticatedUserService>();
            var serviceProvider = ServiceCollection.BuildServiceProvider();

            Mediator = serviceProvider.GetService<IMediator>();

            var request = new AuthenticateUser
            {
                Email = "adminuser@gmail.com",
                Password = "P@ssw0rd",
                IpAddress = "0.0.0.0"
            };
            var response = await Mediator.Send(request);

            Assert.True(response.Succeeded);
            Assert.NotNull(response.Data?.JWToken);

        }

        [Fact]
        public async Task Create_New_Account_Successfully()
        {
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContextAccessor.Setup(o => o.HttpContext.Items["User"]).Returns(new User { Id = (int)UserRoles.Admin });
            ServiceCollection.AddScoped<IHttpContextAccessor>((provider) =>
            {
                return mockHttpContextAccessor.Object;
            });
            ServiceCollection.AddScoped<IAuthenticatedUserService, AuthenticatedUserService>();
            var serviceProvider = ServiceCollection.BuildServiceProvider();
            Mediator = serviceProvider.GetService<IMediator>();


            var request = new RegisterAccount
            {
                FirstName = Faker.Name.First(),
                LastName = Faker.Name.Last(),
                Email = Faker.Internet.Email(),
                UserName = Faker.Internet.UserName(),
                Password = "P@ssw0rd",
                ConfirmPassword = "P@ssw0rd",
            };
            var response = await Mediator.Send(request);
            Assert.True(response.Succeeded);
        }



    }
}