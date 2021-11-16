using System.Threading.Tasks;
using Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence.Context;

namespace Infrastructure.Persistence.Repositories
{
    public class GenericRepositoryAsync<T>: IGenericRepositoryAsync<T> where T: class
    {
        private readonly ApplicationDbContext _dbContext;

        public GenericRepositoryAsync(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<T> AddAsync(T entity)
        {
            await _dbContext.Set<T>().AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(T entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

    }
}