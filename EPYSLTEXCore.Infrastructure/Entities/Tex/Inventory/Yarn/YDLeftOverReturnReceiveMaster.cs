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
    [Table(TableNames.YD_Left_Over_Return_Receive_MASTER)]
    public class YDLeftOverReturnReceiveMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int YDLeftOverReturnReceiveMasterID { get; set; }

        public string YDLeftOverReturnReceiveNo { get; set; } = AppConstants.NEW;

        public DateTime YDLeftOverReturnReceiveDate { get; set; } = DateTime.Now;

        public int YDLeftOverReturnReceiveBy { get; set; } = 0;
        public int YDBookingMasterID { get; set; } = 0;
        public int YDRReceiveMasterID { get; set; } = 0;
        public int YDLOReturnMasterID { get; set; } = 0;

        public string Remarks { get; set; } = "";
        public bool Approve { get; set; } = false;
        public int ApproveBy { get; set; } = 0;
        public System.DateTime? ApproveDate { get; set; }
        public bool Reject { get; set; } = false;
        public int RejectBy { get; set; } = 0;
        public System.DateTime? RejectDate { get; set; }
        public string RejectReason { get; set; } = "";

        public int AddedBy { get; set; } = 0;

        public DateTime DateAdded { get; set; } = DateTime.Now;

        public int UpdatedBy { get; set; } = 0;

        public DateTime? DateUpdated { get; set; }


        #region Additional

        [Write(false)]
        public List<YDLeftOverReturnReceiveChild> Childs { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YDLeftOverReturnReceiveMasterID > 0;

        //[Write(false)]
        //public int YDBookingMasterID { get; set; }

        [Write(false)]
        public string YDLeftOverReturnReceiveByUser { get; set; }

        [Write(false)]
        public string YDBookingNo { get; set; }

        [Write(false)]
        public DateTime YDBookingDate { get; set; }

        [Write(false)]
        public string YDLOReturnNo { get; set; }

        [Write(false)]
        public System.DateTime YDLOReturnDate { get; set; }

        [Write(false)]
        public int YDLOReturnBy { get; set; }

        [Write(false)]
        public string YDLOReturnByUser { get; set; }

        [Write(false)]
        public string YDRReceiveNo { get; set; }

        [Write(false)]
        public DateTime YDRReceiveDate { get; set; }

        [Write(false)]
        public int YDRReceivedBy { get; set; }

        [Write(false)]
        public string YDRReceivedByUser { get; set; }

        [Write(false)]
        public string AddedByUser { get; set; }

        [Write(false)]
        public string UpdatedByUser { get; set; }

        [Write(false)]
        public string Location { get; set; }

        [Write(false)]
        public string Supplier { get; set; }

        [Write(false)]
        public string Spinner { get; set; }
        [Write(false)]
        public string BookingNo { get; set; }
        [Write(false)]
        public string Floor { get; set; }
        [Write(false)]
        public string ReturnFrom { get; set; }
        #endregion Additional

        public YDLeftOverReturnReceiveMaster()
        {
            Childs = new List<YDLeftOverReturnReceiveChild>();
        }
    }
}
