using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn
{
    [Table("YarnQCReqMaster")]
    public class YarnQCReqMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int QCReqMasterID { get; set; }
        public string QCReqNo { get; set; }
        public int QCReqBy { get; set; }
        public DateTime QCReqDate { get; set; }
        public int ReceiveID { get; set; }
        public int QCForId { get; set; }
        public int LocationID { get; set; }
        public int CompanyID { get; set; }
        public int RCompanyID { get; set; }
        public int SupplierID { get; set; }
        public int SpinnerID { get; set; }
        public string PhysicalCount { get; set; }
        public string LotNo { get; set; }
        public bool IsApprove { get; set; }
        public DateTime? ApproveDate { get; set; }
        public int? ApproveBy { get; set; }
        public bool IsAcknowledge { get; set; }
        public DateTime? AcknowledgeDate { get; set; }
        public int? AcknowledgeBy { get; set; }
        public int AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? DateUpdated { get; set; }
        public bool NeedUSTER { get; set; }
        public bool NeedYarnTRF { get; set; }
        public bool NeedFabricTRF { get; set; }
        public bool IsRetest { get; set; }
        public bool IsRetestForRequisition { get; set; } = false;
        public int RetestQCReqMasterID { get; set; }
        public int RetestForRequisitionQCReqMasterID { get; set; } = 0;
        public bool IsSendForApproval { get; set; }
        public DateTime? IsSendDate { get; set; }
        public int? IsSendBy { get; set; }


        public bool IsReject { get; set; }
        public int RejectBy { get; set; }
        public DateTime? RejectDate { get; set; }
        public string RejectReason { get; set; }
        public int RevisioNo { get; set; }
        public int RetestParentQCRemarksMasterID { get; set; }

        #region Additional Columns
        [Write(false)]
        public DateTime? RackBinDate { get; set; }

        [Write(false)]
        public string IsApproveStr
        {
            get
            {
                return (this.IsApprove) ? "Yes" : "No";
            }
        }
        [Write(false)]
        public string IsAcknowledgeStr
        {
            get
            {
                return (this.IsAcknowledge) ? "Yes" : "No";
            }
        }
        [Write(false)]
        public List<Select2OptionModel> QCForList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> ReceiveList { get; set; }
        [Write(false)]
        public string QCReqFor { get; set; }
        [Write(false)]
        public string QCReqByUser { get; set; }
        [Write(false)]
        public string ReceiveNo { get; set; }
        [Write(false)]
        public DateTime? ReceiveDate { get; set; }
        [Write(false)]
        public string Supplier { get; set; }

        [Write(false)]
        public string SupplierName { get; set; }

        [Write(false)]
        public string Spinner { get; set; }
        [Write(false)]
        public string ImportCategory { get; set; }

        [Write(false)]
        public string RCompany { get; set; }
        [Write(false)]
        public string BuyerName { get; set; }
        [Write(false)]
        public string EWO { get; set; }
        [Write(false)]
        public string POFor { get; set; }
        [Write(false)]
        public string PhysicalLot { get; set; }
        [Write(false)]
        public string ChallanLot { get; set; }
        [Write(false)]
        public string Yarndetail { get; set; }
        [Write(false)]
        public string ItemMasterID { get; set; }
        [Write(false)]
        public decimal ReceivedQtyInKg { get; set; }
        [Write(false)]
        public bool IsRetestDiagnostic { get; set; }
        [Write(false)]
        public string ParentQCRemarksNo { get; set; }
        [Write(false)]
        public List<YarnQCIssueMaster> YarnQCIssueMasters { get; set; }

        [Write(false)]
        public List<YarnQCReceiveMaster> YarnQCReceiveMasters { get; set; }

        [Write(false)]
        public List<YarnQCReqChild> YarnQCReqChilds { get; set; }
        [Write(false)]
        public List<YarnReceiveChild> YarnReceiveChilds { get; set; }

        [Write(false)]
        public string Segment1ValueDesc { get; set; }

        [Write(false)]
        public string Segment2ValueDesc { get; set; }

        [Write(false)]
        public string Segment3ValueDesc { get; set; }

        [Write(false)]
        public string Segment4ValueDesc { get; set; }

        [Write(false)]
        public string Segment5ValueDesc { get; set; }

        [Write(false)]
        public string Segment6ValueDesc { get; set; }

        [Write(false)]
        public string Segment7ValueDesc { get; set; }

        [Write(false)]
        public string ShadeCode { get; set; }

        [Write(false)]
        public string ChallanNo { get; set; }

        [Write(false)]
        public DateTime ChallanDate { get; set; }
        [Write(false)]
        public string NoTestRemarks { get; set; }
        [Write(false)]
        public int ReceiveChildID { get; set; }
        [Write(false)]
        public bool IsFromNoTest { get; set; }
        [Write(false)]
        public int QCRemarksChildID { get; set; }
        [Write(false)]
        public string Status { get; set; }
        [Write(false)]
        public string RackNo { get; set; }
        [Write(false)]
        public bool IsRevise { get; set; }
        [Write(false)]
        public bool HasPrevQCReq { get; set; } = false;
        [Write(false)]
        public List<Select2OptionModel> TechnicalNameList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> MCTypeForFabricList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> BuyerList { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.QCReqMasterID > 0;

        #endregion Additional Columns

        public YarnQCReqMaster()
        {
            DateAdded = DateTime.Now;
            IsApprove = false;
            IsAcknowledge = false;
            YarnQCIssueMasters = new List<YarnQCIssueMaster>();
            YarnQCReceiveMasters = new List<YarnQCReceiveMaster>();
            YarnQCReqChilds = new List<YarnQCReqChild>();
            QCReqDate = DateTime.Now;
            QCReqNo = AppConstants.NEW;
            IsApprove = false;
            IsAcknowledge = false;
            QCForList = new List<Select2OptionModel>();
            ReceiveList = new List<Select2OptionModel>();
            YarnReceiveChilds = new List<YarnReceiveChild>();
            TechnicalNameList = new List<Select2OptionModel>();
            MCTypeForFabricList = new List<Select2OptionModel>();
            BuyerList = new List<Select2OptionModel>();
            NoTestRemarks = "";
            ReceiveChildID = 0;
            IsFromNoTest = false;
            IsRetest = false;
            RetestQCReqMasterID = 0;
            Status = "";
            IsSendForApproval = false;
            IsSendBy = 0;
            ReceivedQtyInKg = 0;
            BuyerName = "";
            RackNo = "";
            IsRetestDiagnostic = false;
            IsReject = false;
            RejectBy = 0;
            RejectReason = "";
            RevisioNo = 0;
            RetestParentQCRemarksMasterID = 0;

            IsRevise = false;
            ParentQCRemarksNo = "";
            QCRemarksChildID = 0;
        }
    }
}
