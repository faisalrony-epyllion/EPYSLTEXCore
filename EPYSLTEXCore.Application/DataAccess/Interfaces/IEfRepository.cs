using EPYSLTEX.Core.DTOs;
using EPYSLTEXCore.Infrastructure.Entities;
using System.Linq.Expressions;

namespace EPYSLTEX.Core.Interfaces.Repositories
{
    public interface IEfRepository<T> where T: IBaseEntity
    {
        T Find(int id);
        T Find(ISpecification<T> spec);
        T Find(Expression<Func<T, bool>> criteria);
        bool Exists(int id);
        int Count();
        int GetMaxId();
        int Count(ISpecification<T> spec);
        IQueryable<T> ListAll();
        List<T> ListAll(int page, int pageSize);
        List<T> ListAll(int offset, int limit, FilterByExpressionModel filterByExpressionModel, string sort, string order, out int count);
        List<T> ListAll(Expression<Func<T, bool>> criteria, int offset, int limit, FilterByExpressionModel filterByExpressionModel, string sort, string order, out int count);
        List<T> List(ISpecification<T> spec);
        IQueryable<T> List(Expression<Func<T, bool>> criteria);
        List<T> List(ISpecification<T> spec, int page, int pageSize);
        T Add(T entity, string tableName);
        void Update(T entity);
        void Delete(T entity);

        Task<T> FindAsync(int id);
        Task<T> FindAsync(Expression<Func<T, bool>> criteria);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> criteria);
        Task<int> CountAsync();
        Task<List<T>> ListAllAsync();
        Task<List<T>> ListAllAsync(int page, int pageSize);
        Task<List<T>> ListAllAsync(int offset, int limit, FilterByExpressionModel filterByExpressionModel, string sort, string order);
        Task<List<T>> ListAsync(ISpecification<T> spec);
        Task<List<T>> ListAsync(Expression<Func<T, bool>> criteria);
        Task<List<T>> ListAsync(ISpecification<T> spec, int page, int pageSize);
        Task<List<T>> ListDescAsync(ISpecification<T> spec, int page, int pageSize);
        Task<T> AddAsync(T entity, string tableName);
        Task<IEnumerable<T>> AddManyAsync(IEnumerable<T> entities, string tableName);
        Task<IEnumerable<T>> AddManyAsync(IEnumerable<T> entities);
        Task UpdateAsync(T entity);
        Task UpdateManyAsync(IEnumerable<T> entities);
        Task DeleteAsync(T entity);
    }
}
