using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEX.Core.Entities.Tex
{
    public class CommonInterfaceChildGridColumn : DapperBaseEntity
    {
        [ExplicitKey]
        public int ChildGridColumnID { get; set; }
        /// <summary>
        /// Common Interface Master Id
        /// </summary>
        public int ParentId { get; set; }

        ///<summary>
        /// ColumnName (length: 200)
        ///</summary>
        public string ColumnName { get; set; }

        ///<summary>
        /// Label (length: 200)
        ///</summary>
        public string Label { get; set; }

        ///<summary>
        /// EntryType (length: 50)
        ///</summary>
        public string EntryType { get; set; }

        ///<summary>
        /// IsRequired
        ///</summary>
        public bool IsRequired { get; set; }

        ///<summary>
        /// IsHidden
        ///</summary>
        public bool IsHidden { get; set; }

        ///<summary>
        /// IsEnabled
        ///</summary>
        public bool IsEnabled { get; set; }

        ///<summary>
        /// Seq
        ///</summary>
        public decimal? Seq { get; set; }

        ///<summary>
        /// Tooltip
        ///</summary>
        public string Tooltip { get; set; }

        ///<summary>
        /// HasDependentColumn
        ///</summary>
        public bool HasDependentColumn { get; set; }

        ///<summary>
        /// DependentColumnName (length: 200)
        ///</summary>
        public string DependentColumnName { get; set; }

        ///<summary>
        /// SelectSql (length: 50)
        ///</summary>
        public string SelectSql { get; set; }

        ///<summary>
        /// HasDefault
        ///</summary>
        public bool HasDefault { get; set; }

        ///<summary>
        /// DefaultValue (length: 500)
        ///</summary>
        public string DefaultValue { get; set; }

        ///<summary>
        /// ParameterValue (length: 500)
        ///</summary>
        public string ParameterValue { get; set; }

        ///<summary>
        /// ParentColumnInMaster
        ///</summary>
        public bool ParentColumnInMaster { get; set; }

        ///<summary>
        /// ParentColumnName (length: 100)
        ///</summary>
        public string ParentColumnName { get; set; }

        ///<summary>
        /// MaxLength
        ///</summary>
        public int MaxLength { get; set; }
        public string ApiUrl { get; set; }
        public string ValueColumnName { get; set; }
        public string DisplayColumnName { get; set; }


        [Write(false)]
        public override bool IsModified => ChildGridColumnID > 0 || EntityState == System.Data.Entity.EntityState.Modified;
    }
}
