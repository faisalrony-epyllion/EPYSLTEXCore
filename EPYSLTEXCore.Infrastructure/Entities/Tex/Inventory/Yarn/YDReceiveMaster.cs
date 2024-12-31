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
    [Table(TableNames.YD_RECEIVE_MASTER)]
    public class YDReceiveMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int YDReceiveMasterID { get; set; } = 0;
        public string YDReceiveNo { get; set; } = AppConstants.NEW;
        public int YDReqIssueMasterID { get; set; } = 0;
        public DateTime YDReceiveDate { get; set; }= DateTime.Now;
        public int YDReceiveBy { get; set; }= 0;
        public int YDReqMasterID { get; set; } = 0;
        public int CompanyID { get; set; } = 0;
        public string Remarks { get; set; } = "";
        public int AddedBy { get; set; } = 0;
        public int UpdatedBy { get; set; } = 0;
        public DateTime DateAdded { get; set; }=DateTime.Now;
        public DateTime? DateUpdated { get; set; } = DateTime.Now;

        #region Additional Columns
        [Write(false)]
        public string CompanyName { get; set; } = "";
        [Write(false)]
        public string YDReqIssueNo { get; set; } = "";
        [Write(false)]
        public DateTime YDReqIssueDate { get; set; } =   DateTime.Now;
        [Write(false)]
        public string ChallanNo { get; set; } = "";
        [Write(false)]
        public DateTime ChallanDate { get; set; } = DateTime.Now;
        [Write(false)]
        public string GPNo { get; set; } = "";
        [Write(false)]
        public DateTime GPDate { get; set; } = DateTime.Now;

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YDReceiveMasterID > 0;

        [Write(false)]
        public IEnumerable<Select2OptionModel> CompanyList { get; set; }

        [Write(false)]
        public List<YDReceiveChild> Childs { get; set; }= new List<YDReceiveChild>();


        #endregion Additional Columns

    }

    #region Validators

    public class YDReceiveMasterValidator : AbstractValidator<YDReceiveMaster>
    {
        public YDReceiveMasterValidator()
        {

        }
    }

    #endregion Validators


}
