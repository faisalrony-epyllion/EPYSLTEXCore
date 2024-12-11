using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.SCD
{
    [Table(TableNames.YarnBBLCProposalChild)]
    public class YarnBBLCProposalChild : DapperBaseEntity
    {
        [ExplicitKey]
        public int ChildID { get; set; }

        public int ProposalId { get; set; }

        public int YPIReceiveMasterID { get; set; }

        public int RevisionNo { get; set; }


        #region Additional

        [Write(false)]
        public DateTime PIDate { get; set; }

        [Write(false)]
        public string SupplierName { get; set; }

        [Write(false)]
        public int SupplierId { get; set; }

        [Write(false)]
        public string CompanyName { get; set; }

        [Write(false)]
        public int CompanyId { get; set; }

        [Write(false)]
        public string PiFilePath { get; set; }

        [Write(false)]
        public string Unit { get; set; }

        [Write(false)]
        public decimal TotalQty { get; set; }

        [Write(false)]
        public decimal TotalValue { get; set; }

        [Write(false)]
        public string YPINo { get; set; }

        [Write(false)]
        public int PIRevisionNo { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || ChildID > 0;

        #endregion Additional
    }
    /*//OFF FOR CORE//
    public class YarnBBLCProposalChildVlaidator : AbstractValidator<YarnBBLCProposalChild>
    {
        public YarnBBLCProposalChildVlaidator()
        {
            RuleFor(x => x.YPIReceiveMasterID).NotEmpty();
            RuleFor(x => x.YPINo).NotEmpty();
        }
    }
    */
}
