using System;

namespace Domain.Common
{
    public class AuditableBaseEntity
    {
        public virtual int Id { get; set; }
        public virtual string CreatedBy { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual string UpdatedBy { get; set; }
        public virtual DateTime UpdatedAt { get; set; }

    }
}