using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Application.Enums;
using Application.Features.Documents.Commands.CreateDocument;
using Application.Features.Documents.Commands.DeleteDocument;
using Application.Features.Documents.Queries.DownloadDocumentById;
using Application.Features.Documents.Queries.GetAllDocuments;
using Application.Features.Documents.Queries.GetDocumentById;
using DocManager.Api.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DocManager.Api.Controllers.v1
{
    [ApiVersion("1.0")]
    public class DocumentsController : BaseApiController
    {
        /// <summary>
        /// Allows to download a document by its id to admin or manager roles
        /// </summary>
        /// <param name="id">Document Id</param>
        /// <returns></returns>
        [HttpGet("download/{id}")]
        public async Task<IActionResult> GetDocument(long id)
        {
            var result = await Mediator.Send(new DownloadDocumentByIdQuery { id = id });
            var memoryStream = new MemoryStream(result.Data.Content);
            memoryStream.Position = 0;
            return File(memoryStream, result.Data.ContentType);
        }

        /// <summary>
        /// Allows to get document info to admin or manager roles
        /// </summary>
        /// <param name="id">Document Id</param>
        /// <returns></returns>
        [HttpGet("info/{id}")]
        public async Task<IActionResult> GetInfo(long id)
        {
            return Ok(await Mediator.Send(new GetDocumentInfoByIdQuery { id = id }));
        }

        /// <summary>
        /// Gets all documents 
        /// </summary>
        /// <param name="pagination">Page size and page number</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetDocuments([FromQuery] GetAllDocumentsParameter pagination)
        {
            return Ok(await Mediator.Send(new GetAllDocumentsQuery() { PageSize = pagination.pagesize, PageNumber = pagination.pagenumber }));
        }

        /// <summary>
        /// Uploads a new document
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [Authorize(UserRoles.Admin, UserRoles.Manager)]
        [HttpPost("upload")]
        public async Task<IActionResult> CreateDocument([FromForm] CreateDocumentCommand command)
        {
            // var command = new CreateDocumentCommand { file = file, description = description, category = category };
            return Ok(await Mediator.Send(command));
        }

        /// <summary>
        /// Deletes a document by its id
        /// </summary>
        /// <param name="id">Document Id</param>
        /// <returns></returns>
        [Authorize(UserRoles.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(long id)
        {
            return Ok(await Mediator.Send(new DeleteDocumentByIdCommand { Id = id }));
        }

    }
}