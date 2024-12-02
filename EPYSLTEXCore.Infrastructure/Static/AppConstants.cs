﻿namespace EPYSLTEXCore.Infrastructure.Static
{
    public static class AppConstants
    {
        public const string DB_CONNECTION = "DBConnection";
        public const string NEW = "<<--New-->>";
        public const string SYMMETRIC_SECURITY_KEY = "8b327b47-2e48-4116-9134-dcbcd5aff40b";
        //public const string ConnectionString = "Server=192.168.10.231;User ID=sa;Password=Erp123!;Database=EPYEMS;Integrated Security=False;Trusted_Connection=False;TrustServerCertificate=True";
        public const string SPECIAL_CHARACTER_TO_REMOVE_IN_UPLOAD = "['<>&]|[\n]{2}";
        public const string GMT_CONNECTION = "GmtConnection";
        public const string TEXTILE_CONNECTION = "TexConnection";
        public const int ITEM_SUB_GROUP_YARN_NEW = 102;
        public const int ITEM_SUB_GROUP_YARN = 39;
    }
    public static class DbNames
    {
        public const string EPYSL = "EPYSL";
        public const string EPYSLTEX = "EPYSLTEX";
    }
    public static class ContactCategoryNames
    {
        public const string BUYER = "Buyer";
        public const string SUPPLIER = "Supplier";
        public const string FORWARDER = "Forwarder";
        public const string INSURANCE_COMPANY = "Insurance Company";
        public const string NOTIFY_PARTY = "Notify Party";
        public const string SHIPPING_LINE = "Shipping Line";
        public const string LOCAL_AGENT = "Local Agent";
        public const string CLEARING_AGENT = "Clearing Agent";
        public const string CNF = "CnF";
        public const string CARRYING_CONTRACTOR = "Carrying Contractor";
        public const string CUSTOMER = "Customer";
        public const string CONSIGNEE = "Consignee";
        public const string TREADER = "Treader";
        public const string SPINNER = "Spinner";
    }

    public static class StatusConstants
    {
        public const string PENDING = "Pending";
        public const string ACKNOWLEDGE = "Acknowledge";
        public const string PROPOSED = "Proposed";
        public const string ACCEPT = "Accept";
        public const string REJECT = "Reject";
        public const string CANCEL = "Cancel";
    }

    public static class UserRoles
    {
        public const string SUPER_USER = "SuperUser";
        public const string ADMIN = "Admin";
        public const string GENERAL = "User";
    }
    public static class JwtTokenStorage
    {
        public const string UserID = "UserID";
        public const string CompanyId = "CompanyId"; 

    }

    /// <summary>
    /// Success message constants used throughout the application.
    /// </summary>
    public static class SuccessKeys
    {
        /// <summary>
        /// Success message for email sent.
        /// </summary>
        public const string EmailSent = "A password reset link has been sent to your email address.";

        /// <summary>
        /// Generic success message.
        /// </summary>
        public const string Success = "Your request has been processed successfully.";

        /// <summary>
        /// Success message for a warmateba request.
        /// </summary>
        public const string Warmateba = "The request has been completed successfully.";

        /// <summary>
        /// Success message for deletion with ID.
        /// </summary>
        public const string Delete = "The data has been successfully deleted. ID:";

        /// <summary>
        /// Success message for successful insertion.
        /// </summary>
        public const string InsertSuccess = "The operation was successful, and the server returned a 200 OK status.";
    }

    /// <summary>
    /// Error message constants used throughout the application.
    /// </summary>
    public static class ErrorKeys
    {
        /// <summary>
        /// Bad request error message.
        /// </summary>
        public const string BadRequest = "Bad request. Server did not respond correctly.";

        /// <summary>
        /// Unsuccessful insert error message.
        /// </summary>
        public const string UnsuccesfullInsertUpdate = "Insertion/Update was unsuccessful.";

        /// <summary>
        /// Unsuccessful insert error message.
        /// </summary>
        public const string UnsuccesfullInsert = "Insertion was unsuccessful.";

        /// <summary>
        /// Unsuccessful update error message.
        /// </summary>
        public const string UnsucessfullUpdate = "Update was unsuccessful.";

        /// <summary>
        /// Not found error message.
        /// </summary>
        public const string NotFound = "No related entities found.";

        /// <summary>
        /// No record exists error message.
        /// </summary>
        public const string NoRecord = "No record exists.";

        /// <summary>
        /// Customer does not exist error message.
        /// </summary>
        public const string NoCustomer = "Customer does not exist.";

        /// <summary>
        /// Internal server error message.
        /// </summary>
        public const string InternalServerError = "Internal server error occurred.";

        /// <summary>
        /// Date validation error message.
        /// </summary>
        public const string DateValidation = "Date format is incorrect.";

        /// <summary>
        /// Mapping error message.
        /// </summary>
        public const string Mapped = "Mapping was not successful.";

        /// <summary>
        /// General exception error message.
        /// </summary>
        public const string General = "General exception occurred while processing request.";

        /// <summary>
        /// Argument null error message.
        /// </summary>
        public const string ArgumentNull = "Argument is null. Please check.";

        /// <summary>
        /// No such product exists error message.
        /// </summary>
        public const string NoProduct = "No such product exists in the database.";

        /// <summary>
        /// Model state error message.
        /// </summary>
        public const string ModelState = "Model state is not valid.";
    }
    /// <summary>
    /// In memory cache key used throughout the application.
    /// </summary>
    public static class InMemoryCacheKeys
    {
        public const string APIReports = "DynamicAPIReports";
    }
    public static class CompnayIDConstants
    {
        public const int EFL = 6;
    }
    public static class EnumBaseType // EntityTypeValue
    {
        public const int None = 0;
        public const int ProjectionBasedBulk = 2161;
        public const int ProjectionBasedSample = 2162;
        public const int OrderBasedBulk = 2163;
        public const int OrderBasedSample = 2164;
        public const int GiftYarnReceive = 2165;
    }
    public static class InterfaceFrom
    {
        public const string FreeConcept = "FreeConcept";
        public const string MaterialRequirement = "MaterialRequirement";
        public const string KnittingProgram = "KnittingProgram";
        public const string KnittingConfirmation = "KnittingConfirmation";
        public const string KnittingProduction = "KnittingProduction";
        public const string GrayQC = "GrayQC";
        public const string RecipeRequest = "RecipeRequest";

        public const string YDRecipeRequest = "YDRecipeRequest";

        public const string RecipeDefinition = "RecipeDefinition";
        public const string FinishingProcess = "FinishingProcess";
        public const string FinishingProcessProduction = "FinishingProcessProduction";
        public const string BatchPlan = "BatchPlan";
        public const string BatchPreparation = "BatchPreparation";
        public const string DyeingBatchItem = "DyeingBatchItem";
        public const string LabTestRequisition = "LabTestRequisition";
        public const string LabTestResult = "LabTestResult";
        public const string FirmConcept = "FirmConcept";
        public const string FBookingAcknowledge = "FBookingAcknowledge";
        public const string SampleBooking = "SampleBooking";
        public const string DyeingBatch = "DyeingBatch";
        public const string DyeingProduction = "DyeingProduction";
        public const string YarnPR = "YarnPR";
        public const string YDBooking = "YDBooking";
        public const string LiveProduct = "LiveProduct";
    }
    public static class YarnPRFromTable
    {
        public const int NONE = 0;
        public const int FreeConceptMRBDS = 1;
        public const int ProjectionYarnBookingMaster = 2;
        public const int YarnBookingMaster = 3;
        public const int FBookingAcknowledge = 4;
        public const int YarnAllocationMaster = 5;
        public const int FreeConceptMRMaster = 6;
    }
    public static class EnumFromMenuType
    {
        public const int None = 0;
        public const int PO = 1;
        public const int YarnReceive = 2;
        public const int MRIR = 3;
        public const int YarnQCIssue = 4;
        public const int YarnQCReturnReceive = 5;
        public const int YarnAllocation = 6;
        public const int YarnAllocationAck = 7;
        public const int YarnAllocationUnAck = 8;
        public const int YarnRnDIssueMaster = 9;
        public const int KYIssueMaster = 10;
        public const int KnittingLeftOverReturnReceive = 11;
        public const int FBookingAcknowledgementYarnLiability = 12;
        public const int YarnAllocationReject = 13;
        public const int YarnAllocationReallocation = 14;

        public const int YDReqIssueRAndD = 15; //Not Used
        public const int YDReqIssueBulk = 16; //Not Used

        public const int KnittingSubContractIssueRAndD = 17; //Not Used
        public const int KnittingSubContractIssueBulk = 18; //Not Used

        public const int YarnRnDLeftOverReturnReceive = 19;
        public const int YarnYDLeftOverReturnReceiveRnD = 20;
        public const int YarnYDLeftOverReturnReceiveBulk = 21;

        public const int YarnAllocationUnAckRevision = 22;

        public const int RnDYarnRequisitionApp = 23;

        public const int KSCLOReturnReceive = 24;

        public const int KYReqMasterApp = 25;

        public const int KnittingSubContractReqApp = 26;
        public const int KnittingSubContractIssueApp = 27;

        public const int YDReqApp = 28;
        public const int YDIssueApp = 29;

        public const int YarnYDLeftOverReturnReceive_RnD_Bulk = 30;
    }
    public static class EnumStockFromTable
    {
        public const int None = 0;
        public const int YarnPOChild = 1;
        public const int YarnReceiveChild = 2;
        public const int YarnMRIRChild = 3;
        public const int YarnQCIssueChildRackBinMapping = 4;
        public const int YarnQCReturnReceiveChildRackBinMapping = 5;
        public const int YarnAllocationChildItem = 6;
        public const int YarnAllocationChildPipelineItem = 7;
        public const int YarnRnDIssueChildRackBinMapping = 8;
        public const int KYIssueChildRackBinMapping = 9;
        public const int KYLOReturnReceiveChildRackBinMapping = 10;
        public const int FBookingAcknowledgementYarnLiability = 11;
        public const int YDReqIssueChildRackBinMapping = 12;
        public const int KnittingSubContractIssueChildRackBinMapping = 13;
        public const int YarnRNDReturnReceiveChildRackBinMapping = 14;
        public const int YDLeftOverReturnReceiveChildRackBinMapping = 15;
        public const int YarnStockAdjustmentChildItem = 16;
        public const int YarnRnDReqChild = 17;
        public const int KSCLOReturnReceiveChildRackBinMapping = 18;
        public const int KYReqChild = 19;
        public const int KnittingSubContractReqChild = 20;
        public const int YDReqChild = 21;
    }
    public enum EnumOperationTypes
    {
        None = 0,
        INSERT = 1,
        UPDATE = 2,
        DELETE = 3,
        PROPOSE = 4,
        APPROVE = 5,
        REJECT = 6,
        Acknowledge = 7,
        UnAcknowledge = 8,
        Reallocation = 9,
        Revision = 10
    }
}
