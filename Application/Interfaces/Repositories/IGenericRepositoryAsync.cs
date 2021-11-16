using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface IGenericRepositoryAsync<T> where T: class
    {
        
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
    }
}