using System;

namespace Application.Features.Groups.Queries.GetGroups
{
    public class GetGroupsViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public virtual long CreatedBy { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual long UpdatedBy { get; set; }
        public virtual DateTime UpdatedAt { get; set; }
    }
}