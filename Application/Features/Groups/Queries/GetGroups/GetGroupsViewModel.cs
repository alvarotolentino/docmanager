using System;

namespace Application.Features.Groups.Queries.GetGroups
{
    public class GetGroupsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual int CreatedBy { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual int UpdatedBy { get; set; }
        public virtual DateTime UpdatedAt { get; set; }
    }
}