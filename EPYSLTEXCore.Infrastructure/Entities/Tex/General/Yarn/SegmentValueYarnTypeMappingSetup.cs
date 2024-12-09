using Dapper.Contrib.Extensions;
using EPYSLTEXCore.Infrastructure.Data;
using TableAttribute = Dapper.Contrib.Extensions.TableAttribute;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.General.Yarn
{
    [Table("SegmentValueYarnTypeMappingSetup")]
    public class SegmentValueYarnTypeMappingSetup : DapperBaseEntity
    {
       
        [ExplicitKey]
        public int SegmentValueMappingID { get; set; }
        public int YarnTypeSVID { get; set; }
        public int ManufacturingProcessSVID { get; set; }
        public int SubProcessSVID { get; set; }
        public int QualityParameterSVID { get; set; }
        public string CountUnit { get; set; }
        public int AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime DateUpdated { get; set; }


        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || SegmentValueMappingID > 0;


        public SegmentValueYarnTypeMappingSetup()
        {
            SegmentValueMappingID = 0;
        }
    }
}
