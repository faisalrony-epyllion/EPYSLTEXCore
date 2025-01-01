using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{

    [Table("CDASSReceiveChild")]
    public class CDASSReceiveChild : DapperBaseEntity
    {
        [ExplicitKey]
        public int SSReceiveChildID { get; set; }
        
        ///<summary>
        /// SSReceiveMasterID
        ///</summary>
        public int SSReceiveMasterID { get; set; }

        ///<summary>
        /// LotNo (length: 50)
        ///</summary>
        public string BatchNo { get; set; }

        ///<summary>
        /// ItemMasterID
        ///</summary>
        public int ItemMasterID { get; set; }

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
        /// ReceiveQty
        ///</summary>
        public int ReceiveQty { get; set; }
        #region Additional Columns

        [Write(false)]
        public string ReqByUser { get; set; }
        [Write(false)]
        public string Uom { get; set; }
        [Write(false)]
        public string Supplier { get; set; }
        [Write(false)]
        public string ItemName { get; set; }
        [Write(false)]
        public string AgentName { get; set; }
        [Write(false)]
        public int SSRemarksMasterId { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.SSReceiveChildID > 0;

        #endregion Additional Columns


    }

}

