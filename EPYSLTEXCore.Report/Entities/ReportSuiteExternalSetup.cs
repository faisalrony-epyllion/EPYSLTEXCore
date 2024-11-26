namespace EPYSLTEXCore.Report.Entities
{
    public class ReportSuiteExternalSetup 
    {
        ///<summary>
        /// SetupID (Primary key)
        ///</summary>
        public int Id { get; set; }

        public int Reportid { get; set; }

        public int ExternalId { get; set; }

        ///<summary>
        /// REPORT_NAME (length: 255)
        ///</summary>
        public string ReportName { get; set; }

        ///<summary>
        /// REPORT_PATH_NAME (length: 255)
        ///</summary>
        public string ReportPathName { get; set; }




        /// <summary>
        /// Parent ReportSuite pointed by [ReportSuiteExternalSetup].([Reportid]) (FK_ReportSuiteExternalSetup_ReportSuite)
        /// </summary>
        public virtual ReportSuite ReportSuite { get; set; }

        public ReportSuiteExternalSetup()
        {
           
        }
    }
}
