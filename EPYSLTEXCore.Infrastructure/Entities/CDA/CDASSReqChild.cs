using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{

    [Table("CDASSReqChild")]
    public class CDASSReqChild : DapperBaseEntity
    {
        [ExplicitKey]
        public int SSReqChildID { get; set; }

        ///<summary>
        /// SSReqMasterID
        ///</summary>
        public int SSReqMasterID { get; set; }

        ///<summary>
        /// ItemMasterID
        ///</summary>
        public int ItemMasterID { get; set; }

        ///<summary>
        /// UnitID
        ///</summary>
        public int UnitID { get; set; }

        ///<summary>
        /// ReqQty
        ///</summary>
        public int ReqQty { get; set; }

        ///<summary>
        /// Remarks (length: 250)
        ///</summary>
        public string Remarks { get; set; }

        #region Additional Columns

        [Write(false)]
        public string DisplayUnitDesc { get; set; }
        [Write(false)]
        public int SSReqMasterId { get; set; }
        [Write(false)]
        public string Uom { get; set; }
        [Write(false)]
        public string ItemName { get; set; }
        [Write(false)]
        public string AgentName { get; set; }
        [Write(false)]
        public int CompanyId { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.SSReqChildID > 0;

        #endregion Additional Columns
        public CDASSReqChild()
        {
            UnitID = 28;
            DisplayUnitDesc = "Kg";
        }
    }

}

