using Dapper;
using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.CustomeAttribute;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections;
using System.Data;
using System.Data.Entity;
using Microsoft.Data.SqlClient;
using System.Text.Json.Nodes;
using static Dapper.SqlMapper;
using System.Data.Common;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography.Xml;
using System.Linq.Expressions;
namespace EPYSLTEXCore.Infrastructure.Data
{
    public class DapperCRUDService<T> : IDapperCRUDService<T> where T : class, IDapperBaseEntity
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        public SqlConnection Connection { get; set; }
        public int UserCode { get; set; }

        public DapperCRUDService(IConfiguration configuration)
        {
            this._configuration = configuration;
            this._connectionString = this._configuration.GetConnectionString("GmtConnection");
            Connection = new SqlConnection(this._connectionString);
        }

        public SqlConnection GetConnection(string connectionName = AppConstants.DB_CONNECTION)
        {//This Method modified by Saif
            var connectionString = this._configuration.GetConnectionString(connectionName);
            return new SqlConnection(connectionString);
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

        public async Task<List<dynamic>> GetDynamicDataAsync(string query, SqlConnection connection, object param, CommandType commandType = CommandType.StoredProcedure)
        {
            try
            {
                await connection.OpenAsync();
                var records = await connection.QueryAsync<dynamic>(query, param, commandType: commandType);
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
                if (Connection.State != System.Data.ConnectionState.Open)
                {
                    await Connection.OpenAsync();
                }
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

                await SaveNestedEntityAsync(entity, transaction);

                transaction.Commit();
                return entity;
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                throw new Exception("An error occurred while saving the entity and its nested entities.", ex);
            }
            finally
            {
                await Connection.CloseAsync();
            }
        }


        public async Task SaveNestedEntityAsync(object entity, IDbTransaction transaction)

        {

            if (entity == null) throw new ArgumentNullException(nameof(entity));

            string tableName = EntityReflectionHelper.GetTableName(entity.GetType());

            string keyPropertyName = EntityReflectionHelper.GetKeyPropertyName(entity.GetType());

            if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(keyPropertyName))

            {

                throw new InvalidOperationException("Table name or key column could not be determined for the entity.");

            }

            var keyProperty = entity.GetType().GetProperty(keyPropertyName);

            var keyValue = keyProperty?.GetValue(entity);

            // Check if the entity exists

            string selectQuery = $"SELECT * FROM {tableName} WHERE {keyPropertyName} = @Key";

            var existingEntity = await Connection.QueryFirstOrDefaultAsync(selectQuery, new { Key = keyValue }, transaction);

            if (existingEntity == null)
            {

                // Generate primary key for new entity

                var result = await Connection.QueryAsync<dynamic>(
                    $"SELECT TOP 1 {keyPropertyName} AS ID FROM {tableName} ORDER BY {keyPropertyName} DESC",
                    transaction: transaction
                ).ConfigureAwait(false);

                //int newId = await _signatureService.GetMaxIdAsync(tableName);

                int newId = result.FirstOrDefault()?.ID ?? 0; // Default to 0 if no records exist

                //newId++; // Increment the ID for the new entity

                // Set the new ID value on the entity

                keyProperty?.SetValue(entity, newId);

                entity.GetType().GetProperty("AddedBy")?.SetValue(entity, UserCode);

                entity.GetType().GetProperty("DateAdded")?.SetValue(entity, DateTime.UtcNow);

                // Insert the new entity dynamically

                string insertQuery = GenerateInsertQuery(entity, tableName);
                await Connection.ExecuteAsync(insertQuery, entity, transaction);
                keyValue = newId; // Refresh keyValue for foreign key assignment
            }
            else
            {

                entity.GetType().GetProperty("UpdatedBy")?.SetValue(entity, UserCode);
                entity.GetType().GetProperty("DateUpdated")?.SetValue(entity, DateTime.UtcNow);

                // Update the existing entity dynamically
                string updateQuery = GenerateUpdateQuery(entity, tableName, keyPropertyName);
                await Connection.ExecuteAsync(updateQuery, entity, transaction);

            }

            // Handle child entities

            var childProperties = entity.GetType()
                .GetProperties()
                .Where(prop => prop.GetCustomAttributes(typeof(ChildEntityAttribute), true).Any())
                .ToList();

            foreach (var prop in childProperties)

            {
                if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType.IsGenericType)
                {
                    // Collection of child entities
                    var currentChildEntities = prop.GetValue(entity) as IEnumerable<object>;
                    var childEntityType = prop.PropertyType.GetGenericArguments().FirstOrDefault();
                    var childTableName = EntityReflectionHelper.GetTableName(childEntityType);
                    var foreignKeyName = EntityReflectionHelper.GetForeignKeyName(childEntityType, entity.GetType());

                    if (currentChildEntities != null)
                    {
                        // Step 1: Fetch existing child entities from the database
                        string selectChildQuery = $"SELECT * FROM {childTableName} WHERE {foreignKeyName} = @ParentKey";
                        var existingChildEntities = await Connection.QueryAsync<dynamic>(selectChildQuery, new { ParentKey = keyValue }, transaction);

                        // Step 2: Identify child entities to delete
                        var currentChildIds = currentChildEntities
                            .Select(child => (int)child.GetType().GetProperty(EntityReflectionHelper.GetKeyPropertyName(childEntityType)).GetValue(child))
                            .ToList();

                        //var existingChildIds = existingChildEntities.Select(e => (int)e[EntityReflectionHelper.GetKeyPropertyName(childEntityType)]).ToList();

                        //var idsToDelete = existingChildIds.Except(currentChildIds).ToList();

                        //foreach (var id in idsToDelete)
                        //{
                        //    string deleteQuery = $"DELETE FROM {childTableName} WHERE {EntityReflectionHelper.GetKeyPropertyName(childEntityType)} = @Id";
                        //    await Connection.ExecuteAsync(deleteQuery, new { Id = id }, transaction);
                        //}

                        // Step 3: Save or update current child entities
                        foreach (var childEntity in currentChildEntities)
                        {
                            var foreignKeyProperty = childEntity.GetType().GetProperty(foreignKeyName);
                            foreignKeyProperty?.SetValue(childEntity, keyValue);

                            await SaveNestedEntityAsync(childEntity, transaction);
                        }
                    }
                }


                else if (prop.PropertyType.IsGenericType)

                {

                    var childEntities = prop.GetValue(entity) as IEnumerable<object>;

                    if (childEntities != null)

                    {

                        foreach (var childEntity in childEntities)

                        {

                            var foreignKeyProperty = childEntity.GetType().GetProperties()

                                .FirstOrDefault(p => p.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.Schema.ForeignKeyAttribute), true)

                                                      .Cast<System.ComponentModel.DataAnnotations.Schema.ForeignKeyAttribute>()

                                                      .Any(fk => fk.Name == keyPropertyName));

                            if (foreignKeyProperty != null)

                            {

                                foreignKeyProperty.SetValue(childEntity, keyValue);

                            }

                            await SaveNestedEntityAsync(childEntity, transaction);

                        }

                    }

                }

            }

        }

        private string GenerateInsertQuery(object entity, string tableName)

        {

            var properties = entity.GetType().GetProperties()

                .Where(p => p.GetValue(entity) != null && !Attribute.IsDefined(p, typeof(WriteAttribute)))

                .Select(p => p.Name);

            var columns = string.Join(", ", properties);

            var values = string.Join(", ", properties.Select(p => $"@{p}"));

            return $"INSERT INTO {tableName} ({columns}) VALUES ({values})";

        }

        private string GenerateUpdateQuery(object entity, string tableName, string keyPropertyName)

        {

            var properties = entity.GetType().GetProperties()

                .Where(p => p.GetValue(entity) != null && p.Name != keyPropertyName && !Attribute.IsDefined(p, typeof(WriteAttribute)))

                .Select(p => $"{p.Name} = @{p.Name}");

            var updates = string.Join(", ", properties);

            return $"UPDATE {tableName} SET {updates} WHERE {keyPropertyName} = @{keyPropertyName}";

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

                    //NEED TO WORK 

                    // get max number of primary key value from table
                    //var result = await Connection.queryasync<dynamic>(
                    //    $"select top 1 {keycolumnname} as id from {tableName} order by {keycolumnname} desc",
                    //    transaction: transaction
                    //).configureawait(false);

                    // int id = result.firstordefault()?.id ?? 0; // default to 0 if no records exist

                    // // increment the id to assign the next value
                    // id++;

                    // // set max number to primary key property
                    // typeof(t).getproperty(keycolumnname)?.setvalue(entity, id);
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

        #region signature Methods

        public async Task<int> GetMaxIdAsync(string field, RepeatAfterEnum repeatAfter = RepeatAfterEnum.NoRepeat, SqlTransaction transaction = null, SqlConnection connectionGmt = null)
        {
            var signature = await GetSignatureAsync(field, 1, 1, repeatAfter, transaction, connectionGmt);

            if (signature == null)
            {
                signature = new Signatures
                {
                    Field = field,
                    Dates = DateTime.Today,
                    LastNumber = 1
                };
                await connectionGmt.InsertAsync(signature, transaction);
            }
            else
            {
                signature.LastNumber++;
                signature.EntityState = EntityState.Modified;
                await connectionGmt.UpdateAsync(signature, transaction);
            }

            return (int)signature.LastNumber;
        }

        public async Task<int> GetMaxIdAsync(string field, int increment, RepeatAfterEnum repeatAfter = RepeatAfterEnum.NoRepeat, SqlTransaction transaction = null, SqlConnection connectionGmt = null)
        {
            if (increment == 0) return 0;
            var signature = await GetSignatureAsync(field, 1, 1, repeatAfter, transaction, connectionGmt);

            if (signature == null)
            {
                signature = new Signatures
                {
                    Field = field,
                    Dates = DateTime.Today,
                    LastNumber = increment
                };
                await connectionGmt.InsertAsync(signature, transaction);
            }
            else
            {
                signature.LastNumber += increment;
                signature.EntityState = EntityState.Modified;
                await connectionGmt.UpdateAsync(signature, transaction);
            }
            return Convert.ToInt32(signature.LastNumber - increment + 1);
        }
        public int GetMaxId(string field, int increment, RepeatAfterEnum repeatAfter = RepeatAfterEnum.NoRepeat, SqlTransaction transaction = null, SqlConnection connectionGmt = null)
        {
            //if (increment == 0) return 0;
            var signature = GetSignature(field, 1, 1, repeatAfter, transaction, connectionGmt);
            //var signature = await GetSignatureCmdAsync(field, 1, 1, repeatAfter);

            if (signature == null)
            {
                signature = new Signatures
                {
                    Field = field,
                    Dates = DateTime.Today,
                    LastNumber = increment
                };
                if (connectionGmt.IsNotNull()) connectionGmt.InsertAsync(signature, transaction);
                else Connection.InsertAsync(signature, transaction);
            }
            else
            {
                signature.LastNumber += increment;

                if (connectionGmt.IsNotNull()) connectionGmt.UpdateAsync(signature, transaction);
                else Connection.UpdateAsync(signature, transaction);

            }

            return Convert.ToInt32(signature.LastNumber - increment + 1);
        }
        public int GetMaxId(string field, RepeatAfterEnum repeatAfter = RepeatAfterEnum.NoRepeat, SqlTransaction transaction = null, SqlConnection connectionGmt = null)
        {
            if (connectionGmt.IsNull()) connectionGmt = Connection;

            var signature = GetSignature(field, 1, 1, repeatAfter, transaction, connectionGmt);
            //var signature = await GetSignatureCmdAsync(field, 1, 1, repeatAfter);

            if (signature == null)
            {
                signature = new Signatures
                {
                    Field = field,
                    Dates = DateTime.Today,
                    LastNumber = 1
                };
                connectionGmt.InsertAsync(signature, transaction);
            }
            else
            {
                signature.LastNumber++;
                connectionGmt.UpdateAsync(signature, transaction);
            }

            return Convert.ToInt32(signature.LastNumber);
        }
        private Signatures GetSignature(string field, int companyId, int siteId, RepeatAfterEnum repeatAfter, SqlTransaction transaction = null, SqlConnection connectionGmt = null)
        {
            string query = $@"SELECT TOP 1 * FROM {DbNames.EPYSL}..Signature WHERE Field = @Field AND CompanyId = @CompanyId AND SiteId = @SiteId";
            var parameters = new
            {
                Field = field,
                CompanyId = companyId.ToString(),
                SiteId = siteId.ToString()
            };

            switch (repeatAfter)
            {
                case RepeatAfterEnum.EveryYear:
                    query += " AND YEAR(Dates) = YEAR(GETDATE())";
                    break;
                case RepeatAfterEnum.EveryMonth:
                    query += " AND MONTH(Dates) = MONTH(GETDATE()) AND YEAR(Dates) = YEAR(GETDATE())";
                    break;
                case RepeatAfterEnum.EveryDay:
                    query += " AND CAST(Dates AS DATE) = CAST(GETDATE() AS DATE)";
                    break;
            }

            try
            {
                if (connectionGmt.IsNull()) connectionGmt = Connection;

                if (connectionGmt.State == System.Data.ConnectionState.Closed)
                {
                    connectionGmt.Open();
                }
                var records = connectionGmt.QueryFirstOrDefault(query, parameters, transaction);
                string jsonString = JsonConvert.SerializeObject(records);
                Signatures signature = JsonConvert.DeserializeObject<Signatures>(jsonString);
                //return records.ToList();
                return signature;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                //Connection.Close();
            }
        }
        private async Task<Signatures> GetSignatureAsync(string field, int companyId, int siteId, RepeatAfterEnum repeatAfter, SqlTransaction transaction = null, SqlConnection connectionGmt = null)
        {
            string query = $@"SELECT TOP 1 * FROM {DbNames.EPYSL}..Signature WHERE Field = @Field AND CompanyId = @CompanyId AND SiteId = @SiteId";
            var parameters = new
            {
                Field = field,
                CompanyId = companyId.ToString(),
                SiteId = siteId.ToString()
            };

            switch (repeatAfter)
            {
                case RepeatAfterEnum.EveryYear:
                    query += " AND YEAR(Dates) = YEAR(GETDATE())";
                    break;
                case RepeatAfterEnum.EveryMonth:
                    query += " AND MONTH(Dates) = MONTH(GETDATE()) AND YEAR(Dates) = YEAR(GETDATE())";
                    break;
                case RepeatAfterEnum.EveryDay:
                    query += " AND CAST(Dates AS DATE) = CAST(GETDATE() AS DATE)";
                    break;
            }

            try
            {
                Signatures signature = null;
                if (connectionGmt.IsNull()) connectionGmt = Connection;
                if (connectionGmt.State == System.Data.ConnectionState.Closed)
                {
                    await connectionGmt.OpenAsync();
                }
                var records = await connectionGmt.QueryFirstOrDefaultAsync(query, parameters, transaction);
                string jsonString = JsonConvert.SerializeObject(records);
                signature = JsonConvert.DeserializeObject<Signatures>(jsonString);
                return signature;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Signatures> GetSignatureAsync(string field, int companyId, int siteId, RepeatAfterEnum repeatAfter = RepeatAfterEnum.NoRepeat)
        {
            // Initialize the base query
            string query = @"SELECT TOP 1 * FROM Signature 
                     WHERE Field = @Field 
                     AND CompanyId = @CompanyId 
                     AND SiteId = @SiteId";

            // Set parameters
            var parameters = new
            {
                Field = field,
                CompanyId = companyId,  // Pass as integer (assuming CompanyId is integer)
                SiteId = siteId         // Pass as integer (assuming SiteId is integer)
            };

            // Add conditions based on RepeatAfterEnum
            switch (repeatAfter)
            {
                case RepeatAfterEnum.EveryYear:
                    query += " AND YEAR(Dates) = YEAR(GETDATE())";
                    break;
                case RepeatAfterEnum.EveryMonth:
                    query += " AND MONTH(Dates) = MONTH(GETDATE()) AND YEAR(Dates) = YEAR(GETDATE())";
                    break;
                case RepeatAfterEnum.EveryDay:
                    query += " AND CAST(Dates AS DATE) = CAST(GETDATE() AS DATE)";
                    break;
            }

            try
            {
                // Execute the query asynchronously and retrieve a single record
                var records = await Connection.QueryAsync<Signatures>(query, parameters);

                // Check if the query returned a result and return it
                return records?.FirstOrDefault();  // Assuming records is a list, return the first one
            }
            catch (Exception ex)
            {
                // Log or rethrow the exception with more context if needed
                throw new ApplicationException("Error fetching signature.", ex);
            }
        }


        private async Task<Signatures> GetSignatureCmdAsync(string field, int companyId, int siteId, RepeatAfterEnum repeatAfter)
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            SqlTransaction transaction = null;
            #region Query
            string query = $@"SELECT TOP 1 * FROM Signature WHERE Field = '{field.ToString()}' AND CompanyId = '{companyId}' AND SiteId = '{siteId.ToString()}'";

            switch (repeatAfter)
            {
                case RepeatAfterEnum.EveryYear:
                    query += " AND YEAR(Dates) = YEAR(GETDATE())";
                    break;
                case RepeatAfterEnum.EveryMonth:
                    query += " AND MONTH(Dates) = MONTH(GETDATE()) AND YEAR(Dates) = YEAR(GETDATE())";
                    break;
                case RepeatAfterEnum.EveryDay:
                    query += " AND CAST(Dates AS DATE) = CAST(GETDATE() AS DATE)";
                    break;
            }
            #endregion
            try
            {
                connection.Open();

                // Begin a local transaction
                transaction = connection.BeginTransaction();

                // Create a command and associate it with the transaction
                SqlCommand command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = query;  // Replace with your SQL query

                // Execute the command using BeginExecuteReader (asynchronously)
                var asyncResult = command.BeginExecuteReader();
                Signatures signatures = new Signatures();
                // Wait for the command to finish execution (if needed)
                using (SqlDataReader reader = command.EndExecuteReader(asyncResult))
                {
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            signatures.Field = reader[0].ToString();
                            signatures.Dates = Convert.ToDateTime(reader[1].ToString());
                            signatures.LastNumber = Convert.ToDecimal(reader[2].ToString());
                            signatures.CompanyID = reader[3].ToString();
                            signatures.SiteID = reader[4].ToString();
                        }
                    }
                }

                // Commit the transaction if everything is fine
                transaction.Commit();
                return signatures;
            }
            catch (Exception ex)
            {
                // Rollback the transaction if an error occurs
                if (transaction != null)
                {
                    transaction.Rollback();
                }
                return new Signatures();
            }
            finally
            {
                // Ensure the connection is closed properly
                if (connection.State == System.Data.ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }



        #endregion

        #region Dynamic Table Save
        // Get Column Names from Schema
        private async Task<List<string>> GetColumnNamesAsync(string tableName, SqlConnection connection, IDbTransaction transaction = null)
        {
            var sql = @"
            SELECT COLUMN_NAME 
            FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = @TableName;";
            var result = (await connection.QueryAsync<string>(sql, new { TableName = tableName }, transaction)).ToList();
            return result;
        }

        private List<string> GetColumnNames(string tableName, SqlConnection connection, IDbTransaction transaction = null)
        {
            var sql = @"
            SELECT COLUMN_NAME 
            FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = @TableName;";

            // Synchronous execution
            var result = connection.Query<string>(sql, new { TableName = tableName }, transaction).ToList();
            return result;
        }





        public async Task<int> AddUpDateDeleteDynamicObjectAsync(string tableName, object dataObject, List<string> primaryKeyColumns, SqlConnection connection, IDbTransaction transaction = null)
        {
            if (dataObject is IEnumerable<object> dataList)
            {
                foreach (var item in dataList)
                {
                    var jObject = item as JsonObject;
                    if (jObject != null)
                    {
                        await ProcessStatusAsync(tableName, jObject, primaryKeyColumns, connection, transaction);
                    }
                }
            }
            else
            {
                var jObject = dataObject as JsonObject;
                if (jObject != null)
                {
                    await ProcessStatusAsync(tableName, jObject, primaryKeyColumns, connection, transaction);
                }
            }

            return 0;
        }

        private async Task ProcessStatusAsync(string tableName, JsonObject jObject, List<string> primaryKeyColumns, SqlConnection connection, IDbTransaction transaction = null)
        {
            var objStatus = jObject[StatusConstants.STATUS]?.ToString();
            if (objStatus != null)
            {
                switch (objStatus.ToLower())
                {
                    case StatusConstants.ADD:
                        await AddDynamicObjectAsync(tableName, jObject, connection, transaction);
                        break;
                    case StatusConstants.UPDATE:
                        await UpdateDynamicObjectAsync(tableName, jObject, primaryKeyColumns, connection, transaction);
                        break;
                    case StatusConstants.DELETE:
                        await DeleteDynamicObjectAsync(tableName, jObject, primaryKeyColumns, connection, transaction);
                        break;
                }
            }
        }



        public async Task<int> AddDynamicObjectAsync(string tableName, JsonObject dataObject, SqlConnection connection, IDbTransaction transaction = null)
        {


            try
            {
                var columns = await GetColumnNamesAsync(tableName, connection, transaction);
                var columnNames = "";
                var parameters = "";
                var sql = "";

                var data = dataObject
                 .Where(property => columns.Contains(property.Key))
                 .ToDictionary(property => property.Key, property => ConvertJsonNodeToType<object>(property.Value));


                // Ensure there are valid columns in the data
                if (!data.Any())
                {
                    throw new ArgumentException("The object does not contain any matching columns for the specified table.");
                }

                // Add default columns if they exist in the table
                if (columns.Contains("AddedBy"))
                {
                    data["AddedBy"] = UserCode;
                }
                if (columns.Contains("DateAdded"))
                {
                    data["DateAdded"] = DateTime.UtcNow;
                }
                columnNames = string.Join(", ", data.Select(p => p.Key));
                parameters = string.Join(", ", data.Select(p => "@" + p.Key));
                sql = $"INSERT INTO {tableName} ({columnNames}) VALUES ({parameters});";
                return await connection.ExecuteAsync(sql, data, transaction);
            }
            catch (ArgumentException argEx)
            {
                // Handle argument exceptions (e.g., missing matching columns)
                throw new ArgumentException("Invalid argument encountered.", argEx);
            }
            catch (Exception ex)
            {
                // Catch any other general exceptions
                throw new ApplicationException("An unexpected error occurred.", ex);
            }
        }






        // General method to convert JsonNode to a specific type (object, string, int, etc.)
        public static T ConvertJsonNodeToType<T>(JsonNode jsonNode)
        {
            if (jsonNode == null)
            {
                throw new InvalidCastException("JsonNode is null and cannot be converted.");
            }

            // Handle each specific type conversion
            if (jsonNode is JsonValue jsonValue)
            {
                // Convert JsonValue to the appropriate type based on T
                if (typeof(T) == typeof(string))
                {
                    return (T)(object)jsonValue.GetValue<string>();
                }
                else if (typeof(T) == typeof(int))
                {
                    if (int.TryParse(jsonValue.GetValue<string>(), out var result))
                    {
                        return (T)(object)result;
                    }
                    else
                    {
                        throw new InvalidCastException($"Cannot convert {jsonValue} to type {typeof(T)}");
                    }
                }
                else if (typeof(T) == typeof(decimal))
                {
                    if (decimal.TryParse(jsonValue.GetValue<string>(), out var result))
                    {
                        return (T)(object)result;
                    }
                    else
                    {
                        throw new InvalidCastException($"Cannot convert {jsonValue} to type {typeof(T)}");
                    }
                }
                else if (typeof(T) == typeof(bool))
                {
                    if (bool.TryParse(jsonValue.GetValue<string>(), out var result))
                    {
                        return (T)(object)result;
                    }
                    else
                    {
                        throw new InvalidCastException($"Cannot convert {jsonValue} to type {typeof(T)}");
                    }
                }
                else if (typeof(T) == typeof(DateTime))
                {
                    if (DateTime.TryParse(jsonValue.GetValue<string>(), out var result))
                    {
                        return (T)(object)result;
                    }
                    else
                    {
                        throw new InvalidCastException($"Cannot convert {jsonValue} to type {typeof(T)}");
                    }
                }
                else
                {
                    // If the type is not one of the known types, try to return the string representation
                    return (T)(object)jsonValue.ToString();
                }
            }

            throw new InvalidCastException($"Cannot convert JsonNode of type {jsonNode.GetType()} to type {typeof(T)}");
        }

        public async Task<int> UpdateDynamicObjectAsync(string tableName, JsonObject dataObject, List<string> primaryKeyColumns, SqlConnection connection, IDbTransaction transaction = null)
        {
            var columns = await GetColumnNamesAsync(tableName, connection, transaction);

            try
            {

                if (columns.Contains("UpdatedBy"))
                {
                    dataObject["UpdatedBy"] = UserCode;
                }
                if (columns.Contains("DateUpdated"))
                {
                    dataObject["DateUpdated"] = DateTime.UtcNow.ToString("yyyy-MM-dd");

                }

                var data = dataObject
                 .Where(property => columns.Contains(property.Key))
                 .ToDictionary(property => property.Key, property => ConvertJsonNodeToType<object>(property.Value));

                if (!data.Any())
                {
                    throw new ArgumentException("The object does not contain any matching columns for the specified table.");
                }

                // Separate key columns and update columns

                var keyData = primaryKeyColumns
                    .ToDictionary(pk => pk, pk => data.ContainsKey(pk) ? data[pk] : throw new ArgumentException($"Primary key '{pk}' is missing in the object."));

                var updateColumns = data.Keys
                    .Except(primaryKeyColumns, StringComparer.OrdinalIgnoreCase)
                    .Select(col => $"{col} = @{col}");

                var setClause = string.Join(", ", updateColumns);
                var whereClause = string.Join(" AND ", primaryKeyColumns.Select(pk => $"{pk} = @{pk}"));

                var sql = $"UPDATE {tableName} SET {setClause} WHERE {whereClause};";

                return await connection.ExecuteAsync(sql, data, transaction);
            }
            catch (ArgumentException argEx)
            {
                // Handle argument exceptions (e.g., missing matching columns)
                throw new ArgumentException("Invalid argument encountered.", argEx);
            }
            catch (Exception ex)
            {
                // Catch any other general exceptions
                throw new ApplicationException("An unexpected error occurred.", ex);
            }
        }




        public async Task<int> DeleteDynamicObjectAsync(string tableName, JsonObject dataObject, List<string> primaryKeyColumns, SqlConnection connection, IDbTransaction transaction = null)
        {
            try
            {
                var columns = await GetColumnNamesAsync(tableName, connection, transaction);

                // Use reflection to extract properties and their values
                var data = dataObject
                 .Where(property => columns.Contains(property.Key))
                 .ToDictionary(property => property.Key, property => ConvertJsonNodeToType<object>(property.Value));


                if (!data.Any())
                {
                    throw new ArgumentException("The object does not contain any matching columns for the specified table.");
                }

                // Ensure primary key values exist in the object
                foreach (var pk in primaryKeyColumns)
                {
                    if (!data.ContainsKey(pk))
                    {
                        throw new ArgumentException($"Primary key '{pk}' is missing in the object.");
                    }
                }

                var whereClause = string.Join(" AND ", primaryKeyColumns.Select(pk => $"{pk} = @{pk}"));
                var sql = $"DELETE FROM {tableName} WHERE {whereClause};";

                return await connection.ExecuteAsync(sql, data, transaction);
            }
            catch (ArgumentException argEx)
            {
                // Handle argument exceptions (e.g., missing matching columns)
                throw new ArgumentException("Invalid argument encountered.", argEx);
            }
            catch (Exception ex)
            {
                // Catch any other general exceptions
                throw new ApplicationException("An unexpected error occurred.", ex);
            }
        }

        #endregion

        #region Table Max Number without signature table
        public async Task<int> GetUniqueCodeWithoutSignatureAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            string tableName,
            string fieldName)
        {
            // Call the main method with an empty prefix
            return await GetUniqueCodeWithoutSignatureAsync(connection, transaction, tableName, fieldName, "");
        }

        public async Task<int> GetUniqueCodeWithoutSignatureAsync(
            IDbConnection connection,
            IDbTransaction transaction,
            string tableName,
            string fieldName,
            string prefix)
        {
            try
            {
                // Define stored procedure name and parameters
                string storedProcedure = "spGetIDWithoutSignature";

                // Execute stored procedure
                var result = await connection.QueryAsync<int>(
                    storedProcedure,
                    new { TableName = tableName, FieldName = fieldName, Prefix = prefix },
                    transaction,
                    commandType: CommandType.StoredProcedure);

                // Retrieve and return the unique code
                return result.FirstOrDefault(); // Returns 0 if no rows are found
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error while fetching unique code.", ex);
            }
        }
        #endregion

        #region Check validtion by SP
        public async Task<string> GetMaxNoAsync(string field, int companyId = 1, RepeatAfterEnum repeatAfter = RepeatAfterEnum.NoRepeat, string padWith = "00000", SqlTransaction transaction = null, SqlConnection connectionGmt = null)
        {
            var signature = await GetSignatureAsync(field, companyId, 1, repeatAfter, transaction, connectionGmt);

            if (signature == null)
            {
                signature = new Signatures
                {
                    Field = field,
                    Dates = DateTime.Today,
                    CompanyID = companyId.ToString(),
                    LastNumber = 1
                };
                await connectionGmt.InsertAsync(signature, transaction);
            }
            else
            {
                signature.LastNumber++;
                await connectionGmt.UpdateAsync(signature, transaction);
            }

            //await _dbContext.SaveChangesAsync();

            var datePart = DateTime.Now.ToString("yyMMdd");
            var numberPart = signature.LastNumber.ToString(padWith);
            var comId = companyId.ToString("00");
            var maxNo = $@"{comId}{datePart}{numberPart}";

            return maxNo;
        }
        #endregion


        public async Task<IEnumerable<T>> AddManyAsync(IEnumerable<T> entities, string tableName)
        {

            var transaction = Connection.BeginTransaction();
            try
            {

                var maxId = await GetMaxIdAsync(tableName, entities.Count());

                // Prepare insert query
                var insertQuery = $"INSERT INTO {tableName} ({string.Join(",", typeof(T).GetProperties().Select(p => p.Name))}) " +
                                  $"VALUES ({string.Join(",", typeof(T).GetProperties().Select(p => "@" + p.Name))})";

                // Assign new IDs and insert entities
                foreach (var entity in entities)
                {
                    typeof(T).GetProperty("Id")?.SetValue(entity, ++maxId); // Set new ID
                    await Connection.ExecuteAsync(insertQuery, entity, transaction: transaction);
                }

                transaction.Commit();
                return entities;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw; // Rethrow exception for caller to handle
            }


        }

        public int RunSqlCommand(string query, bool transactionRequired, object parameters = null)
        {

            //Connection.Open();

            if (transactionRequired)
            {
                using (var transaction = Connection.BeginTransaction())
                {
                    try
                    {
                        int result = Connection.Execute(query, parameters, transaction: transaction);
                        transaction.Commit();
                        return result;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            else
            {
                try
                {
                    return Connection.Execute(query, parameters);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        public async Task<T> FindAsync<T>(string tableName, string columnName, object value)
        {
            var query = $"SELECT * FROM {tableName} WHERE {columnName} = @Value";
            var parameters = new { Value = value };

            using (var connection = Connection)
            {
                var res = await connection.QueryFirstOrDefaultAsync<T>(query, parameters);
                return res;
            }
        }
        public async Task<bool> ExistsAsync(string tableName, string columnName1, object value1, string columnName2, object value2)
        {
            var query = $"SELECT 1 FROM {tableName} WHERE {columnName1} = @Value1 AND {columnName2} = @Value2";
            var parameters = new { Value1 = value1, Value2 = value2 };

            using (var connection = Connection) // Replace _dbConnection with your actual connection object
            {
                var result = await connection.QueryFirstOrDefaultAsync<int?>(query, parameters);
                return result.HasValue;
            }
        }

        
        public async Task AddAsync<T>(T entity, string tableName, bool isPrimaryKeyUpdated = false)
        {
            //var query = GenerateInsertQuery(entity, tableName);

            //using (var connection = Connection) // Replace _dbConnection with your actual Dapper connection object
            //{
            //    await connection.ExecuteAsync(query, entity);
            //}

            ///////////

            using var connection = GetConnection(AppConstants.GMT_CONNECTION);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();
            try
            {
                // Retrieve the maximum ID
                var maxId = await GetMaxIdAsync(tableName, RepeatAfterEnum.NoRepeat, transaction, connection);

                var columns = await GetColumnNamesAsync(tableName, connection, transaction);

                // Generate insert query


                var properties = typeof(T).GetProperties()
                          .Where(p => columns.Select(c => c.ToLower())
                                             .Contains(p.Name.ToLower()));
                //var properties = typeof(T).GetProperties().Where(p => p.CanRead && p.CanWrite);
                var columnNames = string.Join(",", properties.Select(p => p.Name));
                var parameterNames = string.Join(",", properties.Select(p => "@" + p.Name));

                var insertQuery = $"INSERT INTO {tableName} ({columnNames}) VALUES ({parameterNames})";

                if (isPrimaryKeyUpdated == false)
                {
                    // Assign new ID
                    //var idProperty = typeof(T).GetProperty("Id");
                    var idProperty = typeof(T).GetProperties()
                              .FirstOrDefault(p => Attribute.IsDefined(p, typeof(ExplicitKeyAttribute)));

                    if (idProperty != null)
                    {
                        idProperty.SetValue(entity, ++maxId);
                    }
                }
                // Execute insert query
                await connection.ExecuteAsync(insertQuery, entity, transaction: transaction);

                // Commit transaction
                transaction.Commit();
            }
            catch
            {
                // Rollback transaction on error
                transaction.Rollback();
                throw; // Propagate the exception
            }
            finally
            {
                // Ensure transaction and connection are disposed
                connection.Close();
            }
        }

        public void Add<T>(T entity, string tableName, bool isPrimaryKeyUpdated = false)
        {
            using var connectionT = GetConnection(AppConstants.GMT_CONNECTION);
            connectionT.Open();

            using var transactionT = connectionT.BeginTransaction();
            try
            {
                // Retrieve the maximum ID (synchronous)
                var maxId = GetMaxId(tableName, RepeatAfterEnum.NoRepeat, transactionT, connectionT);

                // Retrieve column names (synchronous)
                var columns = GetColumnNames(tableName, connectionT, transactionT);

                // Generate insert query
                var properties = typeof(T).GetProperties()
                              .Where(p => columns.Select(c => c.ToLower())
                                                 .Contains(p.Name.ToLower()));

                var columnNames = string.Join(",", properties.Select(p => p.Name));
                var parameterNames = string.Join(",", properties.Select(p => "@" + p.Name));

                var insertQuery = $"INSERT INTO {tableName} ({columnNames}) VALUES ({parameterNames})";

                if (!isPrimaryKeyUpdated)
                {
                    // Assign new ID
                    var idProperty = typeof(T).GetProperties()
                                  .FirstOrDefault(p => Attribute.IsDefined(p, typeof(ExplicitKeyAttribute)));

                    if (idProperty != null)
                    {
                        idProperty.SetValue(entity, ++maxId);
                    }
                }

                // Execute insert query (synchronous)
                connectionT.Execute(insertQuery, entity, transaction: transactionT);

                // Commit transaction
                transactionT.Commit();
            }
            catch
            {
                // Rollback transaction on error
                transactionT.Rollback();
                throw; // Propagate the exception
            }
            finally
            {
                // Ensure transaction and connection are disposed
                connectionT.Close();
            }
        }
        public string GetInsertQuery<T>(T entity, string tableName, SqlTransaction transaction = null, SqlConnection connectionGmt = null)
        {
            try
            {
                // Retrieve column names (synchronous)
                var columns = GetColumnNames(tableName, connectionGmt, transaction);

                // Generate insert query
                var properties = typeof(T).GetProperties()
                              .Where(p => columns.Select(c => c.ToLower())
                                                 .Contains(p.Name.ToLower()));

                var columnNames = string.Join(",", properties.Select(p => p.Name));
                // Get the property values
                var values = properties
                    .Select(p =>
                    {
                        var value = p.GetValue(entity);
                        return value == null ? "NULL" :
                               value is string || value is DateTime ? $"'{value.ToString().Replace("'", "''")}'" :
                               value.ToString();
                    });

                var valueString = string.Join(",", values);

                // Generate the query
                var insertQuery = $"INSERT INTO {tableName} ({columnNames}) VALUES ({valueString})";


                return insertQuery; // Return the generated query
            }
            catch
            {
                throw; // Propagate the exception
            }
        }
        private string GenerateInsertQuery<T>(T entity, string tableName)
        {
            var properties = typeof(T).GetProperties().Where(p => p.CanRead && p.GetValue(entity) != null).ToList();
            var columnNames = string.Join(", ", properties.Select(p => p.Name));
            var columnValues = string.Join(", ", properties.Select(p => "@" + p.Name));

            return $"INSERT INTO {tableName} ({columnNames}) VALUES ({columnValues})";
        }

    }
}
