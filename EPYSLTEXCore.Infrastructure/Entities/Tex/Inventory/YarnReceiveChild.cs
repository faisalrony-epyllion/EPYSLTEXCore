using Dapper.Contrib.Extensions;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;
using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace EPYSLTEXCore.Infrastructure.Entities.Tex
{
    [Table(TableNames.YARN_RECEIVE_CHILD)]
    public class YarnReceiveChild : YarnItemMaster, IDapperBaseEntity
    {
        [ExplicitKey]
        public int ChildID { get; set; }
        public int ReceiveID { get; set; }
        //public int ItemMasterID { get; set; }
        public int InvoiceChildID { get; set; }
        public int POChildID { get; set; }
        public int CCID1 { get; set; }
        public int CCID2 { get; set; }
        public int CCID3 { get; set; }
        public int CCID4 { get; set; }
        public int CCID5 { get; set; }
        public int TSID1 { get; set; }
        public int TSID2 { get; set; }
        public int TSID3 { get; set; }
        public int TSID4 { get; set; }
        public int TSID5 { get; set; }
        public int OriginID { get; set; }
        public int DestinationID { get; set; }
        public int UnitID { get; set; }
        public decimal RelativeFactor { get; set; }
        public int? SUnitID { get; set; }
        public decimal POQty { get; set; }
        public decimal SPOQty { get; set; }
        public decimal InvoiceQty { get; set; }
        public decimal SInvoiceQty { get; set; }
        public decimal ChallanQty { get; set; }
        public decimal SChallanQty { get; set; }
        public decimal InspQty { get; set; }
        public decimal SInspQty { get; set; }
        public decimal QCPassedQty { get; set; }
        public decimal SQCPassedQty { get; set; }
        public decimal QCFailedQty { get; set; }
        public decimal SQCFailedQty { get; set; }
        public decimal ExcessQty { get; set; }
        public decimal SExcessQty { get; set; }
        public decimal ShortQty { get; set; }
        public decimal SShortQty { get; set; }
        public decimal ReceiveQty { get; set; }
        public decimal SReceiveQty { get; set; }
        public decimal Rate { get; set; }
        public decimal AQL { get; set; }
        public decimal ActualAQL { get; set; }
        public decimal InspGoodQty { get; set; }
        public decimal ActualInspGoodQty { get; set; }
        public int PCID { get; set; }
        public string Remarks { get; set; }
        public bool QCPass { get; set; }
        public bool DN1 { get; set; }
        public bool DN2 { get; set; }
        public bool DN3 { get; set; }
        public bool IsIssue { get; set; }
        public bool IsReturn { get; set; }
        public string LotNo { get; set; } //Physical Lot
        public int NoOfCartoon { get; set; }
        public int NoOfCone { get; set; }
        public bool PartialAllocation { get; set; }
        public bool CompleteAllocation { get; set; }
        public int YarnProgramID { get; set; }
        public string ChallanCount { get; set; }
        public string ChallanLot { get; set; }
        public string PhysicalCount { get; set; }
        public string ShadeCode { get; set; }
        public bool IsNoTest { get; set; }
        public string NoTestRemarks { get; set; }
        public int NoTestBy { get; set; }
        public DateTime? NoTestDate { get; set; }
        public int SpinnerID { get; set; }
        public string YarnControlNo { get; set; }
        public string YarnCategory { get; set; }
        public int ReceiveForId { get; set; }
        public int YarnStockSetId { get; set; } = 0;
        public int TagYarnReceiveChildID { get; set; } = 0;
        public int TagBy { get; set; } = 0;
        public DateTime? TagDate { get; set; }
        public int YarnPackingID { get; set; } = 0;

        #region Additional Columns
        [Write(false)]
        public int PO_ItemMasterId { get; set; } = 0;
        [Write(false)]
        public EntityState EntityState { get; set; }
        [Write(false)]
        public int TotalRows { get; set; }
        [Write(false)]
        public decimal TotalQty { get; set; }
        [Write(false)]
        public string YarnSubProgramIds { get; set; }

        [Write(false)]
        public string DisplayUnitDesc { get; set; }
        [Write(false)]
        public decimal PIQtyN { get; set; }
        [Write(false)]
        public string YarnSubProgramIDs { get; set; }
        [Write(false)]
        public string YarnSubProgramNames { get; set; }
        [Write(false)]
        public string YarnChildPoBuyerIds { get; set; }
        [Write(false)]
        public string BuyerIds { get; set; }
        [Write(false)]
        public string BuyerNames { get; set; }
        [Write(false)]
        public string YarnChildPoExportIds { get; set; }
        [Write(false)]
        public decimal BalanceQty { get; set; }
        [Write(false)]
        public string YarnLotNo { get; set; } = "";
        [Write(false)]
        public decimal ReceivedQty { get; set; }
        [Write(false)]
        public string YarnChildPoEWOs { get; set; }
        [Write(false)]
        public string Segment1ValueDesc { get; set; }
        [Write(false)]
        public string Segment2ValueDesc { get; set; }
        [Write(false)]
        public string Segment3ValueDesc { get; set; }
        [Write(false)]
        public string Segment4ValueDesc { get; set; }
        [Write(false)]
        public string Segment5ValueDesc { get; set; }
        [Write(false)]
        public string Segment6ValueDesc { get; set; }
        [Write(false)]
        public string Segment7ValueDesc { get; set; }
        [Write(false)]
        public string Segment8ValueDesc { get; set; }
        [Write(false)]
        public string YarnDetail { get; set; }
        [Write(false)]
        public int SupplierId { get; set; } = 0;
        [Write(false)]
        public int POForID { get; set; } = 0;
        [Write(false)]
        public string ReceiveForName { get; set; }
        [Write(false)]
        public string POForName { get; set; }
        [Write(false)]
        public decimal MaxReceiveQty { get; set; } = 0;
        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;
        [Write(false)]
        public bool IsModified => EntityState == EntityState.Modified;
        [Write(false)]
        public List<YarnReceiveChildRackBin> YarnReceiveChildRackBins { get; set; }
        [Write(false)]
        public List<YarnReceiveChildSubProgram> SubPrograms { get; set; }
        [Write(false)]
        public List<YarnReceiveChildOrder> YarnReceiveChildOrders { get; set; }
        [Write(false)]
        public List<YarnReceiveChildBuyer> YarnReceiveChildBuyers { get; set; }
        [Write(false)]
        public string SpinnerName { get; set; } = "";
        [Write(false)]
        public string PackNo { get; set; } = "";
        #endregion Additional Columns

        public YarnReceiveChild()
        {
            ItemMasterID = 0;
            InvoiceChildID = 0;
            CCID1 = 0;
            CCID2 = 0;
            CCID3 = 0;
            CCID4 = 0;
            CCID5 = 0;
            TSID1 = 0;
            TSID2 = 0;
            TSID3 = 0;
            TSID4 = 0;
            TSID5 = 0;
            Segment6ValueId = 0;
            OriginID = 0;
            RelativeFactor = 0m;
            SUnitID = 0;
            POQty = 0m;
            SPOQty = 0m;
            InvoiceQty = 0m;
            SInvoiceQty = 0m;
            ChallanQty = 0m;
            ChallanLot = "";
            SChallanQty = 0m;
            InspQty = 0m;
            SInspQty = 0m;
            QCPassedQty = 0m;
            SQCPassedQty = 0m;
            QCFailedQty = 0m;
            SQCFailedQty = 0m;
            ExcessQty = 0m;
            SExcessQty = 0m;
            ShortQty = 0m;
            SShortQty = 0m;
            ReceiveQty = 0m;
            SReceiveQty = 0m;
            Rate = 0m;
            AQL = 0m;
            ActualAQL = 0m;
            InspGoodQty = 0m;
            ActualInspGoodQty = 0m;
            PCID = 0;
            Remarks = "0";
            QCPass = false;
            DN1 = false;
            DN2 = false;
            DN3 = false;
            IsIssue = false;
            IsReturn = false;
            PartialAllocation = false;
            CompleteAllocation = false;
            YarnReceiveChildRackBins = new List<YarnReceiveChildRackBin>();
            SubPrograms = new List<YarnReceiveChildSubProgram>();
            YarnReceiveChildOrders = new List<YarnReceiveChildOrder>();
            YarnReceiveChildBuyers = new List<YarnReceiveChildBuyer>();
            IsNoTest = false;
            NoTestRemarks = "";
            NoTestBy = 0;
            YarnDetail = "";
            YarnControlNo = "";
            YarnChildPoBuyerIds = "";
            BuyerIds = "";
            BuyerNames = "";
            YarnChildPoExportIds = "";
            YarnChildPoEWOs = "";
            YarnCategory = "";
            ReceiveForId = 0;
        }
    }
}
