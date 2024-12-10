using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn
{
    [Table("YarnQCRemarksChildResult")]
    public class YarnQCRemarksChildResult : DapperBaseEntity
    {
        [ExplicitKey]
        public int QCRemarksChildResultID { get; set; }
        public int QCRemarksChildID { get; set; }
        public string ACountNe { get; set; }
        public decimal Twist { get; set; }
        public decimal CSP { get; set; }
        public int FabricColorID { get; set; }
        public int DyeingProcessID { get; set; }
        public int ThickThin { get; set; }
        public int BarreMark { get; set; }
        public int Naps { get; set; }
        public int Hairiness { get; set; }
        public int WhiteSpecks { get; set; }
        public int Polypropylyne { get; set; }
        public int Contamination { get; set; }
        public int DyePicksUpPerformance { get; set; }
        public int TestMethodRefID { get; set; }
        public string PillingGrade { get; set; }
        public int TechnicalNameID { get; set; }
        public string Remarks { get; set; }

        #region Additional Columns
        [Write(false)]
        public string DyeingProcessName { get; set; }
        [Write(false)]
        public string BarreMarkName { get; set; }
        [Write(false)]
        public string NapsName { get; set; }
        [Write(false)]
        public string HairinessName { get; set; }
        [Write(false)]
        public string WhiteSpecksName { get; set; }
        [Write(false)]
        public string PolypropylyneName { get; set; }
        [Write(false)]
        public string ContaminationName { get; set; }
        [Write(false)]
        public string DyePicksUpPerformanceName { get; set; }
        [Write(false)]
        public string ColorName { get; set; }
        [Write(false)]
        public string TestMethodRefName { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || QCRemarksChildResultID > 0;

        #endregion Additional Columns

    }
}
