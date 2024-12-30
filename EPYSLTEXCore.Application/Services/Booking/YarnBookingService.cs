using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Application.Interfaces.Booking;
using EPYSLTEXCore.Application.Interfaces.RND;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Booking;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using System.Data;
using System.Data.Entity;
using Microsoft.Data.SqlClient;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Admin;

namespace EPYSLTEXCore.Application.Services.Booking
{
    public class YarnBookingService : IYarnBookingService
    {
        private readonly IDapperCRUDService<YarnBookingMaster> _service;
        private readonly IDapperCRUDService<YarnBookingChild> _childService;
        private readonly IDapperCRUDService<YarnBookingChildItem> _childItemService;
        //private readonly IEntityTypeValueService _serviceEntityTypeValue;
        private readonly IFreeConceptService _serviceFreeConcept;
        private readonly IFreeConceptMRService _serviceFreeConceptMR;
        //private readonly ISignatureRepository _signatureRepository;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;
        private SqlTransaction transaction = null;
        private SqlTransaction transactionGmt = null;

        public YarnBookingService(IDapperCRUDService<YarnBookingMaster> service
            , IDapperCRUDService<YarnBookingChild> childService
            , IDapperCRUDService<YarnBookingChildItem> childItemService
            , IFreeConceptService serviceFreeConcept
            , IFreeConceptMRService serviceFreeConceptMR
            //, IEntityTypeValueService serviceEntityTypeValue
            //, ISignatureRepository signatureRepository
            )
        {
            _service = service;
            _childService = childService;
            _childItemService = childItemService;

            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;

            _serviceFreeConcept = serviceFreeConcept;
            _serviceFreeConceptMR = serviceFreeConceptMR;
        }

        public async Task<List<YarnBookingMaster>> GetPagedAsync(Status status, string PageName, PaginationInfo paginationInfo)
        {
            var orderBy = "";//string.IsNullOrEmpty(paginationInfo.OrderBy) ? "ORDER BY YBookingDate DESC" : paginationInfo.OrderBy;

            string sql = "";

            if (PageName == "YarnBooking")
            {
                if (status == Status.Pending)
                {
                    sql =
                    $@"
                    ;With EOM As (
                        Select ExportOrderID, ExportOrderNo, StyleMasterID, CalendarDays As TNADays
                        From {DbNames.EPYSL}..ExportOrderMaster
                        Where EWOStatusID = 130
                    ), BM As(
                        Select BM.BookingID, BM.BookingNo, BM.BuyerID, BM.BuyerTeamID, BM.CompanyID, BM.ExportOrderID,
                        1 SubGroupID,
                        BM.InHouseDate, ReferenceNo = ISNULL(BM.ReferenceNo,''), BM.SupplierID, BM.AddedBy, EOM.ExportOrderNo, EOM.StyleMasterID, EOM.TNADays, 0 WithoutOB
                        From {DbNames.EPYSL}..BookingMaster BM
                        Inner Join EOM On EOM.ExportOrderID = BM.ExportOrderID
                        Inner Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = BM.SupplierID
                        Where ISNULL(BM.IsCancel,0) = 0 And CAI.InHouse = 1 --And BM.SubGroupID in (1,11,12)
                        Group by BM.BookingID, BM.BookingNo, BM.BuyerID, BM.BuyerTeamID, BM.CompanyID, BM.ExportOrderID,
                        BM.InHouseDate, ISNULL(BM.ReferenceNo,''), BM.SupplierID, BM.AddedBy, EOM.ExportOrderNo, EOM.StyleMasterID, EOM.TNADays
                        Union All
                        Select BM.BookingID, BM.BookingNo, BM.BuyerID, BM.BuyerTeamID, 0 CompanyID, BM.ExportOrderID,
                        1 SubGroupID,
                        BM.InHouseDate, ReferenceNo = ISNULL(BM.ReferenceNo,''), BM.SupplierID, BM.AddedBy, EOM.ExportOrderNo, EOM.StyleMasterID, EOM.TNADays, 1 WithoutOB
                        From {DbNames.EPYSL}..SampleBookingMaster BM
                        Inner Join EOM On EOM.ExportOrderID = BM.ExportOrderID
	                    Inner Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = BM.SupplierID
                        Where ISNULL(BM.IsCancel,0) = 0 And BM.InHouse = 1 And BM.hasPayment = 1 --And BM.SubGroupID in (1,11,12)
                        Group by BM.BookingID, BM.BookingNo, BM.BuyerID, BM.BuyerTeamID, BM.ExportOrderID,
                        BM.InHouseDate, ISNULL(BM.ReferenceNo,''), SupplierID, BM.AddedBy, EOM.ExportOrderNo, EOM.StyleMasterID, EOM.TNADays
                    ), BIMG As(
                        Select BM.BookingNo, ImagePath, SL = ROW_NUMBER() Over(PARTITION BY BM.BookingNo Order By CI.ChildImgID)
                        From BM
                        Inner Join {DbNames.EPYSL}..BookingChildImage CI On CI.BookingID = BM.BookingID
                        Union All
                        Select BM.BookingNo, ImagePath, SL = ROW_NUMBER() Over(PARTITION BY BM.BookingNo Order By CI.ChildImgID)
                        From BM
                        Inner Join {DbNames.EPYSL}..SampleBookingChildImage CI On CI.BookingID = BM.BookingID
                    ), IMG As(
                        Select BookingNo, ImagePath
                        From BIMG
                        Where SL = 1
                    ), TmpFinal As(
                        Select FBA.BOMMasterID, Min(BM.BookingID) BookingID, BM.BookingNo, BM.BuyerID, BM.BuyerTeamID, BM.CompanyID, BM.ExportOrderID, BM.SubGroupID,
                            RequiredDate = BM.InHouseDate, BM.ReferenceNo, BM.SupplierID, BM.AddedBy, BM.ExportOrderNo, BM.StyleMasterID, BM.TNADays, BM.WithoutOB,
                            IG.SubGroupName GroupName, c.ShortName As BuyerName,   
                            CCT.TeamName BuyerDepartment, MerchandiserName = EMP.EmployeeName,FY.YearName,
                            RevStatus = Case When Isnull(EL.FabBTexAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBTexAckStatus,'') <> '' Then 1 Else 0 End, 0 As IsCompleteDelivery,
                            IMG.ImagePath
                        From BM
                        Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA On FBA.BookingID = BM.BookingID
                        Left JOIN {TableNames.YarnBookingMaster_New} YBM On YBM.BookingID = BM.BookingID
                        Left Join IMG On IMG.BookingNo = BM.BookingNo
                        Inner Join {DbNames.EPYSL}..StyleMaster SM On SM.StyleMasterID = BM.StyleMasterID
                        Inner Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = SM.BuyerTeamID
                        Inner Join {DbNames.EPYSL}..ItemSubGroup IG On IG.SubGroupID = BM.SubGroupID
                        Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = BM.ExportOrderID And EL.ContactID = BM.SupplierID And BM.BookingID = EL.BookingID  --And EL.ItemGroupID = ISG.ItemGroupID                                        
                        Inner Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = BM.AddedBy
                        Inner Join {DbNames.EPYSL}..Employee EMP ON EMP.EmployeeCode = LU.EmployeeCode
                        Inner Join {DbNames.EPYSL}..FinancialYear FY On FY.FinancialYearID = SM.FinancialYearID
                        Left Join {DbNames.EPYSL}..Contacts c On c.ContactID = BM.BuyerID
                        Where FBA.BookingID IS NOT NULL AND FBA.IsUnAcknowledge = 0 AND YBM.YBookingID IS NULL
                        Group By FBA.BOMMasterID,BM.BookingNo, BM.BuyerID, BM.BuyerTeamID, BM.CompanyID, BM.ExportOrderID, BM.SubGroupID,
                            BM.InHouseDate, BM.ReferenceNo, BM.SupplierID, BM.AddedBy, BM.ExportOrderNo, BM.StyleMasterID, BM.TNADays, BM.WithoutOB,
                            IG.SubGroupName, c.ShortName,   
                            CCT.TeamName, EMP.EmployeeName,FY.YearName,
                            Case When Isnull(EL.FabBTexAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBTexAckStatus,'') <> '' Then 1 Else 0 End,
                            IMG.ImagePath
                    )
                    Select *, Count(*) Over() TotalRows from TmpFinal ";
                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By BookingID Desc" : paginationInfo.OrderBy;
                }
                else if (status == Status.Report)
                {
                    sql =
                    $@"With RunningEWO As
                    (
	                    Select EWO.ExportOrderID,EWO.StyleMasterID, EWO.ExportOrderNo, EL.FabBTexAckStatus,EL.ContactID
	                    From {DbNames.EPYSL}..ExportOrderMaster EWO    
                        Left Join (
                            Select ExportOrderID, ContactID, BookingID,FabBTexAckStatus From (
                                Select ExportOrderID, ContactID, FabBTexAckStatus,BookingID 
                                From {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild 
                                Where GroupName in ('Fabric','Collar','Cuff')
                            ) A Group By ExportOrderID, ContactID, FabBTexAckStatus,BookingID
                        ) EL On EL.ExportOrderID = EWO.ExportOrderID
                        Inner Join {DbNames.EPYSL}..EntityTypeValue ETV On ETV.ValueID = EWO.EWOStatusID
	                    Where ETV.ValueName = 'Running'
	                    Group By EWO.ExportOrderID,EWO.StyleMasterID, EWO.ExportOrderNo, EL.FabBTexAckStatus,EL.ContactID
                    ),
                    BM As
                    (
	                    Select * from {DbNames.EPYSL}..BookingMaster
                    ),
                    FirstBM As
                    (
	                    Select BookingID = Min(BookingID), BookingNo 
	                    from BM Group By BookingNo
                    ),
                    BC As
                    (
                        Select BM.ExportOrderID,BC.BookingID,BM.SupplierID,BookingNo = BM.BookingNo,BM.BookingDate,
                        BookingRev = BM.RevisionNo, IsFabricCancel = ISNULL(BM.IsCancel,0), BM.BuyerID,
                        IsCompleteDelivery = Min(convert(int,BC.IsCompleteDelivery))
                        From BM
	                    Inner Join FirstBM FBM On FBM.BookingID = BM.BookingID
                        Inner Join {DbNames.EPYSL}..BookingChild BC On BC.BookingID = BM.BookingID
                        Left Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = BC.ContactID
                        Inner Join {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = BC.SubGroupID
                        Where ISG.SubGroupName in ('Fabric','Collar','Cuff') And ISNULL(CAI.InHouse,0) = 1
                        Group By BM.ExportOrderID,BM.SupplierID,BC.BookingID,BM.BookingNo,BM.BookingDate,
                        BM.RevisionNo,ISNULL(BM.IsCancel,0), BM.BuyerID
                    ),
                    StatusList As
                    (
	                    Select EWO.ExportOrderID,EWO.ExportOrderNo,BC.BookingNo,BuyerTeam = CCT.TeamName,FabricRevision = FBA.RevisionNo,
	                    SCAcknowledgeDate =Case When BIA.AcknowledgeDate Is Null Then '' Else  Convert(Varchar,BIA.AcknowledgeDate) END,
	                    SCAcknowledgeStatus = Case When BIA.Status = 1 Then 'Complete' Else 'Pending' END,
	                    PMCAcknowledgeDate =Case When FBA.AcknowledgeDate Is Null Then '' Else  Convert(Varchar,FBA.AcknowledgeDate) END,
	                    PMCAcknowledgeStatus = Case When FBA.Status = 1 Then 'Complete' Else 'Pending' END,
	                    YarnBookingDate = Case When YBM.ProposeDate Is Null Then Case When YBM.YBookingDate Is Null Then '' Else Convert(Varchar,YBM.YBookingDate) End Else Case When YBM.Propose = 1 then Convert(Varchar,YBM.ProposeDate) else '' end End,
	                    YarnAcknowledgeStatus = Case When YBM.Acknowledge = 1 Then 'Complete' Else 'Pending' END,
	                    RevStatus = Case When Isnull(EWO.FabBTexAckStatus,'') = 'Acknowledged' And Isnull(EWO.FabBTexAckStatus,'') <> '' Then 1 Else 0 End,
                        YBookingNo = ISNULL(YBM.YBookingNo,''), BC.IsFabricCancel, IsCancel = ISNULL(YBM.IsCancel,0),
                        BC.BuyerID, c.ShortName as BuyerName, IsCompleteDelivery
	                    From BC  	                                     
	                    Left Join RunningEWO EWO On BC.ExportOrderID = EWO.ExportOrderID And BC.SupplierID = EWO.ContactID 
	                    left Join {DbNames.EPYSL}..StyleMaster SM On SM.StyleMasterID = EWO.StyleMasterID
	                    left Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = SM.BuyerTeamID
	                    Left Join {DbNames.EPYSL}..BookingItemAcknowledge BIA On BIA.BookingID = BC.BookingID
	                    Left JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA On FBA.BookingID = BC.BookingID AND FBA.IsUnAcknowledge = 0
	                    Left JOIN {TableNames.YarnBookingMaster_New} YBM On YBM.BookingID = BC.BookingID and YBM.ExportOrderID = EWO.ExportOrderID
                        Left Join {DbNames.EPYSL}..Contacts c On c.ContactID = BC.BuyerID 
                    )  
                    , F As 
                    (
	                    Select ExportOrderID,ExportOrderNo,BookingNo,BuyerTeam,FabricRevision,SCAcknowledgeDate,SCAcknowledgeStatus,
                        PMCAcknowledgeDate,PMCAcknowledgeStatus, YarnBookingDate,YarnAcknowledgeStatus,RevStatus,YBookingNo, 
                        IsFabricCancel = Convert(bit,Max(Convert(Int,ISNULL(IsFabricCancel,0)))), 
                        IsCancel = Convert(bit,Max(Convert(Int,ISNULL(IsCancel,0)))), BuyerName, IsCompleteDelivery
	                    From StatusList Where YBookingNo != ''
	                    Group By ExportOrderID,ExportOrderNo,BookingNo,BuyerTeam,FabricRevision,SCAcknowledgeDate,SCAcknowledgeStatus,
                        PMCAcknowledgeDate,PMCAcknowledgeStatus, YarnBookingDate,YarnAcknowledgeStatus,RevStatus,YBookingNo, 
                        BuyerName, IsCompleteDelivery                     
                    )

                    Select *,  Count(*) Over() TotalRows From F";

                    if (paginationInfo.OrderBy.IsNullOrEmpty()) orderBy = "Order By ExportOrderNo Desc";
                }
                else
                {
                    if (status == Status.Revise)
                    {
                        sql = $@" With BIMG As(-------Bulk Booking Image
	                                Select BookingID, Min(ChildImgID) ChildImgID 
	                                From {DbNames.EPYSL}..BookingChildImage Group By BookingID
                                ), IMG As(
	                                Select I.BookingID, I.ImagePath
	                                From BIMG
	                                Inner Join {DbNames.EPYSL}..BookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                                ), SBIMG As(-------Sample Booking Image
	                                Select BookingID, Min(ChildImgID) ChildImgID 
	                                From {DbNames.EPYSL}..SampleBookingChildImage Group By BookingID
                                ), SIMG As(
	                                Select I.BookingID, I.ImagePath
	                                From SBIMG BIMG
	                                Inner Join {DbNames.EPYSL}..SampleBookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                                ),
                                FirstYBM As
                                (
	                                Select YBookingID = Min(YBookingID), YBookingNo 
	                                FROM {TableNames.YarnBookingMaster_New}
	                                Group By YBookingNo
                                ),
                                YBL As
                                (
	                                Select FBA.BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
                                    YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID,
                                    AcknowledgeStatus=Case When YB.Propose=0 Then 'N/A' When YB.Propose = 1 And YB.Acknowledge=0 Then 'Pending' Else 'Acknowledged' End, BM.BookingNo,EOM.ExportOrderNo,
	                                ReferenceNo = ISNULL(BM.ReferenceNo,''), EOM.CalendarDays As TNADays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID, 
	                                RequiredDate = ISNULL(YB.FabricInHouseDate, 
                                    Case When IGST.GoupSubTypeName = 'Main Fabric' Then EOM.FabricEDD When IGST.GoupSubTypeName = 'Sewing' Then EOM.SewingEDD When IGST.GoupSubTypeName = 'Finishing' Then EOM.SewingEDD Else EOM.FabricEDD End),
	                                YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, 
                                    YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB, 
                                    GroupName = Case When ISG.SubGroupName in ('Fabric','Collar','Cuff') then 'Fabric' Else '' End, 
                                    YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy, BM.IsSample, YB.YInHouseDate, 
                                    YB.DateRevised, IsCompleteDelivery = Min(convert(int, BC.IsCompleteDelivery)), ISNULL(IMG.ImagePath,'') ImagePath
	                                FROM {TableNames.YarnBookingMaster_New} YB
                                    Left Join IMG ON IMG.BookingID = YB.BookingID
	                                Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA On FBA.BookingID = YB.BookingID AND FBA.IsUnAcknowledge = 0
	                                Inner Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingID = YB.BookingID
                                    Inner Join {DbNames.EPYSL}..BookingChild BC On BC.BookingID = BM.BookingID
	                                Inner Join {DbNames.EPYSL}..ExportOrderMaster EOM On EOM.ExportOrderID = BM.ExportOrderID
	                                left Join {DbNames.EPYSL}..ItemGroup IG On IG.ItemGroupID = FBA.ItemGroupID
	                                left Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = FBA.SubGroupID
	                                left Join {DbNames.EPYSL}..ItemGroupSubType IGST On IGST.GroupSubTypeID = IG.GroupSubTypeID
	                                Where YB.WithoutOB = 0 AND EOM.EWOStatusID = 130 --And ISG.SubGroupName in ('Fabric','Collar','Cuff')
                                    And FBA.RevisionNo <> ISNULL(YB.PreProcessRevNo,0)
                                    Group By FBA.BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
	                                YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID, BM.BookingNo, EOM.ExportOrderNo,
	                                ISNULL(BM.ReferenceNo,''), EOM.CalendarDays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID, 
	                                YB.FabricInHouseDate, IGST.GoupSubTypeName, EOM.FabricEDD, EOM.SewingEDD, YB.HoldYP, YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, 
	                                YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB, 
                                    ISG.SubGroupName, YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy, BM.IsSample, YB.YInHouseDate, YB.DateRevised, ISNULL(IMG.ImagePath,'')

	                                Union All

	                                Select 0 BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
                                    YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID,
	                                AcknowledgeStatus=Case When YB.Propose=0 Then 'N/A' When YB.Propose = 1 And YB.Acknowledge=0 Then 'Pending' Else 'Acknowledged' End, 
                                    BM.BookingNo,EOM.ExportOrderNo,
	                                ReferenceNo = Case When ISNULL(BM.ReferenceNo,'') = '' then 'Sample' Else ISNULL(BM.ReferenceNo,'') End, EOM.CalendarDays As TNADays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID,
	                                RequiredDate = ISNULL(YB.FabricInHouseDate,BM.FirstInHouseDate), YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
	                                'Fabric' GroupName, YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy, IsSample = Convert(bit,1),
                                    YB.YInHouseDate, YB.DateRevised, 0 As IsCompleteDelivery, ISNULL(IMG.ImagePath,'') ImagePath
	 
		                            FROM {TableNames.YarnBookingMaster_New}  YB
	                                Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA On FBA.BookingID = YB.BookingID AND FBA.IsUnAcknowledge = 0								
	                                Inner Join {DbNames.EPYSL}..SampleBookingMaster BM On BM.BookingID = YB.BookingID
	                                Inner Join {DbNames.EPYSL}..ExportOrderMaster EOM On EOM.ExportOrderNo = BM.SLNo
                                    Left Join SIMG IMG ON IMG.BookingID = YB.BookingID
	                                Where YB.WithoutOB = 1 AND EOM.EWOStatusID = 130
                                    And FBA.RevisionNo <> ISNULL(YB.PreProcessRevNo,0)
                                    Group By YB.YBookingNo, YB.YBookingID, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
                                    YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID,
	                                Case When YB.Propose=0 Then 'N/A' When YB.Propose = 1 And YB.Acknowledge=0 Then 'Pending' Else 'Acknowledged' End, 
                                    BM.BookingNo,EOM.ExportOrderNo,
	                                Case When ISNULL(BM.ReferenceNo,'') = '' then 'Sample' Else ISNULL(BM.ReferenceNo,'') End, EOM.CalendarDays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID,
	                                ISNULL(YB.FabricInHouseDate,BM.FirstInHouseDate), YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
	                                YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy,
                                    YB.YInHouseDate, YB.DateRevised, ISNULL(IMG.ImagePath,'')
                                ),
                                TmpFinal As
                                (
                                    Select top 100 YB.BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, 
                                    YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, YB.AcknowledgeStatus, YB.ExportOrderID, 
                                    YB.BookingNo, YB.ExportOrderNo, CCT.TeamName BuyerDepartment,
                                    MerchandiserName = EMP.EmployeeName, ReferenceNo = ISNULL(YB.ReferenceNo,''), YB.TNADays, YB.AdditionalBooking, 
                                    YB.YBRevisionNeed, YB.RequiredDate, 
                                    RevStatus = Case When Isnull(EL.FabBTexAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBTexAckStatus,'') <> '' Then 1 Else Case When YB.IsSample = 1 then 1 else 0 End End, 
                                    YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, CancelReason = CR.ReasonName, YB.CanceledBy, 
                                    YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
                                    YB.GroupName, YB.ContactPerson, ContactPersonName = E.EmployeeName, Depertment = D.Designation +' : '+ ED.DepertmentDescription, 
                                    YB.Remarks, YB.YInHouseDate, YB.DateRevised, c.ShortName As BuyerName, IsCompleteDelivery, YB.ImagePath
                                    from YBL YB
                                    Inner Join FirstYBM FYBM On FYBM.YBookingID = YB.YBookingID
                                    Left Join {DbNames.EPYSL}..Employee E On E.EmployeeCode = YB.ContactPerson
                                    Left Join {DbNames.EPYSL}..EmployeeDepartment ED On ED.DepertmentID = E.DepertmentID
                                    Left Join {DbNames.EPYSL}..EmployeeDesignation D On D.DesigID = E.DesigID
                                    left Join {DbNames.EPYSL}..StyleMaster SM On SM.StyleMasterID = YB.StyleMasterID
                                    left Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = SM.BuyerTeamID
                                    Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = YB.ExportOrderID And EL.ContactID = YB.SupplierID And YB.BookingID = EL.BookingID 
                                    left Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = YB.AddedBy
                                    left Join {DbNames.EPYSL}..Employee EMP ON EMP.EmployeeCode = LU.EmployeeCode
                                    Left Join {DbNames.EPYSL}..CancelReason CR On CR.ReasonID = YB.CancelReasonID 
                                    Left Join {DbNames.EPYSL}..Contacts c On c.ContactID = YB.BuyerID

                                    GROUP BY YB.BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, 
                                    YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, YB.AcknowledgeStatus, YB.ExportOrderID, 
                                    YB.BookingNo, YB.ExportOrderNo, CCT.TeamName,
                                    EMP.EmployeeName, ISNULL(YB.ReferenceNo,''), YB.TNADays, YB.AdditionalBooking, 
                                    YB.YBRevisionNeed, YB.RequiredDate, 
                                    Case When Isnull(EL.FabBTexAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBTexAckStatus,'') <> '' Then 1 Else Case When YB.IsSample = 1 then 1 else 0 End End, 
                                    YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, CR.ReasonName, YB.CanceledBy, 
                                    YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
                                    YB.GroupName, YB.ContactPerson, E.EmployeeName, D.Designation,ED.DepertmentDescription, 
                                    YB.Remarks, YB.YInHouseDate, YB.DateRevised, c.ShortName, IsCompleteDelivery, YB.ImagePath
                                    Order By YB.YBookingID desc
                                )
                                Select *, Count(*) Over() TotalRows From TmpFinal ";
                    }
                    else if (status == Status.Completed)
                    {
                        sql = $@"
                            With BIMG As(-------Bulk Booking Image
	                            Select BookingID, Min(ChildImgID) ChildImgID 
	                            From {DbNames.EPYSL}..BookingChildImage Group By BookingID
                            ), IMG As(
	                            Select I.BookingID, I.ImagePath
	                            From BIMG
	                            left Join {DbNames.EPYSL}..BookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                            ), SBIMG As(-------Sample Booking Image
	                            Select BookingID, Min(ChildImgID) ChildImgID 
	                            From {DbNames.EPYSL}..SampleBookingChildImage Group By BookingID
                            ), SIMG As(
	                            Select I.BookingID, I.ImagePath
	                            From SBIMG BIMG
	                            left Join {DbNames.EPYSL}..SampleBookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                            ),
                            FirstYBM As
                            (
	                            Select YBookingID = Min(YBookingID), YBookingNo 
	                            FROM {TableNames.YarnBookingMaster_New}
	                            Group By YBookingNo
                            ),
                            YBL As
                            (
	                            Select FBA.BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
                                YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID,
                                AcknowledgeStatus=Case When YB.Propose=0 Then 'N/A' When YB.Propose = 1 And YB.Acknowledge=0 Then 'Pending' Else 'Acknowledged' End, FBA.BookingNo,EOM.ExportOrderNo,
	                            ReferenceNo = ISNULL(BM.ReferenceNo,''), EOM.CalendarDays As TNADays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID, 
	                            RequiredDate = ISNULL(YB.FabricInHouseDate, 
                                Case When IGST.GoupSubTypeName = 'Main Fabric' Then EOM.FabricEDD When IGST.GoupSubTypeName = 'Sewing' Then EOM.SewingEDD When IGST.GoupSubTypeName = 'Finishing' Then EOM.SewingEDD Else EOM.FabricEDD End),
	                            YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, 
                                YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB, 
                                GroupName = Case When ISG.SubGroupName in ('Fabric','Collar','Cuff') then 'Fabric' Else '' End, 
                                YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy, BM.IsSample, YB.YInHouseDate, 
                                YB.DateRevised, IsCompleteDelivery = Min(convert(int, BC.IsCompleteDelivery))
	                            FROM {TableNames.YarnBookingMaster_New} YB
	                            left JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA On FBA.BookingID = YB.BookingID AND FBA.IsUnAcknowledge = 0
	                            left Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingID = YB.BookingID
                                left Join {DbNames.EPYSL}..BookingChild BC On BC.BookingID = BM.BookingID
	                            left Join {DbNames.EPYSL}..ExportOrderMaster EOM On EOM.ExportOrderID = BM.ExportOrderID
	                            left Join {DbNames.EPYSL}..ItemGroup IG On IG.ItemGroupID = FBA.ItemGroupID
	                            left Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = FBA.SubGroupID
	                            left Join {DbNames.EPYSL}..ItemGroupSubType IGST On IGST.GroupSubTypeID = IG.GroupSubTypeID
	                            Where YB.WithoutOB = 0   
                                And FBA.RevisionNo = ISNULL(YB.PreProcessRevNo,0) And YB.Propose = 0
                                Group By FBA.BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
	                            YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID, FBA.BookingNo, EOM.ExportOrderNo,
	                            ISNULL(BM.ReferenceNo,''), EOM.CalendarDays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID, 
	                            YB.FabricInHouseDate, IGST.GoupSubTypeName, EOM.FabricEDD, EOM.SewingEDD, YB.HoldYP, YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, 
	                            YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB, 
                                ISG.SubGroupName, YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy, BM.IsSample, YB.YInHouseDate, YB.DateRevised

	                            UNION All

	                            Select 0 BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
                                YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID,
	                            AcknowledgeStatus=Case When YB.Propose=0 Then 'N/A' When YB.Propose = 1 And YB.Acknowledge=0 Then 'Pending' Else 'Acknowledged' End, 
                                FBA.BookingNo,EOM.ExportOrderNo,
	                            ReferenceNo = Case When ISNULL(BM.ReferenceNo,'') = '' then 'Sample' Else ISNULL(BM.ReferenceNo,'') End, EOM.CalendarDays As TNADays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID,
	                            RequiredDate = ISNULL(YB.FabricInHouseDate,BM.FirstInHouseDate), YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
	                            'Fabric' GroupName, YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy, IsSample = Convert(bit,1),
                                YB.YInHouseDate, YB.DateRevised, 0 As IsCompleteDelivery
	 
	                            FROM {TableNames.YarnBookingMaster_New} YB
	                            left JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA On FBA.BookingID = YB.BookingID AND FBA.IsUnAcknowledge = 0									
	                            left Join {DbNames.EPYSL}..SampleBookingMaster BM On BM.BookingID = YB.BookingID
	                            left Join {DbNames.EPYSL}..ExportOrderMaster EOM On EOM.ExportOrderNo = BM.SLNo
                                Left Join SIMG IMG ON IMG.BookingID = YB.BookingID
	                            Where YB.WithoutOB = 1
	                            And FBA.RevisionNo = ISNULL(YB.PreProcessRevNo,0) And YB.Propose = 0
                                Group By YB.YBookingNo, YB.YBookingID, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
                                YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID,
	                            Case When YB.Propose=0 Then 'N/A' When YB.Propose = 1 And YB.Acknowledge=0 Then 'Pending' Else 'Acknowledged' End, 
                                FBA.BookingNo,EOM.ExportOrderNo,
	                            Case When ISNULL(BM.ReferenceNo,'') = '' then 'Sample' Else ISNULL(BM.ReferenceNo,'') End, EOM.CalendarDays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID,
	                            ISNULL(YB.FabricInHouseDate,BM.FirstInHouseDate), YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
	                            YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy,
                                YB.YInHouseDate, YB.DateRevised
                            )
                            ,
                            TmpFinal As
                            (
                                Select top 100 YB.BOMMasterID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, 
                                YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, YB.AcknowledgeStatus, YB.ExportOrderID, 
                                YB.BookingNo, YB.ExportOrderNo, CCT.TeamName BuyerDepartment,
                                MerchandiserName = EMP.EmployeeName, ReferenceNo = ISNULL(YB.ReferenceNo,''), YB.TNADays, YB.AdditionalBooking, 
                                YB.YBRevisionNeed, YB.RequiredDate, 
                                RevStatus = Case When Isnull(EL.FabBTexAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBTexAckStatus,'') <> '' Then 1 Else Case When YB.IsSample = 1 then 1 else 0 End End, 
                                YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, CancelReason = CR.ReasonName, YB.CanceledBy, 
                                YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
                                YB.GroupName, YB.ContactPerson, ContactPersonName = E.EmployeeName, Depertment = D.Designation +' : '+ ED.DepertmentDescription, 
                                YB.Remarks, YB.YInHouseDate, YB.DateRevised, c.ShortName As BuyerName, IsCompleteDelivery
                                from YBL YB
                                left Join FirstYBM FYBM On FYBM.YBookingID = YB.YBookingID
                                Left Join {DbNames.EPYSL}..Employee E On E.EmployeeCode = YB.ContactPerson
                                Left Join {DbNames.EPYSL}..EmployeeDepartment ED On ED.DepertmentID = E.DepertmentID
                                Left Join {DbNames.EPYSL}..EmployeeDesignation D On D.DesigID = E.DesigID
                                left Join {DbNames.EPYSL}..StyleMaster SM On SM.StyleMasterID = YB.StyleMasterID
                                left Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = SM.BuyerTeamID
                                Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = YB.ExportOrderID And EL.ContactID = YB.SupplierID And YB.BookingID = EL.BookingID 
                                left Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = YB.AddedBy
                                left Join {DbNames.EPYSL}..Employee EMP ON EMP.EmployeeCode = LU.EmployeeCode
                                Left Join {DbNames.EPYSL}..CancelReason CR On CR.ReasonID = YB.CancelReasonID 
                                Left Join {DbNames.EPYSL}..Contacts c On c.ContactID = YB.BuyerID

                                GROUP BY YB.BOMMasterID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, 
                                YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, YB.AcknowledgeStatus, YB.ExportOrderID, 
                                YB.BookingNo, YB.ExportOrderNo, CCT.TeamName,
                                EMP.EmployeeName, ISNULL(YB.ReferenceNo,''), YB.TNADays, YB.AdditionalBooking, 
                                YB.YBRevisionNeed, YB.RequiredDate, 
                                Case When Isnull(EL.FabBTexAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBTexAckStatus,'') <> '' Then 1 Else Case When YB.IsSample = 1 then 1 else 0 End End, 
                                YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, CR.ReasonName, YB.CanceledBy, 
                                YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
                                YB.GroupName, YB.ContactPerson, E.EmployeeName, D.Designation,ED.DepertmentDescription, 
                                YB.Remarks, YB.YInHouseDate, YB.DateRevised, c.ShortName, IsCompleteDelivery
                            )
                            Select *, Count(*) Over() TotalRows From TmpFinal  
                        ";
                    }
                    else if (status == Status.Proposed)
                    {
                        sql = $@"With BIMG As(-------Bulk Booking Image
	                        Select BookingID, Min(ChildImgID) ChildImgID 
	                        From {DbNames.EPYSL}..BookingChildImage Group By BookingID
                        ), IMG As(
	                        Select I.BookingID, I.ImagePath
	                        From BIMG
	                        Inner Join {DbNames.EPYSL}..BookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                        ), SBIMG As(-------Sample Booking Image
	                        Select BookingID, Min(ChildImgID) ChildImgID 
	                        From {DbNames.EPYSL}..SampleBookingChildImage Group By BookingID
                        ), SIMG As(
	                        Select I.BookingID, I.ImagePath
	                        From SBIMG BIMG
	                        Inner Join {DbNames.EPYSL}..SampleBookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                        ),
                        FirstYBM As
                        (
	                        Select YBookingID = Min(YBookingID), YBookingNo 
	                        FROM {TableNames.YarnBookingMaster_New}
	                        Group By YBookingNo
                        ),
                        YBL As
                        (
	                        Select FBA.BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
                            YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID,
                            AcknowledgeStatus=Case When YB.Propose=0 Then 'N/A' When YB.Propose = 1 And YB.Acknowledge=0 Then 'Pending' Else 'Acknowledged' End, BM.BookingNo,EOM.ExportOrderNo,
	                        ReferenceNo = ISNULL(BM.ReferenceNo,''), EOM.CalendarDays As TNADays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID, 
	                        RequiredDate = ISNULL(YB.FabricInHouseDate, 
                            Case When IGST.GoupSubTypeName = 'Main Fabric' Then EOM.FabricEDD When IGST.GoupSubTypeName = 'Sewing' Then EOM.SewingEDD When IGST.GoupSubTypeName = 'Finishing' Then EOM.SewingEDD Else EOM.FabricEDD End),
	                        YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, 
                            YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB, 
                            GroupName = Case When ISG.SubGroupName in ('Fabric','Collar','Cuff') then 'Fabric' Else '' End, 
                            YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy, BM.IsSample, YB.YInHouseDate, 
                            YB.DateRevised, IsCompleteDelivery = Min(convert(int, BC.IsCompleteDelivery)), ISNULL(IMG.ImagePath,'') ImagePath
	                        FROM {TableNames.YarnBookingMaster_New} YB
                            Left Join IMG ON IMG.BookingID = YB.BookingID
	                        Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA On FBA.BookingID = YB.BookingID AND FBA.IsUnAcknowledge = 0
	                        Inner Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingID = YB.BookingID
                            Inner Join {DbNames.EPYSL}..BookingChild BC On BC.BookingID = BM.BookingID
	                        Inner Join {DbNames.EPYSL}..ExportOrderMaster EOM On EOM.ExportOrderID = BM.ExportOrderID
	                        left Join {DbNames.EPYSL}..ItemGroup IG On IG.ItemGroupID = FBA.ItemGroupID
	                        left Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = FBA.SubGroupID
	                        left Join {DbNames.EPYSL}..ItemGroupSubType IGST On IGST.GroupSubTypeID = IG.GroupSubTypeID
	                        Where YB.WithoutOB = 0 AND EOM.EWOStatusID = 130 --And ISG.SubGroupName in ('Fabric','Collar','Cuff') --And SM.BuyerID = 1 And SM.BuyerTeamID = 0  
                            And FBA.RevisionNo = ISNULL(YB.PreProcessRevNo,0) And YB.Propose = 1 And YB.Acknowledge = 0
                            Group By FBA.BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
	                        YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID, BM.BookingNo, EOM.ExportOrderNo,
	                        ISNULL(BM.ReferenceNo,''), EOM.CalendarDays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID, 
	                        YB.FabricInHouseDate, IGST.GoupSubTypeName, EOM.FabricEDD, EOM.SewingEDD, YB.HoldYP, YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, 
	                        YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB, 
                            ISG.SubGroupName, YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy, BM.IsSample, YB.YInHouseDate, YB.DateRevised, ISNULL(IMG.ImagePath,'')

	                        Union All

	                        Select 0 BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
                            YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID,
	                        AcknowledgeStatus=Case When YB.Propose=0 Then 'N/A' When YB.Propose = 1 And YB.Acknowledge=0 Then 'Pending' Else 'Acknowledged' End, 
                            BM.BookingNo,EOM.ExportOrderNo,
	                        ReferenceNo = Case When ISNULL(BM.ReferenceNo,'') = '' then 'Sample' Else ISNULL(BM.ReferenceNo,'') End, EOM.CalendarDays As TNADays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID,
	                        RequiredDate = ISNULL(YB.FabricInHouseDate,BM.FirstInHouseDate), YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
	                        'Fabric' GroupName, YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy, IsSample = Convert(bit,1),
                            YB.YInHouseDate, YB.DateRevised, 0 As IsCompleteDelivery, ISNULL(IMG.ImagePath,'') ImagePath
	 
			                FROM {TableNames.YarnBookingMaster_New}  YB
	                        Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA On FBA.BookingID = YB.BookingID AND FBA.IsUnAcknowledge = 0							
	                        Inner Join {DbNames.EPYSL}..SampleBookingMaster BM On BM.BookingID = YB.BookingID
	                        Inner Join {DbNames.EPYSL}..ExportOrderMaster EOM On EOM.ExportOrderNo = BM.SLNo
                            Left Join SIMG IMG ON IMG.BookingID = YB.BookingID
	                        Where YB.WithoutOB = 1 And EOM.EWOStatusID = 130 
                            And FBA.RevisionNo = ISNULL(YB.PreProcessRevNo,0) And YB.Propose = 1 And YB.Acknowledge = 0
                            Group By YB.YBookingNo, YB.YBookingID, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
                            YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID,
	                        Case When YB.Propose=0 Then 'N/A' When YB.Propose = 1 And YB.Acknowledge=0 Then 'Pending' Else 'Acknowledged' End, 
                            BM.BookingNo,EOM.ExportOrderNo,
	                        Case When ISNULL(BM.ReferenceNo,'') = '' then 'Sample' Else ISNULL(BM.ReferenceNo,'') End, EOM.CalendarDays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID,
	                        ISNULL(YB.FabricInHouseDate,BM.FirstInHouseDate), YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
	                        YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy,
                            YB.YInHouseDate, YB.DateRevised, ISNULL(IMG.ImagePath,'')
                        ),
                        TmpFinal As
                        (
                            Select top 100 YB.BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, 
                            YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, YB.AcknowledgeStatus, YB.ExportOrderID, 
                            YB.BookingNo, YB.ExportOrderNo, CCT.TeamName BuyerDepartment,
                            MerchandiserName = EMP.EmployeeName, ReferenceNo = ISNULL(YB.ReferenceNo,''), YB.TNADays, YB.AdditionalBooking, 
                            YB.YBRevisionNeed, YB.RequiredDate, 
                            RevStatus = Case When Isnull(EL.FabBTexAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBTexAckStatus,'') <> '' Then 1 Else Case When YB.IsSample = 1 then 1 else 0 End End, 
                            YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, CancelReason = CR.ReasonName, YB.CanceledBy, 
                            YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
                            YB.GroupName, YB.ContactPerson, ContactPersonName = E.EmployeeName, Depertment = D.Designation +' : '+ ED.DepertmentDescription, 
                            YB.Remarks, YB.YInHouseDate, YB.DateRevised, c.ShortName As BuyerName, IsCompleteDelivery, YB.ImagePath
                            from YBL YB
                            Inner Join FirstYBM FYBM On FYBM.YBookingID = YB.YBookingID
                            Left Join {DbNames.EPYSL}..Employee E On E.EmployeeCode = YB.ContactPerson
                            Left Join {DbNames.EPYSL}..EmployeeDepartment ED On ED.DepertmentID = E.DepertmentID
                            Left Join {DbNames.EPYSL}..EmployeeDesignation D On D.DesigID = E.DesigID
                            left Join {DbNames.EPYSL}..StyleMaster SM On SM.StyleMasterID = YB.StyleMasterID
                            left Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = SM.BuyerTeamID
                            Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = YB.ExportOrderID And EL.ContactID = YB.SupplierID And YB.BookingID = EL.BookingID 
                            left Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = YB.AddedBy
                            left Join {DbNames.EPYSL}..Employee EMP ON EMP.EmployeeCode = LU.EmployeeCode
                            Left Join {DbNames.EPYSL}..CancelReason CR On CR.ReasonID = YB.CancelReasonID 
                            Left Join {DbNames.EPYSL}..Contacts c On c.ContactID = YB.BuyerID

                            GROUP BY YB.BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, 
                            YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, YB.AcknowledgeStatus, YB.ExportOrderID, 
                            YB.BookingNo, YB.ExportOrderNo, CCT.TeamName,
                            EMP.EmployeeName, ISNULL(YB.ReferenceNo,''), YB.TNADays, YB.AdditionalBooking, 
                            YB.YBRevisionNeed, YB.RequiredDate, 
                            Case When Isnull(EL.FabBTexAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBTexAckStatus,'') <> '' Then 1 Else Case When YB.IsSample = 1 then 1 else 0 End End, 
                            YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, CR.ReasonName, YB.CanceledBy, 
                            YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
                            YB.GroupName, YB.ContactPerson, E.EmployeeName, D.Designation,ED.DepertmentDescription, 
                            YB.Remarks, YB.YInHouseDate, YB.DateRevised, c.ShortName, IsCompleteDelivery, YB.ImagePath
                            Order By YB.YBookingID desc
                        )
                        Select *, Count(*) Over() TotalRows From TmpFinal ";
                    }
                    else if (status == Status.Executed)
                    {
                        sql = $@"
                        With BIMG As(-------Bulk Booking Image
	                        Select BookingID, Min(ChildImgID) ChildImgID 
	                        From {DbNames.EPYSL}..BookingChildImage Group By BookingID
                        ), IMG As(
	                        Select I.BookingID, I.ImagePath
	                        From BIMG
	                        Inner Join {DbNames.EPYSL}..BookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                        ), SBIMG As(-------Sample Booking Image
	                        Select BookingID, Min(ChildImgID) ChildImgID 
	                        From {DbNames.EPYSL}..SampleBookingChildImage Group By BookingID
                        ), SIMG As(
	                        Select I.BookingID, I.ImagePath
	                        From SBIMG BIMG
	                        Inner Join {DbNames.EPYSL}..SampleBookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                        ),
                        FirstYBM As
                        (
	                        Select YBookingID = Min(YBookingID), YBookingNo 
	                        FROM {TableNames.YarnBookingMaster_New}
	                        Group By YBookingNo
                        ),
                        YBL As
                        (
	                        Select FBA.BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
                            YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID,
                            AcknowledgeStatus=Case When YB.Propose=0 Then 'N/A' When YB.Propose = 1 And YB.Acknowledge=0 Then 'Pending' Else 'Acknowledged' End, BM.BookingNo,EOM.ExportOrderNo,
	                        ReferenceNo = ISNULL(BM.ReferenceNo,''), EOM.CalendarDays As TNADays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID, 
	                        RequiredDate = ISNULL(YB.FabricInHouseDate, 
                            Case When IGST.GoupSubTypeName = 'Main Fabric' Then EOM.FabricEDD When IGST.GoupSubTypeName = 'Sewing' Then EOM.SewingEDD When IGST.GoupSubTypeName = 'Finishing' Then EOM.SewingEDD Else EOM.FabricEDD End),
	                        YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, 
                            YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB, 
                            GroupName = Case When ISG.SubGroupName in ('Fabric','Collar','Cuff') then 'Fabric' Else '' End, 
                            YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy, BM.IsSample, YB.YInHouseDate, 
                            YB.DateRevised, IsCompleteDelivery = Min(convert(int, BC.IsCompleteDelivery)), ISNULL(IMG.ImagePath,'') ImagePath
	                        FROM {TableNames.YarnBookingMaster_New} YB
                            Left Join IMG ON IMG.BookingID = YB.BookingID
	                        Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA On FBA.BookingID = YB.BookingID AND FBA.IsUnAcknowledge = 0
	                        Inner Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingID = YB.BookingID
                            Inner Join {DbNames.EPYSL}..BookingChild BC On BC.BookingID = BM.BookingID
	                        Inner Join {DbNames.EPYSL}..ExportOrderMaster EOM On EOM.ExportOrderID = BM.ExportOrderID
	                        left Join {DbNames.EPYSL}..ItemGroup IG On IG.ItemGroupID = FBA.ItemGroupID
	                        left Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = FBA.SubGroupID
	                        left Join {DbNames.EPYSL}..ItemGroupSubType IGST On IGST.GroupSubTypeID = IG.GroupSubTypeID
	                        Where YB.WithoutOB = 0 AND EOM.EWOStatusID = 130 --And ISG.SubGroupName in ('Fabric','Collar','Cuff')
                            And FBA.RevisionNo = ISNULL(YB.PreProcessRevNo,0) AND YB.Acknowledge = 1 
                            Group By FBA.BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
	                        YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID, BM.BookingNo, EOM.ExportOrderNo,
	                        ISNULL(BM.ReferenceNo,''), EOM.CalendarDays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID, 
	                        YB.FabricInHouseDate, IGST.GoupSubTypeName, EOM.FabricEDD, EOM.SewingEDD, YB.HoldYP, YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, 
	                        YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB, 
                            ISG.SubGroupName, YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy, BM.IsSample, YB.YInHouseDate, YB.DateRevised, ISNULL(IMG.ImagePath,'')

	                        Union All

	                        Select 0 BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
                            YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID,
	                        AcknowledgeStatus=Case When YB.Propose=0 Then 'N/A' When YB.Propose = 1 And YB.Acknowledge=0 Then 'Pending' Else 'Acknowledged' End, 
                            BM.BookingNo,EOM.ExportOrderNo,
	                        ReferenceNo = Case When ISNULL(BM.ReferenceNo,'') = '' then 'Sample' Else ISNULL(BM.ReferenceNo,'') End, EOM.CalendarDays As TNADays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID,
	                        RequiredDate = ISNULL(YB.FabricInHouseDate,BM.FirstInHouseDate), YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
	                        'Fabric' GroupName, YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy, IsSample = Convert(bit,1),
                            YB.YInHouseDate, YB.DateRevised, 0 As IsCompleteDelivery, ISNULL(IMG.ImagePath,'') ImagePath
	 
	                        FROM {TableNames.YarnBookingMaster_New}  YB
	                        Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA On FBA.BookingID = YB.BookingID AND FBA.IsUnAcknowledge = 0								
	                        Inner Join {DbNames.EPYSL}..SampleBookingMaster BM On BM.BookingID = YB.BookingID
	                        Inner Join {DbNames.EPYSL}..ExportOrderMaster EOM On EOM.ExportOrderNo = BM.SLNo
                            Left Join SIMG IMG ON IMG.BookingID = YB.BookingID
	                        Where YB.WithoutOB = 1 AND EOM.EWOStatusID = 130 --And ISG.SubGroupName in ('Fabric','Collar','Cuff')
                            And FBA.RevisionNo = ISNULL(YB.PreProcessRevNo,0) AND YB.Acknowledge = 1 
                            Group By YB.YBookingNo, YB.YBookingID, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
                            YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID,
	                        Case When YB.Propose=0 Then 'N/A' When YB.Propose = 1 And YB.Acknowledge=0 Then 'Pending' Else 'Acknowledged' End, 
                            BM.BookingNo,EOM.ExportOrderNo,
	                        Case When ISNULL(BM.ReferenceNo,'') = '' then 'Sample' Else ISNULL(BM.ReferenceNo,'') End, EOM.CalendarDays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID,
	                        ISNULL(YB.FabricInHouseDate,BM.FirstInHouseDate), YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
	                        YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy,
                            YB.YInHouseDate, YB.DateRevised, ISNULL(IMG.ImagePath,'')
                        ),
                        TmpFinal As
                        (
                            Select top 100 YB.BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, 
                            YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, YB.AcknowledgeStatus, YB.ExportOrderID, 
                            YB.BookingNo, YB.ExportOrderNo, CCT.TeamName BuyerDepartment,
                            MerchandiserName = EMP.EmployeeName, ReferenceNo = ISNULL(YB.ReferenceNo,''), YB.TNADays, YB.AdditionalBooking, 
                            YB.YBRevisionNeed, YB.RequiredDate, 
                            RevStatus = Case When Isnull(EL.FabBTexAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBTexAckStatus,'') <> '' Then 1 Else Case When YB.IsSample = 1 then 1 else 0 End End, 
                            YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, CancelReason = CR.ReasonName, YB.CanceledBy, 
                            YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
                            YB.GroupName, YB.ContactPerson, ContactPersonName = E.EmployeeName, Depertment = D.Designation +' : '+ ED.DepertmentDescription, 
                            YB.Remarks, YB.YInHouseDate, YB.DateRevised, c.ShortName As BuyerName, IsCompleteDelivery, YB.ImagePath
                            from YBL YB
                            Inner Join FirstYBM FYBM On FYBM.YBookingID = YB.YBookingID
                            Left Join {DbNames.EPYSL}..Employee E On E.EmployeeCode = YB.ContactPerson
                            Left Join {DbNames.EPYSL}..EmployeeDepartment ED On ED.DepertmentID = E.DepertmentID
                            Left Join {DbNames.EPYSL}..EmployeeDesignation D On D.DesigID = E.DesigID
                            left Join {DbNames.EPYSL}..StyleMaster SM On SM.StyleMasterID = YB.StyleMasterID
                            left Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = SM.BuyerTeamID
                            Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = YB.ExportOrderID And EL.ContactID = YB.SupplierID And YB.BookingID = EL.BookingID 
                            left Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = YB.AddedBy
                            left Join {DbNames.EPYSL}..Employee EMP ON EMP.EmployeeCode = LU.EmployeeCode
                            Left Join {DbNames.EPYSL}..CancelReason CR On CR.ReasonID = YB.CancelReasonID 
                            Left Join {DbNames.EPYSL}..Contacts c On c.ContactID = YB.BuyerID

                            GROUP BY YB.BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, 
                            YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, YB.AcknowledgeStatus, YB.ExportOrderID, 
                            YB.BookingNo, YB.ExportOrderNo, CCT.TeamName,
                            EMP.EmployeeName, ISNULL(YB.ReferenceNo,''), YB.TNADays, YB.AdditionalBooking, 
                            YB.YBRevisionNeed, YB.RequiredDate, 
                            Case When Isnull(EL.FabBTexAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBTexAckStatus,'') <> '' Then 1 Else Case When YB.IsSample = 1 then 1 else 0 End End, 
                            YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, CR.ReasonName, YB.CanceledBy, 
                            YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
                            YB.GroupName, YB.ContactPerson, E.EmployeeName, D.Designation,ED.DepertmentDescription, 
                            YB.Remarks, YB.YInHouseDate, YB.DateRevised, c.ShortName, IsCompleteDelivery, YB.ImagePath
                            Order By YB.YBookingID desc
                        )
                        Select *, Count(*) Over() TotalRows From TmpFinal";
                    }
                    else if (status == Status.Additional)
                    {
                        sql = $@"
                        With BIMG As(-------Bulk Booking Image
	                        Select BookingID, Min(ChildImgID) ChildImgID 
	                        From {DbNames.EPYSL}..BookingChildImage Group By BookingID
                        ), IMG As(
	                        Select I.BookingID, I.ImagePath
	                        From BIMG
	                        Inner Join {DbNames.EPYSL}..BookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                        ), SBIMG As(-------Sample Booking Image
	                        Select BookingID, Min(ChildImgID) ChildImgID 
	                        From {DbNames.EPYSL}..SampleBookingChildImage Group By BookingID
                        ), SIMG As(
	                        Select I.BookingID, I.ImagePath
	                        From SBIMG BIMG
	                        Inner Join {DbNames.EPYSL}..SampleBookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                        ),
                        FirstYBM As
                        (
	                        Select YBookingID = Min(YBookingID), YBookingNo 
	                        FROM {TableNames.YarnBookingMaster_New}
	                        Group By YBookingNo
                        ),
                        YBL As
                        (
	                        Select FBA.BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
                            YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID,
                            AcknowledgeStatus=Case When YB.Propose=0 Then 'N/A' When YB.Propose = 1 And YB.Acknowledge=0 Then 'Pending' Else 'Acknowledged' End, BM.BookingNo,EOM.ExportOrderNo,
	                        ReferenceNo = ISNULL(BM.ReferenceNo,''), EOM.CalendarDays As TNADays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID, 
	                        RequiredDate = ISNULL(YB.FabricInHouseDate, 
                            Case When IGST.GoupSubTypeName = 'Main Fabric' Then EOM.FabricEDD When IGST.GoupSubTypeName = 'Sewing' Then EOM.SewingEDD When IGST.GoupSubTypeName = 'Finishing' Then EOM.SewingEDD Else EOM.FabricEDD End),
	                        YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, 
                            YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB, 
                            GroupName = Case When ISG.SubGroupName in ('Fabric','Collar','Cuff') then 'Fabric' Else '' End, 
                            YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy, BM.IsSample, YB.YInHouseDate, 
                            YB.DateRevised, IsCompleteDelivery = Min(convert(int, BC.IsCompleteDelivery)), ISNULL(IMG.ImagePath,'') ImagePath
	                        FROM {TableNames.YarnBookingMaster_New} YB
                            Left Join IMG ON IMG.BookingID = YB.BookingID
	                        Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA On FBA.BookingID = YB.BookingID AND FBA.IsUnAcknowledge = 0
	                        Inner Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingID = YB.BookingID
                            Inner Join {DbNames.EPYSL}..BookingChild BC On BC.BookingID = BM.BookingID
	                        Inner Join {DbNames.EPYSL}..ExportOrderMaster EOM On EOM.ExportOrderID = BM.ExportOrderID
	                        left Join {DbNames.EPYSL}..ItemGroup IG On IG.ItemGroupID = FBA.ItemGroupID
	                        left Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = FBA.SubGroupID
	                        left Join {DbNames.EPYSL}..ItemGroupSubType IGST On IGST.GroupSubTypeID = IG.GroupSubTypeID
	                        Where YB.WithoutOB = 0 AND EOM.EWOStatusID = 130 --And ISG.SubGroupName in ('Fabric','Collar','Cuff')
                            And FBA.RevisionNo = ISNULL(YB.PreProcessRevNo,0) AND YB.Acknowledge = 1 
                            Group By FBA.BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
	                        YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID, BM.BookingNo, EOM.ExportOrderNo,
	                        ISNULL(BM.ReferenceNo,''), EOM.CalendarDays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID, 
	                        YB.FabricInHouseDate, IGST.GoupSubTypeName, EOM.FabricEDD, EOM.SewingEDD, YB.HoldYP, YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, 
	                        YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB, 
                            ISG.SubGroupName, YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy, BM.IsSample, YB.YInHouseDate, YB.DateRevised, ISNULL(IMG.ImagePath,'')

	                        Union All

	                        Select 0 BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
                            YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID,
	                        AcknowledgeStatus=Case When YB.Propose=0 Then 'N/A' When YB.Propose = 1 And YB.Acknowledge=0 Then 'Pending' Else 'Acknowledged' End, 
                            BM.BookingNo,EOM.ExportOrderNo,
	                        ReferenceNo = Case When ISNULL(BM.ReferenceNo,'') = '' then 'Sample' Else ISNULL(BM.ReferenceNo,'') End, EOM.CalendarDays As TNADays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID,
	                        RequiredDate = ISNULL(YB.FabricInHouseDate,BM.FirstInHouseDate), YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
	                        'Fabric' GroupName, YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy, IsSample = Convert(bit,1),
                            YB.YInHouseDate, YB.DateRevised, 0 As IsCompleteDelivery, ISNULL(IMG.ImagePath,'') ImagePath
	 
	                        FROM {TableNames.YarnBookingMaster_New}  YB
	                        Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA On FBA.BookingID = YB.BookingID AND FBA.IsUnAcknowledge = 0									
	                        Inner Join {DbNames.EPYSL}..SampleBookingMaster BM On BM.BookingID = YB.BookingID
	                        Inner Join {DbNames.EPYSL}..ExportOrderMaster EOM On EOM.ExportOrderNo = BM.SLNo
                            Left Join SIMG IMG ON IMG.BookingID = YB.BookingID
	                        Where YB.WithoutOB = 1 AND EOM.EWOStatusID = 130 --And ISG.SubGroupName in ('Fabric','Collar','Cuff')
                            And FBA.RevisionNo = ISNULL(YB.PreProcessRevNo,0) AND YB.Acknowledge = 1 
                            Group By YB.YBookingNo, YB.YBookingID, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
                            YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID,
	                        Case When YB.Propose=0 Then 'N/A' When YB.Propose = 1 And YB.Acknowledge=0 Then 'Pending' Else 'Acknowledged' End, 
                            BM.BookingNo,EOM.ExportOrderNo,
	                        Case When ISNULL(BM.ReferenceNo,'') = '' then 'Sample' Else ISNULL(BM.ReferenceNo,'') End, EOM.CalendarDays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID,
	                        ISNULL(YB.FabricInHouseDate,BM.FirstInHouseDate), YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
	                        YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy,
                            YB.YInHouseDate, YB.DateRevised, ISNULL(IMG.ImagePath,'')
                        ),
                        TmpFinal As
                        (
                            Select top 100 YB.BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, 
                            YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, YB.AcknowledgeStatus, YB.ExportOrderID, 
                            YB.BookingNo, YB.ExportOrderNo, CCT.TeamName BuyerDepartment,
                            MerchandiserName = EMP.EmployeeName, ReferenceNo = ISNULL(YB.ReferenceNo,''), YB.TNADays, YB.AdditionalBooking, 
                            YB.YBRevisionNeed, YB.RequiredDate, 
                            RevStatus = Case When Isnull(EL.FabBTexAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBTexAckStatus,'') <> '' Then 1 Else Case When YB.IsSample = 1 then 1 else 0 End End, 
                            YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, CancelReason = CR.ReasonName, YB.CanceledBy, 
                            YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
                            YB.GroupName, YB.ContactPerson, ContactPersonName = E.EmployeeName, Depertment = D.Designation +' : '+ ED.DepertmentDescription, 
                            YB.Remarks, YB.YInHouseDate, YB.DateRevised, c.ShortName As BuyerName, IsCompleteDelivery, YB.ImagePath
                            from YBL YB
                            Inner Join FirstYBM FYBM On FYBM.YBookingID = YB.YBookingID
                            Left Join {DbNames.EPYSL}..Employee E On E.EmployeeCode = YB.ContactPerson
                            Left Join {DbNames.EPYSL}..EmployeeDepartment ED On ED.DepertmentID = E.DepertmentID
                            Left Join {DbNames.EPYSL}..EmployeeDesignation D On D.DesigID = E.DesigID
                            left Join {DbNames.EPYSL}..StyleMaster SM On SM.StyleMasterID = YB.StyleMasterID
                            left Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = SM.BuyerTeamID
                            Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = YB.ExportOrderID And EL.ContactID = YB.SupplierID And YB.BookingID = EL.BookingID 
                            left Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = YB.AddedBy
                            left Join {DbNames.EPYSL}..Employee EMP ON EMP.EmployeeCode = LU.EmployeeCode
                            Left Join {DbNames.EPYSL}..CancelReason CR On CR.ReasonID = YB.CancelReasonID 
                            Left Join {DbNames.EPYSL}..Contacts c On c.ContactID = YB.BuyerID

                            GROUP BY YB.BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, 
                            YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, YB.AcknowledgeStatus, YB.ExportOrderID, 
                            YB.BookingNo, YB.ExportOrderNo, CCT.TeamName,
                            EMP.EmployeeName, ISNULL(YB.ReferenceNo,''), YB.TNADays, YB.AdditionalBooking, 
                            YB.YBRevisionNeed, YB.RequiredDate, 
                            Case When Isnull(EL.FabBTexAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBTexAckStatus,'') <> '' Then 1 Else Case When YB.IsSample = 1 then 1 else 0 End End, 
                            YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, CR.ReasonName, YB.CanceledBy, 
                            YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
                            YB.GroupName, YB.ContactPerson, E.EmployeeName, D.Designation,ED.DepertmentDescription, 
                            YB.Remarks, YB.YInHouseDate, YB.DateRevised, c.ShortName, IsCompleteDelivery, YB.ImagePath
                            Order By YB.YBookingID desc
                        )
                        Select *, Count(*) Over() TotalRows From TmpFinal";
                    }
                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YBookingDate Desc" : paginationInfo.OrderBy;
                }
            }
            else
            {
                if (status == Status.AwaitingPropose)
                {
                    sql = $@"With BIMG As(-------Bulk Booking Image
	                            Select BookingID, Min(ChildImgID) ChildImgID 
	                            From {DbNames.EPYSL}..BookingChildImage Group By BookingID
                            ), IMG As(
	                            Select I.BookingID, I.ImagePath
	                            From BIMG
	                            Inner Join {DbNames.EPYSL}..BookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                            ), SBIMG As(-------Sample Booking Image
	                            Select BookingID, Min(ChildImgID) ChildImgID 
	                            From {DbNames.EPYSL}..SampleBookingChildImage Group By BookingID
                            ), SIMG As(
	                            Select I.BookingID, I.ImagePath
	                            From SBIMG BIMG
	                            Inner Join {DbNames.EPYSL}..SampleBookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                            ),
                            FirstYBM As
                            (
	                            Select YBookingID = Min(YBookingID), YBookingNo 
	                            FROM {TableNames.YarnBookingMaster_New}
	                            Group By YBookingNo
                            ),
                            YBL As
                            (
	                            Select FBA.BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
                                YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID,
                                AcknowledgeStatus=Case When YB.Propose=0 Then 'N/A' When YB.Propose = 1 And YB.Acknowledge=0 Then 'Pending' Else 'Acknowledged' End, BM.BookingNo,EOM.ExportOrderNo,
	                            ReferenceNo = ISNULL(BM.ReferenceNo,''), EOM.CalendarDays As TNADays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID, 
	                            RequiredDate = ISNULL(YB.FabricInHouseDate, 
                                Case When IGST.GoupSubTypeName = 'Main Fabric' Then EOM.FabricEDD When IGST.GoupSubTypeName = 'Sewing' Then EOM.SewingEDD When IGST.GoupSubTypeName = 'Finishing' Then EOM.SewingEDD Else EOM.FabricEDD End),
	                            YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, 
                                YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB, 
                                GroupName = Case When ISG.SubGroupName in ('Fabric','Collar','Cuff') then 'Fabric' Else '' End, 
                                YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy, BM.IsSample, YB.YInHouseDate, 
                                YB.DateRevised, IsCompleteDelivery = Min(convert(int, BC.IsCompleteDelivery)), ISNULL(IMG.ImagePath,'') ImagePath
	                            FROM {TableNames.YarnBookingMaster_New} YB
                                Left Join IMG ON IMG.BookingID = YB.BookingID
	                            Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA On FBA.BookingID = YB.BookingID AND FBA.IsUnAcknowledge = 0
	                            Inner Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingID = YB.BookingID
                                Inner Join {DbNames.EPYSL}..BookingChild BC On BC.BookingID = BM.BookingID
	                            Inner Join {DbNames.EPYSL}..ExportOrderMaster EOM On EOM.ExportOrderID = BM.ExportOrderID
	                            left Join {DbNames.EPYSL}..ItemGroup IG On IG.ItemGroupID = FBA.ItemGroupID
	                            left Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = FBA.SubGroupID
	                            left Join {DbNames.EPYSL}..ItemGroupSubType IGST On IGST.GroupSubTypeID = IG.GroupSubTypeID
	                            Where YB.WithoutOB = 0 AND EOM.EWOStatusID = 130 
                                And FBA.RevisionNo = ISNULL(YB.PreProcessRevNo,0) AND FBA.IsApprovedByPMC = 1 AND YB.Propose = 1 And YB.Acknowledge = 0
                                Group By FBA.BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
	                            YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID, BM.BookingNo, EOM.ExportOrderNo,
	                            ISNULL(BM.ReferenceNo,''), EOM.CalendarDays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID, 
	                            YB.FabricInHouseDate, IGST.GoupSubTypeName, EOM.FabricEDD, EOM.SewingEDD, YB.HoldYP, YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, 
	                            YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB, 
                                ISG.SubGroupName, YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy, BM.IsSample, YB.YInHouseDate, YB.DateRevised, ISNULL(IMG.ImagePath,'')

	                            Union All

	                            Select 0 BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
                                YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID,
	                            AcknowledgeStatus=Case When YB.Propose=0 Then 'N/A' When YB.Acknowledge=0 Then 'Pending' Else 'Acknowledged' End, 
                                BM.BookingNo,EOM.ExportOrderNo,
	                            ReferenceNo = Case When ISNULL(BM.ReferenceNo,'') = '' then 'Sample' Else ISNULL(BM.ReferenceNo,'') End, EOM.CalendarDays As TNADays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID,
	                            RequiredDate = ISNULL(YB.FabricInHouseDate,BM.FirstInHouseDate), YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
	                            'Fabric' GroupName, YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy, IsSample = Convert(bit,1),
                                YB.YInHouseDate, YB.DateRevised, 0 As IsCompleteDelivery, ISNULL(IMG.ImagePath,'') ImagePath
	 
	                            FROM {TableNames.YarnBookingMaster_New}  YB
	                            Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA On FBA.BookingID = YB.BookingID AND FBA.IsUnAcknowledge = 0							
	                            Inner Join {DbNames.EPYSL}..SampleBookingMaster BM On BM.BookingID = YB.BookingID
	                            Inner Join {DbNames.EPYSL}..ExportOrderMaster EOM On EOM.ExportOrderNo = BM.SLNo
                                Left Join SIMG IMG ON IMG.BookingID = YB.BookingID
	                            Where YB.WithoutOB = 1 And EOM.EWOStatusID = 130 
                                And FBA.RevisionNo = ISNULL(YB.PreProcessRevNo,0) AND YB.Propose = 1 And YB.Acknowledge = 0
                                Group By YB.YBookingNo, YB.YBookingID, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
                                YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID,
	                            Case When YB.Propose=0 Then 'N/A' When YB.Acknowledge=0 Then 'Pending' Else 'Acknowledged' End, 
                                BM.BookingNo,EOM.ExportOrderNo,
	                            Case When ISNULL(BM.ReferenceNo,'') = '' then 'Sample' Else ISNULL(BM.ReferenceNo,'') End, EOM.CalendarDays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID,
	                            ISNULL(YB.FabricInHouseDate,BM.FirstInHouseDate), YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
	                            YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy,
                                YB.YInHouseDate, YB.DateRevised, ISNULL(IMG.ImagePath,'')
                            ),
                            TmpFinal As
                            (
                                Select top 100 YB.BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, 
                                YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, YB.AcknowledgeStatus, YB.ExportOrderID, 
                                YB.BookingNo, YB.ExportOrderNo, CCT.TeamName BuyerDepartment,
                                MerchandiserName = EMP.EmployeeName, ReferenceNo = ISNULL(YB.ReferenceNo,''), YB.TNADays, YB.AdditionalBooking, 
                                YB.YBRevisionNeed, YB.RequiredDate, 
                                RevStatus = Case When Isnull(EL.FabBTexAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBTexAckStatus,'') <> '' Then 1 Else Case When YB.IsSample = 1 then 1 else 0 End End, 
                                YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, CancelReason = CR.ReasonName, YB.CanceledBy, 
                                YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
                                YB.GroupName, YB.ContactPerson, ContactPersonName = E.EmployeeName, Depertment = D.Designation +' : '+ ED.DepertmentDescription, 
                                YB.Remarks, YB.YInHouseDate, YB.DateRevised, c.ShortName As BuyerName, IsCompleteDelivery, YB.ImagePath
                                from YBL YB
                                Inner Join FirstYBM FYBM On FYBM.YBookingID = YB.YBookingID
                                Left Join {DbNames.EPYSL}..Employee E On E.EmployeeCode = YB.ContactPerson
                                Left Join {DbNames.EPYSL}..EmployeeDepartment ED On ED.DepertmentID = E.DepertmentID
                                Left Join {DbNames.EPYSL}..EmployeeDesignation D On D.DesigID = E.DesigID
                                left Join {DbNames.EPYSL}..StyleMaster SM On SM.StyleMasterID = YB.StyleMasterID
                                left Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = SM.BuyerTeamID
                                Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = YB.ExportOrderID And EL.ContactID = YB.SupplierID And YB.BookingID = EL.BookingID 
                                left Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = YB.AddedBy
                                left Join {DbNames.EPYSL}..Employee EMP ON EMP.EmployeeCode = LU.EmployeeCode
                                Left Join {DbNames.EPYSL}..CancelReason CR On CR.ReasonID = YB.CancelReasonID 
                                Left Join {DbNames.EPYSL}..Contacts c On c.ContactID = YB.BuyerID

                                GROUP BY YB.BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, 
                                YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, YB.AcknowledgeStatus, YB.ExportOrderID, 
                                YB.BookingNo, YB.ExportOrderNo, CCT.TeamName,
                                EMP.EmployeeName, ISNULL(YB.ReferenceNo,''), YB.TNADays, YB.AdditionalBooking, 
                                YB.YBRevisionNeed, YB.RequiredDate, 
                                Case When Isnull(EL.FabBTexAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBTexAckStatus,'') <> '' Then 1 Else Case When YB.IsSample = 1 then 1 else 0 End End, 
                                YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, CR.ReasonName, YB.CanceledBy, 
                                YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
                                YB.GroupName, YB.ContactPerson, E.EmployeeName, D.Designation,ED.DepertmentDescription, 
                                YB.Remarks, YB.YInHouseDate, YB.DateRevised, c.ShortName, IsCompleteDelivery, YB.ImagePath
                                Order By YB.YBookingID desc
                            )
                            Select *, Count(*) Over() TotalRows From TmpFinal";
                    /*
                    sql = $@"With BIMG As(-------Bulk Booking Image
	                        Select BookingID, Min(ChildImgID) ChildImgID 
	                        From {DbNames.EPYSL}..BookingChildImage Group By BookingID
                        ), IMG As(
	                        Select I.BookingID, I.ImagePath
	                        From BIMG
	                        Inner Join {DbNames.EPYSL}..BookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                        ), SBIMG As(-------Sample Booking Image
	                        Select BookingID, Min(ChildImgID) ChildImgID 
	                        From {DbNames.EPYSL}..SampleBookingChildImage Group By BookingID
                        ), SIMG As(
	                        Select I.BookingID, I.ImagePath
	                        From SBIMG BIMG
	                        Inner Join {DbNames.EPYSL}..SampleBookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                        ),
                        FirstYBM As
                        (
	                        Select YBookingID = Min(YBookingID), YBookingNo 
	                        FROM {TableNames.YarnBookingMaster_New}
	                        Group By YBookingNo
                        ),
                        YBL As
                        (
	                        Select FBA.BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
                            YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID,
                            AcknowledgeStatus=Case When YB.Propose=0 Then 'N/A' When YB.Propose = 1 And YB.Acknowledge=0 Then 'Pending' Else 'Acknowledged' End, BM.BookingNo,EOM.ExportOrderNo,
	                        ReferenceNo = ISNULL(BM.ReferenceNo,''), EOM.CalendarDays As TNADays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID, 
	                        RequiredDate = ISNULL(YB.FabricInHouseDate, 
                            Case When IGST.GoupSubTypeName = 'Main Fabric' Then EOM.FabricEDD When IGST.GoupSubTypeName = 'Sewing' Then EOM.SewingEDD When IGST.GoupSubTypeName = 'Finishing' Then EOM.SewingEDD Else EOM.FabricEDD End),
	                        YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, 
                            YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB, 
                            GroupName = Case When ISG.SubGroupName in ('Fabric','Collar','Cuff') then 'Fabric' Else '' End, 
                            YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy, BM.IsSample, YB.YInHouseDate, 
                            YB.DateRevised, IsCompleteDelivery = Min(convert(int, BC.IsCompleteDelivery)), ISNULL(IMG.ImagePath,'') ImagePath
	                        FROM {TableNames.YarnBookingMaster_New} YB
                            Left Join IMG ON IMG.BookingID = YB.BookingID
	                        Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA On FBA.BookingID = YB.BookingID AND FBA.IsUnAcknowledge = 0
	                        Inner Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingID = YB.BookingID
                            Inner Join {DbNames.EPYSL}..BookingChild BC On BC.BookingID = BM.BookingID
	                        Inner Join {DbNames.EPYSL}..ExportOrderMaster EOM On EOM.ExportOrderID = BM.ExportOrderID
	                        left Join {DbNames.EPYSL}..ItemGroup IG On IG.ItemGroupID = FBA.ItemGroupID
	                        left Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = FBA.SubGroupID
	                        left Join {DbNames.EPYSL}..ItemGroupSubType IGST On IGST.GroupSubTypeID = IG.GroupSubTypeID
	                        Where YB.WithoutOB = 0 AND EOM.EWOStatusID = 130 --And ISG.SubGroupName in ('Fabric','Collar','Cuff') --And SM.BuyerID = 1 And SM.BuyerTeamID = 0  
                            And FBA.RevisionNo = ISNULL(YB.PreProcessRevNo,0) And YB.Propose = 1 And YB.Acknowledge = 0
                            Group By FBA.BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
	                        YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID, BM.BookingNo, EOM.ExportOrderNo,
	                        ISNULL(BM.ReferenceNo,''), EOM.CalendarDays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID, 
	                        YB.FabricInHouseDate, IGST.GoupSubTypeName, EOM.FabricEDD, EOM.SewingEDD, YB.HoldYP, YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, 
	                        YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB, 
                            ISG.SubGroupName, YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy, BM.IsSample, YB.YInHouseDate, YB.DateRevised, ISNULL(IMG.ImagePath,'')

	                        Union All

	                        Select 0 BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
                            YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID,
	                        AcknowledgeStatus=Case When YB.Propose=0 Then 'N/A' When YB.Propose = 1 And YB.Acknowledge=0 Then 'Pending' Else 'Acknowledged' End, 
                            BM.BookingNo,EOM.ExportOrderNo,
	                        ReferenceNo = Case When ISNULL(BM.ReferenceNo,'') = '' then 'Sample' Else ISNULL(BM.ReferenceNo,'') End, EOM.CalendarDays As TNADays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID,
	                        RequiredDate = ISNULL(YB.FabricInHouseDate,BM.FirstInHouseDate), YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
	                        'Fabric' GroupName, YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy, IsSample = Convert(bit,1),
                            YB.YInHouseDate, YB.DateRevised, 0 As IsCompleteDelivery, ISNULL(IMG.ImagePath,'') ImagePath
	 
			                FROM {TableNames.YarnBookingMaster_New}  YB
	                        Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA On FBA.BookingID = YB.BookingID AND FBA.IsUnAcknowledge = 0									
	                        Inner Join {DbNames.EPYSL}..SampleBookingMaster BM On BM.BookingID = YB.BookingID
	                        Inner Join {DbNames.EPYSL}..ExportOrderMaster EOM On EOM.ExportOrderNo = BM.SLNo
                            Left Join SIMG IMG ON IMG.BookingID = YB.BookingID
	                        Where YB.WithoutOB = 1 And EOM.EWOStatusID = 130 
                            And FBA.RevisionNo = ISNULL(YB.PreProcessRevNo,0) And YB.Propose = 1 And YB.Acknowledge = 0
                            Group By YB.YBookingNo, YB.YBookingID, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
                            YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID,
	                        Case When YB.Propose=0 Then 'N/A' When YB.Propose = 1 And YB.Acknowledge=0 Then 'Pending' Else 'Acknowledged' End, 
                            BM.BookingNo,EOM.ExportOrderNo,
	                        Case When ISNULL(BM.ReferenceNo,'') = '' then 'Sample' Else ISNULL(BM.ReferenceNo,'') End, EOM.CalendarDays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID,
	                        ISNULL(YB.FabricInHouseDate,BM.FirstInHouseDate), YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
	                        YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy,
                            YB.YInHouseDate, YB.DateRevised, ISNULL(IMG.ImagePath,'')
                        ),
                        TmpFinal As
                        (
                            Select top 100 YB.BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, 
                            YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, YB.AcknowledgeStatus, YB.ExportOrderID, 
                            YB.BookingNo, YB.ExportOrderNo, CCT.TeamName BuyerDepartment,
                            MerchandiserName = EMP.EmployeeName, ReferenceNo = ISNULL(YB.ReferenceNo,''), YB.TNADays, YB.AdditionalBooking, 
                            YB.YBRevisionNeed, YB.RequiredDate, 
                            RevStatus = Case When Isnull(EL.FabBTexAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBTexAckStatus,'') <> '' Then 1 Else Case When YB.IsSample = 1 then 1 else 0 End End, 
                            YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, CancelReason = CR.ReasonName, YB.CanceledBy, 
                            YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
                            YB.GroupName, YB.ContactPerson, ContactPersonName = E.EmployeeName, Depertment = D.Designation +' : '+ ED.DepertmentDescription, 
                            YB.Remarks, YB.YInHouseDate, YB.DateRevised, c.ShortName As BuyerName, IsCompleteDelivery, YB.ImagePath
                            from YBL YB
                            Inner Join FirstYBM FYBM On FYBM.YBookingID = YB.YBookingID
                            Left Join {DbNames.EPYSL}..Employee E On E.EmployeeCode = YB.ContactPerson
                            Left Join {DbNames.EPYSL}..EmployeeDepartment ED On ED.DepertmentID = E.DepertmentID
                            Left Join {DbNames.EPYSL}..EmployeeDesignation D On D.DesigID = E.DesigID
                            left Join {DbNames.EPYSL}..StyleMaster SM On SM.StyleMasterID = YB.StyleMasterID
                            left Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = SM.BuyerTeamID
                            Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = YB.ExportOrderID And EL.ContactID = YB.SupplierID And YB.BookingID = EL.BookingID 
                            left Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = YB.AddedBy
                            left Join {DbNames.EPYSL}..Employee EMP ON EMP.EmployeeCode = LU.EmployeeCode
                            Left Join {DbNames.EPYSL}..CancelReason CR On CR.ReasonID = YB.CancelReasonID 
                            Left Join {DbNames.EPYSL}..Contacts c On c.ContactID = YB.BuyerID

                            GROUP BY YB.BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, 
                            YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, YB.AcknowledgeStatus, YB.ExportOrderID, 
                            YB.BookingNo, YB.ExportOrderNo, CCT.TeamName,
                            EMP.EmployeeName, ISNULL(YB.ReferenceNo,''), YB.TNADays, YB.AdditionalBooking, 
                            YB.YBRevisionNeed, YB.RequiredDate, 
                            Case When Isnull(EL.FabBTexAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBTexAckStatus,'') <> '' Then 1 Else Case When YB.IsSample = 1 then 1 else 0 End End, 
                            YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, CR.ReasonName, YB.CanceledBy, 
                            YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
                            YB.GroupName, YB.ContactPerson, E.EmployeeName, D.Designation,ED.DepertmentDescription, 
                            YB.Remarks, YB.YInHouseDate, YB.DateRevised, c.ShortName, IsCompleteDelivery, YB.ImagePath
                            Order By YB.YBookingID desc
                        )
                        Select *, Count(*) Over() TotalRows From TmpFinal ";
                    */
                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YBookingID Desc" : paginationInfo.OrderBy;
                }
                else if (status == Status.Revise)
                {
                    sql = $@"With BIMG As(-------Bulk Booking Image
	                        Select BookingID, Min(ChildImgID) ChildImgID 
	                        From {DbNames.EPYSL}..BookingChildImage Group By BookingID
                        ), IMG As(
	                        Select I.BookingID, I.ImagePath
	                        From BIMG
	                        Inner Join {DbNames.EPYSL}..BookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                        ), SBIMG As(-------Sample Booking Image
	                        Select BookingID, Min(ChildImgID) ChildImgID 
	                        From {DbNames.EPYSL}..SampleBookingChildImage Group By BookingID
                        ), SIMG As(
	                        Select I.BookingID, I.ImagePath
	                        From SBIMG BIMG
	                        Inner Join {DbNames.EPYSL}..SampleBookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                        ),
                        FirstYBM As
                        (
	                        Select YBookingID = Min(YBookingID), YBookingNo 
	                        FROM {TableNames.YarnBookingMaster_New}
	                        Group By YBookingNo
                        ),
                        YBL As
                        (
	                        Select FBA.BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
                            YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID,
                            AcknowledgeStatus=Case When YB.Propose=0 Then 'N/A' When YB.Propose = 1 And YB.Acknowledge=0 Then 'Pending' Else 'Acknowledged' End, BM.BookingNo,EOM.ExportOrderNo,
	                        ReferenceNo = ISNULL(BM.ReferenceNo,''), EOM.CalendarDays As TNADays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID, 
	                        RequiredDate = ISNULL(YB.FabricInHouseDate, 
                            Case When IGST.GoupSubTypeName = 'Main Fabric' Then EOM.FabricEDD When IGST.GoupSubTypeName = 'Sewing' Then EOM.SewingEDD When IGST.GoupSubTypeName = 'Finishing' Then EOM.SewingEDD Else EOM.FabricEDD End),
	                        YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, 
                            YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB, 
                            GroupName = Case When ISG.SubGroupName in ('Fabric','Collar','Cuff') then 'Fabric' Else '' End, 
                            YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy, BM.IsSample, YB.YInHouseDate, 
                            YB.DateRevised, IsCompleteDelivery = Min(convert(int, BC.IsCompleteDelivery)), ISNULL(IMG.ImagePath,'') ImagePath
	                        FROM {TableNames.YarnBookingMaster_New} YB
                            Left Join IMG ON IMG.BookingID = YB.BookingID
	                        Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA On FBA.BookingID = YB.BookingID AND FBA.IsUnAcknowledge = 0
	                        Inner Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingID = YB.BookingID
                            Inner Join {DbNames.EPYSL}..BookingChild BC On BC.BookingID = BM.BookingID
	                        Inner Join {DbNames.EPYSL}..ExportOrderMaster EOM On EOM.ExportOrderID = BM.ExportOrderID
	                        left Join {DbNames.EPYSL}..ItemGroup IG On IG.ItemGroupID = FBA.ItemGroupID
	                        left Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = FBA.SubGroupID
	                        left Join {DbNames.EPYSL}..ItemGroupSubType IGST On IGST.GroupSubTypeID = IG.GroupSubTypeID
	                        Where YB.WithoutOB = 0 AND EOM.EWOStatusID = 130 
                            And FBA.RevisionNo <> ISNULL(YB.PreProcessRevNo,0) AND FBA.IsApprovedByPMC = 1
                            Group By FBA.BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
	                        YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID, BM.BookingNo, EOM.ExportOrderNo,
	                        ISNULL(BM.ReferenceNo,''), EOM.CalendarDays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID, 
	                        YB.FabricInHouseDate, IGST.GoupSubTypeName, EOM.FabricEDD, EOM.SewingEDD, YB.HoldYP, YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, 
	                        YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB, 
                            ISG.SubGroupName, YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy, BM.IsSample, YB.YInHouseDate, YB.DateRevised, ISNULL(IMG.ImagePath,'')

	                        Union All

	                        Select 0 BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
                            YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID,
	                        AcknowledgeStatus=Case When YB.Propose=0 Then 'N/A' When YB.Acknowledge=0 Then 'Pending' Else 'Acknowledged' End, 
                            BM.BookingNo,EOM.ExportOrderNo,
	                        ReferenceNo = Case When ISNULL(BM.ReferenceNo,'') = '' then 'Sample' Else ISNULL(BM.ReferenceNo,'') End, EOM.CalendarDays As TNADays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID,
	                        RequiredDate = ISNULL(YB.FabricInHouseDate,BM.FirstInHouseDate), YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
	                        'Fabric' GroupName, YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy, IsSample = Convert(bit,1),
                            YB.YInHouseDate, YB.DateRevised, 0 As IsCompleteDelivery, ISNULL(IMG.ImagePath,'') ImagePath
	 
	                        FROM {TableNames.YarnBookingMaster_New}  YB
	                        Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA On FBA.BookingID = YB.BookingID AND FBA.IsUnAcknowledge = 0							
	                        Inner Join {DbNames.EPYSL}..SampleBookingMaster BM On BM.BookingID = YB.BookingID
	                        Inner Join {DbNames.EPYSL}..ExportOrderMaster EOM On EOM.ExportOrderNo = BM.SLNo
                            Left Join SIMG IMG ON IMG.BookingID = YB.BookingID
	                        Where YB.WithoutOB = 1 And EOM.EWOStatusID = 130 
                            And FBA.RevisionNo <> ISNULL(YB.PreProcessRevNo,0)
                            Group By YB.YBookingNo, YB.YBookingID, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
                            YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID,
	                        Case When YB.Propose=0 Then 'N/A' When YB.Acknowledge=0 Then 'Pending' Else 'Acknowledged' End, 
                            BM.BookingNo,EOM.ExportOrderNo,
	                        Case When ISNULL(BM.ReferenceNo,'') = '' then 'Sample' Else ISNULL(BM.ReferenceNo,'') End, EOM.CalendarDays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID,
	                        ISNULL(YB.FabricInHouseDate,BM.FirstInHouseDate), YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
	                        YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy,
                            YB.YInHouseDate, YB.DateRevised, ISNULL(IMG.ImagePath,'')
                        ),
                        TmpFinal As
                        (
                            Select top 100 YB.BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, 
                            YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, YB.AcknowledgeStatus, YB.ExportOrderID, 
                            YB.BookingNo, YB.ExportOrderNo, CCT.TeamName BuyerDepartment,
                            MerchandiserName = EMP.EmployeeName, ReferenceNo = ISNULL(YB.ReferenceNo,''), YB.TNADays, YB.AdditionalBooking, 
                            YB.YBRevisionNeed, YB.RequiredDate, 
                            RevStatus = Case When Isnull(EL.FabBTexAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBTexAckStatus,'') <> '' Then 1 Else Case When YB.IsSample = 1 then 1 else 0 End End, 
                            YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, CancelReason = CR.ReasonName, YB.CanceledBy, 
                            YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
                            YB.GroupName, YB.ContactPerson, ContactPersonName = E.EmployeeName, Depertment = D.Designation +' : '+ ED.DepertmentDescription, 
                            YB.Remarks, YB.YInHouseDate, YB.DateRevised, c.ShortName As BuyerName, IsCompleteDelivery, YB.ImagePath
                            from YBL YB
                            Inner Join FirstYBM FYBM On FYBM.YBookingID = YB.YBookingID
                            Left Join {DbNames.EPYSL}..Employee E On E.EmployeeCode = YB.ContactPerson
                            Left Join {DbNames.EPYSL}..EmployeeDepartment ED On ED.DepertmentID = E.DepertmentID
                            Left Join {DbNames.EPYSL}..EmployeeDesignation D On D.DesigID = E.DesigID
                            left Join {DbNames.EPYSL}..StyleMaster SM On SM.StyleMasterID = YB.StyleMasterID
                            left Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = SM.BuyerTeamID
                            Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = YB.ExportOrderID And EL.ContactID = YB.SupplierID And YB.BookingID = EL.BookingID 
                            left Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = YB.AddedBy
                            left Join {DbNames.EPYSL}..Employee EMP ON EMP.EmployeeCode = LU.EmployeeCode
                            Left Join {DbNames.EPYSL}..CancelReason CR On CR.ReasonID = YB.CancelReasonID 
                            Left Join {DbNames.EPYSL}..Contacts c On c.ContactID = YB.BuyerID

                            GROUP BY YB.BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, 
                            YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, YB.AcknowledgeStatus, YB.ExportOrderID, 
                            YB.BookingNo, YB.ExportOrderNo, CCT.TeamName,
                            EMP.EmployeeName, ISNULL(YB.ReferenceNo,''), YB.TNADays, YB.AdditionalBooking, 
                            YB.YBRevisionNeed, YB.RequiredDate, 
                            Case When Isnull(EL.FabBTexAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBTexAckStatus,'') <> '' Then 1 Else Case When YB.IsSample = 1 then 1 else 0 End End, 
                            YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, CR.ReasonName, YB.CanceledBy, 
                            YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
                            YB.GroupName, YB.ContactPerson, E.EmployeeName, D.Designation,ED.DepertmentDescription, 
                            YB.Remarks, YB.YInHouseDate, YB.DateRevised, c.ShortName, IsCompleteDelivery, YB.ImagePath
                            Order By YB.YBookingID desc
                        )
                        Select *, Count(*) Over() TotalRows From TmpFinal";
                    /*
                    sql = $@"
                    With BIMG As(
	                    Select BookingID, Min(ChildImgID) ChildImgID 
	                    From {DbNames.EPYSL}..BookingChildImage Group By BookingID
                    ), IMG As(
	                    Select I.BookingID, I.ImagePath
	                    From BIMG
	                    Inner Join {DbNames.EPYSL}..BookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                    ), SBIMG As(
	                    Select BookingID, Min(ChildImgID) ChildImgID 
	                    From {DbNames.EPYSL}..SampleBookingChildImage Group By BookingID
                    ), SIMG As(
	                    Select I.BookingID, I.ImagePath
	                    From SBIMG BIMG
	                    Inner Join {DbNames.EPYSL}..SampleBookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                    ), YBK AS (
	                    Select *
	                    FROM {TableNames.YarnBookingMaster_New}
                        where ISNULL(IsCancel,0) = 0
                    ),
                    BM As (
	                    Select * from {DbNames.EPYSL}..BookingMaster Where SubGroupID In (1,11,12)
                    ),
                    SBM As (
	                    Select * from {DbNames.EPYSL}..SampleBookingMaster
                    ),YB AS (
	                    Select Y.*, BM.BookingNo, ISNULL(IMG.ImagePath,'') ImagePath
	                    From (Select * from YBK Where WithoutOB = 0) Y
	                    Inner Join BM On BM.BookingID = Y.BookingID 
                        Left Join IMG ON IMG.BookingID = BM.BookingID
                    ),SYB AS (
	                    Select Y.*, SBM.BookingNo, SBKRevisionNo = SBM.RevisionNo, ISNULL(IMG.ImagePath,'') ImagePath
	                    From (Select * from YBK Where WithoutOB = 1) Y
	                    Inner Join SBM On SBM.BookingID = Y.BookingID 
                        Left Join SIMG IMG ON IMG.BookingID = SBM.BookingID
                    ), PYB AS (
	                    Select YB.YBookingNo, FYA.BOMMasterID, MinAck = Min(Convert(int,Case When YB.Acknowledge = 1 Then 1 Else 0 End)), MaxAck = Max(Convert(int,Case When YB.Acknowledge = 1 Then 1 Else 0 End))
	                    ,HasRevision = Convert(bit,Max(Convert(int,Case When YB.PreProcessRevNo >= FYA.RevisionNo Then 0 Else 1 End)))
	                    From YB
	                    Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FYA On FYA.BookingID = YB.BookingID And FYA.WithoutOB = YB.WithoutOB
	                    Group By YB.YBookingNo, FYA.BOMMasterID
	                    Having Min(Convert(int,Case When YB.Propose = 1 Then 1 Else 0 End)) = 1 And Min(Convert(int,Case When FYA.Status = 1 Then 1 Else 0 End)) = 1
                    ), PSYB AS (
	                    Select YB.YBookingNo, 0 BOMMasterID, MinAck = Min(Convert(int,Case When YB.Acknowledge = 1 Then 1 Else 0 End)), MaxAck = Max(Convert(int,Case When YB.Acknowledge = 1 Then 1 Else 0 End))
	                    ,HasRevision = Convert(bit,Max(Convert(int,Case When YB.PreProcessRevNo >= FYA.RevisionNo Then 0 Else 1 End))), BookingID = Min(YB.BookingID), SubGroupID = Min(YB.SubGroupID)
	                    From SYB YB
                        Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FYA On FYA.BookingID = YB.BookingID And FYA.WithoutOB = YB.WithoutOB
	                    Group By YB.YBookingNo
	                    Having Min(Convert(int,Case When YB.Propose = 1 Then 1 Else 0 End)) = 1
                    ),BC AS (
	                    SELECT BKM.BookingNo, BKC.BookingID, BKM.SupplierID, BKC.ConsumptionID, BKC.ItemMasterID, BKC.A1ValueID, BKC.YarnBrandID,ISG.SubGroupName
	                    FROM {DbNames.EPYSL}..BookingChild BKC
	                    Inner Join BM BKM On BKM.BookingID = BKC.BookingID
	                    Inner Join {DbNames.EPYSL}..BOMConsumption BOMCon On BOMCon.ConsumptionID = BKC.ConsumptionID and BKC.BOMmasterid = BOMCon.BOMmasterid
	                    Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = BOMCon.SubGroupID
	                    Where ISG.SubGroupName In ('Fabric','Collar','Cuff') and BKC.ISourcing = 1
	                    Group By BKM.BookingNo, BKC.BookingID, BKM.SupplierID, BKC.ConsumptionID, BKC.ItemMasterID, BKC.A1ValueID, BKC.YarnBrandID,ISG.SubGroupName
                    ),SBC AS (
	                    SELECT BKM.BookingNo, BKC.BookingID, BKM.SupplierID, BKC.ConsumptionID, BKC.ItemMasterID, SBCon.A1ValueID, SBCon.YarnBrandID,ISG.SubGroupName
	                    FROM {DbNames.EPYSL}..SampleBookingConsumptionChild BKC
	                    Inner Join SBM BKM On BKM.BookingID = BKC.BookingID
	                    Inner Join {DbNames.EPYSL}..SampleBookingConsumption SBCon On SBCon.ConsumptionID = BKC.ConsumptionID and BKC.BookingID = SBCon.BookingID
	                    Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SBCon.SubGroupID
	                    Where ISG.SubGroupName In ('Fabric','Collar','Cuff') 
	                    Group By BKM.BookingNo, BKC.BookingID, BKM.SupplierID, BKC.ConsumptionID, BKC.ItemMasterID, SBCon.A1ValueID, SBCon.YarnBrandID,ISG.SubGroupName
                    ),YBC AS (	
	                    SELECT YB.BookingID, YBC.ConsumptionID, YBC.ItemMasterID, YBC.YarnTypeID, YBC.YarnBrandID 
	                    FROM {TableNames.YarnBookingChild_New} YBC
	                    Inner Join YB On YB.YBookingID = YBC.YBookingID 
	                    Group By YB.BookingID, YBC.ConsumptionID, YBC.ItemMasterID, YBC.YarnTypeID, YBC.YarnBrandID
                    ),SYBC AS (	
	                    SELECT YB.BookingID, YBC.ConsumptionID, YBC.ItemMasterID, YBC.YarnTypeID, YBC.YarnBrandID 
	                    FROM {TableNames.YarnBookingChild_New} YBC
	                    Inner Join SYB YB On YB.YBookingID = YBC.YBookingID 
	                    Group By YB.BookingID, YBC.ConsumptionID, YBC.ItemMasterID, YBC.YarnTypeID, YBC.YarnBrandID
                    ), YBP AS(
	                    Select BC.BookingNo, SupplierID = Max(Convert(int,BC.SupplierID)), FBookingID = Max(Convert(int,BC.BookingID)), 
	                    IsPendingYB =  Convert(bit,Min(Convert(int,Case When ISNULL(YBC.ItemMasterID,0) = 0 then 1 Else 0 End)))
	                    From BC 
	                    Inner Join YBC On YBC.BookingID = BC.BookingID And YBC.ConsumptionID = BC.ConsumptionID And YBC.YarnBrandID = BC.YarnBrandID And YBC.YarnTypeID = BC.A1ValueID And YBC.ItemMasterID = BC.ItemMasterID
	                    group by BC.BookingNo
                    ), SYBP AS(
	                    Select BC.BookingNo, SupplierID = Max(Convert(int,BC.SupplierID)), FBookingID = Max(Convert(int,BC.BookingID)), 
	                    IsPendingYB =  Convert(bit,Min(Convert(int,Case When ISNULL(YBC.ItemMasterID,0) = 0 then 1 Else 0 End)))
	                    From SBC BC 
	                    Inner Join SYBC YBC On YBC.BookingID = BC.BookingID And YBC.ConsumptionID = BC.ConsumptionID And YBC.YarnBrandID = BC.YarnBrandID And YBC.YarnTypeID = BC.A1ValueID And YBC.ItemMasterID = BC.ItemMasterID
	                    group by BC.BookingNo
                    ), YBA As(
	                    Select YB.IsYarnStock,YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.BookingID, YB.YInHouseDate, YB.YRequiredDate, 
	                    YB.ContactPerson, YB.Propose, YB.ProposeDate, YB.Acknowledge, YB.AcknowledgeDate, YB.Remarks, YB.AddedBy, YB.DateAdded, YB.UpdatedBy, YB.DateUpdated, YB.DateRevised,
	                    YB.HasYPrice, YB.YPRevisionNo, YB.ApproveFP, YB.RevisionReason, YB.ApproveYP, YB.UnApproveYP, YB.YPPreProcessRevNo, YB.FPRevisionNo, YB.FPPreProcessRevNo, YB.PORevisionNeed,AcknowledgeStatus=Case When YB.Acknowledge = 0 Then 'Pending' Else 'Acknowledged' End,
	                    PYB.BOMMasterID, 'Fabric' GroupName, BookingNo = YBP.BookingNo,BookingDate = BM.BookingDate, EOM.ExportOrderNo, EOM.ExportOrderID, EOM.StyleMasterID, EOM.FabricEDD,
	                    RequiredDate = EOM.FabricEDD, ReferenceNo = ISNULL(BM.ReferenceNo,''), EOM.CalendarDays, PYB.HasRevision, YBP.IsPendingYB, YB.AdditionalBooking, YB.YBRevisionNeed,
	                    YB.FPRejectReason,YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB, YB.SubGroupID, YBP.SupplierID,
                        YB.BuyerID, YB.BuyerTeamID, YB.CompanyID, YB.AllowForNextStep, YB.TNADays, YB.FabricInHouseDate, YB.ImagePath											
                        From YB
	                    Inner Join YBP On YBP.BookingNo = YB.BookingNo
	                    Inner Join PYB On PYB.YBookingNo = YB.YBookingNo
	                    Inner Join (Select * from {DbNames.EPYSL}..ExportOrderMaster where EWOStatusID = 130) EOM On EOM.ExportOrderID = YB.ExportOrderID
	                    Inner Join BM On BM.BookingID = YB.BookingID
	                    Where ((PYB.MinAck = 0 or PYB.MaxAck = 0) And YB.RevisionNo > 0)
	                    Union All
	                    Select YB.IsYarnStock,YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.BookingID, YB.YInHouseDate, YB.YRequiredDate, 
	                    YB.ContactPerson, YB.Propose, YB.ProposeDate, YB.Acknowledge, YB.AcknowledgeDate, YB.Remarks, YB.AddedBy, YB.DateAdded, YB.UpdatedBy, YB.DateUpdated, YB.DateRevised,
	                    YB.HasYPrice, YB.YPRevisionNo, YB.ApproveFP, YB.RevisionReason, YB.ApproveYP, YB.UnApproveYP, YB.YPPreProcessRevNo, YB.FPRevisionNo, YB.FPPreProcessRevNo, YB.PORevisionNeed,AcknowledgeStatus=Case When YB.Acknowledge = 0 Then 'Pending' Else 'Acknowledged' End,
	                    PYB.BOMMasterID, 'Fabric' GroupName, BookingNo = YBP.BookingNo,BookingDate = BM.BookingDate, EOM.ExportOrderNo, EOM.ExportOrderID, EOM.StyleMasterID, EOM.FabricEDD,
	                    RequiredDate = EOM.FabricEDD, ReferenceNo = Case When ISNULL(BM.ReferenceNo,'') = '' and YB.WithoutOB = 1 then 'Sample' Else ISNULL(BM.ReferenceNo,'') End, EOM.CalendarDays, PYB.HasRevision, YBP.IsPendingYB, YB.AdditionalBooking, YB.YBRevisionNeed,
	                    YB.FPRejectReason,YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB, YB.SubGroupID, YBP.SupplierID,
                        YB.BuyerID, YB.BuyerTeamID, YB.CompanyID, YB.AllowForNextStep, YB.TNADays, YB.FabricInHouseDate, YB.ImagePath
	                    From SYB YB
	                    Inner Join SYBP YBP On YBP.BookingNo = YB.BookingNo
	                    Inner Join PSYB PYB On PYB.YBookingNo = YB.YBookingNo And PYB.SubGroupID = YB.SubGroupID
	                    Inner Join SBM BM On BM.BookingID = YB.BookingID
	                    Inner Join (Select * from {DbNames.EPYSL}..ExportOrderMaster where EWOStatusID = 130) EOM On EOM.ExportOrderNo = BM.SLNo
	                    Where ((PYB.MinAck = 0 or PYB.MaxAck = 0) And YB.RevisionNo > 0)
                    ),
                    FinalQ AS 
                    (Select YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, YB.BookingID, YB.YInHouseDate, YB.YRequiredDate, 
                    YB.ContactPerson, YB.Propose, YB.ProposeDate, YB.Acknowledge, YB.AcknowledgeDate, YB.Remarks, YB.AddedBy, YB.DateAdded, YB.UpdatedBy, YB.DateUpdated, YB.DateRevised,
                    YB.HasYPrice, YB.YPRevisionNo, YB.ApproveYP, YB.UnApproveYP, YB.ApproveFP, YB.RevisionReason, YB.YPPreProcessRevNo, YB.FPRevisionNo, YB.FPPreProcessRevNo, YB.PORevisionNeed,AcknowledgeStatus=Case When YB.Acknowledge = 0 Then 'Pending' Else 'Acknowledged' End,
                    YB.BOMMasterID,SM.StyleMasterID,'Fabric' GroupName, 
                    BookingNo = YB.BookingNo,BookingDate = YB.BookingDate, YB.ExportOrderNo, CCT.TeamName BuyerDepartment, MerchandiserName = EMP.EmployeeName, FY.YearName,ContactPersonName = E.EmployeeName,Depertment =D.Designation +' : '+ ED.DepertmentDescription, 
                    RequiredDate = YB.FabricEDD, ReferenceNo = ISNULL(YB.ReferenceNo,''), YB.CalendarDays, YB.HasRevision, YB.IsPendingYB, YB.AdditionalBooking, YB.YBRevisionNeed,
                    RevStatus = Case When WithoutOB = 1 then 1 Else Case When (Isnull(EL.YBStatus,'') = 'Pending for Acknowledge' or Isnull(EL.YBStatus,'') = 'Acknowledged' or Isnull(EL.YBStatus,'') = 'Acknowledged with Additional Booking') And Isnull(EL.YBStatus,'') <> '' Then 1 Else 0 End End, 
                    YB.FPRejectReason,YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB, YB.SubGroupID,
                    YB.BuyerID, c.ShortName As BuyerName, YB.BuyerTeamID, YB.CompanyID, YB.AllowForNextStep, YB.TNADays, YB.FabricInHouseDate,YB.IsYarnStock, YB.ImagePath
                    From YBA YB
                    Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = YB.ExportOrderID And EL.BookingID = YB.BookingID And YB.SupplierID = EL.ContactID
                    left Join {DbNames.EPYSL}..StyleMaster SM On SM.StyleMasterID = YB.StyleMasterID
                    left Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = YB.AddedBy
                    left Join {DbNames.EPYSL}..Employee EMP ON EMP.EmployeeCode = LU.EmployeeCode
                    left Join {DbNames.EPYSL}..FinancialYear FY On FY.FinancialYearID = SM.FinancialYearID
                    left Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = SM.BuyerTeamID
                    left join {DbNames.EPYSL}..Contacts C On C.ContactID = YB.BuyerID
                    Left Join {DbNames.EPYSL}..Employee E On E.EmployeeCode = YB.ContactPerson
                    Left Join {DbNames.EPYSL}..EmployeeDepartment ED On ED.DepertmentID = E.DepertmentID
                    Left Join {DbNames.EPYSL}..EmployeeDesignation D On D.DesigID = E.DesigID)
                
                    SELECT *, Count(*) Over() TotalRows FROM FinalQ ";
                    */
                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YBookingID Desc" : paginationInfo.OrderBy;
                }
                else if (status == Status.Acknowledge)
                {
                    sql =
                      $@"
                    With BIMG As(-------Bulk Booking Image
	                    Select BookingID, Min(ChildImgID) ChildImgID 
	                    From {DbNames.EPYSL}..BookingChildImage Group By BookingID
                    ), IMG As(
	                    Select I.BookingID, I.ImagePath
	                    From BIMG
	                    Inner Join {DbNames.EPYSL}..BookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                    ), SBIMG As(-------Sample Booking Image
	                    Select BookingID, Min(ChildImgID) ChildImgID 
	                    From {DbNames.EPYSL}..SampleBookingChildImage Group By BookingID
                    ), SIMG As(
	                    Select I.BookingID, I.ImagePath
	                    From SBIMG BIMG
	                    Inner Join {DbNames.EPYSL}..SampleBookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                    ),
                    FirstYBM As
                    (
	                    Select YBookingID = Min(YBookingID), YBookingNo 
	                    FROM {TableNames.YarnBookingMaster_New}
	                    Group By YBookingNo
                    ),
                    YBL As
                    (
	                    Select FBA.BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
                        YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID,
                        AcknowledgeStatus=Case When YB.Propose=0 Then 'N/A' When YB.Propose = 1 And YB.Acknowledge=0 Then 'Pending' Else 'Acknowledged' End, BM.BookingNo,EOM.ExportOrderNo,
	                    ReferenceNo = ISNULL(BM.ReferenceNo,''), EOM.CalendarDays As TNADays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID, 
	                    RequiredDate = ISNULL(YB.FabricInHouseDate, 
                        Case When IGST.GoupSubTypeName = 'Main Fabric' Then EOM.FabricEDD When IGST.GoupSubTypeName = 'Sewing' Then EOM.SewingEDD When IGST.GoupSubTypeName = 'Finishing' Then EOM.SewingEDD Else EOM.FabricEDD End),
	                    YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, 
                        YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB, 
                        GroupName = Case When ISG.SubGroupName in ('Fabric','Collar','Cuff') then 'Fabric' Else '' End, 
                        YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy, BM.IsSample, YB.YInHouseDate, 
                        YB.DateRevised, IsCompleteDelivery = Min(convert(int, BC.IsCompleteDelivery)), ISNULL(IMG.ImagePath,'') ImagePath
	                    FROM {TableNames.YarnBookingMaster_New} YB
                        Left Join IMG ON IMG.BookingID = YB.BookingID
	                    Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA On FBA.BookingID = YB.BookingID AND FBA.IsUnAcknowledge = 0
	                    Inner Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingID = YB.BookingID
                        Inner Join {DbNames.EPYSL}..BookingChild BC On BC.BookingID = BM.BookingID
	                    Inner Join {DbNames.EPYSL}..ExportOrderMaster EOM On EOM.ExportOrderID = BM.ExportOrderID
	                    left Join {DbNames.EPYSL}..ItemGroup IG On IG.ItemGroupID = FBA.ItemGroupID
	                    left Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = FBA.SubGroupID
	                    left Join {DbNames.EPYSL}..ItemGroupSubType IGST On IGST.GroupSubTypeID = IG.GroupSubTypeID
	                    Where YB.WithoutOB = 0 AND EOM.EWOStatusID = 130 
                        And FBA.RevisionNo = ISNULL(YB.PreProcessRevNo,0) AND FBA.IsApprovedByPMC = 1 And YB.Propose = 1 And YB.Acknowledge = 1
                        Group By FBA.BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
	                    YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID, BM.BookingNo, EOM.ExportOrderNo,
	                    ISNULL(BM.ReferenceNo,''), EOM.CalendarDays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID, 
	                    YB.FabricInHouseDate, IGST.GoupSubTypeName, EOM.FabricEDD, EOM.SewingEDD, YB.HoldYP, YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, 
	                    YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB, 
                        ISG.SubGroupName, YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy, BM.IsSample, YB.YInHouseDate, YB.DateRevised, ISNULL(IMG.ImagePath,'')

	                    Union All

	                    Select 0 BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
                        YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID,
	                    AcknowledgeStatus=Case When YB.Propose=0 Then 'N/A' When YB.Propose = 1 And YB.Acknowledge=0 Then 'Pending' Else 'Acknowledged' End, 
                        BM.BookingNo,EOM.ExportOrderNo,
	                    ReferenceNo = Case When ISNULL(BM.ReferenceNo,'') = '' then 'Sample' Else ISNULL(BM.ReferenceNo,'') End, EOM.CalendarDays As TNADays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID,
	                    RequiredDate = ISNULL(YB.FabricInHouseDate,BM.FirstInHouseDate), YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
	                    'Fabric' GroupName, YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy, IsSample = Convert(bit,1),
                        YB.YInHouseDate, YB.DateRevised, 0 As IsCompleteDelivery, ISNULL(IMG.ImagePath,'') ImagePath
	 
	                    FROM {TableNames.YarnBookingMaster_New}  YB
	                    Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA On FBA.BookingID = YB.BookingID AND FBA.IsUnAcknowledge = 0							
	                    Inner Join {DbNames.EPYSL}..SampleBookingMaster BM On BM.BookingID = YB.BookingID
	                    Inner Join {DbNames.EPYSL}..ExportOrderMaster EOM On EOM.ExportOrderNo = BM.SLNo
                        Left Join SIMG IMG ON IMG.BookingID = YB.BookingID
	                    Where YB.WithoutOB = 1 And EOM.EWOStatusID = 130 
                        And FBA.RevisionNo = ISNULL(YB.PreProcessRevNo,0) And FBA.IsApprovedByPMC = 1 AND YB.Propose = 1 And YB.Acknowledge = 1
                        Group By YB.YBookingNo, YB.YBookingID, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
                        YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID,
	                    Case When YB.Propose=0 Then 'N/A' When YB.Propose = 1 And YB.Acknowledge=1 Then 'Pending' Else 'Acknowledged' End, 
                        BM.BookingNo,EOM.ExportOrderNo,
	                    Case When ISNULL(BM.ReferenceNo,'') = '' then 'Sample' Else ISNULL(BM.ReferenceNo,'') End, EOM.CalendarDays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID,
	                    ISNULL(YB.FabricInHouseDate,BM.FirstInHouseDate), YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
	                    YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy,
                        YB.YInHouseDate, YB.DateRevised, ISNULL(IMG.ImagePath,'')
                    ),
                    TmpFinal As
                    (
                        Select top 100 YB.BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, 
                        YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, YB.AcknowledgeStatus, YB.ExportOrderID, 
                        YB.BookingNo, YB.ExportOrderNo, CCT.TeamName BuyerDepartment,
                        MerchandiserName = EMP.EmployeeName, ReferenceNo = ISNULL(YB.ReferenceNo,''), YB.TNADays, YB.AdditionalBooking, 
                        YB.YBRevisionNeed, YB.RequiredDate, 
                        RevStatus = Case When Isnull(EL.FabBTexAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBTexAckStatus,'') <> '' Then 1 Else Case When YB.IsSample = 1 then 1 else 0 End End, 
                        YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, CancelReason = CR.ReasonName, YB.CanceledBy, 
                        YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
                        YB.GroupName, YB.ContactPerson, ContactPersonName = E.EmployeeName, Depertment = D.Designation +' : '+ ED.DepertmentDescription, 
                        YB.Remarks, YB.YInHouseDate, YB.DateRevised, c.ShortName As BuyerName, IsCompleteDelivery, YB.ImagePath
                        from YBL YB
                        Inner Join FirstYBM FYBM On FYBM.YBookingID = YB.YBookingID
                        Left Join {DbNames.EPYSL}..Employee E On E.EmployeeCode = YB.ContactPerson
                        Left Join {DbNames.EPYSL}..EmployeeDepartment ED On ED.DepertmentID = E.DepertmentID
                        Left Join {DbNames.EPYSL}..EmployeeDesignation D On D.DesigID = E.DesigID
                        left Join {DbNames.EPYSL}..StyleMaster SM On SM.StyleMasterID = YB.StyleMasterID
                        left Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = SM.BuyerTeamID
                        Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = YB.ExportOrderID And EL.ContactID = YB.SupplierID And YB.BookingID = EL.BookingID 
                        left Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = YB.AddedBy
                        left Join {DbNames.EPYSL}..Employee EMP ON EMP.EmployeeCode = LU.EmployeeCode
                        Left Join {DbNames.EPYSL}..CancelReason CR On CR.ReasonID = YB.CancelReasonID 
                        Left Join {DbNames.EPYSL}..Contacts c On c.ContactID = YB.BuyerID

                        GROUP BY YB.BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, 
                        YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, YB.AcknowledgeStatus, YB.ExportOrderID, 
                        YB.BookingNo, YB.ExportOrderNo, CCT.TeamName,
                        EMP.EmployeeName, ISNULL(YB.ReferenceNo,''), YB.TNADays, YB.AdditionalBooking, 
                        YB.YBRevisionNeed, YB.RequiredDate, 
                        Case When Isnull(EL.FabBTexAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBTexAckStatus,'') <> '' Then 1 Else Case When YB.IsSample = 1 then 1 else 0 End End, 
                        YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, CR.ReasonName, YB.CanceledBy, 
                        YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
                        YB.GroupName, YB.ContactPerson, E.EmployeeName, D.Designation,ED.DepertmentDescription, 
                        YB.Remarks, YB.YInHouseDate, YB.DateRevised, c.ShortName, IsCompleteDelivery, YB.ImagePath
                        Order By YB.YBookingID desc
                    )
                    Select *, Count(*) Over() TotalRows From TmpFinal  ";

                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YBookingNo Desc" : paginationInfo.OrderBy;
                }
            }

            sql += $@"
            {paginationInfo.FilterBy}
            {orderBy}
            {paginationInfo.PageBy}";

            return await _service.GetDataAsync<YarnBookingMaster>(sql);
        }
        public async Task<List<YarnBookingMaster>> GetPagedAsyncV1(Status status, string PageName, PaginationInfo paginationInfo)
        {
            var orderBy = "";//string.IsNullOrEmpty(paginationInfo.OrderBy) ? "ORDER BY YBookingDate DESC" : paginationInfo.OrderBy;

            string sql = "";

            if (PageName == "YarnBooking")
            {
                if (status == Status.Pending)
                {
                    sql =
                    $@"
                    ;With EOM As (
                        Select ExportOrderID, ExportOrderNo, StyleMasterID, CalendarDays As TNADays
                        From {DbNames.EPYSL}..ExportOrderMaster
                        Where EWOStatusID = 130
                    ), BM As(
                        Select BM.BookingID, BM.BookingNo, BM.BuyerID, BM.BuyerTeamID, BM.CompanyID, BM.ExportOrderID,
                        1 SubGroupID,
                        BM.InHouseDate, ReferenceNo = ISNULL(BM.ReferenceNo,''), BM.SupplierID, BM.AddedBy, EOM.ExportOrderNo, EOM.StyleMasterID, EOM.TNADays, 0 WithoutOB
                        From {DbNames.EPYSL}..BookingMaster BM
                        Inner Join EOM On EOM.ExportOrderID = BM.ExportOrderID
                        Where ISNULL(BM.IsCancel,0) = 0 --And BM.SubGroupID in (1,11,12)
                        Group by BM.BookingID, BM.BookingNo, BM.BuyerID, BM.BuyerTeamID, BM.CompanyID, BM.ExportOrderID,
                        BM.InHouseDate, ISNULL(BM.ReferenceNo,''), BM.SupplierID, BM.AddedBy, EOM.ExportOrderNo, EOM.StyleMasterID, EOM.TNADays
                        Union All
                        Select BM.BookingID, BM.BookingNo, BM.BuyerID, BM.BuyerTeamID, 0 CompanyID, BM.ExportOrderID,
                        1 SubGroupID,
                        BM.InHouseDate, ReferenceNo = ISNULL(BM.ReferenceNo,''), BM.SupplierID, BM.AddedBy, EOM.ExportOrderNo, EOM.StyleMasterID, EOM.TNADays, 1 WithoutOB
                        From {DbNames.EPYSL}..SampleBookingMaster BM
                        Inner Join EOM On EOM.ExportOrderID = BM.ExportOrderID
                        Where ISNULL(BM.IsCancel,0) = 0 And BM.InHouse = 1 And BM.hasPayment = 1 --And BM.SubGroupID in (1,11,12)
                        Group by BM.BookingID, BM.BookingNo, BM.BuyerID, BM.BuyerTeamID, BM.ExportOrderID,
                        BM.InHouseDate, ISNULL(BM.ReferenceNo,''), SupplierID, BM.AddedBy, EOM.ExportOrderNo, EOM.StyleMasterID, EOM.TNADays
                    ), BIMG As(
                        Select BM.BookingNo, ImagePath, SL = ROW_NUMBER() Over(PARTITION BY BM.BookingNo Order By CI.ChildImgID)
                        From BM
                        Inner Join {DbNames.EPYSL}..BookingChildImage CI On CI.BookingID = BM.BookingID
                        Union All
                        Select BM.BookingNo, ImagePath, SL = ROW_NUMBER() Over(PARTITION BY BM.BookingNo Order By CI.ChildImgID)
                        From BM
                        Inner Join {DbNames.EPYSL}..SampleBookingChildImage CI On CI.BookingID = BM.BookingID
                    ), IMG As(
                        Select BookingNo, ImagePath
                        From BIMG
                        Where SL = 1
                    ), TmpFinal As(
                        Select FBA.BOMMasterID, Min(BM.BookingID) BookingID, BM.BookingNo, BM.BuyerID, BM.BuyerTeamID, BM.CompanyID, BM.ExportOrderID, BM.SubGroupID,
                            RequiredDate = BM.InHouseDate, BM.ReferenceNo, SupplierID, BM.AddedBy, BM.ExportOrderNo, BM.StyleMasterID, BM.TNADays, BM.WithoutOB,
                            IG.SubGroupName GroupName, c.ShortName As BuyerName,   
                            CCT.TeamName BuyerDepartment, MerchandiserName = EMP.EmployeeName,FY.YearName,
                            RevStatus = Case When Isnull(EL.FabBTexAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBTexAckStatus,'') <> '' Then 1 Else 0 End, 0 As IsCompleteDelivery,
                            IMG.ImagePath
                        From BM
                        Inner JOIN {TableNames.FabricBookingAcknowledge} FBA On FBA.BookingID = BM.BookingID
                        Left JOIN {TableNames.YarnBookingMaster_New} YBM On YBM.BookingID = BM.BookingID
                        Left Join IMG On IMG.BookingNo = BM.BookingNo
                        Inner Join {DbNames.EPYSL}..StyleMaster SM On SM.StyleMasterID = BM.StyleMasterID
                        Inner Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = SM.BuyerTeamID
                        Inner Join {DbNames.EPYSL}..ItemSubGroup IG On IG.SubGroupID = BM.SubGroupID
                        Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = BM.ExportOrderID And EL.ContactID = BM.SupplierID And BM.BookingID = EL.BookingID  --And EL.ItemGroupID = ISG.ItemGroupID                                        
                        Inner Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = BM.AddedBy
                        Inner Join {DbNames.EPYSL}..Employee EMP ON EMP.EmployeeCode = LU.EmployeeCode
                        Inner Join {DbNames.EPYSL}..FinancialYear FY On FY.FinancialYearID = SM.FinancialYearID
                        Left Join {DbNames.EPYSL}..Contacts c On c.ContactID = BM.BuyerID
                        Inner Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = BM.SupplierID
                        Where YBM.BookingID IS NULL And CAI.InHouse = 1
                        Group By FBA.BOMMasterID,BM.BookingNo, BM.BuyerID, BM.BuyerTeamID, BM.CompanyID, BM.ExportOrderID, BM.SubGroupID,
                            BM.InHouseDate, BM.ReferenceNo, SupplierID, BM.AddedBy, BM.ExportOrderNo, BM.StyleMasterID, BM.TNADays, BM.WithoutOB,
                            IG.SubGroupName, c.ShortName,   
                            CCT.TeamName, EMP.EmployeeName,FY.YearName,
                            Case When Isnull(EL.FabBTexAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBTexAckStatus,'') <> '' Then 1 Else 0 End,
                            IMG.ImagePath
                    )
                    Select *, Count(*) Over() TotalRows from TmpFinal ";
                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By BookingID Desc" : paginationInfo.OrderBy;
                }
                /*
                if (status == Status.Pending) //Previous
                {
                    sql =
                     $@"
                    With BIMG As(
	                    Select BookingID, Min(ChildImgID) ChildImgID 
	                    From {DbNames.EPYSL}..BookingChildImage Group By BookingID
                    ), IMG As(
	                    Select I.BookingID, I.ImagePath
	                    From BIMG
	                    Inner Join {DbNames.EPYSL}..BookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                    ), BM As
                    (
	                    Select BM.BookingID, BM.BookingNo, BM.BuyerID, BM.BuyerTeamID, BM.CompanyID, BM.ExportOrderID, 
                        BM.SubGroupID, BM.InHouseDate, ReferenceNo = ISNULL(BM.ReferenceNo,''), BM.SupplierID, BM.AddedBy, 
	                    IsCompleteDelivery = Min(convert(int,BC.IsCompleteDelivery)), ISNULL(IMG.ImagePath,'') ImagePath
	                    From {DbNames.EPYSL}..BookingMaster BM 
                        Inner Join {DbNames.EPYSL}..BookingChild BC On BM.BookingID = BC.BookingID
                        Left Join IMG ON IMG.BookingID = BM.BookingID
	                    Where ISNULL(BM.IsCancel,0) = 0
	                    Group By BM.BookingID, BM.BookingNo, BM.BuyerID, BM.BuyerTeamID, BM.CompanyID, BM.ExportOrderID, BM.SubGroupID,    
                        BM.InHouseDate, ISNULL(BM.ReferenceNo,''), BM.SupplierID, BM.AddedBy, ISNULL(IMG.ImagePath,'') 
                    ),
                    YBM as
                    (
	                    Select BookingID 
	                    FROM {TableNames.YarnBookingMaster_New} Where WithoutOB = 0
                    ),
                    A As
                    (
                        Select FBA.BookingID,FBA.BOMMasterID,SM.StyleMasterID,IG.GroupName, BM.BookingNo,EOM.ExportOrderNo,
                        BM.BuyerID, c.ShortName As BuyerName, BM.BuyerTeamID, BM.CompanyID, BM.ExportOrderID, BM.SubGroupID,  
                        CCT.TeamName BuyerDepartment,MerchandiserName = EMP.EmployeeName,FY.YearName,
                        RequiredDate = BM.InHouseDate, ReferenceNo = ISNULL(BM.ReferenceNo,''), EOM.CalendarDays As TNADays,
                        RevStatus = Case When Isnull(EL.FabBTexAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBTexAckStatus,'') <> '' Then 1 Else 0 End,
                        WithoutOB, BM.IsCompleteDelivery, BM.ImagePath
                        From 
                        (
	                        Select BOMMasterID, BookingID = Min(FBK.BookingID), 	
	                        ItemGroupID = Case When FBK.ItemGroupID in (1,11,12) then 1 else FBK.ItemGroupID End, 
	                        SubGroupID = Case When FBK.SubGroupID in (1,11,12) then 1 else FBK.SubGroupID End,
                            WithoutOB
	                        FROM {TableNames.FabricBookingAcknowledge} FBK
	                        Inner Join BM On BM.BookingID = FBK.BookingID
	                        Where Status = 1 and FBK.WithoutOB = 0--and BOMMasterID = 976
	                        Group By BOMMasterID, BM.BookingNo, --BookingID, 	
	                        Case When FBK.ItemGroupID in (1,11,12) then 1 else FBK.ItemGroupID End, 
	                        Case When FBK.SubGroupID in (1,11,12) then 1 else FBK.SubGroupID End,
                            WithoutOB
                        ) FBA
                        Inner Join BM On BM.BookingID = FBA.BookingID
                        Inner Join {DbNames.EPYSL}..ExportOrderMaster EOM On EOM.ExportOrderID = BM.ExportOrderID                                       
                        Inner Join {DbNames.EPYSL}..StyleMaster SM On SM.StyleMasterID = EOM.StyleMasterID
                        Inner Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = SM.BuyerTeamID
                        Inner Join {DbNames.EPYSL}..ItemGroup IG On IG.ItemGroupID = FBA.ItemGroupID
                        Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = EOM.ExportOrderID And EL.ContactID = BM.SupplierID And BM.BookingID = EL.BookingID  --And EL.ItemGroupID = ISG.ItemGroupID										
                        Inner Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = BM.AddedBy
                        Inner Join {DbNames.EPYSL}..Employee EMP ON EMP.EmployeeCode = LU.EmployeeCode
                        Inner Join {DbNames.EPYSL}..FinancialYear FY On FY.FinancialYearID = SM.FinancialYearID
                        Left Join {DbNames.EPYSL}..Contacts c On c.ContactID = BM.BuyerID 
                        Where EOM.EWOStatusID = 130  
                        And FBA.BookingID not in (Select BookingID From YBM) 
                    ), SBIMG As(
	                    Select BookingID, Min(ChildImgID) ChildImgID 
	                    From {DbNames.EPYSL}..SampleBookingChildImage Group By BookingID
                    ), SIMG As(
	                    Select I.BookingID, I.ImagePath
	                    From SBIMG BIMG
	                    Inner Join {DbNames.EPYSL}..SampleBookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                    ), 
                    B As
                    (   
                        Select BookingID = SBM.BookingID, 0 BOMMasterID, EM.StyleMasterID, 'Fabric' GroupName, SBM.BookingNo, EM.ExportOrderNo, 
                        SBM.BuyerID, c.ShortName As BuyerName, SBM.BuyerTeamID, 0 As CompanyID, SBM.ExportOrderID, SBM.SubGroupID,  
                        CCT.TeamName BuyerDepartment, EMP.EmployeeName MerchandiserName, YearName, RequiredDate = SBM.FirstInHouseDate, 
                        ReferenceNo = Case When ISNULL(SBM.ReferenceNo,'') = '' then 'Sample' Else ISNULL(SBM.ReferenceNo,'') End, EM.CalendarDays  As TNADays, 1 RevStatus,
                        WithoutOB, 0 As IsCompleteDelivery, ISNULL(IMG.ImagePath,'') ImagePath
                        from {DbNames.EPYSL}..SampleBookingMaster SBM
                        Inner Join (Select * FROM {TableNames.FabricBookingAcknowledge} Where WithoutOB = 1) FBK On FBK.BookingID = SBM.BookingID
                        Inner Join {DbNames.EPYSL}..ExportOrderMaster EM On EM.ExportOrderID = SBM.ExportOrderID
                        Inner Join {DbNames.EPYSL}..FinancialYear FY On SBM.BookingDate between FY.StartMonth and FY.EndMonth
                        Inner Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = SBM.BuyerTeamID
                        Inner Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = SBM.AddedBy
                        Inner Join {DbNames.EPYSL}..Employee EMP ON EMP.EmployeeCode = LU.EmployeeCode
                        Left Join {DbNames.EPYSL}..Contacts c On c.ContactID = SBM.BuyerID 
                        Left Join SIMG IMG ON IMG.BookingID = SBM.BookingID
                        Where SBM.IsCancel = 0 And SBM.InHouse = 1 And SBM.hasPayment = 1 And EM.IsAutoGenerateNo = 1 And EM.EWOStatusID = 130 
                        And SBM.BookingID not in (Select BookingID FROM {TableNames.YarnBookingMaster_New} Where WithoutOB = 1) 
                    ),
                    TmpFinal As
                    (
                        Select * From A Union Select * From B
                    )
                    Select *, Count(*) Over() TotalRows from TmpFinal ";
                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By BookingID Desc" : paginationInfo.OrderBy;
                }
                */
                else if (status == Status.Report)
                {
                    sql =
                    $@"With RunningEWO As
                    (
	                    Select EWO.ExportOrderID,EWO.StyleMasterID, EWO.ExportOrderNo, EL.FabBTexAckStatus,EL.ContactID
	                    From {DbNames.EPYSL}..ExportOrderMaster EWO    
                        Left Join (
                            Select ExportOrderID, ContactID, BookingID,FabBTexAckStatus From (
                                Select ExportOrderID, ContactID, FabBTexAckStatus,BookingID 
                                From {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild 
                                Where GroupName in ('Fabric','Collar','Cuff')
                            ) A Group By ExportOrderID, ContactID, FabBTexAckStatus,BookingID
                        ) EL On EL.ExportOrderID = EWO.ExportOrderID
                        Inner Join {DbNames.EPYSL}..EntityTypeValue ETV On ETV.ValueID = EWO.EWOStatusID
	                    Where ETV.ValueName = 'Running'
	                    Group By EWO.ExportOrderID,EWO.StyleMasterID, EWO.ExportOrderNo, EL.FabBTexAckStatus,EL.ContactID
                    ),
                    BM As
                    (
	                    Select * from {DbNames.EPYSL}..BookingMaster
                    ),
                    FirstBM As
                    (
	                    Select BookingID = Min(BookingID), BookingNo 
	                    from BM Group By BookingNo
                    ),
                    BC As
                    (
                        Select BM.ExportOrderID,BC.BookingID,BM.SupplierID,BookingNo = BM.BookingNo,BM.BookingDate,
                        BookingRev = BM.RevisionNo, IsFabricCancel = ISNULL(BM.IsCancel,0), BM.BuyerID,
                        IsCompleteDelivery = Min(convert(int,BC.IsCompleteDelivery))
                        From BM
	                    Inner Join FirstBM FBM On FBM.BookingID = BM.BookingID
                        Inner Join {DbNames.EPYSL}..BookingChild BC On BC.BookingID = BM.BookingID
                        Left Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = BC.ContactID
                        Inner Join {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = BC.SubGroupID
                        Where ISG.SubGroupName in ('Fabric','Collar','Cuff') And ISNULL(CAI.InHouse,0) = 1
                        Group By BM.ExportOrderID,BM.SupplierID,BC.BookingID,BM.BookingNo,BM.BookingDate,
                        BM.RevisionNo,ISNULL(BM.IsCancel,0), BM.BuyerID
                    ),
                    StatusList As
                    (
	                    Select EWO.ExportOrderID,EWO.ExportOrderNo,BC.BookingNo,BuyerTeam = CCT.TeamName,FabricRevision = FBA.RevisionNo,
	                    SCAcknowledgeDate =Case When BIA.AcknowledgeDate Is Null Then '' Else  Convert(Varchar,BIA.AcknowledgeDate) END,
	                    SCAcknowledgeStatus = Case When BIA.Status = 1 Then 'Complete' Else 'Pending' END,
	                    PMCAcknowledgeDate =Case When FBA.AcknowledgeDate Is Null Then '' Else  Convert(Varchar,FBA.AcknowledgeDate) END,
	                    PMCAcknowledgeStatus = Case When FBA.Status = 1 Then 'Complete' Else 'Pending' END,
	                    YarnBookingDate = Case When YBM.ProposeDate Is Null Then Case When YBM.YBookingDate Is Null Then '' Else Convert(Varchar,YBM.YBookingDate) End Else Case When YBM.Propose = 1 then Convert(Varchar,YBM.ProposeDate) else '' end End,
	                    YarnAcknowledgeStatus = Case When YBM.Acknowledge = 1 Then 'Complete' Else 'Pending' END,
	                    RevStatus = Case When Isnull(EWO.FabBTexAckStatus,'') = 'Acknowledged' And Isnull(EWO.FabBTexAckStatus,'') <> '' Then 1 Else 0 End,
                        YBookingNo = ISNULL(YBM.YBookingNo,''), BC.IsFabricCancel, IsCancel = ISNULL(YBM.IsCancel,0),
                        BC.BuyerID, c.ShortName as BuyerName, IsCompleteDelivery
	                    From BC  	                                     
	                    Left Join RunningEWO EWO On BC.ExportOrderID = EWO.ExportOrderID And BC.SupplierID = EWO.ContactID 
	                    left Join {DbNames.EPYSL}..StyleMaster SM On SM.StyleMasterID = EWO.StyleMasterID
	                    left Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = SM.BuyerTeamID
	                    Left Join {DbNames.EPYSL}..BookingItemAcknowledge BIA On BIA.BookingID = BC.BookingID
	                    Left JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA On FBA.BookingID = BC.BookingID
	                    Left JOIN {TableNames.YarnBookingMaster_New} YBM On YBM.BookingID = BC.BookingID and YBM.ExportOrderID = EWO.ExportOrderID
                        Left Join {DbNames.EPYSL}..Contacts c On c.ContactID = BC.BuyerID 
                    )  
                    , F As 
                    (
	                    Select ExportOrderID,ExportOrderNo,BookingNo,BuyerTeam,FabricRevision,SCAcknowledgeDate,SCAcknowledgeStatus,
                        PMCAcknowledgeDate,PMCAcknowledgeStatus, YarnBookingDate,YarnAcknowledgeStatus,RevStatus,YBookingNo, 
                        IsFabricCancel = Convert(bit,Max(Convert(Int,ISNULL(IsFabricCancel,0)))), 
                        IsCancel = Convert(bit,Max(Convert(Int,ISNULL(IsCancel,0)))), BuyerName, IsCompleteDelivery
	                    From StatusList Where YBookingNo != ''
	                    Group By ExportOrderID,ExportOrderNo,BookingNo,BuyerTeam,FabricRevision,SCAcknowledgeDate,SCAcknowledgeStatus,
                        PMCAcknowledgeDate,PMCAcknowledgeStatus, YarnBookingDate,YarnAcknowledgeStatus,RevStatus,YBookingNo, 
                        BuyerName, IsCompleteDelivery                     
                    )

                    Select *,  Count(*) Over() TotalRows From F";

                    if (paginationInfo.OrderBy.IsNullOrEmpty()) orderBy = "Order By ExportOrderNo Desc";
                }
                else
                {
                    string filterQuery1 = "";
                    string filterQuery2 = "";
                    if (status == Status.Revise)
                    {
                        filterQuery1 = "And FBA.RevisionNo > ISNULL(YB.PreProcessRevNo,0) And YB.AdditionalBooking = 0";
                        filterQuery2 = "And ISNULL(FBA.RevisionNo,YB.PreProcessRevNo) > ISNULL(YB.PreProcessRevNo,0) And YB.AdditionalBooking = 0";
                    }
                    else if (status == Status.Completed)
                    {
                        filterQuery1 = "And ((FBA.RevisionNo <= ISNULL(YB.PreProcessRevNo,0) And YB.AdditionalBooking = 0) Or YB.AdditionalBooking >= 1) And YB.Propose = 0";
                        filterQuery2 = "And ISNULL(FBA.RevisionNo,YB.PreProcessRevNo) <= ISNULL(YB.PreProcessRevNo,0) And YB.Propose = 0";
                    }
                    else if (status == Status.Proposed)
                    {
                        filterQuery1 = "And ((FBA.RevisionNo <= ISNULL(YB.PreProcessRevNo,0) And YB.AdditionalBooking = 0) Or YB.AdditionalBooking >= 1) And YB.Propose = 1 And YB.Acknowledge = 0";
                        filterQuery2 = "And ISNULL(FBA.RevisionNo,YB.PreProcessRevNo) <= ISNULL(YB.PreProcessRevNo,0) And YB.Propose = 1 And YB.Acknowledge = 0";
                    }
                    else if (status == Status.Executed)
                    {
                        filterQuery1 = "And ((FBA.RevisionNo <= ISNULL(YB.PreProcessRevNo,0) And YB.AdditionalBooking = 0) Or YB.AdditionalBooking >= 1) And YB.Propose = 1 And YB.Acknowledge = 1 ";
                        filterQuery2 = "And ISNULL(FBA.RevisionNo,YB.PreProcessRevNo) <= ISNULL(YB.PreProcessRevNo,0) And YB.Propose = 1 And YB.Acknowledge = 1 ";
                    }
                    else if (status == Status.Additional)
                    {
                        filterQuery1 = "And FBA.RevisionNo <= ISNULL(YB.PreProcessRevNo,0) And YB.Propose = 1 And YB.Acknowledge = 1 And YB.AdditionalBooking = 0";
                        filterQuery2 = "And ISNULL(FBA.RevisionNo,YB.PreProcessRevNo) <= ISNULL(YB.PreProcessRevNo,0) And YB.Propose = 1 And YB.Acknowledge = 1 And YB.AdditionalBooking = 0 ";
                    }

                    sql = $@"
                    With BIMG As(-------Bulk Booking Image
	                    Select BookingID, Min(ChildImgID) ChildImgID 
	                    From {DbNames.EPYSL}..BookingChildImage Group By BookingID
                    ), IMG As(
	                    Select I.BookingID, I.ImagePath
	                    From BIMG
	                    Inner Join {DbNames.EPYSL}..BookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                    ), SBIMG As(-------Sample Booking Image
	                    Select BookingID, Min(ChildImgID) ChildImgID 
	                    From {DbNames.EPYSL}..SampleBookingChildImage Group By BookingID
                    ), SIMG As(
	                    Select I.BookingID, I.ImagePath
	                    From SBIMG BIMG
	                    Inner Join {DbNames.EPYSL}..SampleBookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                    ),
                    FirstYBM As
                    (
	                    Select YBookingID = Min(YBookingID), YBookingNo 
	                    FROM {TableNames.YarnBookingMaster_New}
	                    Group By YBookingNo
                    ),
                    YBL As
                    (
	                    Select FBA.BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
                        YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID,
                        AcknowledgeStatus=Case When YB.Propose=0 Then 'N/A' When YB.Propose = 1 And YB.Acknowledge=0 Then 'Pending' Else 'Acknowledged' End, BM.BookingNo,EOM.ExportOrderNo,
	                    ReferenceNo = ISNULL(BM.ReferenceNo,''), EOM.CalendarDays As TNADays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID, 
	                    RequiredDate = ISNULL(YB.FabricInHouseDate, 
                        Case When IGST.GoupSubTypeName = 'Main Fabric' Then EOM.FabricEDD When IGST.GoupSubTypeName = 'Sewing' Then EOM.SewingEDD When IGST.GoupSubTypeName = 'Finishing' Then EOM.SewingEDD Else EOM.FabricEDD End),
	                    YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, 
                        YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB, 
                        GroupName = Case When ISG.SubGroupName in ('Fabric','Collar','Cuff') then 'Fabric' Else '' End, 
                        YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy, BM.IsSample, YB.YInHouseDate, 
                        YB.DateRevised, IsCompleteDelivery = Min(convert(int, BC.IsCompleteDelivery)), ISNULL(IMG.ImagePath,'') ImagePath
	                    From (Select * FROM {TableNames.YarnBookingMaster_New} Where WithoutOB = 0) YB
                        Left Join IMG ON IMG.BookingID = YB.BookingID
	                    Inner Join (Select * FROM {TableNames.FabricBookingAcknowledge} Where Status = 1  And WithoutOB = 0) FBA On FBA.BookingID = YB.BookingID
	                    Inner Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingID = YB.BookingID
                        Inner Join {DbNames.EPYSL}..BookingChild BC On BC.BookingID = BM.BookingID
	                    Inner Join {DbNames.EPYSL}..ExportOrderMaster EOM On EOM.ExportOrderID = BM.ExportOrderID
	                    left Join {DbNames.EPYSL}..ItemGroup IG On IG.ItemGroupID = FBA.ItemGroupID
	                    left Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = FBA.SubGroupID
	                    left Join {DbNames.EPYSL}..ItemGroupSubType IGST On IGST.GroupSubTypeID = IG.GroupSubTypeID
	                    Where EOM.EWOStatusID = 130 And ISG.SubGroupName in ('Fabric','Collar','Cuff') --And SM.BuyerID = 1 And SM.BuyerTeamID = 0  
                        {filterQuery1}
                        Group By FBA.BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
	                    YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID, BM.BookingNo, EOM.ExportOrderNo,
	                    ISNULL(BM.ReferenceNo,''), EOM.CalendarDays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID, 
	                    YB.FabricInHouseDate, IGST.GoupSubTypeName, EOM.FabricEDD, EOM.SewingEDD, YB.HoldYP, YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, 
	                    YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB, 
                        ISG.SubGroupName, YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy, BM.IsSample, YB.YInHouseDate, YB.DateRevised, ISNULL(IMG.ImagePath,'')

	                    Union All

	                    Select 0 BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
                        YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID,
	                    AcknowledgeStatus=Case When YB.Propose=0 Then 'N/A' When YB.Propose = 1 And YB.Acknowledge=0 Then 'Pending' Else 'Acknowledged' End, 
                        BM.BookingNo,EOM.ExportOrderNo,
	                    ReferenceNo = Case When ISNULL(BM.ReferenceNo,'') = '' then 'Sample' Else ISNULL(BM.ReferenceNo,'') End, EOM.CalendarDays As TNADays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID,
	                    RequiredDate = ISNULL(YB.FabricInHouseDate,BM.FirstInHouseDate), YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
	                    'Fabric' GroupName, YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy, IsSample = Convert(bit,1),
                        YB.YInHouseDate, YB.DateRevised, 0 As IsCompleteDelivery, ISNULL(IMG.ImagePath,'') ImagePath
	 
						FROM {TableNames.YarnBookingMaster_New}  YB
	                    Inner JOIN {TableNames.FabricBookingAcknowledge} FBA On FBA.BookingID = YB.BookingID										
	                    Inner Join {DbNames.EPYSL}..SampleBookingMaster BM On BM.BookingID = YB.BookingID
	                    Inner Join {DbNames.EPYSL}..ExportOrderMaster EOM On EOM.ExportOrderNo = BM.SLNo
                        Left Join SIMG IMG ON IMG.BookingID = YB.BookingID
	                    Where EOM.EWOStatusID = 130 And YB.WithoutOB = 1 And ISNULL(FBA.Status,0) = 1--And BM.BuyerID = 1 And BM.BuyerTeamID = 0 
                        And ISNULL(FBA.RevisionNo,YB.PreProcessRevNo) <= ISNULL(YB.PreProcessRevNo,0)
                        {filterQuery2}
                        Group By YB.YBookingNo, YB.YBookingID, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, 
                        YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, BM.BuyerID,
	                    Case When YB.Propose=0 Then 'N/A' When YB.Propose = 1 And YB.Acknowledge=0 Then 'Pending' Else 'Acknowledged' End, 
                        BM.BookingNo,EOM.ExportOrderNo,
	                    Case When ISNULL(BM.ReferenceNo,'') = '' then 'Sample' Else ISNULL(BM.ReferenceNo,'') End, EOM.CalendarDays, YB.AdditionalBooking, YB.YBRevisionNeed, EOM.StyleMasterID,
	                    ISNULL(YB.FabricInHouseDate,BM.FirstInHouseDate), YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
	                    YB.ContactPerson, YB.Remarks, BM.SupplierID, BM.AddedBy,
                        YB.YInHouseDate, YB.DateRevised, ISNULL(IMG.ImagePath,'')
                    ),
                    TmpFinal As
                    (
                        Select top 100 YB.BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, 
                        YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, YB.AcknowledgeStatus, YB.ExportOrderID, 
                        YB.BookingNo, YB.ExportOrderNo, CCT.TeamName BuyerDepartment,
                        MerchandiserName = EMP.EmployeeName, ReferenceNo = ISNULL(YB.ReferenceNo,''), YB.TNADays, YB.AdditionalBooking, 
                        YB.YBRevisionNeed, YB.RequiredDate, 
                        RevStatus = Case When Isnull(EL.FabBTexAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBTexAckStatus,'') <> '' Then 1 Else Case When YB.IsSample = 1 then 1 else 0 End End, 
                        YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, CancelReason = CR.ReasonName, YB.CanceledBy, 
                        YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
                        YB.GroupName, YB.ContactPerson, ContactPersonName = E.EmployeeName, Depertment = D.Designation +' : '+ ED.DepertmentDescription, 
                        YB.Remarks, YB.YInHouseDate, YB.DateRevised, c.ShortName As BuyerName, IsCompleteDelivery, YB.ImagePath
                        from YBL YB
                        Inner Join FirstYBM FYBM On FYBM.YBookingID = YB.YBookingID
                        Left Join {DbNames.EPYSL}..Employee E On E.EmployeeCode = YB.ContactPerson
                        Left Join {DbNames.EPYSL}..EmployeeDepartment ED On ED.DepertmentID = E.DepertmentID
                        Left Join {DbNames.EPYSL}..EmployeeDesignation D On D.DesigID = E.DesigID
                        left Join {DbNames.EPYSL}..StyleMaster SM On SM.StyleMasterID = YB.StyleMasterID
                        left Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = SM.BuyerTeamID
                        Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = YB.ExportOrderID And EL.ContactID = YB.SupplierID And YB.BookingID = EL.BookingID 
                        left Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = YB.AddedBy
                        left Join {DbNames.EPYSL}..Employee EMP ON EMP.EmployeeCode = LU.EmployeeCode
                        Left Join {DbNames.EPYSL}..CancelReason CR On CR.ReasonID = YB.CancelReasonID 
                        Left Join {DbNames.EPYSL}..Contacts c On c.ContactID = YB.BuyerID

                        GROUP BY YB.BOMMasterID, YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, 
                        YB.BookingID, YB.YRequiredDate, YB.Propose, YB.Acknowledge, YB.AcknowledgeDate, YB.AcknowledgeStatus, YB.ExportOrderID, 
                        YB.BookingNo, YB.ExportOrderNo, CCT.TeamName,
                        EMP.EmployeeName, ISNULL(YB.ReferenceNo,''), YB.TNADays, YB.AdditionalBooking, 
                        YB.YBRevisionNeed, YB.RequiredDate, 
                        Case When Isnull(EL.FabBTexAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBTexAckStatus,'') <> '' Then 1 Else Case When YB.IsSample = 1 then 1 else 0 End End, 
                        YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, CR.ReasonName, YB.CanceledBy, 
                        YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
                        YB.GroupName, YB.ContactPerson, E.EmployeeName, D.Designation,ED.DepertmentDescription, 
                        YB.Remarks, YB.YInHouseDate, YB.DateRevised, c.ShortName, IsCompleteDelivery, YB.ImagePath
                        Order By YB.YBookingID desc
                    )
                    Select *, Count(*) Over() TotalRows From TmpFinal ";

                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YBookingID Desc" : paginationInfo.OrderBy;
                }
            }
            else
            {
                if (status == Status.AwaitingPropose)
                {
                    sql =
                    $@"
                    With BIMG As(
	                    Select BookingID, Min(ChildImgID) ChildImgID 
	                    From {DbNames.EPYSL}..BookingChildImage Group By BookingID
                    ), IMG As(
	                    Select I.BookingID, I.ImagePath
	                    From BIMG
	                    Inner Join {DbNames.EPYSL}..BookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                    ), SBIMG As(
	                    Select BookingID, Min(ChildImgID) ChildImgID 
	                    From {DbNames.EPYSL}..SampleBookingChildImage Group By BookingID
                    ), SIMG As(
	                    Select I.BookingID, I.ImagePath
	                    From SBIMG BIMG
	                    Inner Join {DbNames.EPYSL}..SampleBookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                    ), YBK AS (
	                    Select MIN(BookingID) BookingID, WithoutOB, YBookingNo, Acknowledge, PreProcessRevNo, Propose,
						IsYarnStock, RevisionNo, YBookingDate, YInHouseDate, YRequiredDate, ContactPerson,
						ProposeDate = CONVERT(date,ProposeDate), HasYPrice, YPRevisionNo, ApproveFP, RevisionReason, AcknowledgeDate, Remarks, AddedBy,
						DateAdded = CONVERT(date,DateAdded), UpdatedBy,  DateUpdated = CONVERT(date,DateUpdated), DateRevised, ApproveYP, UnApproveYP, YPPreProcessRevNo, FPRevisionNo,
						FPPreProcessRevNo, PORevisionNeed, FPRejectReason, HoldYP, HoldYPBy, HoldYPDate, HoldYPReason, IsCancel,
						CancelReasonID, CanceledBy, AdditionalBooking, YBRevisionNeed, DateCanceled, Exported, BuyerID, BuyerTeamID,
						CompanyID, AllowForNextStep, TNADays, FabricInHouseDate, ExportOrderID, ExportNo, DateExported
						FROM {TableNames.YarnBookingMaster_New} YBM
						where ISNULL(IsCancel,0) = 0 AND Propose = 1 AND Acknowledge = 0
						GROUP BY WithoutOB, YBookingNo, Acknowledge, PreProcessRevNo, Propose,
						IsYarnStock, RevisionNo, YBookingDate, YInHouseDate, YRequiredDate, ContactPerson,
						CONVERT(date,ProposeDate), HasYPrice, YPRevisionNo, ApproveFP, RevisionReason, AcknowledgeDate, Remarks, AddedBy,
						CONVERT(date,DateAdded), UpdatedBy, CONVERT(date,DateUpdated), DateRevised, ApproveYP, UnApproveYP, YPPreProcessRevNo, FPRevisionNo,
						FPPreProcessRevNo, PORevisionNeed, FPRejectReason, HoldYP, HoldYPBy, HoldYPDate, HoldYPReason, IsCancel,
						CancelReasonID, CanceledBy, AdditionalBooking, YBRevisionNeed, DateCanceled, Exported, BuyerID, BuyerTeamID,
						CompanyID, AllowForNextStep, TNADays, FabricInHouseDate, ExportOrderID, ExportNo, DateExported
                    ),
                    BM As (
	                    Select * from {DbNames.EPYSL}..BookingMaster Where SubGroupID In (1,11,12)
                    ),
                    SBM As (
	                    Select * from {DbNames.EPYSL}..SampleBookingMaster
                    ),YB AS (
	                    Select Y.*, BM.BookingNo, ISNULL(IMG.ImagePath,'') ImagePath
	                    From (Select * from YBK Where WithoutOB = 0) Y
	                    Inner Join BM On BM.BookingID = Y.BookingID 
                        Left Join IMG ON IMG.BookingID = BM.BookingID
                    ),SYB AS (
	                    Select Y.*, SBM.BookingNo, SBKRevisionNo = SBM.RevisionNo, ISNULL(IMG.ImagePath,'') ImagePath
	                    From (Select * from YBK Where WithoutOB = 1) Y
	                    Inner Join SBM On SBM.BookingID = Y.BookingID
                        Left Join SIMG IMG ON IMG.BookingID = SBM.BookingID
                    ), PYB AS (
	                    Select YB.YBookingNo, FYA.BOMMasterID, MinAck = Min(Convert(int,Case When YB.Acknowledge = 1 Then 1 Else 0 End)), MaxAck = Max(Convert(int,Case When YB.Acknowledge = 1 Then 1 Else 0 End))
	                    ,HasRevision = Convert(bit,Max(Convert(int,Case When YB.PreProcessRevNo >= FYA.RevisionNo Then 0 Else 1 End)))
	                    From YB
	                    Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FYA On FYA.BookingID = YB.BookingID And FYA.WithoutOB = YB.WithoutOB
	                    Group By YB.YBookingNo, FYA.BOMMasterID
	                    Having Min(Convert(int,Case When YB.Propose = 1 Then 1 Else 0 End)) = 1 --And Min(Convert(int,Case When FYA.Status = 1 Then 1 Else 0 End)) = 1
                    ), PSYB AS (
	                    Select YB.YBookingNo, 0 BOMMasterID, MinAck = Min(Convert(int,Case When YB.Acknowledge = 1 Then 1 Else 0 End)), MaxAck = Max(Convert(int,Case When YB.Acknowledge = 1 Then 1 Else 0 End))
	                    ,HasRevision = Convert(bit,Max(Convert(int,Case When YB.PreProcessRevNo >= FYA.RevisionNo Then 0 Else 1 End))), BookingID = Min(YB.BookingID)
	                    From SYB YB
                        Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FYA On FYA.BookingID = YB.BookingID And FYA.WithoutOB = YB.WithoutOB
	                    Group By YB.YBookingNo
	                    Having Min(Convert(int,Case When YB.Propose = 1 Then 1 Else 0 End)) = 1
                    ),BC AS (
	                    SELECT BKM.BookingNo, BKC.BookingID, BKM.SupplierID, BKC.ConsumptionID, BKC.ItemMasterID, BKC.A1ValueID, BKC.YarnBrandID,ISG.SubGroupName
	                    FROM {DbNames.EPYSL}..BookingChild BKC
	                    Inner Join BM BKM On BKM.BookingID = BKC.BookingID
	                    Inner Join {DbNames.EPYSL}..BOMConsumption BOMCon On BOMCon.ConsumptionID = BKC.ConsumptionID and BKC.BOMmasterid = BOMCon.BOMmasterid
	                    Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = BOMCon.SubGroupID
	                    Where ISG.SubGroupName In ('Fabric','Collar','Cuff') and BKC.ISourcing = 1
	                    Group By BKM.BookingNo, BKC.BookingID, BKM.SupplierID, BKC.ConsumptionID, BKC.ItemMasterID, BKC.A1ValueID, BKC.YarnBrandID,ISG.SubGroupName
                    ),SBC AS (
	                    SELECT BKM.BookingNo, BKC.BookingID, BKM.SupplierID, BKC.ConsumptionID, BKC.ItemMasterID, SBCon.A1ValueID, SBCon.YarnBrandID,ISG.SubGroupName
	                    FROM {DbNames.EPYSL}..SampleBookingConsumptionChild BKC
	                    Inner Join SBM BKM On BKM.BookingID = BKC.BookingID
	                    Inner Join {DbNames.EPYSL}..SampleBookingConsumption SBCon On SBCon.ConsumptionID = BKC.ConsumptionID and BKC.BookingID = SBCon.BookingID
	                    Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SBCon.SubGroupID
	                    Where ISG.SubGroupName In ('Fabric','Collar','Cuff') 
	                    Group By BKM.BookingNo, BKC.BookingID, BKM.SupplierID, BKC.ConsumptionID, BKC.ItemMasterID, SBCon.A1ValueID, SBCon.YarnBrandID,ISG.SubGroupName
                    ),YBC AS (	
	                    SELECT YB.BookingID, YBC.ConsumptionID, YBC.ItemMasterID, YBC.YarnTypeID, YBC.YarnBrandID 
	                    FROM {TableNames.YarnBookingChild_New} YBC
						LEFT JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
	                    Inner Join YB On YB.BookingID = YBM.BookingID 
	                    Group By YB.BookingID, YBC.ConsumptionID, YBC.ItemMasterID, YBC.YarnTypeID, YBC.YarnBrandID
                    ),SYBC AS (	
	                    SELECT YB.BookingID, YBC.ConsumptionID, YBC.ItemMasterID, YBC.YarnTypeID, YBC.YarnBrandID 
	                    FROM {TableNames.YarnBookingChild_New} YBC
						LEFT JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
	                    Inner Join SYB YB On YB.BookingID = YBM.BookingID
	                    Group By YB.BookingID, YBC.ConsumptionID, YBC.ItemMasterID, YBC.YarnTypeID, YBC.YarnBrandID
                    ), YBP AS(
	                    Select BC.BookingNo, SupplierID = Max(Convert(int,BC.SupplierID)), FBookingID = Max(Convert(int,BC.BookingID)), 
	                    IsPendingYB =  Convert(bit,Min(Convert(int,Case When ISNULL(YBC.ItemMasterID,0) = 0 then 1 Else 0 End)))
	                    From BC 
	                    Inner Join YBC On YBC.BookingID = BC.BookingID And YBC.ConsumptionID = BC.ConsumptionID And YBC.YarnBrandID = BC.YarnBrandID And YBC.YarnTypeID = BC.A1ValueID And YBC.ItemMasterID = BC.ItemMasterID
	                    group by BC.BookingNo
                    ), SYBP AS(
	                    Select BC.BookingNo, SupplierID = Max(Convert(int,BC.SupplierID)), FBookingID = Max(Convert(int,BC.BookingID)), 
	                    IsPendingYB =  Convert(bit,Min(Convert(int,Case When ISNULL(YBC.ItemMasterID,0) = 0 then 1 Else 0 End)))
	                    From SBC BC 
	                    Inner Join SYBC YBC On YBC.BookingID = BC.BookingID And YBC.ConsumptionID = BC.ConsumptionID And YBC.YarnBrandID = BC.YarnBrandID And YBC.YarnTypeID = BC.A1ValueID And YBC.ItemMasterID = BC.ItemMasterID
	                    group by BC.BookingNo
                    ), YBA As(
	                    Select YB.IsYarnStock,YB.BookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.YInHouseDate, YB.YRequiredDate, 
	                    YB.ContactPerson, YB.Propose, YB.ProposeDate, YB.Acknowledge, YB.AcknowledgeDate, YB.Remarks, YB.AddedBy, YB.DateAdded, YB.UpdatedBy, YB.DateUpdated, YB.DateRevised,
	                    YB.HasYPrice, YB.YPRevisionNo, YB.ApproveFP, YB.RevisionReason, YB.ApproveYP, YB.UnApproveYP, YB.YPPreProcessRevNo, YB.FPRevisionNo, YB.FPPreProcessRevNo, YB.PORevisionNeed,AcknowledgeStatus=Case When YB.Acknowledge = 0 Then 'Pending' Else 'Acknowledged' End,
	                    PYB.BOMMasterID, 'Fabric' GroupName, BookingNo = YBP.BookingNo,BookingDate = BM.BookingDate, EOM.ExportOrderNo, EOM.ExportOrderID, EOM.StyleMasterID, EOM.FabricEDD,
	                    RequiredDate = EOM.FabricEDD, ReferenceNo = ISNULL(BM.ReferenceNo,''), EOM.CalendarDays, PYB.HasRevision, YBP.IsPendingYB, YB.AdditionalBooking, YB.YBRevisionNeed,
	                    YB.FPRejectReason,YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB, YBP.SupplierID,
                        YB.BuyerID, YB.BuyerTeamID, YB.CompanyID, YB.AllowForNextStep, YB.TNADays, YB.FabricInHouseDate, YB.ImagePath											
                        From YB
	                    Inner Join YBP On YBP.BookingNo = YB.BookingNo
	                    Inner Join PYB On PYB.YBookingNo = YB.YBookingNo
	                    Inner Join (Select * from {DbNames.EPYSL}..ExportOrderMaster where EWOStatusID = 130) EOM On EOM.ExportOrderID = YB.ExportOrderID
	                    Inner Join BM On BM.BookingID = YB.BookingID
	                    Where (PYB.MinAck = 0 And PYB.MaxAck = 0 And YB.RevisionNo = 0)
	                    Union All
	                    Select YB.IsYarnStock,YB.BookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.YInHouseDate, YB.YRequiredDate, 
	                    YB.ContactPerson, YB.Propose, YB.ProposeDate, YB.Acknowledge, YB.AcknowledgeDate, YB.Remarks, YB.AddedBy, YB.DateAdded, YB.UpdatedBy, YB.DateUpdated, YB.DateRevised,
	                    YB.HasYPrice, YB.YPRevisionNo, YB.ApproveFP, YB.RevisionReason, YB.ApproveYP, YB.UnApproveYP, YB.YPPreProcessRevNo, YB.FPRevisionNo, YB.FPPreProcessRevNo, YB.PORevisionNeed,AcknowledgeStatus=Case When YB.Acknowledge = 0 Then 'Pending' Else 'Acknowledged' End,
	                    PYB.BOMMasterID, 'Fabric' GroupName, BookingNo = YBP.BookingNo,BookingDate = BM.BookingDate, EOM.ExportOrderNo, EOM.ExportOrderID, EOM.StyleMasterID, EOM.FabricEDD,
	                    RequiredDate = EOM.FabricEDD, ReferenceNo = Case When ISNULL(BM.ReferenceNo,'') = '' and YB.WithoutOB = 1 then 'Sample' Else ISNULL(BM.ReferenceNo,'') End, EOM.CalendarDays, PYB.HasRevision, YBP.IsPendingYB, YB.AdditionalBooking, YB.YBRevisionNeed,
	                    YB.FPRejectReason,YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB, YBP.SupplierID,
                        YB.BuyerID, YB.BuyerTeamID, YB.CompanyID, YB.AllowForNextStep, YB.TNADays, YB.FabricInHouseDate, YB.ImagePath
	                    From SYB YB
	                    Inner Join SYBP YBP On YBP.BookingNo = YB.BookingNo
	                    Inner Join PSYB PYB On PYB.YBookingNo = YB.YBookingNo
	                    Inner Join SBM BM On BM.BookingID = YB.BookingID
	                    Inner Join (Select * from {DbNames.EPYSL}..ExportOrderMaster where EWOStatusID = 130) EOM On EOM.ExportOrderNo = BM.SLNo
	                    Where (PYB.MinAck = 0 And PYB.MaxAck = 0 And YB.RevisionNo = 0)
                    ),
                    FinalQ AS
                    (
						Select YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, YB.BookingID, YB.YInHouseDate, YB.YRequiredDate, 
						YB.ContactPerson, YB.Propose, YB.ProposeDate, YB.Acknowledge, YB.AcknowledgeDate, YB.Remarks, YB.AddedBy, YB.DateAdded, YB.UpdatedBy, YB.DateUpdated, YB.DateRevised,
						YB.HasYPrice, YB.YPRevisionNo, YB.ApproveYP, YB.UnApproveYP, YB.ApproveFP, YB.RevisionReason, YB.YPPreProcessRevNo, YB.FPRevisionNo, YB.FPPreProcessRevNo, YB.PORevisionNeed,AcknowledgeStatus=Case When YB.Acknowledge = 0 Then 'Pending' Else 'Acknowledged' End,
						YB.BOMMasterID,SM.StyleMasterID,'Fabric' GroupName, 
						BookingNo = YB.BookingNo,BookingDate = YB.BookingDate, YB.ExportOrderNo, CCT.TeamName BuyerDepartment, MerchandiserName = EMP.EmployeeName, FY.YearName,ContactPersonName = E.EmployeeName,Depertment =D.Designation +' : '+ ED.DepertmentDescription, 
						RequiredDate = YB.FabricEDD, ReferenceNo = ISNULL(YB.ReferenceNo,''), YB.CalendarDays, YB.HasRevision, YB.IsPendingYB, YB.AdditionalBooking, YB.YBRevisionNeed,
						RevStatus = Case When WithoutOB = 1 then 1 Else Case When (Isnull(EL.YBStatus,'') = 'Pending for Acknowledge' or Isnull(EL.YBStatus,'') = 'Acknowledged' or Isnull(EL.YBStatus,'') = 'Acknowledged with Additional Booking') And Isnull(EL.YBStatus,'') <> '' Then 1 Else 0 End End, 
						YB.FPRejectReason,YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
						YB.BuyerID, c.ShortName As BuyerName, YB.BuyerTeamID, YB.CompanyID, YB.AllowForNextStep, YB.TNADays, YB.FabricInHouseDate,YB.IsYarnStock, YB.ImagePath
						From YBA YB
						Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = YB.ExportOrderID And EL.BookingID = YB.BookingID And YB.SupplierID = EL.ContactID
						left Join {DbNames.EPYSL}..StyleMaster SM On SM.StyleMasterID = YB.StyleMasterID
						left Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = YB.AddedBy
						left Join {DbNames.EPYSL}..Employee EMP ON EMP.EmployeeCode = LU.EmployeeCode
						left Join {DbNames.EPYSL}..FinancialYear FY On FY.FinancialYearID = SM.FinancialYearID
						left Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = SM.BuyerTeamID
						left join {DbNames.EPYSL}..Contacts C On C.ContactID = YB.BuyerID
						Left Join {DbNames.EPYSL}..Employee E On E.EmployeeCode = YB.ContactPerson
						Left Join {DbNames.EPYSL}..EmployeeDepartment ED On ED.DepertmentID = E.DepertmentID
						Left Join {DbNames.EPYSL}..EmployeeDesignation D On D.DesigID = E.DesigID
					)
                    SELECT *, Count(*) Over() TotalRows FROM FinalQ ";

                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YBookingNo Desc" : paginationInfo.OrderBy;
                }
                /*
                if (status == Status.AwaitingPropose)
                {
                    sql =
                    $@"
                    With BIMG As(
	                    Select BookingID, Min(ChildImgID) ChildImgID 
	                    From {DbNames.EPYSL}..BookingChildImage Group By BookingID
                    ), IMG As(
	                    Select I.BookingID, I.ImagePath
	                    From BIMG
	                    Inner Join {DbNames.EPYSL}..BookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                    ), SBIMG As(
	                    Select BookingID, Min(ChildImgID) ChildImgID 
	                    From {DbNames.EPYSL}..SampleBookingChildImage Group By BookingID
                    ), SIMG As(
	                    Select I.BookingID, I.ImagePath
	                    From SBIMG BIMG
	                    Inner Join {DbNames.EPYSL}..SampleBookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                    ), YBK AS (
	                    Select * FROM {TableNames.YarnBookingMaster_New} where ISNULL(IsCancel,0) = 0
                    ),
                    BM As (
	                    Select * from {DbNames.EPYSL}..BookingMaster Where SubGroupID In (1,11,12)
                    ),
                    SBM As (
	                    Select * from {DbNames.EPYSL}..SampleBookingMaster
                    ),YB AS (
	                    Select Y.*, BM.BookingNo, ISNULL(IMG.ImagePath,'') ImagePath
	                    From (Select * from YBK Where WithoutOB = 0) Y
	                    Inner Join BM On BM.BookingID = Y.BookingID 
                        Left Join IMG ON IMG.BookingID = BM.BookingID
                    ),SYB AS (
	                    Select Y.*, SBM.BookingNo, SBKRevisionNo = SBM.RevisionNo, ISNULL(IMG.ImagePath,'') ImagePath
	                    From (Select * from YBK Where WithoutOB = 1) Y
	                    Inner Join SBM On SBM.BookingID = Y.BookingID
                        Left Join SIMG IMG ON IMG.BookingID = SBM.BookingID
                    ), PYB AS (
	                    Select YB.YBookingNo, FYA.BOMMasterID, MinAck = Min(Convert(int,Case When YB.Acknowledge = 1 Then 1 Else 0 End)), MaxAck = Max(Convert(int,Case When YB.Acknowledge = 1 Then 1 Else 0 End))
	                    ,HasRevision = Convert(bit,Max(Convert(int,Case When YB.PreProcessRevNo >= FYA.RevisionNo Then 0 Else 1 End)))
	                    From YB
	                    Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FYA On FYA.BookingID = YB.BookingID And FYA.WithoutOB = YB.WithoutOB
	                    Group By YB.YBookingNo, FYA.BOMMasterID
	                    Having Min(Convert(int,Case When YB.Propose = 1 Then 1 Else 0 End)) = 1 --And Min(Convert(int,Case When FYA.Status = 1 Then 1 Else 0 End)) = 1
                    ), PSYB AS (
	                    Select YB.YBookingNo, 0 BOMMasterID, MinAck = Min(Convert(int,Case When YB.Acknowledge = 1 Then 1 Else 0 End)), MaxAck = Max(Convert(int,Case When YB.Acknowledge = 1 Then 1 Else 0 End))
	                    ,HasRevision = Convert(bit,Max(Convert(int,Case When YB.PreProcessRevNo >= FYA.RevisionNo Then 0 Else 1 End))), BookingID = Min(YB.BookingID), SubGroupID = Min(YB.SubGroupID)
	                    From SYB YB
                        Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FYA On FYA.BookingID = YB.BookingID And FYA.WithoutOB = YB.WithoutOB
	                    Group By YB.YBookingNo
	                    Having Min(Convert(int,Case When YB.Propose = 1 Then 1 Else 0 End)) = 1
                    ),BC AS (
	                    SELECT BKM.BookingNo, BKC.BookingID, BKM.SupplierID, BKC.ConsumptionID, BKC.ItemMasterID, BKC.A1ValueID, BKC.YarnBrandID,ISG.SubGroupName
	                    FROM {DbNames.EPYSL}..BookingChild BKC
	                    Inner Join BM BKM On BKM.BookingID = BKC.BookingID
	                    Inner Join {DbNames.EPYSL}..BOMConsumption BOMCon On BOMCon.ConsumptionID = BKC.ConsumptionID and BKC.BOMmasterid = BOMCon.BOMmasterid
	                    Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = BOMCon.SubGroupID
	                    Where ISG.SubGroupName In ('Fabric','Collar','Cuff') and BKC.ISourcing = 1
	                    Group By BKM.BookingNo, BKC.BookingID, BKM.SupplierID, BKC.ConsumptionID, BKC.ItemMasterID, BKC.A1ValueID, BKC.YarnBrandID,ISG.SubGroupName
                    ),SBC AS (
	                    SELECT BKM.BookingNo, BKC.BookingID, BKM.SupplierID, BKC.ConsumptionID, BKC.ItemMasterID, SBCon.A1ValueID, SBCon.YarnBrandID,ISG.SubGroupName
	                    FROM {DbNames.EPYSL}..SampleBookingConsumptionChild BKC
	                    Inner Join SBM BKM On BKM.BookingID = BKC.BookingID
	                    Inner Join {DbNames.EPYSL}..SampleBookingConsumption SBCon On SBCon.ConsumptionID = BKC.ConsumptionID and BKC.BookingID = SBCon.BookingID
	                    Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SBCon.SubGroupID
	                    Where ISG.SubGroupName In ('Fabric','Collar','Cuff') 
	                    Group By BKM.BookingNo, BKC.BookingID, BKM.SupplierID, BKC.ConsumptionID, BKC.ItemMasterID, SBCon.A1ValueID, SBCon.YarnBrandID,ISG.SubGroupName
                    ),YBC AS (	
	                    SELECT YB.BookingID, YBC.ConsumptionID, YBC.ItemMasterID, YBC.YarnTypeID, YBC.YarnBrandID 
	                    FROM {TableNames.YarnBookingChild_New} YBC
	                    Inner Join YB On YB.YBookingID = YBC.YBookingID 
	                    Group By YB.BookingID, YBC.ConsumptionID, YBC.ItemMasterID, YBC.YarnTypeID, YBC.YarnBrandID
                    ),SYBC AS (	
	                    SELECT YB.BookingID, YBC.ConsumptionID, YBC.ItemMasterID, YBC.YarnTypeID, YBC.YarnBrandID 
	                    FROM {TableNames.YarnBookingChild_New} YBC
	                    Inner Join SYB YB On YB.YBookingID = YBC.YBookingID 
	                    Group By YB.BookingID, YBC.ConsumptionID, YBC.ItemMasterID, YBC.YarnTypeID, YBC.YarnBrandID
                    ), YBP AS(
	                    Select BC.BookingNo, SupplierID = Max(Convert(int,BC.SupplierID)), FBookingID = Max(Convert(int,BC.BookingID)), 
	                    IsPendingYB =  Convert(bit,Min(Convert(int,Case When ISNULL(YBC.ItemMasterID,0) = 0 then 1 Else 0 End)))
	                    From BC 
	                    Inner Join YBC On YBC.BookingID = BC.BookingID And YBC.ConsumptionID = BC.ConsumptionID And YBC.YarnBrandID = BC.YarnBrandID And YBC.YarnTypeID = BC.A1ValueID And YBC.ItemMasterID = BC.ItemMasterID
	                    group by BC.BookingNo
                    ), SYBP AS(
	                    Select BC.BookingNo, SupplierID = Max(Convert(int,BC.SupplierID)), FBookingID = Max(Convert(int,BC.BookingID)), 
	                    IsPendingYB =  Convert(bit,Min(Convert(int,Case When ISNULL(YBC.ItemMasterID,0) = 0 then 1 Else 0 End)))
	                    From SBC BC 
	                    Inner Join SYBC YBC On YBC.BookingID = BC.BookingID And YBC.ConsumptionID = BC.ConsumptionID And YBC.YarnBrandID = BC.YarnBrandID And YBC.YarnTypeID = BC.A1ValueID And YBC.ItemMasterID = BC.ItemMasterID
	                    group by BC.BookingNo
                    ), YBA As(
	                    Select YB.IsYarnStock,YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.BookingID, YB.YInHouseDate, YB.YRequiredDate, 
	                    YB.ContactPerson, YB.Propose, YB.ProposeDate, YB.Acknowledge, YB.AcknowledgeDate, YB.Remarks, YB.AddedBy, YB.DateAdded, YB.UpdatedBy, YB.DateUpdated, YB.DateRevised,
	                    YB.HasYPrice, YB.YPRevisionNo, YB.ApproveFP, YB.RevisionReason, YB.ApproveYP, YB.UnApproveYP, YB.YPPreProcessRevNo, YB.FPRevisionNo, YB.FPPreProcessRevNo, YB.PORevisionNeed,AcknowledgeStatus=Case When YB.Acknowledge = 0 Then 'Pending' Else 'Acknowledged' End,
	                    PYB.BOMMasterID, 'Fabric' GroupName, BookingNo = YBP.BookingNo,BookingDate = BM.BookingDate, EOM.ExportOrderNo, EOM.ExportOrderID, EOM.StyleMasterID, EOM.FabricEDD,
	                    RequiredDate = EOM.FabricEDD, ReferenceNo = ISNULL(BM.ReferenceNo,''), EOM.CalendarDays, PYB.HasRevision, YBP.IsPendingYB, YB.AdditionalBooking, YB.YBRevisionNeed,
	                    YB.FPRejectReason,YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB, YB.SubGroupID, YBP.SupplierID,
                        YB.BuyerID, YB.BuyerTeamID, YB.CompanyID, YB.AllowForNextStep, YB.TNADays, YB.FabricInHouseDate, YB.ImagePath											
                        From YB
	                    Inner Join YBP On YBP.BookingNo = YB.BookingNo
	                    Inner Join PYB On PYB.YBookingNo = YB.YBookingNo
	                    Inner Join (Select * from {DbNames.EPYSL}..ExportOrderMaster where EWOStatusID = 130) EOM On EOM.ExportOrderID = YB.ExportOrderID
	                    Inner Join BM On BM.BookingID = YB.BookingID
	                    Where (PYB.MinAck = 0 And PYB.MaxAck = 0 And YB.RevisionNo = 0)
	                    Union All
	                    Select YB.IsYarnStock,YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.BookingID, YB.YInHouseDate, YB.YRequiredDate, 
	                    YB.ContactPerson, YB.Propose, YB.ProposeDate, YB.Acknowledge, YB.AcknowledgeDate, YB.Remarks, YB.AddedBy, YB.DateAdded, YB.UpdatedBy, YB.DateUpdated, YB.DateRevised,
	                    YB.HasYPrice, YB.YPRevisionNo, YB.ApproveFP, YB.RevisionReason, YB.ApproveYP, YB.UnApproveYP, YB.YPPreProcessRevNo, YB.FPRevisionNo, YB.FPPreProcessRevNo, YB.PORevisionNeed,AcknowledgeStatus=Case When YB.Acknowledge = 0 Then 'Pending' Else 'Acknowledged' End,
	                    PYB.BOMMasterID, 'Fabric' GroupName, BookingNo = YBP.BookingNo,BookingDate = BM.BookingDate, EOM.ExportOrderNo, EOM.ExportOrderID, EOM.StyleMasterID, EOM.FabricEDD,
	                    RequiredDate = EOM.FabricEDD, ReferenceNo = Case When ISNULL(BM.ReferenceNo,'') = '' and YB.WithoutOB = 1 then 'Sample' Else ISNULL(BM.ReferenceNo,'') End, EOM.CalendarDays, PYB.HasRevision, YBP.IsPendingYB, YB.AdditionalBooking, YB.YBRevisionNeed,
	                    YB.FPRejectReason,YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB, YB.SubGroupID, YBP.SupplierID,
                        YB.BuyerID, YB.BuyerTeamID, YB.CompanyID, YB.AllowForNextStep, YB.TNADays, YB.FabricInHouseDate, YB.ImagePath
	                    From SYB YB
	                    Inner Join SYBP YBP On YBP.BookingNo = YB.BookingNo
	                    Inner Join PSYB PYB On PYB.YBookingNo = YB.YBookingNo And PYB.SubGroupID = YB.SubGroupID
	                    Inner Join SBM BM On BM.BookingID = YB.BookingID
	                    Inner Join (Select * from {DbNames.EPYSL}..ExportOrderMaster where EWOStatusID = 130) EOM On EOM.ExportOrderNo = BM.SLNo
	                    Where (PYB.MinAck = 0 And PYB.MaxAck = 0 And YB.RevisionNo = 0)
                    ),
                    FinalQ AS
                    (Select YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, YB.BookingID, YB.YInHouseDate, YB.YRequiredDate, 
                    YB.ContactPerson, YB.Propose, YB.ProposeDate, YB.Acknowledge, YB.AcknowledgeDate, YB.Remarks, YB.AddedBy, YB.DateAdded, YB.UpdatedBy, YB.DateUpdated, YB.DateRevised,
                    YB.HasYPrice, YB.YPRevisionNo, YB.ApproveYP, YB.UnApproveYP, YB.ApproveFP, YB.RevisionReason, YB.YPPreProcessRevNo, YB.FPRevisionNo, YB.FPPreProcessRevNo, YB.PORevisionNeed,AcknowledgeStatus=Case When YB.Acknowledge = 0 Then 'Pending' Else 'Acknowledged' End,
                    YB.BOMMasterID,SM.StyleMasterID,'Fabric' GroupName, 
                    BookingNo = YB.BookingNo,BookingDate = YB.BookingDate, YB.ExportOrderNo, CCT.TeamName BuyerDepartment, MerchandiserName = EMP.EmployeeName, FY.YearName,ContactPersonName = E.EmployeeName,Depertment =D.Designation +' : '+ ED.DepertmentDescription, 
                    RequiredDate = YB.FabricEDD, ReferenceNo = ISNULL(YB.ReferenceNo,''), YB.CalendarDays, YB.HasRevision, YB.IsPendingYB, YB.AdditionalBooking, YB.YBRevisionNeed,
                    RevStatus = Case When WithoutOB = 1 then 1 Else Case When (Isnull(EL.YBStatus,'') = 'Pending for Acknowledge' or Isnull(EL.YBStatus,'') = 'Acknowledged' or Isnull(EL.YBStatus,'') = 'Acknowledged with Additional Booking') And Isnull(EL.YBStatus,'') <> '' Then 1 Else 0 End End, 
                    YB.FPRejectReason,YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB, YB.SubGroupID,
                    YB.BuyerID, c.ShortName As BuyerName, YB.BuyerTeamID, YB.CompanyID, YB.AllowForNextStep, YB.TNADays, YB.FabricInHouseDate,YB.IsYarnStock, YB.ImagePath
                    From YBA YB
                    Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = YB.ExportOrderID And EL.BookingID = YB.BookingID And YB.SupplierID = EL.ContactID
                    left Join {DbNames.EPYSL}..StyleMaster SM On SM.StyleMasterID = YB.StyleMasterID
                    left Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = YB.AddedBy
                    left Join {DbNames.EPYSL}..Employee EMP ON EMP.EmployeeCode = LU.EmployeeCode
                    left Join {DbNames.EPYSL}..FinancialYear FY On FY.FinancialYearID = SM.FinancialYearID
                    left Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = SM.BuyerTeamID
                    left join {DbNames.EPYSL}..Contacts C On C.ContactID = YB.BuyerID
                    Left Join {DbNames.EPYSL}..Employee E On E.EmployeeCode = YB.ContactPerson
                    Left Join {DbNames.EPYSL}..EmployeeDepartment ED On ED.DepertmentID = E.DepertmentID
                    Left Join {DbNames.EPYSL}..EmployeeDesignation D On D.DesigID = E.DesigID)

                    SELECT *, Count(*) Over() TotalRows FROM FinalQ ";

                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YBookingID Desc" : paginationInfo.OrderBy;
                }
                */
                else if (status == Status.Revise)
                {
                    sql = $@"
                    With BIMG As(
	                    Select BookingID, Min(ChildImgID) ChildImgID 
	                    From {DbNames.EPYSL}..BookingChildImage Group By BookingID
                    ), IMG As(
	                    Select I.BookingID, I.ImagePath
	                    From BIMG
	                    Inner Join {DbNames.EPYSL}..BookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                    ), SBIMG As(
	                    Select BookingID, Min(ChildImgID) ChildImgID 
	                    From {DbNames.EPYSL}..SampleBookingChildImage Group By BookingID
                    ), SIMG As(
	                    Select I.BookingID, I.ImagePath
	                    From SBIMG BIMG
	                    Inner Join {DbNames.EPYSL}..SampleBookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                    ), YBK AS (
	                    Select *
	                    FROM {TableNames.YarnBookingMaster_New}
                        where ISNULL(IsCancel,0) = 0
                    ),
                    BM As (
	                    Select * from {DbNames.EPYSL}..BookingMaster Where SubGroupID In (1,11,12)
                    ),
                    SBM As (
	                    Select * from {DbNames.EPYSL}..SampleBookingMaster
                    ),YB AS (
	                    Select Y.*, BM.BookingNo, ISNULL(IMG.ImagePath,'') ImagePath
	                    From (Select * from YBK Where WithoutOB = 0) Y
	                    Inner Join BM On BM.BookingID = Y.BookingID 
                        Left Join IMG ON IMG.BookingID = BM.BookingID
                    ),SYB AS (
	                    Select Y.*, SBM.BookingNo, SBKRevisionNo = SBM.RevisionNo, ISNULL(IMG.ImagePath,'') ImagePath
	                    From (Select * from YBK Where WithoutOB = 1) Y
	                    Inner Join SBM On SBM.BookingID = Y.BookingID 
                        Left Join SIMG IMG ON IMG.BookingID = SBM.BookingID
                    ), PYB AS (
	                    Select YB.YBookingNo, FYA.BOMMasterID, MinAck = Min(Convert(int,Case When YB.Acknowledge = 1 Then 1 Else 0 End)), MaxAck = Max(Convert(int,Case When YB.Acknowledge = 1 Then 1 Else 0 End))
	                    ,HasRevision = Convert(bit,Max(Convert(int,Case When YB.PreProcessRevNo >= FYA.RevisionNo Then 0 Else 1 End)))
	                    From YB
	                    Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FYA On FYA.BookingID = YB.BookingID And FYA.WithoutOB = YB.WithoutOB
	                    Group By YB.YBookingNo, FYA.BOMMasterID
	                    Having Min(Convert(int,Case When YB.Propose = 1 Then 1 Else 0 End)) = 1 And Min(Convert(int,Case When FYA.Status = 1 Then 1 Else 0 End)) = 1
                    ), PSYB AS (
	                    Select YB.YBookingNo, 0 BOMMasterID, MinAck = Min(Convert(int,Case When YB.Acknowledge = 1 Then 1 Else 0 End)), MaxAck = Max(Convert(int,Case When YB.Acknowledge = 1 Then 1 Else 0 End))
	                    ,HasRevision = Convert(bit,Max(Convert(int,Case When YB.PreProcessRevNo >= FYA.RevisionNo Then 0 Else 1 End))), BookingID = Min(YB.BookingID), SubGroupID = Min(YB.SubGroupID)
	                    From SYB YB
                        Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FYA On FYA.BookingID = YB.BookingID And FYA.WithoutOB = YB.WithoutOB
	                    Group By YB.YBookingNo
	                    Having Min(Convert(int,Case When YB.Propose = 1 Then 1 Else 0 End)) = 1
                    ),BC AS (
	                    SELECT BKM.BookingNo, BKC.BookingID, BKM.SupplierID, BKC.ConsumptionID, BKC.ItemMasterID, BKC.A1ValueID, BKC.YarnBrandID,ISG.SubGroupName
	                    FROM {DbNames.EPYSL}..BookingChild BKC
	                    Inner Join BM BKM On BKM.BookingID = BKC.BookingID
	                    Inner Join {DbNames.EPYSL}..BOMConsumption BOMCon On BOMCon.ConsumptionID = BKC.ConsumptionID and BKC.BOMmasterid = BOMCon.BOMmasterid
	                    Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = BOMCon.SubGroupID
	                    Where ISG.SubGroupName In ('Fabric','Collar','Cuff') and BKC.ISourcing = 1
	                    Group By BKM.BookingNo, BKC.BookingID, BKM.SupplierID, BKC.ConsumptionID, BKC.ItemMasterID, BKC.A1ValueID, BKC.YarnBrandID,ISG.SubGroupName
                    ),SBC AS (
	                    SELECT BKM.BookingNo, BKC.BookingID, BKM.SupplierID, BKC.ConsumptionID, BKC.ItemMasterID, SBCon.A1ValueID, SBCon.YarnBrandID,ISG.SubGroupName
	                    FROM {DbNames.EPYSL}..SampleBookingConsumptionChild BKC
	                    Inner Join SBM BKM On BKM.BookingID = BKC.BookingID
	                    Inner Join {DbNames.EPYSL}..SampleBookingConsumption SBCon On SBCon.ConsumptionID = BKC.ConsumptionID and BKC.BookingID = SBCon.BookingID
	                    Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SBCon.SubGroupID
	                    Where ISG.SubGroupName In ('Fabric','Collar','Cuff') 
	                    Group By BKM.BookingNo, BKC.BookingID, BKM.SupplierID, BKC.ConsumptionID, BKC.ItemMasterID, SBCon.A1ValueID, SBCon.YarnBrandID,ISG.SubGroupName
                    ),YBC AS (	
	                    SELECT YB.BookingID, YBC.ConsumptionID, YBC.ItemMasterID, YBC.YarnTypeID, YBC.YarnBrandID 
	                    FROM {TableNames.YarnBookingChild_New} YBC
	                    Inner Join YB On YB.YBookingID = YBC.YBookingID 
	                    Group By YB.BookingID, YBC.ConsumptionID, YBC.ItemMasterID, YBC.YarnTypeID, YBC.YarnBrandID
                    ),SYBC AS (	
	                    SELECT YB.BookingID, YBC.ConsumptionID, YBC.ItemMasterID, YBC.YarnTypeID, YBC.YarnBrandID 
	                    FROM {TableNames.YarnBookingChild_New} YBC
	                    Inner Join SYB YB On YB.YBookingID = YBC.YBookingID 
	                    Group By YB.BookingID, YBC.ConsumptionID, YBC.ItemMasterID, YBC.YarnTypeID, YBC.YarnBrandID
                    ), YBP AS(
	                    Select BC.BookingNo, SupplierID = Max(Convert(int,BC.SupplierID)), FBookingID = Max(Convert(int,BC.BookingID)), 
	                    IsPendingYB =  Convert(bit,Min(Convert(int,Case When ISNULL(YBC.ItemMasterID,0) = 0 then 1 Else 0 End)))
	                    From BC 
	                    Inner Join YBC On YBC.BookingID = BC.BookingID And YBC.ConsumptionID = BC.ConsumptionID And YBC.YarnBrandID = BC.YarnBrandID And YBC.YarnTypeID = BC.A1ValueID And YBC.ItemMasterID = BC.ItemMasterID
	                    group by BC.BookingNo
                    ), SYBP AS(
	                    Select BC.BookingNo, SupplierID = Max(Convert(int,BC.SupplierID)), FBookingID = Max(Convert(int,BC.BookingID)), 
	                    IsPendingYB =  Convert(bit,Min(Convert(int,Case When ISNULL(YBC.ItemMasterID,0) = 0 then 1 Else 0 End)))
	                    From SBC BC 
	                    Inner Join SYBC YBC On YBC.BookingID = BC.BookingID And YBC.ConsumptionID = BC.ConsumptionID And YBC.YarnBrandID = BC.YarnBrandID And YBC.YarnTypeID = BC.A1ValueID And YBC.ItemMasterID = BC.ItemMasterID
	                    group by BC.BookingNo
                    ), YBA As(
	                    Select YB.IsYarnStock,YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.BookingID, YB.YInHouseDate, YB.YRequiredDate, 
	                    YB.ContactPerson, YB.Propose, YB.ProposeDate, YB.Acknowledge, YB.AcknowledgeDate, YB.Remarks, YB.AddedBy, YB.DateAdded, YB.UpdatedBy, YB.DateUpdated, YB.DateRevised,
	                    YB.HasYPrice, YB.YPRevisionNo, YB.ApproveFP, YB.RevisionReason, YB.ApproveYP, YB.UnApproveYP, YB.YPPreProcessRevNo, YB.FPRevisionNo, YB.FPPreProcessRevNo, YB.PORevisionNeed,AcknowledgeStatus=Case When YB.Acknowledge = 0 Then 'Pending' Else 'Acknowledged' End,
	                    PYB.BOMMasterID, 'Fabric' GroupName, BookingNo = YBP.BookingNo,BookingDate = BM.BookingDate, EOM.ExportOrderNo, EOM.ExportOrderID, EOM.StyleMasterID, EOM.FabricEDD,
	                    RequiredDate = EOM.FabricEDD, ReferenceNo = ISNULL(BM.ReferenceNo,''), EOM.CalendarDays, PYB.HasRevision, YBP.IsPendingYB, YB.AdditionalBooking, YB.YBRevisionNeed,
	                    YB.FPRejectReason,YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB, YB.SubGroupID, YBP.SupplierID,
                        YB.BuyerID, YB.BuyerTeamID, YB.CompanyID, YB.AllowForNextStep, YB.TNADays, YB.FabricInHouseDate, YB.ImagePath											
                        From YB
	                    Inner Join YBP On YBP.BookingNo = YB.BookingNo
	                    Inner Join PYB On PYB.YBookingNo = YB.YBookingNo
	                    Inner Join (Select * from {DbNames.EPYSL}..ExportOrderMaster where EWOStatusID = 130) EOM On EOM.ExportOrderID = YB.ExportOrderID
	                    Inner Join BM On BM.BookingID = YB.BookingID
	                    Where ((PYB.MinAck = 0 or PYB.MaxAck = 0) And YB.RevisionNo > 0)
	                    Union All
	                    Select YB.IsYarnStock,YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.BookingID, YB.YInHouseDate, YB.YRequiredDate, 
	                    YB.ContactPerson, YB.Propose, YB.ProposeDate, YB.Acknowledge, YB.AcknowledgeDate, YB.Remarks, YB.AddedBy, YB.DateAdded, YB.UpdatedBy, YB.DateUpdated, YB.DateRevised,
	                    YB.HasYPrice, YB.YPRevisionNo, YB.ApproveFP, YB.RevisionReason, YB.ApproveYP, YB.UnApproveYP, YB.YPPreProcessRevNo, YB.FPRevisionNo, YB.FPPreProcessRevNo, YB.PORevisionNeed,AcknowledgeStatus=Case When YB.Acknowledge = 0 Then 'Pending' Else 'Acknowledged' End,
	                    PYB.BOMMasterID, 'Fabric' GroupName, BookingNo = YBP.BookingNo,BookingDate = BM.BookingDate, EOM.ExportOrderNo, EOM.ExportOrderID, EOM.StyleMasterID, EOM.FabricEDD,
	                    RequiredDate = EOM.FabricEDD, ReferenceNo = Case When ISNULL(BM.ReferenceNo,'') = '' and YB.WithoutOB = 1 then 'Sample' Else ISNULL(BM.ReferenceNo,'') End, EOM.CalendarDays, PYB.HasRevision, YBP.IsPendingYB, YB.AdditionalBooking, YB.YBRevisionNeed,
	                    YB.FPRejectReason,YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB, YB.SubGroupID, YBP.SupplierID,
                        YB.BuyerID, YB.BuyerTeamID, YB.CompanyID, YB.AllowForNextStep, YB.TNADays, YB.FabricInHouseDate, YB.ImagePath
	                    From SYB YB
	                    Inner Join SYBP YBP On YBP.BookingNo = YB.BookingNo
	                    Inner Join PSYB PYB On PYB.YBookingNo = YB.YBookingNo And PYB.SubGroupID = YB.SubGroupID
	                    Inner Join SBM BM On BM.BookingID = YB.BookingID
	                    Inner Join (Select * from {DbNames.EPYSL}..ExportOrderMaster where EWOStatusID = 130) EOM On EOM.ExportOrderNo = BM.SLNo
	                    Where ((PYB.MinAck = 0 or PYB.MaxAck = 0) And YB.RevisionNo > 0)
                    ),
                    FinalQ AS 
                    (Select YB.YBookingID, YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, YB.BookingID, YB.YInHouseDate, YB.YRequiredDate, 
                    YB.ContactPerson, YB.Propose, YB.ProposeDate, YB.Acknowledge, YB.AcknowledgeDate, YB.Remarks, YB.AddedBy, YB.DateAdded, YB.UpdatedBy, YB.DateUpdated, YB.DateRevised,
                    YB.HasYPrice, YB.YPRevisionNo, YB.ApproveYP, YB.UnApproveYP, YB.ApproveFP, YB.RevisionReason, YB.YPPreProcessRevNo, YB.FPRevisionNo, YB.FPPreProcessRevNo, YB.PORevisionNeed,AcknowledgeStatus=Case When YB.Acknowledge = 0 Then 'Pending' Else 'Acknowledged' End,
                    YB.BOMMasterID,SM.StyleMasterID,'Fabric' GroupName, 
                    BookingNo = YB.BookingNo,BookingDate = YB.BookingDate, YB.ExportOrderNo, CCT.TeamName BuyerDepartment, MerchandiserName = EMP.EmployeeName, FY.YearName,ContactPersonName = E.EmployeeName,Depertment =D.Designation +' : '+ ED.DepertmentDescription, 
                    RequiredDate = YB.FabricEDD, ReferenceNo = ISNULL(YB.ReferenceNo,''), YB.CalendarDays, YB.HasRevision, YB.IsPendingYB, YB.AdditionalBooking, YB.YBRevisionNeed,
                    RevStatus = Case When WithoutOB = 1 then 1 Else Case When (Isnull(EL.YBStatus,'') = 'Pending for Acknowledge' or Isnull(EL.YBStatus,'') = 'Acknowledged' or Isnull(EL.YBStatus,'') = 'Acknowledged with Additional Booking') And Isnull(EL.YBStatus,'') <> '' Then 1 Else 0 End End, 
                    YB.FPRejectReason,YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB, YB.SubGroupID,
                    YB.BuyerID, c.ShortName As BuyerName, YB.BuyerTeamID, YB.CompanyID, YB.AllowForNextStep, YB.TNADays, YB.FabricInHouseDate,YB.IsYarnStock, YB.ImagePath
                    From YBA YB
                    Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = YB.ExportOrderID And EL.BookingID = YB.BookingID And YB.SupplierID = EL.ContactID
                    left Join {DbNames.EPYSL}..StyleMaster SM On SM.StyleMasterID = YB.StyleMasterID
                    left Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = YB.AddedBy
                    left Join {DbNames.EPYSL}..Employee EMP ON EMP.EmployeeCode = LU.EmployeeCode
                    left Join {DbNames.EPYSL}..FinancialYear FY On FY.FinancialYearID = SM.FinancialYearID
                    left Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = SM.BuyerTeamID
                    left join {DbNames.EPYSL}..Contacts C On C.ContactID = YB.BuyerID
                    Left Join {DbNames.EPYSL}..Employee E On E.EmployeeCode = YB.ContactPerson
                    Left Join {DbNames.EPYSL}..EmployeeDepartment ED On ED.DepertmentID = E.DepertmentID
                    Left Join {DbNames.EPYSL}..EmployeeDesignation D On D.DesigID = E.DesigID)
                
                    SELECT *, Count(*) Over() TotalRows FROM FinalQ ";

                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YBookingID Desc" : paginationInfo.OrderBy;
                }
                else if (status == Status.Acknowledge)
                {
                    sql =
                      $@"
                   With BIMG As(
	                    Select BookingID, Min(ChildImgID) ChildImgID 
	                    From {DbNames.EPYSL}..BookingChildImage Group By BookingID
                    ), IMG As(
	                    Select I.BookingID, I.ImagePath
	                    From BIMG
	                    Inner Join {DbNames.EPYSL}..BookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                    ), SBIMG As(
	                    Select BookingID, Min(ChildImgID) ChildImgID 
	                    From {DbNames.EPYSL}..SampleBookingChildImage Group By BookingID
                    ), SIMG As(
	                    Select I.BookingID, I.ImagePath
	                    From SBIMG BIMG
	                    Inner Join {DbNames.EPYSL}..SampleBookingChildImage I On I.ChildImgID = BIMG.ChildImgID
                    ), YBK AS (
			            Select MIN(BookingID) BookingID, WithoutOB, YBookingNo, Acknowledge, PreProcessRevNo, Propose,
			            IsYarnStock, RevisionNo, YBookingDate, YInHouseDate, YRequiredDate, ContactPerson,
			            ProposeDate = CONVERT(date,ProposeDate), HasYPrice, YPRevisionNo, ApproveFP, RevisionReason, AcknowledgeDate = CONVERT(date,AcknowledgeDate), Remarks, AddedBy,
			            DateAdded = CONVERT(date,DateAdded), UpdatedBy,  DateUpdated = CONVERT(date,DateUpdated), DateRevised, ApproveYP, UnApproveYP, YPPreProcessRevNo, FPRevisionNo,
			            FPPreProcessRevNo, PORevisionNeed, FPRejectReason, HoldYP, HoldYPBy, HoldYPDate, HoldYPReason, IsCancel,
			            CancelReasonID, CanceledBy, AdditionalBooking, YBRevisionNeed, DateCanceled, Exported, BuyerID, BuyerTeamID,
			            CompanyID, AllowForNextStep, TNADays, FabricInHouseDate, ExportOrderID, ExportNo, DateExported
			            FROM {TableNames.YarnBookingMaster_New} YBM
			            where ISNULL(IsCancel,0) = 0 AND Acknowledge = 1
			            GROUP BY WithoutOB, YBookingNo, Acknowledge, PreProcessRevNo, Propose,
			            IsYarnStock, RevisionNo, YBookingDate, YInHouseDate, YRequiredDate, ContactPerson,
			            CONVERT(date,ProposeDate), HasYPrice, YPRevisionNo, ApproveFP, RevisionReason, CONVERT(date,AcknowledgeDate), Remarks, AddedBy,
			            CONVERT(date,DateAdded), UpdatedBy, CONVERT(date,DateUpdated), DateRevised, ApproveYP, UnApproveYP, YPPreProcessRevNo, FPRevisionNo,
			            FPPreProcessRevNo, PORevisionNeed, FPRejectReason, HoldYP, HoldYPBy, HoldYPDate, HoldYPReason, IsCancel,
			            CancelReasonID, CanceledBy, AdditionalBooking, YBRevisionNeed, DateCanceled, Exported, BuyerID, BuyerTeamID,
			            CompanyID, AllowForNextStep, TNADays, FabricInHouseDate, ExportOrderID, ExportNo, DateExported
                    ),
                    BM As (
	                    Select * from {DbNames.EPYSL}..BookingMaster Where SubGroupID In (1,11,12)
                    ),
                    SBM As (
	                    Select * from {DbNames.EPYSL}..SampleBookingMaster
                    ),YB AS (
	                    Select Y.*, BM.BookingNo, ISNULL(IMG.ImagePath,'') ImagePath
	                    From (Select * from YBK Where WithoutOB = 0) Y
	                    Inner Join BM On BM.BookingID = Y.BookingID 
                        Left Join IMG ON IMG.BookingID = BM.BookingID
                    )
		            ,SYB AS (
	                    Select Y.*, SBM.BookingNo, SBKRevisionNo = SBM.RevisionNo, ISNULL(IMG.ImagePath,'') ImagePath
	                    From (Select * from YBK Where WithoutOB = 1) Y
	                    Inner Join SBM On SBM.BookingID = Y.BookingID 
                        Left Join SIMG IMG ON IMG.BookingID = SBM.BookingID
                    )
		            , PYB AS (
	                    Select YB.YBookingNo, FYA.BOMMasterID, MinAck = Min(Convert(int,Case When YB.Acknowledge = 1 Then 1 Else 0 End)), MaxAck = Max(Convert(int,Case When YB.Acknowledge = 1 Then 1 Else 0 End))
	                    ,HasRevision = Convert(bit,Max(Convert(int,Case When YB.PreProcessRevNo >= FYA.RevisionNo Then 0 Else 1 End)))
	                    From YB
	                    Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FYA On FYA.BookingID = YB.BookingID And FYA.WithoutOB = YB.WithoutOB
	                    Group By YB.YBookingNo, FYA.BOMMasterID
	                    Having Min(Convert(int,Case When YB.Propose = 1 Then 1 Else 0 End)) = 1 --And Min(Convert(int,Case When FYA.Status = 1 Then 1 Else 0 End)) = 1
                    ), PSYB AS (
	                    Select YB.YBookingNo, 0 BOMMasterID, MinAck = Min(Convert(int,Case When YB.Acknowledge = 1 Then 1 Else 0 End)), MaxAck = Max(Convert(int,Case When YB.Acknowledge = 1 Then 1 Else 0 End))
	                    ,HasRevision = Convert(bit,Max(Convert(int,Case When YB.PreProcessRevNo >= FYA.RevisionNo Then 0 Else 1 End))), BookingID = Min(YB.BookingID)
	                    From SYB YB
                        Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FYA On FYA.BookingID = YB.BookingID And FYA.WithoutOB = YB.WithoutOB
	                    Group By YB.YBookingNo
	                    Having Min(Convert(int,Case When YB.Propose = 1 Then 1 Else 0 End)) = 1
                    ),BC AS (
	                    SELECT BKM.BookingNo, BKC.BookingID, BKM.SupplierID, BKC.ConsumptionID, BKC.ItemMasterID, BKC.A1ValueID, BKC.YarnBrandID,ISG.SubGroupName
	                    FROM {DbNames.EPYSL}..BookingChild BKC
	                    Inner Join BM BKM On BKM.BookingID = BKC.BookingID
	                    Inner Join {DbNames.EPYSL}..BOMConsumption BOMCon On BOMCon.ConsumptionID = BKC.ConsumptionID and BKC.BOMmasterid = BOMCon.BOMmasterid
	                    left Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = BOMCon.SubGroupID
	                    Where ISG.SubGroupName In ('Fabric','Collar','Cuff') and BKC.ISourcing = 1
	                    Group By BKM.BookingNo, BKC.BookingID, BKM.SupplierID, BKC.ConsumptionID, BKC.ItemMasterID, BKC.A1ValueID, BKC.YarnBrandID,ISG.SubGroupName
                    ),SBC AS (
	                    SELECT BKM.BookingNo, BKC.BookingID, BKM.SupplierID, BKC.ConsumptionID, BKC.ItemMasterID, SBCon.A1ValueID, SBCon.YarnBrandID,ISG.SubGroupName
	                    FROM {DbNames.EPYSL}..SampleBookingConsumptionChild BKC
	                    Inner Join SBM BKM On BKM.BookingID = BKC.BookingID
	                    Inner Join {DbNames.EPYSL}..SampleBookingConsumption SBCon On SBCon.ConsumptionID = BKC.ConsumptionID and BKC.BookingID = SBCon.BookingID
	                    left Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SBCon.SubGroupID
	                    Where ISG.SubGroupName In ('Fabric','Collar','Cuff') 
	                    Group By BKM.BookingNo, BKC.BookingID, BKM.SupplierID, BKC.ConsumptionID, BKC.ItemMasterID, SBCon.A1ValueID, SBCon.YarnBrandID,ISG.SubGroupName
                    ),YBC AS (	
	                    SELECT YB.BookingID, YBC.ConsumptionID, YBC.ItemMasterID, YBC.YarnTypeID, YBC.YarnBrandID 
	                    FROM {TableNames.YarnBookingChild_New} YBC
			            LEFT JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
	                    Inner Join YB On YB.BookingID = YBM.BookingID 
	                    Group By YB.BookingID, YBC.ConsumptionID, YBC.ItemMasterID, YBC.YarnTypeID, YBC.YarnBrandID
                    ),SYBC AS (	
	                    SELECT YB.BookingID, YBC.ConsumptionID, YBC.ItemMasterID, YBC.YarnTypeID, YBC.YarnBrandID 
	                    FROM {TableNames.YarnBookingChild_New} YBC
			            LEFT JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
	                    Inner Join SYB YB On YB.BookingID = YBM.BookingID 
	                    Group By YB.BookingID, YBC.ConsumptionID, YBC.ItemMasterID, YBC.YarnTypeID, YBC.YarnBrandID
                    ), YBP AS(
	                    Select BC.BookingNo, SupplierID = Max(Convert(int,BC.SupplierID)), FBookingID = Max(Convert(int,BC.BookingID)), 
	                    IsPendingYB =  Convert(bit,Min(Convert(int,Case When ISNULL(YBC.ItemMasterID,0) = 0 then 1 Else 0 End)))
	                    From BC 
	                    Inner Join YBC On YBC.BookingID = BC.BookingID And YBC.ConsumptionID = BC.ConsumptionID And YBC.YarnBrandID = BC.YarnBrandID And YBC.YarnTypeID = BC.A1ValueID And YBC.ItemMasterID = BC.ItemMasterID
	                    group by BC.BookingNo
                    ), SYBP AS(
	                    Select BC.BookingNo, SupplierID = Max(Convert(int,BC.SupplierID)), FBookingID = Max(Convert(int,BC.BookingID)), 
	                    IsPendingYB =  Convert(bit,Min(Convert(int,Case When ISNULL(YBC.ItemMasterID,0) = 0 then 1 Else 0 End)))
	                    From SBC BC 
	                    Inner Join SYBC YBC On YBC.BookingID = BC.BookingID And YBC.ConsumptionID = BC.ConsumptionID And YBC.YarnBrandID = BC.YarnBrandID And YBC.YarnTypeID = BC.A1ValueID And YBC.ItemMasterID = BC.ItemMasterID
	                    group by BC.BookingNo
                    ), YBA As(
	                    Select YB.IsYarnStock,YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.BookingID, YB.YInHouseDate, YB.YRequiredDate, 
	                    YB.ContactPerson, YB.Propose, YB.ProposeDate, YB.Acknowledge, YB.AcknowledgeDate, YB.Remarks, YB.AddedBy, YB.DateAdded, YB.UpdatedBy, YB.DateUpdated, YB.DateRevised,
	                    YB.HasYPrice, YB.YPRevisionNo, YB.ApproveFP, YB.RevisionReason, YB.ApproveYP, YB.UnApproveYP, YB.YPPreProcessRevNo, YB.FPRevisionNo, YB.FPPreProcessRevNo, YB.PORevisionNeed,AcknowledgeStatus=Case When YB.Acknowledge = 0 Then 'Pending' Else 'Acknowledged' End,
	                    PYB.BOMMasterID, 'Fabric' GroupName, BookingNo = YBP.BookingNo,BookingDate = BM.BookingDate, EOM.ExportOrderNo, EOM.ExportOrderID, EOM.StyleMasterID, EOM.FabricEDD,
	                    RequiredDate = EOM.FabricEDD, ReferenceNo = ISNULL(BM.ReferenceNo,''), EOM.CalendarDays, PYB.HasRevision, YBP.IsPendingYB, YB.AdditionalBooking, YB.YBRevisionNeed,
	                    YB.FPRejectReason,YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB, YBP.SupplierID,
                        YB.BuyerID, YB.BuyerTeamID, YB.CompanyID, YB.AllowForNextStep, YB.TNADays, YB.FabricInHouseDate, YB.ImagePath											
                        From YB
	                    Inner Join YBP On YBP.BookingNo = YB.BookingNo
	                    Inner Join PYB On PYB.YBookingNo = YB.YBookingNo
	                    Inner Join (Select * from {DbNames.EPYSL}..ExportOrderMaster where EWOStatusID = 130) EOM On EOM.ExportOrderID = YB.ExportOrderID
	                    Inner Join BM On BM.BookingID = YB.BookingID
											
	                    Union All
	                    Select YB.IsYarnStock,YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.BookingID, YB.YInHouseDate, YB.YRequiredDate, 
	                    YB.ContactPerson, YB.Propose, YB.ProposeDate, YB.Acknowledge, YB.AcknowledgeDate, YB.Remarks, YB.AddedBy, YB.DateAdded, YB.UpdatedBy, YB.DateUpdated, YB.DateRevised,
	                    YB.HasYPrice, YB.YPRevisionNo, YB.ApproveFP, YB.RevisionReason, YB.ApproveYP, YB.UnApproveYP, YB.YPPreProcessRevNo, YB.FPRevisionNo, YB.FPPreProcessRevNo, YB.PORevisionNeed,AcknowledgeStatus=Case When YB.Acknowledge = 0 Then 'Pending' Else 'Acknowledged' End,
	                    PYB.BOMMasterID, 'Fabric' GroupName, BookingNo = YBP.BookingNo,BookingDate = BM.BookingDate, EOM.ExportOrderNo, EOM.ExportOrderID, EOM.StyleMasterID, EOM.FabricEDD,
	                    RequiredDate = EOM.FabricEDD, ReferenceNo = Case When ISNULL(BM.ReferenceNo,'') = '' and YB.WithoutOB = 1 then 'Sample' Else ISNULL(BM.ReferenceNo,'') End, EOM.CalendarDays, PYB.HasRevision, YBP.IsPendingYB, YB.AdditionalBooking, YB.YBRevisionNeed,
	                    YB.FPRejectReason,YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB, YBP.SupplierID,
                        YB.BuyerID, YB.BuyerTeamID, YB.CompanyID, YB.AllowForNextStep, YB.TNADays, YB.FabricInHouseDate, YB.ImagePath
	                    From SYB YB
	                    Inner Join SYBP YBP On YBP.BookingNo = YB.BookingNo
	                    Inner Join PSYB PYB On PYB.YBookingNo = YB.YBookingNo
	                    Inner Join SBM BM On BM.BookingID = YB.BookingID
	                    Inner Join (Select * from {DbNames.EPYSL}..ExportOrderMaster where EWOStatusID = 130) EOM On EOM.ExportOrderNo = BM.SLNo
                    ),
                    FinalQ AS 
                    (Select YB.YBookingNo, YB.PreProcessRevNo, YB.RevisionNo, YB.YBookingDate, YB.ExportOrderID, YB.BookingID, YB.YInHouseDate, YB.YRequiredDate, 
                    YB.ContactPerson, YB.Propose, YB.ProposeDate, YB.Acknowledge, YB.AcknowledgeDate, YB.Remarks, YB.AddedBy, YB.DateAdded, YB.UpdatedBy, YB.DateUpdated, YB.DateRevised,
                    YB.HasYPrice, YB.YPRevisionNo, YB.ApproveYP, YB.UnApproveYP, YB.ApproveFP, YB.RevisionReason, YB.YPPreProcessRevNo, YB.FPRevisionNo, YB.FPPreProcessRevNo, YB.PORevisionNeed,AcknowledgeStatus=Case When YB.Acknowledge = 0 Then 'Pending' Else 'Acknowledged' End,
                    YB.BOMMasterID,SM.StyleMasterID,'Fabric' GroupName, 
                    BookingNo = YB.BookingNo,BookingDate = YB.BookingDate, YB.ExportOrderNo, CCT.TeamName BuyerDepartment, MerchandiserName = EMP.EmployeeName, FY.YearName,ContactPersonName = E.EmployeeName,Depertment =D.Designation +' : '+ ED.DepertmentDescription, 
                    RequiredDate = YB.FabricEDD, ReferenceNo = ISNULL(YB.ReferenceNo,''), YB.CalendarDays, YB.HasRevision, YB.IsPendingYB, YB.AdditionalBooking, YB.YBRevisionNeed,
                    RevStatus = Case When WithoutOB = 1 then 1 Else Case When (Isnull(EL.YBStatus,'') = 'Pending for Acknowledge' or Isnull(EL.YBStatus,'') = 'Acknowledged' or Isnull(EL.YBStatus,'') = 'Acknowledged with Additional Booking') And Isnull(EL.YBStatus,'') <> '' Then 1 Else 0 End End, 
                    YB.FPRejectReason,YB.HoldYP,YB.HoldYPBy,YB.HoldYPDate,YB.HoldYPReason, YB.IsCancel, YB.CancelReasonID, YB.CanceledBy, YB.DateCanceled, YB.Exported, YB.DateExported, YB.ExportNo, YB.WithoutOB,
                    YB.BuyerID, c.ShortName As BuyerName, YB.BuyerTeamID, YB.CompanyID, YB.AllowForNextStep, YB.TNADays, YB.FabricInHouseDate,YB.IsYarnStock, YB.ImagePath
                    From YBA YB
                    Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = YB.ExportOrderID And EL.BookingID = YB.BookingID And YB.SupplierID = EL.ContactID
                    left Join {DbNames.EPYSL}..StyleMaster SM On SM.StyleMasterID = YB.StyleMasterID
                    left Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = YB.AddedBy
                    left Join {DbNames.EPYSL}..Employee EMP ON EMP.EmployeeCode = LU.EmployeeCode
                    left Join {DbNames.EPYSL}..FinancialYear FY On FY.FinancialYearID = SM.FinancialYearID
                    left Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = SM.BuyerTeamID
                    left join {DbNames.EPYSL}..Contacts C On C.ContactID = YB.BuyerID
                    Left Join {DbNames.EPYSL}..Employee E On E.EmployeeCode = YB.ContactPerson
                    Left Join {DbNames.EPYSL}..EmployeeDepartment ED On ED.DepertmentID = E.DepertmentID
                    Left Join {DbNames.EPYSL}..EmployeeDesignation D On D.DesigID = E.DesigID)

                    Select *, Count(*) Over() TotalRows FROM FinalQ ";

                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YBookingNo Desc" : paginationInfo.OrderBy;
                }
            }

            sql += $@"
            {paginationInfo.FilterBy}
            {orderBy}
            {paginationInfo.PageBy}";

            return await _service.GetDataAsync<YarnBookingMaster>(sql);
        }
        public async Task<YarnBookingMaster> GetListWithoutGroupBy(bool isSample, string bookingNo, Status status)
        {
            string sql;
            //No need master table query because master data get in the Data Grid on the front side. 
            sql = $@" 
                     With BM As
                    (
                        Select top 1 * From {DbNames.EPYSL}..BookingMaster Where BookingNo = '{bookingNo}'
                    )  
                    Select FBA.BookingID, FBA.BOMMasterID, SM.StyleMasterID,IG.GroupName,EOM.ExportOrderID,BM.BookingNo,
                    EOM.ExportOrderNo, BM.BuyerID, BM.BuyerTeamID, BM.CompanyID, BM.ExportOrderID, BM.SubGroupID, 
                    CCT.TeamName BuyerDepartment,MerchandiserName = EMP.EmployeeName,FY.YearName,
                    RequiredDate = BM.InHouseDate, ReferenceNo = ISNULL(BM.ReferenceNo,''), EOM.CalendarDays As TNADays,
                    RevStatus = Case When Isnull(EL.FabBTexAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBTexAckStatus,'') <> '' Then 1 Else 0 End,
                    FBA.PreRevisionNo
                    From 
                    (
                    Select BOMMasterID, BookingID = Min(FBK.BookingID), 	
                    ItemGroupID = Case When FBK.ItemGroupID in (1,11,12) then 1 else FBK.ItemGroupID End, 
                    SubGroupID = Case When FBK.SubGroupID in (1,11,12) then 1 else FBK.SubGroupID End,
                    FBK.PreRevisionNo PreProcessRevNo
                    FROM {TableNames.FBBOOKING_ACKNOWLEDGE} FBK Inner Join BM On BM.BookingID = FBK.BookingID
                    Where --Status = 1 and 
                    FBK.WithoutOB = 0  
                    Group By BOMMasterID, BM.BookingNo, 	
                    Case When FBK.ItemGroupID in (1,11,12) then 1 else FBK.ItemGroupID End, 
                    Case When FBK.SubGroupID in (1,11,12) then 1 else FBK.SubGroupID End,
                    FBK.PreRevisionNo
                    ) FBA
                    Inner Join BM On BM.BookingID = FBA.BookingID
                    Inner Join {DbNames.EPYSL}..ExportOrderMaster EOM On EOM.ExportOrderID = BM.ExportOrderID                                       
                    Inner Join {DbNames.EPYSL}..StyleMaster SM On SM.StyleMasterID = EOM.StyleMasterID
                    Inner Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = SM.BuyerTeamID
                    Inner Join {DbNames.EPYSL}..ItemGroup IG On IG.ItemGroupID = FBA.ItemGroupID
                    Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = EOM.ExportOrderID 
                    And EL.ContactID = BM.SupplierID And BM.BookingID = EL.BookingID  --And EL.ItemGroupID = ISG.ItemGroupID										
                    Inner Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = BM.AddedBy
                    Inner Join {DbNames.EPYSL}..Employee EMP ON EMP.EmployeeCode = LU.EmployeeCode
                    Inner Join {DbNames.EPYSL}..FinancialYear FY On FY.FinancialYearID = SM.FinancialYearID
                    Where EOM.EWOStatusID = 130 --And BM.SubGroupID = 12
                    And FBA.BookingID not in (Select BookingID FROM {TableNames.YarnBookingMaster_New} Where WithoutOB = 0)  
                    Group By FBA.BookingID, FBA.BOMMasterID, SM.StyleMasterID,IG.GroupName,EOM.ExportOrderID,BM.BookingNo,
                    EOM.ExportOrderNo, BM.BuyerID, BM.BuyerTeamID, BM.CompanyID, BM.ExportOrderID, BM.SubGroupID, 
                    CCT.TeamName, EMP.EmployeeName, FY.YearName, BM.InHouseDate, BM.ReferenceNo, EOM.CalendarDays,
                    EL.FabBTexAckStatus, FBA.PreRevisionNo; 

                    -- Booking Child Information
                    Select
                    YBChildID=ROW_NUMBER() OVER (ORDER BY BC.ItemMasterID),
                    BC.ConsumptionID,BC.BookingQty,
                    BC.BOMMasterID, BC.ExportOrderID, 
                    BOMCon.ItemGroupID, BOMCon.SubGroupID, BC.ItemMasterID, ISG.SubGroupName, Isnull(BC.ExecutionCompanyID,0) ExecutionCompanyID, 
                    ColorID = Case When ISG.SubGroupName in('Collar','Cuff') Then IM.Segment5ValueID Else IM.Segment3ValueID End,
                    BookingUnitID =  Case When ISG.SubGroupName='Collar' Or ISG.SubGroupName='Cuff' Then 28 Else BC.BookingUnitID End,
                    BookingUOM = Case When ISG.SubGroupName='Collar' Or ISG.SubGroupName='Cuff' Then 'KG' Else U.DisplayUnitDesc End,
                    BOMCon.Remarks, BOMCon.Segment1Desc Segment1ValueDesc, BOMCon.Segment2Desc Segment2ValueDesc, 
                    BOMCon.Segment3Desc Segment3ValueDesc, BOMCon.Segment4Desc Segment4ValueDesc, BOMCon.Segment5Desc Segment5ValueDesc,
                    BOMCon.Segment6Desc Segment6ValueDesc, BOMCon.Segment7Desc Segment7ValueDesc, BOMCon.Segment8Desc Segment8ValueDesc, 
                    BOMCon.Segment9Desc Segment9ValueDesc, BOMCon.Segment10Desc Segment10ValueDesc, BOMCon.Segment11Desc Segment11ValueDesc, 
                    BOMCon.Segment12Desc Segment12ValueDesc, 
                    BOMCon.Segment13Desc Segment13ValueDesc, BOMCon.Segment14Desc Segment14ValueDesc, BOMCon.Segment15Desc Segment15ValueDesc,
                    BOMCon.LengthYds, BOMCon.LengthInch, BOMCon.FUPartID, FUP.PartName, 
                    BOMCon.A1ValueID As YarnTypeID, ISV1.SegmentValue YarnType, 
                    BOMCon.YarnBrandID, ETV.ValueName YarnBrand, ForTechPack=Convert(Varchar(50),''), 
                    ISourcing = Case When BC.ISourcing IS NULL then Convert(bit,0) Else BC.ISourcing End, 
                    ISourcingName = Case When BC.ISourcing IS NULL then 'Out Side' 
                    When BC.ISourcing IS NOT NULL And BC.ISourcing = 1 then 'In-House' 
                    When BC.ISourcing IS NOT NULL And BC.ISourcing = 0 then 'Out Side' End,
                    ContactName = IsNull(Con.Name,''),ContactID = IsNull(BOMCon.ContactID,0),
                    LabDipNo = IsNull(BOMCon.LabDipNo,''),BC.LabdipUpdateDate, BC.IsCompleteReceive, BC.IsCompleteDelivery, 
                    BC.LastDCDate, BC.ToItemMasterID, BC.ClosingRemarks, BM.BuyerID, BM.BuyerTeamID, BM.CompanyID, BM.ExportOrderID, 
                    BM.BookingID, IM.Segment1ValueID ConstructionId,IM.Segment2ValueID CompositionId,IM.Segment4ValueID GSMId,IM.Segment7ValueID KnittingTypeId
                    From  {DbNames.EPYSL}..BookingChild BC 
                    Inner Join {DbNames.EPYSL}..BookingMaster BM On BC.BookingID = BM.BookingID
                    Inner Join {DbNames.EPYSL}..BOMConsumption BOMCon On BOMCon.ConsumptionID = BC.ConsumptionID
                    Left Join {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = BC.ColorID
                    Left Join {DbNames.EPYSL}..Sizes S On S.SizeID = BC.SizeID
                    Left Join {DbNames.EPYSL}..TechPackMaster TPM On TPM.TechPackID = BC.TechPackID
                    Left Join {DbNames.EPYSL}..StyleMaster SM On SM.StyleMasterID = TPM.StyleMasterID
                    Inner Join {DbNames.EPYSL}..Unit U On U.UnitID = BC.BookingUnitID
                    Inner Join {DbNames.EPYSL}..ItemGroup IG On IG.ItemGroupID = BC.ItemGroupID
                    Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = BC.SubGroupID
                    Left Join {DbNames.EPYSL}..Contacts Con On Con.ContactID = BOMCon.ContactID
                    Left Join {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = BOMCon.A1ValueID
                    Left Join {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = BOMCon.YarnBrandID
                    Left Join {DbNames.EPYSL}..FabricUsedPart FUP On FUP.FUPartID = BOMCon.FUPartID
                    LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BC.ItemMasterID
                    Where BM.BookingNo = '{bookingNo}' And BC.ISourcing = 1;

                    -- YarnBookingChildGarmentPart
                    Select BCYSB.*,BC.ConsumptionID, BC.ItemMasterID, FUP.PartName, '0' IsSaveFlag 
                    From {DbNames.EPYSL}..BookingChildGarmentPart BCYSB
                    Inner Join {DbNames.EPYSL}..BookingChild BC On BC.BookingChildID = BCYSB.BookingChildID
                    Inner Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingID = BC.BookingID
                    Inner Join {DbNames.EPYSL}..FabricUsedPart FUP On FUP.FUPartID = BCYSB.FUPartID
                    Where BM.BookingNo = '{bookingNo}' ;

                    -- YarnBookingChildYarnSubBrand
                     Select BCYSB.*,BM.BookingID, BC.ConsumptionID, BC.ItemMasterID, ETV.ValueName YarnSubBrandName, '0' IsSaveFlag 
                     From {DbNames.EPYSL}..BookingChildYarnSubBrand BCYSB
                     Inner Join {DbNames.EPYSL}..BookingChild BC On BC.BookingChildID = BCYSB.BookingChildID
                     Inner Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingID = BC.BookingID
                     Inner Join {DbNames.EPYSL}..EntityTypeValue ETV On ETV.ValueID = BCYSB.YarnSubBrandID
                     Where BM.BookingNo = '{bookingNo}'";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnBookingMaster data = await records.ReadFirstOrDefaultAsync<YarnBookingMaster>();
                Guard.Against.NullObject(data);

                data.Childs = records.Read<YarnBookingChild>().ToList();
                data.yarnBookingChildGarmentPart = records.Read<YarnBookingChildGarmentPart>().ToList();
                data.yarnBookingChildYarnSubBrand = records.Read<YarnBookingChildYarnSubBrand>().ToList();

                foreach (var item in data.Childs)
                {
                    if (item.SubGroupName == "Fabric") data.HasFabric = true;
                    else if (item.SubGroupName == "Collar") data.HasCollar = true;
                    else if (item.SubGroupName == "Cuff") data.HasCuff = true;

                    item.FUPartIDs = data.yarnBookingChildGarmentPart.Where(x => x.BookingID == item.BookingID &&
                        x.ItemMasterID == item.ItemMasterID &&
                        x.ConsumptionID == item.ConsumptionID).Select(x => x.FUPartID).Distinct().ToArray();

                    item.FUPart = string.Join(",", data.yarnBookingChildGarmentPart.Where(
                        x => x.BookingID == item.BookingID &&
                        x.ItemMasterID == item.ItemMasterID &&
                        x.ConsumptionID == item.ConsumptionID).Select(x => x.FUPartID).Distinct());

                    item.PartName = string.Join(",", data.yarnBookingChildGarmentPart.Where(
                        x => x.BookingID == item.BookingID &&
                        x.ItemMasterID == item.ItemMasterID &&
                        x.ConsumptionID == item.ConsumptionID).Select(x => x.PartName).Distinct());

                    item.YarnSubBrandIDs = data.yarnBookingChildYarnSubBrand.Where(x => x.BookingID == item.BookingID &&
                       x.ItemMasterID == item.ItemMasterID &&
                       x.ConsumptionID == item.ConsumptionID).Select(x => x.YarnSubBrandID).Distinct().ToArray();

                    item.YarnSubBrandID = string.Join(",", data.yarnBookingChildYarnSubBrand.Where(
                        x => x.BookingID == item.BookingID &&
                        x.ItemMasterID == item.ItemMasterID &&
                        x.ConsumptionID == item.ConsumptionID).Select(x => x.YarnSubBrandID).Distinct());

                    item.YarnSubBrandName = string.Join(",", data.yarnBookingChildYarnSubBrand.Where(
                        x => x.BookingID == item.BookingID &&
                        x.ItemMasterID == item.ItemMasterID &&
                        x.ConsumptionID == item.ConsumptionID).Select(x => x.YarnSubBrandName).Distinct());
                }
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open) _connection.Close();
            }
        }
        public async Task<YarnBookingMaster> GetPendingYarnList(bool isSample, string bookingNo, Status status)
        {
            string sql;

            var segmentNames = new
            {
                SegmentNames = new string[]
                {
                    ItemSegmentNameConstants.YARN_SUBPROGRAM_NEW,
                    ItemSegmentNameConstants.YARN_CERTIFICATIONS
                }
            };

            if (isSample)
            {
                //No need master table query because master data get in the Data Grid on the front side. 
                sql = $@" 
                    With SBM As
                    (
	                    Select top 1 * From {DbNames.EPYSL}..SampleBookingMaster Where BookingNo = '{bookingNo}'
                    ) 
                    Select BookingID = SBM.BookingID, 0 BOMMasterID, EM.StyleMasterID, 'Fabric' GroupName, 
                    EM.ExportOrderID, SBM.BookingNo, EM.ExportOrderNo, SBM.BuyerID, SBM.BuyerTeamID, 0 As CompanyID, 
                    SBM.ExportOrderID, SBM.SubGroupID, CCT.TeamName BuyerDepartment, EMP.EmployeeName MerchandiserName, 
                    YearName, RequiredDate = SBM.FirstInHouseDate, 
                    ReferenceNo = Case When ISNULL(SBM.ReferenceNo,'') = '' then 'Sample' Else ISNULL(SBM.ReferenceNo,'') End, 
                    EM.CalendarDays  As TNADays, 1 RevStatus,
                    FBK.PreProcessRevNo, YBM.YBookingID
                    from SBM
                    Inner JOIN {TableNames.FabricBookingAcknowledge} FBK On FBK.BookingID = SBM.BookingID
                    Inner Join {DbNames.EPYSL}..ExportOrderMaster EM On EM.ExportOrderID = SBM.ExportOrderID
                    Inner Join {DbNames.EPYSL}..FinancialYear FY On SBM.BookingDate between FY.StartMonth and FY.EndMonth
                    Inner Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = SBM.BuyerTeamID
                    Inner Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = SBM.AddedBy
                    Inner Join {DbNames.EPYSL}..Employee EMP ON EMP.EmployeeCode = LU.EmployeeCode
                    LEFT JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.BookingID = FBK.BookingID
                    Where EM.IsAutoGenerateNo = 1 And EM.EWOStatusID = 130
                    AND YBM.YBookingID IS NULL
                    Group By SBM.BookingID, EM.StyleMasterID, EM.ExportOrderID, SBM.BookingNo, EM.ExportOrderNo, 
                    SBM.BuyerID, SBM.BuyerTeamID, SBM.ExportOrderID, SBM.SubGroupID, CCT.TeamName, EMP.EmployeeName, 
                    YearName,SBM.FirstInHouseDate, SBM.ReferenceNo, EM.CalendarDays, FBK.PreProcessRevNo, YBM.YBookingID; 

                    -- Booking Child Information
                    ;Select SBC.BookingID, SBCon.ItemGroupID, SBCon.SubGroupID, SBC.ItemMasterID, 
                    ISG.SubGroupName, ConsumptionQty = Ceiling(Sum(SBC.ConsumptionQty)),  SBC.ConsumptionID,
                    ColorID = Case When ISG.SubGroupName in('Collar','Cuff') Then IM.Segment5ValueID Else IM.Segment3ValueID End,
                    BookingQty = 
                    Case When ISG.SubGroupName in('Collar','Cuff') 
                    Then (0.045 * ((Convert(decimal(18,3), SBCon.Segment3Desc) * 
                    Convert(decimal(18,3), SBCon.Segment4Desc))/420)) * 
                    (Sum(SBC.RequiredQty) * U.RelativeFactor) 
                    Else Sum(SBC.RequiredQty) End, /*Calculation--42X10 -> 420 -> 0.045 kg*/
                    BookingUnitID = (Case When ISG.SubGroupName='Collar' Or ISG.SubGroupName='Cuff' Then 28 
                    Else SBC.RequiredUnitID End),
                    BookingUOM = Case When ISG.SubGroupName='Collar' Or ISG.SubGroupName='Cuff' Then 'KG' 
                    Else U.DisplayUnitDesc End,  
                    RequisitionQty = Ceiling(Sum(SBC.RequiredQty)), SBCon.Remarks, SBCon.Segment1Desc Segment1ValueDesc,
                    SBCon.Segment2Desc Segment2ValueDesc,SBCon.Segment3Desc Segment3ValueDesc,SBCon.Segment4Desc Segment4ValueDesc,
                    SBCon.Segment5Desc Segment5ValueDesc,SBCon.Segment6Desc Segment6ValueDesc,SBCon.Segment7Desc Segment7ValueDesc,
                    SBCon.Segment8Desc Segment8ValueDesc, SBCon.Segment9Desc Segment9ValueDesc,SBCon.Segment10Desc Segment10ValueDesc,
                    SBCon.Segment11Desc Segment11ValueDesc,SBCon.Segment12Desc Segment12ValueDesc,SBCon.Segment13Desc Segment13ValueDesc,
                    SBCon.Segment14Desc Segment14ValueDesc,SBCon.Segment15Desc Segment15ValueDesc,
                    SBCon.LengthYds,SBCon.LengthInch,SBCon.FUPartID, SBCon.A1ValueID As YarnTypeID,ISV1.SegmentValue YarnType, 
                    SBCon.YarnBrandID, ETV.ValueName YarnBrand, FUP.PartName, ForTechPack=Convert(Varchar(50),''), 
                    ISourcing = Convert(bit,1), ISourcingName = 'In-House', ContactName = '',ContactID = 0,
                    LabDipNo = IsNull(SBCon.LabDipNo,''), Convert(decimal,0) BlockBookingQty, Convert(decimal,0) AdjustQty, 
                    Convert(bit,0) AutoAgree,Price = convert(decimal,0), SuggestedPrice = convert(decimal,0), 
                    Convert(bit,'0') IsCompleteReceive, Convert(bit,'0') IsCompleteDelivery, M.BuyerID, 
                    M.BuyerTeamID, M.ExportOrderID

                    From  {DbNames.EPYSL}..SampleBookingConsumptionChild SBC 
                    Inner Join {DbNames.EPYSL}..SampleBookingConsumption SBCon On SBCon.ConsumptionID = SBC.ConsumptionID
                    Inner Join {DbNames.EPYSL}..SampleBookingMaster M On M.BookingID = SBCon.BookingID
                    Inner Join {DbNames.EPYSL}..Unit U On U.UnitID = SBC.RequiredUnitID
                    LEFT Join {DbNames.EPYSL}..ItemGroup IG On IG.ItemGroupID = SBC.ItemGroupID
                    Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SBC.SubGroupID
                    Left Join {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = SBCon.A1ValueID
                    Left Join {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = SBCon.YarnBrandID
                    Left Join {DbNames.EPYSL}..FabricUsedPart FUP On FUP.FUPartID = SBCon.FUPartID 
                    Left Join {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID=SBC.ItemMasterID
                    Where M.BookingNo = '{bookingNo}'                               
                    Group By SBC.BookingID, SBCon.ItemGroupID, SBCon.SubGroupID, SBC.ItemMasterID, ISG.SubGroupName,
                    SBCon.Segment1Desc,SBCon.Segment2Desc,SBCon.Segment3Desc,SBCon.Segment4Desc,SBCon.Segment5Desc,SBCon.Segment6Desc,
                    SBCon.Segment7Desc,SBCon.Segment8Desc, SBCon.Segment9Desc, SBCon.Segment10Desc, SBCon.Segment11Desc, SBCon.Segment12Desc, 
                    SBCon.Segment13Desc, SBCon.Segment14Desc, SBCon.Segment15Desc,
                    SBC.RequiredUnitID,U.DisplayUnitDesc,SBCon.LengthYds,SBCon.LengthInch,SBCon.FUPartID,SBCon.A1ValueID,SBCon.YarnBrandID,
                    IsNull(SBCon.LabDipNo,''),SBCon.Remarks,ISV1.SegmentValue,ETV.ValueName,FUP.PartName, M.BuyerID, 
                    M.BuyerTeamID, M.ExportOrderID, U.RelativeFactor, SBC.ConsumptionID,
                    Case When ISG.SubGroupName in('Collar','Cuff') Then IM.Segment5ValueID Else IM.Segment3ValueID End; 

                    -- Employee Information
                    ;Select CAST(E.EmployeeCode As varchar) id,E.EmployeeName [text], (D.Designation + ' , ' + ED.DepertmentDescription) [desc]
                    From {DbNames.EPYSL}..Employee E
                    Inner Join  {DbNames.EPYSL}..EmployeeDepartment ED On ED.DepertmentID = E.DepertmentID
                    Inner Join {DbNames.EPYSL}..EmployeeDesignation D On D.DesigID = E.DesigID
                    Where E.IsRegular = 1 and ED.DepertmentDescription = 'Supply Chain';  

                    -- Shade book
                    {CommonQueries.GetYarnShadeBooks()};

                    -- YarnBookingChildGarmentPart
                    With YBK As
                    (
                     Select YBCYSB.YBookingCGPID, YBCYSB.YBChildID, YBCYSB.FUPartID,
                     YBC.ConsumptionID, YBM.BookingID, YBC.ItemMasterID, FUP.PartName
                     FROM {TableNames.YarnBookingChildGarmentPart_New} YBCYSB
                     Inner JOIN {TableNames.YarnBookingChild_New} YBC On YBC.YBChildID = YBCYSB.YBChildID
                     Inner Join (Select * FROM {TableNames.YarnBookingMaster_New} Where WithoutOB = 0) YBM On YBM.YBookingID = YBC.YBookingID
                     Inner Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingID = YBM.BookingID
                     Inner Join {DbNames.EPYSL}..FabricUsedPart FUP On FUP.FUPartID = YBCYSB.FUPartID
                     Where BM.BookingNo = '{bookingNo}'
                    ),
                    BK As (
                     Select BCYSB.*,BC.ConsumptionID, BC.ItemMasterID, FUP.PartName 
                     From {DbNames.EPYSL}..BookingChildGarmentPart BCYSB
                     Inner Join {DbNames.EPYSL}..BookingChild BC On BC.BookingChildID = BCYSB.BookingChildID
                     Inner Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingID = BC.BookingID
                     Inner Join {DbNames.EPYSL}..FabricUsedPart FUP On FUP.FUPartID = BCYSB.FUPartID
                     Where BM.BookingNo = '{bookingNo}'
                    )
                    Select YBookingCGPID = ISNULL(YBK.YBookingCGPID,0), YBChildID = ISNULL(YBK.YBChildID,0), BK.BookingID, BK.ConsumptionID, BK.ItemMasterID, 
                    FUPartID = ISNULL(YBK.FUPartID,BK.FUPartID), PartName = ISNULL(YBK.PartName,BK.PartName),
                    IM.Segment1ValueID ConstructionID,IM.Segment2ValueID CompositionID,IM.Segment4ValueID GSMID,IM.Segment7ValueID KnittingTypeID
                    From BK
                    LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BK.ItemMasterID
                    Left Join YBK On YBK.ConsumptionID = BK.ConsumptionID And YBK.BookingID = BK.BookingID And YBK.ItemMasterID = BK.ItemMasterID;

                    -- YarnBookingChildYarnSubBrand
                    ;With YBK As
                    (
                     Select YBCYSB.*,YBC.ConsumptionID, YBM.BookingID, YBC.ItemMasterID, ETV.ValueName YarnSubBrandName
                     FROM {TableNames.YarnBookingChildYarnSubBrand_New} YBCYSB
                     Inner JOIN {TableNames.YarnBookingChild_New} YBC On YBC.YBChildID = YBCYSB.YBChildID
                     Inner Join (Select * FROM {TableNames.YarnBookingMaster_New} Where WithoutOB = 0) YBM On YBM.YBookingID = YBC.YBookingID
                     Inner Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingID = YBM.BookingID
                     Inner Join {DbNames.EPYSL}..EntityTypeValue ETV On ETV.ValueID = YBCYSB.YarnSubBrandID
                     Where BM.BookingNo = '{bookingNo}' 
                    ),
                    BK As (
                     Select BCYSB.*,BM.BookingID, BC.ConsumptionID, BC.ItemMasterID, ETV.ValueName YarnSubBrandName 
                     From {DbNames.EPYSL}..BookingChildYarnSubBrand BCYSB
                     Inner Join {DbNames.EPYSL}..BookingChild BC On BC.BookingChildID = BCYSB.BookingChildID
                     Inner Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingID = BC.BookingID
                     Inner Join {DbNames.EPYSL}..EntityTypeValue ETV On ETV.ValueID = BCYSB.YarnSubBrandID
                     Where BM.BookingNo = '{bookingNo}'
                    )
                    Select YBookingCYSubBrandID = ISNULL(YBK.YBookingCYSubBrandID,0), YBChildID = ISNULL(YBK.YBChildID,0), BK.BookingID, BK.ConsumptionID, BK.ItemMasterID, 
                    YarnSubBrandID = ISNULL(YBK.YarnSubBrandID,BK.YarnSubBrandID), YarnSubBrandName = ISNULL(YBK.YarnSubBrandName,BK.YarnSubBrandName),
                    IM.Segment1ValueID ConstructionID,IM.Segment2ValueID CompositionID,IM.Segment4ValueID GSMID,IM.Segment7ValueID KnittingTypeID
                    From BK
                    LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BK.ItemMasterID
                    Left Join YBK On YBK.ConsumptionID = BK.ConsumptionID And YBK.BookingID = BK.BookingID And YBK.ItemMasterID = BK.ItemMasterID;

                    -- YarnBookingChildItemYarnSubBrand
                    Select ETV.ValueID id, ETV.ValueName [text]
                    From {DbNames.EPYSL}..EntityTypeValue ETV
                    Inner Join {DbNames.EPYSL}..EntityType ET On ET.EntityTypeID = ETV.EntityTypeID
                    Where ET.EntityTypeName = 'Yarn Sub Brand'
                    Order By ETV.ValueName;

                    ---- Fabric Components
                    {CommonQueries.GetEntityTypeValuesOnly(EntityTypeNameConstants.FABRIC_TYPE)};

                    -- Item Segments
                    {CommonQueries.GetItemSegmentValuesBySegmentNamesWithSegmentName()};";
            }
            else
            {
                //No need master table query because master data get in the Data Grid on the front side. 
                sql = $@" 
                    With BM As
                    (
	                    Select top 1 * From {DbNames.EPYSL}..BookingMaster Where BookingNo = '{bookingNo}'
                    )  
                    Select FBA.BookingID, FBA.BOMMasterID, SM.StyleMasterID,IG.GroupName,EOM.ExportOrderID,BM.BookingNo,
                    EOM.ExportOrderNo, BM.BuyerID, BM.BuyerTeamID, BM.CompanyID, BM.ExportOrderID, BM.SubGroupID, 
                    CCT.TeamName BuyerDepartment,MerchandiserName = EMP.EmployeeName,FY.YearName,
                    RequiredDate = BM.InHouseDate, ReferenceNo = ISNULL(BM.ReferenceNo,''), EOM.CalendarDays As TNADays,
                    RevStatus = Case When Isnull(EL.FabBTexAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBTexAckStatus,'') <> '' Then 1 Else 0 End,
                    FBA.PreProcessRevNo
                    From 
                    (
	                    Select BOMMasterID, BookingID = Min(FBK.BookingID), 	
	                    ItemGroupID = Case When FBK.ItemGroupID in (1,11,12) then 1 else FBK.ItemGroupID End, 
	                    SubGroupID = Case When FBK.SubGroupID in (1,11,12) then 1 else FBK.SubGroupID End,
	                    FBK.PreProcessRevNo
	                    FROM {TableNames.FabricBookingAcknowledge} FBK 
	                    Inner Join BM On BM.BookingID = FBK.BookingID
	                    Group By BOMMasterID, BM.BookingNo, 	
	                    Case When FBK.ItemGroupID in (1,11,12) then 1 else FBK.ItemGroupID End, 
	                    Case When FBK.SubGroupID in (1,11,12) then 1 else FBK.SubGroupID End,
	                    FBK.PreProcessRevNo
                    ) FBA
                    LEFT Join BM On BM.BookingID = FBA.BookingID
                    LEFT Join {DbNames.EPYSL}..ExportOrderMaster EOM On EOM.ExportOrderID = BM.ExportOrderID                                       
                    LEFT Join {DbNames.EPYSL}..StyleMaster SM On SM.StyleMasterID = EOM.StyleMasterID
                    LEFT Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = SM.BuyerTeamID
                    LEFT Join {DbNames.EPYSL}..ItemGroup IG On IG.ItemGroupID = FBA.ItemGroupID
                    Left Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = EOM.ExportOrderID 
                    And EL.ContactID = BM.SupplierID And BM.BookingID = EL.BookingID  --And EL.ItemGroupID = ISG.ItemGroupID										
                    LEFT Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = BM.AddedBy
                    LEFT Join {DbNames.EPYSL}..Employee EMP ON EMP.EmployeeCode = LU.EmployeeCode
                    LEFT Join {DbNames.EPYSL}..FinancialYear FY On FY.FinancialYearID = SM.FinancialYearID
                    LEFT JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.BookingID = FBA.BookingID
                    Where EOM.EWOStatusID = 130 AND YBM.YBookingID IS NULL
                    Group By FBA.BookingID, FBA.BOMMasterID, SM.StyleMasterID,IG.GroupName,EOM.ExportOrderID,BM.BookingNo,
                    EOM.ExportOrderNo, BM.BuyerID, BM.BuyerTeamID, BM.CompanyID, BM.ExportOrderID, BM.SubGroupID, 
                    CCT.TeamName, EMP.EmployeeName, FY.YearName, BM.InHouseDate, BM.ReferenceNo, EOM.CalendarDays,
                    EL.FabBTexAckStatus, FBA.PreProcessRevNo; 

                    -- Booking Child Information
					Select BookingChildID As YBChildID, BC.BOMMasterID, BC.ExportOrderID, 
                    BC.ConsumptionID,
                    BOMCon.ItemGroupID, BOMCon.SubGroupID, BC.ItemMasterID, ISG.SubGroupName, Isnull(BC.ExecutionCompanyID,0) ExecutionCompanyID, 
	                ColorID = Case When ISG.SubGroupName in('Collar','Cuff') Then IM.Segment5ValueID Else IM.Segment3ValueID End,
                    ConsumptionQty = Ceiling(Sum(BC.ConsumptionQty)),  
                    BookingQty = 
                      Case When ISG.SubGroupName in('Collar','Cuff') 
                       Then (0.045 * ((Convert(decimal(18,3), BOMCon.Segment3Desc) * 
                       Convert(decimal(18,3), BOMCon.Segment4Desc))/420)) * 
                       (Sum(BC.BookingQty) * U.RelativeFactor) 
                      Else Sum(BC.BookingQty) End,
                    BookingUnitID =  Case When ISG.SubGroupName='Collar' Or ISG.SubGroupName='Cuff' Then 28 Else BC.BookingUnitID End,
                    BookingUOM = Case When ISG.SubGroupName='Collar' Or ISG.SubGroupName='Cuff' Then 'KG' Else U.DisplayUnitDesc End, 
                    RequisitionQty = Ceiling(Sum(BC.RequisitionQty)),  
                    BOMCon.Remarks, BOMCon.Segment1Desc Segment1ValueDesc, BOMCon.Segment2Desc Segment2ValueDesc, 
                    BOMCon.Segment3Desc Segment3ValueDesc, BOMCon.Segment4Desc Segment4ValueDesc, BOMCon.Segment5Desc Segment5ValueDesc,
                    BOMCon.Segment6Desc Segment6ValueDesc, BOMCon.Segment7Desc Segment7ValueDesc, BOMCon.Segment8Desc Segment8ValueDesc, 
                    BOMCon.Segment9Desc Segment9ValueDesc, BOMCon.Segment10Desc Segment10ValueDesc, BOMCon.Segment11Desc Segment11ValueDesc, 
                    BOMCon.Segment12Desc Segment12ValueDesc, 
                    BOMCon.Segment13Desc Segment13ValueDesc, BOMCon.Segment14Desc Segment14ValueDesc, BOMCon.Segment15Desc Segment15ValueDesc,
                    BOMCon.LengthYds, BOMCon.LengthInch, BOMCon.FUPartID, BOMCon.A1ValueID As YarnTypeID, ISV1.SegmentValue YarnType, 
                    BOMCon.YarnBrandID, ETV.ValueName YarnBrand, FUP.PartName, ForTechPack=Convert(Varchar(50),''), 
                    ISourcing = Case When BC.ISourcing IS NULL then Convert(bit,0) Else BC.ISourcing End, 
                    ISourcingName = Case When BC.ISourcing IS NULL then 'Out Side' 
                    When BC.ISourcing IS NOT NULL And BC.ISourcing = 1 then 'In-House' 
                    When BC.ISourcing IS NOT NULL And BC.ISourcing = 0 then 'Out Side' End,
                    ContactName = IsNull(Con.Name,''),ContactID = IsNull(BOMCon.ContactID,0) ,LabDipNo = IsNull(BOMCon.LabDipNo,''), 
                    BlockBookingQty = SUM(ISNULL(BC.BlockBookingQty,0)), AdjustQty = SUM(ISNULL(BC.AdjustQty,0)), 
                    AutoAgree = Convert(bit,min(convert(int,(ISNULL(BC.AutoAgree,0))))),Price = SUM(ISNULL(BC.Price,0)), 
                    SuggestedPrice = SUM(ISNULL(BC.SuggestedPrice,0)), BC.LabdipUpdateDate, BC.IsCompleteReceive, BC.IsCompleteDelivery, 
                    BC.LastDCDate, BC.ToItemMasterID, BC.ClosingRemarks, BM.BuyerID, BM.BuyerTeamID, BM.CompanyID, BM.ExportOrderID, 
                    BM.BookingID, IM.Segment1ValueID ConstructionId,IM.Segment2ValueID CompositionId,IM.Segment4ValueID GSMId,IM.Segment7ValueID KnittingTypeId
                    From  {DbNames.EPYSL}..BookingChild BC 
                    Inner Join {DbNames.EPYSL}..BookingMaster BM On BC.BookingID = BM.BookingID
                    Inner Join {DbNames.EPYSL}..BOMConsumption BOMCon On BOMCon.ConsumptionID = BC.ConsumptionID
                    Left Join {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = BC.ColorID
                    Left Join {DbNames.EPYSL}..Sizes S On S.SizeID = BC.SizeID
                    Left Join {DbNames.EPYSL}..TechPackMaster TPM On TPM.TechPackID = BC.TechPackID
                    Left Join {DbNames.EPYSL}..StyleMaster SM On SM.StyleMasterID = TPM.StyleMasterID
                    Inner Join {DbNames.EPYSL}..Unit U On U.UnitID = BC.BookingUnitID
                    Inner Join {DbNames.EPYSL}..ItemGroup IG On IG.ItemGroupID = BC.ItemGroupID
                    Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = BC.SubGroupID
                    Left Join {DbNames.EPYSL}..Contacts Con On Con.ContactID = BOMCon.ContactID
                    Left Join {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = BOMCon.A1ValueID
                    Left Join {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = BOMCon.YarnBrandID
                    Left Join {DbNames.EPYSL}..FabricUsedPart FUP On FUP.FUPartID = BOMCon.FUPartID
                    LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BC.ItemMasterID

                    Where BM.BookingNo = '{bookingNo}' And BC.ISourcing = 1 --And BOMCon.SubGroupID = 12                                   
                    Group By BookingChildID, BC.BOMMasterID, BC.ExportOrderID, BOMCon.ItemGroupID, BOMCon.SubGroupID, BC.ItemMasterID, ISG.SubGroupName, Isnull(BC.ExecutionCompanyID,0),
                    BC.ConsumptionID,                    
                    BOMCon.Segment1Desc,BOMCon.Segment2Desc,BOMCon.Segment3Desc,BOMCon.Segment4Desc,BOMCon.Segment5Desc,BOMCon.Segment6Desc,
                    BOMCon.Segment7Desc,BOMCon.Segment8Desc, BOMCon.Segment9Desc, BOMCon.Segment10Desc, BOMCon.Segment11Desc, BOMCon.Segment12Desc, 
                    BOMCon.Segment13Desc, BOMCon.Segment14Desc, BOMCon.Segment15Desc,
                    BC.BookingUnitID,U.DisplayUnitDesc,BOMCon.LengthYds,BOMCon.LengthInch,BOMCon.FUPartID,BOMCon.A1ValueID,BOMCon.YarnBrandID,TPM.IsSet,
                    BC.ISourcing,Con.Name,BOMCon.ContactID,IsNull(BOMCon.LabDipNo,''),BOMCon.Remarks,ISV1.SegmentValue,ETV.ValueName,FUP.PartName, 
                    BC.LabdipUpdateDate, BC.IsCompleteReceive, BC.IsCompleteDelivery, BC.LastDCDate, BC.ToItemMasterID, BC.ClosingRemarks, 
                    BM.BuyerID, BM.BuyerTeamID, BM.CompanyID, BM.ExportOrderID, BM.BookingID, U.RelativeFactor,
                    IM.Segment1ValueID,IM.Segment2ValueID,IM.Segment4ValueID,IM.Segment7ValueID,
		            Case When ISG.SubGroupName in('Collar','Cuff') Then IM.Segment5ValueID Else IM.Segment3ValueID End
                    Order By Remarks, Segment1ValueDesc, Segment2ValueDesc,	Segment5ValueDesc, Segment6ValueDesc, 
                    YarnTypeID, YarnType, YarnBrandID, YarnBrand,	BookingID;

                    -- Employee Information
                    Select CAST(E.EmployeeCode As varchar) id,E.EmployeeName [text], (D.Designation + ' , ' + ED.DepertmentDescription) [desc]
                    From  {DbNames.EPYSL}..Employee E
                    Inner Join  {DbNames.EPYSL}..EmployeeDepartment ED On ED.DepertmentID = E.DepertmentID
                    Inner Join {DbNames.EPYSL}..EmployeeDesignation D On D.DesigID = E.DesigID
                    Where E.IsRegular = 1 and ED.DepertmentDescription = 'Supply Chain';

                    -- Shade book
                    {CommonQueries.GetYarnShadeBooks()};

                    -- YarnBookingChildGarmentPart
                    With YBK As
                    (
                     Select YBCYSB.*,YBC.ConsumptionID, YBM.BookingID, YBC.ItemMasterID, FUP.PartName, '1' IsSaveFlag
                     FROM {TableNames.YarnBookingChildGarmentPart_New} YBCYSB
                     Inner JOIN {TableNames.YarnBookingChild_New} YBC On YBC.YBChildID = YBCYSB.YBChildID
                     Inner Join (Select * FROM {TableNames.YarnBookingMaster_New} Where WithoutOB = 0) YBM On YBM.YBookingID = YBC.YBookingID
                     Inner Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingID = YBM.BookingID
                     Inner Join {DbNames.EPYSL}..FabricUsedPart FUP On FUP.FUPartID = YBCYSB.FUPartID
                     Where BM.BookingNo = '{bookingNo}'   
                    ),
                    BK As (
                     Select BCYSB.*,BC.ConsumptionID, BC.ItemMasterID, FUP.PartName, '0' IsSaveFlag 
                     From {DbNames.EPYSL}..BookingChildGarmentPart BCYSB
                     Inner Join {DbNames.EPYSL}..BookingChild BC On BC.BookingChildID = BCYSB.BookingChildID
                     Inner Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingID = BC.BookingID
                     Inner Join {DbNames.EPYSL}..FabricUsedPart FUP On FUP.FUPartID = BCYSB.FUPartID
                     Where BM.BookingNo = '{bookingNo}' 
                    )
                    Select COALESCE(YBK.IsSaveFlag, BK.IsSaveFlag) IsSaveFlag, YBookingCGPID = ISNULL(YBK.YBookingCGPID,0), YBChildID = ISNULL(YBK.YBChildID,0), BK.BookingID, BK.ConsumptionID, BK.ItemMasterID, 
                    FUPartID = ISNULL(YBK.FUPartID,BK.FUPartID), PartName = ISNULL(YBK.PartName,BK.PartName),
                    IM.Segment1ValueID ConstructionID,IM.Segment2ValueID CompositionID,IM.Segment4ValueID GSMID,IM.Segment7ValueID KnittingTypeID
                    From BK
                    LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BK.ItemMasterID
                    Left Join YBK On YBK.ConsumptionID = BK.ConsumptionID And YBK.BookingID = BK.BookingID And YBK.ItemMasterID = BK.ItemMasterID;

                    -- YarnBookingChildYarnSubBrand
                    With YBK As
                    (
                     Select YBCYSB.*,YBC.ConsumptionID, YBM.BookingID, YBC.ItemMasterID, ETV.ValueName YarnSubBrandName, '1' IsSaveFlag
                     FROM {TableNames.YarnBookingChildYarnSubBrand_New} YBCYSB
                     Inner JOIN {TableNames.YarnBookingChild_New} YBC On YBC.YBChildID = YBCYSB.YBChildID
                     Inner Join (Select * FROM {TableNames.YarnBookingMaster_New} Where WithoutOB = 0) YBM On YBM.YBookingID = YBC.YBookingID
                     Inner Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingID = YBM.BookingID
                     Inner Join {DbNames.EPYSL}..EntityTypeValue ETV On ETV.ValueID = YBCYSB.YarnSubBrandID
                     Where BM.BookingNo = '{bookingNo}'
                    ),
                    BK As (
                     Select BCYSB.*,BM.BookingID, BC.ConsumptionID, BC.ItemMasterID, ETV.ValueName YarnSubBrandName, '0' IsSaveFlag 
                     From {DbNames.EPYSL}..BookingChildYarnSubBrand BCYSB
                     Inner Join {DbNames.EPYSL}..BookingChild BC On BC.BookingChildID = BCYSB.BookingChildID
                     Inner Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingID = BC.BookingID
                     Inner Join {DbNames.EPYSL}..EntityTypeValue ETV On ETV.ValueID = BCYSB.YarnSubBrandID
                     Where BM.BookingNo =  '{bookingNo}'
                    )
                    Select COALESCE(YBK.IsSaveFlag, BK.IsSaveFlag) IsSaveFlag, YBookingCYSubBrandID = ISNULL(YBK.YBookingCYSubBrandID,0), YBChildID = ISNULL(YBK.YBChildID,0), BK.BookingID, BK.ConsumptionID, BK.ItemMasterID, 
                    YarnSubBrandID = ISNULL(YBK.YarnSubBrandID,BK.YarnSubBrandID), YarnSubBrandName = ISNULL(YBK.YarnSubBrandName,BK.YarnSubBrandName),
                    IM.Segment1ValueID ConstructionID,IM.Segment2ValueID CompositionID,IM.Segment4ValueID GSMID,IM.Segment7ValueID KnittingTypeID
                    From BK
                    LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BK.ItemMasterID
                    Left Join YBK On YBK.ConsumptionID = BK.ConsumptionID And YBK.BookingID = BK.BookingID And YBK.ItemMasterID = BK.ItemMasterID;

                    -- YarnBookingChildItemYarnSubBrand
                    Select ETV.ValueID id, ETV.ValueName [text]
                    From {DbNames.EPYSL}..EntityTypeValue ETV
                    Inner Join {DbNames.EPYSL}..EntityType ET On ET.EntityTypeID = ETV.EntityTypeID
                    Where ET.EntityTypeName = 'Yarn Sub Brand'
                    Order By ETV.ValueName;

                    ---- Fabric Components
                    {CommonQueries.GetEntityTypeValuesOnly(EntityTypeNameConstants.FABRIC_TYPE)};

                    -- Item Segments
                    {CommonQueries.GetItemSegmentValuesBySegmentNamesWithSegmentName()};";
            }

            try
            {
                await _connection.OpenAsync();
                //var records = await _connection.QueryMultipleAsync(sql);
                var records = await _connection.QueryMultipleAsync(sql, segmentNames);
                YarnBookingMaster data = await records.ReadFirstOrDefaultAsync<YarnBookingMaster>();
                Guard.Against.NullObject(data);
                data.F_isSample = isSample;
                data.Childs = records.Read<YarnBookingChild>().ToList();
                if (status != Status.Pending)
                {
                    List<YarnBookingChildItem> ChildItems = records.Read<YarnBookingChildItem>().ToList();
                    data.Childs.ForEach(childDetails =>
                    {
                        childDetails.ChildItems = ChildItems.Where(c => c.YBChildID == childDetails.YBChildID).ToList();
                    });
                }
                data.ContactPersonList = await records.ReadAsync<Select2OptionModel>();
                data.YarnShadeBooks = await records.ReadAsync<Select2OptionModel>();
                data.yarnBookingChildGarmentPart = records.Read<YarnBookingChildGarmentPart>().ToList();
                data.yarnBookingChildYarnSubBrand = records.Read<YarnBookingChildYarnSubBrand>().ToList();
                data.YarnSubBrandList = records.Read<Select2OptionModel>().ToList();

                data.FabricComponents = await records.ReadAsync<string>();
                var itemSegments = await records.ReadAsync<Select2OptionModelExtended>();
                data.YarnSubProgramNews = itemSegments.Where(x => x.desc == ItemSegmentNameConstants.YARN_SUBPROGRAM_NEW);
                data.Certifications = itemSegments.Where(x => x.desc == ItemSegmentNameConstants.YARN_CERTIFICATIONS);

                foreach (YarnBookingChild item in data.Childs)
                {
                    if (item.SubGroupName == "Fabric") data.HasFabric = true;
                    else if (item.SubGroupName == "Collar") data.HasCollar = true;
                    else if (item.SubGroupName == "Cuff") data.HasCuff = true;

                    item.yarnBookingChildGarmentPart = data.yarnBookingChildGarmentPart;
                    item.yarnBookingChildYarnSubBrand = data.yarnBookingChildYarnSubBrand;

                    item.FUPartIDs = data.yarnBookingChildGarmentPart.Where(x => x.BookingID == item.BookingID &&
                       x.ItemMasterID == item.ItemMasterID).Select(x => x.FUPartID).Distinct().ToArray();

                    item.FUPart = string.Join(",", data.yarnBookingChildGarmentPart.Where(
                        x => x.BookingID == item.BookingID &&
                        x.ItemMasterID == item.ItemMasterID).Select(x => x.FUPartID).Distinct());

                    item.PartName = string.Join(",", data.yarnBookingChildGarmentPart.Where(
                        x => x.BookingID == item.BookingID &&
                        x.ItemMasterID == item.ItemMasterID).Select(x => x.PartName).Distinct());

                    item.YarnSubBrandIDs = data.yarnBookingChildYarnSubBrand.Where(x => x.BookingID == item.BookingID &&
                      x.ItemMasterID == item.ItemMasterID).Select(x => x.YarnSubBrandID).Distinct().ToArray();

                    item.YarnSubBrandID = string.Join(",", data.yarnBookingChildYarnSubBrand.Where(
                        x => x.BookingID == item.BookingID &&
                        x.ItemMasterID == item.ItemMasterID).Select(x => x.YarnSubBrandID).Distinct());

                    item.YarnSubBrandName = string.Join(",", data.yarnBookingChildYarnSubBrand.Where(
                        x => x.BookingID == item.BookingID &&
                        x.ItemMasterID == item.ItemMasterID).Select(x => x.YarnSubBrandName).Distinct());
                }
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open) _connection.Close();
            }
        }
        public async Task<List<YarnBookingMaster>> GetByYBookingNo(string yBookingNo)
        {
            string sql = $@"WITH 
                        FBA AS
                        (
	                        SELECT TOP(1)FBA.BookingID, FBA.RevisionNo
	                        FROM {TableNames.FBBOOKING_ACKNOWLEDGE} FBA
                            INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.BookingID = FBA.BookingID
	                        WHERE YBookingNo = '{yBookingNo}'
	                        GROUP BY FBA.BookingID, FBA.RevisionNo
                        )
                        SELECT YBM.*, BookingRevisionNo = FBA.RevisionNo
                        FROM {TableNames.YarnBookingMaster_New} YBM
                        INNER JOIN FBA ON FBA.BookingID = YBM.BookingID
                        WHERE YBookingNo = '{yBookingNo}'";
            return await _service.GetDataAsync<YarnBookingMaster>(sql);
        }
        public async Task<List<YarnBookingMaster>> GetAllByYBookingNo(string yBookingNo)
        {
            string sql = $@"WITH 
                        FBA AS
                        (
	                        SELECT FBA.BookingID, FBA.RevisionNo
	                        FROM {TableNames.FBBOOKING_ACKNOWLEDGE} FBA
                            INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.BookingID = FBA.BookingID
	                        WHERE YBookingNo = '{yBookingNo}'
	                        GROUP BY FBA.BookingID, FBA.RevisionNo
                        )
                        SELECT YBM.*, BookingRevisionNo = FBA.RevisionNo
                        FROM {TableNames.YarnBookingMaster_New} YBM
                        INNER JOIN FBA ON FBA.BookingID = YBM.BookingID
                        WHERE YBookingNo = '{yBookingNo}'";
            return await _service.GetDataAsync<YarnBookingMaster>(sql);
        }
        public async Task<List<YarnBookingMaster>> GetByYBookingNos(List<string> yBookingNos)
        {
            if (yBookingNos.Count() == 0) return new List<YarnBookingMaster>();

            string yBookingNo = string.Join("','", yBookingNos.Select(x => x));

            string sql = $@" 
                        SELECT YBM.*
                        FROM {TableNames.YarnBookingMaster_New} YBM
                        WHERE YBM.YBookingNo IN ('{yBookingNo}')";
            return await _service.GetDataAsync<YarnBookingMaster>(sql);
        }
        public async Task<List<YarnBookingMaster>> GetByBookingNo(string bookingNo, bool isAddition)
        {
            string additionQuery = isAddition ? " AND YBM.IsAddition = 1 " : " AND YBM.IsAddition = 0 ";

            string sql = $@"SELECT YBM.* 
            FROM {TableNames.YarnBookingMaster_New} YBM
            LEFT JOIN {DbNames.EPYSL}..BookingMaster BM ON BM.BookingID = YBM.BookingID
            LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = YBM.BookingID
            WHERE (BM.BookingNo = '{bookingNo}' OR SBM.BookingNo = '{bookingNo}') {additionQuery}";
            return await _service.GetDataAsync<YarnBookingMaster>(sql);
        }
        public async Task<List<YarnBookingMaster>> GetYarnByNo(string yBookingNo)
        {
            string sql = $@"

            SELECT YBM.* 
            FROM {TableNames.YarnBookingMaster_New} YBM
            WHERE YBM.YBookingNo = '{yBookingNo}'

            SELECT YBCI.* ,
			Construction = ISV1.SegmentValue, 
            Composition = ISV2.SegmentValue,
            Color = CASE WHEN YBM.SubGroupID = 1 THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END
            FROM {TableNames.YarnBookingChild_New} YBCI
            INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBCI.YBookingID
            INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBAC ON FBAC.BookingChildID = YBCI.BookingChildID AND FBAC.ConsumptionID = YBCI.ConsumptionID
			INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = FBAC.ItemMasterID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
            WHERE YBM.YBookingNo = '{yBookingNo}'

            SELECT YBCI.* 
            FROM {TableNames.YarnBookingChildItem_New} YBCI
            INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBCI.YBookingID
            WHERE YBM.YBookingNo='{yBookingNo}';

            -- Finish Fabric Utilization//--
            ;with ISF As (
	            Select ISF.BookingID, RollID, ISF.ItemMasterID, RollNo, Shade, ISF.BatchNo, TagNo, ISF.GSM, LengthInInch, WidthInInch, CCID1, CCID4, RollQtyInKG = RollQtyInKG, 
	            RollQtyInKGPcs = RollQtyInKGPcs, AOPRoll, ReceiveStatus, ISF.WeightSheetNo, IsReject, IsExcess, IsQtyDecrease, IsOrderCancel, IsReturn, IsOrderCCBreakDown,
	            LocationID, RackID
	            FROM {TableNames.ItemFinishStockRoll} ISF
	            Inner Join  {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = ISF.ItemMasterID
	            Inner JOIN {TableNames.BulkBookingFinishFabricUtilization} BB On BB.ItemMasterID  = IM.ItemMasterID
	            INNER JOIN {TableNames.YarnBookingChild_New} C ON C.YBChildID = BB.YBChildID
	            INNER JOIN {TableNames.YarnBookingMaster_New} M ON M.YBookingID = C.YBookingID
	            INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = M.BookingID
	            WHERE M.YBookingNo='{yBookingNo}'
            )

            SELECT FFU.BBFFUtilizationID,	FFU.YBChildID,	FFU.ExportOrderID,	FFU.ItemMasterID,	FFU.GSM,	FFU.ColorID,	FFU.SubGroupID,	FFU.BuyerID, 
            FFU.GSMID,	FFU.CompositionID,	FFU.Width,	FFU.BatchNo,	FFU.WeightSheetNo, FFU.FinishFabricUtilizationQTYinkg,
			ExportOrderNo = ISNULL(EOM.ExportOrderNo,''), ColorName = ISNULL(ISVC.SegmentValue,''), Buyer = ISNULL(CT.ShortName,''),
            FabricConstruction = ISV2.SegmentValue + ' ' + ISV1.SegmentValue,
            TotalStockQtyinkg  = (Case When FFU.SubGroupID = 1 Then Sum(ISF.RollQtyInKG) Else 0 End) + 
					                (Case When FFU.SubGroupID = 1 Then 0 Else Sum(ISF.RollQtyInKGPcs) End)

            FROM {TableNames.BulkBookingFinishFabricUtilization} FFU
            INNER JOIN {TableNames.YarnBookingChild_New} C ON C.YBChildID = FFU.YBChildID
            INNER JOIN {TableNames.YarnBookingMaster_New} M ON M.YBookingID = C.YBookingID
            INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = M.BookingID
            Left Join {DbNames.EPYSL}..ExportOrderMaster EOM On EOM.ExportOrderID = FFU.ExportOrderID
            Left Join {DbNames.EPYSL}..ItemSegmentValue ISVC On ISVC.SegmentValueID = FFU.ColorID
            Left Join {DbNames.EPYSL}..Contacts CT On CT.ContactID = FFU.BuyerID And CT.ContactID >0
            Inner Join {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = FFU.ItemMasterID
            Inner Join ISF On ISF.ItemMasterID = FFU.ItemMasterID
            Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = IM.SubGroupID
            Left Join {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
            Left Join {DbNames.EPYSL}..ItemSegmentValue ISV2 On ISV2.SegmentValueID = IM.Segment2ValueID
            WHERE M.YBookingNo='{yBookingNo}'
            Group By FFU.BBFFUtilizationID,	FFU.YBChildID,	FFU.ExportOrderID,	FFU.ItemMasterID,	FFU.GSM,	FFU.ColorID,	FFU.SubGroupID,	FFU.BuyerID, 
            FFU.GSMID,	FFU.CompositionID,	FFU.Width,	FFU.BatchNo,	FFU.WeightSheetNo,
            FFU.FinishFabricUtilizationQTYinkg,	EOM.ExportOrderNo,	ISNULL(ISVC.SegmentValue,''),	ISNULL(CT.ShortName,''),	(ISV2.SegmentValue + ' ' + ISV1.SegmentValue);

            Select GFU.* ,ExportOrderNo = ISNULL(EOM.ExportOrderNo,''), ColorName = ISNULL(ISVC.SegmentValue,''), Buyer = ISNULL(CT.ShortName,''),
            FabricType = ISVFT.SegmentValue,Composition  = ISVCOM.SegmentValue
            FROM {TableNames.FBookingAcknowledgeChildGFUtilization} GFU
            INNER JOIN {TableNames.YarnBookingChild_New} C ON C.YBChildID = GFU.YBChildID
            INNER JOIN {TableNames.YarnBookingMaster_New} M ON M.YBookingID = C.YBookingID
            INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = M.BookingID
            Left Join {DbNames.EPYSL}..ExportOrderMaster EOM On EOM.ExportOrderID = GFU.ExportOrderID
            Left Join {DbNames.EPYSL}..ItemSegmentValue ISVC On ISVC.SegmentValueID = GFU.ColorID
            Left Join {DbNames.EPYSL}..ItemSegmentValue ISVFT On ISVFT.SegmentValueID = GFU.FabricTypeID
            Left Join {DbNames.EPYSL}..ItemSegmentValue ISVCOM On ISVCOM.SegmentValueID = GFU.CompositionID
            Left Join {DbNames.EPYSL}..Contacts CT On CT.ContactID = GFU.BuyerID And CT.ContactID >0
            Where M.YBookingNo='{yBookingNo}';



            ---BulkBookingGreyYarnUtilization
            Select 
            ItemMasterID = YSS.ItemMasterId, 
            SpinnerID = YSS.SpinnerId, Spinner = SPIN.ShortName, PhysicalLot = YSS.YarnLotNo,
            YSS.PhysicalCount, Composition = ISV1.SegmentValue, NumaricCount = ISV6.SegmentValue,
            YarnDetails = YSS.YarnCategory,
            YSM.SampleStockQty, YSM.LiabilitiesStockQty, YSM.UnusableStockQty, YSM.LeftoverStockQty,
            GYU.*
            FROM {TableNames.BulkBookingGreyYarnUtilization} GYU
			INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI On YBCI.YBChildItemID = GYU.YBChildItemID
            INNER JOIN {TableNames.YarnBookingChild_New} C ON C.YBChildID = YBCI.YBChildID  AND C.YBookingID = YBCI.YBookingID
            INNER JOIN {TableNames.YarnBookingMaster_New} M ON M.YBookingID = YBCI.YBookingID
            INNER JOIN YarnStockSet YSS ON YSS.YarnStockSetId = GYU.YarnStockSetID
            INNER JOIN YarnStockMaster YSM ON YSM.YarnStockSetId = YSS.YarnStockSetId
            INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YSS.ItemMasterId
            INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
            INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
            LEFT JOIN {DbNames.EPYSL}..Contacts SPIN On SPIN.ContactID = YSS.SpinnerId
            Where M.YBookingNo = '{yBookingNo}';

            ---BulkBookingDyedYarnUtilization
            Select 
            DYU.*,EOM.ExportOrderNo, Buyer = CC.ShortName
            FROM {TableNames.BulkBookingDyedYarnUtilization} DYU
			INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI On YBCI.YBChildItemID = DYU.YBChildItemID
            INNER JOIN {TableNames.YarnBookingChild_New} C ON C.YBChildID = YBCI.YBChildID  AND C.YBookingID = YBCI.YBookingID
            INNER JOIN {TableNames.YarnBookingMaster_New} M ON M.YBookingID = YBCI.YBookingID
			INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = M.BookingID
			INNER JOIN {DbNames.EPYSL}..ExportOrderMaster EOM On EOM.ExportOrderID = DYU.ExportOrderID
			INNER JOIN {DbNames.EPYSL}..Contacts CC On CC.ContactID = DYU.BuyerID
            Where  M.YBookingNo =  '{yBookingNo}';

            ---FBookingAcknowledgeChildReplacement
            SELECT U.*
            FROM {TableNames.FBookingAcknowledgeChildReplacement} U
            INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = U.YBChildID
            INNER JOIN {TableNames.YarnBookingMaster_New} M ON M.YBookingID = YBC.YBookingID
            INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.BookingChildID = U.BookingChildID
            INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.FBAckID = FBC.AcknowledgeID
            WHERE M.YBookingNo =  '{yBookingNo}';
            
            ----FBookingAcknowledgeChildItemNetReqQTY
            SELECT U.*
            FROM {TableNames.FBookingAcknowledgeChildItemNetReqQTY} U
			INNER JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID = U.YBChildItemID
            INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
            INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
            INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.BookingChildID = YBC.BookingChildID
            INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.FBAckID = FBC.AcknowledgeID
            Where YBM.YBookingNo = '{yBookingNo}'
			Group By U.ReplacementID,	U.YBChildItemID,	U.ReasonID,	
			U.DepertmentID,	U.ReplacementQTY,	U.Remarks,	U.AddedBy,	
			U.DateAdded,	U.UpdatedBy,	U.DateUpdated;

            ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                //var records = await _connection.QueryMultipleAsync(sql, segmentNames);
                //List<YarnBookingMaster> datas = records.Read<YarnBookingMaster>().ToList();
                List<YarnBookingMaster> datas = records.Read<YarnBookingMaster>().ToList();
                List<YarnBookingChild> childs = records.Read<YarnBookingChild>().ToList();
                List<YarnBookingChildItem> childItems = records.Read<YarnBookingChildItem>().ToList();
                List<BulkBookingFinishFabricUtilization> fFinishFabricUtilizationList = records.Read<BulkBookingFinishFabricUtilization>().ToList();
                List<FBookingAcknowledgeChildGFUtilization> gGreyFabricUtilizationList = records.Read<FBookingAcknowledgeChildGFUtilization>().ToList();
                List<BulkBookingGreyYarnUtilization> gGreyYarnUtilizationList = records.Read<BulkBookingGreyYarnUtilization>().ToList();
                List<BulkBookingDyedYarnUtilization> gDyedYarnUtilizationList = records.Read<BulkBookingDyedYarnUtilization>().ToList();
                List<FBookingAcknowledgeChildReplacement> childReplacementList = records.Read<FBookingAcknowledgeChildReplacement>().ToList();
                List<FBookingAcknowledgeChildItemNetReqQTY> childItemNetReqQTYList = records.Read<FBookingAcknowledgeChildItemNetReqQTY>().ToList();

                datas.ForEach(m =>
                {
                    m.Childs = childs.ToList();
                    m.Childs.ForEach(x =>
                    {
                        x.ChildItems = childItems.Where(ci => ci.YBChildID == x.YBChildID).ToList();
                        x.ChildItems.ForEach(CI =>
                        {
                            CI.GreyYarnUtilizationPopUpList = gGreyYarnUtilizationList.Where(CItem => CItem.YBChildItemID == CI.YBChildItemID).ToList();
                            CI.DyedYarnUtilizationPopUpList = gDyedYarnUtilizationList.Where(CItem => CItem.YBChildItemID == CI.YBChildItemID).ToList();
                            CI.AdditionalNetReqPOPUPList = childItemNetReqQTYList.Where(CItem => CItem.YBChildItemID == CI.YBChildItemID).ToList();

                        });
                        //x.ChildItemsRevision = yarnBookingChildItemsRevision.Where(c => c.YBChildID == x.YBChildID).ToList();
                        x.FinishFabricUtilizationPopUpList = fFinishFabricUtilizationList.Where(c => c.YBChildID == x.YBChildID).ToList();
                        x.GreyFabricUtilizationPopUpList = gGreyFabricUtilizationList.Where(c => c.YBChildID == x.YBChildID).ToList();
                        x.AdditionalReplacementPOPUPList = childReplacementList.Where(c => c.YBChildID == x.YBChildID).ToList();

                    });
                });



                /* datas.ForEach(m =>
                 {
                     m.Childs = childs.Where(c => c.YBookingID == m.YBookingID).ToList();
                     m.Childs.ForEach(c =>
                     {
                         c.ChildItems = childItems.Where(ci => ci.YBChildID == c.YBChildID).ToList();
                     });
                 });*/

                return datas;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open) _connection.Close();
            }
        }
        public async Task<List<YarnBookingChildItem>> GetYanBookingChildItems(string sYBChildItemIDs)
        {
            string sql = $@"SELECT * FROM {TableNames.YarnBookingChildItem_New} WHERE YBChildItemID IN ({sYBChildItemIDs})";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<YarnBookingChildItem> datas = records.Read<YarnBookingChildItem>().ToList();
                return datas;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open) _connection.Close();
            }
        }
        public async Task<List<YarnBookingChildItem>> GetYanBookingChildItemsWithRevision(string sYBChildItemIDs)
        {
            string sql = $@"SELECT * FROM {TableNames.YarnBookingChildItem_New} WHERE YBChildItemID IN ({sYBChildItemIDs})
                
                            SELECT * FROM {TableNames.YarnBookingChildItem_New_Revision} WHERE YBChildItemID IN ({sYBChildItemIDs})";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<YarnBookingChildItem> datas = records.Read<YarnBookingChildItem>().ToList();
                List<YarnBookingChildItem> datasRevision = records.Read<YarnBookingChildItem>().ToList();
                if (datasRevision.Count > 0)
                {
                    datas = datasRevision;
                }
                return datas;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open) _connection.Close();
            }
        }
        public async Task<List<YarnBookingChildItem>> GetYanBookingChildItemsByBookingNo(string bookingNo)
        {
            string sql = $@"SELECT YBCI.*, YBC.BookingChildID, SubGroupId = YBM.SubGroupID
                    FROM {TableNames.YarnBookingChildItem_New} YBCI
                    INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
                    INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
                    INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = YBM.BookingID
                    WHERE FBA.BookingNo = '{bookingNo}'";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<YarnBookingChildItem> datas = records.Read<YarnBookingChildItem>().ToList();

                return datas;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open) _connection.Close();
            }
        }
        public async Task<List<YarnBookingChildItem>> GetYanBookingChildItemsByBookingNoWithRevision(string bookingNo, bool isAddition)
        {
            string sql = $@"SELECT YBCI.*, YBC.BookingChildID, SubGroupId = YBM.SubGroupID
                    ,Segment1ValueId = ISV1.SegmentValueID
                    ,Segment1ValueDesc = ISV1.SegmentValue
                    ,Segment2ValueId = ISV2.SegmentValueID
                    ,Segment2ValueDesc = ISV2.SegmentValue
                    ,Segment3ValueId = ISV3.SegmentValueID
                    ,Segment3ValueDesc = ISV3.SegmentValue
                    ,Segment4ValueId = ISV4.SegmentValueID
                    ,Segment4ValueDesc = ISV4.SegmentValue
                    ,Segment5ValueId = ISV5.SegmentValueID
                    ,Segment5ValueDesc = ISV5.SegmentValue
                    ,Segment6ValueId = ISV6.SegmentValueID
                    ,Segment6ValueDesc = ISV6.SegmentValue
                    FROM {TableNames.YarnBookingChildItem_New} YBCI
                    INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
                    INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
                    INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = YBM.BookingID

                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YBCI.YItemMasterID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                    WHERE FBA.BookingNo = '{bookingNo}'

                    SELECT YBCI.*, YBC.BookingChildID, SubGroupId = YBM.SubGroupID
                    ,Segment1ValueId = ISV1.SegmentValueID
                    ,Segment1ValueDesc = ISV1.SegmentValue
                    ,Segment2ValueId = ISV2.SegmentValueID
                    ,Segment2ValueDesc = ISV2.SegmentValue
                    ,Segment3ValueId = ISV3.SegmentValueID
                    ,Segment3ValueDesc = ISV3.SegmentValue
                    ,Segment4ValueId = ISV4.SegmentValueID
                    ,Segment4ValueDesc = ISV4.SegmentValue
                    ,Segment5ValueId = ISV5.SegmentValueID
                    ,Segment5ValueDesc = ISV5.SegmentValue
                    ,Segment6ValueId = ISV6.SegmentValueID
                    ,Segment6ValueDesc = ISV6.SegmentValue
                    FROM {TableNames.YarnBookingChildItem_New_Revision} YBCI
                    INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
                    INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
                    INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = YBM.BookingID

                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YBCI.YItemMasterID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                    WHERE FBA.BookingNo = '{bookingNo}'";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<YarnBookingChildItem> datas = records.Read<YarnBookingChildItem>().ToList();
                List<YarnBookingChildItem> datasRevision = records.Read<YarnBookingChildItem>().ToList();
                if (datasRevision.Count > 0)
                {
                    datas = datasRevision;
                }
                return datas;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open) _connection.Close();
            }
        }
        public async Task<List<YarnBookingChildItem>> GetYanBookingChildItemsByYBookingNoWithRevision(string ybookingNo, bool isAddition)
        {
            string sql = $@"SELECT YBCI.*, YBC.BookingChildID, SubGroupId = YBM.SubGroupID
                    ,Segment1ValueId = ISV1.SegmentValueID
                    ,Segment1ValueDesc = ISV1.SegmentValue
                    ,Segment2ValueId = ISV2.SegmentValueID
                    ,Segment2ValueDesc = ISV2.SegmentValue
                    ,Segment3ValueId = ISV3.SegmentValueID
                    ,Segment3ValueDesc = ISV3.SegmentValue
                    ,Segment4ValueId = ISV4.SegmentValueID
                    ,Segment4ValueDesc = ISV4.SegmentValue
                    ,Segment5ValueId = ISV5.SegmentValueID
                    ,Segment5ValueDesc = ISV5.SegmentValue
                    ,Segment6ValueId = ISV6.SegmentValueID
                    ,Segment6ValueDesc = ISV6.SegmentValue
                    FROM {TableNames.YarnBookingChildItem_New} YBCI
                    INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
                    INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
                    INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = YBM.BookingID

                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YBCI.YItemMasterID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                    WHERE YBM.YBookingNo = '{ybookingNo}'

                    SELECT YBCI.*, YBC.BookingChildID, SubGroupId = YBM.SubGroupID
                    ,Segment1ValueId = ISV1.SegmentValueID
                    ,Segment1ValueDesc = ISV1.SegmentValue
                    ,Segment2ValueId = ISV2.SegmentValueID
                    ,Segment2ValueDesc = ISV2.SegmentValue
                    ,Segment3ValueId = ISV3.SegmentValueID
                    ,Segment3ValueDesc = ISV3.SegmentValue
                    ,Segment4ValueId = ISV4.SegmentValueID
                    ,Segment4ValueDesc = ISV4.SegmentValue
                    ,Segment5ValueId = ISV5.SegmentValueID
                    ,Segment5ValueDesc = ISV5.SegmentValue
                    ,Segment6ValueId = ISV6.SegmentValueID
                    ,Segment6ValueDesc = ISV6.SegmentValue
                    FROM {TableNames.YarnBookingChildItem_New_Revision} YBCI
                    INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
                    INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
                    INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = YBM.BookingID

                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YBCI.YItemMasterID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                    WHERE YBM.YBookingNo = '{ybookingNo}'";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<YarnBookingChildItem> datas = records.Read<YarnBookingChildItem>().ToList();
                List<YarnBookingChildItem> datasRevision = records.Read<YarnBookingChildItem>().ToList();
                if (datasRevision.Count > 0)
                {
                    datas = datasRevision;
                }
                return datas;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open) _connection.Close();
            }
        }
        public async Task<List<YarnBookingChild>> GetYarnChilds(string bookingNo, int subGroupId, int consumptionID, int itemMasterId, string construction)
        {
            var query = $@"
                WITH YB AS
                (
	                SELECT YBC.YBChildID,YBC.YBookingID,YBC.ConsumptionID,YBC.ItemMasterID,YBC.YarnTypeID,YBC.YarnBrandID,YBC.FUPartID,YBC.BookingUnitID,YBC.BookingQty,YBC.FTechnicalName,YBC.IsCompleteReceive,YBC.IsCompleteDelivery,YBC.LastDCDate,YBC.ClosingRemarks,YBC.QtyInKG,YBC.ExcessPercentage,YBC.ExcessQty,YBC.ExcessQtyInKG,YBC.TotalQty,YBC.TotalQtyInKG,YBC.BookingChildID,YBC.GreyReqQty,YBC.GreyLeftOverQty,YBC.GreyProdQty,YBC.IsForFabric,YBC.YarnAllowance,YBC.FinishFabricUtilizationQty,YBC.ReqFinishFabricQty,
	                IM.Segment1ValueID,IM.Segment2ValueID,IM.Segment3ValueID,IM.Segment4ValueID,
	                IM.Segment5ValueID,IM.Segment6ValueID,IM.Segment7ValueID,
            	
	                Segment1ValueDesc = ISV1.SegmentValue, Segment2ValueDesc = ISV2.SegmentValue,
	                Segment3ValueDesc = ISV3.SegmentValue, Segment4ValueDesc = ISV4.SegmentValue,
	                Segment5ValueDesc = ISV5.SegmentValue, Segment6ValueDesc = ISV6.SegmentValue,
	                Segment7ValueDesc = ISV7.SegmentValue,
				
	                ISV11.SegmentValue

	                FROM {TableNames.YarnBookingChildItem_New} YBCI
	                INNER JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID = YBCI.YBChildID
	                INNER JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID = YBC.YBookingID
	                INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBAC ON FBAC.BookingChildID = YBC.BookingChildID AND FBAC.BookingID = YBM.BookingID AND FBAC.ConsumptionID = YBC.ConsumptionID
	                INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = YBM.BookingID
	                INNER JOIN {DbNames.EPYSL}..ItemMaster IM1 ON IM1.ItemMasterID = FBAC.ItemMasterID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV11 ON ISV11.SegmentValueID = IM1.Segment1ValueID
				
	                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YBCI.YItemMasterID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
	                WHERE YBM.YBookingNo LIKE '%{bookingNo}%'
	                AND YBM.YBookingNo NOT LIKE '%-Add-%'
	                AND YBM.SubGroupID = {subGroupId}
	                AND YBC.ItemMasterID = {itemMasterId}
	                AND ISV11.SegmentValue = '{construction}'
                ),
                BDS AS
                (
	                SELECT YBChildID = 0,YBookingID = 0,FCM.ConsumptionID,YBCI.ItemMasterID,YarnTypeID=0,FBAC.YarnBrandID,FUPartID = 0,FBAC.BookingUnitID,YBCI.BookingQty,
	                FTechnicalName='',FBAC.IsCompleteReceive,FBAC.IsCompleteDelivery,FBAC.LastDCDate,FBAC.ClosingRemarks,FCM.QtyInKG,FCM.ExcessPercentage,FCM.ExcessQty,
	                FCM.ExcessQtyInKG,FCM.TotalQty,FCM.TotalQtyInKG,FCM.BookingChildID,FBAC.GreyReqQty,FBAC.GreyLeftOverQty,FBAC.GreyProdQty,IsForFabric=0,YarnAllowance=YBCI.Allowance,
	                FinishFabricUtilizationQty=0,ReqFinishFabricQty=0,
	                IM.Segment1ValueID,IM.Segment2ValueID,IM.Segment3ValueID,IM.Segment4ValueID,
	                IM.Segment5ValueID,IM.Segment6ValueID,IM.Segment7ValueID,
            	
	                Segment1ValueDesc = ISV1.SegmentValue, Segment2ValueDesc = ISV2.SegmentValue,
	                Segment3ValueDesc = ISV3.SegmentValue, Segment4ValueDesc = ISV4.SegmentValue,
	                Segment5ValueDesc = ISV5.SegmentValue, Segment6ValueDesc = ISV6.SegmentValue,
	                Segment7ValueDesc = ISV7.SegmentValue,
				
	                ISV11.SegmentValue

	                FROM {TableNames.RND_FREE_CONCEPT_MR_CHILD} YBCI
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} YBC ON YBC.FCMRMasterID = YBCI.FCMRMasterID
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = YBC.ConceptID
	                INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBAC ON FBAC.BookingChildID = FCM.BookingChildID AND FBAC.BookingChildID = FCM.BookingChildID

	                INNER JOIN {DbNames.EPYSL}..ItemMaster IM1 ON IM1.ItemMasterID = FCM.ItemMasterID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV11 ON ISV11.SegmentValueID = IM1.Segment1ValueID
				
	                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YBCI.ItemMasterID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
	                WHERE FCM.GroupConceptNo LIKE '%{bookingNo}%'
	                AND FCM.SubGroupID = {subGroupId}
	                AND YBCI.ItemMasterID = {itemMasterId}
	                AND ISV11.SegmentValue = '{construction}'
	                AND FCM.IsBDS = 1
                ),
                Concept AS
                (
	                SELECT YBChildID = 0,YBookingID = 0,FCM.ConsumptionID,YBCI.ItemMasterID,YarnTypeID=0,FBAC.YarnBrandID,FUPartID = 0,FBAC.BookingUnitID,YBCI.BookingQty,
	                FTechnicalName='',FBAC.IsCompleteReceive,FBAC.IsCompleteDelivery,FBAC.LastDCDate,FBAC.ClosingRemarks,FCM.QtyInKG,FCM.ExcessPercentage,FCM.ExcessQty,
	                FCM.ExcessQtyInKG,FCM.TotalQty,FCM.TotalQtyInKG,FCM.BookingChildID,FBAC.GreyReqQty,FBAC.GreyLeftOverQty,FBAC.GreyProdQty,IsForFabric=0,YarnAllowance=YBCI.Allowance,
	                FinishFabricUtilizationQty=0,ReqFinishFabricQty=0,
	                IM.Segment1ValueID,IM.Segment2ValueID,IM.Segment3ValueID,IM.Segment4ValueID,
	                IM.Segment5ValueID,IM.Segment6ValueID,IM.Segment7ValueID,
            	
	                Segment1ValueDesc = ISV1.SegmentValue, Segment2ValueDesc = ISV2.SegmentValue,
	                Segment3ValueDesc = ISV3.SegmentValue, Segment4ValueDesc = ISV4.SegmentValue,
	                Segment5ValueDesc = ISV5.SegmentValue, Segment6ValueDesc = ISV6.SegmentValue,
	                Segment7ValueDesc = ISV7.SegmentValue,
				
	                ISV11.SegmentValue

	                FROM {TableNames.RND_FREE_CONCEPT_MR_CHILD} YBCI
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} YBC ON YBC.FCMRMasterID = YBCI.FCMRMasterID
	                INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = YBC.ConceptID
	                INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBAC ON FBAC.BookingChildID = FCM.BookingChildID AND FBAC.BookingChildID = FCM.BookingChildID

	                INNER JOIN {DbNames.EPYSL}..ItemMaster IM1 ON IM1.ItemMasterID = FCM.ItemMasterID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV11 ON ISV11.SegmentValueID = IM1.Segment1ValueID
				
	                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YBCI.ItemMasterID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
	                WHERE FCM.GroupConceptNo LIKE '%{bookingNo}%'
	                AND FCM.SubGroupID = {subGroupId}
	                AND YBCI.ItemMasterID = {itemMasterId}
	                AND ISV11.SegmentValue = '{construction}'
	                AND FCM.IsBDS = 0
                ),
                FinalList AS
                (
	                SELECT * FROM YB
	                UNION
	                SELECT * FROM BDS
	                UNION
	                SELECT * FROM Concept
                )
                SELECT * FROM FinalList

                --WHERE RS.RefSourceNo LIKE '{bookingNo}%' 
                --AND YBC.ConsumptionID = {consumptionID} AND YBM.SubGroupID = {subGroupId}";

            return await _service.GetDataAsync<YarnBookingChild>(query);
        }
        public async Task<List<FBookingAckChildFinishingProcess>> GetFinishingProcesses(string bookingNo, int subGroupId, int consumptionID, int itemMasterId, string construction)
        {
            var query = $@"
                SELECT FP.FPChildID, FP.BookingChildID, FP.ProcessID, FP.ColorID, FP.SeqNo, FP.ProcessTypeID, FP.IsPreProcess, FP.Remarks,
                FMC.FMProcessID ProcessID, FMC.ProcessName ProcessName, FMC.ProcessTypeID, ET.ValueName ProcessType,FMC.FMCMasterID, b.ProcessName MachineName,
                FBC.MachineDia,FBC.MachineGauge
                FROM {TableNames.FBBOOKING_ACKNOWLEDGE} FBA
                INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.AcknowledgeID = FBA.FBAckID AND FBC.BookingID = FBA.BookingID
                INNER JOIN {TableNames.FBookingAckChildFinishingProcess} FP ON FP.BookingChildID = FBC.BookingChildID
				Inner JOIN {TableNames.FinishingMachineProcess_HK} FMC ON FMC.FMProcessID = FP.ProcessID
                Inner JOIN {TableNames.FINISHING_MACHINE_CONFIGURATION_MASTER} b on b.FMCMasterID = FMC.FMCMasterID
                Inner join {DbNames.EPYSL}..EntityTypeValue ET on ET.ValueID = FMC.ProcessTypeID
                WHERE FBA.BookingNo LIKE '%{bookingNo}%' 
                AND FBC.SubGroupID = {subGroupId}
                --AND FBC.ConsumptionID = {consumptionID}
                AND FBC.ItemMasterID = {itemMasterId}
				GROUP BY FP.FPChildID, FP.BookingChildID, FP.ProcessID, FP.ColorID, FP.SeqNo, FP.ProcessTypeID, FP.IsPreProcess, FP.Remarks,
                FMC.FMProcessID, FMC.ProcessName, FMC.ProcessTypeID, ET.ValueName,FMC.FMCMasterID, b.ProcessName,FBC.MachineDia,FBC.MachineGauge;";

            return await _service.GetDataAsync<FBookingAckChildFinishingProcess>(query);
        }

        public async Task<YarnBookingMaster> GetSaveYarnList(bool isSample, string bookingNo, string yBookingNo,
            string reasonStatus, int yBookingID, Status status)
        {
            string sql;
            //var segmentNames = new
            //{
            //    SegmentNames = new string[]
            //    {
            //        ItemSegmentNameConstants.YARN_SUBPROGRAM_NEW,
            //        ItemSegmentNameConstants.YARN_CERTIFICATIONS
            //    }
            //};
            if (isSample)
            {
                //No need master table query because master data get in the Data Grid on the front side.  
                sql = $@" 
                    With YBM1 As
                    (
						SELECT * FROM {TableNames.YarnBookingMaster_New} WHERE YBookingID = {yBookingID}
                    ) 
                    Select BookingID = SBM.BookingID, 0 BOMMasterID, EM.StyleMasterID, 'Fabric' GroupName, 
                    EM.ExportOrderID, SBM.BookingNo, EM.ExportOrderNo, SBM.BuyerID, SBM.BuyerTeamID, 0 As CompanyID, 
                    SBM.ExportOrderID, SBM.SubGroupID, CCT.TeamName BuyerDepartment, EMP.EmployeeName MerchandiserName, 
                    YearName, RequiredDate = SBM.FirstInHouseDate, 
                    ReferenceNo = Case When ISNULL(SBM.ReferenceNo,'') = '' then 'Sample' Else ISNULL(SBM.ReferenceNo,'') End, 
                    EM.CalendarDays  As TNADays, 1 RevStatus,
                    ISNULL(FBK.RevisionNo,0) PreProcessRevNo, YBM.YBookingID, YBM.YBookingNo, YBM.RevisionNo, YBM.YBookingDate, CE.CompanyName, 
                    YBM.YInHouseDate, YBM.YRequiredDate, YBM.ContactPerson, YBM.WithoutOB, YBM.AdditionalBooking
                    from YBM1
					LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = YBM1.BookingID
                    LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBK On FBK.BookingID = SBM.BookingID
                    Inner Join {DbNames.EPYSL}..ExportOrderMaster EM On EM.ExportOrderID = SBM.ExportOrderID
                    Inner Join {DbNames.EPYSL}..FinancialYear FY On SBM.BookingDate between FY.StartMonth and FY.EndMonth
                    Inner Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = SBM.BuyerTeamID
                    Inner Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = SBM.AddedBy
                    Inner Join {DbNames.EPYSL}..Employee EMP ON EMP.EmployeeCode = LU.EmployeeCode
                    Left Join 
                    (Select top 1 * FROM {TableNames.YarnBookingMaster_New} Where BookingID In(
                        Select top 1 BookingID From {DbNames.EPYSL}..SampleBookingMaster Where BookingNo = '{bookingNo}'))YBM
                    On YBM.BookingID = SBM.BookingID
                    Left Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = SBM.ExecutionCompanyID
                    Where EM.IsAutoGenerateNo = 1
                    Group By SBM.BookingID, EM.StyleMasterID, EM.ExportOrderID, SBM.BookingNo, EM.ExportOrderNo, SBM.BuyerID, SBM.BuyerTeamID,  
                    SBM.ExportOrderID, SBM.SubGroupID, CCT.TeamName, EMP.EmployeeName, YearName, SBM.FirstInHouseDate, 
                    SBM.ReferenceNo, EM.CalendarDays,ISNULL(FBK.RevisionNo,0), YBM.YBookingID, YBM.YBookingNo, YBM.RevisionNo, YBM.YBookingDate, CE.CompanyName, 
                    YBM.YInHouseDate, YBM.YRequiredDate, YBM.ContactPerson, YBM.WithoutOB, YBM.AdditionalBooking;  

                    -- Booking Child Information
                    With BM As 
                    (
	                    Select SBC.BookingID, SBC.ConsumptionID, SBC.ItemMasterID, SBCon.SubGroupID, ISG.SubGroupName,
                        ColorID = Case When ISG.SubGroupName in('Collar','Cuff') Then IM.Segment5ValueID Else IM.Segment3ValueID End,
	                    BookingQty = 
		                    Case When ISG.SubGroupName in('Collar','Cuff') 
			                    Then (0.045 * ((Convert(decimal(18,3), SBCon.Segment3Desc) * 
			                    Convert(decimal(18,3), SBCon.Segment4Desc))/420)) * 
			                    (Sum(SBC.RequiredQty) * U.RelativeFactor) 
		                    Else Sum(SBC.RequiredQty) End,  
	                    BookingUnitID = (Case When ISG.SubGroupName='Collar' Or ISG.SubGroupName='Cuff' Then 28 
					                    Else SBC.RequiredUnitID End),
	                    BookingUOM = Case When ISG.SubGroupName='Collar' Or ISG.SubGroupName='Cuff' Then 'KG' 
				                     Else U.DisplayUnitDesc End,  
	                    SBCon.Segment1Desc Segment1ValueDesc, SBCon.Segment2Desc Segment2ValueDesc,SBCon.Segment3Desc Segment3ValueDesc,
	                    SBCon.Segment4Desc Segment4ValueDesc,
	                    SBCon.Segment5Desc Segment5ValueDesc,SBCon.Segment6Desc Segment6ValueDesc,SBCon.Segment7Desc Segment7ValueDesc,
	                    SBCon.Segment8Desc Segment8ValueDesc, SBCon.Segment9Desc Segment9ValueDesc,SBCon.Segment10Desc Segment10ValueDesc,
	                    SBCon.Segment11Desc Segment11ValueDesc,SBCon.Segment12Desc Segment12ValueDesc,SBCon.Segment13Desc Segment13ValueDesc,
	                    SBCon.Segment14Desc Segment14ValueDesc,SBCon.Segment15Desc Segment15ValueDesc, 
	                    SBCon.FUPartID, SBCon.A1ValueID As YarnTypeID, ISV1.SegmentValue YarnType, SBCon.YarnBrandID, ETV.ValueName YarnBrand, FUP.PartName, 
	                    Convert(bit,'0') IsCompleteReceive, Convert(bit,'0') IsCompleteDelivery  
	                    From {DbNames.EPYSL}..SampleBookingConsumptionChild SBC 
	                    Inner Join {DbNames.EPYSL}..SampleBookingConsumption SBCon On SBCon.ConsumptionID = SBC.ConsumptionID
	                    Inner Join {DbNames.EPYSL}..SampleBookingMaster M On M.BookingID = SBCon.BookingID
	                    Inner Join {DbNames.EPYSL}..Unit U On U.UnitID = SBC.RequiredUnitID
	                    Inner Join {DbNames.EPYSL}..ItemGroup IG On IG.ItemGroupID = SBC.ItemGroupID
	                    Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SBC.SubGroupID
	                    Left Join {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = SBCon.A1ValueID
	                    Left Join {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = SBCon.YarnBrandID
	                    Left Join {DbNames.EPYSL}..FabricUsedPart FUP On FUP.FUPartID = SBCon.FUPartID
                        Left Join {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID=SBC.ItemMasterID
	                    Where M.BookingNo = '{bookingNo}'                               
	                    Group By SBC.BookingID, SBC.ConsumptionID, SBCon.SubGroupID, SBC.ItemMasterID, ISG.SubGroupName,
	                    SBCon.Segment1Desc,SBCon.Segment2Desc,SBCon.Segment3Desc,SBCon.Segment4Desc,SBCon.Segment5Desc,SBCon.Segment6Desc,
	                    SBCon.Segment7Desc,SBCon.Segment8Desc, SBCon.Segment9Desc, SBCon.Segment10Desc, SBCon.Segment11Desc, SBCon.Segment12Desc, 
	                    SBCon.Segment13Desc, SBCon.Segment14Desc, SBCon.Segment15Desc,
	                    SBC.RequiredUnitID,U.DisplayUnitDesc,SBCon.LengthYds,SBCon.LengthInch,SBCon.FUPartID,SBCon.A1ValueID,SBCon.YarnBrandID,
	                    IsNull(SBCon.LabDipNo,''),SBCon.Remarks,ISV1.SegmentValue,ETV.ValueName,FUP.PartName, M.BuyerID, 
	                    M.BuyerTeamID, M.ExportOrderID, U.RelativeFactor,
                        Case When ISG.SubGroupName in('Collar','Cuff') Then IM.Segment5ValueID Else IM.Segment3ValueID End
                    ),
                    YBM As 
                    (
	                    Select YBC.YBChildID, YBC.YBookingID, YBC.ConsumptionID, YBC.ItemMasterID, YBC.YarnTypeID, YBC.YarnBrandID, ETV.ValueName YarnBrand, 
                        YBC.FUPartID, FUP.PartName, YBC.BookingUnitID, BC.Remarks, ISV1.SegmentValue YarnType, U.DisplayUnitDesc As BookingUOM, YBM.BookingID, 
                        Ceiling(Sum(YBC.BookingQty))BookingQty, 
	                    YBC.FTechnicalName, YBC.IsCompleteReceive, YBC.IsCompleteDelivery, BC.SubGroupID,ISG.SubGroupName,
                        BC.Segment1Desc Segment1ValueDesc, BC.Segment2Desc Segment2ValueDesc, BC.Segment3Desc Segment3ValueDesc, BC.Segment4Desc Segment4ValueDesc, 
                        BC.Segment5Desc Segment5ValueDesc, BC.Segment6Desc Segment6ValueDesc, BC.Segment7Desc Segment7ValueDesc, BC.Segment8Desc Segment8ValueDesc,
                        BC.Segment9Desc Segment9ValueDesc, BC.Segment10Desc Segment10ValueDesc, BC.Segment11Desc Segment11ValueDesc, BC.Segment12Desc Segment12ValueDesc,
                        BC.Segment13Desc Segment13ValueDesc, BC.Segment14Desc Segment14ValueDesc, BC.Segment15Desc Segment15ValueDesc  
                        FROM {TableNames.YarnBookingMaster_New} YBM 
                        Inner JOIN {TableNames.YarnBookingChild_New} YBC On YBC.YBookingID = YBM.YBookingID
                        left Join {DbNames.EPYSL}..BOMConsumption BC On BC.ConsumptionID = YBC.ConsumptionID
                        Left Join {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = YBC.YarnBrandID 
                        Left Join {DbNames.EPYSL}..FabricUsedPart FUP On FUP.FUPartID = YBC.FUPartID
                        LEFT JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = YBC.BookingUnitID
                        Left Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = BC.SubGroupID
                        Left Join {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = BC.A1ValueID 
	                    Where YBM.YBookingNo = '{yBookingNo}'
                        Group By YBC.YBChildID, YBC.YBookingID, YBC.ConsumptionID, YBC.ItemMasterID, YBC.YarnTypeID, YBC.YarnBrandID, ETV.ValueName, 
                        YBC.FUPartID, FUP.PartName, BC.A1ValueID, ISV1.SegmentValue, BC.Remarks, YBC.BookingUnitID, U.DisplayUnitDesc, YBM.BookingID, YBC.FTechnicalName, YBC.IsCompleteReceive,
                        YBC.IsCompleteDelivery, BC.SubGroupID,ISG.SubGroupName, BC.Segment1Desc, BC.Segment2Desc, BC.Segment3Desc, BC.Segment4Desc, 
                        BC.Segment5Desc, BC.Segment6Desc, BC.Segment7Desc, BC.Segment8Desc, BC.Segment9Desc, BC.Segment10Desc, BC.Segment11Desc, BC.Segment12Desc,
                        BC.Segment13Desc, BC.Segment14Desc, BC.Segment15Desc 
                    ) 
                    Select BM.BookingID, Isnull(YBM.YBookingID,0)YBookingID, Isnull(YBM.YBChildID,0)YBChildID,  
                    BM.ConsumptionID, BM.ItemMasterID, BM.YarnTypeID, BM.YarnBrand, BM.FUPartID, BM.BookingUnitID, BM.BookingUOM, BM.BookingQty, BM.ColorID, YBM.FTechnicalName,
                    BM.IsCompleteReceive, BM.IsCompleteDelivery,
                    BM.SubGroupID,  BM.SubGroupName, BM.YarnBrandID, BM.PartName,  
                    BM.Segment1ValueDesc, BM.Segment2ValueDesc, BM.Segment3ValueDesc, BM.Segment4ValueDesc, BM.Segment5ValueDesc, BM.Segment6ValueDesc,
                    BM.Segment7ValueDesc, BM.Segment8ValueDesc, BM.Segment9ValueDesc, BM.Segment10ValueDesc, BM.Segment11ValueDesc, BM.Segment12ValueDesc,
                    BM.Segment13ValueDesc, BM.Segment14ValueDesc, BM.Segment15ValueDesc
                    From BM Left Join YBM On BM.BookingID = YBM.BookingID 
                    And BM.ItemMasterID = YBM.ItemMasterID AND BM.ConsumptionID = YBM.ConsumptionID; 

                    -- Yarn Booking Child Items
                    ;With YBM As 
                    (
	                    Select * FROM {TableNames.YarnBookingMaster_New} Where YBookingNo = '{yBookingNo}'
                    ),
                    YBCI As ( 
	                    Select YBC.ConsumptionID, YBCI.YBChildItemID, YBCI.YBChildID, YBCI.YBookingID, YBCI.YItemMasterID As ItemMasterID, YBCI.UnitID, U.DisplayUnitDesc, YBCI.Blending, 
	                    (Case When Blending = 1 then 'Blend' else 'Non-Blend' End)BlendingName, YBCI.YarnCategory, YBCI.Distribution, YBCI.BookingQty, YBCI.Allowance, 
                        YBCI.RequiredQty, YBCI.ShadeCode, Y.ShadeCode as ShadeName, 
	                    YBCI.Remarks, YBCI.Specification, YBCI.YD, YBCI.YDItem, YBM.BookingID,
	                    IM.Segment1ValueID Segment1ValueId, IM.Segment2ValueID Segment2ValueId, IM.Segment3ValueID Segment3ValueId, IM.Segment4ValueID Segment4ValueId,
	                    IM.Segment5ValueID Segment5ValueId, IM.Segment6ValueID Segment6ValueId, IM.Segment7ValueID Segment7ValueId, IM.Segment8ValueID Segment8ValueId,
	                    IM.Segment9ValueID Segment9ValueId, IM.Segment10ValueID Segment10ValueId, IM.Segment11ValueID Segment11ValueId, IM.Segment12ValueID Segment12ValueId,
	                    IM.Segment13ValueID Segment13ValueId, IM.Segment14ValueID Segment14ValueId, IM.Segment15ValueID Segment15ValueId,
	                    ISV1.SegmentValue AS Segment1ValueDesc, ISV2.SegmentValue AS Segment2ValueDesc, ISV3.SegmentValue AS Segment3ValueDesc,
	                    ISV4.SegmentValue AS Segment4ValueDesc, ISV5.SegmentValue AS Segment5ValueDesc, ISV6.SegmentValue AS Segment6ValueDesc,
	                    ISV7.SegmentValue AS Segment7ValueDesc, ISV8.SegmentValue AS Segment8ValueDesc, YBM.SubGroupID, ISG.SubGroupName,
                        Isnull(YPM.YDProductionMasterID,0)YDProductionMasterID
	                    From YBM Inner JOIN {TableNames.YarnBookingChild_New} YBC On YBM.YBookingID = YBC.YBookingID
	                    Inner JOIN {TableNames.YarnBookingChildItem_New} YBCI On YBCI.YBChildID = YBC.YBChildID 
	                    Left Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = YBM.SubGroupID
	                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YBCI.YItemMasterID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID
	                    LEFT JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = YBCI.UnitID
                        LEFT JOIN {TableNames.YARN_SHADE_BOOK} Y ON Y.ShadeCode = YBCI.ShadeCode
                        LEFT JOIN {TableNames.YD_BOOKING_MASTER} YDBM ON YDBM.YBookingID = YBM.YBookingID And YDBM.YBookingID = YBCI.YBookingID
	                    LEFT JOIN {TableNames.YD_PRODUCTION_MASTER} YPM ON YPM.YDBookingMasterID = YDBM.YDBookingMasterID
                    ),
                    BM As 
                    (
	                    Select SBC.BookingID, SBC.ConsumptionID, SBCon.SubGroupID,  
	                    BookingQty = 
		                    Case When ISG.SubGroupName in('Collar','Cuff') 
			                    Then (0.045 * ((Convert(decimal(18,3), SBCon.Segment3Desc) * 
			                    Convert(decimal(18,3), SBCon.Segment4Desc))/420)) * 
			                    (Sum(SBC.RequiredQty) * U.RelativeFactor) 
		                    Else Sum(SBC.RequiredQty) End 

	                    From {DbNames.EPYSL}..SampleBookingConsumptionChild SBC 
	                    Inner Join {DbNames.EPYSL}..SampleBookingConsumption SBCon On SBCon.ConsumptionID = SBC.ConsumptionID
	                    Inner Join {DbNames.EPYSL}..SampleBookingMaster M On M.BookingID = SBCon.BookingID
	                    Inner Join {DbNames.EPYSL}..Unit U On U.UnitID = SBC.RequiredUnitID 
	                    Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SBC.SubGroupID
 
	                    Where M.BookingNo = '{bookingNo}'                               
	                    Group By SBC.BookingID, SBC.ConsumptionID, SBCon.SubGroupID, ISG.SubGroupName, SBCon.Segment3Desc, SBCon.Segment4Desc, U.RelativeFactor
                    )
                    Select YBCI.ConsumptionID, YBCI.YBChildItemID, YBCI.YBChildID, YBCI.YBookingID, ItemMasterID, YBCI.UnitID, YBCI.DisplayUnitDesc, YBCI.Blending, 
                    YBCI.BlendingName, YBCI.YarnCategory,  
                    ((BM.BookingQty * YBCI.Distribution) / 100)BookingQty, YBCI.Allowance, YBCI.Distribution,   
                    ((BM.BookingQty * YBCI.Distribution) / 100) + ((((BM.BookingQty * YBCI.Distribution) / 100) * YBCI.Allowance) / 100)RequiredQty, 
                    YBCI.ShadeCode, YBCI.ShadeName,
                    YBCI.Remarks, YBCI.Specification, YBCI.YD, YBCI.YDItem, YBCI.BookingID,
                    YBCI.Segment1ValueId, YBCI.Segment2ValueId, YBCI.Segment3ValueId, YBCI.Segment4ValueId,
                    YBCI.Segment5ValueId, YBCI.Segment6ValueId, YBCI.Segment7ValueId, YBCI.Segment8ValueId,
                    YBCI.Segment9ValueId, YBCI.Segment10ValueId, YBCI.Segment11ValueId, YBCI.Segment12ValueId,
                    YBCI.Segment13ValueId, YBCI.Segment14ValueId, YBCI.Segment15ValueId,
                    YBCI.Segment1ValueDesc, YBCI.Segment2ValueDesc, YBCI.Segment3ValueDesc,
                    YBCI.Segment4ValueDesc, YBCI.Segment5ValueDesc, YBCI.Segment6ValueDesc,
                    YBCI.Segment7ValueDesc, YBCI.Segment8ValueDesc, YBCI.SubGroupID, YBCI.SubGroupName, YBCI.YDProductionMasterID   
                    From YBCI Inner Join BM On YBCI.BookingID = BM.BookingID AND BM.ConsumptionID = YBCI.ConsumptionID; 

                    --All Yarn Booking Master 
                    Select* FROM {TableNames.YarnBookingMaster_New} Where YBookingNo = '{yBookingNo}';

                    -- Employee Information
                    ;Select CAST(E.EmployeeCode As varchar) id,E.EmployeeName [text], (D.Designation + ' , ' + ED.DepertmentDescription) [desc]
                    From  {DbNames.EPYSL}..Employee E
                    Inner Join  {DbNames.EPYSL}..EmployeeDepartment ED On ED.DepertmentID = E.DepertmentID
                    Inner Join {DbNames.EPYSL}..EmployeeDesignation D On D.DesigID = E.DesigID
                    Where E.IsRegular = 1 and ED.DepertmentDescription = 'Supply Chain';

                    -- Shade book
                    {CommonQueries.GetYarnShadeBooks()};

                    -- YarnBookingChildGarmentPart
                    With YBK As
                    (
	                    Select YBCYSB.*,YBC.ConsumptionID, YBM.BookingID, YBC.ItemMasterID, FUP.PartName
	                    FROM {TableNames.YarnBookingChildGarmentPart_New} YBCYSB
	                    Inner JOIN {TableNames.YarnBookingChild_New} YBC On YBC.YBChildID = YBCYSB.YBChildID
	                    Inner Join (Select * FROM {TableNames.YarnBookingMaster_New} Where WithoutOB = 0) YBM On YBM.YBookingID = YBC.YBookingID
	                    Inner Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingID = YBM.BookingID
	                    Inner Join {DbNames.EPYSL}..FabricUsedPart FUP On FUP.FUPartID = YBCYSB.FUPartID
	                    Where BM.BookingNo = '{bookingNo}'
                    ),
                    BK As (
	                    Select BCYSB.*,BC.ConsumptionID, BC.ItemMasterID, FUP.PartName 
	                    From {DbNames.EPYSL}..BookingChildGarmentPart BCYSB
	                    Inner Join {DbNames.EPYSL}..BookingChild BC On BC.BookingChildID = BCYSB.BookingChildID
	                    Inner Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingID = BC.BookingID
	                    Inner Join {DbNames.EPYSL}..FabricUsedPart FUP On FUP.FUPartID = BCYSB.FUPartID
	                    Where BM.BookingNo = '{bookingNo}'
                    )
                    Select YBookingCGPID = ISNULL(YBK.YBookingCGPID,0), YBChildID = ISNULL(YBK.YBChildID,0), BK.BookingID, BK.ConsumptionID, BK.ItemMasterID, 
                    FUPartID = ISNULL(YBK.FUPartID,BK.FUPartID), PartName = ISNULL(YBK.PartName,BK.PartName),
                    IM.Segment1ValueID ConstructionID,IM.Segment2ValueID CompositionID,IM.Segment4ValueID GSMID,IM.Segment7ValueID KnittingTypeID
                    From BK
                    LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BK.ItemMasterID
                    Left Join YBK On YBK.ConsumptionID = BK.ConsumptionID And YBK.BookingID = BK.BookingID And YBK.ItemMasterID = BK.ItemMasterID;

                    -- YarnBookingChildYarnSubBrand
                    ;With YBK As
                    (
	                    Select YBCYSB.*,YBC.ConsumptionID, YBM.BookingID, YBC.ItemMasterID, ETV.ValueName YarnSubBrandName
	                    FROM {TableNames.YarnBookingChildYarnSubBrand_New} YBCYSB
	                    Inner JOIN {TableNames.YarnBookingChild_New} YBC On YBC.YBChildID = YBCYSB.YBChildID
	                    Inner Join (Select * FROM {TableNames.YarnBookingMaster_New} Where WithoutOB = 0) YBM On YBM.YBookingID = YBC.YBookingID
	                    Inner Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingID = YBM.BookingID
	                    Inner Join {DbNames.EPYSL}..EntityTypeValue ETV On ETV.ValueID = YBCYSB.YarnSubBrandID
	                    Where BM.BookingNo = '{bookingNo}'
                    ),
                    BK As (
	                    Select BCYSB.*,BM.BookingID, BC.ConsumptionID, BC.ItemMasterID, ETV.ValueName YarnSubBrandName 
	                    From {DbNames.EPYSL}..BookingChildYarnSubBrand BCYSB
	                    Inner Join {DbNames.EPYSL}..BookingChild BC On BC.BookingChildID = BCYSB.BookingChildID
	                    Inner Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingID = BC.BookingID
	                    Inner Join {DbNames.EPYSL}..EntityTypeValue ETV On ETV.ValueID = BCYSB.YarnSubBrandID
	                    Where BM.BookingNo = '{bookingNo}'
                    )
                    Select YBookingCYSubBrandID = ISNULL(YBK.YBookingCYSubBrandID,0), YBChildID = ISNULL(YBK.YBChildID,0), BK.BookingID, BK.ConsumptionID, BK.ItemMasterID, 
                    YarnSubBrandID = ISNULL(YBK.YarnSubBrandID,BK.YarnSubBrandID), YarnSubBrandName = ISNULL(YBK.YarnSubBrandName,BK.YarnSubBrandName),
                   IM.Segment1ValueID ConstructionID,IM.Segment2ValueID CompositionID,IM.Segment4ValueID GSMID,IM.Segment7ValueID KnittingTypeID
                    From BK
                    LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BK.ItemMasterID
                    Left Join YBK On YBK.ConsumptionID = BK.ConsumptionID And YBK.BookingID = BK.BookingID And YBK.ItemMasterID = BK.ItemMasterID;

                    -- YarnBookingChildItemYarnSubBrand
                    Select ETV.ValueID id, ETV.ValueName [text]
                    From {DbNames.EPYSL}..EntityTypeValue ETV
                    Inner Join {DbNames.EPYSL}..EntityType ET On ET.EntityTypeID = ETV.EntityTypeID
                    Where ET.EntityTypeName = 'Yarn Sub Brand'
                    Order By ETV.ValueName; ";

                if (reasonStatus == "BookingList" || reasonStatus == "AdditionalList")
                {
                    sql += $@"

                        With BAR as
                        (  
	                        Select * from {DbNames.EPYSL}..BookingAdditionalReason Where UseInYBooking = 1 And IsAddition = 1 
                        ),
                        BKRR as
                        ( 
	                        Select YBookingID, ReasonID, YBKReasonID FROM {TableNames.YarnBookingReason_New} Where YBookingID = {yBookingID}
                        )
                        Select Isnull(BKRR.YBKReasonID,0)YBKReasonID, Isnull(BKRR.YBookingID,0)YBookingID, BAR.ReasonID, 
                        BAR.ReasonName, BAR.IsRePurchase, BAR.IsAddition, BAR.IsRevision, BAR.UseInBooking, BAR.Remarks, 
                        Selected = Case When ISNULL(BKRR.ReasonID,0) = 0 then 0 Else 1 End
                        from BAR Left Join BKRR On BKRR.ReasonID = BAR.ReasonID ";
                }
                else if (reasonStatus == "ProposeList" || reasonStatus == "ReviseList" || reasonStatus == "ExecutedList" || reasonStatus == "ExecutedList")
                {
                    sql += $@"
                        With BAR as
                        (  
	                        Select * from {DbNames.EPYSL}..BookingAdditionalReason Where UseInYBooking = 1 And IsRevision = 1 
                        ),
                        BKRR as
                        ( 
	                        Select YBookingID, ReasonID, YBKReasonID FROM {TableNames.YarnBookingReason_New} Where YBookingID = {yBookingID}
                        )
                        Select Isnull(BKRR.YBKReasonID,0)YBKReasonID, Isnull(BKRR.YBookingID,0)YBookingID, BAR.ReasonID, 
                        BAR.ReasonName, BAR.IsRePurchase, BAR.IsAddition, BAR.IsRevision, BAR.UseInBooking, BAR.Remarks, 
                        Selected = Case When ISNULL(BKRR.ReasonID,0) = 0 then 0 Else 1 End
                        from BAR Left Join BKRR On BKRR.ReasonID = BAR.ReasonID ";
                }

                sql += $@"Select * FROM {TableNames.YarnBookingMaster_New} Where YBookingNo = '{yBookingNo}'";
            }
            else
            {
                sql = $@" 
                    With YBM As
                    (
						SELECT * FROM {TableNames.YarnBookingMaster_New} WHERE YBookingID = {yBookingID}
                    )  
                    Select FBA.BookingID, FBA.BOMMasterID, SM.StyleMasterID, EOM.ExportOrderID,BM.BookingNo,
                    EOM.ExportOrderNo, BM.BuyerID, BM.BuyerTeamID, BM.CompanyID, BM.ExportOrderID, BM.SubGroupID, 
                    CCT.TeamName BuyerDepartment,MerchandiserName = EMP.EmployeeName,FY.YearName,
                    RequiredDate = BM.InHouseDate, ReferenceNo = ISNULL(BM.ReferenceNo,''), EOM.CalendarDays As TNADays,
                    RevStatus = Case When Isnull(EL.FabBTexAckStatus,'') = 'Acknowledged' And Isnull(EL.FabBTexAckStatus,'') <> '' Then 1 Else 0 End,
                    ISNULL(FBA.RevisionNo,0) PreProcessRevNo, YBM.YBookingID, YBM.YBookingNo, YBM.RevisionNo, YBM.YBookingDate, CE.CompanyName, YBM.YInHouseDate, YBM.YRequiredDate, 
                    YBM.ContactPerson, YBM.WithoutOB, YBM.AdditionalBooking
                    
					FROM YBM
                    Inner Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingID = YBM.BookingID
					LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = YBM.BookingID 
                    LEFT Join {DbNames.EPYSL}..ExportOrderMaster EOM On EOM.ExportOrderID = BM.ExportOrderID                                       
                    LEFT Join {DbNames.EPYSL}..StyleMaster SM On SM.StyleMasterID = EOM.StyleMasterID 
                    LEFT Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = SM.BuyerTeamID
                    Inner Join {DbNames.EPYSL}..ExportWorkOrderLifeCycleChild EL On EL.ExportOrderID = EOM.ExportOrderID 
	                And EL.ContactID = BM.SupplierID And BM.BookingID = EL.BookingID  --And EL.ItemGroupID = ISG.ItemGroupID										
                    Inner Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = BM.AddedBy
                    Inner Join {DbNames.EPYSL}..Employee EMP ON EMP.EmployeeCode = LU.EmployeeCode
                    Inner Join {DbNames.EPYSL}..FinancialYear FY On FY.FinancialYearID = SM.FinancialYearID
                    Inner Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = BM.CompanyID
                    Group By FBA.BookingID, FBA.BOMMasterID, SM.StyleMasterID,EOM.ExportOrderID,BM.BookingNo,
                    EOM.ExportOrderNo, BM.BuyerID, BM.BuyerTeamID, BM.CompanyID, BM.ExportOrderID, BM.SubGroupID, 
                    CCT.TeamName, EMP.EmployeeName, FY.YearName, BM.InHouseDate, BM.ReferenceNo, EOM.CalendarDays,
                    EL.FabBTexAckStatus, ISNULL(FBA.RevisionNo,0), YBM.YBookingID, YBM.YBookingNo, YBM.RevisionNo, YBM.YBookingDate, CE.CompanyName, YBM.YInHouseDate, YBM.YRequiredDate, 
                    YBM.ContactPerson, YBM.WithoutOB, YBM.AdditionalBooking;

                    -- Booking Child Information
                    With BM As 
                    (
	                    Select BM.BookingID, BC.ConsumptionID, BOMCon.SubGroupID, BC.ItemMasterID, ISG.SubGroupName,  
	                    BookingQty = 
			                    Case When ISG.SubGroupName in('Collar','Cuff') 
				                    Then (0.045 * ((Convert(decimal(18,3), BOMCon.Segment3Desc) * 
				                    Convert(decimal(18,3), BOMCon.Segment4Desc))/420)) * 
				                    (Sum(BC.BookingQty) * U.RelativeFactor) 
			                    Else Sum(BC.BookingQty) End,
                        ColorID = Case When ISG.SubGroupName in('Collar','Cuff') Then IM.Segment5ValueID Else IM.Segment3ValueID End,
	                    BookingUnitID =  Case When ISG.SubGroupName='Collar' Or ISG.SubGroupName='Cuff' Then 28 Else BC.BookingUnitID End,
	                    BookingUOM = Case When ISG.SubGroupName='Collar' Or ISG.SubGroupName='Cuff' Then 'KG' Else U.DisplayUnitDesc End, 
	                    BOMCon.Segment1Desc Segment1ValueDesc, BOMCon.Segment2Desc Segment2ValueDesc, 
	                    BOMCon.Segment3Desc Segment3ValueDesc, BOMCon.Segment4Desc Segment4ValueDesc, BOMCon.Segment5Desc Segment5ValueDesc,
	                    BOMCon.Segment6Desc Segment6ValueDesc, BOMCon.Segment7Desc Segment7ValueDesc, BOMCon.Segment8Desc Segment8ValueDesc, 
	                    BOMCon.Segment9Desc Segment9ValueDesc, BOMCon.Segment10Desc Segment10ValueDesc, BOMCon.Segment11Desc Segment11ValueDesc, 
	                    BOMCon.Segment12Desc Segment12ValueDesc, BOMCon.Segment13Desc Segment13ValueDesc, BOMCon.Segment14Desc Segment14ValueDesc, 
	                    BOMCon.Segment15Desc Segment15ValueDesc,
	                    BOMCon.FUPartID, BOMCon.A1ValueID As YarnTypeID, ISV1.SegmentValue YarnType, 
	                    BOMCon.YarnBrandID, ETV.ValueName YarnBrand, FUP.PartName, BC.IsCompleteReceive, BC.IsCompleteDelivery, 
	                    BC.LastDCDate, BOMCon.Remarks, 
                        IM.Segment1ValueID ConstructionId,IM.Segment2ValueID CompositionId,IM.Segment4ValueID GSMId,IM.Segment7ValueID KnittingTypeId
	                    From  {DbNames.EPYSL}..BookingChild BC 
	                    Inner Join {DbNames.EPYSL}..BookingMaster BM On BC.BookingID = BM.BookingID
	                    Inner Join {DbNames.EPYSL}..BOMConsumption BOMCon On BOMCon.ConsumptionID = BC.ConsumptionID 
	                    Inner Join {DbNames.EPYSL}..Unit U On U.UnitID = BC.BookingUnitID
	                    Inner Join {DbNames.EPYSL}..ItemGroup IG On IG.ItemGroupID = BC.ItemGroupID
	                    Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = BC.SubGroupID 
	                    Left Join {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = BOMCon.A1ValueID
	                    Left Join {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = BOMCon.YarnBrandID
	                    Left Join {DbNames.EPYSL}..FabricUsedPart FUP On FUP.FUPartID = BOMCon.FUPartID 
                        LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BC.ItemMasterID
	                    Where BM.BookingNo = '{bookingNo}' And BC.ISourcing = 1                                      
	                    Group By BC.ConsumptionID, BC.BOMMasterID, BOMCon.SubGroupID, BC.ItemMasterID, ISG.SubGroupName, 
	                    BOMCon.Segment1Desc,BOMCon.Segment2Desc,BOMCon.Segment3Desc,BOMCon.Segment4Desc,BOMCon.Segment5Desc,BOMCon.Segment6Desc,
	                    BOMCon.Segment7Desc,BOMCon.Segment8Desc, BOMCon.Segment9Desc, BOMCon.Segment10Desc, BOMCon.Segment11Desc, BOMCon.Segment12Desc, 
	                    BOMCon.Segment13Desc, BOMCon.Segment14Desc, BOMCon.Segment15Desc, BC.BookingUnitID,U.DisplayUnitDesc, BOMCon.FUPartID, 
	                    BOMCon.A1ValueID, BOMCon.YarnBrandID, ISV1.SegmentValue, ETV.ValueName, FUP.PartName, BC.IsCompleteReceive, 
	                    BC.IsCompleteDelivery, BC.LastDCDate, BOMCon.Remarks, BM.BookingID, U.RelativeFactor,
                        IM.Segment1ValueID,IM.Segment2ValueID,IM.Segment4ValueID,IM.Segment7ValueID,
                        Case When ISG.SubGroupName in('Collar','Cuff') Then IM.Segment5ValueID Else IM.Segment3ValueID End
                    ),
                    YBM As 
                    (
	                    Select YBC.YBChildID, YBC.YBookingID, YBC.ConsumptionID, YBC.ItemMasterID, YBC.YarnTypeID, YBC.YarnBrandID, ETV.ValueName YarnBrand, 
                        YBC.FUPartID, FUP.PartName, YBC.BookingUnitID, BC.Remarks, ISV1.SegmentValue YarnType, U.DisplayUnitDesc As BookingUOM, YBM.BookingID, 
                        Ceiling(Sum(YBC.BookingQty))BookingQty, 
	                    YBC.FTechnicalName, YBC.IsCompleteReceive, YBC.IsCompleteDelivery, BC.SubGroupID,ISG.SubGroupName,
                        ISV1.SegmentValue Segment1ValueDesc,ISV2.SegmentValue Segment2ValueDesc,ISV3.SegmentValue Segment3ValueDesc,ISV4.SegmentValue Segment4ValueDesc,
						ISV5.SegmentValue Segment5ValueDesc,ISV6.SegmentValue Segment6ValueDesc,ISV7.SegmentValue Segment7ValueDesc,ISV8.SegmentValue Segment8ValueDesc 
                        FROM {TableNames.YarnBookingMaster_New} YBM 
                        Inner JOIN {TableNames.YarnBookingChild_New} YBC On YBC.YBookingID = YBM.YBookingID
                        Inner Join {DbNames.EPYSL}..BOMConsumption BC On BC.ConsumptionID = YBC.ConsumptionID
                        Left Join {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = YBC.YarnBrandID 
                        Left Join {DbNames.EPYSL}..FabricUsedPart FUP On FUP.FUPartID = YBC.FUPartID
                        LEFT JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = YBC.BookingUnitID
                        Left Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = BC.SubGroupID

                        LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YBC.ItemMasterID
						LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
						LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
						LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
						LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
						LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
						LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
						LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
						LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID

	                    Where YBM.YBookingNo = '{yBookingNo}'
                        Group By YBC.YBChildID, YBC.YBookingID, YBC.ConsumptionID, YBC.ItemMasterID, YBC.YarnTypeID, YBC.YarnBrandID, ETV.ValueName, 
                        YBC.FUPartID, FUP.PartName, BC.A1ValueID, ISV1.SegmentValue, BC.Remarks, YBC.BookingUnitID, U.DisplayUnitDesc, YBM.BookingID, YBC.FTechnicalName, YBC.IsCompleteReceive,
                        YBC.IsCompleteDelivery, BC.SubGroupID,ISG.SubGroupName, 
	                    ISV1.SegmentValue,ISV2.SegmentValue,ISV3.SegmentValue,ISV4.SegmentValue,
						ISV5.SegmentValue,ISV6.SegmentValue,ISV7.SegmentValue,ISV8.SegmentValue
                    ) 
                    Select BM.BookingID, Isnull(YBM.YBookingID,0)YBookingID, Isnull(YBM.YBChildID,0)YBChildID,  BM.ConsumptionID, BM.ItemMasterID,
                    BM.YarnTypeID, BM.YarnBrand, BM.FUPartID, BM.BookingUnitID, BM.BookingUOM, BM.BookingQty, YBM.FTechnicalName,
                    BM.IsCompleteReceive, BM.IsCompleteDelivery, BM.LastDCDate, BM.Remarks, 
                    BM.SubGroupID,  BM.SubGroupName, BM.YarnType, BM.YarnBrandID, BM.PartName,  
                    BM.Segment1ValueDesc, BM.Segment2ValueDesc, BM.Segment3ValueDesc, BM.Segment4ValueDesc, BM.Segment5ValueDesc, BM.Segment6ValueDesc,
                    BM.Segment7ValueDesc, BM.Segment8ValueDesc, BM.Segment9ValueDesc, BM.Segment10ValueDesc, BM.Segment11ValueDesc, BM.Segment12ValueDesc,
                    BM.Segment13ValueDesc, BM.Segment14ValueDesc, BM.Segment15ValueDesc, BM.ColorID
                    From BM Left Join YBM On BM.BookingID = YBM.BookingID 
                    And BM.ItemMasterID = YBM.ItemMasterID AND BM.ConsumptionID = YBM.ConsumptionID
                    --Where BM.SubGroupID = 12
                    ; 

                    -- Yarn Booking Child Item
                    ;With YBM As 
                    (
	                    Select * FROM {TableNames.YarnBookingMaster_New} Where YBookingNo = '{yBookingNo}'
                    ),
                    YBCI As ( 
	                    Select YBC.ConsumptionID, YBCI.YBChildItemID, YBCI.YBChildID, YBCI.YBookingID, YBCI.YItemMasterID, YBCI.UnitID, U.DisplayUnitDesc, YBCI.Blending, 
	                    (Case When Blending = 1 then 'Blend' else 'Non-Blend' End)BlendingName, YarnCategory=ISNULL(YBCI.YarnCategory,''), YBCI.Distribution, YBCI.BookingQty, YBCI.Allowance, 
                        YBCI.RequiredQty, YBCI.ShadeCode, Y.ShadeCode as ShadeName,
	                    YBCI.Remarks, YBCI.Specification, YBCI.YD, YBCI.YDItem, YBM.BookingID,
	                    IM.Segment1ValueID Segment1ValueId, IM.Segment2ValueID Segment2ValueId, IM.Segment3ValueID Segment3ValueId, IM.Segment4ValueID Segment4ValueId,
	                    IM.Segment5ValueID Segment5ValueId, IM.Segment6ValueID Segment6ValueId, IM.Segment7ValueID Segment7ValueId, IM.Segment8ValueID Segment8ValueId,
	                    IM.Segment9ValueID Segment9ValueId, IM.Segment10ValueID Segment10ValueId, IM.Segment11ValueID Segment11ValueId, IM.Segment12ValueID Segment12ValueId,
	                    IM.Segment13ValueID Segment13ValueId, IM.Segment14ValueID Segment14ValueId, IM.Segment15ValueID Segment15ValueId,
	                    ISV1.SegmentValue AS Segment1ValueDesc, ISV2.SegmentValue AS Segment2ValueDesc, ISV3.SegmentValue AS Segment3ValueDesc,
	                    ISV4.SegmentValue AS Segment4ValueDesc, ISV5.SegmentValue AS Segment5ValueDesc, ISV6.SegmentValue AS Segment6ValueDesc,
	                    ISV7.SegmentValue AS Segment7ValueDesc, ISV8.SegmentValue AS Segment8ValueDesc, YBM.SubGroupID, ISG.SubGroupName,
                        Isnull(YPM.YDProductionMasterID,0)YDProductionMasterID
	                    From YBM Inner JOIN {TableNames.YarnBookingChild_New} YBC On YBM.YBookingID = YBC.YBookingID
	                    Inner JOIN {TableNames.YarnBookingChildItem_New} YBCI On YBCI.YBChildID = YBC.YBChildID
	                    --Inner Join {DbNames.EPYSL}..BOMConsumption BC On BC.ConsumptionID = YBC.ConsumptionID
	                    Left Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = YBM.SubGroupID
	                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YBCI.YItemMasterID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
	                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID
	                    LEFT JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = YBCI.UnitID
                        LEFT JOIN {TableNames.YARN_SHADE_BOOK} Y ON Y.ShadeCode = YBCI.ShadeCode
                        LEFT JOIN {TableNames.YD_BOOKING_MASTER} YDBM ON YDBM.YBookingID = YBM.YBookingID And YDBM.YBookingID = YBCI.YBookingID
	                    LEFT JOIN {TableNames.YD_PRODUCTION_MASTER} YPM ON YPM.YDBookingMasterID = YDBM.YDBookingMasterID
                    ),
                    BM As 
                    (
	                    Select BM.BookingID, BC.ConsumptionID, BOMCon.SubGroupID,
	                    BookingQty = 
			                    Case When ISG.SubGroupName in('Collar','Cuff') 
				                    Then (0.045 * ((Convert(decimal(18,3), BOMCon.Segment3Desc) * 
				                    Convert(decimal(18,3), BOMCon.Segment4Desc))/420)) * 
				                    (Sum(BC.BookingQty) * U.RelativeFactor) 
			                    Else Sum(BC.BookingQty) End 
	                    From  {DbNames.EPYSL}..BookingChild BC 
	                    Inner Join {DbNames.EPYSL}..BookingMaster BM On BC.BookingID = BM.BookingID
	                    Inner Join {DbNames.EPYSL}..BOMConsumption BOMCon On BOMCon.ConsumptionID = BC.ConsumptionID 
	                    Inner Join {DbNames.EPYSL}..Unit U On U.UnitID = BC.BookingUnitID 
	                    Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = BC.SubGroupID 
	                    Where BM.BookingNo = '{bookingNo}' And BC.ISourcing = 1                                      
	                    Group By BM.BookingID, BC.ConsumptionID, BOMCon.SubGroupID, BOMCon.Segment3Desc, BOMCon.Segment4Desc, U.RelativeFactor, ISG.SubGroupName 
                    )
                    Select YBCI.ConsumptionID, YBCI.YBChildItemID, YBCI.YBChildID, YBCI.YBookingID, YItemMasterID, YBCI.UnitID, YBCI.DisplayUnitDesc, YBCI.Blending, ItemMasterID = YItemMasterID, 
                    YBCI.BlendingName, YarnCategory=ISNULL(YBCI.YarnCategory,''),  
                    ((BM.BookingQty * YBCI.Distribution) / 100)BookingQty, YBCI.Allowance, YBCI.Distribution,   
                    ((BM.BookingQty * YBCI.Distribution) / 100) + ((((BM.BookingQty * YBCI.Distribution) / 100) * YBCI.Allowance) / 100)RequiredQty, 
                    YBCI.ShadeCode, YBCI.ShadeName,
                    YBCI.Remarks, YBCI.Specification, YBCI.YD, YBCI.YDItem, YBCI.BookingID,
                    YBCI.Segment1ValueId, YBCI.Segment2ValueId, YBCI.Segment3ValueId, YBCI.Segment4ValueId,
                    YBCI.Segment5ValueId, YBCI.Segment6ValueId, YBCI.Segment7ValueId, YBCI.Segment8ValueId,
                    YBCI.Segment9ValueId, YBCI.Segment10ValueId, YBCI.Segment11ValueId, YBCI.Segment12ValueId,
                    YBCI.Segment13ValueId, YBCI.Segment14ValueId, YBCI.Segment15ValueId,
                    YBCI.Segment1ValueDesc, YBCI.Segment2ValueDesc, YBCI.Segment3ValueDesc,
                    YBCI.Segment4ValueDesc, YBCI.Segment5ValueDesc, YBCI.Segment6ValueDesc,
                    YBCI.Segment7ValueDesc, YBCI.Segment8ValueDesc, YBCI.SubGroupID, YBCI.SubGroupName, YBCI.YDProductionMasterID  
                    From YBCI Inner Join BM On YBCI.BookingID = BM.BookingID AND BM.ConsumptionID = YBCI.ConsumptionID;

                    --All Yarn Booking Master 
                    Select* FROM {TableNames.YarnBookingMaster_New} Where YBookingNo = '{yBookingNo}';

                    -- Employee Information
                    Select CAST(E.EmployeeCode As varchar) id,E.EmployeeName [text], (D.Designation + ' , ' + ED.DepertmentDescription) [desc]
                    From  {DbNames.EPYSL}..Employee E
                    Inner Join  {DbNames.EPYSL}..EmployeeDepartment ED On ED.DepertmentID = E.DepertmentID
                    Inner Join {DbNames.EPYSL}..EmployeeDesignation D On D.DesigID = E.DesigID
                    Where E.IsRegular = 1 and ED.DepertmentDescription = 'Supply Chain';

                    -- Shade book
                    {CommonQueries.GetYarnShadeBooks()};

                    -- YarnBookingChildGarmentPart
                    With YBK As
                    (
	                    Select YBCYSB.*,YBC.ConsumptionID, YBM.BookingID, YBC.ItemMasterID, FUP.PartName, '1' IsSaveFlag
	                    FROM {TableNames.YarnBookingChildGarmentPart_New} YBCYSB
	                    Inner JOIN {TableNames.YarnBookingChild_New} YBC On YBC.YBChildID = YBCYSB.YBChildID
	                    Inner Join (Select * FROM {TableNames.YarnBookingMaster_New} Where WithoutOB = 0) YBM On YBM.YBookingID = YBC.YBookingID
	                    Inner Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingID = YBM.BookingID
	                    Inner Join {DbNames.EPYSL}..FabricUsedPart FUP On FUP.FUPartID = YBCYSB.FUPartID
	                    Where BM.BookingNo = '{bookingNo}'   
                    ),
                    BK As (
	                    Select BCYSB.*,BC.ConsumptionID, BC.ItemMasterID, FUP.PartName, '0' IsSaveFlag 
	                    From {DbNames.EPYSL}..BookingChildGarmentPart BCYSB
	                    Inner Join {DbNames.EPYSL}..BookingChild BC On BC.BookingChildID = BCYSB.BookingChildID
	                    Inner Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingID = BC.BookingID
	                    Inner Join {DbNames.EPYSL}..FabricUsedPart FUP On FUP.FUPartID = BCYSB.FUPartID
	                    Where BM.BookingNo = '{bookingNo}' 
                    )
                    Select COALESCE(YBK.IsSaveFlag, BK.IsSaveFlag) IsSaveFlag, YBookingCGPID = ISNULL(YBK.YBookingCGPID,0), YBChildID = ISNULL(YBK.YBChildID,0), BK.BookingID, BK.ConsumptionID, BK.ItemMasterID, 
                    FUPartID = ISNULL(YBK.FUPartID,BK.FUPartID), PartName = ISNULL(YBK.PartName,BK.PartName),
                    IM.Segment1ValueID ConstructionID,IM.Segment2ValueID CompositionID,IM.Segment4ValueID GSMID,IM.Segment7ValueID KnittingTypeID
                    From BK
                    LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BK.ItemMasterID
                    Left Join YBK On YBK.ConsumptionID = BK.ConsumptionID And YBK.BookingID = BK.BookingID And YBK.ItemMasterID = BK.ItemMasterID;

                    -- YarnBookingChildYarnSubBrand
                    With YBK As
                    (
	                    Select YBCYSB.*,YBC.ConsumptionID, YBM.BookingID, YBC.ItemMasterID, ETV.ValueName YarnSubBrandName, '1' IsSaveFlag
	                    FROM {TableNames.YarnBookingChildYarnSubBrand_New} YBCYSB
	                    Inner JOIN {TableNames.YarnBookingChild_New} YBC On YBC.YBChildID = YBCYSB.YBChildID
	                    Inner Join (Select * FROM {TableNames.YarnBookingMaster_New} Where WithoutOB = 0) YBM On YBM.YBookingID = YBC.YBookingID
	                    Inner Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingID = YBM.BookingID
	                    Inner Join {DbNames.EPYSL}..EntityTypeValue ETV On ETV.ValueID = YBCYSB.YarnSubBrandID
	                    Where BM.BookingNo = '{bookingNo}'
                    ),
                    BK As (
	                    Select BCYSB.*,BM.BookingID, BC.ConsumptionID, BC.ItemMasterID, ETV.ValueName YarnSubBrandName, '0' IsSaveFlag 
	                    From {DbNames.EPYSL}..BookingChildYarnSubBrand BCYSB
	                    Inner Join {DbNames.EPYSL}..BookingChild BC On BC.BookingChildID = BCYSB.BookingChildID
	                    Inner Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingID = BC.BookingID
	                    Inner Join {DbNames.EPYSL}..EntityTypeValue ETV On ETV.ValueID = BCYSB.YarnSubBrandID
	                    Where BM.BookingNo =  '{bookingNo}'
                    )
                    Select COALESCE(YBK.IsSaveFlag, BK.IsSaveFlag) IsSaveFlag, YBookingCYSubBrandID = ISNULL(YBK.YBookingCYSubBrandID,0), YBChildID = ISNULL(YBK.YBChildID,0), BK.BookingID, BK.ConsumptionID, BK.ItemMasterID, 
                    YarnSubBrandID = ISNULL(YBK.YarnSubBrandID,BK.YarnSubBrandID), YarnSubBrandName = ISNULL(YBK.YarnSubBrandName,BK.YarnSubBrandName),
                    IM.Segment1ValueID ConstructionID,IM.Segment2ValueID CompositionID,IM.Segment4ValueID GSMID,IM.Segment7ValueID KnittingTypeID
                    From BK
                    LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = BK.ItemMasterID
                    Left Join YBK On YBK.ConsumptionID = BK.ConsumptionID And YBK.BookingID = BK.BookingID And YBK.ItemMasterID = BK.ItemMasterID;
                
                    -- YarnBookingChildItemYarnSubBrand
                    Select ETV.ValueID id, ETV.ValueName [text]
                    From {DbNames.EPYSL}..EntityTypeValue ETV
                    Inner Join {DbNames.EPYSL}..EntityType ET On ET.EntityTypeID = ETV.EntityTypeID
                    Where ET.EntityTypeName = 'Yarn Sub Brand'
                    Order By ETV.ValueName; 

                    --YarnBookingChildItemYarnSubBrand
                    Select YBCIB.YBCItemYSubBrandID, YBCIB.YBChildItemID, YBCIB.YarnSubBrandID, ETV.ValueName YarnSubBrandName
                    FROM {TableNames.YarnBookingChildItemYarnSubBrand_New} as YBCIB
                    left join {DbNames.EPYSL}..EntityTypeValue ETV on YBCIB.YarnSubBrandID = ETV.ValueID
                    left JOIN {TableNames.YarnBookingChildItem_New} as YBCI on YBCI.YBChildItemID = YBCIB.YBChildItemID
                    left JOIN {TableNames.YarnBookingChild_New} as YBC on YBC.YBChildID = YBCI.YBChildID
                    left JOIN {TableNames.YarnBookingMaster_New} as YBM on YBM.YBookingID = YBC.YBookingID
                    Where YBM.YBookingNo = '{yBookingNo}'; 

                     -- Cancel Reason List
                    Select ReasonID As id, ReasonName As [text]
                    From {DbNames.EPYSL}..CancelReason
                    Where RefNo in (Select _ID From [dbo].[fnReturnStringArray]('YBK','~₪~')); 

                    -- Get Top Additional Value
                    Select top 1 AdditionalBooking As LastAdditionalBooking FROM {TableNames.YarnBookingMaster_New} 
	                Where YBookingNo Like '%{yBookingNo}%' Order By YBookingID Desc; ";

                if (reasonStatus == "BookingList" || reasonStatus == "AdditionalList")
                {
                    sql += $@"
                        With BAR as
                        (  
	                        Select * from {DbNames.EPYSL}..BookingAdditionalReason Where UseInYBooking = 1 And IsAddition = 1 
                        ),
                        BKRR as
                        ( 
	                        Select YBookingID, ReasonID, YBKReasonID FROM {TableNames.YarnBookingReason_New} Where YBookingID = {yBookingID}
                        )
                        Select Isnull(BKRR.YBKReasonID,0)YBKReasonID, Isnull(BKRR.YBookingID,0)YBookingID, BAR.ReasonID, 
                        BAR.ReasonName, BAR.IsRePurchase, BAR.IsAddition, BAR.IsRevision, BAR.UseInBooking, BAR.Remarks, 
                        Selected = Case When ISNULL(BKRR.ReasonID,0) = 0 then 0 Else 1 End
                        from BAR Left Join BKRR On BKRR.ReasonID = BAR.ReasonID ;";
                }
                else if (reasonStatus == "ProposeList" || reasonStatus == "ReviseList" || reasonStatus == "ExecutedList" || reasonStatus == "ExecutedList")
                {
                    sql += $@"
                        With BAR as
                        (  
	                        Select * from {DbNames.EPYSL}..BookingAdditionalReason Where UseInYBooking = 1 And IsRevision = 1 
                        ),
                        BKRR as
                        ( 
	                        Select YBookingID, ReasonID, YBKReasonID FROM {TableNames.YarnBookingReason_New} Where YBookingID = {yBookingID}
                        )
                        Select Isnull(BKRR.YBKReasonID,0)YBKReasonID, Isnull(BKRR.YBookingID,0)YBookingID, BAR.ReasonID, 
                        BAR.ReasonName, BAR.IsRePurchase, BAR.IsAddition, BAR.IsRevision, BAR.UseInBooking, BAR.Remarks, 
                        Selected = Case When ISNULL(BKRR.ReasonID,0) = 0 then 0 Else 1 End
                        from BAR Left Join BKRR On BKRR.ReasonID = BAR.ReasonID ;";
                }

            }
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                //var records = await _connection.QueryMultipleAsync(sql, segmentNames);
                YarnBookingMaster data = await records.ReadFirstOrDefaultAsync<YarnBookingMaster>();
                Guard.Against.NullObject(data);
                data.F_isSample = isSample;
                data.Childs = records.Read<YarnBookingChild>().ToList();
                List<YarnBookingChildItem> ChildItems = records.Read<YarnBookingChildItem>().ToList();
                data.YarnBookings = records.Read<YarnBookingMaster>().ToList();
                data.ContactPersonList = await records.ReadAsync<Select2OptionModel>();
                data.YarnShadeBooks = await records.ReadAsync<Select2OptionModel>();
                data.yarnBookingChildGarmentPart = records.Read<YarnBookingChildGarmentPart>().ToList();
                data.yarnBookingChildYarnSubBrand = records.Read<YarnBookingChildYarnSubBrand>().ToList();
                data.YarnSubBrandList = records.Read<Select2OptionModel>().ToList();
                data.yarnBookingChildItemYarnSubBrand = records.Read<YarnBookingChildItemYarnSubBrand>().ToList();
                //6-sep-2022-anis
                ////data.FabricComponents = await records.ReadAsync<string>();
                ////var itemSegments = await records.ReadAsync<Select2OptionModel>();
                ////data.YarnSubProgramNews = itemSegments.Where(x => x.desc == ItemSegmentNameConstants.YARN_SUBPROGRAM_NEW);
                ////data.Certifications = itemSegments.Where(x => x.desc == ItemSegmentNameConstants.YARN_CERTIFICATIONS);
                //6-sep-2022-anis
                foreach (YarnBookingChild item in data.Childs)
                {
                    if (item.SubGroupName == "Fabric") data.HasFabric = true;
                    else if (item.SubGroupName == "Collar") data.HasCollar = true;
                    else if (item.SubGroupName == "Cuff") data.HasCuff = true;

                    item.ChildItems = ChildItems.Where(c => c.YBChildID == item.YBChildID).ToList();

                    item.FUPartIDs = data.yarnBookingChildGarmentPart.Where(x => x.BookingID == item.BookingID &&
                        x.ItemMasterID == item.ItemMasterID &&
                        x.ConsumptionID == item.ConsumptionID).Select(x => x.FUPartID).Distinct().ToArray();

                    item.PartName = string.Join(",", data.yarnBookingChildGarmentPart.Where(
                        x => x.BookingID == item.BookingID &&
                        x.ItemMasterID == item.ItemMasterID &&
                        x.ConsumptionID == item.ConsumptionID).Select(x => x.PartName).Distinct());

                    item.YarnSubBrandIDs = data.yarnBookingChildYarnSubBrand.Where(x => x.BookingID == item.BookingID &&
                        x.ItemMasterID == item.ItemMasterID &&
                        x.ConsumptionID == item.ConsumptionID).Select(x => x.YarnSubBrandID).Distinct().ToArray();

                    item.YarnSubBrandID = string.Join(",", data.yarnBookingChildYarnSubBrand.Where(
                        x => x.BookingID == item.BookingID &&
                        x.ItemMasterID == item.ItemMasterID &&
                        x.ConsumptionID == item.ConsumptionID).Select(x => x.YarnSubBrandID).Distinct());

                    item.YarnSubBrandName = string.Join(",", data.yarnBookingChildYarnSubBrand.Where(
                        x => x.BookingID == item.BookingID &&
                        x.ItemMasterID == item.ItemMasterID &&
                        x.ConsumptionID == item.ConsumptionID).Select(x => x.YarnSubBrandName).Distinct());

                    foreach (var itemChildSBrand in item.ChildItems)
                    {
                        itemChildSBrand.YarnSubBrandIDs = data.yarnBookingChildItemYarnSubBrand.Where(
                            x => x.YBChildItemID == itemChildSBrand.YBChildItemID).Select(x => x.YarnSubBrandID).Distinct().ToArray();
                        itemChildSBrand.YarnSubBrandName = string.Join(",", data.yarnBookingChildItemYarnSubBrand.Where(
                            x => x.YBChildItemID == itemChildSBrand.YBChildItemID).Select(x => x.YarnSubBrandName).Distinct());
                        itemChildSBrand.YarnSubBrandID = string.Join(",", data.yarnBookingChildItemYarnSubBrand.Where(
                            x => x.YBChildItemID == itemChildSBrand.YBChildItemID).Select(x => x.YarnSubBrandID).Distinct());
                    }
                }
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open) _connection.Close();
            }
        }
        public async Task<YarnBookingMaster> GetNewAsyncByGroup(bool isSample, string bookingNo, string yBookingNo,
            string reasonStatus, int yBookingID, Status status)
        {
            YarnBookingMaster data = new YarnBookingMaster();
            if (status == Status.Pending)
            {
                data = await GetPendingYarnList(isSample, bookingNo, status);
            }
            else
            {
                data = await GetSaveYarnList(isSample, bookingNo, yBookingNo, reasonStatus, yBookingID, status);
            }

            data = await this.GetGroupByYarnBooking(data, false);

            #region no need
            /*
            int iYBChildID = 1;
            int iYBChildItemId = 1;

            #region YBChild Group Fabric

            #region Group Fabric child
            var gYBChildDataF = from x in data.Childs.Where(m => m.SubGroupId == 1)
                                group x by new
                                {
                                    x.Segment1ValueDesc,
                                    x.Segment2ValueDesc,
                                    x.Segment3ValueDesc,
                                    x.Segment4ValueDesc,
                                    x.Segment5ValueDesc,
                                    x.Segment6ValueDesc,
                                    x.Segment7ValueDesc,
                                    x.FTechnicalName,
                                    x.ItemMasterID,
                                    x.YarnTypeID,
                                    x.YarnType,
                                    x.YarnBrandID,
                                    x.YarnBrand,
                                    x.Remarks,
                                    x.YarnSubBrandID,
                                    x.YarnSubBrandName,
                                    x.SubGroupId,
                                    x.SubGroupName,
                                    x.BookingUnitID,
                                    x.BookingUOM,
                                    x.BookingID
                                }
            into g
                                select new YarnBookingChild()
                                {
                                    Segment1ValueDesc = g.Key.Segment1ValueDesc,
                                    Segment2ValueDesc = g.Key.Segment2ValueDesc,
                                    Segment3ValueDesc = g.Key.Segment3ValueDesc,
                                    Segment4ValueDesc = g.Key.Segment4ValueDesc,
                                    Segment5ValueDesc = g.Key.Segment5ValueDesc,
                                    Segment6ValueDesc = g.Key.Segment6ValueDesc,
                                    Segment7ValueDesc = g.Key.Segment7ValueDesc,
                                    FTechnicalName = g.Key.FTechnicalName,
                                    ItemMasterID = g.Key.ItemMasterID,
                                    YarnTypeID = g.Key.YarnTypeID,
                                    YarnType = g.Key.YarnType,
                                    YarnBrandID = g.Key.YarnBrandID,
                                    YarnBrand = g.Key.YarnBrand,
                                    Remarks = g.Key.Remarks,
                                    YarnSubBrandID = g.Key.YarnSubBrandID,
                                    YarnSubBrandName = g.Key.YarnSubBrandName,
                                    SubGroupId = g.Key.SubGroupId,
                                    SubGroupName = g.Key.SubGroupName,
                                    BookingUnitID = g.Key.BookingUnitID,
                                    BookingID = g.Key.BookingID,
                                    BookingUOM = g.Key.BookingUOM,
                                    BookingQty = g.Sum(xt => xt.BookingQty)
                                };
            #endregion

            #region Set YBChildGroupID in new Group List
            foreach (YarnBookingChild item in gYBChildDataF)
            {
                item.YBChildGroupID = iYBChildID++;   ////set group id

                YarnBookingChild objYBChild = new YarnBookingChild();
                objYBChild.YBChildGroupID = item.YBChildGroupID;
                objYBChild.ItemMasterID = item.ItemMasterID;
                objYBChild.Segment1ValueDesc = item.Segment1ValueDesc;
                objYBChild.Segment2ValueDesc = item.Segment2ValueDesc;
                objYBChild.Segment3ValueDesc = item.Segment3ValueDesc;
                objYBChild.Segment4ValueDesc = item.Segment4ValueDesc;
                objYBChild.Segment5ValueDesc = item.Segment5ValueDesc;
                objYBChild.Segment6ValueDesc = item.Segment6ValueDesc;
                objYBChild.Segment7ValueDesc = item.Segment7ValueDesc;
                objYBChild.FTechnicalName = item.FTechnicalName;
                objYBChild.YarnTypeID = item.YarnTypeID;
                objYBChild.YarnType = item.YarnType;
                objYBChild.YarnBrandID = item.YarnBrandID;
                objYBChild.YarnBrand = item.YarnBrand;
                objYBChild.Remarks = item.Remarks;
                objYBChild.YarnSubBrandID = item.YarnSubBrandID;
                objYBChild.YarnSubBrandName = item.YarnSubBrandName;
                objYBChild.SubGroupId = item.SubGroupId;
                objYBChild.SubGroupName = item.SubGroupName;
                objYBChild.BookingUnitID = item.BookingUnitID;
                objYBChild.BookingID = item.BookingID;
                objYBChild.BookingUOM = item.BookingUOM;
                objYBChild.BookingQty = item.BookingQty;

                #region Create ChildItemGroup
                foreach (YarnBookingChild child in data.Childs.Where(y => y.Segment1ValueDesc == item.Segment1ValueDesc
                        && y.Segment2ValueDesc == item.Segment2ValueDesc
                        && y.Segment3ValueDesc == item.Segment3ValueDesc
                        && y.Segment4ValueDesc == item.Segment4ValueDesc
                        && y.Segment5ValueDesc == item.Segment5ValueDesc
                        && y.Segment6ValueDesc == item.Segment6ValueDesc
                        && y.Segment7ValueDesc == item.Segment7ValueDesc
                        && y.ItemMasterID == item.ItemMasterID
                        && y.FTechnicalName == item.FTechnicalName
                        && y.YarnTypeID == item.YarnTypeID
                        && y.YarnBrandID == item.YarnBrandID
                        && y.Remarks == item.Remarks /// it should be child remarks
                        && y.YarnSubBrandID == item.YarnSubBrandID
                        && y.SubGroupId == item.SubGroupId

                        ))
                {

                    #region this part only for Edit as there are no yarn item for new
                    foreach (YarnBookingChildItem cItems in child.ChildItems.Where(y => y.SubGroupId == child.SubGroupId && y.YBChildID == child.YBChildID))
                    {
                        cItems.YBChildGroupID = item.YBChildGroupID;

                        cItems.YarnSubBrandIDs = data.yarnBookingChildItemYarnSubBrand.Where(x => x.YBChildItemID == cItems.YBChildItemID).Select(x => x.YarnSubBrandID).Distinct().ToArray();
                        cItems.YarnSubBrandName = string.Join(",", data.yarnBookingChildItemYarnSubBrand.Where(x => x.YBChildItemID == cItems.YBChildItemID).Select(x => x.YarnSubBrandName).Distinct());

                    }
                    #endregion

                    #region Group Faric Yarn item (YBChildItem)
                    var gDataFabric = from x in child.ChildItems.Where(m => m.SubGroupId == 1)
                                      group x by new
                                      {
                                          x.YBChildGroupID,
                                          x.YBookingID,
                                          x.YItemMasterID,
                                          x.UnitID,
                                          x.DisplayUnitDesc,
                                          x.Blending,
                                          x.YarnCategory,
                                          x.Allowance,
                                          x.Distribution,
                                          x.ShadeCode,
                                          x.ShadeName,
                                          x.Remarks,
                                          x.Specification,
                                          x.YD,
                                          x.Segment1ValueId,
                                          x.Segment2ValueId,
                                          x.Segment3ValueId,
                                          x.Segment4ValueId,
                                          x.Segment5ValueId,
                                          x.Segment6ValueId,
                                          x.Segment7ValueId,
                                          x.Segment8ValueId,
                                          x.SubGroupId,
                                          x.SubGroupName,

                                          x.YarnSubBrandIDs,
                                          x.YarnSubBrandID,
                                          x.YarnSubBrandName,
                                          x.yarnItemPrice
                                      }
                                into g
                                      select new YarnBookingChildItem()
                                      {
                                          YBChildGroupID = g.Key.YBChildGroupID,
                                          YBookingID = g.Key.YBookingID,
                                          YItemMasterID = g.Key.YItemMasterID,
                                          UnitID = g.Key.UnitID,
                                          DisplayUnitDesc = g.Key.DisplayUnitDesc,
                                          Blending = g.Key.Blending,
                                          YarnCategory = g.Key.YarnCategory,
                                          BookingQty = g.Sum(xt => xt.BookingQty),
                                          Allowance = g.Key.Allowance,
                                          Distribution = g.Key.Distribution,
                                          RequiredQty = g.Sum(xt => xt.RequiredQty),
                                          ShadeCode = g.Key.ShadeCode,
                                          ShadeName = g.Key.ShadeName,
                                          Remarks = g.Key.Remarks,
                                          Specification = g.Key.Specification,
                                          YD = g.Key.YD,
                                          Segment1ValueId = g.Key.Segment1ValueId,
                                          Segment2ValueId = g.Key.Segment2ValueId,
                                          Segment3ValueId = g.Key.Segment3ValueId,
                                          Segment4ValueId = g.Key.Segment4ValueId,
                                          Segment5ValueId = g.Key.Segment5ValueId,
                                          Segment6ValueId = g.Key.Segment6ValueId,
                                          Segment7ValueId = g.Key.Segment7ValueId,
                                          Segment8ValueId = g.Key.Segment8ValueId,
                                          SubGroupId = g.Key.SubGroupId,
                                          SubGroupName = g.Key.SubGroupName,

                                          YarnSubBrandIDs = g.Key.YarnSubBrandIDs,
                                          YarnSubBrandID = g.Key.YarnSubBrandID,
                                          YarnSubBrandName = g.Key.YarnSubBrandName,
                                          yarnItemPrice = g.Key.yarnItemPrice
                                      };
                    #endregion

                    #region Add ChildItem in Child
                    foreach (YarnBookingChildItem citem in gDataFabric.Where(y => y.YBChildGroupID == item.YBChildGroupID && y.SubGroupId == item.SubGroupId))
                    {

                        var objTemp = objYBChild.ChildItems.Find(o =>
                          o.YBChildGroupID == item.YBChildGroupID &&
                          o.YBookingID == item.YBookingID &&
                          o.YItemMasterID == citem.YItemMasterID &&
                          o.UnitID == citem.UnitID &&
                          o.DisplayUnitDesc == citem.DisplayUnitDesc &&
                          o.Blending == citem.Blending &&
                          o.YarnCategory == citem.YarnCategory &&
                          o.ShadeCode == citem.ShadeCode &&
                          o.ShadeName == citem.ShadeName &&
                          o.Specification == citem.Specification &&
                          o.YD == citem.YD &&
                          o.Segment1ValueId == citem.Segment1ValueId &&
                          o.Segment2ValueId == citem.Segment2ValueId &&
                          o.Segment3ValueId == citem.Segment3ValueId &&
                          o.Segment4ValueId == citem.Segment4ValueId &&
                          o.Segment5ValueId == citem.Segment5ValueId &&
                          o.Segment6ValueId == citem.Segment6ValueId &&
                          o.SubGroupId == citem.SubGroupId &&
                          o.SubGroupName == citem.SubGroupName
                      );

                        if (objTemp == null)
                        {
                            var currentChilds = data.Childs.Where(y => y.Segment1ValueDesc == item.Segment1ValueDesc
                                                            && y.Segment2ValueDesc == item.Segment2ValueDesc
                                                            && y.Segment3ValueDesc == item.Segment3ValueDesc
                                                            && y.Segment4ValueDesc == item.Segment4ValueDesc
                                                            && y.Segment5ValueDesc == item.Segment5ValueDesc
                                                            && y.Segment6ValueDesc == item.Segment6ValueDesc
                                                            && y.Segment7ValueDesc == item.Segment7ValueDesc
                                                            && y.ItemMasterID == item.ItemMasterID
                                                            && y.FTechnicalName == item.FTechnicalName
                                                            && y.YarnTypeID == item.YarnTypeID
                                                            && y.YarnBrandID == item.YarnBrandID
                                                            && y.Remarks == item.Remarks /// it should be child remarks
                                                            && y.YarnSubBrandID == item.YarnSubBrandID
                                                            && y.SubGroupId == item.SubGroupId);
                            decimal bookingQty = 0;
                            if (currentChilds != null) bookingQty = currentChilds.Sum(x => x.BookingQty);

                            YarnBookingChildItem objYBChildItems = new YarnBookingChildItem();
                            objYBChildItems.YBChildGroupID = item.YBChildGroupID;
                            objYBChildItems.YBChildItemID = iYBChildItemId++;
                            objYBChildItems.YBookingID = item.YBookingID;
                            objYBChildItems.YItemMasterID = citem.YItemMasterID;
                            objYBChildItems.UnitID = citem.UnitID;
                            objYBChildItems.DisplayUnitDesc = citem.DisplayUnitDesc;
                            objYBChildItems.Blending = citem.Blending;
                            objYBChildItems.YarnCategory = citem.YarnCategory;
                            objYBChildItems.BookingQty = (bookingQty * citem.Distribution) / 100;
                            objYBChildItems.Allowance = citem.Allowance;
                            objYBChildItems.Distribution = citem.Distribution;
                            objYBChildItems.RequiredQty = objYBChildItems.BookingQty + ((((bookingQty * citem.Distribution) / 100) * citem.Allowance) / 100);
                            objYBChildItems.ShadeCode = citem.ShadeCode;
                            objYBChildItems.ShadeName = citem.ShadeName;
                            objYBChildItems.Specification = citem.Specification;
                            objYBChildItems.YD = citem.YD;
                            objYBChildItems.Segment1ValueId = citem.Segment1ValueId;
                            objYBChildItems.Segment2ValueId = citem.Segment2ValueId;
                            objYBChildItems.Segment3ValueId = citem.Segment3ValueId;
                            objYBChildItems.Segment4ValueId = citem.Segment4ValueId;
                            objYBChildItems.Segment5ValueId = citem.Segment5ValueId;
                            objYBChildItems.Segment6ValueId = citem.Segment6ValueId;
                            objYBChildItems.SubGroupId = citem.SubGroupId;
                            objYBChildItems.SubGroupName = citem.SubGroupName;

                            objYBChildItems.YarnSubBrandIDs = citem.YarnSubBrandIDs;
                            objYBChildItems.YarnSubBrandID = citem.YarnSubBrandID;
                            objYBChildItems.YarnSubBrandName = citem.YarnSubBrandName;
                            objYBChildItems.yarnItemPrice = citem.yarnItemPrice;
                            objYBChildItems.Remarks = citem.Remarks;

                            objYBChild.ChildItems.Add(objYBChildItems);
                        }

                    }
                    #endregion
                }
                #endregion

                //// Finally add Child in data for Fabric
                data.ChildsGroup.Add(objYBChild);

            }
            #endregion

            #endregion

            #region YBChild Group Collar / Cuff

            #region Group Collar & Cuff child
            var gYBChildData = from x in data.Childs.Where(m => m.SubGroupId != 1)
                               group x by new
                               {
                                   x.Segment1ValueDesc,
                                   x.Segment2ValueDesc,
                                   x.Segment5ValueDesc,
                                   x.YarnTypeID,
                                   x.YarnType,
                                   x.YarnBrandID,
                                   x.YarnBrand,
                                   x.Remarks,
                                   x.YarnSubBrandID,
                                   x.YarnSubBrandName,
                                   x.SubGroupId,
                                   x.SubGroupName,
                                   x.BookingUnitID,
                                   x.BookingUOM,
                                   x.BookingID
                               }
            into g
                               select new YarnBookingChild()
                               {
                                   Segment1ValueDesc = g.Key.Segment1ValueDesc,
                                   Segment2ValueDesc = g.Key.Segment2ValueDesc,
                                   Segment5ValueDesc = g.Key.Segment5ValueDesc,
                                   YarnTypeID = g.Key.YarnTypeID,
                                   YarnType = g.Key.YarnType,
                                   YarnBrandID = g.Key.YarnBrandID,
                                   YarnBrand = g.Key.YarnBrand,
                                   Remarks = g.Key.Remarks,
                                   YarnSubBrandID = g.Key.YarnSubBrandID,
                                   YarnSubBrandName = g.Key.YarnSubBrandName,
                                   SubGroupId = g.Key.SubGroupId,
                                   SubGroupName = g.Key.SubGroupName,
                                   BookingUnitID = g.Key.BookingUnitID,
                                   BookingID = g.Key.BookingID,
                                   BookingUOM = g.Key.BookingUOM,
                                   BookingQty = g.Sum(xt => xt.BookingQty)
                               };
            #endregion

            #region Set YBChildGroupID in new Group List
            foreach (YarnBookingChild item in gYBChildData)
            {
                item.YBChildGroupID = iYBChildID++;   ////set group id

                YarnBookingChild objYBChild = new YarnBookingChild();
                objYBChild.YBChildGroupID = item.YBChildGroupID;
                objYBChild.Segment1ValueDesc = item.Segment1ValueDesc;
                objYBChild.Segment2ValueDesc = item.Segment2ValueDesc;
                objYBChild.Segment5ValueDesc = item.Segment5ValueDesc;
                objYBChild.YarnTypeID = item.YarnTypeID;
                objYBChild.YarnType = item.YarnType;
                objYBChild.YarnBrandID = item.YarnBrandID;
                objYBChild.YarnBrand = item.YarnBrand;
                objYBChild.Remarks = item.Remarks;
                objYBChild.YarnSubBrandID = item.YarnSubBrandID;
                objYBChild.YarnSubBrandName = item.YarnSubBrandName;
                objYBChild.SubGroupId = item.SubGroupId;
                objYBChild.SubGroupName = item.SubGroupName;
                objYBChild.BookingUnitID = item.BookingUnitID;
                objYBChild.BookingID = item.BookingID;
                objYBChild.BookingUOM = item.BookingUOM;
                objYBChild.BookingQty = item.BookingQty;

                #region Create ChildItemGroup
                foreach (YarnBookingChild child in data.Childs.Where(y => y.Segment1ValueDesc == item.Segment1ValueDesc
                        && y.Segment2ValueDesc == item.Segment2ValueDesc
                        && y.Segment5ValueDesc == item.Segment5ValueDesc
                        && y.YarnTypeID == item.YarnTypeID
                        && y.YarnBrandID == item.YarnBrandID
                        && y.Remarks == item.Remarks /// it should be child remarks
                        && y.YarnSubBrandID == item.YarnSubBrandID
                        && y.SubGroupId == item.SubGroupId))
                {

                    #region this part only for Edit as there are no yarn item for new
                    foreach (YarnBookingChildItem cItems in child.ChildItems.Where(y => y.SubGroupId == child.SubGroupId && y.YBChildID == child.YBChildID))
                    {
                        cItems.YBChildGroupID = item.YBChildGroupID;

                        cItems.YarnSubBrandIDs = data.yarnBookingChildItemYarnSubBrand.Where(x => x.YBChildItemID == cItems.YBChildItemID).Select(x => x.YarnSubBrandID).Distinct().ToArray();
                        cItems.YarnSubBrandName = string.Join(",", data.yarnBookingChildItemYarnSubBrand.Where(x => x.YBChildItemID == cItems.YBChildItemID).Select(x => x.YarnSubBrandName).Distinct());

                    }
                    #endregion

                    #region Group Collar & Cuff Yarn item (YBChildItem)
                    var gDataCollarCuff = from x in child.ChildItems.Where(m => m.SubGroupId == item.SubGroupId && m.YBChildGroupID == item.YBChildGroupID)
                                          group x by new
                                          {
                                              x.YBChildGroupID,
                                              x.YBookingID,
                                              x.YItemMasterID,
                                              x.UnitID,
                                              x.DisplayUnitDesc,
                                              x.Blending,
                                              //x.YarnCategory,
                                              x.Allowance,
                                              x.Distribution,
                                              x.ShadeCode,
                                              x.ShadeName,
                                              x.Specification,
                                              x.YD,
                                              x.Segment1ValueId,
                                              x.Segment2ValueId,
                                              x.Segment3ValueId,
                                              x.Segment4ValueId,
                                              x.Segment5ValueId,
                                              x.Segment6ValueId,
                                              x.SubGroupId,
                                              x.SubGroupName,

                                              x.YarnSubBrandIDs,
                                              x.YarnSubBrandID,
                                              x.YarnSubBrandName,
                                              x.yarnItemPrice
                                          }
                                into g
                                          select new YarnBookingChildItem()
                                          {
                                              YBChildGroupID = g.Key.YBChildGroupID,
                                              YBookingID = g.Key.YBookingID,
                                              YItemMasterID = g.Key.YItemMasterID,
                                              UnitID = g.Key.UnitID,
                                              DisplayUnitDesc = g.Key.DisplayUnitDesc,
                                              Blending = g.Key.Blending,
                                              //YarnCategory = g.Key.YarnCategory,
                                              BookingQty = g.Sum(xt => xt.BookingQty),
                                              Allowance = g.Key.Allowance,
                                              Distribution = g.Key.Distribution,
                                              RequiredQty = g.Sum(xt => xt.RequiredQty),
                                              ShadeCode = g.Key.ShadeCode,
                                              ShadeName = g.Key.ShadeName,
                                              Specification = g.Key.Specification,
                                              YD = g.Key.YD,
                                              Segment1ValueId = g.Key.Segment1ValueId,
                                              Segment2ValueId = g.Key.Segment2ValueId,
                                              Segment3ValueId = g.Key.Segment3ValueId,
                                              Segment4ValueId = g.Key.Segment4ValueId,
                                              Segment5ValueId = g.Key.Segment5ValueId,
                                              Segment6ValueId = g.Key.Segment6ValueId,
                                              SubGroupId = g.Key.SubGroupId,
                                              SubGroupName = g.Key.SubGroupName,

                                              YarnSubBrandIDs = g.Key.YarnSubBrandIDs,
                                              YarnSubBrandID = g.Key.YarnSubBrandID,
                                              YarnSubBrandName = g.Key.YarnSubBrandName,
                                              yarnItemPrice = g.Key.yarnItemPrice
                                          };
                    #endregion


                    #region Add ChildItem in Child
                    foreach (YarnBookingChildItem citem in gDataCollarCuff.Where(y => y.YBChildGroupID == item.YBChildGroupID && y.SubGroupId == item.SubGroupId))
                    {
                        var objTemp = objYBChild.ChildItemsGroup.Find(o =>
                           o.YBChildGroupID == item.YBChildGroupID &&
                           o.YBookingID == citem.YBookingID &&
                           o.YItemMasterID == citem.YItemMasterID &&
                           o.UnitID == citem.UnitID &&
                           o.DisplayUnitDesc == citem.DisplayUnitDesc &&
                           o.Blending == citem.Blending &&
                           o.ShadeCode == citem.ShadeCode &&
                           o.ShadeName == citem.ShadeName &&
                           o.Specification == citem.Specification &&
                           o.YD == citem.YD &&
                           o.Segment1ValueId == citem.Segment1ValueId &&
                           o.Segment2ValueId == citem.Segment2ValueId &&
                           o.Segment3ValueId == citem.Segment3ValueId &&
                           o.Segment4ValueId == citem.Segment4ValueId &&
                           o.Segment5ValueId == citem.Segment5ValueId &&
                           o.Segment6ValueId == citem.Segment6ValueId &&
                           o.SubGroupId == citem.SubGroupId &&
                           o.SubGroupName == citem.SubGroupName
                       );
                        if (objTemp == null)
                        {
                            decimal bookingQty = data.Childs.Where(y => y.Segment1ValueDesc == item.Segment1ValueDesc
                                                && y.Segment2ValueDesc == item.Segment2ValueDesc
                                                && y.Segment5ValueDesc == item.Segment5ValueDesc
                                                && y.YarnTypeID == item.YarnTypeID
                                                && y.YarnBrandID == item.YarnBrandID
                                                && y.Remarks == item.Remarks
                                                && y.YarnSubBrandID == item.YarnSubBrandID
                                                && y.SubGroupId == item.SubGroupId).Sum(b => b.BookingQty);

                            YarnBookingChildItem objYBChildItems = new YarnBookingChildItem();
                            objYBChildItems.YBChildGroupID = item.YBChildGroupID;
                            objYBChildItems.YBChildItemID = iYBChildItemId++;
                            objYBChildItems.YBookingID = citem.YBookingID;
                            objYBChildItems.YItemMasterID = citem.YItemMasterID;
                            objYBChildItems.UnitID = citem.UnitID;
                            objYBChildItems.DisplayUnitDesc = citem.DisplayUnitDesc;
                            objYBChildItems.Blending = citem.Blending;
                            objYBChildItems.YarnCategory = citem.YarnCategory;
                            objYBChildItems.BookingQty = (bookingQty * citem.Distribution) / 100;
                            objYBChildItems.Allowance = citem.Allowance;
                            objYBChildItems.Distribution = citem.Distribution;
                            objYBChildItems.RequiredQty = objYBChildItems.BookingQty + ((((bookingQty * citem.Distribution) / 100) * citem.Allowance) / 100);
                            objYBChildItems.ShadeCode = citem.ShadeCode;
                            objYBChildItems.ShadeName = citem.ShadeName;
                            objYBChildItems.Specification = citem.Specification;
                            objYBChildItems.YD = citem.YD;
                            objYBChildItems.Segment1ValueId = citem.Segment1ValueId;
                            objYBChildItems.Segment2ValueId = citem.Segment2ValueId;
                            objYBChildItems.Segment3ValueId = citem.Segment3ValueId;
                            objYBChildItems.Segment4ValueId = citem.Segment4ValueId;
                            objYBChildItems.Segment5ValueId = citem.Segment5ValueId;
                            objYBChildItems.Segment6ValueId = citem.Segment6ValueId;
                            objYBChildItems.SubGroupId = citem.SubGroupId;
                            objYBChildItems.SubGroupName = citem.SubGroupName;

                            objYBChildItems.YarnSubBrandIDs = citem.YarnSubBrandIDs;
                            objYBChildItems.YarnSubBrandID = citem.YarnSubBrandID;
                            objYBChildItems.YarnSubBrandName = citem.YarnSubBrandName;
                            objYBChildItems.yarnItemPrice = citem.yarnItemPrice;
                            objYBChildItems.Remarks = citem.Remarks;

                            objYBChild.ChildItemsGroup.Add(objYBChildItems);
                        }
                    }
                    #endregion
                }
                #endregion

                //// Finally add Child in data for collar & Cuff
                data.ChildsGroup.Add(objYBChild);

            }
            #endregion

            #endregion

            */
            #endregion No Need

            return data;
        }
        public async Task<YarnBookingMaster> GetAsync(string yBookingNo, bool isSample, string reasonStatus, int yBookingID)
        {
            var segmentNames = new
            {
                SegmentNames = new string[]
                {
                    ItemSegmentNameConstants.YARN_SUBPROGRAM_NEW,
                    ItemSegmentNameConstants.YARN_CERTIFICATIONS
                }
            };

            string sql;

            if (isSample)
            {
                sql = $@"
                -- Yarn Booking Master
                With YBM As 
                (
	                Select top 1 * FROM {TableNames.YarnBookingMaster_New} Where YBookingNo = '{yBookingNo}'
                )
                Select YBM.YBookingID, YBM.YBookingNo, BM.BookingNo, BM.BookingID, ISNULL(BM.ReferenceNo,'') As ReferenceNo, BM.InHouseDate As RequiredDate,
                Isnull(FBA.PreRevisionNo,0)PreProcessRevNo, YBM.RevisionNo, YBM.YBookingDate, YBM.BuyerID, C.ShortName As BuyerName, YBM.BuyerTeamID, 
                CCT.TeamName BuyerDepartment, YBM.CompanyID, CE.CompanyName, YBM.ExportOrderID, EOM.ExportOrderNo, EOM.CalendarDays As TNADays, 
                YBM.YInHouseDate, YBM.YRequiredDate, YBM.ContactPerson, EMP.EmployeeName As MerchandiserName, YBM.SubGroupID, ISG.SubGroupName,
                YBM.WithoutOB, YBM.AdditionalBooking, YBM.IsCancel, YBM.CancelReasonID, YBM.CanceledBy, YBM.DateCanceled 
                From YBM Inner Join {DbNames.EPYSL}..SampleBookingMaster BM On BM.BookingID = YBM.BookingID
                Inner Join {DbNames.EPYSL}..Contacts C On C.ContactID = YBM.BuyerID
                Left Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = YBM.BuyerTeamID
                Left Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = YBM.CompanyID
                Left Join {DbNames.EPYSL}..ExportOrderMaster EOM On EOM.ExportOrderID = YBM.ExportOrderID
                Left Join {DbNames.EPYSL}..Employee EMP On EMP.EmployeeCode = YBM.ContactPerson
                Left Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = YBM.SubGroupID 
                Left JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA On FBA.BookingID = YBM.BookingID 
                Group By YBM.YBookingID, YBM.YBookingNo, BM.BookingNo, BM.BookingID, BM.ReferenceNo, BM.InHouseDate,
                FBA.PreRevisionNo, YBM.RevisionNo, YBM.YBookingDate, YBM.BuyerID, C.ShortName,
                YBM.BuyerTeamID, CCT.TeamName, YBM.CompanyID, CE.CompanyName, YBM.ExportOrderID, EOM.ExportOrderNo, EOM.CalendarDays, 
                YBM.YInHouseDate, YBM.YRequiredDate, YBM.ContactPerson, EMP.EmployeeName, YBM.SubGroupID, ISG.SubGroupName, 
                YBM.WithoutOB, YBM.AdditionalBooking, YBM.IsCancel, YBM.CancelReasonID, YBM.CanceledBy, YBM.DateCanceled;

                -- Yarn Booking Child
                ;With YBM As 
                (
	                Select * FROM {TableNames.YarnBookingMaster_New} Where YBookingNo = '{yBookingNo}'
                )
                Select YBC.YBChildID, YBC.YBookingID, YBC.ConsumptionID, YBC.ItemMasterID, YBC.YarnTypeID, YBC.YarnBrandID, ETV.ValueName YarnBrand, 
                YBC.FUPartID, FUP.PartName, YBC.BookingUnitID, BC.Remarks, BC.A1ValueID As YarnTypeID, ISV1.SegmentValue YarnType, U.DisplayUnitDesc As BookingUOM, YBM.BookingID, 
                Ceiling(Sum(YBC.BookingQty))BookingQty, YBC.FTechnicalName, YBC.IsCompleteReceive, YBC.IsCompleteDelivery, YBM.SubGroupID,ISG.SubGroupName,
                ISV1.SegmentValue Segment1ValueDesc,ISV2.SegmentValue Segment2ValueDesc,ISV3.SegmentValue Segment3ValueDesc,ISV4.SegmentValue Segment4ValueDesc,
                ISV5.SegmentValue Segment5ValueDesc,ISV6.SegmentValue Segment6ValueDesc,ISV7.SegmentValue Segment7ValueDesc,ISV8.SegmentValue Segment8ValueDesc

                From YBM Inner JOIN {TableNames.YarnBookingChild_New} YBC On YBC.YBookingID = YBM.YBookingID
                Left Join {DbNames.EPYSL}..SampleBookingConsumption BC On BC.ConsumptionID = YBC.ConsumptionID
                Left Join {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = YBC.YarnBrandID 
                Left Join {DbNames.EPYSL}..FabricUsedPart FUP On FUP.FUPartID = YBC.FUPartID
                LEFT JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = YBC.BookingUnitID
                Left Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = YBM.SubGroupID
                LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YBC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID
                Group By YBC.YBChildID, YBC.YBookingID, YBC.ConsumptionID, YBC.ItemMasterID, YBC.YarnTypeID, YBC.YarnBrandID, ETV.ValueName, 
                YBC.FUPartID, FUP.PartName, BC.A1ValueID, ISV1.SegmentValue, BC.Remarks, YBC.BookingUnitID, U.DisplayUnitDesc, YBM.BookingID, YBC.FTechnicalName, YBC.IsCompleteReceive,
                YBC.IsCompleteDelivery, YBM.SubGroupID,ISG.SubGroupName,
                ISV1.SegmentValue,ISV2.SegmentValue,ISV3.SegmentValue,ISV4.SegmentValue,
                ISV5.SegmentValue,ISV6.SegmentValue,ISV7.SegmentValue,ISV8.SegmentValue;

                -- Yarn Booking Child Item
                ;With YBM As 
                (
	                Select * FROM {TableNames.YarnBookingMaster_New} Where YBookingNo = '{yBookingNo}'
                )
 
                Select YBCI.YBChildItemID, YBCI.YBChildID, YBCI.YBookingID As BookingID, YBCI.YItemMasterID As ItemMasterID, YBCI.UnitID, U.DisplayUnitDesc, YBCI.Blending, 
                (Case When Blending = 1 then 'Blend' else 'Non-Blend' End)BlendingName, YBCI.YarnCategory, YBCI.Distribution, YBCI.BookingQty, YBCI.Allowance, 
                YBCI.RequiredQty, YBCI.ShadeCode, Y.ShadeCode as ShadeName, YBCI.PhysicalCount,YBCI.StitchLength,YBCI.BatchNo,YBCI.SpinnerID,YBCI.YarnLotNo,
                YBCI.Remarks, YBCI.Specification, YBCI.YD, YBCI.YDItem, YBM.BookingID,
                IM.Segment1ValueID Segment1ValueId, IM.Segment2ValueID Segment2ValueId, IM.Segment3ValueID Segment3ValueId, IM.Segment4ValueID Segment4ValueId,
                IM.Segment5ValueID Segment5ValueId, IM.Segment6ValueID Segment6ValueId, IM.Segment7ValueID Segment7ValueId, IM.Segment8ValueID Segment8ValueId,
                IM.Segment9ValueID Segment9ValueId, IM.Segment10ValueID Segment10ValueId, IM.Segment11ValueID Segment11ValueId, IM.Segment12ValueID Segment12ValueId,
                IM.Segment13ValueID Segment13ValueId, IM.Segment14ValueID Segment14ValueId, IM.Segment15ValueID Segment15ValueId,
                ISV1.SegmentValue AS Segment1ValueDesc, ISV2.SegmentValue AS Segment2ValueDesc, ISV3.SegmentValue AS Segment3ValueDesc,
                ISV4.SegmentValue AS Segment4ValueDesc, ISV5.SegmentValue AS Segment5ValueDesc, ISV6.SegmentValue AS Segment6ValueDesc,
                ISV7.SegmentValue AS Segment7ValueDesc, ISV8.SegmentValue AS Segment8ValueDesc, YBM.SubGroupID, ISG.SubGroupName,
                Isnull(YPM.YDProductionMasterID,0)YDProductionMasterID
                From YBM Inner JOIN {TableNames.YarnBookingChild_New} YBC On YBM.YBookingID = YBC.YBookingID
                Inner JOIN {TableNames.YarnBookingChildItem_New} YBCI On YBCI.YBChildID = YBC.YBChildID
                Left Join {DbNames.EPYSL}..SampleBookingConsumption BC On BC.ConsumptionID = YBC.ConsumptionID
                Left Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = YBM.SubGroupID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YBCI.YItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID
                LEFT JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = YBCI.UnitID
                LEFT JOIN {TableNames.YARN_SHADE_BOOK} Y ON Y.ShadeCode = YBCI.ShadeCode 
                LEFT JOIN {TableNames.YD_BOOKING_MASTER} YDBM ON YDBM.YBookingID = YBM.YBookingID And YDBM.YBookingID = YBCI.YBookingID
                LEFT JOIN {TableNames.YD_PRODUCTION_MASTER} YPM ON YPM.YDBookingMasterID = YDBM.YDBookingMasterID ; 

                -- Contact Person List
                Select E.EmployeeCode As id,E.EmployeeName As [text], (D.Designation + ' , ' + ED.DepertmentDescription) As [desc]
                From  {DbNames.EPYSL}..Employee E
                Inner Join  {DbNames.EPYSL}..EmployeeDepartment ED On ED.DepertmentID = E.DepertmentID
                Inner Join {DbNames.EPYSL}..EmployeeDesignation D On D.DesigID = E.DesigID
                Where E.IsRegular = 1 and ED.DepertmentDescription = 'Supply Chain';

                -- Shade book
                {CommonQueries.GetYarnShadeBooks()};

                -- Yarn Booking Child Garment Part
                With YBK As
                (
	                Select YBCYSB.*,YBC.ConsumptionID, YBM.BookingID, YBC.ItemMasterID, FUP.PartName
	                FROM {TableNames.YarnBookingChildGarmentPart_New} YBCYSB
	                Inner JOIN {TableNames.YarnBookingChild_New} YBC On YBC.YBChildID = YBCYSB.YBChildID
	                Inner Join (Select * FROM {TableNames.YarnBookingMaster_New} Where WithoutOB = 0) YBM On YBM.YBookingID = YBC.YBookingID  
	                Inner Join {DbNames.EPYSL}..FabricUsedPart FUP On FUP.FUPartID = YBCYSB.FUPartID
	                Where YBM.YBookingNo = '{yBookingNo}'
                ) 
                Select YBookingCGPID = ISNULL(YBK.YBookingCGPID,0), YBChildID = ISNULL(YBK.YBChildID,0), YBK.BookingID, YBK.ConsumptionID, YBK.ItemMasterID, 
                YBK.FUPartID, YBK.PartName
                From YBK;
    
                -- YarnBookingChildYarnSubBrand
                With YBK As
                (
	                Select YBCYSB.*,YBC.ConsumptionID, YBM.BookingID, YBC.ItemMasterID, ETV.ValueName YarnSubBrandName
	                FROM {TableNames.YarnBookingChildYarnSubBrand_New} YBCYSB
	                Inner JOIN {TableNames.YarnBookingChild_New} YBC On YBC.YBChildID = YBCYSB.YBChildID
	                Inner Join (Select * FROM {TableNames.YarnBookingMaster_New} Where WithoutOB = 0) YBM On YBM.YBookingID = YBC.YBookingID 
	                Inner Join {DbNames.EPYSL}..EntityTypeValue ETV On ETV.ValueID = YBCYSB.YarnSubBrandID
	                Where YBM.YBookingNo = '{yBookingNo}'
                ) 
                Select YBookingCYSubBrandID = ISNULL(YBK.YBookingCYSubBrandID,0), YBChildID = ISNULL(YBK.YBChildID,0), YBK.BookingID, YBK.ConsumptionID, YBK.ItemMasterID, 
                YarnSubBrandID = ISNULL(YBK.YarnSubBrandID,0), YBK.YarnSubBrandName
                From YBK;
    
                -- YarnBookingChildItemYarnSubBrand - ALL
                Select ETV.ValueID id, ETV.ValueName [text]
                From {DbNames.EPYSL}..EntityTypeValue ETV
                Inner Join {DbNames.EPYSL}..EntityType ET On ET.EntityTypeID = ETV.EntityTypeID
                Where ET.EntityTypeName = 'Yarn Sub Brand'
                Order By ETV.ValueName;

                -- YarnBookingChildItemYarnSubBrand
                Select YBCIB.YBChildItemID, YBCIB.YarnSubBrandID, ETV.ValueName YarnSubBrandName 
                FROM {TableNames.YarnBookingChildItemYarnSubBrand_New} as YBCIB 
                left join {DbNames.EPYSL}..EntityTypeValue ETV on YBCIB.YarnSubBrandID = ETV.ValueID 
                left JOIN {TableNames.YarnBookingChildItem_New} as YBCI on YBCI.YBChildItemID = YBCIB.YBChildItemID
                left JOIN {TableNames.YarnBookingChild_New} as YBC on YBC.YBChildID = YBCI.YBChildID
                left JOIN {TableNames.YarnBookingMaster_New} as YBM on YBM.YBookingID = YBC.YBookingID
                Where YBM.YBookingNo = '{yBookingNo}'

                -- Cancel Reason List
                Select ReasonID As id, ReasonName As [text]
                From {DbNames.EPYSL}..CancelReason
                Where RefNo in (Select _ID From [dbo].[fnReturnStringArray]('YBK','~₪~')); 
                
                -- Get Top Additional Value
                Select top 1 AdditionalBooking As LastAdditionalBooking FROM {TableNames.YarnBookingMaster_New} Where YBookingNo Like '%{yBookingNo}%' Order By YBookingID Desc; ";

                if (reasonStatus == "BookingList" || reasonStatus == "AdditionalList")
                {
                    sql += $@"
                    With BAR as
                    (  
	                    Select * from {DbNames.EPYSL}..BookingAdditionalReason Where UseInYBooking = 1 And IsAddition = 1 
                    ),
                    BKRR as
                    ( 
	                    Select YBookingID, ReasonID, YBKReasonID FROM {TableNames.YarnBookingReason_New} Where YBookingID = {yBookingID}
                    )
                    Select Isnull(BKRR.YBKReasonID,0)YBKReasonID, Isnull(BKRR.YBookingID,0)YBookingID, BAR.ReasonID, 
                    BAR.ReasonName, BAR.IsRePurchase, BAR.IsAddition, BAR.IsRevision, BAR.UseInBooking, BAR.Remarks, 
                    Selected = Case When ISNULL(BKRR.ReasonID,0) = 0 then 0 Else 1 End
                    from BAR Left Join BKRR On BKRR.ReasonID = BAR.ReasonID ";
                }
                else if (reasonStatus == "ProposeList" || reasonStatus == "ReviseList" || reasonStatus == "ExecutedList" || reasonStatus == "ExecutedList")
                {
                    sql += $@"
                    With BAR as
                    (  
	                    Select * from {DbNames.EPYSL}..BookingAdditionalReason Where UseInYBooking = 1 And IsRevision = 1 
                    ),
                    BKRR as
                    ( 
	                    Select YBookingID, ReasonID, YBKReasonID FROM {TableNames.YarnBookingReason_New} Where YBookingID = {yBookingID}
                    )
                    Select Isnull(BKRR.YBKReasonID,0)YBKReasonID, Isnull(BKRR.YBookingID,0)YBookingID, BAR.ReasonID, 
                    BAR.ReasonName, BAR.IsRePurchase, BAR.IsAddition, BAR.IsRevision, BAR.UseInBooking, BAR.Remarks, 
                    Selected = Case When ISNULL(BKRR.ReasonID,0) = 0 then 0 Else 1 End
                    from BAR Left Join BKRR On BKRR.ReasonID = BAR.ReasonID ";
                }
                sql += $@"Select * FROM {TableNames.YarnBookingMaster_New} Where YBookingNo = '{yBookingNo}';
                 ---- Fabric Components
                    {CommonQueries.GetEntityTypeValuesOnly(EntityTypeNameConstants.FABRIC_TYPE)};

                    -- Item Segments
                    {CommonQueries.GetItemSegmentValuesBySegmentNamesWithSegmentName()};
                ";
            }
            else
            {
                sql = $@"
                -- Yarn Booking Master
                With YBM As 
                (
	                Select top 1 * FROM {TableNames.YarnBookingMaster_New} Where YBookingNo = '{yBookingNo}'
                )
                Select YBM.YBookingID, YBM.YBookingNo, BM.BookingNo, BM.BookingID, ISNULL(BM.ReferenceNo,'') As ReferenceNo, BM.InHouseDate As RequiredDate,
                Isnull(FBA.PreRevisionNo,0)PreProcessRevNo, YBM.RevisionNo, YBM.YBookingDate, YBM.BuyerID, C.ShortName As BuyerName, YBM.BuyerTeamID, 
                CCT.TeamName BuyerDepartment, YBM.CompanyID, CE.CompanyName, YBM.ExportOrderID, EOM.ExportOrderNo, EOM.CalendarDays As TNADays, 
                YBM.YInHouseDate, YBM.YRequiredDate, YBM.ContactPerson, EMP.EmployeeName As MerchandiserName, YBM.SubGroupID, ISG.SubGroupName,
                YBM.WithoutOB, YBM.AdditionalBooking, YBM.IsCancel, YBM.CancelReasonID, YBM.CanceledBy, YBM.DateCanceled 
                From YBM Inner Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingID = YBM.BookingID
                Inner Join {DbNames.EPYSL}..Contacts C On C.ContactID = YBM.BuyerID
                Left Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = YBM.BuyerTeamID
                Left Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = YBM.CompanyID
                Left Join {DbNames.EPYSL}..ExportOrderMaster EOM On EOM.ExportOrderID = YBM.ExportOrderID
                Left Join {DbNames.EPYSL}..Employee EMP On EMP.EmployeeCode = YBM.ContactPerson
                Left Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = YBM.SubGroupID 
                Left JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA On FBA.BookingID = YBM.BookingID 
                Group By YBM.YBookingID, YBM.YBookingNo, BM.BookingNo, BM.BookingID, BM.ReferenceNo, BM.InHouseDate,
                FBA.PreRevisionNo, YBM.RevisionNo, YBM.YBookingDate, YBM.BuyerID, C.ShortName,
                YBM.BuyerTeamID, CCT.TeamName, YBM.CompanyID, CE.CompanyName, YBM.ExportOrderID, EOM.ExportOrderNo, EOM.CalendarDays, 
                YBM.YInHouseDate, YBM.YRequiredDate, YBM.ContactPerson, EMP.EmployeeName, YBM.SubGroupID, ISG.SubGroupName, 
                YBM.WithoutOB, YBM.AdditionalBooking, YBM.IsCancel, YBM.CancelReasonID, YBM.CanceledBy, YBM.DateCanceled;

                -- Yarn Booking Child
                ;With YBM As 
                (
	                Select * FROM {TableNames.YarnBookingMaster_New} Where YBookingNo = '{yBookingNo}'
                )
                Select YBC.YBChildID, YBC.YBookingID, YBC.ConsumptionID, YBC.ItemMasterID, YBC.YarnTypeID, YBC.YarnBrandID, ETV.ValueName YarnBrand, 
                YBC.FUPartID, FUP.PartName, YBC.BookingUnitID, BC.Remarks, BC.A1ValueID As YarnTypeID, ISV1.SegmentValue YarnType, U.DisplayUnitDesc As BookingUOM, YBM.BookingID, 
                Ceiling(Sum(YBC.BookingQty))BookingQty, YBC.FTechnicalName, YBC.IsCompleteReceive, YBC.IsCompleteDelivery, YBM.SubGroupID,ISG.SubGroupName,
                BC.Segment1Desc Segment1ValueDesc, BC.Segment2Desc Segment2ValueDesc, BC.Segment3Desc Segment3ValueDesc, BC.Segment4Desc Segment4ValueDesc, 
                BC.Segment5Desc Segment5ValueDesc, BC.Segment6Desc Segment6ValueDesc, BC.Segment7Desc Segment7ValueDesc, BC.Segment8Desc Segment8ValueDesc,
                BC.Segment9Desc Segment9ValueDesc, BC.Segment10Desc Segment10ValueDesc, BC.Segment11Desc Segment11ValueDesc, BC.Segment12Desc Segment12ValueDesc,
                BC.Segment13Desc Segment13ValueDesc, BC.Segment14Desc Segment14ValueDesc, BC.Segment15Desc Segment15ValueDesc 

                From YBM Inner JOIN {TableNames.YarnBookingChild_New} YBC On YBC.YBookingID = YBM.YBookingID
                Left Join {DbNames.EPYSL}..BOMConsumption BC On BC.ConsumptionID = YBC.ConsumptionID
                Left Join {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = YBC.YarnBrandID 
                Left Join {DbNames.EPYSL}..FabricUsedPart FUP On FUP.FUPartID = YBC.FUPartID
                LEFT JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = YBC.BookingUnitID
                Left Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = YBM.SubGroupID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = BC.A1ValueID 
                Group By YBC.YBChildID, YBC.YBookingID, YBC.ConsumptionID, YBC.ItemMasterID, YBC.YarnTypeID, YBC.YarnBrandID, ETV.ValueName, 
                YBC.FUPartID, FUP.PartName, BC.A1ValueID, ISV1.SegmentValue, BC.Remarks, YBC.BookingUnitID, U.DisplayUnitDesc, YBM.BookingID, YBC.FTechnicalName, YBC.IsCompleteReceive,
                YBC.IsCompleteDelivery, YBM.SubGroupID,ISG.SubGroupName, BC.Segment1Desc, BC.Segment2Desc, BC.Segment3Desc, BC.Segment4Desc, 
                BC.Segment5Desc, BC.Segment6Desc, BC.Segment7Desc, BC.Segment8Desc, BC.Segment9Desc, BC.Segment10Desc, BC.Segment11Desc, BC.Segment12Desc,
                BC.Segment13Desc, BC.Segment14Desc, BC.Segment15Desc;

                -- Yarn Booking Child Item
                ;With YBM As 
                (
	                Select * FROM {TableNames.YarnBookingMaster_New} Where YBookingNo = '{yBookingNo}'
                ) 
                Select YBCI.YBChildItemID, YBCI.YBChildID, YBCI.YBookingID As BookingID, YBCI.YItemMasterID As ItemMasterID, YBCI.UnitID, U.DisplayUnitDesc, YBCI.Blending, 
                (Case When Blending = 1 then 'Blend' else 'Non-Blend' End)BlendingName, YBCI.YarnCategory, YBCI.YItemMasterID, YBCI.Distribution, YBCI.BookingQty, YBCI.Allowance, 
                YBCI.RequiredQty, YBCI.ShadeCode, Y.ShadeCode as ShadeName, YBCI.PhysicalCount,YBCI.StitchLength,YBCI.BatchNo,YBCI.SpinnerID,YBCI.YarnLotNo,
                YBCI.Remarks, YBCI.Specification, YBCI.YD, YBCI.YDItem, YBM.BookingID,
                IM.Segment1ValueID Segment1ValueId, IM.Segment2ValueID Segment2ValueId, IM.Segment3ValueID Segment3ValueId, IM.Segment4ValueID Segment4ValueId,
                IM.Segment5ValueID Segment5ValueId, IM.Segment6ValueID Segment6ValueId, IM.Segment7ValueID Segment7ValueId, IM.Segment8ValueID Segment8ValueId,
                IM.Segment9ValueID Segment9ValueId, IM.Segment10ValueID Segment10ValueId, IM.Segment11ValueID Segment11ValueId, IM.Segment12ValueID Segment12ValueId,
                IM.Segment13ValueID Segment13ValueId, IM.Segment14ValueID Segment14ValueId, IM.Segment15ValueID Segment15ValueId,
                ISV1.SegmentValue AS Segment1ValueDesc, ISV2.SegmentValue AS Segment2ValueDesc, ISV3.SegmentValue AS Segment3ValueDesc,
                ISV4.SegmentValue AS Segment4ValueDesc, ISV5.SegmentValue AS Segment5ValueDesc, ISV6.SegmentValue AS Segment6ValueDesc,
                ISV7.SegmentValue AS Segment7ValueDesc, ISV8.SegmentValue AS Segment8ValueDesc, BC.SubGroupID, ISG.SubGroupName,
                Isnull(YPM.YDProductionMasterID,0)YDProductionMasterID
                FROM {TableNames.YarnBookingChildItem_New} YBCI
				Inner JOIN {TableNames.YarnBookingChild_New} YBC On YBC.YBChildID = YBCI.YBChildID
                Inner Join YBM On YBM.YBookingID = YBC.YBookingID
                Inner Join {DbNames.EPYSL}..BOMConsumption BC On BC.ConsumptionID = YBC.ConsumptionID
                Left Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = YBM.SubGroupID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YBCI.YItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID
                LEFT JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = YBCI.UnitID
                LEFT JOIN {TableNames.YARN_SHADE_BOOK} Y ON Y.ShadeCode = YBCI.ShadeCode 
                LEFT JOIN {TableNames.YD_BOOKING_MASTER} YDBM ON YDBM.YBookingID = YBM.YBookingID And YDBM.YBookingID = YBCI.YBookingID
                LEFT JOIN {TableNames.YD_PRODUCTION_MASTER} YPM ON YPM.YDBookingMasterID = YDBM.YDBookingMasterID ;

                -- Contact Person List
                Select E.EmployeeCode As id,E.EmployeeName As [text], (D.Designation + ' , ' + ED.DepertmentDescription) As [desc]
                From  {DbNames.EPYSL}..Employee E
                Inner Join  {DbNames.EPYSL}..EmployeeDepartment ED On ED.DepertmentID = E.DepertmentID
                Inner Join {DbNames.EPYSL}..EmployeeDesignation D On D.DesigID = E.DesigID
                Where E.IsRegular = 1 and ED.DepertmentDescription = 'Supply Chain';

                -- Shade book
                {CommonQueries.GetYarnShadeBooks()};

                -- Yarn Booking Child Garment Part
                With YBK As
                (
	                Select YBCYSB.*,YBC.ConsumptionID, YBM.BookingID, YBC.ItemMasterID, FUP.PartName
	                FROM {TableNames.YarnBookingChildGarmentPart_New} YBCYSB
	                Inner JOIN {TableNames.YarnBookingChild_New} YBC On YBC.YBChildID = YBCYSB.YBChildID
	                Inner Join (Select * FROM {TableNames.YarnBookingMaster_New} Where WithoutOB = 0) YBM On YBM.YBookingID = YBC.YBookingID  
	                Inner Join {DbNames.EPYSL}..FabricUsedPart FUP On FUP.FUPartID = YBCYSB.FUPartID
	                Where YBM.YBookingNo = '{yBookingNo}'
                ) 
                Select YBookingCGPID = ISNULL(YBK.YBookingCGPID,0), YBChildID = ISNULL(YBK.YBChildID,0), YBK.BookingID, YBK.ConsumptionID, YBK.ItemMasterID, 
                YBK.FUPartID, YBK.PartName
                From YBK;
    
                -- YarnBookingChildYarnSubBrand
                With YBK As
                (
	                Select YBCYSB.*,YBC.ConsumptionID, YBM.BookingID, YBC.ItemMasterID, ETV.ValueName YarnSubBrandName
	                FROM {TableNames.YarnBookingChildYarnSubBrand_New} YBCYSB
	                Inner JOIN {TableNames.YarnBookingChild_New} YBC On YBC.YBChildID = YBCYSB.YBChildID
	                Inner Join (Select * FROM {TableNames.YarnBookingMaster_New} Where WithoutOB = 0) YBM On YBM.YBookingID = YBC.YBookingID 
	                Inner Join {DbNames.EPYSL}..EntityTypeValue ETV On ETV.ValueID = YBCYSB.YarnSubBrandID
	                Where YBM.YBookingNo = '{yBookingNo}'
                ) 
                Select YBookingCYSubBrandID = ISNULL(YBK.YBookingCYSubBrandID,0), YBChildID = ISNULL(YBK.YBChildID,0), YBK.BookingID, YBK.ConsumptionID, YBK.ItemMasterID, 
                YarnSubBrandID = ISNULL(YBK.YarnSubBrandID,0), YBK.YarnSubBrandName
                From YBK;
    
                -- YarnBookingChildItemYarnSubBrand - ALL
                Select ETV.ValueID id, ETV.ValueName [text]
                From {DbNames.EPYSL}..EntityTypeValue ETV
                Inner Join {DbNames.EPYSL}..EntityType ET On ET.EntityTypeID = ETV.EntityTypeID
                Where ET.EntityTypeName = 'Yarn Sub Brand'
                Order By ETV.ValueName;

                -- YarnBookingChildItemYarnSubBrand
                Select YBCIB.YBChildItemID, YBCIB.YarnSubBrandID, ETV.ValueName YarnSubBrandName 
                FROM {TableNames.YarnBookingChildItemYarnSubBrand_New} as YBCIB 
                left join {DbNames.EPYSL}..EntityTypeValue ETV on YBCIB.YarnSubBrandID = ETV.ValueID 
                left JOIN {TableNames.YarnBookingChildItem_New} as YBCI on YBCI.YBChildItemID = YBCIB.YBChildItemID
                left JOIN {TableNames.YarnBookingChild_New} as YBC on YBC.YBChildID = YBCI.YBChildID
                left JOIN {TableNames.YarnBookingMaster_New} as YBM on YBM.YBookingID = YBC.YBookingID
                Where YBM.YBookingNo = '{yBookingNo}'

                -- Cancel Reason List
                Select ReasonID As id, ReasonName As [text]
                From {DbNames.EPYSL}..CancelReason
                Where RefNo in (Select _ID From [dbo].[fnReturnStringArray]('YBK','~₪~')); 

                -- Get Top Additional Value
                Select top 1 AdditionalBooking As LastAdditionalBooking FROM {TableNames.YarnBookingMaster_New} Where YBookingNo Like '%{yBookingNo}%' Order By YBookingID Desc; ";

                if (reasonStatus == "BookingList" || reasonStatus == "AdditionalList")
                {
                    sql += $@"
                    With BAR as
                    (  
	                    Select * from {DbNames.EPYSL}..BookingAdditionalReason Where UseInYBooking = 1 And IsAddition = 1 
                    ),
                    BKRR as
                    ( 
	                    Select YBookingID, ReasonID, YBKReasonID FROM {TableNames.YarnBookingReason_New} Where YBookingID = {yBookingID}
                    )
                    Select Isnull(BKRR.YBKReasonID,0)YBKReasonID, Isnull(BKRR.YBookingID,0)YBookingID, BAR.ReasonID, 
                    BAR.ReasonName, BAR.IsRePurchase, BAR.IsAddition, BAR.IsRevision, BAR.UseInBooking, BAR.Remarks, 
                    Selected = Case When ISNULL(BKRR.ReasonID,0) = 0 then 0 Else 1 End
                    from BAR Left Join BKRR On BKRR.ReasonID = BAR.ReasonID ";
                }
                else if (reasonStatus == "ProposeList" || reasonStatus == "ReviseList" || reasonStatus == "ExecutedList" || reasonStatus == "ExecutedList")
                {
                    sql += $@"
                    With BAR as
                    (  
	                    Select * from {DbNames.EPYSL}..BookingAdditionalReason Where UseInYBooking = 1 And IsRevision = 1 
                    ),
                    BKRR as
                    ( 
	                    Select YBookingID, ReasonID, YBKReasonID FROM {TableNames.YarnBookingReason_New} Where YBookingID = {yBookingID}
                    )
                    Select Isnull(BKRR.YBKReasonID,0)YBKReasonID, Isnull(BKRR.YBookingID,0)YBookingID, BAR.ReasonID, 
                    BAR.ReasonName, BAR.IsRePurchase, BAR.IsAddition, BAR.IsRevision, BAR.UseInBooking, BAR.Remarks, 
                    Selected = Case When ISNULL(BKRR.ReasonID,0) = 0 then 0 Else 1 End
                    from BAR Left Join BKRR On BKRR.ReasonID = BAR.ReasonID ";
                }

                sql += $@"Select * FROM {TableNames.YarnBookingMaster_New} Where YBookingNo = '{yBookingNo}';

                 ---- Fabric Components
                {CommonQueries.GetEntityTypeValuesOnly(EntityTypeNameConstants.FABRIC_TYPE)};

                -- Item Segments
                {CommonQueries.GetItemSegmentValuesBySegmentNamesWithSegmentName()};

                --Spinner
                {CommonQueries.GetYarnSpinners()}
                ";
            }
            try
            {
                await _connection.OpenAsync();
                //var records = await _connection.QueryMultipleAsync(sql);
                var records = await _connection.QueryMultipleAsync(sql, segmentNames);
                YarnBookingMaster data = await records.ReadFirstOrDefaultAsync<YarnBookingMaster>();
                Guard.Against.NullObject(data);
                data.Childs = records.Read<YarnBookingChild>().ToList();
                List<YarnBookingChildItem> ChildItems = records.Read<YarnBookingChildItem>().ToList();
                data.ContactPersonList = await records.ReadAsync<Select2OptionModel>();
                data.YarnShadeBooks = await records.ReadAsync<Select2OptionModel>();
                data.yarnBookingChildGarmentPart = records.Read<YarnBookingChildGarmentPart>().ToList();
                data.yarnBookingChildYarnSubBrand = records.Read<YarnBookingChildYarnSubBrand>().ToList();
                data.YarnSubBrandList = records.Read<Select2OptionModel>().ToList();
                data.yarnBookingChildItemYarnSubBrand = records.Read<YarnBookingChildItemYarnSubBrand>().ToList();

                foreach (YarnBookingChild item in data.Childs)
                {
                    if (item.SubGroupName == "Fabric") data.HasFabric = true;
                    else if (item.SubGroupName == "Collar") data.HasCollar = true;
                    else if (item.SubGroupName == "Cuff") data.HasCuff = true;

                    item.ChildItems = ChildItems.Where(c => c.YBChildID == item.YBChildID).ToList();

                    item.FUPartIDs = data.yarnBookingChildGarmentPart.Where(x => x.BookingID == item.BookingID &&
                        x.ItemMasterID == item.ItemMasterID &&
                        x.ConsumptionID == item.ConsumptionID).Select(x => x.FUPartID).Distinct().ToArray();

                    item.PartName = string.Join(",", data.yarnBookingChildGarmentPart.Where(
                        x => x.BookingID == item.BookingID &&
                        x.ItemMasterID == item.ItemMasterID &&
                        x.ConsumptionID == item.ConsumptionID).Select(x => x.PartName).Distinct());

                    item.YarnSubBrandIDs = data.yarnBookingChildYarnSubBrand.Where(x => x.BookingID == item.BookingID &&
                        x.ItemMasterID == item.ItemMasterID &&
                        x.ConsumptionID == item.ConsumptionID).Select(x => x.YarnSubBrandID).Distinct().ToArray();

                    item.YarnSubBrandName = string.Join(",", data.yarnBookingChildYarnSubBrand.Where(
                        x => x.BookingID == item.BookingID &&
                        x.ItemMasterID == item.ItemMasterID &&
                        x.ConsumptionID == item.ConsumptionID).Select(x => x.YarnSubBrandName).Distinct());

                    foreach (var itemChildSBrand in item.ChildItems)
                    {
                        itemChildSBrand.YarnSubBrandIDs = data.yarnBookingChildItemYarnSubBrand.Where(
                            x => x.YBChildItemID == itemChildSBrand.YBChildItemID).Select(x => x.YarnSubBrandID).Distinct().ToArray();

                        itemChildSBrand.YarnSubBrandName = string.Join(",", data.yarnBookingChildItemYarnSubBrand.Where(
                            x => x.YBChildItemID == itemChildSBrand.YBChildItemID).Select(x => x.YarnSubBrandName).Distinct());
                    }
                    item.ChildItemsGroup = item.ChildItems;
                }
                data.CancelReasonList = await records.ReadAsync<Select2OptionModel>();

                //Get Last Additional Booking
                LastAdditionalBookingList LAddBookingList = await records.ReadFirstOrDefaultAsync<LastAdditionalBookingList>();
                data.LastAdditionalBooking = LAddBookingList.LastAdditionalBooking;

                //reason
                if (reasonStatus != "Others")
                {
                    data.yarnBookingReason = records.Read<YarnBookingReason>().ToList();
                }
                data.YarnBookings = records.Read<YarnBookingMaster>().ToList();
                //05-Sep-2022-anis
                data.FabricComponents = await records.ReadAsync<string>();
                var itemSegments = await records.ReadAsync<Select2OptionModelExtended>();
                data.YarnSubProgramNews = itemSegments.Where(x => x.desc == ItemSegmentNameConstants.YARN_SUBPROGRAM_NEW);
                data.Certifications = itemSegments.Where(x => x.desc == ItemSegmentNameConstants.YARN_CERTIFICATIONS);

                data.Spinners = records.Read<Select2OptionModel>().ToList();

                //05-sep-2022-anis
                data = await this.GetGroupByYarnBooking(data, true);

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open) _connection.Close();
            }
        }
        private async Task<YarnBookingMaster> GetGroupByYarnBooking(YarnBookingMaster data, bool isReadMode)
        {
            int iYBChildID = 1;
            int iYBChildItemId = 1;

            #region YBChild Group Fabric

            #region Group Fabric child
            var gYBChildDataF = from x in data.Childs.Where(m => m.SubGroupId == 1)
                                group x by new
                                {
                                    x.Segment1ValueDesc,
                                    x.Segment2ValueDesc,
                                    x.Segment3ValueDesc,
                                    x.Segment4ValueDesc,
                                    x.Segment5ValueDesc,
                                    x.Segment6ValueDesc,
                                    x.Segment7ValueDesc,
                                    x.FTechnicalName,
                                    x.ItemMasterID,
                                    x.YarnTypeID,
                                    x.YarnType,
                                    x.YarnBrandID,
                                    x.YarnBrand,
                                    x.Remarks,
                                    x.YarnSubBrandID,
                                    x.YarnSubBrandName,
                                    x.SubGroupId,
                                    x.SubGroupName,
                                    x.BookingUnitID,
                                    x.BookingUOM,
                                    x.BookingID,
                                    x.ConsumptionID
                                }
            into g
                                select new YarnBookingChild()
                                {
                                    Segment1ValueDesc = g.Key.Segment1ValueDesc,
                                    Segment2ValueDesc = g.Key.Segment2ValueDesc,
                                    Segment3ValueDesc = g.Key.Segment3ValueDesc,
                                    Segment4ValueDesc = g.Key.Segment4ValueDesc,
                                    Segment5ValueDesc = g.Key.Segment5ValueDesc,
                                    Segment6ValueDesc = g.Key.Segment6ValueDesc,
                                    Segment7ValueDesc = g.Key.Segment7ValueDesc,
                                    FTechnicalName = g.Key.FTechnicalName,
                                    ItemMasterID = g.Key.ItemMasterID,
                                    YarnTypeID = g.Key.YarnTypeID,
                                    YarnType = g.Key.YarnType,
                                    YarnBrandID = g.Key.YarnBrandID,
                                    YarnBrand = g.Key.YarnBrand,
                                    Remarks = g.Key.Remarks,
                                    YarnSubBrandID = g.Key.YarnSubBrandID,
                                    YarnSubBrandName = g.Key.YarnSubBrandName,
                                    SubGroupId = g.Key.SubGroupId,
                                    SubGroupName = g.Key.SubGroupName,
                                    BookingUnitID = g.Key.BookingUnitID,
                                    BookingID = g.Key.BookingID,
                                    BookingUOM = g.Key.BookingUOM,
                                    BookingQty = g.Sum(xt => xt.BookingQty),
                                    ConsumptionID = g.Key.ConsumptionID
                                };
            #endregion

            #region Set YBChildGroupID in new Group List
            foreach (YarnBookingChild item in gYBChildDataF)
            {
                item.YBChildGroupID = iYBChildID++;   ////set group id

                YarnBookingChild objYBChild = new YarnBookingChild();
                objYBChild.YBChildGroupID = item.YBChildGroupID;
                objYBChild.ItemMasterID = item.ItemMasterID;
                objYBChild.Segment1ValueDesc = item.Segment1ValueDesc;
                objYBChild.Segment2ValueDesc = item.Segment2ValueDesc;
                objYBChild.Segment3ValueDesc = item.Segment3ValueDesc;
                objYBChild.Segment4ValueDesc = item.Segment4ValueDesc;
                objYBChild.Segment5ValueDesc = item.Segment5ValueDesc;
                objYBChild.Segment6ValueDesc = item.Segment6ValueDesc;
                objYBChild.Segment7ValueDesc = item.Segment7ValueDesc;
                objYBChild.FTechnicalName = item.FTechnicalName;
                objYBChild.YarnTypeID = item.YarnTypeID;
                objYBChild.YarnType = item.YarnType;
                objYBChild.YarnBrandID = item.YarnBrandID;
                objYBChild.YarnBrand = item.YarnBrand;
                objYBChild.Remarks = item.Remarks;
                objYBChild.YarnSubBrandID = item.YarnSubBrandID;
                objYBChild.YarnSubBrandName = item.YarnSubBrandName;
                objYBChild.SubGroupId = item.SubGroupId;
                objYBChild.SubGroupName = item.SubGroupName;
                objYBChild.BookingUnitID = item.BookingUnitID;
                objYBChild.BookingID = item.BookingID;
                objYBChild.BookingUOM = item.BookingUOM;
                objYBChild.BookingQty = item.BookingQty;
                if (item.SubGroupId == 1)
                {
                    objYBChild.ConsumptionID = item.ConsumptionID;
                }

                #region Create ChildItemGroup
                foreach (YarnBookingChild child in data.Childs.Where(y => y.Segment1ValueDesc == item.Segment1ValueDesc
                        && y.Segment2ValueDesc == item.Segment2ValueDesc
                        && y.Segment3ValueDesc == item.Segment3ValueDesc
                        && y.Segment4ValueDesc == item.Segment4ValueDesc
                        && y.Segment5ValueDesc == item.Segment5ValueDesc
                        && y.Segment6ValueDesc == item.Segment6ValueDesc
                        && y.Segment7ValueDesc == item.Segment7ValueDesc
                        && y.ItemMasterID == item.ItemMasterID
                        && y.FTechnicalName == item.FTechnicalName
                        && y.YarnTypeID == item.YarnTypeID
                        && y.YarnBrandID == item.YarnBrandID
                        && y.Remarks == item.Remarks /// it should be child remarks
                        && y.YarnSubBrandID == item.YarnSubBrandID
                        && y.SubGroupId == item.SubGroupId

                        ))
                {

                    #region this part only for Edit as there are no yarn item for new
                    foreach (YarnBookingChildItem cItems in child.ChildItems.Where(y => y.SubGroupId == child.SubGroupId && y.YBChildID == child.YBChildID))
                    {
                        cItems.YBChildGroupID = item.YBChildGroupID;

                        cItems.YarnSubBrandIDs = data.yarnBookingChildItemYarnSubBrand.Where(x => x.YBChildItemID == cItems.YBChildItemID).Select(x => x.YarnSubBrandID).Distinct().ToArray();
                        cItems.YarnSubBrandName = string.Join(",", data.yarnBookingChildItemYarnSubBrand.Where(x => x.YBChildItemID == cItems.YBChildItemID).Select(x => x.YarnSubBrandName).Distinct());

                    }
                    #endregion

                    #region Group Faric Yarn item (YBChildItem)
                    var gDataFabric = from x in child.ChildItems.Where(m => m.SubGroupId == 1)
                                      group x by new
                                      {
                                          x.YBChildGroupID,
                                          x.YBookingID,
                                          x.YItemMasterID,
                                          x.UnitID,
                                          x.DisplayUnitDesc,
                                          x.Blending,
                                          x.YarnCategory,
                                          x.Allowance,
                                          x.Distribution,
                                          x.ShadeCode,
                                          x.ShadeName,
                                          x.Remarks,
                                          x.Specification,
                                          x.YDItem,
                                          x.YD,
                                          x.Segment1ValueId,
                                          x.Segment2ValueId,
                                          x.Segment3ValueId,
                                          x.Segment4ValueId,
                                          x.Segment5ValueId,
                                          x.Segment6ValueId,
                                          x.Segment7ValueId,
                                          x.Segment8ValueId,
                                          x.SubGroupId,
                                          x.SubGroupName,

                                          x.YarnSubBrandIDs,
                                          x.YarnSubBrandID,
                                          x.YarnSubBrandName,
                                          x.yarnItemPrice
                                      }
                                into g
                                      select new YarnBookingChildItem()
                                      {
                                          YBChildGroupID = g.Key.YBChildGroupID,
                                          YBookingID = g.Key.YBookingID,
                                          YItemMasterID = g.Key.YItemMasterID,
                                          UnitID = g.Key.UnitID,
                                          DisplayUnitDesc = g.Key.DisplayUnitDesc,
                                          Blending = g.Key.Blending,
                                          YarnCategory = g.Key.YarnCategory,
                                          BookingQty = g.Sum(xt => xt.BookingQty),
                                          Allowance = g.Key.Allowance,
                                          Distribution = g.Key.Distribution,
                                          RequiredQty = g.Sum(xt => xt.RequiredQty),
                                          ShadeCode = g.Key.ShadeCode,
                                          ShadeName = g.Key.ShadeName,
                                          Remarks = g.Key.Remarks,
                                          Specification = g.Key.Specification,
                                          YDItem = g.Key.YDItem,
                                          YD = g.Key.YD,
                                          Segment1ValueId = g.Key.Segment1ValueId,
                                          Segment2ValueId = g.Key.Segment2ValueId,
                                          Segment3ValueId = g.Key.Segment3ValueId,
                                          Segment4ValueId = g.Key.Segment4ValueId,
                                          Segment5ValueId = g.Key.Segment5ValueId,
                                          Segment6ValueId = g.Key.Segment6ValueId,
                                          Segment7ValueId = g.Key.Segment7ValueId,
                                          Segment8ValueId = g.Key.Segment8ValueId,
                                          SubGroupId = g.Key.SubGroupId,
                                          SubGroupName = g.Key.SubGroupName,

                                          YarnSubBrandIDs = g.Key.YarnSubBrandIDs,
                                          YarnSubBrandID = g.Key.YarnSubBrandID,
                                          YarnSubBrandName = g.Key.YarnSubBrandName,
                                          yarnItemPrice = g.Key.yarnItemPrice
                                      };
                    #endregion

                    #region Add ChildItem in Child
                    foreach (YarnBookingChildItem citem in gDataFabric.Where(y => y.YBChildGroupID == item.YBChildGroupID && y.SubGroupId == item.SubGroupId))
                    {

                        var objTemp = objYBChild.ChildItems.Find(o =>
                          o.YBChildGroupID == item.YBChildGroupID &&
                          o.YBookingID == item.YBookingID &&
                          o.YItemMasterID == citem.YItemMasterID &&
                          o.UnitID == citem.UnitID &&
                          o.DisplayUnitDesc == citem.DisplayUnitDesc &&
                          o.Blending == citem.Blending &&
                          o.YarnCategory == citem.YarnCategory &&
                          o.ShadeCode == citem.ShadeCode &&
                          o.ShadeName == citem.ShadeName &&
                          o.Specification == citem.Specification &&
                          o.YDItem == citem.YD &&
                          o.YD == citem.YD &&
                          o.Segment1ValueId == citem.Segment1ValueId &&
                          o.Segment2ValueId == citem.Segment2ValueId &&
                          o.Segment3ValueId == citem.Segment3ValueId &&
                          o.Segment4ValueId == citem.Segment4ValueId &&
                          o.Segment5ValueId == citem.Segment5ValueId &&
                          o.Segment6ValueId == citem.Segment6ValueId &&
                          o.SubGroupId == citem.SubGroupId &&
                          o.SubGroupName == citem.SubGroupName
                      );

                        if (objTemp == null)
                        {
                            var currentChilds = data.Childs.Where(y => y.Segment1ValueDesc == item.Segment1ValueDesc
                                                            && y.Segment2ValueDesc == item.Segment2ValueDesc
                                                            && y.Segment3ValueDesc == item.Segment3ValueDesc
                                                            && y.Segment4ValueDesc == item.Segment4ValueDesc
                                                            && y.Segment5ValueDesc == item.Segment5ValueDesc
                                                            && y.Segment6ValueDesc == item.Segment6ValueDesc
                                                            && y.Segment7ValueDesc == item.Segment7ValueDesc
                                                            && y.ItemMasterID == item.ItemMasterID
                                                            && y.FTechnicalName == item.FTechnicalName
                                                            && y.YarnTypeID == item.YarnTypeID
                                                            && y.YarnBrandID == item.YarnBrandID
                                                            && y.Remarks == item.Remarks /// it should be child remarks
                                                            && y.YarnSubBrandID == item.YarnSubBrandID
                                                            && y.SubGroupId == item.SubGroupId);
                            decimal bookingQty = 0;
                            if (currentChilds != null) bookingQty = currentChilds.Sum(x => x.BookingQty);

                            YarnBookingChildItem objYBChildItems = new YarnBookingChildItem();
                            objYBChildItems.YBChildGroupID = item.YBChildGroupID;
                            objYBChildItems.YBChildItemID = iYBChildItemId++;
                            objYBChildItems.YBookingID = item.YBookingID;
                            objYBChildItems.YItemMasterID = citem.YItemMasterID;
                            objYBChildItems.UnitID = citem.UnitID;
                            objYBChildItems.DisplayUnitDesc = citem.DisplayUnitDesc;
                            objYBChildItems.Blending = citem.Blending;
                            objYBChildItems.YarnCategory = citem.YarnCategory;
                            objYBChildItems.BookingQty = (bookingQty * citem.Distribution) / 100;
                            objYBChildItems.Allowance = citem.Allowance;
                            objYBChildItems.Distribution = citem.Distribution;
                            objYBChildItems.RequiredQty = isReadMode ? citem.RequiredQty : objYBChildItems.BookingQty + ((((bookingQty * citem.Distribution) / 100) * citem.Allowance) / 100);
                            objYBChildItems.ShadeCode = citem.ShadeCode;
                            objYBChildItems.ShadeName = citem.ShadeName;
                            objYBChildItems.Specification = citem.Specification;
                            objYBChildItems.YDItem = citem.YDItem;
                            objYBChildItems.YD = citem.YD;
                            objYBChildItems.Segment1ValueId = citem.Segment1ValueId;
                            objYBChildItems.Segment2ValueId = citem.Segment2ValueId;
                            objYBChildItems.Segment3ValueId = citem.Segment3ValueId;
                            objYBChildItems.Segment4ValueId = citem.Segment4ValueId;
                            objYBChildItems.Segment5ValueId = citem.Segment5ValueId;
                            objYBChildItems.Segment6ValueId = citem.Segment6ValueId;
                            objYBChildItems.SubGroupId = citem.SubGroupId;
                            objYBChildItems.SubGroupName = citem.SubGroupName;

                            objYBChildItems.YarnSubBrandIDs = citem.YarnSubBrandIDs;
                            objYBChildItems.YarnSubBrandID = citem.YarnSubBrandID;
                            objYBChildItems.YarnSubBrandName = citem.YarnSubBrandName;
                            objYBChildItems.yarnItemPrice = citem.yarnItemPrice;
                            objYBChildItems.Remarks = citem.Remarks;

                            objYBChild.ChildItems.Add(objYBChildItems);
                        }

                    }
                    #endregion
                }
                #endregion

                //// Finally add Child in data for Fabric
                data.ChildsGroup.Add(objYBChild);

            }
            #endregion

            #endregion

            #region YBChild Group Collar / Cuff

            #region Group Collar & Cuff child
            var gYBChildData = from x in data.Childs.Where(m => m.SubGroupId != 1)
                               group x by new
                               {
                                   x.Segment1ValueDesc,
                                   x.Segment2ValueDesc,
                                   x.Segment5ValueDesc,
                                   x.YarnTypeID,
                                   x.YarnType,
                                   x.YarnBrandID,
                                   x.YarnBrand,
                                   x.Remarks,
                                   x.YarnSubBrandID,
                                   x.YarnSubBrandName,
                                   x.SubGroupId,
                                   x.SubGroupName,
                                   x.BookingUnitID,
                                   x.BookingUOM,
                                   x.BookingID
                               }
            into g
                               select new YarnBookingChild()
                               {
                                   Segment1ValueDesc = g.Key.Segment1ValueDesc,
                                   Segment2ValueDesc = g.Key.Segment2ValueDesc,
                                   Segment5ValueDesc = g.Key.Segment5ValueDesc,
                                   YarnTypeID = g.Key.YarnTypeID,
                                   YarnType = g.Key.YarnType,
                                   YarnBrandID = g.Key.YarnBrandID,
                                   YarnBrand = g.Key.YarnBrand,
                                   Remarks = g.Key.Remarks,
                                   YarnSubBrandID = g.Key.YarnSubBrandID,
                                   YarnSubBrandName = g.Key.YarnSubBrandName,
                                   SubGroupId = g.Key.SubGroupId,
                                   SubGroupName = g.Key.SubGroupName,
                                   BookingUnitID = g.Key.BookingUnitID,
                                   BookingID = g.Key.BookingID,
                                   BookingUOM = g.Key.BookingUOM,
                                   BookingQty = g.Sum(xt => xt.BookingQty)
                               };
            #endregion

            #region Set YBChildGroupID in new Group List
            foreach (YarnBookingChild item in gYBChildData)
            {
                item.YBChildGroupID = iYBChildID++;   ////set group id

                YarnBookingChild objYBChild = new YarnBookingChild();
                objYBChild.YBChildGroupID = item.YBChildGroupID;
                objYBChild.Segment1ValueDesc = item.Segment1ValueDesc;
                objYBChild.Segment2ValueDesc = item.Segment2ValueDesc;
                objYBChild.Segment5ValueDesc = item.Segment5ValueDesc;
                objYBChild.YarnTypeID = item.YarnTypeID;
                objYBChild.YarnType = item.YarnType;
                objYBChild.YarnBrandID = item.YarnBrandID;
                objYBChild.YarnBrand = item.YarnBrand;
                objYBChild.Remarks = item.Remarks;
                objYBChild.YarnSubBrandID = item.YarnSubBrandID;
                objYBChild.YarnSubBrandName = item.YarnSubBrandName;
                objYBChild.SubGroupId = item.SubGroupId;
                objYBChild.SubGroupName = item.SubGroupName;
                objYBChild.BookingUnitID = item.BookingUnitID;
                objYBChild.BookingID = item.BookingID;
                objYBChild.BookingUOM = item.BookingUOM;
                objYBChild.BookingQty = item.BookingQty;
                if (item.SubGroupId == 1)
                {
                    objYBChild.ConsumptionID = item.ConsumptionID;
                }

                #region Create ChildItemGroup
                foreach (YarnBookingChild child in data.Childs.Where(y => y.Segment1ValueDesc == item.Segment1ValueDesc
                        && y.Segment2ValueDesc == item.Segment2ValueDesc
                        && y.Segment5ValueDesc == item.Segment5ValueDesc
                        && y.YarnTypeID == item.YarnTypeID
                        && y.YarnBrandID == item.YarnBrandID
                        && y.Remarks == item.Remarks /// it should be child remarks
                        && y.YarnSubBrandID == item.YarnSubBrandID
                        && y.SubGroupId == item.SubGroupId))
                {

                    #region this part only for Edit as there are no yarn item for new
                    foreach (YarnBookingChildItem cItems in child.ChildItems.Where(y => y.SubGroupId == child.SubGroupId && y.YBChildID == child.YBChildID))
                    {
                        cItems.YBChildGroupID = item.YBChildGroupID;

                        cItems.YarnSubBrandIDs = data.yarnBookingChildItemYarnSubBrand.Where(x => x.YBChildItemID == cItems.YBChildItemID).Select(x => x.YarnSubBrandID).Distinct().ToArray();
                        cItems.YarnSubBrandName = string.Join(",", data.yarnBookingChildItemYarnSubBrand.Where(x => x.YBChildItemID == cItems.YBChildItemID).Select(x => x.YarnSubBrandName).Distinct());

                    }
                    #endregion

                    #region Group Collar & Cuff Yarn item (YBChildItem)
                    var gDataCollarCuff = from x in child.ChildItems.Where(m => m.SubGroupId == item.SubGroupId && m.YBChildGroupID == item.YBChildGroupID)
                                          group x by new
                                          {
                                              x.YBChildGroupID,
                                              x.YBookingID,
                                              x.YItemMasterID,
                                              x.UnitID,
                                              x.DisplayUnitDesc,
                                              x.Blending,
                                              //x.YarnCategory,
                                              x.Allowance,
                                              x.Distribution,
                                              x.ShadeCode,
                                              x.ShadeName,
                                              x.Specification,
                                              x.YDItem,
                                              x.YD,
                                              x.Segment1ValueId,
                                              x.Segment2ValueId,
                                              x.Segment3ValueId,
                                              x.Segment4ValueId,
                                              x.Segment5ValueId,
                                              x.Segment6ValueId,
                                              x.SubGroupId,
                                              x.SubGroupName,

                                              x.YarnSubBrandIDs,
                                              x.YarnSubBrandID,
                                              x.YarnSubBrandName,
                                              x.yarnItemPrice
                                          }
                                into g
                                          select new YarnBookingChildItem()
                                          {
                                              YBChildGroupID = g.Key.YBChildGroupID,
                                              YBookingID = g.Key.YBookingID,
                                              YItemMasterID = g.Key.YItemMasterID,
                                              UnitID = g.Key.UnitID,
                                              DisplayUnitDesc = g.Key.DisplayUnitDesc,
                                              Blending = g.Key.Blending,
                                              //YarnCategory = g.Key.YarnCategory,
                                              BookingQty = g.Sum(xt => xt.BookingQty),
                                              Allowance = g.Key.Allowance,
                                              Distribution = g.Key.Distribution,
                                              RequiredQty = g.Sum(xt => xt.RequiredQty),
                                              ShadeCode = g.Key.ShadeCode,
                                              ShadeName = g.Key.ShadeName,
                                              Specification = g.Key.Specification,
                                              YDItem = g.Key.YDItem,
                                              YD = g.Key.YD,
                                              Segment1ValueId = g.Key.Segment1ValueId,
                                              Segment2ValueId = g.Key.Segment2ValueId,
                                              Segment3ValueId = g.Key.Segment3ValueId,
                                              Segment4ValueId = g.Key.Segment4ValueId,
                                              Segment5ValueId = g.Key.Segment5ValueId,
                                              Segment6ValueId = g.Key.Segment6ValueId,
                                              SubGroupId = g.Key.SubGroupId,
                                              SubGroupName = g.Key.SubGroupName,

                                              YarnSubBrandIDs = g.Key.YarnSubBrandIDs,
                                              YarnSubBrandID = g.Key.YarnSubBrandID,
                                              YarnSubBrandName = g.Key.YarnSubBrandName,
                                              yarnItemPrice = g.Key.yarnItemPrice
                                          };
                    #endregion

                    #region Add ChildItem in Child
                    foreach (YarnBookingChildItem citem in gDataCollarCuff.Where(y => y.YBChildGroupID == item.YBChildGroupID && y.SubGroupId == item.SubGroupId))
                    {
                        var objTemp = objYBChild.ChildItemsGroup.Find(o =>
                           o.YBChildGroupID == item.YBChildGroupID &&
                           o.YBookingID == citem.YBookingID &&
                           o.YItemMasterID == citem.YItemMasterID &&
                           o.UnitID == citem.UnitID &&
                           o.DisplayUnitDesc == citem.DisplayUnitDesc &&
                           o.Blending == citem.Blending &&
                           o.ShadeCode == citem.ShadeCode &&
                           o.ShadeName == citem.ShadeName &&
                           o.Specification == citem.Specification &&
                           o.YDItem == citem.YDItem &&
                           o.YD == citem.YD &&
                           o.Segment1ValueId == citem.Segment1ValueId &&
                           o.Segment2ValueId == citem.Segment2ValueId &&
                           o.Segment3ValueId == citem.Segment3ValueId &&
                           o.Segment4ValueId == citem.Segment4ValueId &&
                           o.Segment5ValueId == citem.Segment5ValueId &&
                           o.Segment6ValueId == citem.Segment6ValueId &&
                           o.SubGroupId == citem.SubGroupId &&
                           o.SubGroupName == citem.SubGroupName
                       );
                        if (objTemp == null)
                        {
                            decimal bookingQty = data.Childs.Where(y => y.Segment1ValueDesc == item.Segment1ValueDesc
                                                && y.Segment2ValueDesc == item.Segment2ValueDesc
                                                && y.Segment5ValueDesc == item.Segment5ValueDesc
                                                && y.YarnTypeID == item.YarnTypeID
                                                && y.YarnBrandID == item.YarnBrandID
                                                && y.Remarks == item.Remarks
                                                && y.YarnSubBrandID == item.YarnSubBrandID
                                                && y.SubGroupId == item.SubGroupId).Sum(b => b.BookingQty);

                            YarnBookingChildItem objYBChildItems = new YarnBookingChildItem();
                            objYBChildItems.YBChildGroupID = item.YBChildGroupID;
                            objYBChildItems.YBChildItemID = iYBChildItemId++;
                            objYBChildItems.YBookingID = citem.YBookingID;
                            objYBChildItems.YItemMasterID = citem.YItemMasterID;
                            objYBChildItems.UnitID = citem.UnitID;
                            objYBChildItems.DisplayUnitDesc = citem.DisplayUnitDesc;
                            objYBChildItems.Blending = citem.Blending;
                            objYBChildItems.YarnCategory = citem.YarnCategory;
                            objYBChildItems.BookingQty = (bookingQty * citem.Distribution) / 100;
                            objYBChildItems.Allowance = citem.Allowance;
                            objYBChildItems.Distribution = citem.Distribution;
                            objYBChildItems.RequiredQty = objYBChildItems.BookingQty + ((((bookingQty * citem.Distribution) / 100) * citem.Allowance) / 100);
                            objYBChildItems.ShadeCode = citem.ShadeCode;
                            objYBChildItems.ShadeName = citem.ShadeName;
                            objYBChildItems.Specification = citem.Specification;
                            objYBChildItems.YDItem = citem.YDItem;
                            objYBChildItems.YD = citem.YD;
                            objYBChildItems.Segment1ValueId = citem.Segment1ValueId;
                            objYBChildItems.Segment2ValueId = citem.Segment2ValueId;
                            objYBChildItems.Segment3ValueId = citem.Segment3ValueId;
                            objYBChildItems.Segment4ValueId = citem.Segment4ValueId;
                            objYBChildItems.Segment5ValueId = citem.Segment5ValueId;
                            objYBChildItems.Segment6ValueId = citem.Segment6ValueId;
                            objYBChildItems.SubGroupId = citem.SubGroupId;
                            objYBChildItems.SubGroupName = citem.SubGroupName;

                            objYBChildItems.YarnSubBrandIDs = citem.YarnSubBrandIDs;
                            objYBChildItems.YarnSubBrandID = citem.YarnSubBrandID;
                            objYBChildItems.YarnSubBrandName = citem.YarnSubBrandName;
                            objYBChildItems.yarnItemPrice = citem.yarnItemPrice;
                            objYBChildItems.Remarks = citem.Remarks;

                            objYBChild.ChildItemsGroup.Add(objYBChildItems);
                        }
                    }
                    #endregion
                }
                #endregion

                //// Finally add Child in data for collar & Cuff
                data.ChildsGroup.Add(objYBChild);

            }
            #endregion

            #endregion
            return data;
        }
        public async Task<YarnBookingMaster> GetFBARevisionPending(string bookingNo, string exportOrderNo)
        {
            string sql = $@"
            With EWO as
            (
	            Select * From {DbNames.EPYSL}..ExportOrderMaster Where ExportOrderNo='{exportOrderNo}'
            ),
	        HasAB As 
	        (
		        Select AdditionalBooking = Max(AdditionalBooking),BM.ExportOrderID,1 SubGroupID
		        From {DbNames.EPYSL}..BookingMaster BM
		        Inner Join {DbNames.EPYSL}..BookingChild BC On BC.BookingID = BM.BookingID                                                   
		        Where BM.ExportOrderID = (Select Top 1 ExportOrderID From EWO) And BM.BookingNo = '{bookingNo}'
		        Group By BM.ExportOrderID
	        ), BMStatus As	
	        (
		        Select BMM.ExportOrderID,BC.ContactID,BC.BOMMasterID,1 SubGroupID, Proposed = min(Convert(int,BM.Proposed))
		        From {DbNames.EPYSL}..BookingMaster BM
		        Inner Join {DbNames.EPYSL}..BookingChild BC On BC.BookingID = BM.BookingID    
		        Inner Join {DbNames.EPYSL}..BOMMaster BMM On BMM.BOMMasterID = BC.BOMMasterID
		        Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = BC.SubGroupID  
		        left Join {DbNames.EPYSL}..ContactAdditionalInfo SAI On SAI.ContactID = BM.SupplierID                                             
		        Where BMM.ExportOrderID = (Select Top 1 ExportOrderID From EWO)  And BM.BookingNo = '{bookingNo}' 
		        And ISG.SubGroupName in ('Fabric','Collar','Cuff')	And Isnull(SAI.InHouse,0) = 1
		        Group By BMM.ExportOrderID,BC.ContactID,BC.BOMMasterID									
	        ),
	        BIG As 
	        (										
		        Select BMM.ExportOrderID, BMM.BOMMasterID, SubGroupID = 1,BIGC.ContactID, RevisionNo = Sum(BIGC.RevisionNo),
		        IsSkipRevision = Max(Convert(int,IsNull(BIG.IsSkipRevision,0))),
		        MaterialStatus= IsNull(Min(Convert(int,BIG.MaterialStatus)),0),
		        Status= IsNull(Min(Convert(int,BIG.Status)),0),
		        ISApproved= IsNull(Min(Convert(int,IsApproved)),0),
		        SendToApproval= IsNull(Min(Convert(int,BIG.SendToApproval)),0)
		        From {DbNames.EPYSL}..BOMItemGroup BIG
		        Inner Join {DbNames.EPYSL}..BOMItemGroupContact BIGC On BIGC.BOMMasterID = BIG.BOMMasterID 
                And BIGC.SubGroupID = BIG.SubGroupID 
		        And BIGC.ItemGroupID = BIG.ItemGroupID
		        Inner Join {DbNames.EPYSL}..BOMMaster BMM On BMM.BOMMasterID = BIG.BOMMasterID
		        Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = BIG.SubGroupID
		        left Join {DbNames.EPYSL}..ContactAdditionalInfo SAI On SAI.ContactID = BIGC.ContactID
		        Where BMM.ExportOrderID = (Select Top 1 ExportOrderID From EWO)  
                And ISG.SubGroupName in ('Fabric','Collar','Cuff')
		        And Isnull(SAI.InHouse,0) = 1 And BIG.IsOnlineBooking  in (0,2)
		        Group by BMM.ExportOrderID,BMM.BOMMasterID,BIGC.ContactID
            )
            Select Distinct BOMStatus = Case When BIG.RevisionNo > BM.PreProcessRevNo And BIG.IsApproved = 0 Then 'BOMRevisionPending'
            When BIG.IsSkipRevision = 0 And BM.RevisionNeed = 1 Then 'BookingRevisionPending'
            When BIG.IsSkipRevision = 0 And BIG.IsApproved=1 And BM.Proposed=0 Then 'BookingRevisionPending'
            When BIG.IsSkipRevision = 0 And BIG.IsApproved=1 And BM.Proposed=0 Then 'BookingRevisionPending'
            When BM.RevisionNo > BIA.PreProcessRevNo And BIG.IsApproved=1 And BM.Proposed=1 Then 'BookingIARevisionPending'
            When BIA.RevisionNo > IsNull(FBA.PreRevisionNo,0) And BIG.IsApproved=1 And BM.Proposed=1 Then 'FBARevisionPending'
            Else 'YBMRevisionPending' End 
            FROM {DbNames.EPYSL}..ExportOrderMaster EOM
            left Join {DbNames.EPYSL}..BOMMaster BMM On BMM.ExportOrderID = EOM.ExportOrderID
            left Join BIG BIG On BIG.BOMMasterID = BMM.BOMMasterID And BIG.ExportOrderID = EOM.ExportOrderID
            left Join 
            ( 
	            Select BM.ExportOrderID,BM.BookingID,BMStatus.Proposed,Convert(int,BM.RevisionNeed) RevisionNeed,
	            PreProcessRevNo = Case When isnull(IsSkipRevision,0) = 1 then BIG.RevisionNo else BM.PreProcessRevNo end,
	            --BM.PreProcessRevNo,
	            BM.RevisionNo,BMStatus.SubGroupID, BMStatus.ContactID
	            From {DbNames.EPYSL}..BookingMaster BM
	            Inner Join BMStatus BMStatus On BMStatus.ExportOrderID = BM.ExportOrderID And BM.SupplierID = BMStatus.ContactID And BMStatus.SubGroupID = BM.SubGroupID		                                    
	            --   Inner Join BookingChild BC On BC.BookingID = BM.BookingID
	            Inner Join BIG BIG On BIG.BOMMasterID = BMStatus.BOMMasterID And BIG.SubGroupID = BMStatus.SubGroupID And BIG.ContactID = BMStatus.ContactID
	            Inner Join HasAB HAB On HAB.ExportOrderID = BM.ExportOrderID And BM.AdditionalBooking = HAB.AdditionalBooking 
	            And HAB.SubGroupID = BMStatus.SubGroupID
	            --	Inner Join BMStatus BMStatus On BMStatus.ExportOrderID = BM.ExportOrderID And BM.SupplierID = BMStatus.ContactID And BMStatus.SubGroupID = BMStatus.SubGroupID		                                    
	            Where BM.ExportOrderID = (Select Top 1 ExportOrderID From EWO)  And BM.BookingNo =  '{bookingNo}'
	            Group By BM.ExportOrderID,BM.BookingID,BMStatus.Proposed,Convert(int,BM.RevisionNeed),BM.PreProcessRevNo,BM.RevisionNo,BMStatus.SubGroupID, 
	            BMStatus.ContactID,isnull(IsSkipRevision,0),BIG.RevisionNo                                     
            ) BM On BM.ExportOrderID = EOM.ExportOrderID And BM.SubGroupID = BIG.SubGroupID And BIG.ContactID = BM.ContactID
            left Join {DbNames.EPYSL}..BookingItemAcknowledge BIA On BIA.BookingID = BM.BookingID
            Left JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA On FBA.BookingID = BIA.BookingID
            Where EOM.ExportOrderID = (Select Top 1 ExportOrderID From EWO)";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnBookingMaster data = await records.ReadFirstOrDefaultAsync<YarnBookingMaster>();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open) _connection.Close();
            }
        }
        public async Task<List<YarnBookingReason>> GetReason(int yBookingID, string reasonStatus)
        {
            var sql = "";

            if (reasonStatus == "BookingList" || reasonStatus == "AdditionalList")
            {
                sql = $@"
                With BAR as
                (  
	                Select * from {DbNames.EPYSL}..BookingAdditionalReason Where UseInYBooking = 1 And IsAddition = 1 
                ),
                BKRR as
                ( 
	                Select YBookingID, ReasonID, YBKReasonID FROM {TableNames.YarnBookingReason_New} Where YBookingID = {yBookingID}
                )
                Select Isnull(BKRR.YBKReasonID,0)YBKReasonID, Isnull(BKRR.YBookingID,0)YBookingID, BAR.ReasonID, 
                BAR.ReasonName, BAR.IsRePurchase, BAR.IsAddition, BAR.IsRevision, BAR.UseInBooking, BAR.Remarks, 
                Selected = Case When ISNULL(BKRR.ReasonID,0) = 0 then 0 Else 1 End
                from BAR Left Join BKRR On BKRR.ReasonID = BAR.ReasonID ";
            }
            else if (reasonStatus == "ProposeList" || reasonStatus == "ReviseList" || reasonStatus == "ExecutedList" || reasonStatus == "ExecutedList")
            {
                sql = $@"
                With BAR as
                (  
	                Select * from {DbNames.EPYSL}..BookingAdditionalReason Where UseInYBooking = 1 And IsRevision = 1 
                ),
                BKRR as
                ( 
	                Select YBookingID, ReasonID, YBKReasonID FROM {TableNames.YarnBookingReason_New} Where YBookingID = {yBookingID}
                )
                Select Isnull(BKRR.YBKReasonID,0)YBKReasonID, Isnull(BKRR.YBookingID,0)YBookingID, BAR.ReasonID, 
                BAR.ReasonName, BAR.IsRePurchase, BAR.IsAddition, BAR.IsRevision, BAR.UseInBooking, BAR.Remarks, 
                Selected = Case When ISNULL(BKRR.ReasonID,0) = 0 then 0 Else 1 End
                from BAR Left Join BKRR On BKRR.ReasonID = BAR.ReasonID ";
            }
            return await _service.GetDataAsync<YarnBookingReason>(sql);
        }
        public async Task<string> GetYarnRequiredDate(string ExportOrderNo, string CDays)
        {
            if (ExportOrderNo.IsNullOrEmpty()) return null;

            var sql = $@"
            With EC As 
            (
	            Select T.*,E.ExportOrderID,E.ExportOrderNo 
                From {DbNames.EPYSL}..ExportOrderMaster E
	            Inner Join {DbNames.EPYSL}..TNAEventCalander T On T.BuyerTeamID = E.BuyerTeamID
	            Where E.ExportOrderNo = {ExportOrderNo}
            ),
            DDuration As
            (
	            Select OBEC.ExportOrderID, EC.BuyerTeamID, EDD = (OBEC.EDD)
	            From {DbNames.EPYSL}..OrderEventCalander OBEC
	            Inner Join EC On EC.EventID = OBEC.EventID And EC.CDays = OBEC.CDays and EC.ExportOrderID = OBEC.ExportOrderID
	            where EC.ExportOrderNo = {ExportOrderNo}
            )
            Select 
            --DD.ExportOrderID, T.BuyerTeamID, T.CDays, TE.EventDescription, 
            EventDate = Replace(CONVERT(varchar, Min(DATEADD(DD,T.TNADays,DD.EDD)), 106), ' ','-') --Min(DATEADD(DD,T.TNADays,DD.EDD))
            --, EDD = Min(DD.EDD)
            from DDuration DD
            Inner Join {DbNames.EPYSL}..TNAEventCalander T On T.BuyerTeamID = DD.BuyerTeamID
            Inner Join {DbNames.EPYSL}..TNAEvent TE On TE.EventID = T.EventID
            where T.CDays = {CDays} and TE.EventDescription in ('Yarn Allocation')
            group by T.SeqNo, DD.ExportOrderID, T.BuyerTeamID, T.CDays, TE.EventDescription
            Order by T.SeqNo ";

            return await _service.GetSingleStringFieldAsync(sql);
        }
        public async Task<YarnBookingMaster> GetAllByIDAsync(int id)
        {
            string sql = $@"
            ;Select * FROM {TableNames.YarnBookingMaster_New} Where YBookingID = {id}

            ;Select * FROM {TableNames.YarnBookingChild_New} Where YBookingID = {id}

            ;Select * FROM {TableNames.YarnBookingChildItem_New} Where YBookingID = {id}

            ;Select * FROM {TableNames.YarnBookingReason_New} Where YBookingID = {id} 

            ;Select * FROM {TableNames.YarnBookingChildYarnSubBrand_New} where YBChildID IN(Select YBChildID FROM {TableNames.YarnBookingChild_New} Where YBookingID = {id})
            
            ;Select * FROM {TableNames.YarnBookingChildGarmentPart_New} where YBChildID IN(Select YBChildID FROM {TableNames.YarnBookingChild_New} Where YBookingID = {id})

            ;Select * FROM {TableNames.YarnBookingChildItemYarnSubBrand_New} Where YBChildItemID In(Select YBChildItemID FROM {TableNames.YarnBookingChildItem_New} Where YBookingID ={id})

            ;Select * FROM {TableNames.YarnItemPrice} Where YBChildItemID In(Select YBChildItemID FROM {TableNames.YarnBookingChildItem_New} Where YBookingID = {id}) ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnBookingMaster data = records.Read<YarnBookingMaster>().FirstOrDefault();
                Guard.Against.NullObject(data);
                data.Childs = records.Read<YarnBookingChild>().ToList();
                data.ChildItems = records.Read<YarnBookingChildItem>().ToList();
                foreach (YarnBookingChild item in data.Childs)
                {
                    item.ChildItems = data.ChildItems.Where(x => x.YBChildID == item.YBChildID).ToList();
                }
                data.yarnBookingReason = records.Read<YarnBookingReason>().ToList();

                data.yarnBookingChildYarnSubBrand = records.Read<YarnBookingChildYarnSubBrand>().ToList();
                foreach (YarnBookingChild item in data.Childs)
                {
                    item.yarnBookingChildYarnSubBrand = data.yarnBookingChildYarnSubBrand.Where(x => x.YBChildID == item.YBChildID).ToList();
                }
                data.yarnBookingChildGarmentPart = records.Read<YarnBookingChildGarmentPart>().ToList();
                foreach (YarnBookingChild item in data.Childs)
                {
                    item.yarnBookingChildGarmentPart = data.yarnBookingChildGarmentPart.Where(x => x.YBChildID == item.YBChildID).ToList();
                }

                data.yarnBookingChildItemYarnSubBrand = records.Read<YarnBookingChildItemYarnSubBrand>().ToList();
                foreach (YarnBookingChild item in data.Childs)
                {
                    foreach (YarnBookingChildItem itemChild in item.ChildItems)
                    {
                        itemChild.yarnBookingChildItemYarnSubBrand = data.yarnBookingChildItemYarnSubBrand.Where(
                            x => x.YBChildItemID == itemChild.YBChildItemID).ToList();
                    }
                }
                data.yarnItemPrice = records.Read<YarnItemPrice>().ToList();
                foreach (YarnBookingChild item in data.Childs)
                {
                    foreach (YarnBookingChildItem itemChild in item.ChildItems)
                    {
                        itemChild.yarnItemPrice = data.yarnItemPrice.Where(
                            x => x.YBChildItemID == itemChild.YBChildItemID &&
                            x.YBookingID == itemChild.YBookingID).ToList();
                    }
                }
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open) _connection.Close();
            }
        }
        public async Task UpdateEntityAsync(YarnBookingMaster entity)
        {
            SqlTransaction transaction = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _service.SaveSingleAsync(entity, transaction);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }
        public async Task UpdateYB(int userId, List<YarnBookingMaster> yarnBookings, bool isRevised = false, List<FBookingAcknowledge> FBAList = null, bool isAdditionalAcknowledge = false)
        {
            SqlTransaction transaction = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                List<YarnBookingMaster> yarnBookingsList = new List<YarnBookingMaster>();
                List<YarnBookingChild> yarnBookingChildList = new List<YarnBookingChild>();
                List<YarnBookingChildItem> yarnBookingChildItemList = new List<YarnBookingChildItem>();

                yarnBookings.ForEach(x =>
                {
                    yarnBookingChildList.AddRange(x.Childs);
                    x.Childs.ForEach(c =>
                    {
                        yarnBookingChildItemList.AddRange(c.ChildItems);
                    });
                });
                if (FBAList.Count > 0)
                {
                    await _service.SaveAsync(FBAList, transaction);
                }
                await _service.SaveAsync(yarnBookings, transaction);
                foreach (YarnBookingMaster item in yarnBookings)
                {
                    //await _service.ValidationSingleAsync(item, transaction, "sp_Validation_YarnBookingMaster_1", item.EntityState, userId, item.YBookingID);
                    await _connection.ExecuteAsync("sp_Validation_YarnBookingMaster_1", new { EntityState = item.EntityState, UserId = userId, PrimaryKeyId = item.YBookingID }, transaction, 30, CommandType.StoredProcedure);
                }
                await _service.SaveAsync(yarnBookingChildList, transaction);
                foreach (YarnBookingChild item in yarnBookingChildList)
                {
                    //await _service.ValidationSingleAsync(item, transaction, "sp_Validation_YarnBookingChild_1", item.EntityState, userId, item.YBChildID);
                    await _connection.ExecuteAsync("sp_Validation_YarnBookingChild_1", new { EntityState = item.EntityState, UserId = userId, PrimaryKeyId = item.YBChildID }, transaction, 30, CommandType.StoredProcedure);
                }
                await _service.SaveAsync(yarnBookingChildItemList, transaction);
                foreach (YarnBookingChildItem item in yarnBookingChildItemList)
                {
                    //await _service.ValidationSingleAsync(item, transaction, "sp_Validation_YarnBookingChildItem_1", item.EntityState, userId, item.YBChildItemID);
                    await _connection.ExecuteAsync("sp_Validation_YarnBookingChildItem_1", new { EntityState = item.EntityState, UserId = userId, PrimaryKeyId = item.YBChildItemID }, transaction, 30, CommandType.StoredProcedure);
                }
                if (isRevised)
                {
                    await _connection.ExecuteAsync("spYarnAllocationChildItemUpdateForRevision", new { YBookingNo = yarnBookings[0].YBookingNo, UserID = userId }, transaction, 30, CommandType.StoredProcedure);
                }

                if (isAdditionalAcknowledge && yarnBookings[0].AcknowledgeCount > 1)
                {
                    await _connection.ExecuteAsync("spYarnBooking_BK", new { YBookingNo = yarnBookings[0].YBookingNo, IsFinalApprove = true, IsFinalReject = false, IsFabricRevision = false }, transaction, 30, CommandType.StoredProcedure);
                }
                transaction.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }

        //Save Operation
        //public async Task AcknowledgeforAutoPR(int YBookingID)
        //{

        //}
        public async Task<List<YarnBookingMaster>> GetAllByNoAsync(string yBookingNo)
        {
            string sql = $@"
            ;Select * FROM {TableNames.YarnBookingMaster_New} Where YBookingNo = '{yBookingNo}'

            ;Select YBC.YBChildID, YBC.YBookingID, YBC.ConsumptionID, YBC.ItemMasterID, YBC.YarnTypeID, YBC.YarnBrandID, 
            YBC.FUPartID, YBC.BookingUnitID, YBC.BookingQty, YBC.FTechnicalName, YBC.IsCompleteReceive, YBC.IsCompleteDelivery, 
            YBC.LastDCDate, YBC.ClosingRemarks, YBM.SubGroupID 
            FROM {TableNames.YarnBookingChild_New} YBC 
            Inner JOIN {TableNames.YarnBookingMaster_New} YBM On YBC.YBookingID = YBM.YBookingID
            Where YBM.YBookingNo = '{yBookingNo}'; 

            ;Select YBCI.YBChildItemID, YBCI.YBChildID, YBCI.YBookingID, YBCI.YItemMasterID, YBCI.UnitID, YBCI.Blending, 
            YBCI.YarnCategory, YBCI.Distribution, YBCI.BookingQty, YBCI.Allowance, YBCI.RequiredQty, YBCI.ShadeCode, 
            YBCI.Remarks, YBCI.Specification, YBCI.YD, YBCI.YDItem, YBM.SubGroupID 
            FROM {TableNames.YarnBookingChildItem_New} YBCI
            Inner JOIN {TableNames.YarnBookingMaster_New} YBM On YBCI.YBookingID = YBM.YBookingID
            Where YBM.YBookingNo = '{yBookingNo}'; 
 
            ;Select YBR.YBKReasonID, YBR.YBookingID, YBR.RevisionNo, YBR.ReasonID, YBR.DateAdded 
            FROM {TableNames.YarnBookingReason_New} YBR
            Inner JOIN {TableNames.YarnBookingMaster_New} YBM On YBR.YBookingID = YBM.YBookingID
            Where YBM.YBookingNo = '{yBookingNo}';

            ;Select * FROM {TableNames.YarnBookingChildYarnSubBrand_New} where YBChildID IN(Select YBChildID FROM {TableNames.YarnBookingChild_New} Where YBookingID IN(
	            Select YBookingID FROM {TableNames.YarnBookingMaster_New} Where YBookingNo = '{yBookingNo}'))

            ;Select * FROM {TableNames.YarnBookingChildGarmentPart_New} where YBChildID IN(Select YBChildID FROM {TableNames.YarnBookingChild_New} Where YBookingID IN(
	            Select YBookingID FROM {TableNames.YarnBookingMaster_New} Where YBookingNo = '{yBookingNo}'))

            ;Select * FROM {TableNames.YarnBookingChildItemYarnSubBrand_New} Where YBChildItemID In(Select YBChildItemID FROM {TableNames.YarnBookingChildItem_New} Where YBookingID IN(
	            Select YBookingID FROM {TableNames.YarnBookingMaster_New} Where YBookingNo = '{yBookingNo}'))

            ;Select * FROM {TableNames.YarnItemPrice} Where YBChildItemID In(Select YBChildItemID FROM {TableNames.YarnBookingChildItem_New} Where YBookingID In(
	            Select YBookingID FROM {TableNames.YarnBookingMaster_New} Where YBookingNo = '{yBookingNo}')) ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<YarnBookingMaster> data = records.Read<YarnBookingMaster>().ToList();

                //Child
                List<YarnBookingChild> childs = records.Read<YarnBookingChild>().ToList();
                data.ForEach(datas =>
                {
                    datas.Childs = childs.Where(x => x.YBookingID == datas.YBookingID).ToList();
                });

                //Child Items
                List<YarnBookingChildItem> childsItems = records.Read<YarnBookingChildItem>().ToList();
                childs.ForEach(datas =>
                {
                    datas.ChildItems = childsItems.Where(x => x.YBChildID == datas.YBChildID).ToList();
                });

                //Reason
                List<YarnBookingReason> yReason = records.Read<YarnBookingReason>().ToList();
                data.ForEach(datas =>
                {
                    datas.yarnBookingReason = yReason.Where(x => x.YBookingID == datas.YBookingID).ToList();
                });

                //YarnBookingChildYarnSubBrand
                List<YarnBookingChildYarnSubBrand> ChildYarnSubBrand = records.Read<YarnBookingChildYarnSubBrand>().ToList();
                childs.ForEach(datas =>
                {
                    datas.yarnBookingChildYarnSubBrand = ChildYarnSubBrand.Where(x => x.YBChildID == datas.YBChildID).ToList();
                });

                //YarnBookingChildGarmentPart
                List<YarnBookingChildGarmentPart> ChildGarmentPart = records.Read<YarnBookingChildGarmentPart>().ToList();
                childs.ForEach(datas =>
                {
                    datas.yarnBookingChildGarmentPart = ChildGarmentPart.Where(x => x.YBChildID == datas.YBChildID).ToList();
                });

                //YarnBookingChildItemYarnSubBrand
                List<YarnBookingChildItemYarnSubBrand> ChildItemYarnSubBrand = records.Read<YarnBookingChildItemYarnSubBrand>().ToList();
                childsItems.ForEach(datas =>
                {
                    datas.yarnBookingChildItemYarnSubBrand = ChildItemYarnSubBrand.Where(x => x.YBChildItemID == datas.YBChildItemID).ToList();
                });

                //YarnItemPrice
                List<YarnItemPrice> yarnIPrice = records.Read<YarnItemPrice>().ToList();
                childsItems.ForEach(datas =>
                {
                    datas.yarnItemPrice = yarnIPrice.Where(x => x.YBChildItemID == datas.YBChildItemID &&
                            x.YBookingID == datas.YBookingID).ToList();
                });
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }
        public async Task SaveMultipleAsync(List<YarnBookingMaster> entities, EntityState entityState, bool isRevice)
        {
            try
            {
                #region Set Primary Keys
                int countYBM = 0,
                   countYBC = 0,
                   countYBCI = 0,
                   countYSB = 0,
                   countGP = 0,
                   countCIYSB = 0;

                entities.ForEach(master =>
                {
                    countYBM++;
                    master.Fabrics.ForEach(fabric =>
                    {
                        if (fabric.EntityState == EntityState.Added)
                        {
                            countYBC++;
                        }
                        fabric.yarnBookingChildYarnSubBrand.Where(x => x.EntityState == EntityState.Added).ToList().ForEach(ysb =>
                        {
                            countYSB++;
                        });
                        fabric.yarnBookingChildGarmentPart.Where(x => x.EntityState == EntityState.Added).ToList().ForEach(g =>
                        {
                            countGP++;
                        });
                        fabric.ChildItems.ForEach(ci =>
                        {
                            if (ci.EntityState == EntityState.Added)
                            {
                                countYBCI++;
                            }
                            ci.yarnBookingChildItemYarnSubBrand.Where(x => x.EntityState == EntityState.Added).ToList().ForEach(ysb =>
                            {
                                countCIYSB++;
                            });
                        });
                    });
                    master.Collars.ForEach(collar =>
                    {
                        if (collar.EntityState == EntityState.Added)
                        {
                            countYBC++;
                        }
                        collar.yarnBookingChildYarnSubBrand.Where(x => x.EntityState == EntityState.Added).ToList().ForEach(ysb =>
                        {
                            countYSB++;
                        });
                        collar.yarnBookingChildGarmentPart.Where(x => x.EntityState == EntityState.Added).ToList().ForEach(g =>
                        {
                            countGP++;
                        });
                        collar.ChildItems.ForEach(ci =>
                        {
                            if (ci.EntityState == EntityState.Added)
                            {
                                countYBCI++;
                            }
                            ci.yarnBookingChildItemYarnSubBrand.Where(x => x.EntityState == EntityState.Added).ToList().ForEach(ysb =>
                            {
                                countCIYSB++;
                            });
                        });
                    });
                    master.Cuffs.ForEach(cuff =>
                    {
                        if (cuff.EntityState == EntityState.Added)
                        {
                            countYBC++;
                        }
                        cuff.yarnBookingChildYarnSubBrand.Where(x => x.EntityState == EntityState.Added).ToList().ForEach(ysb =>
                        {
                            countYSB++;
                        });
                        cuff.yarnBookingChildGarmentPart.Where(x => x.EntityState == EntityState.Added).ToList().ForEach(g =>
                        {
                            countGP++;
                        });
                        cuff.ChildItems.ForEach(ci =>
                        {
                            if (ci.EntityState == EntityState.Added)
                            {
                                countYBCI++;
                            }
                            ci.yarnBookingChildItemYarnSubBrand.Where(x => x.EntityState == EntityState.Added).ToList().ForEach(ysb =>
                            {
                                countCIYSB++;
                            });
                        });
                    });
                });

                int pkYBM = await _service.GetMaxIdAsync(TableNames.YarnBookingMaster, countYBM),
                    pkYBC = await _service.GetMaxIdAsync(TableNames.YarnBookingChild, countYBC),
                    pkYBCI = await _service.GetMaxIdAsync(TableNames.YarnBookingChildItem, countYBCI),
                    pkYSB = await _service.GetMaxIdAsync(TableNames.YarnBookingChildYarnSubBrand, countYSB),
                    pkGP = await _service.GetMaxIdAsync(TableNames.YarnBookingChildGarmentPart, countGP),
                    pkCIYSB = await _service.GetMaxIdAsync(TableNames.YarnBookingChildItemYarnSubBrand, countCIYSB);

                #endregion

                if (isRevice)
                {
                    await _service.ExecuteAsync("spBackupYarnBooking_Full_New", new { YBookingNo = entities[0].YBookingNo }, 30, CommandType.StoredProcedure);
                }

                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                /*
                switch (entityState)
                {
                    case EntityState.Added:
                        entities = await AddManyAsync(entities);
                        break;

                    case EntityState.Modified:
                        entities = await UpdateMany(entities);
                        break;
                    default:
                        break;
                }
                */

                List<YarnBookingMaster> yBookings = new List<YarnBookingMaster>();

                List<YarnBookingChild> childs = new List<YarnBookingChild>();
                List<YarnBookingChildYarnSubBrand> ChildYarnSubBrand = new List<YarnBookingChildYarnSubBrand>();
                List<YarnBookingChildGarmentPart> ChildGarmentPart = new List<YarnBookingChildGarmentPart>();

                List<YarnBookingChildItem> ChildItems = new List<YarnBookingChildItem>();
                List<YarnBookingChildItem> ChildItemsGroup = new List<YarnBookingChildItem>();

                List<YarnBookingChildItemYarnSubBrand> ChildItemYarnSubBrand = new List<YarnBookingChildItemYarnSubBrand>();
                List<YarnBookingChildItemYarnSubBrand> ChildItemYarnSubBrandGroup = new List<YarnBookingChildItemYarnSubBrand>();

                string YBookingNo = "";
                if (entityState == EntityState.Added && entities.Count() > 0) YBookingNo = entities[0].BookingNo + "-YB";// await _service.GetMaxNoAsync(TableNames.YBooking_No);

                entities.ForEach(master =>
                {
                    if (master.EntityState == EntityState.Added)
                    {
                        master.YBookingID = pkYBM++;
                        if (entityState == EntityState.Added)
                        {
                            master.YBookingNo = YBookingNo;
                        }
                    }
                    yBookings.Add(master);

                    master.Fabrics.ForEach(fabric =>
                    {
                        if (fabric.EntityState == EntityState.Added)
                        {
                            fabric.YBChildID = pkYBC++;
                            fabric.YBookingID = master.YBookingID;
                        }
                        childs.Add(CommonFunction.DeepClone(fabric));

                        fabric.yarnBookingChildYarnSubBrand.ForEach(ysb =>
                        {
                            if (ysb.EntityState == EntityState.Added)
                            {
                                ysb.YBookingCYSubBrandID = pkYSB++;
                                ysb.YBChildID = fabric.YBChildID;
                            }
                            ChildYarnSubBrand.Add(CommonFunction.DeepClone(ysb));
                        });
                        fabric.yarnBookingChildGarmentPart.ForEach(g =>
                        {
                            if (g.EntityState == EntityState.Added)
                            {
                                g.YBookingCGPID = pkGP++;
                                g.YBChildID = fabric.YBChildID;
                            }
                            ChildGarmentPart.Add(CommonFunction.DeepClone(g));
                        });
                        fabric.ChildItems.ForEach(ci =>
                        {
                            if (ci.EntityState == EntityState.Added)
                            {
                                ci.YBChildItemID = pkYBCI++;
                                ci.YBChildID = fabric.YBChildID;
                                ci.YBookingID = master.YBookingID;
                            }
                            ChildItems.Add(CommonFunction.DeepClone(ci));

                            ci.yarnBookingChildItemYarnSubBrand.ForEach(ysb =>
                            {
                                if (ysb.EntityState == EntityState.Added)
                                {
                                    ysb.YBCItemYSubBrandID = pkCIYSB++;
                                    ysb.YBChildItemID = ci.YBChildItemID;
                                    ysb.YBChildID = ci.YBChildID;
                                }
                                ChildItemYarnSubBrand.Add(CommonFunction.DeepClone(ysb));
                            });
                        });
                    });
                    master.Collars.ForEach(collar =>
                    {
                        if (collar.EntityState == EntityState.Added)
                        {
                            collar.YBChildID = pkYBC++;
                            collar.YBookingID = master.YBookingID;
                        }
                        childs.Add(CommonFunction.DeepClone(collar));

                        collar.yarnBookingChildYarnSubBrand.ForEach(ysb =>
                        {
                            if (ysb.EntityState == EntityState.Added)
                            {
                                ysb.YBookingCYSubBrandID = pkYSB++;
                                ysb.YBChildID = collar.YBChildID;
                            }
                            ChildYarnSubBrand.Add(CommonFunction.DeepClone(ysb));
                        });
                        collar.yarnBookingChildGarmentPart.ForEach(g =>
                        {
                            if (g.EntityState == EntityState.Added)
                            {
                                g.YBookingCGPID = pkGP++;
                                g.YBChildID = collar.YBChildID;
                            }
                            ChildGarmentPart.Add(CommonFunction.DeepClone(g));
                        });
                        collar.ChildItems.ForEach(ci =>
                        {
                            if (ci.EntityState == EntityState.Added)
                            {
                                ci.YBChildItemID = pkYBCI++;
                                ci.YBChildID = collar.YBChildID;
                                ci.YBookingID = master.YBookingID;

                            }
                            ChildItems.Add(CommonFunction.DeepClone(ci));

                            ci.yarnBookingChildItemYarnSubBrand.ForEach(ysb =>
                            {
                                if (ysb.EntityState == EntityState.Added)
                                {
                                    ysb.YBCItemYSubBrandID = pkCIYSB++;
                                    ysb.YBChildItemID = ci.YBChildItemID;
                                    ysb.YBChildID = ci.YBChildID;
                                }
                                ChildItemYarnSubBrand.Add(CommonFunction.DeepClone(ysb));
                            });
                        });
                    });
                    master.Cuffs.ForEach(cuff =>
                    {
                        if (cuff.EntityState == EntityState.Added)
                        {
                            cuff.YBChildID = pkYBC++;
                            cuff.YBookingID = master.YBookingID;
                        }
                        childs.Add(CommonFunction.DeepClone(cuff));

                        cuff.yarnBookingChildYarnSubBrand.ForEach(ysb =>
                        {
                            if (ysb.EntityState == EntityState.Added)
                            {
                                ysb.YBookingCYSubBrandID = pkYSB++;
                                ysb.YBChildID = cuff.YBChildID;
                            }
                            ChildYarnSubBrand.Add(CommonFunction.DeepClone(ysb));
                        });
                        cuff.yarnBookingChildGarmentPart.ForEach(g =>
                        {
                            if (g.EntityState == EntityState.Added)
                            {
                                g.YBookingCGPID = pkGP++;
                                g.YBChildID = cuff.YBChildID;
                            }
                            ChildGarmentPart.Add(CommonFunction.DeepClone(g));
                        });
                        cuff.ChildItems.ForEach(ci =>
                        {
                            if (ci.EntityState == EntityState.Added)
                            {
                                ci.YBChildItemID = pkYBCI++;
                                ci.YBChildID = cuff.YBChildID;
                                ci.YBookingID = master.YBookingID;
                            }
                            ChildItems.Add(CommonFunction.DeepClone(ci));

                            ci.yarnBookingChildItemYarnSubBrand.ForEach(ysb =>
                            {
                                if (ysb.EntityState == EntityState.Added)
                                {
                                    ysb.YBCItemYSubBrandID = pkCIYSB++;
                                    ysb.YBChildItemID = ci.YBChildItemID;
                                    ysb.YBChildID = ci.YBChildID;
                                }
                                ChildItemYarnSubBrand.Add(CommonFunction.DeepClone(ysb));
                            });
                        });
                    });
                });
                string c = string.Join(",", childs.Select(x => x.YBChildID));
                string m = string.Join(",", childs.Select(x => x.YBookingID));
                string sg = string.Join(",", childs.Select(x => x.SubGroupId));

                string im = string.Join(",", childs.Select(x => x.ItemMasterID));
                string con = string.Join(",", childs.Select(x => x.ConsumptionID));

                await _service.SaveAsync(entities, transaction);
                await _service.SaveAsync(childs, transaction);
                await _service.SaveAsync(ChildItems, transaction);
                await _service.SaveAsync(ChildYarnSubBrand, transaction);
                await _service.SaveAsync(ChildGarmentPart, transaction);
                await _service.SaveAsync(ChildItemYarnSubBrand, transaction);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                throw ex;
            }
            finally
            {
                if (transaction != null) transaction.Dispose();
                _connection.Close();
            }
        }
        private async Task<List<YarnBookingMaster>> AddManyAsync(List<YarnBookingMaster> entities)
        {
            string a = "";
            int YBookingID = await _service.GetMaxIdAsync(TableNames.YarnBookingMaster, entities.Count);
            string YBookingNo = await _service.GetMaxNoAsync(TableNames.YBooking_No);
            int maxChildId = await _service.GetMaxIdAsync(TableNames.YarnBookingChild, entities.Sum(x => x.Childs.Count));

            //YarnBookingChildYarnSubBrand
            int pkCYSubBrandID = 0;
            entities.ToList().ForEach(entity =>
            {
                entity.Childs.ToList().ForEach(ch =>
                {
                    ch.yarnBookingChildYarnSubBrand.ToList().ForEach(cg =>
                    {
                        pkCYSubBrandID++;
                    });
                });
            });
            int maxYBookingCYSubBrandID = await _service.GetMaxIdAsync(TableNames.YarnBookingChildYarnSubBrand, pkCYSubBrandID);

            //YarnBookingChildGarmentPart
            int pkChildGarmentPart = 0;
            entities.ToList().ForEach(entity =>
            {
                entity.Childs.ToList().ForEach(ch =>
                {
                    ch.yarnBookingChildGarmentPart.ToList().ForEach(cg =>
                    {
                        pkChildGarmentPart++;
                    });
                });
            });
            int maxYBookingCGPID = await _service.GetMaxIdAsync(TableNames.YarnBookingChildGarmentPart, pkChildGarmentPart);

            //YarnBookingChildItem
            int pkChildItem = 0;
            entities.ToList().ForEach(entity =>
            {
                entity.Childs.ToList().ForEach(ch =>
                {
                    ch.ChildItems.ToList().ForEach(cg =>
                    {
                        pkChildItem++;
                    });
                });
            });
            int maxChildItemId = await _service.GetMaxIdAsync(TableNames.YarnBookingChildItem, pkChildItem);

            int pkChildItemGrup = 0;
            entities.ToList().ForEach(entity =>
            {
                entity.Childs.ToList().ForEach(ch =>
                {
                    ch.ChildItemsGroup.ToList().ForEach(cg =>
                    {
                        pkChildItemGrup++;
                    });
                });
            });
            int maxChildItemIdGroup = await _service.GetMaxIdAsync(TableNames.YarnBookingChildItem, pkChildItemGrup);

            //YarnBookingChildItemYarnSubBrand
            int pkChildItemYarnSubBrand = 0;
            entities.ToList().ForEach(entity =>
            {
                entity.Childs.ToList().ForEach(ch =>
                {
                    ch.ChildItems.ToList().ForEach(cg =>
                    {
                        cg.yarnBookingChildItemYarnSubBrand.ToList().ForEach(ybci =>
                        {
                            pkChildItemYarnSubBrand++;
                        });
                    });
                });
            });
            int maxYBCItemYSubBrandID = await _service.GetMaxIdAsync(TableNames.YarnBookingChildItemYarnSubBrand, pkChildItemYarnSubBrand);

            int pkChildItemYarnSubBrandGroup = 0;
            entities.ToList().ForEach(entity =>
            {
                entity.Childs.ToList().ForEach(ch =>
                {
                    ch.ChildItemsGroup.ToList().ForEach(cg =>
                    {
                        cg.yarnBookingChildItemYarnSubBrand.ToList().ForEach(ybci =>
                        {
                            pkChildItemYarnSubBrandGroup++;
                        });
                    });
                });
            });
            int maxYBCItemYSubBrandIDGroup = await _service.GetMaxIdAsync(TableNames.YarnBookingChildItemYarnSubBrand, pkChildItemYarnSubBrandGroup);


            //YarnItemPrice
            int pkYItemPrice = 0;
            entities.ToList().ForEach(entity =>
            {
                entity.Childs.ToList().ForEach(ch =>
                {
                    ch.ChildItems.ToList().ForEach(cg =>
                    {
                        cg.yarnItemPrice.ToList().ForEach(ybci =>
                        {
                            pkYItemPrice++;
                        });
                    });
                });
            });
            int maxYIPriceID = await _service.GetMaxIdAsync(TableNames.YarnItemPrice, pkYItemPrice);

            int pkYItemPriceGroup = 0;
            entities.ToList().ForEach(entity =>
            {
                entity.Childs.ToList().ForEach(ch =>
                {
                    ch.ChildItemsGroup.ToList().ForEach(cg =>
                    {
                        cg.yarnItemPrice.ToList().ForEach(ybci =>
                        {
                            pkYItemPriceGroup++;
                        });
                    });
                });
            });
            int maxYIPriceIDGroup = await _service.GetMaxIdAsync(TableNames.YarnItemPrice, pkYItemPriceGroup);

            entities.ToList().ForEach(entity =>
            {
                entity.YBookingID = YBookingID++;
                entity.YBookingNo = YBookingNo;
                entity.EntityState = EntityState.Added;
                var childs = entity.Childs.Where(x => x.SubGroupId == entity.SubGroupID && x.BookingID == entity.BookingID).ToList();
                foreach (YarnBookingChild item in childs)
                {
                    int vYBChildID = maxChildId++;
                    item.YBookingID = entity.YBookingID;

                    //YarnBookingChildYarnSubBrand
                    foreach (YarnBookingChildYarnSubBrand ChildYarnSubBrand in item.yarnBookingChildYarnSubBrand)
                    {
                        ChildYarnSubBrand.YBookingCYSubBrandID = maxYBookingCYSubBrandID++;
                        ChildYarnSubBrand.YBChildID = vYBChildID;

                    }
                    //YarnBookingChildGarmentPart
                    foreach (YarnBookingChildGarmentPart ChildGarmentPart in item.yarnBookingChildGarmentPart)
                    {
                        ChildGarmentPart.YBookingCGPID = maxYBookingCGPID++;
                        ChildGarmentPart.YBChildID = vYBChildID;
                    }

                    foreach (YarnBookingChildItem itemDtls in item.ChildItems.Where(x => x.SubGroupId == entity.SubGroupID && x.YBChildID == item.YBChildID))
                    {
                        itemDtls.YBChildItemID = maxChildItemId++;
                        itemDtls.YBChildID = vYBChildID;
                        itemDtls.YBookingID = item.YBookingID;

                        a += itemDtls.YBChildItemID.ToString() + ",";

                        //YarnBookingChildItemYarnSubBrand
                        foreach (YarnBookingChildItemYarnSubBrand ChildItemYarnSubBrand in itemDtls.yarnBookingChildItemYarnSubBrand)
                        {
                            ChildItemYarnSubBrand.YBCItemYSubBrandID = maxYBCItemYSubBrandID++;
                            ChildItemYarnSubBrand.YBChildItemID = itemDtls.YBChildItemID;
                        }
                        //YarnItemPrice
                        foreach (YarnItemPrice ChildYarnItemPrice in itemDtls.yarnItemPrice)
                        {
                            ChildYarnItemPrice.YIPriceID = maxYIPriceID++;
                            ChildYarnItemPrice.YBChildItemID = itemDtls.YBChildItemID;
                            ChildYarnItemPrice.YBookingID = entity.YBookingID;
                        }
                    }

                    foreach (YarnBookingChildItem itemDtls in item.ChildItemsGroup.Where(x => x.SubGroupId == entity.SubGroupID && x.YBChildID == item.YBChildID))
                    {
                        itemDtls.YBChildItemID = maxChildItemIdGroup++;
                        itemDtls.YBChildID = vYBChildID;
                        itemDtls.YBookingID = item.YBookingID;

                        a += itemDtls.YBChildItemID.ToString() + ",";

                        //YarnBookingChildItemYarnSubBrand
                        foreach (YarnBookingChildItemYarnSubBrand ChildItemYarnSubBrand in itemDtls.yarnBookingChildItemYarnSubBrand)
                        {
                            ChildItemYarnSubBrand.YBCItemYSubBrandID = maxYBCItemYSubBrandIDGroup++;
                            ChildItemYarnSubBrand.YBChildItemID = itemDtls.YBChildItemID;
                        }
                        //YarnItemPrice
                        foreach (YarnItemPrice ChildYarnItemPrice in itemDtls.yarnItemPrice)
                        {
                            ChildYarnItemPrice.YIPriceID = maxYIPriceIDGroup++;
                            ChildYarnItemPrice.YBChildItemID = itemDtls.YBChildItemID;
                            ChildYarnItemPrice.YBookingID = entity.YBookingID;
                        }
                    }
                    item.YBChildID = vYBChildID;
                }
            });

            return entities;
        }
        private async Task<List<YarnBookingMaster>> UpdateMany(List<YarnBookingMaster> entities)
        {
            int pkChildItem = 0;
            int pkChildItemYarnSubBrand = 0;
            int pkYItemPrice = 0;
            int pkYBCYS = 0;
            int pkGarment = 0;

            var pkChildId = entities.Select(a => a.Childs.Where(x => x.EntityState == EntityState.Added).Count());
            int maxChildId = await _service.GetMaxIdAsync(TableNames.YarnBookingChild, pkChildId.Count());

            entities.ToList().ForEach(entity =>
            {
                entity.Childs.ToList().ForEach(ch =>
                {
                    ch.yarnBookingChildYarnSubBrand.Where(x => x.EntityState == EntityState.Added).ToList().ForEach(ybcy =>
                    {
                        pkYBCYS++;
                    });
                    ch.yarnBookingChildGarmentPart.Where(x => x.EntityState == EntityState.Added).ToList().ForEach(ybci =>
                    {
                        pkGarment++;
                    });
                    ch.ChildItems.ToList().ForEach(cg =>
                    {
                        if (cg.EntityState == EntityState.Added)
                        {
                            pkChildItem++;
                        }
                        cg.yarnBookingChildItemYarnSubBrand.Where(x => x.EntityState == EntityState.Added).ToList().ForEach(ybci =>
                        {
                            pkChildItemYarnSubBrand++;
                        });
                        cg.yarnItemPrice.Where(x => x.EntityState == EntityState.Added).ToList().ForEach(ybci =>
                        {
                            pkYItemPrice++;
                        });
                    });
                });
            });
            int maxChildItemId = await _service.GetMaxIdAsync(TableNames.YarnBookingChildItem, pkChildItem);
            int maxYBCItemYSubBrandID = await _service.GetMaxIdAsync(TableNames.YarnBookingChildItemYarnSubBrand, pkChildItemYarnSubBrand);
            int maxYIPriceID = await _service.GetMaxIdAsync(TableNames.YarnItemPrice, pkYItemPrice);
            int maxYBCYSId = await _service.GetMaxIdAsync(TableNames.YarnBookingChildYarnSubBrand, pkYBCYS);
            int maxGarmentId = await _service.GetMaxIdAsync(TableNames.YarnBookingChildGarmentPart, pkGarment);

            //Child Update
            entities.ForEach(m =>
            {
                m.Childs.ForEach(c =>
                {
                    if (c.EntityState == EntityState.Added)
                    {
                        c.YBChildID = maxChildId++;
                        c.YBookingID = m.YBookingID;
                    }
                    else if (c.EntityState == EntityState.Modified)
                    {
                        c.EntityState = EntityState.Modified;
                    }
                    c.yarnBookingChildYarnSubBrand.ForEach(ybcy =>
                    {
                        if (c.EntityState == EntityState.Deleted)
                        {
                            ybcy.EntityState = EntityState.Deleted;
                        }
                        else if (ybcy.EntityState == EntityState.Added)
                        {
                            ybcy.YBookingCYSubBrandID = maxYBCYSId++;
                            ybcy.YBChildID = c.YBChildID;
                            ybcy.EntityState = EntityState.Added;
                        }
                        else if (ybcy.EntityState == EntityState.Modified)
                        {
                            ybcy.EntityState = EntityState.Modified;
                        }
                    });
                    c.yarnBookingChildGarmentPart.ForEach(g =>
                    {
                        if (c.EntityState == EntityState.Deleted)
                        {
                            g.EntityState = EntityState.Deleted;
                        }
                        else if (g.EntityState == EntityState.Added)
                        {
                            g.YBookingCGPID = maxGarmentId++;
                            g.YBChildID = c.YBChildID;
                            g.EntityState = EntityState.Added;
                        }
                        else if (g.EntityState == EntityState.Modified)
                        {
                            g.EntityState = EntityState.Modified;
                        }
                    });
                    c.ChildItems.ForEach(ci =>
                    {
                        if (c.EntityState == EntityState.Deleted)
                        {
                            ci.EntityState = EntityState.Deleted;
                        }
                        else if (ci.EntityState == EntityState.Added)
                        {
                            ci.YBChildItemID = maxChildItemId++;
                            ci.YBChildID = c.YBChildID;
                            ci.YBookingID = m.YBookingID;
                            ci.EntityState = EntityState.Added;
                        }
                        else if (ci.EntityState == EntityState.Modified)
                        {
                            ci.EntityState = EntityState.Modified;
                        }
                        ci.yarnBookingChildItemYarnSubBrand.ToList().ForEach(ybci =>
                        {
                            if (c.EntityState == EntityState.Deleted)
                            {
                                ybci.EntityState = EntityState.Deleted;
                            }
                            else if (ybci.EntityState == EntityState.Added)
                            {
                                ybci.YBCItemYSubBrandID = maxYBCItemYSubBrandID++;
                                ybci.YBChildItemID = ci.YBChildItemID;
                            }
                            else if (ybci.EntityState == EntityState.Modified)
                            {
                                ybci.EntityState = EntityState.Modified;
                            }
                        });
                        ci.yarnItemPrice.ToList().ForEach(yip =>
                        {
                            if (c.EntityState == EntityState.Deleted)
                            {
                                yip.EntityState = EntityState.Deleted;
                            }
                            else if (yip.EntityState == EntityState.Added)
                            {
                                yip.YIPriceID = maxYIPriceID++;
                                yip.YBChildItemID = ci.YBChildItemID;
                                yip.YBookingID = m.YBookingID;
                            }
                            else if (yip.EntityState == EntityState.Modified)
                            {
                                yip.YBookingID = m.YBookingID;
                                yip.EntityState = EntityState.Modified;
                            }
                        });
                    });
                });
            });
            return entities;
        }

        //Additional Operation
        public async Task AdditionalSaveMultipleAsync(List<YarnBookingMaster> entities, EntityState entityState)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                entities = await AdditionalAddManyAsync(entities);

                await _service.SaveAsync(entities, transaction);
                //Child
                List<YarnBookingChild> childs = new List<YarnBookingChild>();
                entities.ForEach(entity =>
                {
                    childs.AddRange(entity.Childs.Where(x => x.SubGroupId == entity.SubGroupID));
                });
                await _service.SaveAsync(childs, transaction);

                //YarnBookingChildYarnSubBrand
                List<YarnBookingChildYarnSubBrand> ChildYarnSubBrand = new List<YarnBookingChildYarnSubBrand>();
                childs.ForEach(CYarnSubBrand =>
                {
                    ChildYarnSubBrand.AddRange(CYarnSubBrand.yarnBookingChildYarnSubBrand);
                });
                await _service.SaveAsync(ChildYarnSubBrand, transaction);

                //YarnBookingChildGarmentPart
                List<YarnBookingChildGarmentPart> ChildGarmentPart = new List<YarnBookingChildGarmentPart>();
                childs.ForEach(CGarmentPart =>
                {
                    ChildGarmentPart.AddRange(CGarmentPart.yarnBookingChildGarmentPart);
                });
                await _service.SaveAsync(ChildGarmentPart, transaction);

                //YarnBookingChildItem
                List<YarnBookingChildItem> ChildItems = new List<YarnBookingChildItem>();
                childs.ForEach(CItems =>
                {
                    ChildItems.AddRange(CItems.ChildItems);
                });
                await _service.SaveAsync(ChildItems, transaction);

                //YarnBookingChildItemYarnSubBrand
                List<YarnBookingChildItemYarnSubBrand> ChildItemYarnSubBrand = new List<YarnBookingChildItemYarnSubBrand>();
                ChildItems.ForEach(CIYarnSubBrand =>
                {
                    ChildItemYarnSubBrand.AddRange(CIYarnSubBrand.yarnBookingChildItemYarnSubBrand);
                });
                await _service.SaveAsync(ChildItemYarnSubBrand, transaction);

                //YarnItemPrice
                List<YarnItemPrice> yarnItemPrice = new List<YarnItemPrice>();
                ChildItems.ForEach(YItemPrice =>
                {
                    yarnItemPrice.AddRange(YItemPrice.yarnItemPrice);
                });
                await _service.SaveAsync(yarnItemPrice, transaction);

                //YarnBookingReason
                List<YarnBookingReason> yarnBookingReason = new List<YarnBookingReason>();
                //entities.ForEach(entity =>
                //{
                yarnBookingReason.AddRange(entities[0].yarnBookingReason);
                //});
                await _service.SaveAsync(yarnBookingReason, transaction);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                throw ex;
            }
            finally
            {
                if (transaction != null) transaction.Dispose();
                _connection.Close();
            }
        }
        private async Task<List<YarnBookingMaster>> AdditionalAddManyAsync(List<YarnBookingMaster> entities)
        {
            int YBookingID = await _service.GetMaxIdAsync(TableNames.YarnBookingMaster, entities.Count);
            int ybkReasonID = await _service.GetMaxIdAsync(TableNames.YarnBookingReason, entities.Sum(x => x.yarnBookingReason.Count));
            int maxChildId = await _service.GetMaxIdAsync(TableNames.YarnBookingChild, entities.Sum(x => x.Childs.Count));

            //YarnBookingChildYarnSubBrand
            int pkCYSubBrandID = 0;
            entities.ToList().ForEach(entity =>
            {
                entity.Childs.ToList().ForEach(ch =>
                {
                    ch.yarnBookingChildYarnSubBrand.ToList().ForEach(cg =>
                    {
                        pkCYSubBrandID++;
                    });
                });
            });
            int maxYBookingCYSubBrandID = await _service.GetMaxIdAsync(TableNames.YarnBookingChildYarnSubBrand, pkCYSubBrandID);

            //YarnBookingChildGarmentPart
            int pkChildGarmentPart = 0;
            entities.ToList().ForEach(entity =>
            {
                entity.Childs.ToList().ForEach(ch =>
                {
                    ch.yarnBookingChildGarmentPart.ToList().ForEach(cg =>
                    {
                        pkChildGarmentPart++;
                    });
                });
            });
            int maxYBookingCGPID = await _service.GetMaxIdAsync(TableNames.YarnBookingChildGarmentPart, pkChildGarmentPart);

            //YarnBookingChildItem
            int pkChildItem = 0;
            entities.ToList().ForEach(entity =>
            {
                entity.Childs.ToList().ForEach(ch =>
                {
                    ch.ChildItems.ToList().ForEach(cg =>
                    {
                        pkChildItem++;
                    });
                });
            });
            int maxChildItemId = await _service.GetMaxIdAsync(TableNames.YarnBookingChildItem, pkChildItem);

            //YarnBookingChildItemYarnSubBrand
            int pkChildItemYarnSubBrand = 0;
            entities.ToList().ForEach(entity =>
            {
                entity.Childs.ToList().ForEach(ch =>
                {
                    ch.ChildItems.ToList().ForEach(cg =>
                    {
                        cg.yarnBookingChildItemYarnSubBrand.ToList().ForEach(ybci =>
                        {
                            pkChildItemYarnSubBrand++;
                        });
                    });
                });
            });
            int maxYBCItemYSubBrandID = await _service.GetMaxIdAsync(TableNames.YarnBookingChildItemYarnSubBrand, pkChildItemYarnSubBrand);

            //YarnItemPrice
            int pkYItemPrice = 0;
            entities.ToList().ForEach(entity =>
            {
                entity.Childs.ToList().ForEach(ch =>
                {
                    ch.ChildItems.ToList().ForEach(cg =>
                    {
                        cg.yarnItemPrice.ToList().ForEach(ybci =>
                        {
                            pkYItemPrice++;
                        });
                    });
                });
            });
            int maxYIPriceID = await _service.GetMaxIdAsync(TableNames.YarnItemPrice, pkYItemPrice);

            //Set YBookingNo
            string addID = Convert.ToString(entities[0].LastAdditionalBooking + 1);
            string YBookingNo = entities[0].YBookingNo + "-Add-" + addID;

            entities.ToList().ForEach(entity =>
            {
                entity.YBookingID = YBookingID++;
                entity.YBookingNo = YBookingNo;
                entity.EntityState = EntityState.Added;

                foreach (YarnBookingChild item in entity.Childs.Where(x => x.SubGroupId == entity.SubGroupID))
                {
                    item.YBChildID = maxChildId++;
                    item.YBookingID = entity.YBookingID;

                    //YarnBookingChildYarnSubBrand
                    foreach (YarnBookingChildYarnSubBrand ChildYarnSubBrand in item.yarnBookingChildYarnSubBrand)
                    {
                        ChildYarnSubBrand.YBookingCYSubBrandID = maxYBookingCYSubBrandID++;
                        ChildYarnSubBrand.YBChildID = item.YBChildID;

                    }
                    //YarnBookingChildGarmentPart
                    foreach (YarnBookingChildGarmentPart ChildGarmentPart in item.yarnBookingChildGarmentPart)
                    {
                        ChildGarmentPart.YBookingCGPID = maxYBookingCGPID++;
                        ChildGarmentPart.YBChildID = item.YBChildID;
                    }

                    foreach (YarnBookingChildItem itemDtls in item.ChildItems.Where(x => x.SubGroupId == entity.SubGroupID))
                    {
                        itemDtls.YBChildItemID = maxChildItemId++;
                        itemDtls.YBChildID = item.YBChildID;
                        itemDtls.YBookingID = item.YBookingID;

                        //YarnBookingChildItemYarnSubBrand
                        foreach (YarnBookingChildItemYarnSubBrand ChildItemYarnSubBrand in itemDtls.yarnBookingChildItemYarnSubBrand)
                        {
                            ChildItemYarnSubBrand.YBCItemYSubBrandID = maxYBCItemYSubBrandID++;
                            ChildItemYarnSubBrand.YBChildItemID = itemDtls.YBChildItemID;
                        }
                        //YarnItemPrice
                        foreach (YarnItemPrice ChildYarnItemPrice in itemDtls.yarnItemPrice)
                        {
                            ChildYarnItemPrice.YIPriceID = maxYIPriceID++;
                            ChildYarnItemPrice.YBChildItemID = itemDtls.YBChildItemID;
                            ChildYarnItemPrice.YBookingID = entity.YBookingID;
                        }
                    }
                }
            });

            //Reason
            entities.ForEach(entity =>
            {
                foreach (YarnBookingReason reason in entity.yarnBookingReason)
                {
                    reason.YBKReasonID = ybkReasonID++;
                    reason.YBookingID = entity.YBookingID;
                }
            });

            return entities;
        }

        //Revise Operation
        public async Task ReviseSaveMultipleAsync(List<YarnBookingMaster> entities, EntityState entityState)
        {
            try
            {
                await _service.ExecuteAsync("spBackupYarnBooking_Full", new { YBookingNo = entities[0].YBookingNo }, 30, CommandType.StoredProcedure);

                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                entities = await ReviseUpdateMany(entities);

                //YarnBookingMaster 
                await _service.SaveAsync(entities, transaction);

                //Child
                List<YarnBookingChild> childs = new List<YarnBookingChild>();
                List<YarnBookingChild> delChilds = new List<YarnBookingChild>();
                entities.ForEach(entity =>
                {
                    childs.AddRange(entity.Childs.Where(x => x.SubGroupId == entity.SubGroupID && x.EntityState != EntityState.Deleted));
                    delChilds.AddRange(entity.Childs.Where(x => x.SubGroupId == entity.SubGroupID && x.EntityState == EntityState.Deleted));
                });
                await _service.SaveAsync(childs, transaction);

                //YarnBookingChildYarnSubBrand
                List<YarnBookingChildYarnSubBrand> ChildYarnSubBrand = new List<YarnBookingChildYarnSubBrand>();
                List<YarnBookingChildYarnSubBrand> delChildYarnSubBrand = new List<YarnBookingChildYarnSubBrand>();
                childs.ForEach(CYarnSubBrand =>
                {
                    ChildYarnSubBrand.AddRange(CYarnSubBrand.yarnBookingChildYarnSubBrand.Where(x => x.EntityState != EntityState.Deleted));
                });
                delChilds.ForEach(CYarnSubBrand =>
                {
                    delChildYarnSubBrand.AddRange(CYarnSubBrand.yarnBookingChildYarnSubBrand.Where(x => x.EntityState == EntityState.Deleted));
                });
                await _service.SaveAsync(ChildYarnSubBrand, transaction);

                //YarnBookingChildGarmentPart
                List<YarnBookingChildGarmentPart> ChildGarmentPart = new List<YarnBookingChildGarmentPart>();
                List<YarnBookingChildGarmentPart> delChildGarmentPart = new List<YarnBookingChildGarmentPart>();
                childs.ForEach(CGarmentPart =>
                {
                    ChildGarmentPart.AddRange(CGarmentPart.yarnBookingChildGarmentPart.Where(x => x.EntityState != EntityState.Deleted));
                });
                delChilds.ForEach(CGarmentPart =>
                {
                    delChildGarmentPart.AddRange(CGarmentPart.yarnBookingChildGarmentPart.Where(x => x.EntityState == EntityState.Deleted));
                });
                await _service.SaveAsync(ChildGarmentPart, transaction);

                //YarnBookingChildItem
                List<YarnBookingChildItem> ChildItems = new List<YarnBookingChildItem>();
                List<YarnBookingChildItem> delChildItems = new List<YarnBookingChildItem>();
                childs.ForEach(CItems =>
                {
                    ChildItems.AddRange(CItems.ChildItems.Where(x => x.EntityState != EntityState.Deleted));
                });
                delChilds.ForEach(CItems =>
                {
                    delChildItems.AddRange(CItems.ChildItems.Where(x => x.EntityState == EntityState.Deleted));
                });
                //entities.ForEach(entity =>
                //{
                //    entity.Childs.ForEach(CItems =>
                //    {
                //        ChildItems.AddRange(CItems.ChildItems.Where(x => x.EntityState != EntityState.Deleted &&
                //        CItems.EntityState != EntityState.Deleted));
                //        delChildItems.AddRange(CItems.ChildItems.Where(x => x.EntityState == EntityState.Deleted &&
                //        CItems.EntityState == EntityState.Deleted));
                //    });
                //});
                await _service.SaveAsync(ChildItems, transaction);

                //YarnBookingChildItemYarnSubBrand
                List<YarnBookingChildItemYarnSubBrand> ChildItemYarnSubBrand = new List<YarnBookingChildItemYarnSubBrand>();
                List<YarnBookingChildItemYarnSubBrand> delChildItemYarnSubBrand = new List<YarnBookingChildItemYarnSubBrand>();
                ChildItems.ForEach(CIYarnSubBrand =>
                {
                    ChildItemYarnSubBrand.AddRange(CIYarnSubBrand.yarnBookingChildItemYarnSubBrand.Where(x => x.EntityState != EntityState.Deleted));
                });
                delChildItems.ForEach(CIYarnSubBrand =>
                {
                    delChildItemYarnSubBrand.AddRange(CIYarnSubBrand.yarnBookingChildItemYarnSubBrand.Where(x => x.EntityState == EntityState.Deleted));
                });
                await _service.SaveAsync(ChildItemYarnSubBrand, transaction);

                //YarnItemPrice
                List<YarnItemPrice> yarnItemPrice = new List<YarnItemPrice>();
                List<YarnItemPrice> delYarnItemPrice = new List<YarnItemPrice>();
                ChildItems.ForEach(YItemPrice =>
                {
                    yarnItemPrice.AddRange(YItemPrice.yarnItemPrice.Where(x => x.EntityState != EntityState.Deleted));
                });
                delChildItems.ForEach(YItemPrice =>
                {
                    delYarnItemPrice.AddRange(YItemPrice.yarnItemPrice.Where(x => x.EntityState == EntityState.Deleted));
                });
                await _service.SaveAsync(yarnItemPrice, transaction);

                //YarnBookingReason
                List<YarnBookingReason> yarnBookingReason = new List<YarnBookingReason>();
                List<YarnBookingReason> delYarnBookingReason = new List<YarnBookingReason>();
                yarnBookingReason.AddRange(entities[0].yarnBookingReason.Where(x => x.EntityState != EntityState.Deleted));
                delYarnBookingReason.AddRange(entities[0].yarnBookingReason.Where(x => x.EntityState == EntityState.Deleted));
                await _service.SaveAsync(yarnBookingReason, transaction);

                //Deleted
                //YarnBookingReason 
                await _service.SaveAsync(delYarnBookingReason, transaction);
                await _service.SaveAsync(delYarnItemPrice, transaction);
                await _service.SaveAsync(delChildItemYarnSubBrand, transaction);
                await _service.SaveAsync(delChildItems, transaction);
                await _service.SaveAsync(delChildGarmentPart, transaction);
                await _service.SaveAsync(delChildYarnSubBrand, transaction);
                await _service.SaveAsync(delChilds, transaction);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                throw ex;
            }
            finally
            {
                if (transaction != null) transaction.Dispose();
                _connection.Close();
            }
        }
        private async Task<List<YarnBookingMaster>> ReviseUpdateMany(List<YarnBookingMaster> entities)
        {




            var pkChildId = entities.Select(a => a.Childs.Where(x => x.EntityState == EntityState.Added).Count());
            int maxChildId = await _service.GetMaxIdAsync(TableNames.YarnBookingChild, pkChildId.Count());
            //var ybkReasonID = await _service.GetMaxIdAsync(TableNames.YarnBookingReason, entities.Count);
            int ybkReasonID = await _service.GetMaxIdAsync(TableNames.YarnBookingReason, entities.Sum(x => x.yarnBookingReason.Count));

            //YarnBookingChildYarnSubBrand
            int pkCYSubBrandID = 0;
            entities.ToList().ForEach(entity =>
            {
                entity.Childs.ToList().ForEach(ch =>
                {
                    ch.yarnBookingChildYarnSubBrand.Where(h => h.EntityState == EntityState.Added).ToList().ForEach(cg =>
                    {



                        pkCYSubBrandID++;
                    });
                });
            });
            int maxYBookingCYSubBrandID = await _service.GetMaxIdAsync(TableNames.YarnBookingChildYarnSubBrand, pkCYSubBrandID);

            //YarnBookingChildGarmentPart
            int pkChildGarmentPart = 0;
            entities.ToList().ForEach(entity =>
            {
                entity.Childs.ToList().ForEach(ch =>
                {
                    ch.yarnBookingChildGarmentPart.Where(h => h.EntityState == EntityState.Added).ToList().ForEach(cg =>
                    {



                        pkChildGarmentPart++;
                    });
                });
            });
            int maxYBookingCGPID = await _service.GetMaxIdAsync(TableNames.YarnBookingChildGarmentPart, pkChildGarmentPart);


            //YarnBookingChildItem
            int pkChildItem = 0;
            entities.ToList().ForEach(entity =>
            {
                entity.Childs.ToList().ForEach(ch =>
                {
                    ch.ChildItems.Where(x => x.EntityState == EntityState.Added).ToList().ForEach(cg =>
                    {



                        pkChildItem++;
                    });
                });
            });
            int maxChildItemId = await _service.GetMaxIdAsync(TableNames.YarnBookingChildItem, pkChildItem);

            //YarnBookingChildItemYarnSubBrand
            int pkChildItemYarnSubBrand = 0;
            entities.ToList().ForEach(entity =>
            {
                entity.Childs.ToList().ForEach(ch =>
                {
                    ch.ChildItems.ToList().ForEach(cg =>
                    {
                        cg.yarnBookingChildItemYarnSubBrand.Where(h => h.EntityState == EntityState.Added).ToList().ForEach(ybci =>
                        {




                            pkChildItemYarnSubBrand++;
                        });
                    });
                });
            });
            int maxYBCItemYSubBrandID = await _service.GetMaxIdAsync(TableNames.YarnBookingChildItemYarnSubBrand, pkChildItemYarnSubBrand);

            //YarnItemPrice
            int pkYItemPrice = 0;
            entities.ToList().ForEach(entity =>
            {
                entity.Childs.ToList().ForEach(ch =>
                {
                    ch.ChildItems.ToList().ForEach(cg =>
                    {
                        cg.yarnItemPrice.Where(h => h.EntityState == EntityState.Added).ToList().ForEach(ybci =>
                        {




                            pkYItemPrice++;
                        });
                    });
                });
            });
            int maxYIPriceID = await _service.GetMaxIdAsync(TableNames.YarnItemPrice, pkYItemPrice);
            //Child Update
            entities.ForEach(x =>
            {
                x.Childs.ForEach(y =>
                {
                    switch (y.EntityState)
                    {
                        case EntityState.Added:
                            y.YBChildID = maxChildId++;
                            y.YBookingID = x.YBookingID;
                            break;

                        case EntityState.Modified:
                            y.EntityState = EntityState.Modified;
                            break;
                        default:
                            break;
                    }

                    foreach (YarnBookingChildYarnSubBrand CYSubBrand in y.yarnBookingChildYarnSubBrand.ToList())
                    {
                        switch (CYSubBrand.EntityState)
                        {
                            case EntityState.Added:
                                CYSubBrand.YBookingCYSubBrandID = maxYBookingCYSubBrandID++;
                                CYSubBrand.YBChildID = y.YBChildID;
                                break;
                            case EntityState.Modified:
                                CYSubBrand.EntityState = EntityState.Modified;
                                break;
                            default:
                                break;
                        }
                    }

                    foreach (YarnBookingChildGarmentPart YBCGarmentPart in y.yarnBookingChildGarmentPart.ToList())
                    {
                        switch (YBCGarmentPart.EntityState)
                        {
                            case EntityState.Added:
                                YBCGarmentPart.YBookingCGPID = maxYBookingCGPID++;
                                YBCGarmentPart.YBChildID = y.YBChildID;
                                break;
                            case EntityState.Modified:
                                YBCGarmentPart.EntityState = EntityState.Modified;
                                break;
                            default:
                                break;
                        }
                    }
                });
            });

            //Child Items Update
            entities.ForEach(x =>
            {
                x.Childs.ForEach(y =>
                {


                    y.ChildItems.ForEach(m =>
                    {
                        switch (m.EntityState)
                        {
                            case EntityState.Added:
                                m.YBChildItemID = maxChildItemId++;
                                m.YBChildID = y.YBChildID;
                                m.YBookingID = x.YBookingID;
                                m.EntityState = EntityState.Added;
                                break;

                            case EntityState.Modified:
                                m.EntityState = EntityState.Modified;
                                break;

                            default:
                                break;
                        }
                    });
                });
            });

            entities.ForEach(x =>
            {
                x.Childs.ForEach(y =>
                {
                    y.ChildItems.ForEach(m =>
                    {
                        foreach (YarnBookingChildItemYarnSubBrand CItemYSubBrand in m.yarnBookingChildItemYarnSubBrand.ToList())
                        {
                            switch (CItemYSubBrand.EntityState)
                            {
                                case EntityState.Added:
                                    CItemYSubBrand.YBCItemYSubBrandID = maxYBCItemYSubBrandID++;
                                    CItemYSubBrand.YBChildItemID = m.YBChildItemID;
                                    break;
                                case EntityState.Modified:
                                    CItemYSubBrand.EntityState = EntityState.Modified;
                                    break;
                                default:
                                    break;
                            }
                        }

                        foreach (YarnItemPrice yarnItemPrice in m.yarnItemPrice.ToList())
                        {
                            switch (yarnItemPrice.EntityState)
                            {
                                case EntityState.Added:
                                    yarnItemPrice.YIPriceID = maxYIPriceID++;
                                    yarnItemPrice.YBChildItemID = m.YBChildItemID;
                                    yarnItemPrice.YBookingID = x.YBookingID;
                                    break;
                                case EntityState.Modified:
                                    yarnItemPrice.YBookingID = x.YBookingID;
                                    yarnItemPrice.EntityState = EntityState.Modified;
                                    break;
                                default:
                                    break;
                            }
                        }

                    });
                });
            });

            //Reason
            foreach (YarnBookingReason yBookingReason in entities[0].yarnBookingReason.ToList())
            {
                switch (yBookingReason.EntityState)
                {
                    case EntityState.Added:
                        yBookingReason.YBKReasonID = ybkReasonID++;
                        yBookingReason.YBookingID = entities[0].YBookingID;
                        break;
                    case EntityState.Modified:
                        yBookingReason.EntityState = EntityState.Modified;
                        break;
                    default:
                        break;
                }
            }

            return entities;
        }

        //Email 
        public async Task<YarnBookingMaster> GetBookingInformation(string YBookingNo, bool WithoutOB)
        {
            string sql = "";
            if (!WithoutOB)
            {
                sql = $@"
                Select top 1 YBM.YBookingID, YBM.YBookingNo, YBM.PreProcessRevNo, YBM.RevisionNo, YBM.YBookingDate, YBM.ExportOrderID, YBM.BookingID, YBM.YInHouseDate, YBM.YRequiredDate, 
                YBM.ContactPerson, YBM.Propose, YBM.ProposeDate, YBM.Acknowledge, YBM.AcknowledgeDate, YBM.Remarks, Isnull(YBM.AddedBy,0)AddedBy, YBM.DateAdded, Isnull(YBM.UpdatedBy,0)UpdatedBy, YBM.DateUpdated, YBM.DateRevised,
                YBM.HasYPrice, YBM.YPRevisionNo, YBM.ApproveYP, YBM.UnApproveYP, YBM.ApproveFP, YBM.RevisionReason, YBM.YPPreProcessRevNo, YBM.FPRevisionNo, YBM.FPPreProcessRevNo, YBM.PORevisionNeed,ISG.SubGroupName, 
                BM.BookingNo, FBA.SubGroupID, YBM.AdditionalBooking, YBM.YBRevisionNeed, C.ShortName BuyerName, CCT.TeamName BuyerDepartment, YBM.FPRejectReason,
                YBM.HoldYP,YBM.HoldYPBy,YBM.HoldYPDate,YBM.HoldYPReason,YBM.IsCancel, YBM.CancelReasonID, YBM.CanceledBy, YBM.DateCanceled, YBM.Exported, YBM.DateExported, YBM.ExportNo,
                EOM.ExportOrderNo, BookingBy = Case When BM.UpdatedBy = 0 then BM.AddedBy else BM.UpdatedBy End, YBM.WithoutOB, YBM.SubGroupID,
                YBM.BuyerID, YBM.BuyerTeamID, YBM.CompanyID, YBM.AllowForNextStep, YBM.TNADays, YBM.FabricInHouseDate,YBM.IsYarnStock
                From (Select * FROM {TableNames.YarnBookingMaster_New} Where WithoutOB = 0) YBM
                Inner JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA On FBA.BookingID = YBM.BookingID
                Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = FBA.SubGroupID
			    Inner Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingID = YBM.BookingID
			    Inner Join {DbNames.EPYSL}..ExportOrderMaster EOM On EOM.ExportOrderID = BM.ExportOrderID
			    Inner Join {DbNames.EPYSL}..StyleMaster SM On SM.StyleMasterID = EOM.StyleMasterID
			    Inner Join {DbNames.EPYSL}..Contacts C On C.ContactID = SM.BuyerID
			    Inner Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = SM.BuyerTeamID
			    Where YBM.YBookingNo = '{YBookingNo}' ";
            }
            else
            {
                sql = $@"
                Select top 1 YBM.YBookingID, YBM.YBookingNo, YBM.PreProcessRevNo, YBM.RevisionNo, YBM.YBookingDate, YBM.ExportOrderID, YBM.BookingID, YBM.YInHouseDate, YBM.YRequiredDate, 
                YBM.ContactPerson, YBM.Propose, YBM.ProposeDate, YBM.Acknowledge, YBM.AcknowledgeDate, YBM.Remarks, Isnull(YBM.AddedBy,0)AddedBy, YBM.DateAdded, Isnull(YBM.UpdatedBy,0)UpdatedBy, YBM.DateUpdated, YBM.DateRevised,
                YBM.HasYPrice, YBM.YPRevisionNo, YBM.ApproveYP, YBM.UnApproveYP, YBM.ApproveFP, YBM.RevisionReason, YBM.YPPreProcessRevNo, YBM.FPRevisionNo, YBM.FPPreProcessRevNo, YBM.PORevisionNeed, ISG.SubGroupName, 
                BM.BookingNo, YBM.AdditionalBooking, YBM.YBRevisionNeed, C.ShortName BuyerName, CCT.TeamName BuyerDepartment, YBM.FPRejectReason,
                YBM.HoldYP,YBM.HoldYPBy,YBM.HoldYPDate,YBM.HoldYPReason,YBM.IsCancel, YBM.CancelReasonID, YBM.CanceledBy, YBM.DateCanceled, YBM.Exported, YBM.DateExported, YBM.ExportNo,
                EOM.ExportOrderNo, BookingBy = Case When BM.UpdatedBy = 0 then BM.AddedBy else BM.UpdatedBy End, YBM.WithoutOB, YBM.SubGroupID,
                YBM.BuyerID, YBM.BuyerTeamID, YBM.CompanyID, YBM.AllowForNextStep, YBM.TNADays, YBM.FabricInHouseDate,YBM.IsYarnStock
                From (Select * FROM {TableNames.YarnBookingMaster_New} Where WithoutOB = 1) YBM
				Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = YBM.SubGroupID
                Inner Join {DbNames.EPYSL}..SampleBookingMaster BM On BM.BookingID = YBM.BookingID
				Inner Join {DbNames.EPYSL}..ExportOrderMaster EOM On EOM.ExportOrderNo = BM.SLNo
				Inner Join {DbNames.EPYSL}..StyleMaster SM On SM.StyleMasterID = EOM.StyleMasterID
				Inner Join {DbNames.EPYSL}..Contacts C On C.ContactID = SM.BuyerID
				Inner Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = SM.BuyerTeamID
				Where YBM.YBookingNo = '{YBookingNo}' ";
            }

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnBookingMaster data = records.Read<YarnBookingMaster>().FirstOrDefault();
                Guard.Against.NullObject(data);
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open) _connection.Close();
            }
        }
        private FreeConceptMaster GetFreeConcept(int conceptId, YarnBookingChild child, YarnBookingMaster master, int conceptNoCount, int entityTypeValueId)
        {
            return new FreeConceptMaster()
            {
                ConceptID = conceptId,
                ConceptNo = conceptNoCount == 0 ? master.BookingNo : master.BookingNo + "_" + conceptNoCount,
                ConceptDate = DateTime.Now,
                TrialNo = 0,
                TrialDate = null,
                ConceptFor = 0,
                ConsumptionID = child.ConsumptionID,

                KnittingTypeID = child.KnittingTypeID,
                ConstructionId = child.ConstructionID,
                CompositionId = child.CompositionID,
                GSMId = child.GSMID,

                Qty = child.BookingQty,
                QtyInKG = 0, //Calculation Pending
                ConceptStatusId = entityTypeValueId, //Running
                Remarks = master.Remarks,
                AddedBy = master.AddedBy,
                DateAdded = master.DateAdded,
                UpdatedBy = master.UpdatedBy,
                DateUpdated = master.DateUpdated,
                ProdStart = false,
                ProdComplete = false,
                TechnicalNameId = 0,
                RevisionPending = false,
                RevisionPendingDate = null,
                SubGroupID = master.SubGroupID,
                Active = true,
                MCSubClassID = 0,
                CompanyID = (int)master.CompanyID,
                IsBDS = 2,
                ItemMasterID = child.ItemMasterID,
                BookingID = master.YBookingID,
                BookingChildID = child.YBChildID,
                GroupConceptNo = master.BookingNo,
                ConceptTypeID = 0,
                FUPartID = (int)child.FUPartID,
                IsYD = false,
                MachineGauge = 0,
                Length = child.LengthYds,
                Width = 0,//child.Width,
                PreProcessRevNo = (int)master.PreProcessRevNo,
                RevisionNo = (int)master.RevisionNo,
                RevisionDate = master.RevisionDate,
                RevisionBy = 0,
                RevisionReason = master.RevisionReason,
                DeliveryComplete = child.IsCompleteDelivery,
                StatusRemarks = "",
                ChildColors = new List<FreeConceptChildColor>()
            };
        }
        private FreeConceptChildColor GetFreeConceptChildColor(int cCColorID, int conceptId, YarnBookingChild child)
        {
            var freeConceptChildColor = new FreeConceptChildColor()
            {
                CCColorID = cCColorID,
                ConceptID = conceptId,
                ColorId = child.ColorID,
                ColorCode = "",
                RequestRecipe = false,
                RequestBy = 0,
                RequestDate = null,
                RequestAck = false,
                RequestAckBy = 0,
                GrayFabricOK = false,
                Remarks = "",
                DPID = 0,
                DPProcessInfo = "",
                ColorName = child.Segment3ValueDesc,
                IsFirm = false,
                IsLive = false
            };
            return freeConceptChildColor;
        }
        private FreeConceptMRMaster GetFreeConceptMR(int conceptMRId, FreeConceptMaster freeConcept)
        {
            var mr = new FreeConceptMRMaster()
            {
                FCMRMasterID = conceptMRId,
                ReqDate = DateTime.Now,
                ConceptID = freeConcept.ConceptID,
                TrialNo = freeConcept.TrialNo,
                PreProcessRevNo = freeConcept.PreProcessRevNo,
                RevisionNo = freeConcept.RevisionNo,
                RevisionDate = freeConcept.RevisionDate,
                RevisionBy = freeConcept.RevisionBy,
                RevisionReason = freeConcept.RevisionReason,
                HasYD = false,
                Remarks = "",
                AddedBy = freeConcept.AddedBy,
                DateAdded = DateTime.Now,
                IsBDS = 2,
                FabricID = 0,
                IsComplete = true,
                Childs = new List<FreeConceptMRChild>()
            };
            return mr;
        }
        public async Task<YarnBookingMaster> GetYBForBulkAsync(string bookingNo, bool isSample)
        {
            //string joinName = isSample ? "SBCon" : "M";
            //string joinName = isSample ? "SBCon" : "SBC";
            var segmentNames = new
            {
                SegmentNames = new string[]
                {
                    ItemSegmentNameConstants.YARN_CERTIFICATIONS,
                    ItemSegmentNameConstants.YARN_SUBPROGRAM_NEW
                }
            };
            string sql = $@"  
                --Seq 1
                With AllBookingID As
                (
	                Select BookingID FROM {TableNames.FBBOOKING_ACKNOWLEDGE} Where BookingNo = '{bookingNo}'
                ),
				 FBA As
                (
	                Select top 1 FBA.* 
                    FROM {TableNames.FBBOOKING_ACKNOWLEDGE} FBA
					INNER JOIN AllBookingID ABI ON ABI.BookingID=FBA.BookingID
                ),
                FBA1 AS
                (
                    SELECT a.BookingID,a.BOMMasterID,a.ItemGroupID,a.SubGroupID,a.Status,PreRevisionNo=a.PreProcessRevNo,RevisionNo=a.RevisionNo,
	                b.BookingNo,b.BookingDate,b.Remarks,SM.StyleNo,b.ExportOrderID,SLNo='',SM.StyleMasterID,
	                b.BuyerID,b.BuyerTeamID,b.CompanyID,b.SupplierID,SM.SeasonID,a.WithoutOB,b.InHouseDate,BookingQty= SUM(BC.BookingQty),
                    IsSample = 0
                    FROM {TableNames.FabricBookingAcknowledge} A
				    JOIN {DbNames.EPYSL}..BookingMaster b on b.BookingID = a.BookingID
					INNER JOIN AllBookingID ABI ON ABI.BookingID=b.BookingID
					Inner Join {DbNames.EPYSL}..BookingChild BC On BC.BookingID = A.BookingID
				    JOIN {DbNames.EPYSL}..ExportOrderMaster EO ON EO.ExportOrderID = B.ExportOrderID
				    JOIN {DbNames.EPYSL}..StyleMaster SM ON SM.StyleMasterID = EO.StyleMasterID
					Group By a.BookingID,a.BOMMasterID,a.ItemGroupID,a.SubGroupID,a.Status,a.PreProcessRevNo,a.RevisionNo,
	                b.BookingNo,b.BookingDate,b.Remarks,SM.StyleNo,b.ExportOrderID,SM.StyleMasterID,
	                b.BuyerID,b.BuyerTeamID,b.CompanyID,b.SupplierID,SM.SeasonID,a.WithoutOB,b.InHouseDate
                ),
				FBA2 AS
                (
                    SELECT a.BookingID,a.BOMMasterID,a.ItemGroupID,a.SubGroupID,a.Status,PreRevisionNo=a.PreProcessRevNo,RevisionNo=a.RevisionNo,
	                b.BookingNo,b.BookingDate,b.Remarks,b.StyleNo,b.ExportOrderID,b.SLNo,b.StyleMasterID,
	                b.BuyerID,b.BuyerTeamID,CompanyID=b.ExecutionCompanyID,b.SupplierID,b.SeasonID,a.WithoutOB,b.InHouseDate,BookingQty= SUM(c.RequiredQty),
                    IsSample = 1
                    FROM {TableNames.FabricBookingAcknowledge} A
				    Inner Join {DbNames.EPYSL}..SampleBookingMaster b on b.BookingID = a.BookingID
					INNER JOIN AllBookingID ABI ON ABI.BookingID=b.BookingID
					Inner Join {DbNames.EPYSL}..SampleBookingConsumptionChild c ON c.BookingID = a.BookingID
					Group By a.BookingID,a.BOMMasterID,a.ItemGroupID,a.SubGroupID,a.Status,a.PreProcessRevNo,a.RevisionNo,
	                b.BookingNo,b.BookingDate,b.Remarks,b.StyleNo,b.ExportOrderID,b.SLNo,b.StyleMasterID,
	                b.BuyerID,b.BuyerTeamID,b.ExecutionCompanyID,b.SupplierID,b.SeasonID,a.WithoutOB,b.InHouseDate
                ),
				FBC AS
                (
	                SELECT *FROM FBA1
	                UNION
	                SELECT *FROM FBA2
                )
                ,EOPT AS
                (
	                Select BM.ExportOrderID, ExecutionCompanyID = Min(E.ExecutionCompanyID)
	                From FBA BM
	                Inner Join {DbNames.EPYSL}..ExportOrderPOTechPack E On E.ExportOrderID = BM.ExportOrderID
	                Group By BM.ExportOrderID
                ),
                 --,CE As (
	               -- Select EOPT.ExportOrderID, CE.CompanyName, CE.ShortName
	               -- From EOPT
	               -- Inner Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = EOPT.ExecutionCompanyID 
	               -- Group By EOPT.ExportOrderID, CE.CompanyName, CE.ShortName
                --),
				EC AS (
					SELECT TOP(1)PO.ExecutionCompanyID,PO.ExportOrderID
					FROM FBC
					INNER JOIN {DbNames.EPYSL}..ExportOrderPO PO ON PO.ExportOrderID = FBC.ExportOrderID
				)
                Select FBA.FBAckID, BookingID = FBA.BookingID,BookingQty=SBM.BookingQty, BookingDate = convert(date,FBA.BookingDate), FBA.IsSample, 0 BOMMasterID, EM.StyleMasterID, 'Fabric' GroupName, 
                EM.ExportOrderID, FBA.BookingNo, EM.ExportOrderNo, FBA.BuyerID, FBA.BuyerTeamID, CompanyID = S.MappingCompanyID, 
                FBA.SubGroupID, CCT.TeamName BuyerDepartment, EMP.EmployeeName MerchandiserName, 
                YearName, EM.CalendarDays  As TNADays, 1 RevStatus, CTS.SeasonName, SBM.SLNo, SBM.Remarks,
                BuyerName = ISNULL(CTO.ShortName,''), BuyerTeamName = ISNULL(CCT.TeamName,''), CompanyName = CASE WHEN ISNULL(SBM.IsSample,0) = 0 THEN CE.CompanyName ELSE CES.CompanyName END,
                SBM.StyleNo, SupplierName = S.ShortName, TNACalendarDays = EM.CalendarDays, RequiredFabricDeliveryDate = Max(SBM.InHouseDate),TL.EmployeeName TeamLeader
                from FBA
                LEFT Join {DbNames.EPYSL}..ExportOrderMaster EM On EM.ExportOrderID = FBA.ExportOrderID
                LEFT Join {DbNames.EPYSL}..FinancialYear FY On FBA.BookingDate between FY.StartMonth and FY.EndMonth
                LEFT JOIN {TableNames.Buyer_Team_Wise_TeamLeader_Setup} BTL ON BTL.BuyerTeamID=FBA.BuyerTeamID
				LEFT JOIN {DbNames.EPYSL}..Employee TL ON TL.EmployeeCode=BTL.TeamLeaderEmployeeCode
                LEFT Join {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = FBA.AddedBy
                LEFT Join {DbNames.EPYSL}..Employee EMP ON EMP.EmployeeCode = LU.EmployeeCode
                --LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = FBA.BookingID
                --LEFT JOIN {DbNames.EPYSL}..BookingMaster BM ON BM.BookingID = FBA.BookingID
                LEFT Join FBC SBM ON SBM.BookingID = FBA.BookingID
                LEFT Join {DbNames.EPYSL}..OrderBankMaster OBM On OBM.StyleMasterID = EM.StyleMasterID
                LEFT Join {DbNames.EPYSL}..StyleMaster SM ON SM.StyleMasterID = OBM.StyleMasterID
                LEFT Join {DbNames.EPYSL}..ContactSeason CTS On CTS.SeasonID = SBM.SeasonID
                LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = FBA.BuyerID
                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = FBA.BuyerTeamID
                LEFT Join {DbNames.EPYSL}..Contacts S On S.ContactID = FBA.SupplierID
                --Left Join CE On CE.ExportOrderID = EM.ExportOrderID
				Left Join EC On EC.ExportOrderID = EM.ExportOrderID 
				LEFT JOIN {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = EC.ExecutionCompanyID 
				LEFT JOIN {DbNames.EPYSL}..CompanyEntity CES On CES.CompanyID = SBM.CompanyID 
                Group By FBA.FBAckID, FBA.BookingID,SBM.BookingQty, convert(date,FBA.BookingDate), FBA.IsSample, EM.StyleMasterID, EM.ExportOrderID, FBA.BookingNo, EM.ExportOrderNo, 
                FBA.BuyerID, FBA.BuyerTeamID, FBA.SubGroupID, CCT.TeamName, EMP.EmployeeName, 
                YearName,EM.CalendarDays, CTS.SeasonName, SBM.SLNo,ISNULL(CTO.ShortName,''),ISNULL(CCT.TeamName,''),CE.CompanyName, SBM.Remarks,
                SBM.StyleNo, S.ShortName,S.MappingCompanyID,TL.EmployeeName,CASE WHEN ISNULL(SBM.IsSample,0) = 0 THEN CE.CompanyName ELSE CES.CompanyName END

                --Seq 2
                ;WITH 
                FBAC AS
                (
	                Select SBC.BookingID
	                , SBC.BookingChildID,SBC.ItemGroupID, SBC.SubGroupID, SBC.ItemMasterID
	                , U.RelativeFactor,
	                ISG.SubGroupName, ConsumptionQty = Ceiling(Sum(SBC.ConsumptionQty)),  SBC.ConsumptionID, SBC.MachineTypeId,SBC.TechnicalNameID,
	                ColorID = CASE WHEN SBC.SubGroupID = 1 THEN IM.Segment3ValueID ELSE IM.Segment5ValueID END,
	                BookingUnitID = (CASE WHEN SBC.SubGroupID = 1 THEN SBC.BookingUnitID ELSE 28 END),
	                BookingUOM = CASE WHEN SBC.SubGroupID = 1 THEN  U.DisplayUnitDesc ELSE 'KG' END, 
	                BookingQty = Ceiling(Sum(SBC.BookingQty)),
	                RequisitionQty = Ceiling(Sum(SBC.BookingQty)),SBC.Remarks,

	                SBC.LengthYds,SBC.LengthInch,SBC.FUPartID, 

	                YarnTypeID = SBC.A1ValueID,
	                YarnType = ISVA1.SegmentValue,
	                YarnBrandID = SBC.YarnBrandID,
	                YarnBrand = ETV.ValueName,
	                YarnProgram = ETV.ValueName,
	                Instruction = CASE WHEN M.IsSample = 0 THEN BC.Remarks ELSE SC.Remarks END,
	                PartName = FUP.PartName,
                    LabDipNo = SBC.LabDipNo,

	                ForTechPack = Convert(Varchar(50),''),

	                ISourcing = Convert(bit,1), ISourcingName = 'In-House', ContactName = '',ContactID = 0,
	                BlockBookingQty = Convert(decimal,0), AdjustQty = Convert(decimal,0), 
	                AutoAgree = Convert(bit,0),Price = convert(decimal,0), SuggestedPrice = convert(decimal,0), 
	                IsCompleteReceive = Convert(bit,'0'), IsCompleteDelivery = Convert(bit,'0'), M.BuyerID, M.BuyerTeamID, M.ExportOrderID,
                    ConstructionID = ISV1.SegmentValueID,
	                Construction = ISV1.SegmentValue, 
	                Composition = ISV2.SegmentValue,
	                Color = CASE WHEN SBC.SubGroupID = 1 THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END, 
	                Gsm = CASE WHEN SBC.SubGroupID = 1 THEN ISV4.SegmentValue ELSE '' END, 
	                DyeingType = CASE WHEN SBC.SubGroupID = 1 THEN ISV6.SegmentValue ELSE '' END, 
	                KnittingType = CASE WHEN SBC.SubGroupID = 1 THEN ISV7.SegmentValue ELSE '' END,
	                Length = CASE WHEN SBC.SubGroupID = 1 THEN 0 ELSE CONVERT(decimal(18,2),ISV3.SegmentValue) END,
	                Width = CASE WHEN SBC.SubGroupID = 1 THEN 0 ELSE CONVERT(decimal(18,2),ISV4.SegmentValue) END,
	                FabricWidth = CASE WHEN SBC.SubGroupID = 1 THEN ISV5.SegmentValue ELSE '' END
	
	                FROM {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} SBC
	                INNER JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} M On M.FBAckID = SBC.AcknowledgeID AND SBC.BookingID = M.BookingID
	                LEFT JOIN {DbNames.EPYSL}..BookingChild BC ON BC.BookingID = M.BookingID AND BC.ItemMasterID = SBC.ItemMasterID AND BC.ConsumptionID = SBC.ConsumptionID
	                LEFT JOIN {DbNames.EPYSL}..SampleBookingConsumption SC ON SC.BookingID = M.BookingID AND SC.ConsumptionID = SBC.ConsumptionID
	                LEFT Join {DbNames.EPYSL}..Unit U On U.UnitID = SBC.BookingUnitID
	                INNER Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SBC.SubGroupID
	                Left Join {DbNames.EPYSL}..ItemSegmentValue ISVA1 On ISVA1.SegmentValueID = SBC.A1ValueID
	                Left Join {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = SBC.YarnBrandID
	                Left Join {DbNames.EPYSL}..FabricUsedPart FUP On FUP.FUPartID = SBC.FUPartID
	                Left Join {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = SBC.ItemMasterID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
	                Where M.BookingNo = '{bookingNo}' AND SBC.IsDeleted=0 AND SBC.BookingQty > 0                        
	                GROUP BY SBC.BookingID
	                , SBC.BookingChildID,SBC.ItemGroupID, SBC.SubGroupID, SBC.ItemMasterID
	                , U.RelativeFactor,
	                ISG.SubGroupName, SBC.ConsumptionID, SBC.MachineTypeId,SBC.TechnicalNameID,
	                CASE WHEN SBC.SubGroupID = 1 THEN IM.Segment3ValueID ELSE IM.Segment5ValueID END,
	                CASE WHEN SBC.SubGroupID = 1 THEN SBC.BookingUnitID ELSE 28 END,
	                CASE WHEN SBC.SubGroupID = 1 THEN  U.DisplayUnitDesc ELSE 'KG' END, 
	                SBC.Remarks,
	                SBC.LengthYds,SBC.LengthInch,SBC.FUPartID, 
	                M.BuyerID, M.BuyerTeamID, M.ExportOrderID,
	                ISV1.SegmentValueID,
	                ISV1.SegmentValue, 
	                ISV2.SegmentValue,
	                CASE WHEN SBC.SubGroupID = 1 THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END, 
	                CASE WHEN SBC.SubGroupID = 1 THEN ISV4.SegmentValue ELSE '' END, 
	                CASE WHEN SBC.SubGroupID = 1 THEN ISV6.SegmentValue ELSE '' END, 
	                CASE WHEN SBC.SubGroupID = 1 THEN ISV7.SegmentValue ELSE '' END,
	                CASE WHEN SBC.SubGroupID = 1 THEN 0 ELSE CONVERT(decimal(18,2),ISV3.SegmentValue) END,
	                CASE WHEN SBC.SubGroupID = 1 THEN 0 ELSE CONVERT(decimal(18,2),ISV4.SegmentValue) END,
	                CASE WHEN SBC.SubGroupID = 1 THEN ISV5.SegmentValue ELSE '' END,

					SBC.A1ValueID,
	                ISVA1.SegmentValue,
	                SBC.YarnBrandID,
	                ETV.ValueName,
	                ETV.ValueName,
	                CASE WHEN M.IsSample = 0 THEN BC.Remarks ELSE SC.Remarks END,
	                FUP.PartName,
                    SBC.LabDipNo
                ),
                FinalList AS
                (
	                SELECT FBAC.BookingID, FBAC.BookingChildID, FBAC.ItemGroupID, FBAC.SubGroupID, FBAC.ItemMasterID
	                , FBAC.SubGroupName,
	                BookingUnitID,LengthYds,LengthInch,FUPartID,YarnBrandID,
	                LabDipNo,Remarks,FBAC.YarnTypeID,FBAC.YarnType,YarnBrand,PartName, FBAC.BuyerID, FBAC.
	                BuyerTeamID, FBAC.ExportOrderID, FBAC.ConsumptionID, FBAC.MachineTypeId,FBAC.TechnicalNameID, FBAC.ConsumptionQty, FBAC.BookingUOM, FBAC.RequisitionQty,
	                FBAC.ForTechPack, FBAC.ISourcing, FBAC.ISourcingName, FBAC.ContactName, FBAC.ContactID, FBAC.BlockBookingQty, FBAC.AdjustQty,
	                AutoAgree, FBAC.Price, FBAC.SuggestedPrice, FBAC.IsCompleteReceive, FBAC.IsCompleteDelivery, FBAC.BookingQty,
	                ColorID, FBAC.Construction, FBAC.ConstructionID, FBAC.Composition, FBAC.Color, FBAC.Gsm, FBAC.DyeingType, FBAC.KnittingType, FBAC.Length, FBAC.Width,
	                RS.RefSourceID, RS.RefSourceNo, RS.SourceConsumptionID, SourceItemMasterID = RS.ItemMasterID, FBAC.FabricWidth, T.TechnicalName, KMS.SubClassName MachineType,FBAC.Instruction, FBAC.YarnProgram
                    FROM FBAC
                    LEFT JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} KMS ON KMS.SubClassID = FBAC.MachineTypeId
                    LEFT JOIN {TableNames.FabricTechnicalName} T ON T.TechnicalNameId = FBAC.TechnicalNameID
	                LEFT JOIN {DbNames.EPYSL}..BookingChildReferenceSource RS ON RS.BookingID = FBAC.BookingID AND RS.ConsumptionID = FBAC.ConsumptionID
	                Group By FBAC.BookingID, FBAC.BookingChildID, FBAC.ItemGroupID, FBAC.SubGroupID, FBAC.ItemMasterID
	                , FBAC.SubGroupName,
	                BookingUnitID,LengthYds,LengthInch,FUPartID,YarnBrandID,
	                LabDipNo,Remarks,FBAC.YarnType,YarnBrand,PartName, FBAC.BuyerID, FBAC.BuyerTeamID, FBAC.ExportOrderID, FBAC.ConsumptionID, FBAC.MachineTypeId,FBAC.TechnicalNameID, FBAC.ConsumptionQty, FBAC.BookingUOM, FBAC.RequisitionQty,
	                FBAC.YarnTypeID, FBAC.ForTechPack, FBAC.ISourcing, FBAC.ISourcingName, FBAC.ContactName, FBAC.ContactID, FBAC.BlockBookingQty, FBAC.AdjustQty,
	                AutoAgree, FBAC.Price, FBAC.SuggestedPrice, FBAC.IsCompleteReceive, FBAC.IsCompleteDelivery, FBAC.BookingQty,
	                ColorID, FBAC.Construction, FBAC.ConstructionID, FBAC.Composition, FBAC.Color, FBAC.Gsm, FBAC.DyeingType, FBAC.KnittingType, FBAC.Length, FBAC.Width, FBAC.RelativeFactor
                    ,RS.RefSourceID, RS.RefSourceNo, RS.SourceConsumptionID, RS.ItemMasterID, FBAC.FabricWidth, T.TechnicalName, KMS.SubClassName,FBAC.Instruction, FBAC.YarnProgram
                )
                SELECT * FROM FinalList;

                --Seq 3
                --Shade book
                {CommonQueries.GetYarnShadeBooks()};

                --Seq 4
                -- YarnBookingChildItemYarnSubBrand
                Select ETV.ValueID id, ETV.ValueName [text]
                From {DbNames.EPYSL}..EntityTypeValue ETV
                Inner Join {DbNames.EPYSL}..EntityType ET On ET.EntityTypeID = ETV.EntityTypeID
                Where ET.EntityTypeName = 'Yarn Sub Brand'
                Order By ETV.ValueName;

                --Seq 5
                --Technical Name
                SELECT Cast(T.TechnicalNameId As varchar) id, T.TechnicalName [text], ISNULL(ST.[Days], 0) [desc], Cast(SC.SubClassID as varchar) additionalValue
                FROM {TableNames.FabricTechnicalName} T
                LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME_KMACHINE_SUB_CLASS} SC ON SC.TechnicalNameID = T.TechnicalNameId
                LEFT JOIN {TableNames.KNITTING_MACHINE_STRUCTURE_TYPE_HK} ST ON ST.StructureTypeID = SC.StructureTypeID
                Group By T.TechnicalNameId, T.TechnicalName, ST.Days, SC.SubClassID;

                --Seq 6
                --M/c type
                ;SELECT CAST(a.MachineSubClassID AS varchar) [id], b.SubClassName [text]
                FROM {TableNames.KNITTING_MACHINE} a
                INNER JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} b ON b.SubClassID = a.MachineSubClassID
                Inner JOIN {TableNames.KNITTING_MACHINE_TYPE} c On c.TypeID = b.TypeID
                --Where c.TypeName != 'Flat Bed'
                GROUP BY a.MachineSubClassID, b.SubClassName, b.TypeID, c.TypeName;

                --Seq 7
                --M/c type
                ;SELECT CAST(a.MachineSubClassID AS varchar) [id], b.SubClassName [text], b.TypeID [desc], c.TypeName additionalValue
                FROM {TableNames.KNITTING_MACHINE} a
                INNER JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} b ON b.SubClassID = a.MachineSubClassID
                Inner JOIN {TableNames.KNITTING_MACHINE_TYPE} c On c.TypeID = b.TypeID
                --Where c.TypeName != 'Flat Bed'
                GROUP BY a.MachineSubClassID, b.SubClassName, b.TypeID, c.TypeName;

                --Seq 8
                --CriteriaNames
                ;SELECT CriteriaName,CriteriaSeqNo,(CASE WHEN CriteriaName  IN('Batch Preparation','Quality Check') THEN '1'ELSE'0'END) AS TotalTime 
                FROM {TableNames.BDS_CRITERIA_HK} --WHERE CriteriaName NOT IN('Batch Preparation','Testing')
                GROUP BY CriteriaSeqNo,CriteriaName order by CriteriaSeqNo,CriteriaName;

                --Seq 9
                --FBAChildPlannings
                ;SELECT * FROM {TableNames.BDS_CRITERIA_HK} order by CriteriaSeqNo, OperationSeqNo, CriteriaName;

                --Seq 10
                --Spinner
                {CommonQueries.GetYarnSpinners()}

                --Seq 11
                --Yarn Childs for suggestion
                /*;WITH
                CRS AS
                (
	                SELECT BookingNo = CRS.RefSourceNo, CRS.ConsumptionID, CRS.ItemMasterID
	                FROM {DbNames.EPYSL}..BookingChildReferenceSource CRS
	                LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = CRS.BookingID
	                WHERE FBA.BookingNo = '{bookingNo}'
                )
                SELECT YBChildID = MRC.FCMRChildID,YBookingID = MRC.FCMRMasterID, FCM.ConsumptionID, FCM.ItemMasterID, YarnTypeID = 0, FCM.FUPartID, BookingUnitID = 0,
                MRC.BookingQty,FCM.QtyInKG,FCM.ExcessPercentage,FCM.ExcessQty,FCM.ExcessQtyInKG,FCM.TotalQty,FCM.TotalQtyInKG,
                FCM.BookingChildID,GreyReqQty = 0,GreyLeftOverQty = 0,GreyProdQty = 0,FCM.SubGroupID,
                IM.Segment1ValueID,IM.Segment2ValueID,IM.Segment3ValueID,IM.Segment4ValueID,
                IM.Segment5ValueID,IM.Segment6ValueID,IM.Segment7ValueID,
                Segment1ValueDesc = ISV1.SegmentValue, Segment2ValueDesc = ISV2.SegmentValue,
                Segment3ValueDesc = ISV3.SegmentValue, Segment4ValueDesc = ISV4.SegmentValue,
                Segment5ValueDesc = ISV5.SegmentValue, Segment6ValueDesc = ISV6.SegmentValue,
                Segment7ValueDesc = ISV7.SegmentValue

                FROM {TableNames.RND_FREE_CONCEPT_MR_CHILD} MRC
                INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR ON MR.FCMRMasterID = MRC.FCMRMasterID
                INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = MR.ConceptID
                INNER JOIN CRS RS ON RS.BookingNo = FCM.GroupConceptNo
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = MRC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                GROUP BY MRC.FCMRChildID, MRC.FCMRMasterID, FCM.ConsumptionID, FCM.ItemMasterID, FCM.FUPartID, 
                MRC.BookingQty,FCM.QtyInKG,FCM.ExcessPercentage,FCM.ExcessQty,FCM.ExcessQtyInKG,FCM.TotalQty,FCM.TotalQtyInKG,
                FCM.BookingChildID,FCM.SubGroupID,IM.Segment1ValueID,IM.Segment2ValueID,IM.Segment3ValueID,IM.Segment4ValueID,
                IM.Segment5ValueID,IM.Segment6ValueID,IM.Segment7ValueID,            
                ISV1.SegmentValue,ISV2.SegmentValue,ISV3.SegmentValue,ISV4.SegmentValue,ISV5.SegmentValue,ISV6.SegmentValue,ISV7.SegmentValue;
                */
                ;WITH
                CRS AS
                (
	                SELECT BookingNo = CRS.RefSourceNo, CRS.ConsumptionID, CRS.ItemMasterID,CRS.BookingID
	                FROM {DbNames.EPYSL}..BookingChildReferenceSource CRS
	                LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = CRS.BookingID
	                WHERE FBA.BookingNo = '242146-FBR'
                )
                SELECT YBChildID = MRC.FCMRChildID,YBookingID = MRC.FCMRMasterID, FCM.ConsumptionID, FCM.ItemMasterID, YarnTypeID = 0, FCM.FUPartID, BookingUnitID = 0,
                MRC.BookingQty,FCM.QtyInKG,FCM.ExcessPercentage,FCM.ExcessQty,FCM.ExcessQtyInKG,FCM.TotalQty,FCM.TotalQtyInKG,
                FCM.BookingChildID,GreyReqQty = 0,GreyLeftOverQty = 0,GreyProdQty = 0,FCM.SubGroupID,
                IM.Segment1ValueID,IM.Segment2ValueID,IM.Segment3ValueID,IM.Segment4ValueID,
                IM.Segment5ValueID,IM.Segment6ValueID,IM.Segment7ValueID,
                Segment1ValueDesc = ISV1.SegmentValue, Segment2ValueDesc = ISV2.SegmentValue,
                Segment3ValueDesc = ISV3.SegmentValue, Segment4ValueDesc = ISV4.SegmentValue,
                Segment5ValueDesc = ISV5.SegmentValue, Segment6ValueDesc = ISV6.SegmentValue,
                Segment7ValueDesc = ISV7.SegmentValue

                FROM {TableNames.RND_FREE_CONCEPT_MR_CHILD} MRC
                INNER JOIN {TableNames.RND_FREE_CONCEPT_MR_MASTER} MR ON MR.FCMRMasterID = MRC.FCMRMasterID
                INNER JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = MR.ConceptID
                INNER JOIN CRS RS ON RS.BookingID = FCM.BookingID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = MRC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                GROUP BY MRC.FCMRChildID, MRC.FCMRMasterID, FCM.ConsumptionID, FCM.ItemMasterID, FCM.FUPartID, 
                MRC.BookingQty,FCM.QtyInKG,FCM.ExcessPercentage,FCM.ExcessQty,FCM.ExcessQtyInKG,FCM.TotalQty,FCM.TotalQtyInKG,
                FCM.BookingChildID,FCM.SubGroupID,IM.Segment1ValueID,IM.Segment2ValueID,IM.Segment3ValueID,IM.Segment4ValueID,
                IM.Segment5ValueID,IM.Segment6ValueID,IM.Segment7ValueID,            
                ISV1.SegmentValue,ISV2.SegmentValue,ISV3.SegmentValue,ISV4.SegmentValue,ISV5.SegmentValue,ISV6.SegmentValue,ISV7.SegmentValue;
                --Seq 12
                --SpinnerList
                 {CommonQueries.GetYarnSpinners()}

                --Seq 13
                SELECT DISTINCT id = KM.GG, [text] = KM.GG
                FROM {TableNames.KNITTING_MACHINE} KM
                WHERE KM.GG > 0
                GROUP BY KM.GG
                ORDER BY KM.GG;

                --Seq 14
                SELECT id = KM.Dia, [text] = KM.Dia, additionalValue = KM.GG
                FROM {TableNames.KNITTING_MACHINE} KM
                WHERE KM.GG > 0
                GROUP BY KM.Dia, KM.GG
                ORDER BY KM.GG;

            --Item Segments
              /*{CommonQueries.GetItemSegmentValuesBySegmentNamesWithSegmentName()}*/;
            {CommonQueries.GetCertifications()};
            --Fabric Components
                /*{CommonQueries.GetEntityTypeValuesOnly(EntityTypeNameConstants.FABRIC_TYPE)};*/
                  {CommonQueries.GetFabricComponents(EntityTypeNameConstants.FABRIC_TYPE)};

            --Shade book
                {CommonQueries.GetYarnShadeBooks()}

            --Item Segments
                {CommonQueries.GetSubPrograms()}; 

            --Color Wise Size Collar
                {CommonQueries.GetColorWiseSizeCollar(bookingNo)}; 

            --Color Wise Size Cuff
                {CommonQueries.GetColorWiseSizeCuff(bookingNo)}; 

            --Color Wise All Size Collar
                {CommonQueries.GetAllColorWiseSizeCollar(bookingNo)}; 

            --Color Wise All Size Cuff
                {CommonQueries.GetAllColorWiseSizeCuff(bookingNo)}; 

            -- Machine Brand
            SELECT KM.KnittingMachineID,KM.KnittingUnitID,KM.MachineNo,KM.MachineTypeID,KM.MachineSubClassID,
            KM.GG,KM.Dia,KM.BrandID,KM.Capacity,KU.ShortName AS Contact,EV.ValueName AS Brand,
            IsSubContact = 0
            FROM {TableNames.KNITTING_MACHINE} KM
            Left JOIN {TableNames.KNITTING_UNIT} KU ON KU.KnittingUnitID = KM.KnittingUnitID
            Left Join {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = KM.BrandID
            Left Join {DbNames.EPYSL}..Contacts C ON C.ContactID = KU.ContactID
            Left Join {DbNames.EPYSL}..ContactAdditionalInfo CAI ON CAI.ContactID = C.ContactID
            ;--Where CAI.InHouse = 1;

            -- Machine Brand Collar/Cuff
            SELECT KM.BrandID,EV.ValueName AS Brand
            FROM {TableNames.KNITTING_MACHINE} KM
            Left JOIN {TableNames.KNITTING_UNIT} KU ON KU.KnittingUnitID = KM.KnittingUnitID
            Left Join {DbNames.EPYSL}..EntityTypeValue EV ON EV.ValueID = KM.BrandID
            Left Join {DbNames.EPYSL}..Contacts C ON C.ContactID = KU.ContactID
            Left Join {DbNames.EPYSL}..ContactAdditionalInfo CAI ON CAI.ContactID = C.ContactID
            --Where CAI.InHouse = 1
            GROUP BY KM.BrandID,EV.ValueName
            ORDER BY EV.ValueName;

            --Fiber-SubProgram-Certifications Mapping Setup
            Select * FROM {DbNames.EPYSL}..FabricComponentMappingSetup
            ";
            try
            {//--
                await _connection.OpenAsync();
                //var records = await _connection.QueryMultipleAsync(sql);
                var records = await _connection.QueryMultipleAsync(sql, segmentNames);
                YarnBookingMaster yarnBookingMaster = new YarnBookingMaster();
                List<YarnBookingMaster> data = records.Read<YarnBookingMaster>().ToList();
                Guard.Against.NullObject(data);
                yarnBookingMaster = data != null ? data.FirstOrDefault() : new YarnBookingMaster();

                var childs = records.Read<YarnBookingChild>().ToList();
                List<Select2OptionModel> yarnShadeBooks = records.Read<Select2OptionModel>().ToList();
                yarnBookingMaster.YarnShadeBooks = yarnShadeBooks;
                List<Select2OptionModel> yarnSubBrandList = records.Read<Select2OptionModel>().ToList();
                yarnBookingMaster.TechnicalNameList = await records.ReadAsync<Select2OptionModel>();
                yarnBookingMaster.KnittingMachines = records.Read<KnittingMachine>().ToList();

                List<Select2OptionModel> mcTypeList = records.Read<Select2OptionModel>().ToList();
                yarnBookingMaster.MCTypeForFabricList = mcTypeList.Where(x => x.additionalValue != "Flat Bed").ToList();
                yarnBookingMaster.MCTypeForOtherList = mcTypeList.Where(x => x.additionalValue == "Flat Bed").ToList();

                List<FBookingAcknowledgeChild> criteriaNames = records.Read<FBookingAcknowledgeChild>().ToList();
                List<FBAChildPlanning> fbaChildPlannings = records.Read<FBAChildPlanning>().ToList();

                List<Select2OptionModel> spinners = records.Read<Select2OptionModel>().ToList();
                List<YarnBookingChild> suggestedChilds = records.Read<YarnBookingChild>().ToList();
                var spinnerList = records.Read<Select2OptionModel>().ToList();
                spinnerList.Insert(0, new Select2OptionModel()
                {
                    id = 0.ToString(),
                    text = "N/A"
                });
                yarnBookingMaster.GaugeList = records.Read<Select2OptionModel>().ToList();
                yarnBookingMaster.DiaList = records.Read<Select2OptionModel>().ToList();

                List<YarnBookingChildItem> childItems = new List<YarnBookingChildItem>();

                int maxCol = 999;
                data.ForEach(m =>
                {
                    m.SpinnerList = spinnerList;

                    m.Childs = childs.Where(c => c.YBookingID == m.YBookingID).ToList();
                    m.Childs.ForEach(c =>
                    {
                        c.CriteriaNames = criteriaNames;
                        c.FBAChildPlannings = fbaChildPlannings;

                        c.ChildItems = new List<YarnBookingChildItem>();
                        List<YarnBookingChild> sChilds = suggestedChilds.Where(x => x.ConsumptionID == c.SourceConsumptionID && x.SubGroupId == c.SubGroupId).ToList();
                        sChilds.ForEach(childData =>
                        {
                            YarnBookingChildItem childItem = new YarnBookingChildItem();
                            childItem.YBChildItemID = maxCol++;
                            childItem.YBChildID = c.YBChildID; //childData.BookingChildID;
                            childItem.BookingChildID = c.BookingChildID;
                            childItem.ConsumptionID = c.ConsumptionID;
                            childItem.StitchLength = c.StitchLength;
                            childItem.Distribution = (100 / sChilds.Count());
                            childItem.Allowance = 0;
                            childItem.YDItem = false;

                            childItem.Segment1ValueId = childData.Segment1ValueId;
                            childItem.Segment2ValueId = childData.Segment2ValueId;
                            childItem.Segment3ValueId = childData.Segment3ValueId;
                            childItem.Segment4ValueId = childData.Segment4ValueId;
                            childItem.Segment5ValueId = childData.Segment5ValueId;
                            childItem.Segment6ValueId = childData.Segment6ValueId;
                            childItem.Segment7ValueId = childData.Segment7ValueId;

                            childItem.Segment1ValueDesc = childData.Segment1ValueDesc;
                            childItem.Segment2ValueDesc = childData.Segment2ValueDesc;
                            childItem.Segment3ValueDesc = childData.Segment3ValueDesc;
                            childItem.Segment4ValueDesc = childData.Segment4ValueDesc;
                            childItem.Segment5ValueDesc = childData.Segment5ValueDesc;
                            childItem.Segment6ValueDesc = childData.Segment6ValueDesc;
                            childItem.Segment7ValueDesc = childData.Segment7ValueDesc;

                            c.ChildItems.Add(childItem);
                        });
                    });

                    yarnBookingMaster.Fabrics = m.Childs.Where(x => x.SubGroupId == 1).ToList();
                    if (yarnBookingMaster.Fabrics.Count() > 0)
                    {
                        yarnBookingMaster.HasFabric = true;

                        yarnBookingMaster.Fabrics[0].YarnShadeBooks = yarnShadeBooks;
                        yarnBookingMaster.Fabrics[0].YarnSubBrandList = yarnSubBrandList;
                        yarnBookingMaster.Fabrics[0].Spinners = spinners;
                    }

                    yarnBookingMaster.Collars = m.Childs.Where(x => x.SubGroupId == 11).ToList();
                    if (yarnBookingMaster.Collars.Count() > 0)
                    {
                        yarnBookingMaster.HasCollar = true;

                        yarnBookingMaster.Collars[0].YarnShadeBooks = yarnShadeBooks;
                        yarnBookingMaster.Collars[0].YarnSubBrandList = yarnSubBrandList;
                        yarnBookingMaster.Collars[0].Spinners = spinners;
                    }

                    yarnBookingMaster.Cuffs = m.Childs.Where(x => x.SubGroupId == 12).ToList();
                    if (yarnBookingMaster.Cuffs.Count() > 0)
                    {
                        yarnBookingMaster.HasCuff = true;

                        yarnBookingMaster.Cuffs[0].YarnShadeBooks = yarnShadeBooks;
                        yarnBookingMaster.Cuffs[0].YarnSubBrandList = yarnSubBrandList;
                        yarnBookingMaster.Cuffs[0].Spinners = spinners;
                    }
                });

                if (!yarnBookingMaster.BookingNo.IsNullOrEmpty())
                {
                    //223213-FBR-Add-1
                    if (yarnBookingMaster.BookingNo.ToUpper().IndexOf("-ADD-") > -1)
                    {
                        string[] spliteSLNo = yarnBookingMaster.BookingNo.Split('-');
                        if (spliteSLNo.Length == 4)
                        {
                            yarnBookingMaster.SLNo = spliteSLNo[3] + "A" + spliteSLNo[0];
                        }
                    }
                    else
                    {
                        yarnBookingMaster.SLNo = yarnBookingMaster.ExportOrderNo;
                    }
                }

                var _recvCertifications = await records.ReadAsync<Select2OptionModelExtended>();
                yarnBookingMaster.Certifications = _recvCertifications.Where(x => x.desc == ItemSegmentNameConstants.YARN_CERTIFICATIONS);
                yarnBookingMaster.FabricComponentsNew = await records.ReadAsync<Select2OptionModel>();
                yarnBookingMaster.YarnShadeBooks = await records.ReadAsync<Select2OptionModel>();
                var itemSegments = await records.ReadAsync<Select2OptionModelExtended>();
                yarnBookingMaster.YarnSubProgramNews = itemSegments.Where(x => x.desc == ItemSegmentNameConstants.YARN_SUBPROGRAM_NEW);
                yarnBookingMaster.IsSample = isSample;

                yarnBookingMaster.CollarSizeList = records.Read<Select2OptionModel>().ToList();
                yarnBookingMaster.CuffSizeList = records.Read<Select2OptionModel>().ToList();
                yarnBookingMaster.AllCollarSizeList = records.Read<FBookingAcknowledgeChild>().ToList();
                yarnBookingMaster.AllCuffSizeList = records.Read<FBookingAcknowledgeChild>().ToList();

                yarnBookingMaster.MachineBrandList = records.Read<KnittingMachine>().ToList();
                yarnBookingMaster.CollarCuffBrandList = records.Read<KnittingMachine>().ToList();
                yarnBookingMaster.FabricComponentMappingSetupList = records.Read<FabricComponentMappingSetup>().ToList();
                #region Grouping Collar Cuff
                //Collar
                List<YarnBookingChild> tempChildsCollar = CommonFunction.DeepClone(childs.Where(x => x.SubGroupId == 11).ToList());
                var disList = tempChildsCollar.Select(m => new { m.Construction, m.Composition, m.Color })
                        .Distinct()
                        .ToList();

                List<YarnBookingChild> tempChilds = new List<YarnBookingChild>();
                int consumptionID = 1;
                disList.ForEach(c =>
                {
                    var tempChilds1 = tempChildsCollar.Where(x => x.Construction == c.Construction && x.Composition == c.Composition && x.Color == c.Color).ToList();
                    var firstChild = tempChilds1.First();

                    tempChilds.Add(new YarnBookingChild()
                    {
                        ConsumptionID = consumptionID++,
                        BookingID = firstChild.BookingID,
                        //BookingChildID = firstChild.BookingChildID,

                        SubGroupId = firstChild.SubGroupId,
                        SubGroupName = firstChild.SubGroupName,

                        ConstructionID = firstChild.ConstructionID,
                        Construction = firstChild.Construction,

                        CompositionID = firstChild.CompositionID,
                        Composition = firstChild.Composition,

                        MachineTypeId = firstChild.MachineTypeId,
                        MachineType = firstChild.MachineType,

                        TechnicalNameId = firstChild.TechnicalNameId,
                        TechnicalName = firstChild.TechnicalName,

                        YarnAllowance = firstChild.YarnAllowance,

                        ColorID = firstChild.ColorID,
                        Color = firstChild.Color,

                        YarnTypeID = firstChild.YarnTypeID,
                        YarnType = firstChild.YarnType,

                        YarnProgram = firstChild.YarnProgram,
                        DyeingType = firstChild.DyeingType,
                        Instruction = firstChild.Instruction,
                        LabDipNo = firstChild.LabDipNo,
                        RefSourceNo = firstChild.RefSourceNo,
                        BookingQty = tempChilds1.Sum(x => x.BookingQty),
                        FinishFabricUtilizationQty = tempChilds1.Sum(x => x.FinishFabricUtilizationQty),
                        ReqFinishFabricQty = tempChilds1.Sum(x => x.ReqFinishFabricQty),
                        TotalQty = tempChilds1.Sum(x => x.TotalQty),
                        GreyReqQty = 0, //tempChilds1.Sum(x => x.GreyReqQty),
                        GreyLeftOverQty = 0, //tempChilds1.Sum(x => x.GreyLeftOverQty),
                        GreyProdQty = 0, //tempChilds1.Sum(x => x.GreyProdQty),

                        YarnShadeBooks = yarnShadeBooks,
                        YarnSubBrandList = yarnSubBrandList,
                        Spinners = spinners
                    });
                });

                //Cuff

                List<YarnBookingChild> tempChildsCuff = CommonFunction.DeepClone(childs.Where(x => x.SubGroupId == 12).ToList());
                disList = tempChildsCuff.Select(m => new { m.Construction, m.Composition, m.Color })
                        .Distinct()
                        .ToList();

                disList.ForEach(c =>
                {
                    var tempChilds1 = tempChildsCuff.Where(x => x.Construction == c.Construction && x.Composition == c.Composition && x.Color == c.Color).ToList();
                    var firstChild = tempChilds1.First();
                    tempChilds.Add(new YarnBookingChild()
                    {
                        ConsumptionID = consumptionID++,
                        BookingID = firstChild.BookingID,
                        //BookingChildID = firstChild.BookingChildID,

                        SubGroupId = firstChild.SubGroupId,
                        SubGroupName = firstChild.SubGroupName,

                        ConstructionID = firstChild.ConstructionID,
                        Construction = firstChild.Construction,

                        CompositionID = firstChild.CompositionID,
                        Composition = firstChild.Composition,

                        MachineTypeId = firstChild.MachineTypeId,
                        MachineType = firstChild.MachineType,

                        TechnicalNameId = firstChild.TechnicalNameId,
                        TechnicalName = firstChild.TechnicalName,

                        YarnAllowance = firstChild.YarnAllowance,

                        ColorID = firstChild.ColorID,
                        Color = firstChild.Color,

                        YarnTypeID = firstChild.YarnTypeID,
                        YarnType = firstChild.YarnType,

                        YarnProgram = firstChild.YarnProgram,
                        DyeingType = firstChild.DyeingType,
                        Instruction = firstChild.Instruction,
                        LabDipNo = firstChild.LabDipNo,
                        RefSourceNo = firstChild.RefSourceNo,
                        BookingQty = tempChilds1.Sum(x => x.BookingQty),
                        FinishFabricUtilizationQty = tempChilds1.Sum(x => x.FinishFabricUtilizationQty),
                        ReqFinishFabricQty = tempChilds1.Sum(x => x.ReqFinishFabricQty),
                        TotalQty = tempChilds1.Sum(x => x.TotalQty),
                        GreyReqQty = tempChilds1.Sum(x => x.GreyReqQty),
                        GreyLeftOverQty = tempChilds1.Sum(x => x.GreyLeftOverQty),
                        GreyProdQty = tempChilds1.Sum(x => x.GreyProdQty),

                        YarnShadeBooks = yarnShadeBooks,
                        YarnSubBrandList = yarnSubBrandList,
                        Spinners = spinners,

                        CriteriaNames = new List<FBookingAcknowledgeChild>()
                    });
                });
                #endregion

                childs = childs.Where(x => x.SubGroupId == 1).ToList();
                childs.AddRange(tempChilds);

                yarnBookingMaster.BookingQty = childs.Where(x => x.SubGroupId == 1).Sum(x => x.BookingQty);

                yarnBookingMaster.Collars = childs.Where(x => x.SubGroupId == 11).ToList();
                yarnBookingMaster.Cuffs = childs.Where(x => x.SubGroupId == 12).ToList();

                return yarnBookingMaster;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open) _connection.Close();
            }
        }


        /*
        public async Task<YarnBookingMaster> GetYBForBulkAsync(string bookingNo)
        {
            string sql = $@" WITH Q1 AS(
	                              SELECT YBM.*,ExecutionCompanyID=YBM.CompanyID, E.ExportOrderNo,
	                             BM.BookingNo,BM.BookingDate,BuyerName=c.ShortName,BuyerTeamName=CCT.TeamName,CE.CompanyName,StyleNo='',
                                 YBM.PreProcessRevNo PreRevisionNo, FBA.FBAckID
	                             FROM {TableNames.YarnBookingMaster_New} YBM
	                             LEFT JOIN {DbNames.EPYSL}..BookingMaster BM on BM.BookingID=YBM.BookingID
	                             LEFT JOIN {DbNames.EPYSL}..Contacts c On c.ContactID = YBM.BuyerID
	                             LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = YBM.BuyerTeamID
	                             LEFT JOIN {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = YBM.CompanyID
                                 INNER JOIN {DbNames.EPYSL}..ExportOrderMaster E ON E.ExportOrderID = YBM.ExportOrderID
                                 LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = YBM.BookingID
	                             WHERE YBM.WithoutOB=0 AND BM.BookingNo='{bookingNo}'
                             ),
                             Q2 AS(
	                              SELECT YBM.*,ExecutionCompanyID=YBM.CompanyID, E.ExportOrderNo,
	                             BM.BookingNo,BM.BookingDate,BuyerName=c.ShortName,BuyerTeamName=CCT.TeamName,CE.CompanyName,BM.StyleNo,
                                 YBM.PreProcessRevNo PreRevisionNo, FBA.FBAckID
	                             FROM {TableNames.YarnBookingMaster_New} YBM
	                             LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster BM on BM.BookingID=YBM.BookingID
	                             LEFT JOIN {DbNames.EPYSL}..Contacts c On c.ContactID = YBM.BuyerID
	                             LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = YBM.BuyerTeamID
	                             LEFT JOIN {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = YBM.CompanyID
                                 INNER JOIN {DbNames.EPYSL}..ExportOrderMaster E ON E.ExportOrderID = YBM.ExportOrderID
                                 LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID = YBM.BookingID
	                             WHERE YBM.WithoutOB=1 AND BM.BookingNo='{bookingNo}'
                            ),
                            Q AS(
	                            SELECT * FROM Q1
	                            UNION
	                            SELECT * FROM Q2
                            )
                            SELECT * FROM Q;

                            --YarnBookingChild_New 
                            ; WITH C1 AS(
                            SELECT YBC.YBookingID,YBC.ItemMasterID, YBC.ConsumptionID, YBC.YarnTypeID,YBC.YarnBrandID,
	                        YBC.BookingUnitID,BookingQty = SUM(YBC.BookingQty),FTechnicalName=ISNULL(YBC.FTechnicalName,''),YBC.IsCompleteReceive,YBC.IsCompleteDelivery,YBC.QtyInKG,YBC.ExcessPercentage,
	                        YBC.ExcessQtyInKG,YBC.TotalQty,YBC.TotalQtyInKG,
                            YBM.BookingID,ExecutionCompanyID=YBM.CompanyID,YBM.ExportOrderID,Isnull(BCC.TechPackID,0)TechPackID,
                            ConsumptionQty = SUM(BCC.ConsumptionQty),RequisitionQty = SUM(BCC.RequisitionQty), Isnull(BCC.ContactID,0)ContactID,
                            ISV1.SegmentValueID ConstructionId, ISV1.SegmentValue Construction, 
                            ISV2.SegmentValueID CompositionId, ISV2.SegmentValue Composition, 
                            IM.Segment3ValueID ColorID, Color = Case When IM.SubGroupID = 1 Then ISV3.SegmentValue Else '' End,
                            IM.Segment4ValueID GSMId,GSM= Case When IM.SubGroupID = 1 Then ISV4.SegmentValue Else '' End,
                            FabricWidth=BCCC.Segment5Desc,
                            IM.Segment7ValueID KnittingTypeId,KnittingType=BCCC.Segment7Desc,
                            YarnType=ISV.SegmentValue,
                            YarnProgram=ETV.ValueName,
                            DyeingType=BCCC.Segment6Desc,
                            Instruction=BCC.Remarks,
                            FBC.LabDipNo,
                            Length = Case When IM.SubGroupID <> 1 Then ISV3.SegmentValue Else '' End,
                            Height = Case When IM.SubGroupID <> 1 Then ISV4.SegmentValue Else '' End

                            FROM {TableNames.YarnBookingChild_New} YBC
                            LEFT JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID=YBC.YBookingID
                            LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID=YBM.BookingID
                            LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.AcknowledgeID=FBA.FBAckID
                            LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = YBC.YarnBrandID
                            LEFT JOIN {DbNames.EPYSL}..BookingMaster BM on BM.BookingID=YBM.BookingID
                            LEFT JOIN {DbNames.EPYSL}..BookingChild BCC ON BCC.BookingID = YBM.BookingID AND BCC.ConsumptionID=YBC.ConsumptionID
                            LEFT JOIN {DbNames.EPYSL}..BOMConsumption BCCC ON BCCC.ConsumptionID=YBC.ConsumptionID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = FBC.A1ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON YBC.ItemMasterID = IM.ItemMasterID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                            WHERE YBM.WithoutOB=0 AND BM.BookingNo='{bookingNo}'
	                        GROUP BY YBC.YBookingID,YBC.ItemMasterID, YBC.ConsumptionID, YBC.YarnTypeID,YBC.YarnBrandID,
	                        YBC.BookingUnitID,ISNULL(YBC.FTechnicalName,''),YBC.IsCompleteReceive,YBC.IsCompleteDelivery,YBC.QtyInKG,YBC.ExcessPercentage,
	                        YBC.ExcessQtyInKG,YBC.TotalQty,YBC.TotalQtyInKG,
                            YBM.BookingID,YBM.CompanyID,YBM.ExportOrderID,Isnull(BCC.TechPackID,0),Isnull(BCC.ContactID,0),
                            ISV1.SegmentValueID, ISV1.SegmentValue, ISV2.SegmentValueID, ISV2.SegmentValue, IM.Segment3ValueID, IM.SubGroupID, ISV3.SegmentValue, IM.Segment4ValueID, ISV4.SegmentValue,
                            BCCC.Segment5Desc,IM.Segment7ValueID,BCCC.Segment7Desc,ISV.SegmentValue,ETV.ValueName, BCCC.Segment6Desc,BCC.Remarks,FBC.LabDipNo
                        ),
                        C2 AS(
                            SELECT YBC.YBookingID,YBC.ItemMasterID, YBC.ConsumptionID, YBC.YarnTypeID,YBC.YarnBrandID,
	                        YBC.BookingUnitID,BookingQty = SUM(YBC.BookingQty),FTechnicalName=ISNULL(YBC.FTechnicalName,''),YBC.IsCompleteReceive,YBC.IsCompleteDelivery,YBC.QtyInKG,YBC.ExcessPercentage,
	                        YBC.ExcessQtyInKG,YBC.TotalQty,YBC.TotalQtyInKG,
	                        YBM.BookingID,ExecutionCompanyID=YBM.CompanyID,YBM.ExportOrderID, Isnull(FBC.TechPackID,0)TechPackID,
                            ConsumptionQty = SUM(SBC.ConsumptionQty), RequisitionQty = SUM(FBC.RequisitionQty), Isnull(FBC.ContactID,0)ContactID,
                            ISV1.SegmentValueID ConstructionId, ISV1.SegmentValue Construction, 
	                        ISV2.SegmentValueID CompositionId, ISV2.SegmentValue Composition, 

                            IM.Segment3ValueID ColorID, Color = Case When IM.SubGroupID = 1 Then ISV3.SegmentValue Else '' End,
                            IM.Segment4ValueID GSMId,GSM= Case When IM.SubGroupID = 1 Then ISV4.SegmentValue Else '' End,

                            FabricWidth=SBC.Segment5Desc,
                            KnittingTypeId=IM.Segment7ValueID,KnittingType=SBC.Segment7Desc,
                            YarnType=ISV.SegmentValue,
                            YarnProgram=ETV.ValueName,
                            DyeingType=SBC.Segment6Desc,
                            Instruction=SBC.Remarks,
                            FBC.LabDipNo,
                            Length = Case When IM.SubGroupID <> 1 Then ISV3.SegmentValue Else '' End,
                            Height = Case When IM.SubGroupID <> 1 Then ISV4.SegmentValue Else '' End
                            FROM {TableNames.YarnBookingChild_New} YBC
                            LEFT JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID=YBC.YBookingID
                            LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM on SBM.BookingID=YBM.BookingID
                            LEFT JOIN {DbNames.EPYSL}..SampleBookingConsumption SBC ON SBC.BookingID = SBM.BookingID AND SBC.ConsumptionID = YBC.ConsumptionID
                            LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA ON FBA.BookingID=YBM.BookingID
                            LEFT JOIN {TableNames.FBBOOKING_ACKNOWLEDGE_CHILD} FBC ON FBC.AcknowledgeID=FBA.FBAckID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = SBC.A1ValueID
                            LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = FBC.YarnBrandID
                            LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON YBC.ItemMasterID = IM.ItemMasterID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                            WHERE YBM.WithoutOB=1 AND SBM.BookingNo='{bookingNo}'
	                        GROUP BY YBC.YBookingID,YBC.ItemMasterID, YBC.ConsumptionID, YBC.YarnTypeID,YBC.YarnBrandID,
	                        YBC.BookingUnitID,ISNULL(YBC.FTechnicalName,''),YBC.IsCompleteReceive,YBC.IsCompleteDelivery,YBC.QtyInKG,YBC.ExcessPercentage,
	                        YBC.ExcessQtyInKG,YBC.TotalQty,YBC.TotalQtyInKG, ISV3.SegmentValue, ISV4.SegmentValue,
	                        YBM.BookingID,YBM.CompanyID,YBM.ExportOrderID, Isnull(FBC.TechPackID,0),Isnull(FBC.ContactID,0),
                            ISV1.SegmentValueID, ISV1.SegmentValue, ISV2.SegmentValueID, ISV2.SegmentValue, IM.Segment3ValueID, IM.SubGroupID, IM.Segment4ValueID,
	                        SBC.Segment5Desc, IM.Segment7ValueID,SBC.Segment7Desc, ISV.SegmentValue, ETV.ValueName, SBC.Segment6Desc,  SBC.Remarks, FBC.LabDipNo
                            ),
                            C AS(
                                SELECT * FROM C1
                                UNION
                                SELECT * FROM C2
                            )
                            SELECT * FROM C;

                             --YarnBookingChildItem_New
                            WITH CI1 AS(
                                SELECT YBCI.*,ISV1.SegmentValue Segment1ValueDesc,ISV2.SegmentValue Segment2ValueDesc,ISV3.SegmentValue Segment3ValueDesc,ISV4.SegmentValue Segment4ValueDesc
								,ISV5.SegmentValue Segment5ValueDesc,ISV6.SegmentValue Segment6ValueDesc
                                FROM {TableNames.YarnBookingChildItem_New} YBCI
                                LEFT JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID=YBCI.YBChildID
                                LEFT JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID=YBC.YBookingID
                                LEFT JOIN {DbNames.EPYSL}..BookingMaster BM on BM.BookingID=YBM.BookingID
								LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YBCI.YItemMasterId
								LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
								LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
								LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
								LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
								LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
								LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                                WHERE YBM.WithoutOB=0 AND BM.BookingNo='{bookingNo}'
                            ),
                            CI2 AS(
                                SELECT YBCI.*,ISV1.SegmentValue Segment1ValueDesc,ISV2.SegmentValue Segment2ValueDesc,ISV3.SegmentValue Segment3ValueDesc,ISV4.SegmentValue Segment4ValueDesc
								,ISV5.SegmentValue Segment5ValueDesc,ISV6.SegmentValue Segment6ValueDesc
                                FROM {TableNames.YarnBookingChildItem_New} YBCI
                                LEFT JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID=YBCI.YBChildID
                                LEFT JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID=YBC.YBookingID
                                LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster BM on BM.BookingID=YBM.BookingID
								LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YBCI.YItemMasterId
								LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
								LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
								LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
								LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
								LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
								LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                                WHERE YBM.WithoutOB=1 AND BM.BookingNo='{bookingNo}'
                            ),
                            CI AS(
                                SELECT * FROM CI1
                                UNION
                                SELECT * FROM CI2
                            )
                            SELECT * FROM CI;

                            --CriteriaNames
                            ;SELECT CriteriaName,CriteriaSeqNo,(CASE WHEN CriteriaName  IN('Batch Preparation','Quality Check') THEN '1'ELSE'0'END) AS TotalTime 
                            FROM {TableNames.BDS_CRITERIA_HK} --WHERE CriteriaName NOT IN('Batch Preparation','Testing')
                            GROUP BY CriteriaSeqNo,CriteriaName order by CriteriaSeqNo,CriteriaName;

                            --FBAChildPlannings
                            ;SELECT * FROM {TableNames.BDS_CRITERIA_HK} order by CriteriaSeqNo, OperationSeqNo, CriteriaName;

                            --Technical Name
                            SELECT Cast(T.TechnicalNameId As varchar) id, T.TechnicalName [text], ISNULL(ST.[Days], 0) [desc], Cast(SC.SubClassID as varchar) additionalValue
                            FROM {TableNames.FabricTechnicalName} T
                            LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME_KMACHINE_SUB_CLASS} SC ON SC.TechnicalNameID = T.TechnicalNameId
                            LEFT JOIN {TableNames.KNITTING_MACHINE_STRUCTURE_TYPE_HK} ST ON ST.StructureTypeID = SC.StructureTypeID
                            Group By T.TechnicalNameId, T.TechnicalName, ST.Days, SC.SubClassID;

                            --M/c type
                            ;SELECT CAST(a.MachineSubClassID AS varchar) [id], b.SubClassName [text], b.TypeID [desc], c.TypeName additionalValue
                            FROM {TableNames.KNITTING_MACHINE} a
                            INNER JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} b ON b.SubClassID = a.MachineSubClassID
                            Inner JOIN {TableNames.KNITTING_MACHINE_TYPE} c On c.TypeID = b.TypeID
                            --Where c.TypeName != 'Flat Bed'
                            GROUP BY a.MachineSubClassID, b.SubClassName, b.TypeID, c.TypeName;

                            --Brand List
							;SELECT DISTINCT(KM.BrandID) [id], EV.ValueName [text]
							FROM {TableNames.KNITTING_MACHINE} KM
							LEFT JOIN {TableNames.KNITTING_UNIT} KU ON KU.KnittingUnitID = KM.KnittingUnitID
							LEFT JOIN {DbNames.EPYSL}..EntityTypeValue EV ON ValueID = KM.BrandID
							ORDER BY [text];

                            -- Shade book
                            {CommonQueries.GetYarnShadeBooks()};

                            -- YarnBookingChildItemYarnSubBrand
                            Select ETV.ValueID id, ETV.ValueName [text]
                            From {DbNames.EPYSL}..EntityTypeValue ETV
                            Inner Join {DbNames.EPYSL}..EntityType ET On ET.EntityTypeID = ETV.EntityTypeID
                            Where ET.EntityTypeName = 'Yarn Sub Brand'
                            Order By ETV.ValueName;";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnBookingMaster yarnBookingMaster = new YarnBookingMaster();

                List<YarnBookingMaster> data = records.Read<YarnBookingMaster>().ToList();
                Guard.Against.NullObject(data);

                yarnBookingMaster = data != null ? data.FirstOrDefault() : new YarnBookingMaster();

                var childs = records.Read<YarnBookingChild>().ToList();
                List<YarnBookingChildItem> ChildItems = records.Read<YarnBookingChildItem>().ToList();
                List<FBookingAcknowledgeChild> criteriaNames = records.Read<FBookingAcknowledgeChild>().ToList();
                List<FBAChildPlanning> fbaChildPlannings = records.Read<FBAChildPlanning>().ToList();
                yarnBookingMaster.TechnicalNameList = await records.ReadAsync<Select2OptionModel>();

                List<Select2OptionModel> mcTypeList = records.Read<Select2OptionModel>().ToList();
                yarnBookingMaster.MCTypeForFabricList = mcTypeList.Where(x => x.additionalValue != "Flat Bed").ToList();
                yarnBookingMaster.MCTypeForOtherList = mcTypeList.Where(x => x.additionalValue == "Flat Bed").ToList();

                yarnBookingMaster.KnittingMachines = records.Read<KnittingMachine>().ToList();

                List<Select2OptionModel> yarnShadeBooks = records.Read<Select2OptionModel>().ToList();
                List<Select2OptionModel> yarnSubBrandList = records.Read<Select2OptionModel>().ToList();

                data.ForEach(m =>
                {
                    m.Childs = childs.Where(c => c.YBookingID == m.YBookingID).ToList();
                    m.Childs.ForEach(c =>
                    {
                        c.SubGroupId = m.SubGroupID;
                        c.CriteriaNames = criteriaNames;
                        c.FBAChildPlannings = fbaChildPlannings;
                        c.ChildItems = ChildItems.Where(d => d.YBChildID == c.YBChildID).ToList();
                    });

                    if (m.SubGroupID == 1)
                    {
                        yarnBookingMaster.Fabrics = m.Childs;
                        if (yarnBookingMaster.Fabrics.Count() > 0)
                        {
                            yarnBookingMaster.Fabrics[0].YarnShadeBooks = yarnShadeBooks;
                            yarnBookingMaster.Fabrics[0].YarnSubBrandList = yarnSubBrandList;
                        }
                        yarnBookingMaster.HasFabric = m.Childs.Where(x => x.SubGroupId == 1).ToList().Count() > 0 ? true : false;
                    }
                    else if (m.SubGroupID == 11)
                    {
                        yarnBookingMaster.Collars = m.Childs;
                        if (yarnBookingMaster.Collars.Count() > 0)
                        {
                            yarnBookingMaster.Collars[0].YarnShadeBooks = yarnShadeBooks;
                            yarnBookingMaster.Collars[0].YarnSubBrandList = yarnSubBrandList;
                        }
                        yarnBookingMaster.HasCollar = m.Childs.Where(x => x.SubGroupId == 11).ToList().Count() > 0 ? true : false;
                    }
                    else if (m.SubGroupID == 12)
                    {
                        yarnBookingMaster.Cuffs = m.Childs;
                        if (yarnBookingMaster.Cuffs.Count() > 0)
                        {
                            yarnBookingMaster.Cuffs[0].YarnShadeBooks = yarnShadeBooks;
                            yarnBookingMaster.Cuffs[0].YarnSubBrandList = yarnSubBrandList;
                        }
                        yarnBookingMaster.HasCuff = m.Childs.Where(x => x.SubGroupId == 12).ToList().Count() > 0 ? true : false;
                    }
                });

                if (!yarnBookingMaster.BookingNo.IsNullOrEmpty())
                {
                    //223213-FBR-Add-1
                    if (yarnBookingMaster.BookingNo.ToUpper().IndexOf("-ADD-") > -1)
                    {
                        string[] spliteSLNo = yarnBookingMaster.BookingNo.Split('-');
                        if (spliteSLNo.Length == 4)
                        {
                            yarnBookingMaster.SLNo = spliteSLNo[3] + "A" + spliteSLNo[0];
                        }
                    }
                    else
                    {
                        yarnBookingMaster.SLNo = yarnBookingMaster.ExportOrderNo;
                    }
                }

                return yarnBookingMaster;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open) _connection.Close();
            }
        }
        */
        public async Task<YarnBookingMaster> GetAsyncforAutoPR(string yBookingNo)
        {
            string sql;
            sql = $@"
                -- Yarn Booking Master
                With YBM As 
                (
	                Select top 1 * FROM {TableNames.YarnBookingMaster_New} Where YBookingNo = '{yBookingNo}'
                )
                Select YBM.YBookingID, YBM.YBookingNo, BM.BookingNo, BM.BookingID, ISNULL(BM.ReferenceNo,'') As ReferenceNo, BM.InHouseDate As RequiredDate,
                Isnull(FBA.PreRevisionNo,0)PreProcessRevNo, YBM.RevisionNo, YBM.YBookingDate, YBM.BuyerID, C.ShortName As BuyerName, YBM.BuyerTeamID, 
                CCT.TeamName BuyerDepartment, YBM.CompanyID, CE.CompanyName, YBM.ExportOrderID, EOM.ExportOrderNo, EOM.CalendarDays As TNADays, 
                YBM.YInHouseDate, YBM.YRequiredDate, YBM.ContactPerson, EMP.EmployeeName As MerchandiserName, YBM.SubGroupID, ISG.SubGroupName,
                YBM.WithoutOB, YBM.AdditionalBooking, YBM.IsCancel, YBM.CancelReasonID, YBM.CanceledBy, YBM.DateCanceled 
                From YBM left Join {DbNames.EPYSL}..BookingMaster BM On BM.BookingID = YBM.BookingID
                left Join {DbNames.EPYSL}..Contacts C On C.ContactID = YBM.BuyerID
                Left Join {DbNames.EPYSL}..ContactCategoryTeam CCT On CCT.CategoryTeamID = YBM.BuyerTeamID
                Left Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = YBM.CompanyID
                Left Join {DbNames.EPYSL}..ExportOrderMaster EOM On EOM.ExportOrderID = YBM.ExportOrderID
                Left Join {DbNames.EPYSL}..Employee EMP On EMP.EmployeeCode = YBM.ContactPerson
                Left Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = YBM.SubGroupID 
                Left JOIN {TableNames.FBBOOKING_ACKNOWLEDGE} FBA On FBA.BookingID = YBM.BookingID 
                Group By YBM.YBookingID, YBM.YBookingNo, BM.BookingNo, BM.BookingID, BM.ReferenceNo, BM.InHouseDate,
                FBA.PreRevisionNo, YBM.RevisionNo, YBM.YBookingDate, YBM.BuyerID, C.ShortName,
                YBM.BuyerTeamID, CCT.TeamName, YBM.CompanyID, CE.CompanyName, YBM.ExportOrderID, EOM.ExportOrderNo, EOM.CalendarDays, 
                YBM.YInHouseDate, YBM.YRequiredDate, YBM.ContactPerson, EMP.EmployeeName, YBM.SubGroupID, ISG.SubGroupName, 
                YBM.WithoutOB, YBM.AdditionalBooking, YBM.IsCancel, YBM.CancelReasonID, YBM.CanceledBy, YBM.DateCanceled;

                -- Yarn Booking Child
                ;With YBM As 
                (
	                Select * FROM {TableNames.YarnBookingMaster_New} Where YBookingNo = '{yBookingNo}'
                )
                Select YBC.YBChildID, YBC.YBookingID, YBC.ConsumptionID, YBC.ItemMasterID, YBC.YarnTypeID, YBC.YarnBrandID, ETV.ValueName YarnBrand, 
                YBC.FUPartID, FUP.PartName, YBC.BookingUnitID, BC.Remarks, BC.A1ValueID As YarnTypeID, ISV1.SegmentValue YarnType, U.DisplayUnitDesc As BookingUOM, YBM.BookingID, 
                Ceiling(Sum(YBC.BookingQty))TotalQty, YBC.FTechnicalName, YBC.IsCompleteReceive, YBC.IsCompleteDelivery, YBM.SubGroupID,ISG.SubGroupName,
                BC.Segment1Desc Segment1ValueDesc,BC.Segment1ValueID, 
                BC.Segment2Desc Composition, BC.Segment2ValueID,BC.Segment3Desc Color, BC.Segment3ValueID,BC.Segment4Desc GSM, 
                BC.Segment4ValueID,BC.Segment5Desc Segment5ValueDesc, BC.Segment5ValueID,BC.Segment6Desc DyeingType, BC.Segment6ValueID,BC.Segment7Desc Segment7ValueDesc, BC.Segment7ValueID,BC.Segment8Desc Segment8ValueDesc,
                BC.Segment8ValueID,
                BC.Segment9Desc Segment9ValueDesc, BC.Segment9ValueID,BC.Segment10Desc Segment10ValueDesc, BC.Segment11Desc Segment11ValueDesc, BC.Segment12Desc Segment12ValueDesc,
                BC.Segment13Desc Segment13ValueDesc, BC.Segment14Desc Segment14ValueDesc, BC.Segment15Desc Segment15ValueDesc 

                From YBM Inner JOIN {TableNames.YarnBookingChild_New} YBC On YBC.YBookingID = YBM.YBookingID
                Left Join {DbNames.EPYSL}..BOMConsumption BC On BC.ConsumptionID = YBC.ConsumptionID
                Left Join {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = YBC.YarnBrandID 
                Left Join {DbNames.EPYSL}..FabricUsedPart FUP On FUP.FUPartID = YBC.FUPartID
                LEFT JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = YBC.BookingUnitID
                Left Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = YBM.SubGroupID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = BC.A1ValueID 
                Group By YBC.YBChildID, YBC.YBookingID, YBC.ConsumptionID, YBC.ItemMasterID, YBC.YarnTypeID, YBC.YarnBrandID, ETV.ValueName, 
                YBC.FUPartID, FUP.PartName, BC.A1ValueID, ISV1.SegmentValue, BC.Remarks, YBC.BookingUnitID, U.DisplayUnitDesc, YBM.BookingID, YBC.FTechnicalName, YBC.IsCompleteReceive,
                YBC.IsCompleteDelivery, YBM.SubGroupID,ISG.SubGroupName, 
                BC.Segment1Desc, 
                BC.Segment2Desc, 
                BC.Segment3Desc, 
                BC.Segment4Desc, 
                BC.Segment5Desc, 
                BC.Segment6Desc, 
                BC.Segment7Desc, 
                BC.Segment8Desc, 
                BC.Segment9Desc, BC.Segment10Desc, BC.Segment11Desc, BC.Segment12Desc,
                BC.Segment13Desc, BC.Segment14Desc, BC.Segment15Desc,
                BC.Segment1ValueID,BC.Segment2ValueID,BC.Segment3ValueID,BC.Segment4ValueID,BC.Segment4ValueID,BC.Segment5ValueID,
                BC.Segment6ValueID,BC.Segment7ValueID,BC.Segment8ValueID,BC.Segment9ValueID;
                
                -- Yarn Booking Child Item
                ;With YBM As 
                (
	                Select * FROM {TableNames.YarnBookingMaster_New} Where YBookingNo = '{yBookingNo}'
                ) 
                Select YBCI.YBChildItemID, YBCI.YBChildID, YBCI.YBookingID As BookingID, YBCI.YItemMasterID As ItemMasterID, YBCI.UnitID, U.DisplayUnitDesc, YBCI.Blending, 
                (Case When Blending = 1 then 'Blend' else 'Non-Blend' End)BlendingName, YBCI.YarnCategory, YBCI.YItemMasterID, YBCI.Distribution, YBCI.BookingQty, YBCI.Allowance, 
                YBCI.RequiredQty, YBCI.ShadeCode, Y.ShadeCode as ShadeName,
                YBCI.Remarks, YBCI.Specification, YBCI.YD, YBCI.YDItem, YBM.BookingID,
                IM.Segment1ValueID Segment1ValueId, IM.Segment2ValueID Segment2ValueId, IM.Segment3ValueID Segment3ValueId, IM.Segment4ValueID Segment4ValueId,
                IM.Segment5ValueID Segment5ValueId, IM.Segment6ValueID Segment6ValueId, IM.Segment7ValueID Segment7ValueId, IM.Segment8ValueID Segment8ValueId,
                IM.Segment9ValueID Segment9ValueId, IM.Segment10ValueID Segment10ValueId, IM.Segment11ValueID Segment11ValueId, IM.Segment12ValueID Segment12ValueId,
                IM.Segment13ValueID Segment13ValueId, IM.Segment14ValueID Segment14ValueId, IM.Segment15ValueID Segment15ValueId,
                ISV1.SegmentValue AS Segment1ValueDesc, ISV2.SegmentValue AS Segment2ValueDesc, ISV3.SegmentValue AS Segment3ValueDesc,
                ISV4.SegmentValue AS Segment4ValueDesc, ISV5.SegmentValue AS Segment5ValueDesc, ISV6.SegmentValue AS Segment6ValueDesc,
                ISV7.SegmentValue AS Segment7ValueDesc, ISV8.SegmentValue AS Segment8ValueDesc, BC.SubGroupID, ISG.SubGroupName,
                Isnull(YPM.YDProductionMasterID,0)YDProductionMasterID
                From YBM Inner JOIN {TableNames.YarnBookingChild_New} YBC On YBM.YBookingID = YBC.YBookingID
                Inner JOIN {TableNames.YarnBookingChildItem_New} YBCI On YBCI.YBChildID = YBC.YBChildID
                Inner Join {DbNames.EPYSL}..BOMConsumption BC On BC.ConsumptionID = YBC.ConsumptionID
                Left Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = YBM.SubGroupID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YBCI.YItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID
                LEFT JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = YBCI.UnitID
                LEFT JOIN {TableNames.YARN_SHADE_BOOK} Y ON Y.ShadeCode = YBCI.ShadeCode 
                LEFT JOIN {TableNames.YD_BOOKING_MASTER} YDBM ON YDBM.YBookingID = YBM.YBookingID And YDBM.YBookingID = YBCI.YBookingID
                LEFT JOIN {TableNames.YD_PRODUCTION_MASTER} YPM ON YPM.YDBookingMasterID = YDBM.YDBookingMasterID ;

                -- Shade book
                {CommonQueries.GetYarnShadeBooks()};

                ";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                //var records = await _connection.QueryMultipleAsync(sql, segmentNames);
                YarnBookingMaster data = await records.ReadFirstOrDefaultAsync<YarnBookingMaster>();
                Guard.Against.NullObject(data);
                data.Childs = records.Read<YarnBookingChild>().ToList();
                List<YarnBookingChildItem> ChildItems = records.Read<YarnBookingChildItem>().ToList();
                data.YarnShadeBooks = await records.ReadAsync<Select2OptionModel>();

                foreach (YarnBookingChild item in data.Childs)
                {
                    if (item.SubGroupName == "Fabric") data.HasFabric = true;
                    else if (item.SubGroupName == "Collar") data.HasCollar = true;
                    else if (item.SubGroupName == "Cuff") data.HasCuff = true;

                    item.ChildItems = ChildItems.Where(c => c.YBChildID == item.YBChildID).ToList();

                    item.FUPartIDs = data.yarnBookingChildGarmentPart.Where(x => x.BookingID == item.BookingID &&
                        x.ItemMasterID == item.ItemMasterID &&
                        x.ConsumptionID == item.ConsumptionID).Select(x => x.FUPartID).Distinct().ToArray();

                    item.PartName = string.Join(",", data.yarnBookingChildGarmentPart.Where(
                        x => x.BookingID == item.BookingID &&
                        x.ItemMasterID == item.ItemMasterID &&
                        x.ConsumptionID == item.ConsumptionID).Select(x => x.PartName).Distinct());

                    item.YarnSubBrandIDs = data.yarnBookingChildYarnSubBrand.Where(x => x.BookingID == item.BookingID &&
                        x.ItemMasterID == item.ItemMasterID &&
                        x.ConsumptionID == item.ConsumptionID).Select(x => x.YarnSubBrandID).Distinct().ToArray();

                    item.YarnSubBrandName = string.Join(",", data.yarnBookingChildYarnSubBrand.Where(
                        x => x.BookingID == item.BookingID &&
                        x.ItemMasterID == item.ItemMasterID &&
                        x.ConsumptionID == item.ConsumptionID).Select(x => x.YarnSubBrandName).Distinct());

                    foreach (var itemChildSBrand in item.ChildItems)
                    {
                        itemChildSBrand.YarnSubBrandIDs = data.yarnBookingChildItemYarnSubBrand.Where(
                            x => x.YBChildItemID == itemChildSBrand.YBChildItemID).Select(x => x.YarnSubBrandID).Distinct().ToArray();

                        itemChildSBrand.YarnSubBrandName = string.Join(",", data.yarnBookingChildItemYarnSubBrand.Where(
                            x => x.YBChildItemID == itemChildSBrand.YBChildItemID).Select(x => x.YarnSubBrandName).Distinct());
                    }
                    item.ChildItemsGroup = item.ChildItems;
                }
                //data.YarnBookings = records.Read<YarnBookingMaster>().ToList();
                data = await this.GetGroupByYarnBooking(data, true);

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open) _connection.Close();
            }
        }

        public async Task AcknowledgeEntityAsync(YarnPRMaster yarnPRMaster, int userId)
        {
            SqlTransaction transaction = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _connectionGmt.OpenAsync();
                transactionGmt = _connectionGmt.BeginTransaction();


                yarnPRMaster.YarnPRMasterID = await _service.GetMaxIdAsync(TableNames.YARN_PR_MASTER);
                yarnPRMaster.YarnPRNo = await _service.GetMaxNoAsync(TableNames.YARN_PRNO, 1, RepeatAfterEnum.NoRepeat, "00000", transactionGmt, _connectionGmt);
                int maxChildId = await _service.GetMaxIdAsync(TableNames.YARN_PR_CHILD, yarnPRMaster.Childs.Count);
                foreach (YarnPRChild child in yarnPRMaster.Childs)
                {
                    child.YarnPRChildID = maxChildId++;
                    child.YarnPRMasterID = yarnPRMaster.YarnPRMasterID;
                }
                await _service.SaveSingleAsync(yarnPRMaster, transaction);
                await _service.SaveAsync(yarnPRMaster.Childs, transaction);
                foreach (YarnPRChild item in yarnPRMaster.Childs)
                {
                    //await _service.ValidationSingleAsync(item, transaction, "sp_Validation_YarnPRChild", item.EntityState, userId, item.YarnPRChildID);
                    await _connection.ExecuteAsync("sp_Validation_YarnPRChild", new { EntityState = item.EntityState, UserId = userId, PrimaryKeyId = item.YarnPRChildID }, transaction, 30, CommandType.StoredProcedure);
                }
                transaction.Commit();
                transactionGmt.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                if (transactionGmt != null) transactionGmt.Rollback();
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }

        public async Task<string> GetExportDataAsync(string bookingNo, bool isSample)
        {
            string sql = "";
            if (!isSample)
            {
                sql = String.Format(@"With BM AS(
                                            Select BookingID, BookingNo, ExportOrderID, BookingDate, FirstInHouseDate, InHouseDate, RevisionNo, RevisionDate, AddedBy, Remarks 
                                            From EPYSL..BookingMaster 
                                            Where BookingNo = '{0}'
                                        ), BD AS(
                                            Select BC.BookingChildID, BC.ConsumptionID, BC.ItemMasterID, BCD.SizeID, S.SizeCode, Sum(BCD.BookingQty) BookingQty, Sum(BCD.OrderQty) OrderQty --BookingNo, ExportOrderID, BookingDate, FirstInHouseDate, InHouseDate, RevisionNo, RevisionDate, AddedBy, Remarks 
                                            From BM 
                                            Inner Join EPYSL..BookingChild BC On BC.BookingID = BM.BookingID
                                            Inner Join EPYSL..BookingChildDetails BCD On BCD.BookingChildID = BC.BookingChildID 
                                            Inner Join EPYSL..ItemSubGroup ISG On ISG.SubGroupID = BC.SubGroupID
                                            Left Join EPYSL..Sizes S On S.SizeID = BCD.SizeID
                                            Where ISG.SubGroupName <> 'Fabric'
                                            Group By BC.BookingChildID, BC.ConsumptionID, BC.ItemMasterID, BCD.SizeID, S.SizeCode
                                        ), OPO As(
                                            Select OrderBankMasterID, Max(CalendarDays) CalendarDays, Sum(TotalPOQty) POQty  
                                            From EPYSL..OrderBankPO Group by OrderBankMasterID
                                        )
                                    Select C.ShortName buyer, merchandising_team = (Select Top 1 ET.TeamName 
												                                    From EPYSL..EmployeeTeamAssign ETA
												                                    Inner Join EPYSL..EmployeeTeamHK ET On ET.TeamID = ETA.TeamID
												                                    Where ETA.EmployeeCode = LU.EmployeeCode),
                                    CCT.TeamName buyer_team, BM.BookingNo fabric_booking_no, YBM.RevisionNo revision_no, YBM.DateRevised revision_date, OPO.CalendarDays tna_cal, 
                                    EOM.ExportOrderNo export_work_order_no, 'Bulk' order_type, SM.StyleNo style_no, BM.BookingDate issue_date, BM.Remarks comment, section = Case When ISG.SubGroupName = 'Fabric' Then 'Body' Else ISG.SubGroupName End, 
                                    color = CASE WHEN ISG.SubGroupName = 'Fabric' THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END, --Case When ISG.SubGroupName = 'Fabric' Then ISV3.SegmentValue Else 'N/A' End ,
                                    fiber_composition = Case When ISG.SubGroupName = 'Fabric' Then ISV2.SegmentValue Else 'N/A' End, 
                                    gsm = Case When ISG.SubGroupName = 'Fabric' Then ISV4.SegmentValue Else 'N/A' End, dia = Case When ISG.SubGroupName = 'Fabric' Then ISV5.SegmentValue Else 'N/A' End, 
                                    order_qty = Case When ISG.SubGroupName = 'Fabric' Then YBC.BookingQty Else OPO.POQty End,
                                    fabric_type = T.TechnicalName, 
                                    BM.FirstInHouseDate delivery_start, BM.InHouseDate delivery_end, EMP.EmployeeName booked_by, ISNULL(BD.SizeCode,'') collar_Cuff_Size, ISNULL(BD.OrderQty,0) collar_Cuff_Qty,
                                    note = Case When ISG.SubGroupName = 'Fabric' Then ISV1.SegmentValue Else ISNULL(BC.Remarks,'') End, 
                                    'N/A' color_Type, YBCI.Allowance process_Loss, 
                                    YISV6.SegmentValue yarn_Count, 
                                    YISV1.SegmentValue composition,
                                    'N/A' yarn_color, 
                                    ETV.ValueName program, YBCI.ShadeCode shade_Code, YBCI.Specification yarn_Spacification, YBCI.RequiredQty req_Qty
                                    From BM
                                    Inner Join EPYSLTEX.."+TableNames.YarnBookingMaster_New+ @" YBM ON YBM.BookingID = BM.BookingID
                                    Inner Join EPYSLTEX.."+TableNames.YarnBookingChild_New + @" YBC On YBC.YBookingID = YBM.YBookingID
                                    Inner Join EPYSLTEX.."+TableNames.YarnBookingChildItem_New + @" YBCI ON YBCI.YBChildID = YBC.YBChildID
                                    INNER JOIN EPYSLTEX.."+TableNames.FBBOOKING_ACKNOWLEDGE_CHILD + @" FBC ON FBC.BookingChildID = YBC.BookingChildID AND FBC.BookingID = YBM.BookingID
                                    Inner Join EPYSLTEX.."+TableNames.FabricTechnicalName + @" T On T.TechnicalNameId = FBC.TechnicalNameID
                                    Left Join EPYSL..BookingChild BC On BC.BookingID = BM.BookingID And BC.ConsumptionID = YBC.ConsumptionID And BC.ItemMasterID = YBC.ItemMasterID
                                    Inner Join EPYSL..ExportOrderMaster EOM On EOM.ExportOrderID = BM.ExportOrderID                                       
                                    Inner Join EPYSL..StyleMaster SM On SM.StyleMasterID = EOM.StyleMasterID
                                    Inner Join EPYSL..OrderBankMaster OBM On OBM.StyleMasterID = SM.StyleMasterID
                                    Inner Join OPO ON OPO.OrderBankMasterID = OBM.OrderBankMasterID
                                    Inner Join EPYSL..ContactCategoryTeam CCT On CCT.CategoryTeamID = SM.BuyerTeamID
                                    Inner Join EPYSL..LoginUser LU ON LU.UserCode = BM.AddedBy
                                    Inner Join EPYSL..Employee EMP ON EMP.EmployeeCode = LU.EmployeeCode
                                    Inner Join EPYSL..Contacts C On C.ContactID = SM.BuyerID
                                    Inner Join EPYSL..ItemMaster IM On IM.ItemMasterID = YBC.ItemMasterID
                                    Inner Join EPYSL..ItemSubGroup ISG On ISG.SubGroupID = IM.SubGroupID
                                    Left Join EPYSL..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                                    Left Join EPYSL..ItemSegmentValue ISV2 On ISV2.SegmentValueID = IM.Segment2ValueID
                                    Left Join EPYSL..ItemSegmentValue ISV3 On ISV3.SegmentValueID = IM.Segment3ValueID
                                    Left Join EPYSL..ItemSegmentValue ISV4 On ISV4.SegmentValueID = IM.Segment4ValueID
                                    Left Join EPYSL..ItemSegmentValue ISV5 On ISV5.SegmentValueID = IM.Segment5ValueID
                                    Left Join EPYSL..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                                    Left Join BD On BD.ConsumptionID = YBC.ConsumptionID And BD.ItemMasterID = YBC.ItemMasterID
                                    Left Join EPYSL..EntityTypeValue ETV On ETV.ValueID = YBC.YarnBrandID
                                    Inner Join EPYSL..ItemMaster YIM On YIM.ItemMasterID = YBCI.YItemMasterID
                                    LEFT JOIN EPYSL..ItemSegmentValue YISV1 ON YISV1.SegmentValueID = YIM.Segment1ValueID
                                    LEFT JOIN EPYSL..ItemSegmentValue YISV2 ON YISV2.SegmentValueID = YIM.Segment2ValueID
                                    LEFT JOIN EPYSL..ItemSegmentValue YISV3 ON YISV3.SegmentValueID = YIM.Segment3ValueID
                                    LEFT JOIN EPYSL..ItemSegmentValue YISV4 ON YISV4.SegmentValueID = YIM.Segment4ValueID
                                    LEFT JOIN EPYSL..ItemSegmentValue YISV5 ON YISV5.SegmentValueID = YIM.Segment5ValueID
                                    LEFT JOIN EPYSL..ItemSegmentValue YISV6 ON YISV6.SegmentValueID = YIM.Segment6ValueID
                                    LEFT JOIN EPYSL..ItemSegmentValue YISV7 ON YISV7.SegmentValueID = YIM.Segment7ValueID", bookingNo);
            }
            else
            {
                sql = String.Format(@"With BM AS(
                                            Select BookingID, BookingNo, ExportOrderID, BookingDate, FirstInHouseDate, InHouseDate, RevisionNo, RevisionDate, AddedBy, Remarks, OrderQty 
                                            from EPYSL..SampleBookingMaster 
                                            Where BookingNo = '{0}'
                                        ), BD AS(
                                                Select 0 BookingChildID, BC.ConsumptionID, BCD.ItemMasterID, 0 SizeID, '' SizeCode, Sum(BCD.RequiredQty) BookingQty, Max(BM.OrderQty) OrderQty --BookingNo, ExportOrderID, BookingDate, FirstInHouseDate, InHouseDate, RevisionNo, RevisionDate, AddedBy, Remarks 
                                                From BM 
                                                Inner Join EPYSL..SampleBookingConsumption BC On BC.BookingID = BM.BookingID
                                                Inner Join EPYSL..SampleBookingConsumptionChild BCD On BCD.ConsumptionID = BC.ConsumptionID 
                                                Inner Join EPYSL..ItemSubGroup ISG On ISG.SubGroupID = BC.SubGroupID
                                                Where ISG.SubGroupName <> 'Fabric'
                                                Group By BC.ConsumptionID, BCD.ItemMasterID
                                        )
                                        Select C.ShortName buyer, merchandising_team = (Select Top 1 ET.TeamName 
												                                        From EPYSL..EmployeeTeamAssign ETA
												                                        Inner Join EPYSL..EmployeeTeamHK ET On ET.TeamID = ETA.TeamID
												                                        Where ETA.EmployeeCode = LU.EmployeeCode),
                                        CCT.TeamName buyer_team, BM.BookingNo fabric_booking_no, YBM.RevisionNo revision_no, YBM.DateRevised revision_date, 0 tna_cal, 
                                        EOM.ExportOrderNo export_work_order_no, 'Bulk' order_type, SM.StyleNo style_no, BM.BookingDate issue_date, BM.Remarks comment, section = Case When ISG.SubGroupName = 'Fabric' Then 'Body' Else ISG.SubGroupName End, 
                                        color = CASE WHEN ISG.SubGroupName = 'Fabric' THEN ISV3.SegmentValue ELSE ISV5.SegmentValue END, --Case When ISG.SubGroupName = 'Fabric' Then ISV3.SegmentValue Else 'N/A' End , 
                                        fiber_composition = Case When ISG.SubGroupName = 'Fabric' Then ISV2.SegmentValue Else 'N/A' End, 
                                        gsm = Case When ISG.SubGroupName = 'Fabric' Then ISV4.SegmentValue Else 'N/A' End, dia = Case When ISG.SubGroupName = 'Fabric' Then ISV5.SegmentValue Else 'N/A' End, 
                                        order_qty = Case When ISG.SubGroupName = 'Fabric' Then YBC.BookingQty Else BM.OrderQty End,
                                        fabric_type = T.TechnicalName,
                                        BM.FirstInHouseDate delivery_start, BM.InHouseDate delivery_end, EMP.EmployeeName booked_by, ISNULL(BD.SizeCode,'') collar_Cuff_Size, ISNULL(BD.OrderQty,0) collar_Cuff_Qty,
                                        note = Case When ISG.SubGroupName = 'Fabric' Then ISV1.SegmentValue Else BC.Remarks End, 
                                        'N/A' color_Type, YBCI.Allowance process_Loss, 
                                        YISV6.SegmentValue yarn_Count, 
                                        YISV1.SegmentValue composition,
                                        'N/A' yarn_color, 
                                        ETV.ValueName program, YBCI.ShadeCode shade_Code, YBCI.Specification yarn_Spacification, YBCI.RequiredQty req_Qty
                                        From BM
                                        Inner Join EPYSLTEX.."+TableNames.YarnBookingMaster_New+ @" YBM ON YBM.BookingID = BM.BookingID
                                        Inner Join EPYSLTEX.."+TableNames.YarnBookingChild_New + @" YBC On YBC.YBookingID = YBM.YBookingID
                                        Inner Join EPYSLTEX.."+TableNames.YarnBookingChildItem_New + @" YBCI ON YBCI.YBChildID = YBC.YBChildID
                                        INNER JOIN EPYSLTEX.."+TableNames.FBBOOKING_ACKNOWLEDGE_CHILD + @" FBC ON FBC.BookingChildID = YBC.BookingChildID AND FBC.BookingID = YBM.BookingID
                                        Inner Join EPYSLTEX.."+TableNames.FabricTechnicalName + @" T On T.TechnicalNameId = FBC.TechnicalNameID
                                        Inner Join EPYSL..SampleBookingConsumption BC On BC.BookingID = BM.BookingID And BC.ConsumptionID = YBC.ConsumptionID
                                        Inner Join EPYSL..ExportOrderMaster EOM On EOM.ExportOrderID = BM.ExportOrderID                                       
                                        Inner Join EPYSL..StyleMaster SM On SM.StyleMasterID = EOM.StyleMasterID
                                        Inner Join EPYSL..ContactCategoryTeam CCT On CCT.CategoryTeamID = SM.BuyerTeamID
                                        Inner Join EPYSL..LoginUser LU ON LU.UserCode = BM.AddedBy
                                        Inner Join EPYSL..Employee EMP ON EMP.EmployeeCode = LU.EmployeeCode
                                        Inner Join EPYSL..Contacts C On C.ContactID = SM.BuyerID
                                        Inner Join EPYSL..ItemMaster IM On IM.ItemMasterID = YBC.ItemMasterID
                                        Inner Join EPYSL..ItemSubGroup ISG On ISG.SubGroupID = IM.SubGroupID
                                        Left Join EPYSL..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                                        Left Join EPYSL..ItemSegmentValue ISV2 On ISV2.SegmentValueID = IM.Segment2ValueID
                                        Left Join EPYSL..ItemSegmentValue ISV3 On ISV3.SegmentValueID = IM.Segment3ValueID
                                        Left Join EPYSL..ItemSegmentValue ISV4 On ISV4.SegmentValueID = IM.Segment4ValueID
                                        Left Join EPYSL..ItemSegmentValue ISV5 On ISV5.SegmentValueID = IM.Segment5ValueID
                                        Left Join EPYSL..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                                        Left Join BD On BD.ConsumptionID = YBC.ConsumptionID And BD.ItemMasterID = YBC.ItemMasterID
                                        Left Join EPYSL..EntityTypeValue ETV On ETV.ValueID = YBC.YarnBrandID
                                        Inner Join EPYSL..ItemMaster YIM On YIM.ItemMasterID = YBCI.YItemMasterID
                                        LEFT JOIN EPYSL..ItemSegmentValue YISV1 ON YISV1.SegmentValueID = YIM.Segment1ValueID
                                        LEFT JOIN EPYSL..ItemSegmentValue YISV2 ON YISV2.SegmentValueID = YIM.Segment2ValueID
                                        LEFT JOIN EPYSL..ItemSegmentValue YISV3 ON YISV3.SegmentValueID = YIM.Segment3ValueID
                                        LEFT JOIN EPYSL..ItemSegmentValue YISV4 ON YISV4.SegmentValueID = YIM.Segment4ValueID
                                        LEFT JOIN EPYSL..ItemSegmentValue YISV5 ON YISV5.SegmentValueID = YIM.Segment5ValueID
                                        LEFT JOIN EPYSL..ItemSegmentValue YISV6 ON YISV6.SegmentValueID = YIM.Segment6ValueID
                                        LEFT JOIN EPYSL..ItemSegmentValue YISV7 ON YISV7.SegmentValueID = YIM.Segment7ValueID", bookingNo);

            }
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.ExecuteReaderAsync(sql);

                DataTable dataTable = new DataTable();
                dataTable.Load(records);
                string JSONString = string.Empty;
                //JSONString = StaticInfo.DataTableToJSONWithStringBuilder(dataTable);
                JSONString = StaticInfo.DataTableToJSONWithJavaScriptSerializer(dataTable);


                return JSONString;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open) _connection.Close();
            }



        }
        public async Task<List<YarnBookingMaster>> GetAllYarnBookingMasterByBookingNo(string bookingNo)
        {
            string sql = "";
            sql = String.Format(@"Select YBM.*
                                        FROM EPYSLTEX.." + TableNames.YarnBookingMaster_New+ @" YBM
                                        Inner JOIN EPYSLTEX.." + TableNames.FabricBookingAcknowledge+@" FBA On FBA.BookingID = YBM.BookingID and FBA.WithoutOB = YBM.WithoutOB
                                        Inner Join EPYSL..ItemSubGroup ISG On ISG.SubGroupID = FBA.SubGroupID										
										Inner Join (
											Select BookingID, BookingNo, convert(bit,0) WithoutOB  from EPYSL..BookingMaster
											Union All
											Select BookingID, BookingNo, convert(bit,1) WithoutOB  from EPYSL..SampleBookingMaster
										) BM On BM.BookingID = YBM.BookingID And BM.WithoutOB = YBM.WithoutOB
					                    Where BM.BookingNo = '{0}'", bookingNo);


            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                return records.Read<YarnBookingMaster>().ToList();

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open) _connection.Close();
            }



        }
        public async Task<List<FBookingAcknowledge>> GetFBookingAcknowledgeByBookingNo(string BookingNo)
        {
            var sql = $@"Select * FROM {TableNames.FBBOOKING_ACKNOWLEDGE} where BookingNo = '{BookingNo}';";


            try
            {
                List<FBookingAcknowledge> data = await _service.GetDataAsync<FBookingAcknowledge>(sql);
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open) _connection.Close();
            }
        }
        public async Task UpdateDataExportInfo(List<YarnBookingMaster> entities)
        {
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();


                await _service.SaveAsync(entities, transaction);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                throw ex;
            }
            finally
            {
                if (transaction != null) transaction.Dispose();
                _connection.Close();
            }
        }
    }
}
