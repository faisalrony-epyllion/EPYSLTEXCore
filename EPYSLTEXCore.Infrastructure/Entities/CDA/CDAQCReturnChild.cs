using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDAQCReturnChild")]
    public class CDAQCReturnChild : DapperBaseEntity
    {
        [ExplicitKey]
        public int QCReturnChildID { get; set; }

        ///<summary>
        /// QCReturnMasterID
        ///</summary>
        public int QCReturnMasterId { get; set; }

        ///<summary>
        /// LotNo (length: 50)
        ///</summary>
        public string LotNo { get; set; }

        ///<summary>
        /// ReceiveQtyCarton
        ///</summary>
        public int ReceiveQty { get; set; }
        ///<summary>
        /// UnitID
        ///</summary>
        public int UnitID { get; set; }

        ///<summary>
        /// ReturnQtyCarton
        ///</summary>
        public int ReturnQty { get; set; }
        ///<summary>
        /// Rate
        ///</summary>
        public decimal Rate { get; set; }
        ///<summary>
        /// Remarks (length: 200)
        ///</summary>
        public string Remarks { get; set; }
        public int ItemMasterID { get; set; }

        #region Additional Columns

        [Write(false)]
        public int QCReceiveMasterId { get; set; }
        [Write(false)]
        public int QCRemarksMasterID { get; set; }
        [Write(false)]
        public int SupplierId { get; set; }
        [Write(false)]
        public int SpinnerId { get; set; }
        [Write(false)]
        public string Uom { get; set; }
        [Write(false)]
        public string Supplier { get; set; }
        [Write(false)]
        public string ItemName { get; set; }
        [Write(false)]
        public string AgentName { get; set; }
        [Write(false)]
        public int ReqQty { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.QCReturnChildID > 0;

        #endregion Additional Columns

        public CDAQCReturnChild()
        {
            UnitID = 28;
        }
    }

}
