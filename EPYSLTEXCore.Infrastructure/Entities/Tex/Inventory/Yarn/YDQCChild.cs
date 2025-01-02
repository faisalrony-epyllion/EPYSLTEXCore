using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn
{
    [Table(TableNames.YD_QC_CHILD)]
    public class YDQCChild : DapperBaseEntity
    {
        [ExplicitKey]
        public int YDQCChildID { get; set; }
        ///<summary>
        /// YDQCMasterID
        ///</summary>
        public int YDQCMasterID { get; set; }

        public int YDBItemReqID { get; set; } = 0;
        ///<summary>
        /// SupplierID
        ///</summary>
        public int SupplierID { get; set; }

        ///<summary>
        /// SpinnerID
        ///</summary>
        public int SpinnerID { get; set; }

        ///<summary>
        /// LotNo (length: 50)
        ///</summary>
        public string LotNo { get; set; }

        ///<summary>
        /// UnitID
        ///</summary>
        public int UnitID { get; set; }

        ///<summary>
        /// ItemMasterID
        ///</summary>
        public int ItemMasterID { get; set; }

        ///<summary>
        /// YarnProgramID
        ///</summary>
        public int YarnProgramID { get; set; }

        ///<summary>
        /// BookingQty
        ///</summary>
        public int BookingQty { get; set; }

        ///<summary>
        /// ProducedQty
        ///</summary>
        public int ProducedQty { get; set; }

        ///<summary>
        /// ProductionQty
        ///</summary>
        public int ProductionQty { get; set; }

        public int ConeQty { get; set; } = 0;
        public int PacketQty { get; set; } = 0;

        ///<summary>
        /// Remarks (length: 200)
        ///</summary>
        public string Remarks { get; set; }

        ///<summary>
        /// QCPass
        ///</summary>
        public bool QCPass { get; set; }

        ///<summary>
        /// QCPassBy
        ///</summary>
        public int QCPassBy { get; set; }

        ///<summary>
        /// QCPassDate
        ///</summary>
        public DateTime? QCPassDate { get; set; }

        ///<summary>
        /// QCFail
        ///</summary>
        public bool QCFail { get; set; }

        ///<summary>
        /// QCFailBy
        ///</summary>
        public int QCFailBy { get; set; }

        ///<summary>
        /// QCFailDate
        ///</summary>
        public DateTime? QCFailDate { get; set; }

        ///<summary>
        /// ReTest
        ///</summary>
        public bool ReTest { get; set; }

        ///<summary>
        /// ReTestBy
        ///</summary>
        public int ReTestBy { get; set; }

        ///<summary>
        /// ReTestDate
        ///</summary>
        public DateTime? ReTestDate { get; set; }

        public int HardWindingChildID { get; set; } = 0;
        public int YDRICRBId { get; set; } = 0;
        // Foreign keys

        /// <summary>
        /// Parent YDQCMaster pointed by [YDQCChild].([YDQCMasterID]) (FK_YarnDyeingQCChild_YarnDyeingQCMaster)
        /// </summary>
        //public virtual YDQCMaster YDQCMaster { get; set; } // FK_YarnDyeingQCChild_YarnDyeingQCMaster
        #region Additional Columns

        [Write(false)]
        public string Uom { get; set; }
        [Write(false)]
        public string YarnType { get; set; }
        [Write(false)]
        public string YarnCount { get; set; }
        [Write(false)]
        public string YarnComposition { get; set; }
        [Write(false)]
        public string YarnShade { get; set; }
        [Write(false)]
        public string YarnColor { get; set; }
        [Write(false)]
        public int YDBatchID { get; set; } = 0;
        [Write(false)]
        public int YDBookingChildID { get; set; } = 0;
        [Write(false)]
        public int YDBookingMasterID { get; set; } = 0;
        [Write(false)]
        public string ShadeCode { get; set; } = "";
        [Write(false)]
        public int NoOfThread { get; set; } = 0;
        [Write(false)]
        public int ColorID { get; set; } = 0;
        [Write(false)]
        public string YarnCategory { get; set; } = "";
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YDQCChildID > 0;

        #endregion Additional Columns

        public YDQCChild()
        {
            SupplierID = 0;
            SpinnerID = 0;
            BookingQty = 0;
        }
    }
}
