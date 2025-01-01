using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDASSIssueChild")]
    public class CDASSIssueChild : DapperBaseEntity
    {
        [ExplicitKey]
        public int SSIssueChildID { get; set; }
        ///<summary>
        /// SSIssueMasterID
        ///</summary>
        public int SSIssueMasterID { get; set; }

        ///<summary>
        /// ItemMasterID
        ///</summary>
        public int ItemMasterID { get; set; }
        public string BatchNo { get; set; }


        ///<summary>
        /// UnitID
        ///</summary>
        public int UnitID { get; set; }
        ///<summary>
        /// Rate
        ///</summary>
        public decimal Rate { get; set; }
        ///<summary>
        /// ReqQty
        ///</summary>
        public int ReqQty { get; set; }

        ///<summary>
        /// IssueQty
        ///</summary>
        public int IssueQty { get; set; }

        ///<summary>
        /// Remarks (length: 250)
        ///</summary>
        public string Remarks { get; set; }
        #region Additional Columns
        [Write(false)]
        public string Uom { get; set; }
        [Write(false)]
        public string ItemName { get; set; }
        [Write(false)]
        public string AgentName { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.SSIssueChildID > 0;

        #endregion Additional Columns
        public CDASSIssueChild()
        {
            UnitID = 28;
            Uom = "Kg";
        }
    }
}

