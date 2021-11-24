using System;
using Domain.Common;

namespace Domain.Entities
{
    public class Documents: AuditableBaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string ContentType { get; set; }
        public long Length { get; set; }
        public byte[] Data { get; set; }

    }
}