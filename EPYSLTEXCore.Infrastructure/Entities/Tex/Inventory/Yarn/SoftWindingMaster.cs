using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.SOFT_WINDING_MASTER)]
    public class SoftWindingMaster: DapperBaseEntity
    {
        #region Table Properties
        [ExplicitKey]
        public int SoftWindingMasterID { get; set; } = 0;
        public string SoftWindingNo { get; set; } = "";
        public int YDBookingMasterID { get; set; } = 0;
        public DateTime SoftWindingDate { get; set; }= DateTime.Now;
        public int YDBatchID { get; set; } = 0;
        public string Remarks { get; set; } = "";
        public bool IsSendForApprove { get; set; } = false;
        public int SendForApproveBy { get; set; } = 0;
        public DateTime? SendForApproveDate { get; set; }=DateTime.Now;
        public bool IsApprove { get; set; } = false;
        public DateTime? ApproveDate { get; set; }
        public int ApproveBy { get; set; } = 0;
        public bool IsReject { get; set; } = false;
        public DateTime? RejectDate { get; set; }
        public int RejectBy { get; set; } = 0;
        public string RejectReason { get; set; } = "";
        public int AddedBy { get; set; } = 0;
        public int UpdatedBy { get; set; } = 0;
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public DateTime? DateUpdated { get; set; } = DateTime.Now;
        #endregion

        #region Additional Columns
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.SoftWindingMasterID > 0;

        [Write(false)]
        public List<SoftWindingChild> Childs { get; set; }=new List<SoftWindingChild>();

        [Write(false)]
        public string YDBookingNo { get; set; } = "";
        [Write(false)]
        public DateTime YDBookingDate { get; set; } = DateTime.Now;
        [Write(false)]
        public string ConceptNo { get; set; } = "";
        [Write(false)]
        public int ConceptID { get; set; } = 0;
        [Write(false)]
        public int TotalBookingQty { get; set; } = 0;
        [Write(false)]
        public int BuyerID { get; set; } = 0;
        [Write(false)]
        public string BuyerName { get; set; } = "";
        [Write(false)]
        public int ReqFromID { get; set; } = 0;
        [Write(false)]
        public string SendForApproveName { get; set; } = "";
        [Write(false)]
        public string ApprovedName { get; set; } = "";
        [Write(false)]
        public string RejectName { get; set; } = "";
        [Write(false)]
        public decimal Qty { get; set; } = 0;
        [Write(false)]
        public int Cone { get; set; } = 0;
        [Write(false)]
        public decimal ReceiveQty { get; set; } = 0;
        [Write(false)]
        public string Company { get; set; } = "";
        [Write(false)]
        public string YDBatchNo { get; set; } = "";
        [Write(false)]
        public DateTime YDBatchDate { get; set; }
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

    }

    #region Validators

    //public class SoftWindingMasterValidator : AbstractValidator<SoftWindingMaster>
    //{
    //    public SoftWindingMasterValidator()
    //    {
    //        RuleFor(x => x.SoftWindingDate).NotEmpty().WithMessage("SoftWinding Date is required.");
    //        RuleForEach(x => x.Childs).SetValidator(new SoftWindingChildValidator());
    //    }
    //}

    #endregion Validators
}
