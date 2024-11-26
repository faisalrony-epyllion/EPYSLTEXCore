namespace EPYSLTEXCore.Application.Interfaces
{
    /// <summary>
    /// Generic interface for basic CRUD operations.
    /// </summary>
    /// <typeparam name="T">The type of entity.</typeparam>
    public interface ICommonService<T> where T : class
    {
        /// <summary>
        /// Asynchronously adds a new entity to the repository.
        /// </summary>
        /// <param name="item">The entity to add.</param>
        /// <returns>The ID of the newly added entity.</returns>
        Task<T> AddAsync(T item);

        /// <summary>
        /// Asynchronously deletes an entity from the repository based on its ID.
        /// </summary>
        /// <param name="Id">The ID of the entity to delete.</param>
        /// <returns>True if deletion was successful, otherwise false.</returns>
        Task<string> DeleteAsync(string Id);

        /// <summary>
        /// Asynchronously retrieves an entity from the repository based on its ID.
        /// </summary>
        /// <param name="Id">The ID of the entity to retrieve.</param>
        /// <returns>The retrieved entity, or null if not found.</returns>
        Task<T> GetByIdAsync(string Id);

        /// <summary>
        /// Asynchronously retrieves all entities from the repository.
        /// </summary>
        /// <returns>A collection of all entities.</returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Asynchronously updates an existing entity in the repository based on its ID.
        /// </summary>
        /// <param name="Id">The ID of the entity to update.</param>
        /// <param name="item">The updated entity data.</param>
        /// <returns>True if update was successful, otherwise false.</returns>
        Task<bool> UpdateAsync(long Id, T item);


       
    }
}
