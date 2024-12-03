using EPYSLTEXCore.Infrastructure.Entities;

namespace EPYSLTEX.Core.Interfaces.Repositories
{
    public interface IRepository<T> where T : IBaseEntity
    {
        T GetById(int id);
        T GetSingleBySpec(ISpecification<T> spec);
        bool Exists(int id);
        int Count();
        int Count(ISpecification<T> spec);
        IEnumerable<T> ListAll();
        IEnumerable<T> ListAll(int page, int pageSize);
        IEnumerable<T> List(ISpecification<T> spec);
        IEnumerable<T> List(ISpecification<T> spec, int page, int pageSize);
        T Add(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
