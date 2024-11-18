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

}
