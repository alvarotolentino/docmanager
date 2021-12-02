using System;

namespace Application.Features.Documents.Queries.GetDocumentById
{
    public class GetDocumentInfoViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string ContentType { get; set; }
        public string Length { get; set; }
        public virtual string CreatedBy { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual string UpdatedBy { get; set; }
        public virtual DateTime UpdatedAt { get; set; }

    }
}