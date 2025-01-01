using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.YD_Left_Over_Return_MASTER)]
    public class YDLeftOverReturnMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int YDLOReturnMasterID { get; set; } = 0;

        public string YDLOReturnNo { get; set; } = AppConstants.NEW;

        public DateTime YDLOReturnDate { get; set; } = DateTime.Now;

        public int YDLOReturnBy { get; set; } = 0;
        public int YDBookingMasterID { get; set; } = 0;
        public int YDRReceiveMasterID { get; set; } = 0;

        public string Remarks { get; set; } = "";

        public int AddedBy { get; set; } = 0;

        public DateTime DateAdded { get; set; } = DateTime.Now;

        public int UpdatedBy { get; set; } = 0;

        public DateTime? DateUpdated { get; set; }
        public int RCompanyID { get; set; } = 0;
        public int OCompanyID { get; set; } = 0;
        public int LocationID { get; set; } = 0;
        public int SupplierID { get; set; } = 0;
        public int SpinnerID { get; set; } = 0;

        public bool IsSendToMCD { get; set; } = false;
        public DateTime? SendToMCDDate { get; set; }
        public int SendToMCDBy { get; set; } = 0;

        public bool IsApprove { get; set; } = false;
        public DateTime? ApproveDate { get; set; }
        public int ApproveBy { get; set; } = 0;

        #region Additional

        [Write(false)]
        public List<YDLeftOverReturnChild> Childs { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YDLOReturnMasterID > 0;

        [Write(false)]
        public string YDLOReturnByUser { get; set; } = "";

        [Write(false)]
        public string AddedByUser { get; set; } = "";

        [Write(false)]
        public string UpdatedByUser { get; set; } = "";

        [Write(false)]
        public string YDRReceiveNo { get; set; } = "";

        [Write(false)]
        public int YDRReceivedBy { get; set; } = 0;

        [Write(false)]
        public string YDRReceivedByUser { get; set; } = "";

        [Write(false)]
        public DateTime YDRReceiveDate { get; set; } = DateTime.Now;

        [Write(false)]
        public string YDBookingNo { get; set; } = "";

        [Write(false)]
        public DateTime YDBookingDate { get; set; } = DateTime.Now;

        [Write(false)]
        public int YDRMasterID { get; set; } = 0;

        [Write(false)]
        public string YDRNo { get; set; } = "";

        [Write(false)]
        public DateTime YDRDate { get; set; } = DateTime.Now;

        [Write(false)]
        public int YDRBy { get; set; } = 0;

        [Write(false)]
        public string YDRByUser { get; set; } = "";

        [Write(false)]
        public string Location { get; set; } = "";

        [Write(false)]
        public string Supplier { get; set; } = "";

        [Write(false)]
        public string Spinner { get; set; } = "";
        [Write(false)]
        public string ProgramName { get; set; } = "";

        [Write(false)]
        public int ItemMasterID { get; set; } = 0;
        [Write(false)]
        public int YDReqIssueChildID { get; set; } = 0;
        [Write(false)]
        public int YDReqIssueMasterID { get; set; } = 0;
        [Write(false)]
        public string YDReqIssueNo { get; set; } = "";
        [Write(false)]
        public string YarnDetails { get; set; } = "";
        [Write(false)]
        public string PhysicalCount { get; set; } = "";
        [Write(false)]
        public string YarnCount { get; set; } = "";
        [Write(false)]
        public string LotNo { get; set; } = "";
        [Write(false)]
        public string BookingNo { get; set; } = "";
        [Write(false)]
        public string BatchNo { get; set; } = "";
        [Write(false)]
        public string Floor { get; set; } = "";
        [Write(false)]
        public decimal BalanceQuantity { get; set; } = 0;
        [Write(false)]
        public string ExportOrderNo { get; set; } = "";
        //public List<Select2OptionModel> QCForList { get; set; }
        //public List<Select2OptionModel> ReceiveList { get; set; }
        //public string[] ReceiveIds { get; set; }
        //public string QCReqFor { get; set; }
        //public string QCReqByUser { get; set; }

        #endregion Additional

        public YDLeftOverReturnMaster()
        {
            Childs = new List<YDLeftOverReturnChild>();
        }
    }
}
