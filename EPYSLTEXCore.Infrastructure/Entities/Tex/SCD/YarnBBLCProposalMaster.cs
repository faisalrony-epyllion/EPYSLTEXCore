using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using EPYSLTEXCore.Infrastructure.Static;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.SCD
{
    [Table(TableNames.YarnBBLCProposalMaster)]
    public class YarnBBLCProposalMaster : DapperBaseEntity
    {
        public YarnBBLCProposalMaster()
        {
            DateAdded = DateTime.Now;
            RevisionNo = 0;
            ProposalNo = AppConstants.NEW;
            ProposalDate = DateTime.Now;
            IsCDA = false;
            IsContract = false;
            CashStatus = false;
            ProposeBankID = 0;
            Childs = new List<YarnBBLCProposalChild>();
            YarnLcMasters = new List<YarnLcMaster>();
            YarnLCChilds = new List<YarnLcChild>();
        }

        [ExplicitKey]
        public int ProposalID { get; set; }
        public string ProposalNo { get; set; }
        public DateTime ProposalDate { get; set; }
        public string YPINo { get; set; }
        public int SupplierID { get; set; }
        public int CompanyID { get; set; }
        public int AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? DateUpdated { get; set; }
        public int RevisionNo { get; set; }
        public string Remarks { get; set; }
        public bool IsCDA { get; set; }
        public bool IsContract { get; set; }
        public int ProposeContractID { get; set; }
        //public string ProposeContract { get; set; }
        public bool CashStatus { get; set; }
        public int ProposeBankID { get; set; }
        public string ProposeBankName { get; set; }

        public int RetirementModeID { get; set; }

        #region Additional

        [Write(false)]
        public List<YarnBBLCProposalChild> Childs { get; set; }
        [Write(false)]
        public List<YarnLcMaster> YarnLcMasters { get; set; }
        [Write(false)]
        public List<YarnLcChild> YarnLCChilds { get; set; }
        [Write(false)]
        public List<YarnPIReceiveMaster> YarnPIReceives { get; set; } = new List<YarnPIReceiveMaster>();
        [Write(false)]
        public string SupplierName { get; set; }

        [Write(false)]
        public string CompanyName { get; set; }

        [Write(false)]
        public int YPIReceiveMasterID { get; set; }

        [Write(false)]
        public DateTime PIDate { get; set; }

        [Write(false)]
        public string PONo { get; set; }
        [Write(false)]
        public string LCNo { get; set; }

        [Write(false)]
        public DateTime LCDate { get; set; }

        [Write(false)]
        public decimal TotalQty { get; set; }

        [Write(false)]
        public decimal TotalValue { get; set; }
        [Write(false)]
        public string ProposeContract { get; set; }
        [Write(false)]
        public string BusinessNature { get; set; }
        [Write(false)]
        public int LCID { get; set; }
        [Write(false)]
        public bool isMerge { get; set; }

        [Write(false)]
        public bool isRevision { get; set; }

        [Write(false)]
        public string BranchName { get; set; }

        [Write(false)]
        public string RetirementMode { get; set; }

        [Write(false)]
        public bool PIAcceptStatus { get; set; }

        [Write(false)]
        public IList<Select2OptionModel> TExportLCList { get; set; }
        [Write(false)]
        public IList<Select2OptionModel> ProposeBankList { get; set; }

        [Write(false)]
        public IList<Select2OptionModel> RetirementModeList { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || ProposalID > 0;

        #endregion Additional
    }
    /*//OFF FOR CORE//
    public class YarnBBLCProposalMasterValidator : AbstractValidator<YarnBBLCProposalMaster>
    {
        public YarnBBLCProposalMasterValidator()
        {
            RuleFor(x => x.Remarks).MaximumLength(500);
            RuleFor(x => x.CompanyID).NotEmpty();
            RuleFor(x => x.SupplierID).NotEmpty();
            RuleForEach(x => x.Childs).SetValidator(new YarnBBLCProposalChildVlaidator());
        }
    }*/
}
