using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities
{
    // This can easily be modified to be BaseEntity<T> and public T Id to support different key types.
    // Using non-generic integer types for simplicity and to ease caching logic
    public interface IBaseEntity
    {
        int Id { get; set; }
        EntityState EntityState { get; set; }
    }

    // This can easily be modified to be BaseEntity<T> and public T Id to support different key types.
    // Using non-generic integer types for simplicity and to ease caching logic
   
}
