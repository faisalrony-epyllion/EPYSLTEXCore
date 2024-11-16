using Dapper;
using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Static;
using Microsoft.Extensions.Configuration;
using System.Collections;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;

namespace EPYSLTEXCore.Infrastructure.Data
{
    public class DapperCRUDService<T> : IDapperCRUDService<T> where T : class, IDapperBaseEntity
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        public SqlConnection Connection { get; set; }
        //private readonly ISignatureRepository _signatureRepository;

        public DapperCRUDService(IConfiguration configuration)
        {
            this._configuration = configuration;
            this._connectionString = this._configuration.GetConnectionString("DBConnection");
            //_signatureRepository = signatureRepository;
            Connection = new SqlConnection(this._connectionString);
        }

        public SqlConnection GetConnection(string connectionName = AppConstants.DB_CONNECTION)
        {
            return new SqlConnection(this._connectionString);
        }

        public async Task<SqlConnection> OpenTexConnectionAsync()
        {
            var connection = new SqlConnection(this._connectionString);
            await connection.OpenAsync();
            return connection;
        }

        public async Task<SqlConnection> OpenGmtConnectionAsync()
        {
            var connection = new SqlConnection(this._connectionString);
            await connection.OpenAsync();
            return connection;
        }

        public async Task<List<dynamic>> GetDynamicDataAsync(string query)
        {
            try
            {
                await Connection.OpenAsync();
                var records = await Connection.QueryAsync(query);
                return records.AsList();
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                Connection.Close();
            }
        }

