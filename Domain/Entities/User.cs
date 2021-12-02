using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities
{
    public class User : IdentityUser<int>
    {

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<RefreshToken> RefreshTokens { get; set; }

        public List<Group> Groups { get; set; }

        public List<Role> Roles { get; set; }

        public virtual int CreatedBy { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual int UpdatedBy { get; set; }
        public virtual DateTime UpdatedAt { get; set; }

        public bool OwnsToken(string token)
        {
            return this.RefreshTokens?.Find(x => x.Token == token) != null;
        }

    }
}