namespace Application.Features.Documents.Queries.GetAllDocuments
{
    public class GetAllDocumentsViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string PathFile { get; set; }
        public string ContentType { get; set; }
        public string Length { get; set; }
    }
}