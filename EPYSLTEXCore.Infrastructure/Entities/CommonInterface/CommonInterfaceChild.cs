using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities
{
    public class CommonInterfaceChild : DapperBaseEntity
    {
        [ExplicitKey]
        public int ChildID { get; set; }
        public int ParentId { get; set; }

        ///<summary>
        /// ColumnName (length: 200)
        ///</summary>
        public string ColumnName { get; set; }

        ///<summary>
        /// Label (length: 500)
        ///</summary>
        public string Label { get; set; }

        ///<summary>
        /// EntryType (length: 100)
        ///</summary>
        public string EntryType { get; set; }

        public int Scale { get; set; }

        public bool IsSys { get; set; }

        public bool IsRequired { get; set; }

        public bool IsHidden { get; set; }

        public bool IsEnable { get; set; }

        public string IdPrefix { get; set; }

        public decimal? Seq { get; set; }

        public string Tooltip { get; set; }

        public bool HasFinder { get; set; }

        public string FinderApiUrl { get; set; }

        ///<summary>
        /// FinderHeaderColumns (length: 500)
        ///</summary>
        public string FinderHeaderColumns { get; set; }

        ///<summary>
        /// FinderDisplayColumns (length: 500)
        ///</summary>
        public string FinderDisplayColumns { get; set; }

        public string FinderColumnAligns { get; set; }

        public string FinderColumnWidths { get; set; }

        public string FinderColumnSortings { get; set; }

        public string FinderFilterColumns { get; set; }

        ///<summary>
        /// FinderValueColumn (length: 100)
        ///</summary>
        public string FinderValueColumn { get; set; }

        ///<summary>
        /// FinderDisplayOthersColumn (length: 1000)
        ///</summary>
        public string FinderDisplayOthersColumn { get; set; }

        public bool HasSelectionChangeMethod { get; set; }
        
        ///<summary>
        /// ComboSelectionChangeMethodName (length: 500)
        ///</summary>
        public string SelectApiUrl { get; set; }

        /// <summary>
        /// Select SQL
        /// </summary>
        public string SelectSql { get; set; }

        public bool HasDependentColumn { get; set; }

        public string DependentColumnName { get; set; }

        public bool HasDefault { get; set; }

        ///<summary>
        /// DefaultValue (length: 500)
        ///</summary>
        public string DefaultValue { get; set; }

        ///<summary>
        /// ParameterValue (length: 500)
        ///</summary>
        public string ParameterValue { get; set; }

        /// <summary>
        /// Max length for string value
        /// </summary>
        public int MaxLength { get; set; }

        /// <summary>
        /// Finder Sql
        /// </summary>
        public string FinderSql { get; set; }

        ///<summary>
        /// ParameterValue (length: 500)
        ///</summary>
        public string MinRange { get; set; }

        ///<summary>
        /// ParameterValue (length: 500)
        ///</summary>
        public string MaxRange { get; set; }

        [Write(false)]
        public override bool IsModified => ChildID > 0 || EntityState == System.Data.Entity.EntityState.Modified;

        public CommonInterfaceChild()
        {
            Scale = 0;
            IsSys = false;
            IsRequired = false;
            IsHidden = true;
            IsEnable = true;
            HasFinder = false;
            HasSelectionChangeMethod = false;
            HasDefault = false;
        }
    }
}
