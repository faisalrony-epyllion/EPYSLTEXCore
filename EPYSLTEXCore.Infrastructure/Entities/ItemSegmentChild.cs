using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities
{
    [Table("ItemSegmentChild")]
    public class ItemSegmentChild : DapperBaseEntity
    {
        public int SegmentValueId { get; set; } = 0;
        public bool IsInactive { get; set; } = true;
        public string SegmentValueName { get; set; } = "";

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || SegmentValueId > 0;
    }
}
