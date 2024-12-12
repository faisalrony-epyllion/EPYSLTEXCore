using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.SCD
{
    [Table(TableNames.YARN_CI_CHILD)]
    public class YarnCIChild : DapperBaseEntity
    {
        [ExplicitKey]
        public int ChildID { get; set; }

        ///<summary>
        /// CIID
        ///</summary>
        public int CIID { get; set; }

        ///<summary>
        /// ItemMasterID
        ///</summary>
        public int ItemMasterID { get; set; }

        public string ShadeCode { get; set; }

        ///<summary>
        /// ItemDescription
        ///</summary>
        public string ItemDescription { get; set; }

        ///<summary>
        /// UnitID
        ///</summary>
        public int UnitID { get; set; }

        ///<summary>
        /// NoOfCarton
        ///</summary>
        public int NoOfCarton { get; set; }

        ///<summary>
        /// NoOfCarton
        ///</summary>
        public int NoOfCone { get; set; }

        ///<summary>
        /// GrossWeight
        ///</summary>
        public decimal GrossWeight { get; set; }

        ///<summary>
        /// NetWeight
        ///</summary>
        public decimal NetWeight { get; set; }

        ///<summary>
        /// InvoiceQty
        ///</summary>
        public decimal? InvoiceQty { get; set; }

        ///<summary>
        /// InvoiceQty
        ///</summary>
        public decimal PIQty { get; set; }

        ///<summary>
        /// Rate
        ///</summary>
        public decimal? Rate { get; set; }

        ///<summary>
        /// PDValue
        ///</summary>
        public decimal PdValue { get; set; }

        ///<summary>
        /// PDValue
        ///</summary>
        public decimal PIValue { get; set; }

        /// <summary>
        /// Yarn Program Id
        /// </summary>
        public int YarnProgramId { get; set; }

        #region Additional Columns

        [Write(false)]
        public string YarnType { get; set; }

        [Write(false)]
        public string YarnComposition { get; set; }

        [Write(false)]
        public string YarnCount { get; set; }

        [Write(false)]
        public string YarnShade { get; set; }

        [Write(false)]
        public string YarnColor { get; set; }

        [Write(false)]
        public string DisplayUnitDesc { get; set; }

        [Write(false)]
        public decimal PIQtyN { get; set; }

        [Write(false)]
        public decimal PIRateN { get; set; }

        [Write(false)]
        public decimal PIValueN { get; set; }

        public string UOM = "Kg";

        [Write(false)]
        public string YarnSubProgramIDs { get; set; }

        [Write(false)]
        public string YarnSubProgramNames { get; set; }

        [Write(false)]
        public decimal BalPIQty { get; set; }
        [Write(false)]
        public decimal BalPIValue { get; set; }

        [Write(false)]
        public List<YarnCIChildYarnSubProgram> SubPrograms { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || ChildID > 0;

        #endregion Additional Columns

        public YarnCIChild()
        {
            YarnSubProgramIDs = "";
            ShadeCode = "";
            NoOfCarton = 0;
            GrossWeight = 0m;
            NetWeight = 0m;
            InvoiceQty = 0m;
            Rate = 0m;
            PdValue = 0m;
            ShadeCode = "";
            SubPrograms = new List<YarnCIChildYarnSubProgram>();
        }
    }
}
