using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities
{
    public class CommonInterfaceChildGrid : DapperBaseEntity
    {
        [ExplicitKey]
        public int ChildGridID { get; set; }
        public int ParentId { get; set; }

        public string ChildGridName { get; set; }

        public string ColumnNames { get; set; }

        public string ColumnHeaders { get; set; }

        public string ColumnAligns { get; set; }

        public string ColumnWidths { get; set; }

        public string HiddenColumns { get; set; }

        public string ColumnFilters { get; set; }

        public string ColumnTypes { get; set; }

        public string ColumnSortings { get; set; }

        public string PrimaryKeyColumn { get; set; }

        public string ParentColumn { get; set; }

        public string InsertSql { get; set; }

        public string UpdateSql { get; set; }

        public string TableName { get; set; }

        public string ConName { get; set; }

        [Write(false)]
        public override bool IsModified => ChildGridID > 0 || EntityState == System.Data.Entity.EntityState.Modified;
    }

}
