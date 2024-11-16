using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Static
{
    public enum Status
    {
        None = 0,
        All = 1,
        Pending = 2,
        PartiallyCompleted = 7,
        Completed = 3,
        ProposedForApproval = 34,
        AwaitingPropose = 18,
        Proposed = 4,
        UnApproved = 17,
        Approved = 5,
        ProposedForAcknowledge = 36,
        Acknowledge = 6,
        UnAcknowledge = 14,
        AllStatus = 8,
        Pass = 44,
        Fail = 45,
        Reject = 9,
        Revise = 10,
        Additional = 11,
        Active = 22,
        InActive = 23,
        Executed = 24,
        New = 30,
        Edit = 32,
        Check = 33,
        CheckReject = 35,
        ProposedForAcknowledgeAcceptence = 37,
        AcknowledgeAcceptence = 38,
        Confirm = 46,
        Rework = 47,
        Hold = 48,
        PendingConfirmation = 49,
        Draft = 52,
        Others = 58,
    }

    public enum RepeatAfterEnum
    {
        /// <summary>
        /// No Repeat
        /// </summary>
        NoRepeat = 1,

        /// <summary>
        /// Repeat Every Year
        /// </summary>
        EveryYear = 2,

        /// <summary>
        /// Every Month
        /// </summary>
        EveryMonth = 3,

        /// <summary>
        /// Every Day
        /// </summary>
        EveryDay = 4
    }
}
