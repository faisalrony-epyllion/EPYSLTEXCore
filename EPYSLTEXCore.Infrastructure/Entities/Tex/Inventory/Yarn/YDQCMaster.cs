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
    [Table(TableNames.YD_QC_MASTER)]
    public class YDQCMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int YDQCMasterID { get; set; }
        ///<summary>
        /// YDQCNo (length: 50)
        ///</summary>
        public string YDQCNo { get; set; }

        ///<summary>
        /// YDQCDate
        ///</summary>
        public DateTime YDQCDate { get; set; }

        ///<summary>
        /// YDBookingMasterID
        ///</summary>
        public int YDBookingMasterID { get; set; }

        ///<summary>
        /// YDProductionMasterID
        ///</summary>
        public int YDProductionMasterID { get; set; } = 0;

        public int YDBatchID { get; set; } = 0;

        ///<summary>
        /// Remarks (length: 500)
        ///</summary>
        public string Remarks { get; set; }
        public bool IsSendForApprove { get; set; } = false;
        public int SendForApproveBy { get; set; } = 0;
        public DateTime? SendForApproveDate { get; set; }
        public bool IsApprove { get; set; } = false;
        public DateTime? ApproveDate { get; set; }
        public int ApproveBy { get; set; } = 0;
        public bool IsReject { get; set; } = false;
        public DateTime? RejectDate { get; set; }
        public int RejectBy { get; set; } = 0;
        public string RejectReason { get; set; } = "";
        ///<summary>
        /// AddedBy
        ///</summary>
        public int AddedBy { get; set; }

        ///<summary>
        /// DateAdded
        ///</summary>
        public DateTime DateAdded { get; set; }

        ///<summary>
        /// UpdatedBy
        ///</summary>
        public int? UpdatedBy { get; set; }

        ///<summary>
        /// DateUpdated
        ///</summary>
        public DateTime? DateUpdated { get; set; }

        // Reverse navigation

        /// <summary>
        /// Child YDQCChilds where [YDQCChild].[YDQCMasterID] point to this entity (FK_YarnDyeingQCChild_YarnDyeingQCMaster)
        /// </summary>
        //public virtual System.Collections.Generic.ICollection<YDQCChild> Childs { get; set; } // YDQCChild.FK_YarnDyeingQCChild_YarnDyeingQCMaster
        #region Additional Columns

        [Write(false)]
        public string YDBookingNo { get; set; }
        [Write(false)]
        public string YDBookingDate { get; set; }
        [Write(false)]
        public string YDProductionNo { get; set; }
        [Write(false)]
        public string YDBatchNo { get; set; }
        [Write(false)]
        public DateTime YDBatchDate { get; set; }
        [Write(false)]
        public int BuyerID { get; set; } = 0;
        [Write(false)]
        public string Buyer { get; set; } = "";
        [Write(false)]
        public DateTime YDProductionDate { get; set; }
        [Write(false)]
        public List<Select2OptionModel> SupplierList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> SpinnerList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<YDQCChild> Childs { get; set; } = new List<YDQCChild>();

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YDQCMasterID > 0;

        #endregion Additional Columns
        public YDQCMaster()
        {
            YDQCDate = DateTime.Now;
            DateAdded = DateTime.Now;
            YDQCNo = "";
            Childs = new List<YDQCChild>();
            SupplierList = new List<Select2OptionModel>();
            SpinnerList = new List<Select2OptionModel>();
        }
    }
}
