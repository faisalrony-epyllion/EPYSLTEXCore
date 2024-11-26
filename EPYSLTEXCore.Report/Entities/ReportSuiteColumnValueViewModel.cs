namespace EPYSLTEXCore.Report.Entities
{
    public class ReportSuiteColumnValueViewModel 
    {
        /// <summary>
        /// ReportId
        /// </summary>
        public int ReportId { get; set; }

        ///<summary>
        /// ColumnID (length: 100)
        ///</summary>
        public string ColumnId { get; set; }

        ///<summary>
        /// DefaultValue (length: 100)
        ///</summary>
        public string DefaultValue { get; set; }

        ///<summary>
        /// Source (length: 150)
        ///</summary>
        public string Source { get; set; }

        ///<summary>
        /// DAOClass (length: 150)
        ///</summary>
        public string DaoClass { get; set; }

        ///<summary>
        /// MethodName (length: 150)
        ///</summary>
        public string MethodName { get; set; }

        ///<summary>
        /// ParameterName (length: 200)
        ///</summary>
        public string ParameterName { get; set; }

        public bool IsHidden { get; set; }

        ///<summary>
        /// ParentColumns (length: 200)
        ///</summary>
        public string ParentColumns { get; set; }

        ///<summary>
        /// ValueColumnID (length: 100)
        ///</summary>
        public string ValueColumnId { get; set; }

        ///<summary>
        /// DisplayColumnID (length: 100)
        ///</summary>
        public string DisplayColumnId { get; set; }

        public bool IsMultipleSelection { get; set; }

        ///<summary>
        /// DefaultValueDAOClass (length: 150)
        ///</summary>
        public string DefaultValueDaoClass { get; set; }

        ///<summary>
        /// DefaultValueMethodName (length: 150)
        ///</summary>
        public string DefaultValueMethodName { get; set; }

        ///<summary>
        /// DefaultValueColumnID (length: 100)
        ///</summary>
        public string DefaultValueColumnId { get; set; }

        public bool ShowAdditionalColumn { get; set; }

        ///<summary>
        /// AdditionalColumnID (length: 150)
        ///</summary>
        public string AdditionalColumnId { get; set; }

        ///<summary>
        /// AdditionalColumnHeader (length: 200)
        ///</summary>
        public string AdditionalColumnHeader { get; set; }

        ///<summary>
        /// ColumnWidth (length: 200)
        ///</summary>
        public string ColumnWidth { get; set; }
    }
}