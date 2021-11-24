using System;

namespace Domain.Common
{
    public class AuditableBaseEntity
    {
        public virtual long Id { get; set; }
        public virtual long CreatedBy { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual long UpdatedBy { get; set; }
        public virtual DateTime UpdatedAt { get; set; }

    }
}