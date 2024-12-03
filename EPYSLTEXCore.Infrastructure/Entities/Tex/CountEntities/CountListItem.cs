using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.CountEntities
{

    public class CountListItem
    {
        public CountListItem()
        {
            Pending = 0;
            Draft = 0;
            SendingForApproval = 0;
            Reject = 0;
            Reject2 = 0;
            Approved = 0;
            Acknowledged = 0;
            UnAcknowledged = 0;
            AllCount = 0;
            CheckCount = 0;
            NewCount = 0;
            Revision = 0;
            Cancel = 0;
            Pending2 = 0;
            PendingAllowance = 0;
            UtilizationProposalPending = 0;
            UtilizationConfirmationPending = 0;
            UtilizationConfirmed = 0;
        }
        public int Pending { get; set; }
        public int Draft { get; set; }
        public int SendingForApproval { get; set; }
        public int Reject { get; set; }
        public int Reject2 { get; set; }
        public int Approved { get; set; }
        public int Acknowledged { get; set; }
        public int UnAcknowledged { get; set; }
        public int AllCount { get; set; }
        public int CheckCount { get; set; }
        public int NewCount { get; set; }
        public int Revision { get; set; }
        public int Cancel { get; set; }
        public int Pending2 { get; set; }
        public int Proposal { get; set; }
        public int PendingAllowance { get; set; }
        public int UtilizationProposalPending { get; set; }
        public int UtilizationConfirmationPending { get; set; }
        public int UtilizationConfirmed { get; set; }
    }
}
