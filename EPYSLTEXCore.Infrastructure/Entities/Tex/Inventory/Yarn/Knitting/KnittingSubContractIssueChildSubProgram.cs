using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn.Knitting
{
    [Table(TableNames.KNITTING_SUB_CONTRACT_ISSUE_CHILD_SUB_PROGRAM)]
    public class KnittingSubContractIssueChildSubProgram : BaseEntity
    {
        ///<summary>
        /// KSCIssueMasterID
        ///</summary>
        public int KSCIssueMasterID { get; set; }

        ///<summary>
        /// KSCIssueChildID
        ///</summary>
        public int KSCIssueChildID { get; set; }

        ///<summary>
        /// SubProgramID
        ///</summary>
        public int SubProgramID { get; set; }

        // Foreign keys

        /// <summary>
        /// Parent KnittingSubContractIssueChild pointed by [KnittingSubContractIssueChildSubProgram].([KSCChildID]) (FK_KSCChildSubGroup_KnittingSubContractChild)
        /// </summary>
        public virtual KnittingSubContractIssueChild KnittingSubContractIssueChild { get; set; } // FK_KSCChildSubGroup_KnittingSubContractChild
    }
}
