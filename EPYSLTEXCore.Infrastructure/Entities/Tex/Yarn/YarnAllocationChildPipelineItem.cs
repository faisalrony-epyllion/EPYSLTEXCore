using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn
{
    [Table(TableNames.YARN_ALLOCATION_CHILD_PIPELINE_ITEM)]
    public class YarnAllocationChildPipelineItem : IDapperBaseEntity
    {
        [ExplicitKey]
        public int AllocationChildPLItemID { get; set; }
        public int AllocationChildID { get; set; } = 0;
        public string YarnCategory { get; set; } = "";
        public int YarnStockSetId { get; set; } = 0;
        public decimal PipelineAllocationQty { get; set; } = 0;
        public decimal TotalAllocationQty { get; set; } = 0;
        public int PreProcessRevNo { get; set; } = 0;
        public int RevisionNo { get; set; } = 0;
        public bool IsRevised { get; set; } = false;
        public int RevisionBy { get; set; } = 0;
        public DateTime? RevisionDate { get; set; }


        #region Additional Columns

        [Write(false)]
        public string NumericCount { get; set; } = "";
        [Write(false)]
        public string Spinner { get; set; } = "";
        [Write(false)]
        public string PhysicalCount { get; set; } = "";
        [Write(false)]
        public string PhysicalLot { get; set; } = "";
        [Write(false)]
        public int YarnAge { get; set; } = 0;
        [Write(false)]
        public decimal POPrice { get; set; } = 0;
        [Write(false)]
        public string TestResult { get; set; } = "";
        [Write(false)]
        public string TestResultComments { get; set; } = "";
        [Write(false)]
        public decimal PipelineStockQty { get; set; } = 0;
        [Write(false)]
        public string PONos { get; set; } = "";
        [Write(false)]
        public string Suppliers { get; set; } = "";
        [Write(false)]
        public string DeliveryEndDates { get; set; } = "";
        [Write(false)]
        public string DeliveryStartDates { get; set; } = "";
        [Write(false)]
        public string POFors { get; set; } = "";
        [Write(false)]
        public string POCompanys { get; set; } = "";
        [Write(false)]
        public string POPrices { get; set; } = "";
        [Write(false)]
        public EntityState EntityState { get; set; }
        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;
        [Write(false)]
        public bool IsModified => EntityState == EntityState.Modified;
        [Write(false)]
        public int TotalRows { get; set; }

        #endregion Additional Columns
        public YarnAllocationChildPipelineItem()
        {
            EntityState = EntityState.Added;

        }
    }
}
