using System.Collections.Generic;
using Domain.Common;

namespace Domain.Entities
{
    public class Group: AuditableBaseEntity
    {
        public string Name { get; set; }
        public List<UserGroup> UserGroups { get; set; }
    }
}