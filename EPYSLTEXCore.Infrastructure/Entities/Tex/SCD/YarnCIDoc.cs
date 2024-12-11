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
    [Table(TableNames.YARN_CI_DOC)]
    public class YarnCIDoc : DapperBaseEntity
    {
        public YarnCIDoc()
        {
            YarnCIDocID = 0;
            CIID = 0;
            FileName = "";
            ImagePath = "";
            PreviewTemplate = "";
            DocTypeID = 0;

            IsDelete = false;
            DocTypeName = "";
        }

        [ExplicitKey]
        public int YarnCIDocID { get; set; }
        public int CIID { get; set; }
        public string FileName { get; set; }
        public string ImagePath { get; set; }
        public string PreviewTemplate { get; set; }
        public int DocTypeID { get; set; }

        #region Additional Property
        [Write(false)]
        public bool IsDelete { get; set; }
        [Write(false)]
        public string DocTypeName { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YarnCIDocID > 0;

        #endregion Additional Property
    }
}
