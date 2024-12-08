using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Gmt.General
{
    [Dapper.Contrib.Extensions.Table(TableNames.FABRIC_COLOR_BOOK_SETUP)]
    public class FabricColorBookSetup : DapperBaseEntity
    {
        [ExplicitKey]
        ///<summary>
        /// PTNID (Primary key)
        ///</summary>
        public int PTNID { get; set; }

        ///<summary>
        /// Color Id value from ItemSegmentValueId where segment name is 'Fabric Color'
        ///</summary>
        public int ColorID { get; set; }

        ///<summary>
        /// ColorSource (length: 10)
        ///</summary>
        public string ColorSource { get; set; }

        ///<summary>
        /// ColorCode (length: 50)
        ///</summary>
        public string ColorCode { get; set; }

        ///<summary>
        /// RGB (length: 20)
        ///</summary>
        public string RGBOrHex { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.PTNID > 0;
    }
}
