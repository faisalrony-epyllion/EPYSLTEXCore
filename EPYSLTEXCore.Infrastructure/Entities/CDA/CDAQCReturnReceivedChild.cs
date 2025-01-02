using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.CDA
{
    [Table("CDAQCReturnReceivedChild")]
    public class CDAQCReturnReceivedChild : DapperBaseEntity
    {
        [ExplicitKey]
        public int QCReturnReceivedChildID { get; set; }

        ///<summary>
        /// QCReturnChildID
        ///</summary>
        public int QCReturnChildId { get; set; }

        ///<summary>
        /// QCReturnMasterID
        ///</summary>
        public int QCReturnMasterId { get; set; }

        ///<summary>
        /// QCReturnReceivedMasterID
        ///</summary>
        public int QCReturnReceivedMasterId { get; set; }

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

        ///<summary>
        /// ItemMasterID
        ///</summary>
        public int ItemMasterID { get; set; }
        public string ChallanLot { get; set; }

        #region Additional Columns

        [Write(false)]
        public int QCReturnReceivedMasterID { get; set; }
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
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.QCReturnReceivedChildID > 0;

        #endregion Additional Columns
        public CDAQCReturnReceivedChild()
        {
            UnitID = 28;
            ChallanLot = "";
        }
    }
  
}
