using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn
{
    [Table(TableNames.YARN_QC_RETURNRECEIVED_MASTER)]
    public class YarnQCReturnReceivedMaster : DapperBaseEntity
    {
        [ExplicitKey]
        public int QCReturnReceivedMasterID { get; set; }
        ///<summary>
        /// QCReturnReceivedDate
        ///</summary>
        public DateTime QCReturnReceivedDate { get; set; }

        ///<summary>
        /// QCReturnReceivedBy
        ///</summary>
        public int QCReturnReceivedBy { get; set; }

        ///<summary>
        /// QCReturnMasterID
        ///</summary>
        public int QCReturnMasterID { get; set; }

        ///<summary>
        /// QCReturnNo (length: 50)
        ///</summary>
        public string QCReturnNo { get; set; }

        ///<summary>
        /// QCReturnBy
        ///</summary>
        public int QCReturnBy { get; set; }

        ///<summary>
        /// QCReturnDate
        ///</summary>
        public DateTime QCReturnDate { get; set; }

        ///<summary>
        /// QCReqMasterID
        ///</summary>
        public int QCReqMasterID { get; set; }

        ///<summary>
        /// QCRemarksMasterID
        ///</summary>
        public int QCRemarksMasterID { get; set; }

        ///<summary>
        /// IsApprove
        ///</summary>
        public bool IsApprove { get; set; }

        ///<summary>
        /// ApproveDate
        ///</summary>
        public DateTime? ApproveDate { get; set; }

        ///<summary>
        /// ApproveBy
        ///</summary>
        public int? ApproveBy { get; set; }

        ///<summary>
        /// IsAcknowledge
        ///</summary>
        public bool IsAcknowledge { get; set; }

        ///<summary>
        /// AcknowledgeDate
        ///</summary>
        public DateTime? AcknowledgeDate { get; set; }

        ///<summary>
        /// AcknowledgeBy
        ///</summary>
        public int? AcknowledgeBy { get; set; }

        ///<summary>
        /// AddedBy
        ///</summary>
        public int AddedBy { get; set; }

        ///<summary>
        /// DateAdded
        ///</summary>
        public DateTime DateAdded { get; set; }

        ///<summary>
        /// UpdatedBy
        ///</summary>
        public int? UpdatedBy { get; set; }

        ///<summary>
        /// DateUpdated
        ///</summary>
        public DateTime? DateUpdated { get; set; }

        // Reverse navigation

        /// <summary>
        /// Child YarnQCReturnReceivedChilds where [YarnQCReturnReceivedChild].[QCReturnReceivedMasterID] point to this entity (FK_YarnQCReturnReceivedChild_YarnQCReturnReceivedMaster)
        /// </summary>
        // public virtual ICollection<YarnQCReturnReceivedChild> YarnQCReturnReceivedChilds { get; set; } // YarnQCReturnReceivedChild.FK_YarnQCReturnReceivedChild_YarnQCReturnReceivedMaster
        #region Additional Columns

        [Write(false)]
        public string QCReqFor { get; set; }
        [Write(false)]
        public DateTime QCReceiveDate { get; set; }
        [Write(false)]
        public string QCReceiveNo { get; set; }

        [Write(false)]
        public string ReceiveNo { get; set; }
        [Write(false)]
        public string QCIssueNo { get; set; }

        [Write(false)]
        public string QCReqNo { get; set; }
        [Write(false)]
        public DateTime QCReqDate { get; set; }
        [Write(false)]
        public int QCReqQty { get; set; }
        [Write(false)]
        public int QCReturnQty { get; set; }
        [Write(false)]
        public string QCReturnByUser { get; set; }
        [Write(false)]
        public int ReceiveQty { get; set; }
        [Write(false)]
        public int ReceiveQtyCarton { get; set; }
        [Write(false)]
        public int ReceiveQtyCone { get; set; }
        [Write(false)]
        public int ReturnQty { get; set; }
        [Write(false)]
        public int ReturnQtyCarton { get; set; }
        [Write(false)]
        public int ReturnQtyCone { get; set; }
        [Write(false)]
        public string ReturnReceivedByUser { get; set; }
        [Write(false)]
        public int UserId { get; set; }
        [Write(false)]
        public List<YarnQCReturnReceivedChild> Childs { get; set; }
        [Write(false)]
        public IEnumerable<Select2OptionModel> LocationList { get; set; }
        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || this.QCReturnReceivedMasterID > 0;

        #endregion Additional Columns
        public YarnQCReturnReceivedMaster()
        {
            IsApprove = false;
            IsAcknowledge = false;
            UserId = 0;
            QCReturnReceivedDate = DateTime.Now;
            Childs = new List<YarnQCReturnReceivedChild>();
        }
    }
    //#region Validators
    //public class YarnQCReturnReceivedMasterValidator : AbstractValidator<YarnQCReturnReceivedMaster>
    //{
    //    public YarnQCReturnReceivedMasterValidator()
    //    {
    //        RuleFor(x => x.QCReturnReceivedDate).NotEmpty();
    //        //RuleFor(x => x.QCRemarksMasterID).NotEmpty();
    //        RuleFor(x => x.QCReturnNo).NotEmpty();
    //        //RuleFor(x => x.Childs).Must(x => x.Count() > 0).WithMessage("You must add at least one Child Item.");
    //        //When(x => x.Childs.Any(), () =>
    //        //{
    //        //    RuleForEach(x => x.Childs).SetValidator(new YarnQCReturnReceivedChildBindingModelValidator());
    //        //});
    //    }
    //}


    //#endregion


}
