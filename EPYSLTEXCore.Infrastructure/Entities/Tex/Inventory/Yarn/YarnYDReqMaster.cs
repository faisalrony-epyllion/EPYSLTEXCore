using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using EPYSLTEXCore.Infrastructure.Static;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.YARN_YD_REQ_MASTER)]
    public class YarnYDReqMaster : DapperBaseEntity
    {
        #region Table Properties
        [ExplicitKey]
        public int YDReqMasterID { get; set; }
        public string YDReqNo { get; set; }
        public int YDBookingMasterID { get; set; }
        public int YDReqBy { get; set; }
        public DateTime YDReqDate { get; set; }
        public int ReqFromID { get; set; }
        public string Remarks { get; set; }
        public bool IsSendForApprove { get; set; }
        public int? SendForApproveBy { get; set; }
        public DateTime? SendForApproveDate { get; set; }
        public bool IsApprove { get; set; }
        public DateTime? ApproveDate { get; set; }
        public int? ApproveBy { get; set; }
        public bool IsAcknowledge { get; set; }
        public DateTime? AcknowledgeDate { get; set; }
        public int? AcknowledgeBy { get; set; }
        public bool IsReject { get; set; }
        public DateTime? RejectDate { get; set; }
        public int? RejectBy { get; set; }
        public string RejectReason { get; set; }
        public int AddedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateUpdated { get; set; }
        #endregion

        #region Additional Columns
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.YDReqMasterID > 0;

        [Write(false)]
        public List<YarnYDReqChild> Childs { get; set; }

        [Write(false)]
        public string YDBookingNo { get; set; }
        [Write(false)]
        public DateTime YDBookingDate { get; set; }
        [Write(false)]
        public int ConceptID { get; set; }
        [Write(false)]
        public string ConceptNo { get; set; }

        [Write(false)]
        public int TotalBookingQty { get; set; }

        [Write(false)]
        public string BuyerName { get; set; }
        [Write(false)]
        public string SendForApproveName { get; set; }
        [Write(false)]
        public string ApprovedName { get; set; }
        [Write(false)]
        public string AcknowledgeName { get; set; }
        [Write(false)]
        public string RejectName { get; set; }

        [Write(false)]
        public int RequiredQty { get; set; }

        [Write(false)]
        public int RequsitionQty { get; set; }

        [Write(false)]
        public string ColorName { get; set; }
        [Write(false)]
        public string BookingForName { get; set; }


        [Write(false)]
        public IEnumerable<Select2OptionModel> SupplierList { get; set; }

        [Write(false)]
        public IEnumerable<Select2OptionModel> YarBrandList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> ReqFromList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> YarnColorList { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> YarnDyeingForList { get; set; }

        #endregion Additional Columns

        public YarnYDReqMaster()
        {
            DateAdded = DateTime.Now;
            YDReqDate = DateTime.Now;
            YDReqNo = AppConstants.NEW;
            IsSendForApprove = false;
            IsApprove = false;
            IsAcknowledge = false;
            IsReject = false;
            SendForApproveDate = DateTime.Now;
            Childs = new List<YarnYDReqChild>();
        }
    }
}
