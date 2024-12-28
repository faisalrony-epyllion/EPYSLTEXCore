using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.DTOs;
using EPYSLTEX.Core.Statics;
using FluentValidation;
using System;
using System.Collections.Generic;

namespace EPYSLTEX.Core.Entities.Tex
{
    [Table("LabTestRequisitionSpecialFinishDyeMethod")]
    public class LabTestRequisitionSpecialFinishDyeMethod : DapperBaseEntity
    {
        public LabTestRequisitionSpecialFinishDyeMethod()
        {
            LTRFinishDyeMethodID = 0;
            LTReqBuyerID = 0;
            LTReqMasterID = 0;
            FinishDyeMethodID = 0;
            SeqNo = 0;
        }

        [ExplicitKey]
        public int LTRFinishDyeMethodID { get; set; }
        public int LTReqBuyerID { get; set; }
        public int LTReqMasterID { get; set; }
        public int FinishDyeMethodID { get; set; }
        public int SeqNo { get; set; }

        #region Additional Props
        [Write(false)]
        public string FinishDyeName { get; set; }
        [Write(false)]
        public string MethodType { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || LTRFinishDyeMethodID > 0;
        #endregion
    }
}
