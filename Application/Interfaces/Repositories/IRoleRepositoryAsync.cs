using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Interfaces.Repositories
{
    public interface IRoleRepositoryAsync: IRoleStore<Role>
    {
         
    }
}