        public async Task<List<dynamic>> GetDynamicDataAsync(string query, SqlConnection connection)
        {
            try
            {
                await connection.OpenAsync();
                var records = await connection.QueryAsync(query);
                return records.AsList();
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }

        public async Task<List<dynamic>> GetDynamicDataAsync(string query, object param)
        {
            try
            {
                await Connection.OpenAsync();
                var records = await Connection.QueryAsync<dynamic>(query, param);
                return records.AsList();
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                Connection.Close();
            }
        }

        public async Task<List<dynamic>> GetDynamicDataAsync(string query, SqlConnection connection, object param)
        {
            try
            {
                await connection.OpenAsync();
                var records = await connection.QueryAsync<dynamic>(query, param);
                return records.AsList();
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }

        public async Task<dynamic> GetFirstOrDefaultDynamicDataAsync(string query)
        {
            try
            {
                await Connection.OpenAsync();
                return await Connection.QueryFirstOrDefaultAsync(query);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                Connection.Close();
            }
        }

        public async Task<dynamic> GetFirstOrDefaultDynamicDataAsync(string query, SqlConnection connection)
        {
            try
            {
                await connection.OpenAsync();
                return await connection.QueryFirstOrDefaultAsync(query);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }

        public async Task<dynamic> GetFirstOrDefaultDynamicDataAsync(string query, object param)
        {
            try
            {
                await Connection.OpenAsync();
                var records = await Connection.QueryFirstOrDefaultAsync(query, param);
                return records.ToList();
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                Connection.Close();
            }
        }

        public async Task<dynamic> GetFirstOrDefaultDynamicDataAsync(string query, SqlConnection connection, object param)
        {
            try
            {
                await connection.OpenAsync();
                var records = await connection.QueryFirstOrDefaultAsync(query, param);
                return records.ToList();
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }

        public async Task<List<T>> GetDataAsync(string query)
        {
            try
            {
                await Connection.OpenAsync();
                var records = await Connection.QueryAsync<T>(query);
                return records.ToList();
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                Connection.Close();
            }
        }

        public async Task<List<T>> GetDataAsync(string query, SqlConnection connection)
        {
            try
            {
                await connection.OpenAsync();
                var records = await connection.QueryAsync<T>(query);
                return records.ToList();
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }

        public async Task<List<T>> GetDataAsync(string query, object param)
        {
            try
            {
                await Connection.OpenAsync();
                var records = await Connection.QueryAsync<T>(query, param);
                return records.ToList();
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                Connection.Close();
            }
        }

        public async Task<List<T>> GetDataAsync(string query, SqlConnection connection, object param)
        {
            try
            {
                await connection.OpenAsync();
                var records = await connection.QueryAsync<T>(query, param);
                return records.ToList();
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }

        public async Task<List<CT>> GetDataAsync<CT>(string query) where CT : class
        {
            try
            {
                await Connection.OpenAsync();
                var records = await Connection.QueryAsync<CT>(query);
                return records.ToList();
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                Connection.Close();
            }
        }

        public async Task<List<CT>> GetDataAsync<CT>(string query, SqlConnection connection) where CT : class
        {
            try
            {
                await connection.OpenAsync();
                var records = await connection.QueryAsync<CT>(query);
                return records.ToList();
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }

        public async Task<List<CT>> GetDataAsync<CT>(string query, object param) where CT : class
        {
            try
            {
                await Connection.OpenAsync();
                var records = await Connection.QueryAsync<CT>(query, param);
                return records.ToList();
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                Connection.Close();
            }
        }

        public async Task<List<CT>> GetDataAsync<CT>(string query, SqlConnection connection, object param) where CT : class
        {
            try
            {
                await connection.OpenAsync();
                var records = await connection.QueryAsync<CT>(query, param);
                return records.ToList();
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }

        public async Task<int> GetSingleIntFieldAsync(string query)
        {
            try
            {
                await Connection.OpenAsync();
                return await Connection.QueryFirstOrDefaultAsync<int>(query);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                Connection.Close();
            }
        }

        public async Task<int> GetSingleIntFieldAsync(string query, SqlConnection connection)
        {
            try
            {
                await connection.OpenAsync();
                return await connection.QueryFirstOrDefaultAsync<int>(query);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }

        public async Task<string> GetSingleStringFieldAsync(string query)
        {
            try
            {
                await Connection.OpenAsync();
                return await Connection.QueryFirstOrDefaultAsync<string>(query);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                Connection.Close();
            }
        }

        public async Task<string> GetSingleStringFieldAsync(string query, SqlConnection connection)
        {
            try
            {
                await connection.OpenAsync();
                return await connection.QueryFirstOrDefaultAsync<string>(query);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }

        public async Task<bool> GetSingleBooleanFieldAsync(string query)
        {
            try
            {
                await Connection.OpenAsync();
                return await Connection.QueryFirstOrDefaultAsync<bool>(query);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                Connection.Close();
            }
        }

        public async Task<bool> GetSingleBooleanFieldAsync(string query, SqlConnection connection)
        {
            try
            {
                await connection.OpenAsync();
                return await connection.QueryFirstOrDefaultAsync<bool>(query);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }

        public async Task<T> GetFirstOrDefaultAsync(string query)
        {
            try
            {
                await Connection.OpenAsync();
                return await Connection.QueryFirstOrDefaultAsync<T>(query);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                Connection.Close();
            }
        }

        public async Task<T> GetFirstOrDefaultAsync(string query, SqlConnection connection)
        {
            try
            {
                await connection.OpenAsync();
                return await connection.QueryFirstOrDefaultAsync<T>(query);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }

        public async Task<CT> GetFirstOrDefaultAsync<CT>(string query) where CT : class
        {
            try
            {
                await Connection.OpenAsync();
                return await Connection.QueryFirstOrDefaultAsync<CT>(query);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                Connection.Close();
            }
        }

        public async Task<CT> GetFirstOrDefaultAsync<CT>(string query, SqlConnection connection) where CT : class
        {
            try
            {
                await connection.OpenAsync();
                return await connection.QueryFirstOrDefaultAsync<CT>(query);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }

        public async Task<T> GetFirstOrDefaultAsync(string query, object param)
        {
            try
            {
                await Connection.OpenAsync();
                return await Connection.QueryFirstOrDefaultAsync<T>(query, param);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                Connection.Close();
            }
        }

        public async Task<T> GetFirstOrDefaultAsync(string query, SqlConnection connection, object param)
        {
            try
            {
                await connection.OpenAsync();
                return await connection.QueryFirstOrDefaultAsync<T>(query, param);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }

        public async Task<CT> GetFirstOrDefaultAsync<CT>(string query, object param) where CT : class
        {
            try
            {
                await Connection.OpenAsync();
                return await Connection.QueryFirstOrDefaultAsync<CT>(query, param);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                Connection.Close();
            }
        }

        public async Task<CT> GetFirstOrDefaultAsync<CT>(string query, SqlConnection connection, object param) where CT : class
        {
            try
            {
                await connection.OpenAsync();
                return await connection.QueryFirstOrDefaultAsync<CT>(query, param);
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                connection.Close();
            }
        }

        public async Task SaveSingleAsync(T entity, SqlTransaction transaction)
        {
            switch (entity.EntityState)
            {
                case EntityState.Added:
                    await Connection.InsertAsync(entity, transaction);
                    break;
                case EntityState.Deleted:
                    await Connection.DeleteAsync(entity, transaction);
                    break;
                case EntityState.Modified:
                    await Connection.UpdateAsync(entity, transaction);
                    break;
                default:
                    break;
            }
        }

        public async Task SaveSingleAsync(T entity, SqlConnection connection, SqlTransaction transaction)
        {
            switch (entity.EntityState)
            {
                case EntityState.Added:
                    await connection.InsertAsync(entity, transaction);
                    break;
                case EntityState.Deleted:
                    await connection.DeleteAsync(entity, transaction);
                    break;
                case EntityState.Modified:
                    await connection.UpdateAsync(entity, transaction);
                    break;
                default:
                    break;
            }
        }

        public async Task SaveSingleAsync<CT>(CT entity, SqlTransaction transaction) where CT : class, IDapperBaseEntity
        {
            switch (entity.EntityState)
            {
                case EntityState.Added:
                    await Connection.InsertAsync(entity, transaction);
                    break;
                case EntityState.Deleted:
                    await Connection.DeleteAsync(entity, transaction);
                    break;
                case EntityState.Modified:
                    await Connection.UpdateAsync(entity, transaction);
                    break;
                default:
                    break;
            }
        }

        public async Task SaveSingleAsync<CT>(CT entity, SqlConnection connection, SqlTransaction transaction) where CT : class, IDapperBaseEntity
        {
            switch (entity.EntityState)
            {
                case EntityState.Added:
                    await connection.InsertAsync(entity, transaction);
                    break;
                case EntityState.Deleted:
                    await connection.DeleteAsync(entity, transaction);
                    break;
                case EntityState.Modified:
                    await connection.UpdateAsync(entity, transaction);
                    break;
                default:
                    break;
            }
        }

        public async Task SaveAsync(IEnumerable<T> entities, SqlTransaction transaction)
        {
            var addList = entities.Where(x => x.EntityState == EntityState.Added);
            if (addList.Any()) await Connection.InsertAsync(addList, transaction);

            var updateList = entities.Where(x => x.EntityState == EntityState.Modified);
            if (updateList.Any()) await Connection.UpdateAsync(updateList, transaction);

            var deleteList = entities.Where(x => x.EntityState == EntityState.Deleted);
            if (deleteList.Any()) await Connection.DeleteAsync(deleteList, transaction);
        }

        public async Task SaveAsync(IEnumerable<T> entities, SqlConnection connection, SqlTransaction transaction)
        {
            var addList = entities.Where(x => x.EntityState == EntityState.Added);
            if (addList.Any()) await connection.InsertAsync(addList, transaction);

            var updateList = entities.Where(x => x.EntityState == EntityState.Modified);
            if (updateList.Any()) await connection.UpdateAsync(updateList, transaction);

            var deleteList = entities.Where(x => x.EntityState == EntityState.Deleted);
            if (deleteList.Any()) await connection.DeleteAsync(deleteList, transaction);
        }

        public async Task SaveAsync<CT>(IEnumerable<CT> entities, SqlTransaction transaction) where CT : class, IDapperBaseEntity
        {
            var addList = entities.Where(x => x.EntityState == EntityState.Added);
            if (addList.Any()) await Connection.InsertAsync(addList, transaction);

            var updateList = entities.Where(x => x.EntityState == EntityState.Modified);
            if (updateList.Any()) await Connection.UpdateAsync(updateList, transaction);

            var deleteList = entities.Where(x => x.EntityState == EntityState.Deleted);
            if (deleteList.Any()) await Connection.DeleteAsync(deleteList, transaction);
        }

        public async Task SaveAsync<CT>(IEnumerable<CT> entities, SqlConnection connection, SqlTransaction transaction) where CT : class, IDapperBaseEntity
        {
            var addList = entities.Where(x => x.EntityState == EntityState.Added);
            if (addList.Any()) await connection.InsertAsync(addList, transaction);

            var updateList = entities.Where(x => x.EntityState == EntityState.Modified);
            if (updateList.Any()) await connection.UpdateAsync(updateList, transaction);

            var deleteList = entities.Where(x => x.EntityState == EntityState.Deleted);
            if (deleteList.Any()) await connection.DeleteAsync(deleteList, transaction);
        }

        public async Task<int> ExecuteAsync(string query, object param, int commandTimeOut = 30, CommandType commandType = CommandType.Text)
        {
            SqlTransaction transaction = null;

            try
            {
                await Connection.OpenAsync();
                transaction = Connection.BeginTransaction();
                int rows = await Connection.ExecuteAsync(query, param, transaction, commandTimeOut, commandType);
                transaction.Commit();
                return rows;
            }
            catch (System.Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                throw ex;
            }
            finally
            {
                transaction.Dispose();
                Connection.Close();
            }
        }

        public int ExecuteWithTransactionAsync(string query, ref SqlTransaction transaction, object param = null, int commandTimeOut = 30, CommandType commandType = CommandType.Text)
        {
            //SqlTransaction transaction = null;

            try
            {
                //await Connection.OpenAsync();
                //transaction = Connection.BeginTransaction();
                int rows = Connection.Execute(query, param, transaction, commandTimeOut, commandType);
                //transaction.Commit();
                return rows;
            }
            catch (System.Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                throw ex;
            }

        }

        public Task OpenAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task QueryMultipleAsync(string sql)
        {
            throw new System.NotImplementedException();
        }

        public Task<List<CT>> QueryMultipleAsync<CT>(string query, SqlConnection connection, object param) where CT : class
        {
            throw new System.NotImplementedException();
        }

        public int QueryMultipleAsync(string query, ref SqlTransaction transaction, object param = null, int commandTimeOut = 30, CommandType commandType = CommandType.Text)
        {
            throw new System.NotImplementedException();
        }

        public async Task<T> SaveEntityAsync(T entity)
        {
            SqlTransaction transaction = null;
            try
            {
                await Connection.OpenAsync();
                transaction = Connection.BeginTransaction();

                string tableName = EntityReflectionHelper.GetTableName<T>();
                string keyColumnName = EntityReflectionHelper.GetKeyColumnName<T>();

                if (tableName == null || keyColumnName == null)
                {
                    throw new InvalidOperationException("Table name or key column could not be determined for the entity.");
                }

                // Use reflection to get the value of the key property
                var keyValue = typeof(T).GetProperty(keyColumnName)?.GetValue(entity, null);

                // Check if entity exists
                string selectQuery = $"SELECT * FROM {tableName} WHERE {keyColumnName} = @Key";
                var existingEntity = await Connection.QueryFirstOrDefaultAsync<T>(selectQuery, new { Key = keyValue }, transaction);

                if (existingEntity == null)
                {
                    // If the entity does not exist, set it to be added
                    entity.EntityState = System.Data.Entity.EntityState.Added;
                }
                else
                {
                    // If the entity exists, set it to be modified
                    entity.EntityState = System.Data.Entity.EntityState.Modified;

                    // Update the existing entity's properties with the new values
                    var properties = EntityReflectionHelper.GetColumnNames<T>();
                    foreach (var property in properties)
                    {
                        var newValue = typeof(T).GetProperty(property)?.GetValue(entity, null);
                        typeof(T).GetProperty(property)?.SetValue(existingEntity, newValue);
                    }
                }

                // Save the entity using the CRUD service
                await SaveSingleAsync(entity, transaction);
                transaction.Commit();

                return entity;
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                throw ex;
            }
            finally
            {
                Connection.Close();
            }
        }

        public async Task<bool> DeleteEntityAsync(T entity, string keyValue)
        {
            SqlTransaction transaction = null;
            try
            {
                await Connection.OpenAsync();
                transaction = Connection.BeginTransaction();

                string tableName = EntityReflectionHelper.GetTableName<T>();
                string keyColumnName = EntityReflectionHelper.GetKeyColumnName<T>();

                if (tableName == null || keyColumnName == null)
                {
                    throw new InvalidOperationException("Table name or key column could not be determined for the entity.");
                }

                // Use reflection to get the value of the key property
                //var keyValue = typeof(T).GetProperty(keyColumnName)?.GetValue(entity, null);

                if (keyValue == null)
                {
                    throw new ArgumentException("Entity key value cannot be null.");
                }

                // Create the DELETE SQL statement
                string deleteQuery = $"DELETE FROM {tableName} WHERE {keyColumnName} = @Key";

                // Execute the DELETE query
                int rowsAffected = await Connection.ExecuteAsync(deleteQuery, new { Key = keyValue }, transaction);

                transaction.Commit();

                // Return true if a row was deleted, otherwise false
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                throw ex;
            }
            finally
            {
                Connection.Close();
            }
        }

        public async Task<T> SaveEntityCompositKeyAsync(T entity)
        {
            SqlTransaction transaction = null;
            try
            {
                await Connection.OpenAsync();
                transaction = Connection.BeginTransaction();

                string tableName = EntityReflectionHelper.GetTableName<T>();
                List<string> keyColumnNames = EntityReflectionHelper.GetMultipleKeyColumnName<T>();

                if (tableName == null || keyColumnNames == null || !keyColumnNames.Any())
                {
                    throw new InvalidOperationException("Table name or key columns could not be determined for the entity.");
                }

                // Use reflection to get the values of the key properties
                var keyValues = keyColumnNames.Select(key => new
                {
                    ColumnName = key,
                    Value = typeof(T).GetProperty(key)?.GetValue(entity, null)
                }).ToDictionary(k => k.ColumnName, k => k.Value);

                // Construct the WHERE clause for multiple keys
                string whereClause = string.Join(" AND ", keyValues.Select(kv => $"{kv.Key} = @{kv.Key}"));

                // Check if the entity exists
                string selectQuery = $"SELECT * FROM {tableName} WHERE {whereClause}";
                var existingEntity = await Connection.QueryFirstOrDefaultAsync<T>(selectQuery, keyValues, transaction);

                if (existingEntity == null)
                {
                    // If the entity does not exist, set it to be added
                    entity.EntityState = System.Data.Entity.EntityState.Added;
                }
                else
                {
                    // If the entity exists, set it to be modified
                    entity.EntityState = System.Data.Entity.EntityState.Modified;

                    // Update the existing entity's properties with the new values
                    var properties = EntityReflectionHelper.GetColumnNames<T>();
                    foreach (var property in properties)
                    {
                        var newValue = typeof(T).GetProperty(property)?.GetValue(entity, null);
                        typeof(T).GetProperty(property)?.SetValue(existingEntity, newValue);
                    }
                }

                // Save the entity using the CRUD service
                await SaveSingleAsync(entity, transaction);
                transaction.Commit();

                return entity;
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                throw ex;
            }
            finally
            {
                Connection.Close();
            }
        }

        public async Task<bool> DeleteEntityCompositKeyAsync(T entity)
        {
            SqlTransaction transaction = null;
            try
            {
                await Connection.OpenAsync();
                transaction = Connection.BeginTransaction();

                string tableName = EntityReflectionHelper.GetTableName<T>();
                List<string> keyColumnNames = EntityReflectionHelper.GetMultipleKeyColumnName<T>();

                if (tableName == null || keyColumnNames == null || !keyColumnNames.Any())
                {
                    throw new InvalidOperationException("Table name or key columns could not be determined for the entity.");
                }

                // Use reflection to get the values of the key properties
                var keyValues = keyColumnNames.Select(key => new
                {
                    ColumnName = key,
                    Value = typeof(T).GetProperty(key)?.GetValue(entity, null)
                }).ToDictionary(k => k.ColumnName, k => k.Value);

                // Ensure all key values are provided
                if (keyValues.Any(kv => kv.Value == null))
                {
                    throw new ArgumentException("Entity key values cannot be null.");
                }

                // Construct the WHERE clause for multiple keys
                string whereClause = string.Join(" AND ", keyValues.Select(kv => $"{kv.Key} = @{kv.Key}"));

                // Create the DELETE SQL statement
                string deleteQuery = $"DELETE FROM {tableName} WHERE {whereClause}";

                // Execute the DELETE query
                int rowsAffected = await Connection.ExecuteAsync(deleteQuery, keyValues, transaction);

                transaction.Commit();

                // Return true if a row was deleted, otherwise false
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                throw ex;
            }
            finally
            {
                Connection.Close();
            }
        }

        public async Task SaveNestedEntityAsync(T entity, IDbTransaction transaction = null)
        {
            if (Connection.State != ConnectionState.Open)
            {
                await Connection.OpenAsync();
            }

            using (var transactionScope = transaction ?? Connection.BeginTransaction())
            {
                try
                {
                    string tableName = EntityReflectionHelper.GetTableName(entity.GetType());
                    string keyPropertyName = EntityReflectionHelper.GetKeyPropertyName(entity.GetType());

                    var keyValue = entity.GetType().GetProperty(keyPropertyName).GetValue(entity);
                    bool isNew = keyValue == null || Convert.ToInt64(keyValue) == 0;

                    if (isNew)
                    {
                        await Connection.InsertAsync(entity, transactionScope);
                    }
                    else
                    {
                        await Connection.UpdateAsync(entity, transactionScope);
                    }

                    foreach (var prop in entity.GetType().GetProperties())
                    {
                        if (EntityReflectionHelper.IsForeignKey(prop))
                        {
                            var childEntity = prop.GetValue(entity);
                            if (childEntity != null)
                            {
                                var foreignKeyProperty = childEntity.GetType().GetProperty(prop.Name);
                                foreignKeyProperty.SetValue(childEntity, keyValue);

                                await SaveNestedEntityAsync(childEntity as T, transactionScope);
                            }
                        }
                        else if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType.IsGenericType)
                        {
                            var childEntities = prop.GetValue(entity) as IEnumerable;
                            if (childEntities != null)
                            {
                                foreach (var childEntity in childEntities)
                                {
                                    var foreignKeyProperty = childEntity.GetType().GetProperty($"{entity.GetType().Name}Id");
                                    foreignKeyProperty.SetValue(childEntity, keyValue);

                                    await SaveNestedEntityAsync(childEntity as T, transactionScope);
                                }
                            }
                        }
                    }

                    transactionScope.Commit();
                }
                catch (Exception ex)
                {
                    transactionScope.Rollback();
                    throw ex;
                }
                finally
                {
                    if (transaction == null)
                    {
                        Connection.Close();
                    }
                }
            }
        }

        public async Task DeleteNestedEntityAsync(T entity, IDbTransaction transaction = null)
        {
            if (Connection.State != ConnectionState.Open)
            {
                await Connection.OpenAsync();
            }

            using (var transactionScope = transaction ?? Connection.BeginTransaction())
            {
                try
                {
                    string keyPropertyName = EntityReflectionHelper.GetKeyPropertyName(entity.GetType());
                    var keyValue = entity.GetType().GetProperty(keyPropertyName).GetValue(entity);

                    foreach (var prop in entity.GetType().GetProperties())
                    {
                        if (EntityReflectionHelper.IsForeignKey(prop))
                        {
                            var childEntity = prop.GetValue(entity);
                            if (childEntity != null)
                            {
                                await DeleteNestedEntityAsync(childEntity as T, transactionScope);
                            }
                        }
                        else if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType.IsGenericType)
                        {
                            var childEntities = prop.GetValue(entity) as IEnumerable;
                            if (childEntities != null)
                            {
                                foreach (var childEntity in childEntities)
                                {
                                    await DeleteNestedEntityAsync(childEntity as T, transactionScope);
                                }
                            }
                        }
                    }

                    await Connection.DeleteAsync(entity, transactionScope);

                    transactionScope.Commit();
                }
                catch (Exception ex)
                {
                    transactionScope.Rollback();
                    throw ex;
                }
                finally
                {
                    if (transaction == null)
                    {
                        Connection.Close();
                    }
                }
            }
        }


    }
}
