using System;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities
{
    public class Role : IdentityRole<int>
    {
        public virtual int CreatedBy { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual int UpdatedBy { get; set; }
        public virtual DateTime UpdatedAt { get; set; }
    }
}