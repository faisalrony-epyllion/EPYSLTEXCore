using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.YARN_MRIR_CHILD)]
    public class YarnMRIRChild : DapperBaseEntity
    {
        [ExplicitKey]
        public int MRIRChildID { get; set; }
        public int MRIRMasterId { get; set; }
        public string LotNo { get; set; }
        public int ItemMasterID { get; set; }

        public int QCRemarksChildID { get; set; }
        public int ReceiveChildID { get; set; }
        public int RackLocationId { get; set; }
        public int UnitID { get; set; }

        public decimal ReceiveQty { get; set; }

        public int ReceiveQtyCarton { get; set; }
        public int ReceiveQtyCone { get; set; }
        public decimal Rate { get; set; }
        public string Remarks { get; set; }
        public int YarnProgramId { get; set; }
        public string ChallanCount { get; set; }
        public string POCount { get; set; }
        public string PhysicalCount { get; set; }
        public string YarnCategory { get; set; }
        public int NoOfThread { get; set; }
        public bool Approve { get; set; }
        public int ApproveBy { get; set; }
        public DateTime? ApproveDate { get; set; }
        public bool Reject { get; set; }
        public int RejectBy { get; set; }
        public DateTime? RejectDate { get; set; }
        public bool ReTest { get; set; }
        public int ReTestBy { get; set; }
        public DateTime? ReTestDate { get; set; }


        //public virtual YarnMRIRMaster YarnMRIRMaster { get; set; } // FK_YarnQCReceiveInfoChild_YarnQCReceiveInfoMaster
        #region Additional Columns
        [Write(false)]
        public string Uom { get; set; }
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
        public string QCRemarksNo { get; set; }
        [Write(false)]
        public DateTime? QCRemarksDate { get; set; }
        [Write(false)]
        public string PONo { get; set; }
        [Write(false)]
        public string InvoiceNo { get; set; }
        [Write(false)]
        public string ReceiveNo { get; set; }
        [Write(false)]
        public string ReceiveDate { get; set; }
        [Write(false)]
        public string ChallanNo { get; set; }
        [Write(false)]
        public string Supplier { get; set; }
        [Write(false)]
        public string Spinner { get; set; }
        [Write(false)]
        public string YarnControlNo { get; set; }
        [Write(false)]
        public string VehicalNo { get; set; }
        [Write(false)]
        public string POUnit { get; set; }
        [Write(false)]
        public string QCRemarksByUser { get; set; }
        [Write(false)]
        public int QCReceiveMasterID { get; set; }
        [Write(false)]
        public int QCIssueMasterID { get; set; }
        [Write(false)]
        public int QCReqMasterID { get; set; }
        [Write(false)]
        public string QCReceiveByUser { get; set; }
        [Write(false)]
        public string Status { get; set; }
        [Write(false)]
        public string QCReqNo { get; set; }
        [Write(false)]
        public int RCompanyID { get; set; }
        [Write(false)]
        public int CompanyID { get; set; }
        [Write(false)]
        public int SupplierID { get; set; }
        [Write(false)]
        public int SpinnerID { get; set; }
        [Write(false)]
        public int ReceiveNoteType { get; set; }
        [Write(false)]
        public string MRIRNo { get; set; }
        [Write(false)]
        public string GRNNo { get; set; }
        [Write(false)]
        public string MRNNo { get; set; }
        [Write(false)]
        public int POQty { get; set; }
        [Write(false)]
        public int ReceiveNoteQty { get; set; }
        [Write(false)]
        public string TestType { get; set; }
        [Write(false)]
        public string YarnDetail { get; set; } = "";
        [Write(false)]
        public int AllocationChildID { get; set; } = 0;
        [Write(false)]
        public string ReceiveFrom { get; set; } = "";
        [Write(false)]
        public YarnAllocationMaster YarnAllocation { get; set; } = new YarnAllocationMaster();
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.MRIRChildID > 0;

        [Write(false)]
        public string QCRemarksMasterID { get; set; }
        #endregion Additional Columns

        public YarnMRIRChild()
        {
            MRIRMasterId = 0;
            LotNo = "";
            ItemMasterID = 0;
            QCRemarksChildID = 0;
            ReceiveChildID = 0;
            RackLocationId = 0;
            UnitID = 0;
            ReceiveQty = 0;
            ReceiveQtyCarton = 0;
            ReceiveQtyCone = 0;
            Rate = 0;
            Remarks = "";
            YarnProgramId = 0;
            ChallanCount = "";
            POCount = "";
            PhysicalCount = "";
            YarnCategory = "";
            NoOfThread = 0;
            Approve = false;
            ApproveBy = 0;
            ApproveDate = DateTime.Now;
            Reject = false;
            RejectBy = 0;
            RejectDate = DateTime.Now;
            ReTest = false;
            ReTestBy = 0;
            ReTestDate = DateTime.Now;

            // EntityState = EntityState.Added;
        }
    }
}
