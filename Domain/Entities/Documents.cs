using System;
using Domain.Common;

namespace Domain.Entities
{
    public class Documents: AuditableBaseEntity
    {
        public DateTime PostedAt { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string PathFile { get; set; }
        public byte[] Data { get; set; }
    }
}