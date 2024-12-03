using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Interfaces;
using EPYSLTEXCore.Infrastructure.Statics;

namespace EPYSLTEXCore.Application.Services
{
    public class SignatureService: ISignatureService
    {
        private readonly IDapperCRUDService<Signatures> _dbContext;

        public SignatureService(IDapperCRUDService<Signatures> dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> GetMaxIdAsync(string field, RepeatAfterEnum repeatAfter = RepeatAfterEnum.NoRepeat)
        {
            var signature = await GetSignatureAsync(field, 1, 1, repeatAfter);

            if (signature == null)
            {
                signature = new Signatures
                {
                    Field = field,
                    Dates = DateTime.Today,
                    LastNumber = 1
                };
                await _dbContext.SaveEntityAsync(signature);
            }
            else
            {
                signature.LastNumber++;
                await _dbContext.SaveEntityAsync(signature);
            }

            return (int)signature.LastNumber;
        }

        public async Task<int> GetMaxIdAsync(string field, int increment, RepeatAfterEnum repeatAfter = RepeatAfterEnum.NoRepeat)
        {
            if (increment == 0) return 0;
            var signature = await GetSignatureAsync(field, 1, 1, repeatAfter);

            if (signature == null)
            {
                signature = new Signatures
                {
                    Field = field,
                    Dates = DateTime.Today,
                    LastNumber = increment
                };
                await _dbContext.SaveEntityAsync(signature);
            }
            else
            {
                signature.LastNumber += increment;
                await _dbContext.SaveEntityAsync(signature);
            }

            return (int)(signature.LastNumber - increment + 1);
        }

        public async Task<int> GetMaxNoAsync(string field, int companyId = 1, RepeatAfterEnum repeatAfter = RepeatAfterEnum.NoRepeat, string padWith = "00000")
        {
            var signature = await GetSignatureAsync(field, companyId, 1, repeatAfter);

            if (signature == null)
            {
                signature = new Signatures
                {
                    Field = field,
                    Dates = DateTime.Today,
                    CompanyId = companyId.ToString(),
                    LastNumber = 1
                };
                await _dbContext.SaveEntityAsync(signature);
            }
            else
            {
                signature.LastNumber++;
                await _dbContext.SaveEntityAsync(signature);
            }

            var datePart = DateTime.Now.ToString("yyMMdd");
            var numberPart = signature.LastNumber.ToString(padWith);
            return Convert.ToInt32($"{companyId}{datePart}{numberPart}");
        }

        private async Task<Signatures> GetSignatureAsync(string field, int companyId, int siteId, RepeatAfterEnum repeatAfter)
        {
            string query = @"SELECT TOP 1 * FROM Signature WHERE Field = @Field AND CompanyId = @CompanyId AND SiteId = @SiteId";
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

            return await _dbContext.GetFirstOrDefaultAsync<Signatures>(query, parameters);
        }

    }
}
