using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn
{
    [Table(TableNames.YARN_QC_RETURNRECEIVED_CHILD)]
    public class YarnQCReturnReceivedChild : DapperBaseEntity
    {
        [ExplicitKey]
        public int QCReturnReceivedChildID { get; set; }
        ///<summary>
        /// QCReturnChildID
        ///</summary>
        public int QCReturnChildID { get; set; }

        ///<summary>
        /// QCReturnMasterID
        ///</summary>
        public int QCReturnMasterID { get; set; }

        ///<summary>
        /// QCReturnReceivedMasterID
        ///</summary>
        public int QCReturnReceivedMasterID { get; set; }

        ///<summary>
        /// SupplierID
        ///</summary>
        public int SupplierID { get; set; }

        ///<summary>
        /// SpinnerID
        ///</summary>
        public int SpinnerID { get; set; }

        ///<summary>
        /// LotNo (length: 50)
        ///</summary>
        public string LotNo { get; set; }

        public decimal ReceiveQty { get; set; }
        ///<summary>
        /// ReceiveQtyCarton
        ///</summary>
        public int ReceiveQtyCarton { get; set; }

        ///<summary>
        /// ReceiveQtyCone
        ///</summary>
        public int ReceiveQtyCone { get; set; }

        ///<summary>
        /// UnitID
        ///</summary>
        public int UnitID { get; set; }
        public decimal ReturnQty { get; set; }
        ///<summary>
        /// ReturnQtyCarton
        ///</summary>
        public int ReturnQtyCarton { get; set; }

        ///<summary>
        /// ReturnQtyCone
        ///</summary>
        public int ReturnQtyCone { get; set; }

        ///<summary>
        /// Remarks (length: 200)
        ///</summary>
        public string Remarks { get; set; }

        ///<summary>
        /// ItemMasterID
        ///</summary>
        public int ItemMasterID { get; set; }
        public string ChallanLot { get; set; }
        // Foreign keys

        /// <summary>
        /// Parent YarnQCReturnReceivedMaster pointed by [YarnQCReturnReceivedChild].([QCReturnReceivedMasterId]) (FK_YarnQCReturnReceivedChild_YarnQCReturnReceivedMaster)
        /// </summary>
        // public virtual YarnQCReturnReceivedMaster YarnQCReturnReceivedMaster { get; set; } // FK_YarnQCReturnReceivedChild_YarnQCReturnReceivedMaster
        #region Additional Columns
        [Write(false)]
        public int QCReceiveMasterID { get; set; }
        [Write(false)]
        public int QCRemarksMasterID { get; set; }
        [Write(false)]
        public string Uom { get; set; }
        [Write(false)]
        public string Supplier { get; set; }
        [Write(false)]
        public string Spinner { get; set; }
        [Write(false)]
        public string YarnType { get; set; }
        [Write(false)]
        public string YarnCount { get; set; }
        [Write(false)]
        public string YarnComposition { get; set; }
        [Write(false)]
        public string Shade { get; set; }
        [Write(false)]
        public string YarnColor { get; set; }
        [Write(false)]
        public string YarnCategory { get; set; }
        [Write(false)]
        public string ChallanCount { get; set; }
        [Write(false)]
        public string PhysicalCount { get; set; }
        [Write(false)]
        public string DisplayUnitDesc { get; set; }
        [Write(false)]
        public int LocationID { get; set; }
        [Write(false)]
        public int ReceiveChildID { get; set; }
        [Write(false)]
        public List<YarnQCReturnReceiveChildRackBinMapping> ChildRackBins { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.QCReturnReceivedChildID > 0;

        #endregion Additional Columns
        public YarnQCReturnReceivedChild()
        {
            UnitID = 0;
            UnitID = 1;
            Uom = "Pcs";
            ChallanLot = "";
            LocationID = 0;
            ReceiveChildID = 0;
            ChildRackBins = new List<YarnQCReturnReceiveChildRackBinMapping>();

            YarnCategory = "";
            ChallanCount = "";
            PhysicalCount = "";
            DisplayUnitDesc = "";
        }
    }
    //#region Validators
    //public class YarnQCReturnReceivedChildValidator : AbstractValidator<YarnQCReturnReceivedChild>
    //{
    //    public YarnQCReturnReceivedChildValidator()
    //    {
    //        //RuleFor(x => x.QCReturnReceivedDate).NotEmpty();
    //        //RuleFor(x => x.QCRemarksMasterID).NotEmpty();
    //        //RuleFor(x => x.QCReturnNo).NotEmpty();
    //    }
    //}


    //#endregion
}
