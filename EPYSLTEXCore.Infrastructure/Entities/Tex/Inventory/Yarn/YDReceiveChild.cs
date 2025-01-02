using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.YD_RECEIVE_CHILD)]
    public class YDReceiveChild : DapperBaseEntity
    {
        [ExplicitKey]
        public int YDReceiveChildID { get; set; } = 0;
        public int YDReceiveMasterID { get; set; }= 0;
        public int YDReqIssueChildID { get; set; } = 0;
        public int YDReqChildID { get; set; } = 0;
        public int ItemMasterID { get; set; } = 0;
        public int UnitID { get; set; } = 0;
        public decimal ReceiveQty { get; set; }= 0;
        public int ReceiveCone { get; set; } = 0;
        public int ReceiveCarton { get; set; } = 0;
        public string Remarks { get; set; } = "";
        public string YarnCategory { get; set; } = "";
        public int YDRICRBId { get; set; } = 0;

        #region Additional Columns
        [Write(false)]
        public string Unit { get; set; } = "";
        [Write(false)]
        public decimal ReqQty { get; set; } = 0;
        [Write(false)]
        public decimal IssueQty { get; set; } = 0;
        [Write(false)]
        public decimal IssueQtyCone { get; set; } = 0;
        [Write(false)]
        public decimal IssueQtyCarton { get; set; } = 0;
        [Write(false)]
        public string LocationName { get; set; } = "";
        [Write(false)]
        public string RackNo { get; set; } = "";


        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YDReceiveChildID > 0;

        #endregion Additional Columns

        
    }

    #region Validators

    public class YDReceiveChildValidator : AbstractValidator<YDReceiveChild>
    {
        public YDReceiveChildValidator()
        {

        }
    }

    #endregion Validators

}
