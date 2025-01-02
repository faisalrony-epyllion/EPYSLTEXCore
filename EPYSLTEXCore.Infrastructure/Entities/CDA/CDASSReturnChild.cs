using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{

    [Table("CDASSReturnChild")]
    public class CDASSReturnChild : DapperBaseEntity
    {
        [ExplicitKey]
        public int SSReturnChildID { get; set; }

        ///<summary>
        /// SSReturnMasterID
        ///</summary>
        public int SSReturnMasterID { get; set; }

        ///<summary>
        /// BatchNo (length: 50)
        ///</summary>
        public string BatchNo { get; set; }

        ///<summary>
        /// UnitID
        ///</summary>
        public int UnitID { get; set; }

        ///<summary>
        /// Remarks (length: 200)
        ///</summary>
        public string Remarks { get; set; }

        ///<summary>
        /// ItemMasterID
        ///</summary>
        public int ItemMasterID { get; set; }
        ///<summary>
        /// Rate
        ///</summary>
        public decimal Rate { get; set; }
        ///<summary>
        /// ReceiveQty
        ///</summary>
        public int ReceiveQty { get; set; }

        ///<summary>
        /// ReturnQty
        ///</summary>
        public int ReturnQty { get; set; }
        #region Additional Columns

        [Write(false)]
        public int SSReceiveMasterId { get; set; }
        [Write(false)]
        public string Uom { get; set; }
        [Write(false)]
        public string ItemName { get; set; }
        [Write(false)]
        public string AgentName { get; set; }
        [Write(false)]
        public int SSRemarksMasterID { get; set; }
        [Write(false)]
        public int ReqQty { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.SSReturnChildID > 0;
        #endregion Additional Columns
    }
  
}

