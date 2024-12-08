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
        public int ChildID { get; set; } = 0;
        public int ReceiveID { get; set; } = 0;
        //public int ItemMasterID { get; set; } = 0;
        public int InvoiceChildID { get; set; } = 0;
        public int POChildID { get; set; } = 0;
        public int CCID1 { get; set; } = 0;
        public int CCID2 { get; set; } = 0;
        public int CCID3 { get; set; } = 0;
        public int CCID4 { get; set; } = 0;
        public int CCID5 { get; set; } = 0;
        public int TSID1 { get; set; } = 0;
        public int TSID2 { get; set; } = 0;
        public int TSID3 { get; set; } = 0;
        public int TSID4 { get; set; } = 0;
        public int TSID5 { get; set; } = 0;
        public int OriginID { get; set; } = 0;
        public int DestinationID { get; set; } = 0;
        public int UnitID { get; set; } = 0;
        public decimal RelativeFactor { get; set; } = 0;
        public int SUnitID { get; set; } = 0;
        public decimal POQty { get; set; } = 0;
        public decimal SPOQty { get; set; } = 0;
        public decimal InvoiceQty { get; set; } = 0;
        public decimal SInvoiceQty { get; set; } = 0;
        public decimal ChallanQty { get; set; } = 0;
        public decimal SChallanQty { get; set; } = 0;
        public decimal InspQty { get; set; } = 0;
        public decimal SInspQty { get; set; } = 0;
        public decimal QCPassedQty { get; set; } = 0;
        public decimal SQCPassedQty { get; set; } = 0;
        public decimal QCFailedQty { get; set; } = 0;
        public decimal SQCFailedQty { get; set; } = 0;
        public decimal ExcessQty { get; set; } = 0;
        public decimal SExcessQty { get; set; } = 0;
        public decimal ShortQty { get; set; } = 0;
        public decimal SShortQty { get; set; } = 0;
        public decimal ReceiveQty { get; set; } = 0;
        public decimal SReceiveQty { get; set; } = 0;
        public decimal Rate { get; set; } = 0;
        public decimal AQL { get; set; } = 0;
        public decimal ActualAQL { get; set; } = 0;
        public decimal InspGoodQty { get; set; } = 0;
        public decimal ActualInspGoodQty { get; set; } = 0;
        public int PCID { get; set; } = 0;
        public string Remarks { get; set; } = "";
        public bool QCPass { get; set; } = false;
        public bool DN1 { get; set; } = false;
        public bool DN2 { get; set; } = false;
        public bool DN3 { get; set; } = false;
        public bool IsIssue { get; set; } = false;
        public bool IsReturn { get; set; } = false;
        public string LotNo { get; set; } = "";//Physical Lot
        public int NoOfCartoon { get; set; } = 0;
        public int NoOfCone { get; set; } = 0;
        public bool PartialAllocation { get; set; } = false;
        public bool CompleteAllocation { get; set; } = false;
        public int YarnProgramID { get; set; } = 0;
        public string ChallanCount { get; set; } = "";
        public string ChallanLot { get; set; } = "";
        public string PhysicalCount { get; set; } = "";
        public string ShadeCode { get; set; } = "";
        public bool IsNoTest { get; set; } = false;
        public string NoTestRemarks { get; set; } = "";
        public int NoTestBy { get; set; } = 0;
        public DateTime? NoTestDate { get; set; }
        public int SpinnerID { get; set; } = 0;
        public string YarnControlNo { get; set; } = "";
        public string YarnCategory { get; set; } = "";
        public int ReceiveForId { get; set; } = 0;
        public int YarnStockSetId { get; set; } = 0;
        public int TagYarnReceiveChildID { get; set; } = 0;
        public int TagBy { get; set; } = 0;
        public DateTime? TagDate { get; set; }
        public int YarnPackingID { get; set; } = 0;

        #region Additional Columns
        [Write(false)]
        public int PO_ItemMasterId { get; set; } = 0;
        [Write(false)]
        public EntityState EntityState { get; set; } = EntityState.Added;
        [Write(false)]
        public int TotalRows { get; set; } = 0;
        [Write(false)]
        public decimal TotalQty { get; set; } = 0;
        [Write(false)]
        public string YarnSubProgramIds { get; set; } = "";
        [Write(false)]
        public string DisplayUnitDesc { get; set; } = "";
        [Write(false)]
        public decimal PIQtyN { get; set; } = 0;
        [Write(false)]
        public string YarnSubProgramIDs { get; set; } = "";
        [Write(false)]
        public string YarnSubProgramNames { get; set; } = "";
        [Write(false)]
        public string YarnChildPoBuyerIds { get; set; } = "";
        [Write(false)]
        public string BuyerIds { get; set; } = "";
        [Write(false)]
        public string BuyerNames { get; set; } = "";
        [Write(false)]
        public string YarnChildPoExportIds { get; set; } = "";
        [Write(false)]
        public decimal BalanceQty { get; set; } = 0;
        [Write(false)]
        public string YarnLotNo { get; set; } = "";
        [Write(false)]
        public decimal ReceivedQty { get; set; } = 0;
        [Write(false)]
        public string YarnChildPoEWOs { get; set; } = "";
        [Write(false)]
        public string Segment1ValueDesc { get; set; } = "";
        [Write(false)]
        public string Segment2ValueDesc { get; set; } = "";
        [Write(false)]
        public string Segment3ValueDesc { get; set; } = "";
        [Write(false)]
        public string Segment4ValueDesc { get; set; } = "";
        [Write(false)]
        public string Segment5ValueDesc { get; set; } = "";
        [Write(false)]
        public string Segment6ValueDesc { get; set; } = "";
        [Write(false)]
        public string Segment7ValueDesc { get; set; } = "";
        [Write(false)]
        public string Segment8ValueDesc { get; set; } = "";
        [Write(false)]
        public string YarnDetail { get; set; } = "";
        [Write(false)]
        public int SupplierId { get; set; } = 0;
        [Write(false)]
        public int POForID { get; set; } = 0;
        [Write(false)]
        public string ReceiveForName { get; set; } = "";
        [Write(false)]
        public string POForName { get; set; } = "";
        [Write(false)]
        public decimal MaxReceiveQty { get; set; } = 0;
        [Write(false)]
        public bool IsNew => EntityState == EntityState.Added;
        [Write(false)]
        public bool IsModified => EntityState == EntityState.Modified;
        [Write(false)]
        public List<YarnReceiveChildRackBin> YarnReceiveChildRackBins { get; set; } = new List<YarnReceiveChildRackBin>();
        [Write(false)]
        public List<YarnReceiveChildSubProgram> SubPrograms { get; set; } = new List<YarnReceiveChildSubProgram>();
        [Write(false)]
        public List<YarnReceiveChildOrder> YarnReceiveChildOrders { get; set; } = new List<YarnReceiveChildOrder>();
        [Write(false)]
        public List<YarnReceiveChildBuyer> YarnReceiveChildBuyers { get; set; } = new List<YarnReceiveChildBuyer>();
        [Write(false)]
        public string SpinnerName { get; set; } = "";
        [Write(false)]
        public string PackNo { get; set; } = "";
        #endregion Additional Columns
    }
}
