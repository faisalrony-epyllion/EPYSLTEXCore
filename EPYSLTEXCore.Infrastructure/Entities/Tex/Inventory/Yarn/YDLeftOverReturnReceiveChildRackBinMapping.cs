using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.YD_Left_Over_Return_Receive_CHILD_RACK_BIN_MAPPING)]
    public class YDLeftOverReturnReceiveChildRackBinMapping : DapperBaseEntity
    {
        public YDLeftOverReturnReceiveChildRackBinMapping()
        {
            YDLORRCRBId = 0;
            YDLeftOverReturnReceiveChildID = 0;
            IsUsable = false;
            ChildRackBinID = 0;
            ReceiveQtyCone = 0;
            ReceiveCartoon = 0;
            ReceiveQtyKg = 0;
            YarnStockSetId = 0;
        }
        [ExplicitKey]
        public int YDLORRCRBId { get; set; }
        public int YDLeftOverReturnReceiveChildID { get; set; } = 0;
        public int ChildRackBinID { get; set; } = 0;
        public bool IsUsable { get; set; } = false;
        public int ReceiveQtyCone { get; set; } = 0;
        public int ReceiveCartoon { get; set; } = 0;
        public decimal ReceiveQtyKg { get; set; } = 0;
        public int YarnStockSetId { get; set; } = 0;

        #region Additional properties
        [Write(false)]
        public int ChildID { get; set; } = 0;
        [Write(false)]
        public int LocationID { get; set; } = 0;
        [Write(false)]
        public int RackID { get; set; } = 0;
        [Write(false)]
        public int NoOfCartoon { get; set; } = 0;
        [Write(false)]
        public int NoOfCone { get; set; } = 0;
        [Write(false)]
        public decimal ReceiveQty { get; set; } = 0;
        [Write(false)]
        public string RackNo { get; set; } = "";
        [Write(false)]
        public string LocationName { get; set; } = "";

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YDLORRCRBId > 0;
        #endregion
    }
}
