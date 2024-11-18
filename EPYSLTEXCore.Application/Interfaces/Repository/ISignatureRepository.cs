using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Static;
using System.Threading.Tasks;

namespace EPYSLTEX.Core.Interfaces.Repositories
{
    /// <summary>
    /// ISignatureRepository
    /// </summary>
    public interface ISignatureRepository
    {
        /// <summary>
        /// Get Max Id
        /// </summary>
        /// <param name="field">Field or Table name</param>
        /// <param name="repeatAfter"><see cref="RepeatAfterEnum"/> Repeat After</param>
        /// <returns></returns>
        int GetMaxId(string field, RepeatAfterEnum repeatAfter = RepeatAfterEnum.NoRepeat);

        /// <summary>
        /// Get Max Id Async
        /// </summary>
        /// <param name="field">Field or Table name</param>
        /// <param name="repeatAfter"><see cref="RepeatAfterEnum"/> Repeat After</param>
        /// <returns></returns>
        Task<int> GetMaxIdAsync(string field, RepeatAfterEnum repeatAfter = RepeatAfterEnum.NoRepeat);

        /// <summary>
        /// Get Max Id
        /// </summary>
        /// <param name="field">Field or Table name</param>
        /// <param name="increment">Increment</param>
        /// <param name="repeatAfter"><see cref="RepeatAfterEnum"/>Repeat After</param>
        /// <returns></returns>
        int GetMaxId(string field, int increment, RepeatAfterEnum repeatAfter = RepeatAfterEnum.NoRepeat);

        /// <summary>
        /// Get Max Id Async
        /// </summary>
        /// <param name="field">Field or Table name</param>
        /// <param name="increment">Increment</param>
        /// <param name="repeatAfter"><see cref="RepeatAfterEnum"/>Repeat After</param>
        /// <returns></returns>
        Task<int> GetMaxIdAsync(string field, int increment, RepeatAfterEnum repeatAfter = RepeatAfterEnum.NoRepeat);

        string GetMaxNo(string field, int companyId = 1, RepeatAfterEnum repeatAfter = RepeatAfterEnum.NoRepeat, string padWith = "00000");
        string GetDCGPMaxNo(string field, System.DateTime dtDate, string companyCode, string prefix="15", int companyId = 1,  RepeatAfterEnum repeatAfter = RepeatAfterEnum.NoRepeat, string padWith = "00000");
       
        Task<string> GetMaxNoAsync(string field, int companyId = 1, RepeatAfterEnum repeatAfter = RepeatAfterEnum.NoRepeat, string padWith = "00000");
        Task<int> GetMaxNoAsync(string tableName, string columnName, string replaceValue);
        Task<int> GetMaxNoAsync(string tableName, string columnName, string replaceValue, int length);
    }
}
