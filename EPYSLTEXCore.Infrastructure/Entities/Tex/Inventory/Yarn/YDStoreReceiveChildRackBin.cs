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
    [Table(TableNames.YDStoreReceiveChildRackBin)]
    public class YDStoreReceiveChildRackBin : DapperBaseEntity
    {
        [ExplicitKey]
        public int ChildRackBinID { get; set; }
        public int ChildID { get; set; }
        public int LocationID { get; set; }
        public int RackID { get; set; }
        public int BinID { get; set; }
        public string LotNo { get; set; } = "";
        public string PhysicalCount { get; set; } = "";
        public string ShadeCode { get; set; } = "";
        public int NoOfCartoon { get; set; }
        public int NoOfCone { get; set; }
        public decimal ReceiveQty { get; set; }
        public string Remarks { get; set; }
        public string EmployeeID { get; set; }
        public int AddedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateUpdated { get; set; }
        public int SupplierID { get; set; } = 0;
        public int SpinnerID { get; set; } = 0;
        public int ItemMasterID { get; set; } = 0;
        public string YarnCategory { get; set; } = "";

        #region Additional Columns
        [Write(false)]
        public string RackNo { get; set; }
        [Write(false)]
        public string LocationName { get; set; }
        [Write(false)]
        public string BinNo { get; set; }
        [Write(false)]
        public int IssueQtyCone { get; set; }
        [Write(false)]
        public int IssueCartoon { get; set; }
        [Write(false)]
        public decimal IssueQtyKg { get; set; }
        [Write(false)]
        public int YPRCRBId { get; set; }
        [Write(false)]
        public int YQCICRBId { get; set; }
        [Write(false)]
        public int YQCRRId { get; set; }
        [Write(false)]
        public int YRICRBId { get; set; }
        [Write(false)]
        public int KSCICRBId { get; set; }
        [Write(false)]
        public int YDRICRBId { get; set; }
        [Write(false)]
        public string RackBinType { get; set; }
        [Write(false)]
        public string SpinnerName { get; set; }
        [Write(false)]
        public int YarnStockSetId { get; set; } = 0;
        [Write(false)]
        public string YarnControlNo { get; set; }
        [Write(false)]
        public int MinNoOfCartoon { get; set; } = 0;
        [Write(false)]
        public int MinNoOfCone { get; set; } = 0;
        [Write(false)]
        public decimal MinReceiveQty { get; set; } = 0;
        [Write(false)]
        public decimal RackQty { get; set; } = 0;
        [Write(false)]
        public decimal AvgCartoonWeight { get; set; } = 0;
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || ChildRackBinID > 0;

        #endregion Additional Columns

        public YDStoreReceiveChildRackBin()
        {
            BinID = 0;
            ReceiveQty = 0;
            DateAdded = DateTime.Now;
            EntityState = System.Data.Entity.EntityState.Added;
            LocationName = "";
            BinNo = "";
            IssueQtyCone = 0;
            IssueCartoon = 0;
            IssueQtyKg = 0;

            YPRCRBId = 0;
            YQCICRBId = 0;
            RackBinType = "";
            SpinnerName = "";
            YarnControlNo = "";
        }
    }
}
