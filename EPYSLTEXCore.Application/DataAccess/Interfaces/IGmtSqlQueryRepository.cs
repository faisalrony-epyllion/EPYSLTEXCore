using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace EPYSLTEX.Core.Interfaces.Repositories
{
    public interface IGmtSqlQueryRepository<T> where T : class
    {
        List<T> GetData(string query, params object[] parameters);

        Task<List<T>> GetDataAsync(string query, params object[] parameters);

        List<CT> GetData<CT>(string query, params object[] parameters) where CT : class;

        Task<List<CT>> GetDataAsync<CT>(string query, params object[] parameters) where CT : class;

        List<dynamic> GetDynamicData(string query, params SqlParameter[] parameters);

        List<dynamic> GetDynamicData(string query, bool IsSP, params SqlParameter[] parameters);

        string GetStringValue(string query);

        Task<string> GetStringValueAsync(string query);

        int GetIntData(string query);

        decimal GetDecimalData(string query);

        int RunSqlCommand(string query, bool transactionRequired, params object[] parameters);

        Task<int> RunSqlCommandAsync(string query, params object[] parameters);

        Task<dynamic> GetDynamicDataDapperAsync(string query);

        Task<dynamic> GetDynamicDataDapperAsync(string query, object param);

        Task<List<T>> GetDataDapperAsync(string query);

        Task<List<T>> GetDataDapperAsync(string query, object param);

        Task<List<CT>> GetDataDapperAsync<CT>(string query) where CT : class;

        Task<List<CT>> GetDataDapperAsync<CT>(string query, object param) where CT : class;

        Task<CT> GetFirstOrDefaultDapperAsync<CT>(string query) where CT : class;

        Task<T> GetFirstOrDefaultDapperAsync(string query, object param);

        Task<CT> GetFirstOrDefaultDapperAsync<CT>(string query, object param) where CT : class;
    }
}
