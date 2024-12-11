using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.SCD
{
    [Table(TableNames.ImportDocumentAcceptanceChargeDetails)]
    public class ImportDocumentAcceptanceChargeDetails : DapperBaseEntity
    {
        #region Table properties

        [ExplicitKey]
        public int ADetailsID { get; set; }

        public string BankRefNo { get; set; }

        public int SGHeadID { get; set; }

        public int DHeadID { get; set; }

        public int SHeadID { get; set; }

        public int CalculationOn { get; set; }

        public decimal ValueInFC { get; set; }

        public decimal ValueInLC { get; set; }

        public decimal CurConvRate { get; set; }

        #endregion Table properties

        #region Additional Columns

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || ADetailsID > 0;

        [Write(false)]
        public int CTCategoryID { get; set; }

        [Write(false)]
        public bool DHeadNeed { get; set; }
        [Write(false)]
        public bool SHeadNeed { get; set; }
        [Write(false)]
        public string SGHeadName { get; set; }

        [Write(false)]
        public List<ImportDocumentAcceptanceChargeDetails> IDACDetailSub { get; set; }

        #endregion Additional Columns

        public ImportDocumentAcceptanceChargeDetails()
        {
            IDACDetailSub = new List<ImportDocumentAcceptanceChargeDetails>();
            CurConvRate = 0m;
        }

    }
}
