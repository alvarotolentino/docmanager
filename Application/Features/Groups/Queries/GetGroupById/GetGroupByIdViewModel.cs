using System;

namespace Application.Features.Groups.Queries.GetGroupById
{
    public class GetGroupByIdViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual string CreatedBy { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual string UpdatedBy { get; set; }
        public virtual DateTime UpdatedAt { get; set; }
    }
}