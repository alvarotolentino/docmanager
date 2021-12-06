using System.IO;
using System.Net.Mime;
using System.Reflection;
using System.Threading.Tasks;
using Application.Enums;
using Application.Features.Documents.Commands.CreateDocument;
using Application.Features.Documents.Queries.DownloadDocumentById;
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
    public class DocumentTests : BaseTest
    {

        [Fact]
        public async Task Upload_Document_As_Admin_Successfully()
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

            var request = new CreateDocument
            {
                Category = "test",
                Description = "test"
            };

            var assembly = Assembly.GetExecutingAssembly();
            using var stream = File.OpenRead("Resources/file.txt");

            request.File = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name))
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/plain"
            };

            var response = await Mediator.Send(request);
            Assert.True(response.Succeeded);
            Assert.True(response.Data.Id > 0);

        }

        [Fact]
        public async Task Download_Document_As_Admin_Successfully()
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

            var request = new CreateDocument
            {
                Category = "test",
                Description = "test"
            };

            var assembly = Assembly.GetExecutingAssembly();
            using var stream = File.OpenRead("Resources/file.txt");

            request.File = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name))
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/plain"
            };

            var response = await Mediator.Send(request);
            Assert.True(response.Succeeded);
            Assert.True(response.Data.Id > 0);

            var documentResponse = await Mediator.Send(new DownloadDocumentByIdQuery { Id = response.Data.Id });

            Assert.True(documentResponse.Succeeded);
            Assert.NotNull(documentResponse.Data.Content);
            Assert.True(documentResponse.Data.Content.Length > 0);

        }
    }
}