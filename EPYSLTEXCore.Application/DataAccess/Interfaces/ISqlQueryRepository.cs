using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace EPYSLTEX.Core.Interfaces.Repositories
{
    public interface ISqlQueryRepository<T> where T : class
    {
        List<T> GetData(string query, params object[] parameters);

        Task<List<T>> GetDataAsync(string query, params object[] parameters);

        List<CT> GetData<CT>(string query, params object[] parameters) where CT : class;

        Task<List<CT>> GetDataAsync<CT>(string query, params object[] parameters) where CT : class;

        int GetIntData(string query);

        decimal GetDecimalData(string query);

        int RunSqlCommand(string query, params object[] parameters);

        Task<int> RunSqlCommandAsync(string query, params object[] parameters);

        Task<List<dynamic>> GetDynamicDataDapperAsync(string query);

        Task<List<dynamic>> GetDynamicDataDapperAsync(string query, object param);

        Task<dynamic> GetFirstOrDefaultDynamicDataDapperAsync(string query);

        Task<dynamic> GetFirstOrDefaultDynamicDataDapperAsync(string query, object param);

        Task<List<T>> GetDataDapperAsync(string query);

        Task<List<T>> GetDataDapperAsync(string query, object param);

        Task<List<CT>> GetDataDapperAsync<CT>(string query) where CT : class;

        List<CT> GetDataDapper<CT>(string query) where CT : class;

        Task<List<CT>> GetDataDapperAsync<CT>(string query, object param) where CT : class;

        Task<T> GetFirstOrDefaultDapperAsync(string query);

        Task<T> GetFirstOrDefaultDapperAsync(string query, object param);

        Task<CT> GetFirstOrDefaultDapperAsync<CT>(string query) where CT : class;

        Task<CT> GetFirstOrDefaultDapperAsync<CT>(string query, object param) where CT : class;

        Task<int> ExecuteAsync(string query);

        Task<int> ExecuteAsync(string query, object param, CommandType commandType);
    }
}
