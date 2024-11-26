namespace EPYSLTEXCore.Report.Entities
{
    public class FilterSets
    {
        public FilterSets()
        {
        }

        public string Caption { get; set; }
        public string ColumnName { get; set; }
        public string Operators { get; set; }
        public string ColumnValue { get; set; }
        public string OrAnd { get; set; }
        public string DataType { get; set; }
        public string TableName { get; set; }
        public bool IsParameter { get; set; }
        public bool IsSystemParameter { get; set; }
        public bool IsHidden { get; set; }
        public bool IsDefault { get; set; }
        public string DefaultValue { get; set; }
        public string Button { get; set; }
        public bool HasParent { get; set; }
        public string ParentColumn { get; set; }
        public string DAOClass { get; set; }
        public string MethodName { get; set; }
        public string ValueColumnId { get; set; }
        public string DisplayColumnId { get; set; }
        public bool IsMultipleSelection { get; set; }
        public bool ShowAdditionalColumn { get; set; }
        public string AdditionalColumnId { get; set; }
        public string AdditionalColumnHeader { get; set; }
        public string ColumnWidth { get; set; }
        public bool ExtraFilterColumn { get; set; }       
    }
}
