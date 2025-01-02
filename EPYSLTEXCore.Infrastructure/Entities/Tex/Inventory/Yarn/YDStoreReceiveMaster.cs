using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.YDStoreReceiveMaster)]
    public class YDStoreReceiveMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int YDStoreReceiveMasterID { get; set; }
        public string YDStoreReceiveNo { get; set; }
        public int SendToYDStoreMasterID { get; set; }
        public DateTime YDStoreReceiveDate { get; set; }
        public int YDStoreReceiveBy { get; set; }
        public int YDBatchID { get; set; } = 0;
        public int CompanyID { get; set; } = 0;
        public int LocationID { get; set; } = 0;
        public string LotNo { get; set; }
        public bool PartialAllocation { get; set; }
        public bool CompleteAllocation { get; set; }
        public string Remarks { get; set; }
        public int AddedBy { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateUpdated { get; set; }

        #region Additional Columns
        [Write(false)]
        public string CompanyName { get; set; }
        [Write(false)]
        public string SendToYDStoreNo { get; set; }
        [Write(false)]
        public DateTime SendToYDStoreDate { get; set; }
        [Write(false)]
        public string ChallanNo { get; set; }
        [Write(false)]
        public DateTime ChallanDate { get; set; }
        [Write(false)]
        public string GPNo { get; set; }
        [Write(false)]
        public DateTime GPDate { get; set; }
        [Write(false)]
        public string YDBatchNo { get; set; } = "";
        [Write(false)]
        public int YDBookingMasterID { get; set; } = 0;
        [Write(false)]
        public string YDBookingNo { get; set; } = "";
        [Write(false)]
        public int SupplierID { get; set; } = 0;
        [Write(false)]
        public string SupplierName { get; set; } = "";
        [Write(false)]
        public int SpinnerID { get; set; } = 0;
        [Write(false)]
        public string LocationName { get; set; } = "";
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YDStoreReceiveMasterID > 0;

        [Write(false)]
        public IEnumerable<Select2OptionModel> CompanyList { get; set; }

        [Write(false)]
        public List<YDStoreReceiveChild> Childs { get; set; } = new List<YDStoreReceiveChild>();
        [Write(false)]
        public List<Select2OptionModel> TransportTypeList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> TransportModeList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> TransportAgencyList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> ShipmentStatusList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> LocationList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> SpinnerList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> CContractorList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> RCompanyList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> SupplierList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModel> BuyerList { get; set; } = new List<Select2OptionModel>();
        [Write(false)]
        public List<Select2OptionModelExtended> SpinnerWisePackingList { get; set; } = new List<Select2OptionModelExtended>();

        [Write(false)]
        public List<Select2OptionModel> RackList { get; set; } = new List<Select2OptionModel>();

        [Write(false)]
        public List<Select2OptionModel> BinList { get; set; } = new List<Select2OptionModel>();

        [Write(false)]
        public List<Select2OptionModel> EmployeeList { get; set; } = new List<Select2OptionModel>();

        [Write(false)]
        public List<Select2OptionModel> ReceiveForList { get; set; } = new List<Select2OptionModel>();
        #endregion Additional Columns

        public YDStoreReceiveMaster()
        {
            YDStoreReceiveNo = AppConstants.NEW;
            SendToYDStoreMasterID = 0;
            YDStoreReceiveDate = DateTime.Now;
            YDStoreReceiveBy = 0;
            YDBatchID = 0;
            CompanyID = 0;
            Remarks = "";
            AddedBy = 0;
            UpdatedBy = 0;
            DateAdded = DateTime.Now;
            DateUpdated = DateTime.Now;
            Childs = new List<YDStoreReceiveChild>();

            CompanyName = "";
            SendToYDStoreNo = "";
            SendToYDStoreDate = DateTime.Now;
            ChallanNo = "";
            ChallanDate = DateTime.Now;
            GPNo = "";
            GPDate = DateTime.Now;
        }
    }
}
