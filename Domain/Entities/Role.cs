using System;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities
{
    public class Role : IdentityRole<long>
    {
        public virtual long CreatedBy { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual long UpdatedBy { get; set; }
        public virtual DateTime UpdatedAt { get; set; }
    }
}