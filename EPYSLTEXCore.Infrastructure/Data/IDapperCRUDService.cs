using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json.Nodes;

namespace EPYSLTEXCore.Infrastructure.Data
{
    public interface IDapperCRUDService<T> where T : class, IDapperBaseEntity
    {
        SqlConnection Connection { get; set; }
        public int UserCode { get; set; }
        SqlConnection GetConnection(string connectionName = AppConstants.DB_CONNECTION);

        Task<SqlConnection> OpenTexConnectionAsync();

        Task<SqlConnection> OpenGmtConnectionAsync();

        Task<List<dynamic>> GetDynamicDataAsync(string query);

        Task<List<dynamic>> GetDynamicDataAsync(string query, SqlConnection connection);

        Task<List<dynamic>> GetDynamicDataAsync(string query, object param);

        Task<List<dynamic>> GetDynamicDataAsync(string query, SqlConnection connection, object param);
        Task<List<dynamic>> GetDynamicDataAsync(string query, SqlConnection connection, object param, CommandType commandType = CommandType.StoredProcedure);

        Task<int> GetSingleIntFieldAsync(string query);

        Task<int> GetSingleIntFieldAsync(string query, SqlConnection connection);

        Task<string> GetSingleStringFieldAsync(string query);

        Task<string> GetSingleStringFieldAsync(string query, SqlConnection connection);

        Task<bool> GetSingleBooleanFieldAsync(string query);

        Task<bool> GetSingleBooleanFieldAsync(string query, SqlConnection connection);
        Task OpenAsync();
        Task<dynamic> GetFirstOrDefaultDynamicDataAsync(string query);

        Task<dynamic> GetFirstOrDefaultDynamicDataAsync(string query, SqlConnection connection);

        Task<dynamic> GetFirstOrDefaultDynamicDataAsync(string query, object param);

        Task<dynamic> GetFirstOrDefaultDynamicDataAsync(string query, SqlConnection connection, object param);

        Task<List<T>> GetDataAsync(string query);

        Task<List<T>> GetDataAsync(string query, SqlConnection connection);

        Task<List<T>> GetDataAsync(string query, object param);

        Task<List<T>> GetDataAsync(string query, SqlConnection connection, object param);

        Task<List<CT>> GetDataAsync<CT>(string query) where CT : class;

        Task<List<CT>> GetDataAsync<CT>(string query, SqlConnection connection) where CT : class;

        Task<List<CT>> GetDataAsync<CT>(string query, object param) where CT : class;

        Task<List<CT>> GetDataAsync<CT>(string query, SqlConnection connection, object param) where CT : class;
        Task QueryMultipleAsync(string sql);
        Task<T> GetFirstOrDefaultAsync(string query);

        Task<T> GetFirstOrDefaultAsync(string query, SqlConnection connection);

        Task<CT> GetFirstOrDefaultAsync<CT>(string query) where CT : class;

        Task<CT> GetFirstOrDefaultAsync<CT>(string query, SqlConnection connection) where CT : class;

        Task<T> GetFirstOrDefaultAsync(string query, object param);

        Task<T> GetFirstOrDefaultAsync(string query, SqlConnection connection, object param);

        Task<CT> GetFirstOrDefaultAsync<CT>(string query, object param) where CT : class;

        Task<CT> GetFirstOrDefaultAsync<CT>(string query, SqlConnection connection, object param) where CT : class;

        Task SaveSingleAsync(T entity, SqlTransaction transaction);

        Task SaveSingleAsync(T entity, SqlConnection connection, SqlTransaction transaction);
        Task SaveSingleAsync<CT>(CT entity, SqlTransaction transaction) where CT : class, IDapperBaseEntity;

        Task SaveSingleAsync<CT>(CT entity, SqlConnection connection, SqlTransaction transaction) where CT : class, IDapperBaseEntity;

        Task SaveAsync(IEnumerable<T> entities, SqlTransaction transaction);
        Task SaveAsync(IEnumerable<T> entities, SqlConnection connection, SqlTransaction transaction);

        Task SaveAsync<CT>(IEnumerable<CT> entities, SqlTransaction transaction) where CT : class, IDapperBaseEntity;

        Task SaveAsync<CT>(IEnumerable<CT> entities, SqlConnection connection, SqlTransaction transaction) where CT : class, IDapperBaseEntity;

