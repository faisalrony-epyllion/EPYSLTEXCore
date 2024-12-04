using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex.RND
{
    [Table(TableNames.LIVE_PRODUCT_FORM)]
    public class LiveProductForm : DapperBaseEntity
    {
        [ExplicitKey]
        public int LPFormID { get; set; }

        public int FirmConceptMasterID { get; set; }

        public int FCItemID { get; set; }

        public int FormID { get; set; }

        public int QtyInPcs { get; set; }

        public decimal QtyinKG { get; set; }

        public int SubmitedQtyInPcs { get; set; }

        public decimal SubmitedQtyinKG { get; set; }
        public int ItemMasterID { get; set; }
        public int ColorID { get; set; }
        public string BoxNo { get; set; }
        public string Remarks { get; set; }
        public string ReferenceNo { get; set; }

        #region Additional Columns

        [Write(false)]
        public int ConceptID { get; set; }

        [Write(false)]
        public string ImagePath { get; set; }

        [Write(false)]
        public int FinalCompositionID { get; set; }

        [Write(false)]
        public string FinalComposition { get; set; }

        [Write(false)]
        public int FinalGSM { get; set; }

        [Write(false)]
        public string HangerRemarks { get; set; }

        [Write(false)]
        public List<LiveProductFormImage> LiveProductFormImages { get; set; }
        [Write(false)]
        public List<PriceRequestBuyer> PriceRequestBuyers { get; set; }

        [Write(false)]
        public override bool IsModified => EntityState == System.Data.Entity.EntityState.Modified || LPFormID > 0;

        [Write(false)]
        public string KnittingType { get; set; }

        [Write(false)]
        public string TechnicalName { get; set; }

        [Write(false)]
        public string CommercialName { get; set; }

        [Write(false)]
        public string Composition { get; set; }

        [Write(false)]
        public string Gsm { get; set; }

        [Write(false)]
        public string ConceptNo { get; set; }

        [Write(false)]
        public bool NeedSearch { get; set; }
        [Write(false)]
        public string ColorName { get; set; }
        [Write(false)]
        public int LPPRID { get; set; }
        [Write(false)]
        public DateTime? RequestDate { get; set; }
        [Write(false)]
        public DateTime? ValidUpToDate { get; set; }
        [Write(false)]
        public decimal Price { get; set; }
        [Write(false)]
        public int LPPRID1 { get; set; }
        [Write(false)]
        public bool IsBuyerSpecific { get; set; }
        [Write(false)]
        public int pageNo { get; set; }
        [Write(false)]
        public int perPageRecord { get; set; }
        [Write(false)]
        public List<DyeingBatchItemRoll> DyeingBatchItemRolls { get; set; }
        [Write(false)]
        public int BatchID { get; set; }
        #endregion Additional Columns

        public LiveProductForm()
        {
            QtyInPcs = 0;
            LiveProductFormImages = new List<LiveProductFormImage>();
            PriceRequestBuyers = new List<PriceRequestBuyer>();
            DyeingBatchItemRolls = new List<DyeingBatchItemRoll>();
            Remarks = "";
            HangerRemarks = "";
            NeedSearch = false;
            LPPRID = 0;
            LPPRID1 = 0;
            ColorName = "";
            RequestDate = null;
            ValidUpToDate = null;
            IsBuyerSpecific = false;
        }
    }

    //public class LiveProductFormValidator : AbstractValidator<LiveProductForm>
    //{
    //    public LiveProductFormValidator()
    //    {
    //        RuleFor(x => x.FirmConceptMasterID).NotEmpty().WithMessage("Firm Concept Master is required.");
    //        RuleFor(x => x.FormID).NotEmpty().WithMessage("Form ID is required.");
    //        RuleFor(x => x.QtyinKG).NotEmpty().WithMessage("Quantity in KG is required.");
    //    }
    //}
}
