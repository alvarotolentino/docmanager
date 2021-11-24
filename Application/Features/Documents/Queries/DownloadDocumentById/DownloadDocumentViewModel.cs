namespace Application.Features.Documents.Queries.DownloadDocumentById
{
    public class DownloadDocumentViewModel
    {
        public byte[] Content { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
    }
}