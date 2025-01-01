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
    [Table(TableNames.YD_Left_Over_Return_Receive_CHILD)]
    public class YDLeftOverReturnReceiveChild : DapperBaseEntity
    {
        [ExplicitKey]
        public int YDLeftOverReturnReceiveChildID { get; set; }

        public int YDLeftOverReturnReceiveMasterID { get; set; }
        public int YDLOReturnChildID { get; set; } = 0;
        public decimal UseableReceiveQtyKG { get; set; } = 0;
        public int UseableReceiveQtyCone { get; set; } = 0;
        public int UseableReceiveQtyBag { get; set; } = 0;
        public decimal UnuseableReceiveQtyKG { get; set; } = 0;
        public int UnuseableReceiveQtyCone { get; set; } = 0;
        public int UnuseableReceiveQtyBag { get; set; } = 0;
        public string Remarks { get; set; } = "";
        public int YarnStockSetId { get; set; } = 0;

        #region Additional

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
        public string YarnProgramName { get; set; } = "";
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YDLeftOverReturnReceiveChildID > 0;
        [Write(false)]
        public List<YDLeftOverReturnReceiveChildRackBinMapping> ChildRackBins { get; set; } = new List<YDLeftOverReturnReceiveChildRackBinMapping>();
        [Write(false)]
        public List<YDLeftOverReturnReceiveChildRackBinMapping> ChildRackBinsUnuseable { get; set; } = new List<YDLeftOverReturnReceiveChildRackBinMapping>();
        #endregion Additional

        public YDLeftOverReturnReceiveChild()
        {
            //UnitID = 28;
            Uom = "Kg";
        }
    }
}
