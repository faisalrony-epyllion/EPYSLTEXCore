namespace EPYSLTEXCore.Infrastructure.Static
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
        public const int ITEM_SUB_GROUP_YARN_LIVE = 106;

        public const int ITEM_SUB_GROUP_YARN_NEW = 102;
        public const int ITEM_SUB_GROUP_YARN = 39;
        public const int APPLICATION_ID=11;

        public const string ITEM_SUB_GROUP_FABRIC = "Fabric";
    }
    public static class DbNames
    {
        public const string EPYSL = "EPYSL";
        public const string EPYSLTEX = "EPYSLTEX";
    }
    public static class EnumStockType
    {
        public const int None = 0;
        public const int PipelineStock = 1;
        public const int QuarantineStock = 2;
        public const int AdvanceStock = 3;
        public const int AllocatedStock = 4;
        public const int SampleStock = 5;
        public const int LeftoverStock = 6;
        public const int LiabilitiesStock = 7;
        public const int UnusableStock = 8;
        public const int BlockUnBlockStock = 9;
    }
    public static class UploadLocations {
        public const string YARN_CI_FILE_PATH = "/Uploads/YarnCI";
        public const string YARN_PI_FILE_PATH = "/Uploads/YarnPI";
        public const string LC_FILE_PATH = "/Uploads/LC";

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
        public const string STATUS = "Status";
        public const string ADD = "add";
        public const string UPDATE = "edit";
        public const string DELETE = "delete";
       
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
    public static class ItemSegmentConstants
    {
        public const int COMPOSITION = 2;
        public const int CONSTRUCTION = 3;
        public const int COLOR = 23;
        public const int YARN_TYPE = 108;
        public const int YARN_COMPOSITION = 111;
        public const int YARN_COUNT = 109;
        public const int YARN_COLOR = 131;
        public const int YARN_SHADE = 112;
        public const int YARN_KNITTING_TYPE = 14;
        public const int YARN_GSM = 4;
        public const int CDA_ITEM_DYES = 251;
        public const int CDA_AGENT_DYES = 250;
        public const int CDA_ITEM_CHEM = 252;
        public const int CDA_AGENT_CHEM = 253;
        public const int FIBER_TYPE = 258;
        public const int BLEND_TYPE = 259;
        public const int YARN_PROGRAM = 260;
        public const int YARN_SUB_PROGRAM = 261;
        public const int MANUFACTURING_LINE = 262;
        public const int MANUFACTURING_PROCESS = 263;
        public const int MANUFACTURING_SUB_PROCESS = 264;
        public const int COLOR_GRADE = 265;
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
        public const string CommonInterfaceConfig = "CommonInterfaceConfig";
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
    public static class EntityConceptStatus
    {
        public const int RUNNING = 1089;
        public const int FIRM_CONCEPT = 1090;
        public const int LIVE_PRODUCT = 1091;
        public const int DROP = 1370;
    }
    public static class ItemSegmentNameConstants
    {
        public const string DYES_GROUP = "Dyes_Group";
        public const string DYES_ITEM_NAME = "Dyes_Item Name";
        public const string CHEMICALS_AGENT = "Chemicals_Agent";
        public const string CHEMICALS_FORM = "Chemicals_Form";
        public const string CHEMICALS_GROUP = "Chemicals_Group";
        public const string CHEMICALS_ITEM_NAME = "Chemicals_Item Name";
        public const string FABRIC_COLOR = "Fabric Color";
        public const string CONSTRUCTION = "Construction";
        public const string COMPOSITION = "Composition";
        public const string GSM = "GSM";

        #region Yarn Item Segments
        public const string YARN_COMPOSITION = "Yarn Composition Live";
        public const string YARN_TYPE = "Yarn Type Live";
        public const string YARN_MANUFACTURING_PROCESS = "Yarn Manufacturing Process Live";
        public const string YARN_MANUFACTURING_SUB_PROCESS = "Yarn Manufacturing Sub Process Live";
        public const string YARN_QUALITY_PARAMETER = "Yarn Quality Parameter Live";
        public const string SHADE = "Shade";
        public const string YARN_COUNT = "Yarn Count New";
        public const string YARN_COUNT_MASTER = "Yarn Count Master";
        public const string NO_OF_PLY = "No of Ply";

        public const string FIBER_TYPE = "Fiber Type";
        public const string BLEND_TYPE = "Blend Type";
        public const string YARN_PROGRAM = "Yarn Program";
        public const string YARN_SUB_PROGRAM = "Yarn Sub Program";
        public const string YARN_SUBPROGRAM_NEW = "Yarn Sub Program New";
        public const string YARN_CERTIFICATIONS = "Yarn Certifications";
        public const string YARN_MANUFACTURING_LINE = "Yarn Manufacturing Line";
        public const string YARN_COLOR = "Yarn Color";
        public const string YARN_COLOR_GRADE = "Yarn Color Grade";

        public const string FABRIC_TYPE = "Fabric Type";
        #endregion
    }
    public static class EntityTypeNameConstants
    {
        public const string STATUS = "Status";
        public const string MACHINE_KNITTING_TYPE = "Machine Knitting Type";
        public const string RM_TRIGGER_POINTS = "RM Trigger Points";
        public const string FABRIC_TYPE = "Fabric Type";
        public const string KNITTING_MACHINE_NATURE = "Knitting Machine Nature";
        public const string KNITTING_MACHINE_BRAND = "Knitting Machine Brand";
        public const string GREY_QC_MACHINE_PROCESS = "Grey QC Machine Process";
        public const string GREY_QC_MACHINE_MACHINE_TYPE = "Grey QC Machine Type";
        public const string GREY_QC_MACHINE_BRAND = "Grey QC Machine Brand";
        public const string DYEING_MACHINE_BRAND = "Dyeing Machine Brand";
        public const string CONCEPT_STATUS = "Concept Status";
        public const string CONFIRMATION_STATUS = "Confirmation Status";
    }
    public static class PRFromName
    {
        public const string CONCEPT = "Concept";
        public const string BDS = "BDS";
        public const string BULK_BOOKING = "Bulk Booking";
        public const string PROJECTION_YARN_BOOKING = "Projection Yarn Booking";
        public const string FABRIC_PROJECTION_YARN_BOOKING = "Fabric Projection Yarn Booking";
        public const string ROL_BASE_BOOKING = "ROL Base Booking";
    }
    public static class DB_TYPE
    {
        public const int textile = 1;
        public const int gmt = 2;

    }
    public static class ItemSegmentValueConstants
    {
        public const string ANIMAL_FIBER = "Animal Fiber [AF]";
        public const string BLENDED = "Blended";
        public const string COLOR_MELLANGE = "Color Mellange [CM]";
        public const string MAN_MADE_FIBER = "Man Made Fiber [MMF]";
        public const string NATURAL_FIBER = "Natural Fiber [NF]";
    }
    public static class PageNames
    {
        public const string CPR = "CPR";
        public const string FPR = "FPR";
        public const string YDBB = "YDBB";
        public const string MRSA = "MRSA";
        public const string MRSACK = "MRSACK";
        public const string YDMRSACK = "YDMRSACK";
    }
    public static class EnumDepertmentDescription
    {
        //All numbers are DB primary key value

        public const int None = 0;

        //19,35,31,38,11
        public const int Knitting = 19;
        public const int Operation = 35;
        public const int OperationTextile = 31;
        public const int PlanningMonitoringAndControl = 38;
        public const int ProductionManagementControl = 11;

        public const int ResearchAndDevelopment = 26;

        public const int SupplyChain = 10;

        public const int MerchandiserFabric = 25;
        public const int Merchandising = 40;

        public const int MarketingAndMerchandising = 4;
    }
    public static class PRFrom
    {
        public const int CONCEPT = 1;
        public const int BDS = 2;
        public const int BULK_BOOKING = 3;
        public const int PROJECTION_YARN_BOOKING = 4;
        public const int FABRIC_PROJECTION_YARN_BOOKING = 5;
    }
    public static class ContactCategoryConstants
    {
        public const int CONTACT_CATEGORY_BUYER = 1;
        public const int CONTACT_CATEGORY_SUPPLIER = 2;
        public const int CONTACT_CATEGORY_CONSIGNEE = 12;
        public const int CONTACT_CATEGORY_NOTIFY_PARTY = 5;
    }
  
    public enum EnumRackBinOperationType
    {
        None = 0,
        Addition = 1,
        Deduction = 2
    }
   

    public static class EntityTypeConstants
    {
        public const int PARTIAL_SHIPMENT = 20;
        public const int PORT_OF_LOADING = 38;
        public const int PORT_OF_DISCHARGE = 37;
        public const int CALCULATION_OF_TENURE = 72;
        public const int LC_TENURE = 73;
        public const int SHIPMENT_MODE = 41;
        public const int TRANS_SHIPMENT_ALLOW = 32;
        public const int BANK_ACCEPTANCE_FROM = 101;
        public const int MATURITY_CALCULATION = 80;
        public const int CI_DECLARATION = 99;
        public const int BI_DECLARATION = 100;
        public const int TRANSPORT_TYPE = 63;
        public const int TRANSPORT_MODE = 83;
        public const int TRANSPORT_AGENCY = 84;
        public const int AVAILABLE_WITH = 31;
        public const int SHIPMENT_STATUS = 85;
        public const int OFFER_VALIDITY = 74;
        public const int PAYMENT_INSTRUMENT = 26;
        public const int SERVICE_WO_APPLY = 64;
        public const int QUALITY_APPROVAL_PROCEDURE = 85;
        public const int YARN_QC_REQ_FOR = 106;
        public const int KNITTING_BRAND = 115;
        public const int KNITTING_TYPE = 110;
        public const int CDA_QC_REQ_FOR = 107;
        public const int CDA_SS_REQ_FOR = 200;
        public const int CDA_Floor_REQ_FOR = 201;
        public const int YARN_BRAND = 12;
        public const int YARN_SUB_BRAND = 42;
        public const int YARN_FIBER_TYPE = 116;
        public const int YARN_BLEND_TYPE = 117;
        public const int YARN_TYPE = 118;
        public const int YARN_PROGRAM = 119;
        public const int YARN_SUB_PROGRAM = 120;
        public const int YARN_CERTIFICATION = 121;
        public const int YARN_Q_PARAMETER = 122;
        public const int YARN_MANUFACTURING_LINE = 123;
        public const int YARN_MANUFACTURING_PROCESS = 124;
        public const int YARN_SUB_M_PROCESS = 125;
        public const int YARN_COLOR = 126;
        public const int YARN_COLOR_GRADE = 127;
        public const int RECIPE_DEFINITION_FOR = 132;
        public const int RECIPE_DEFINITION_PROCESS = 131;
        public const int RECIPE_DEFINITION_PARTICULARS = 133;
        public const int Roll_FINISHING_PROCESS = 134;
        public const int Roll_FINISHING_QC_STATUS = 135;
        public const int FLOOR_REQUISITION_STATUS = 136;
        public const int FIRM_CONCEPT_IMAGE = 139;
        public const int FIRM_CONCEPT_DECLARATION = 140;
        public const int DYEING_MACHINE_GROUP = 144;
        public const int DYEING_MACHINE_BRAND = 145;
        public const int DYEING_MACHINE_CAPACITY = 146;
        public const int DYEING_MACHINE_DYE_PROCESS = 147;
        public const int DYEING_MACHINE_FABRICATION = 148;
        public const int DYEING_FLOOR = 149;
        public const int DYEING_MACHINE_NOZZLE = 150;
        public const int DYEING_MACHINE_CC = 151;
        public const int FLOOR_REQUISITION_STATUS_FRESH = 1118;
        public const int FLOOR_REQUISITION_STATUS_RE_DYEING = 1119;
        public const int CONCEPT_FOR_STRUCTURE_BASE = 1092;
        public const int CONCEPT_FOR_COLOR_BASE = 1093;
        public const int FIRM_CONCEPT_IMAGE_PRODUCT = 1127;
        public const int FIRM_CONCEPT_IMAGE_CAM = 1128;
        public const int FIRM_CONCEPT_IMAGE_NEEDLE = 1129;
        public const int FIRM_CONCEPT_DECLARATION_YES = 1130;
        public const int FIRM_CONCEPT_DECLARATION_NO = 1131;
        public const int FIRM_CONCEPT_DECLARATION_REWORK = 1132;
        public const int FIRM_CONCEPT_DECLARATION_DECISION_PENDING = 1133;
        public const int TESTING_REQUIREMENT_TYPE = 153;
        public const int TESTING_REQUIREMENT_METHOD = 152;
        public const int TESTING_REQUIREMENT_DRYING = 154;
        public const int GREY_QC_MACHINE_CRITERIA = 155;
        public const int GREY_QC_MACHINE_PROCESS = 156;
        public const int GREY_QC_MACHINE_MACHINE_TYPE = 157;
        public const int GREY_QC_MACHINE_BRAND = 161;
        public const int DEFECTS_CATEGORY = 158;
        public const int DEFECT = 159;
        public const int DECISION = 160;
    }
    public static class EnumPBookingType //Also have a DB table named PBookingType
    {
        public const int None = 0;
        public const int Merchandising = 1;
        public const int Rnd = 2;
        public const int Textile = 3;
        public const int SupplyChain = 4;
        public const int MnM = 5;
    }

    public static class CacheKeys
    {
        public const string Yarn_Item_Segments = "YarnItemSegments";

    }

    public static class EnumSegmentType
    {
        public const int None = 0;
        public const int Composition = 1;
        public const int YarnType = 2;
        public const int Process = 3;
        public const int SubProcess = 4;
        public const int QualityParameter = 5;
        public const int Count = 6;
        public const int Fiber = 7;
        public const int SubProgram = 8;
        public const int Certification = 9;
    }
    public static class EnumBDSType
    {
        public const int Concept = 0;
        public const int BDS = 1;
        public const int Bulk = 2;
        public const int ProjectionBooking = 3;
    }
    public static class DateFormats
    {
        /// <summary>
        /// Default date format
        /// </summary>
        public const string DEFAULT_DATE_FORMAT = "dd-MMM-yyyy"; //"MM/dd/yyyy";

        /// <summary>
        /// Example: 02-Mar-2020
        /// </summary>
        public const string DATE_FORMAT_1 = "dd-MMM-yyyy";
    }
    public static class EnumTransectionType
    {
        public const int None = 0;
        public const int Receive = 1;
        public const int Issue = 2;
        public const int Return = 3;
        public const int Block = 4;
        public const int Request = 5;
        public const int CancelOrUnBlock = 6;
        public const int StockTransfer = 7;
        public const int LocationTransfer = 8;
    }
    public static class SubGroupNames
    {
        public const string YARNS = "Yarn Live";
        public const string DYES = "Dyes";
        public const string CHEMICALS = "Chemicals";
    }
}
