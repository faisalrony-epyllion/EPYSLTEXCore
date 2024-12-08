using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Statics
{
    public enum Status
    {
        None = 0,
        All = 1,
        Pending = 2,
        Completed = 3,
        Proposed = 4,
        Approved = 5,
        Acknowledge = 6,
        PartiallyCompleted = 7,
        AllStatus = 8,
        Reject = 9,
        Revise = 10,
        Additional = 11,
        ReturnProposedPrice = 12,
        ReTest = 13,
        UnAcknowledge = 14,
        PendingReceiveCI = 15,
        PendingReceivePO = 16,
        UnApproved = 17,
        AwaitingPropose = 18,
        PendingBatch = 19,
        CPR = 20,
        FPR = 21,
        Active = 22,
        InActive = 23,
        Executed = 24,
        YDPPending = 25,
        YDPComplete = 26,
        YDQCPending = 27,
        YDQCComplete = 28,
        YDQCFail = 29,
        New = 30,
        Report = 31,
        Edit = 32,
        Check = 33,
        ProposedForApproval = 34,
        CheckReject = 35,
        ProposedForAcknowledge = 36,
        ProposedForAcknowledgeAcceptence = 37,
        AcknowledgeAcceptence = 38,
        Indent_Pending = 39,
        RejectReview = 40,
        Pre_Pending = 41,
        Post_Pending = 42,
        ReviseForAcknowledge = 43,
        Pass = 44,
        Fail = 45,
        Confirm = 46,
        Rework = 47,
        Hold = 48,
        PendingConfirmation = 49,
        PendingGroup = 50,
        PendingReceiveSF = 51,
        Draft = 52,
        ApprovedDone = 53,
        APPROVED_PMC = 54,
        REJECT_PMC = 55,
        APPROVED_Allowance = 56,
        REJECT_Allowance = 57,
        Others = 58,
        Approved2 = 59,
        Internal_Rejection = 60,
        Revise2 = 61,
        Pending2 = 62,
        Pending3 = 63,
        Completed2 = 64,
        Completed3 = 65,
        Return = 66,
        PendingRevise = 67,
        PendingReturnConfirmation = 68,
        PendingExportData = 69,
        ExportData = 70,
        ADDITIONAL_INTERNAL_REJECTION = 71,
        ADDITIONAL_APPROVED_OPERATION_HEAD = 72

    }
    public enum FreeConceptStatus
    {
        All = 1,
        Live = 2,
        Preserved = 3,
        Dropped = 4,
        SourcingPending = 5,
        YDPending = 6,
        KnitPending = 7,
        BatchPending = 8,
        DyeingPending = 9,
        FinishPending = 10,
        TestPending = 11,
        WaitingForLivePending = 12
    }
    public enum SampleBookingStatus
    {
        Pending = 1,
        Completed = 2,
        Revise = 3,
        Export = 4,
        Batch = 5
    }

    public enum TextileServiceWOStatus
    {
        Pending = 1,
        Completed = 2,
        Reject = 3,
        Revise = 4,
        New = 5,
        Batch = 6
    }

    public enum YarnPOFor
    {
        Advance = 1,
        SpecificOrder = 2,
        Both = 3
    }

    public enum YarnRequisitionStatus
    {
        PendingRequisition = 1,
        ApprovedRequisition = 2,
        UnapprovedRequisition = 3
    }

    public enum YarnProposedStatus
    {
        PendingProposed = 1,
        ApprovedProposed = 2,
        UnapprovedProposed = 3
    }

    public enum YarnIssueStatus
    {
        PendingIssue = 1,
        PendingIssueApproval = 2,
        ApprovedIssueLists = 3
    }

    public enum YarnTransferStatus
    {
        PendingTransfer = 1,
        PendingTransferApproval = 2,
        ApprovedTransferLists = 3
    }

    public enum GreyFabricProductionStatus
    {
        PendingBooking = 1,
        PendingProductionApproval = 2,
        ApprovedProductionLists = 3,
        PendingProductionQAApproval = 4,
        ApprovedProductionQALists = 5
    }

    /// <summary>
    /// Repeat After
    /// </summary>
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

    public enum BookingType
    {
        Sample = 0,
        Bulk = 1,
        Revised = 2,
    }

    public enum FirmConceptImageType
    {
        Product = 1,
        Needle = 2,
        CAM = 3
    }

    public enum KnittingProgramType
    {
        Concept = 1,
        BDS = 2,
        Bulk = 3
    }
    public enum EnumRackBinOperationType
    {
        None = 0,
        Addition = 1,
        Deduction = 2
    }
    public enum EnumBDSAcknowledgeParamType
    {
        BDSAcknowledge = 0,
        BulkBookingAck = 1,
        Projection = 2,
        BulkBookingCheck = 3,
        BulkBookingApprove = 4,
        BulkBookingFinalApprove = 5,
        BulkBookingYarnAllowance = 6,

        LabdipBookingAcknowledge = 7,
        LabdipBookingAcknowledgeRnD = 8,

        AdditionalYarnBooking = 9,
        AYBQtyFinalizationPMC = 10,
        AYBProdHeadApproval = 11,
        AYBTextileHeadApproval = 12,
        AYBKnittingUtilization = 13,
        AYBKnittingHeadApproval = 14,
        AYBOperationHeadApproval = 15,
        BulkBookingUtilizationProposal = 16,
        BulkBookingUtilizationConfirmation = 17
    }
    public enum ReceiveNoteType
    {
        MRIR = 1,
        GRN = 2,
        MRN = 3
    }
    public enum YarnBookingRevisionTypeEnum
    {
        None = 0,
        FabricRevision = 1,
        YarnInternalRevision = 2
    }
}
