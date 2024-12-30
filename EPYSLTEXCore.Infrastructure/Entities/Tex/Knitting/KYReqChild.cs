using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Knitting
{
    [Table(TableNames.KY_Req_Child)]
    public class KYReqChild: DapperBaseEntity
    {
        [ExplicitKey]
        public int KYReqChildID { get; set; }
        public int KYReqMasterID { get; set; }
        public int YarnProgramID { get; set; }
        public int UnitID { get; set; }
        public int ItemMasterID { get; set; }
        public string YarnCategory { get; set; }
        public string Remarks { get; set; }
        public decimal BookingQty { get; set; }
        public decimal ReqQty { get; set; }
        public int ConceptID { get; set; }
        public int FCMRMasterID { get; set; }
        public int FCMRChildID { get; set; }
        public string YarnLotNo { get; set; }
        public int YarnBrandID { get; set; }
        public int KPYarnID { get; set; }
        public decimal ReqCone { get; set; }
        public string PhysicalCount { get; set; }
        public string ShadeCode { get; set; }
        public string BatchNo { get; set; }
        public int PreProcessRevNo { get; set; }
        public int RevisionNo { get; set; }
        public bool InActive { get; set; }
        public int BookingID { get; set; }
        public int AllocationChildItemID { get; set; }

        #region Additional
        [Write(false)]
        public string Uom { get; set; }
        [Write(false)]
        public string YarnType { get; set; }
        [Write(false)]
        public string YarnCount { get; set; }
        [Write(false)]
        public string YarnComposition { get; set; }
        [Write(false)]
        public string Shade { get; set; }
        [Write(false)]
        public string YarnColor { get; set; }
        [Write(false)]
        public string YarnProgramName { get; set; }
        [Write(false)]
        public List<KYReqChildSubProgram> ChildSubPrograams { get; set; } = new List<KYReqChildSubProgram>();
        [Write(false)]
        public string YarnSubProgramIDs { get; set; }
        [Write(false)]
        public string YarnSubProgramNames { get; set; }
        [Write(false)]
        public string ConceptNo { get; set; }
        [Write(false)]
        public string DisplayUnitDesc { get; set; }

        [Write(false)]
        public string YarnBrand { get; set; }
        [Write(false)]
        public bool YD { get; set; }
        [Write(false)]
        public bool YDItem { get; set; }
        [Write(false)]
        public int FloorID { get; set; }
        [Write(false)]
        public string FloorName { get; set; }
        [Write(false)]
        public string GroupConceptNo { get; set; }
        [Write(false)]
        public DateTime ConceptDate { get; set; }
        [Write(false)]
        public int IsBDS { get; set; }
        [Write(false)]
        public decimal YarnReqQty { get; set; }
        [Write(false)]
        public decimal AllocatedQty { get; set; }
        [Write(false)]
        public decimal UsedQty { get; set; }
        [Write(false)]
        public decimal PendingQty { get; set; }
        [Write(false)]
        public decimal MaxReqQty { get; set; }
        [Write(false)]
        public EntityState EntityState { get; set; }
        [Write(false)]
        public int TotalRows { get; set; }
        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || KYReqChildID > 0;


        #endregion Additional


    }
}
