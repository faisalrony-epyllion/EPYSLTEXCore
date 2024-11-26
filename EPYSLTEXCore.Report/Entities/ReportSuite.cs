using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace EPYSLTEXCore.Report.Entities
{
    public class ReportSuite 
    {
        ///<summary>
        /// REPORTID (Primary key)
        ///</summary>
        public int Id { get; set; }

        ///<summary>
        /// PARENT_KEY
        ///</summary>
        public int? ParentKey { get; set; }

        ///<summary>
        /// NODE_TEXT (length: 200)
        ///</summary>
        public string NodeText { get; set; }

        ///<summary>
        /// REPORT_NAME (length: 255)
        ///</summary>
        public string ReportName { get; set; }

        ///<summary>
        /// REPORT_PATH_NAME (length: 255)
        ///</summary>
        public string ReportPathName { get; set; }

        ///<summary>
        /// REPORT_SQL (length: 8000)
        ///</summary>
        public string ReportSql { get; set; }

        public int SeqNo { get; set; }

        public bool IsVisible { get; set; }

        public bool HasDefaultValue { get; set; }

        public bool IsMultipleSelection { get; set; }

        public int ApplicationId { get; set; }

        public bool HasExternalReport { get; set; }

        public bool IsSessionUse { get; set; }

        public bool IsApi { get; set; }

        [NotMapped]
        public EntityState EntityState { get; set; }

        /// <summary>
        /// Child ReportSuiteColumnValues where [ReportSuiteColumnValue].[ReportID] point to this entity (FK_ReportSuiteColumnValue_ReportSuite)
        /// </summary>
        public virtual ICollection<ReportSuiteColumnValue> ReportSuiteColumnValues { get; set; }

        /// <summary>
        /// Child ReportSuiteExternalSetups where [ReportSuiteExternalSetup].[REPORTID] point to this entity (FK_ReportSuiteExternalSetup_ReportSuite)
        /// </summary>
        public virtual ICollection<ReportSuiteExternalSetup> ReportSuiteExternalSetups { get; set; }

        /// <summary>
        /// Child SecurityRuleReports where [SecurityRuleReport].[ReportID] point to this entity (FK_SecurityRuleReport_ReportSuite)
        /// </summary>
        

        public ReportSuite()
        {
            IsVisible = true;
            HasDefaultValue = false;
            IsMultipleSelection = true;
            ApplicationId = 0;
            HasExternalReport = false;
            IsSessionUse = false;
            IsApi = false;
            EntityState = EntityState.Added;
            ReportSuiteColumnValues = new List<ReportSuiteColumnValue>();
            ReportSuiteExternalSetups = new List<ReportSuiteExternalSetup>();
            
        }
    }


    public class CustomeParameter
    {
        public string ParameterName { get; set; }
        public string ParameterValue { get; set; }

    }
}
