﻿using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.General
{
    [Table(TableNames.SpinnerWiseYarnPackingHK)]
    public class SpinnerWiseYarnPackingHK : DapperBaseEntity
    {
        [ExplicitKey]
        public int YarnPackingID { get; set; }
        public int SpinnerID { get; set; }
        public string PackNo { get; set; }
        public int Cone { get; set; }
        public decimal NetWeight { get; set; }
        public int AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime DateUpdated { get; set; }


        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || YarnPackingID > 0;
        [Write(false)]
        public string Spinner { get; set; }
        [Write(false)]
        public List<Select2OptionModel> SpinnerList { get; set; }

        public SpinnerWiseYarnPackingHK()
        {
            YarnPackingID = 0;
        }
    }
}