       // Task ValidationAsync<CT>(IEnumerable<CT> entities, SqlTransaction transaction, string validationStoreProcedureName, int primaryKeyValue, int userId, EntityState entityState) where CT : class, IDapperBaseEntity;


        Task<int> ExecuteAsync(string query, object param, int commandTimeOut = 30, CommandType commandType = CommandType.Text);

        int ExecuteWithTransactionAsync(string query, ref SqlTransaction transaction, object param = null, int commandTimeOut = 30, CommandType commandType = CommandType.Text);

        Task<List<CT>> QueryMultipleAsync<CT>(string query, SqlConnection connection, object param) where CT : class;
        int QueryMultipleAsync(string query, ref SqlTransaction transaction, object param = null, int commandTimeOut = 30, CommandType commandType = CommandType.Text);
        Task<T> SaveEntityAsync(T entity);
        Task<bool> DeleteEntityAsync(T entity, string keyValue);
        Task<T> SaveEntityCompositKeyAsync(T entity);
        Task<bool> DeleteEntityCompositKeyAsync(T entity);
        Task SaveNestedEntityAsync(Object T, IDbTransaction transaction = null);
        // Task SaveNestedEntityAsync(T entity, IDbTransaction transaction = null);
        // Task SaveNestedEntityAsync<T>(T entity, IDbTransaction transaction);

        Task<int> AddDynamicObjectAsync(string tableName, JsonObject dataObject, SqlConnection connection, IDbTransaction transaction = null);
        Task<int> DeleteDynamicObjectAsync(string tableName, JsonObject dataObject, List<string> primaryKeyColumns, SqlConnection connection, IDbTransaction transaction = null);
        Task<int> UpdateDynamicObjectAsync(string tableName, JsonObject dataObject, List<string> primaryKeyColumns, SqlConnection connection, IDbTransaction transaction = null);
        Task DeleteNestedEntityAsync(T entity, IDbTransaction transaction = null);
        #region signature Methods
        Task<int> GetMaxIdAsync(string field, RepeatAfterEnum repeatAfter = RepeatAfterEnum.NoRepeat, SqlTransaction transaction = null, SqlConnection connectionGmt = null);
        Task<int> GetMaxIdAsync(string field, int increment, RepeatAfterEnum repeatAfter = RepeatAfterEnum.NoRepeat, SqlTransaction transaction = null, SqlConnection connectionGmt = null);
        int GetMaxId(string field, int increment, RepeatAfterEnum repeatAfter = RepeatAfterEnum.NoRepeat, SqlTransaction transaction = null, SqlConnection connectionGmt = null);
        Task<IEnumerable<T>> AddManyAsync(IEnumerable<T> entities, string tableName);
        Task<string> GetMaxNoAsync(string field, int companyId = 1, RepeatAfterEnum repeatAfter = RepeatAfterEnum.NoRepeat, string padWith = "00000", SqlTransaction transaction = null, SqlConnection connectionGmt = null);
        Task<int> GetMaxNoAsync(string tableName, string columnName, string replacedValue, int length, SqlConnection connectionGmt = null);
        #endregion
        int RunSqlCommand(string query, bool transactionRequired, object parameters = null);
        Task<int> GetUniqueCodeWithoutSignatureAsync(IDbConnection connection, IDbTransaction transaction, string tableName, string fieldName);
        Task<int> GetUniqueCodeWithoutSignatureAsync(IDbConnection connection, IDbTransaction transaction, string tableName, string fieldName, string preFix);

        Task<Signatures> GetSignatureAsync(string field, int companyId, int siteId, RepeatAfterEnum repeatAfter = RepeatAfterEnum.NoRepeat);
        Task<int> AddUpDateDeleteDynamicObjectAsync(string tableName, object dataObject, List<string> primaryKeyColumns, SqlConnection connection, IDbTransaction transaction = null);
        Task<T> FindAsync<T>(string tableName, string columnName, object value);
        Task<bool> ExistsAsync(string tableName, string columnName1, object value1, string columnName2, object value2);
        Task AddAsync<T>(T entity, string tableName, bool isPrimaryKeyUpdated = false);
        void Add<T>(T entity, string tableName, bool isPrimaryKeyUpdated = false);
        string GetInsertQuery<T>(T entity, string tableName, SqlTransaction transaction = null, SqlConnection connectionGmt = null);
       
    }
}
