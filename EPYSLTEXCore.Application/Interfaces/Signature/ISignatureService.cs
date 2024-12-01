using EPYSLTEXCore.Infrastructure.Static;

namespace EPYSLTEXCore.Application.Interfaces
{
    /// <summary>
    /// ISignatureService
    /// </summary>
    public interface ISignatureService 
    {
        /// <summary>
        /// Get Max Id Async
        /// </summary>
        /// <param name="field">Field or Table name</param>
        /// <param name="repeatAfter"><see cref="RepeatAfterEnum"/> Repeat After</param>
        /// <returns></returns>
        Task<int> GetMaxIdAsync(string field, RepeatAfterEnum repeatAfter = RepeatAfterEnum.NoRepeat);

        /// <summary>
        /// Get Max Id Async
        /// </summary>
        /// <param name="field">Field or Table name</param>
        /// <param name="increment">Increment</param>
        /// <param name="repeatAfter"><see cref="RepeatAfterEnum"/>Repeat After</param>
        /// <returns></returns>
        Task<int> GetMaxIdAsync(string field, int increment, RepeatAfterEnum repeatAfter = RepeatAfterEnum.NoRepeat);
        Task<int> GetMaxNoAsync(string field, int companyId = 1, RepeatAfterEnum repeatAfter = RepeatAfterEnum.NoRepeat, string padWith = "00000");
        //Task<Signatures> GetSignatureAsync(string field, int companyId, int siteId, RepeatAfterEnum repeatAfter);
    }
}

