using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Booking;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;

namespace EPYSLTEXCore.Application.Services.Inventory
{
    public class YarnDyeingBookingService : IYarnDyeingBookingService
    {
        private readonly IDapperCRUDService<YDBookingMaster> _service;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;
        private readonly ItemMasterService<YDBookingChildTwisting> _itemMasterRepository;

        public YarnDyeingBookingService(IDapperCRUDService<YDBookingMaster> service
            , ItemMasterService<YDBookingChildTwisting> itemMasterRepository)
        {
            _service = service;
            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
            _itemMasterRepository = itemMasterRepository;
        }
        public async Task<List<YDBookingMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo, string pageName)
        {
            bool isNeedImage = true;
            string tempGuid = CommonFunction.GetNewGuid();
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YDBookingMasterID Desc" : paginationInfo.OrderBy;
            string sql;
            if (status == Status.Pending)
            {
                if (pageName == PageNames.YDBB)
                {
                    sql = $@"
                    ;WITH M As 
                    (
                        Select YBM.YDBookingMasterID, YBM.YDBookingNo, YBM.YDBookingBy, YBM.YDBookingDate, 
                        YBM.SwatchFilePath, FCM.GroupConceptNo, FCM.BookingID, YBM.PreviewTemplate, YBM.BuyerID, 
                        YBM.Remarks, YBM.IsApprove, YBM.IsAcknowledge, 
                        IsSample = CASE WHEN ISNULL(SBM.BookingID,0) > 0 THEN 1 ELSE 0 END,
	                    (Case When FCM.IsBDS = 0 Then 'R&D' Else c.ShortName End) As BuyerName,  
                        ProgramName = CASE WHEN FCM.IsBDS = 0 THEN 'Concept' 
					                    WHEN FCM.IsBDS = 1 THEN 'BDS' 
					                    WHEN FCM.IsBDS = 2 THEN 'Bulk'
					                    WHEN FCM.IsBDS = 3 THEN 'Projection'
					                    ELSE '-' End 
                        From YDBookingMaster YBM
	                    Inner JOIN FreeConceptMaster FCM ON FCM.GroupConceptNo = YBM.GroupConceptNo
                        Inner JOIN FreeConceptMRMaster FCMR ON FCMR.ConceptID = FCM.ConceptID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts C ON YBM.BuyerID = C.ContactID
                        LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = FCM.BookingID
                        Where FCMR.RevisionNo = YBM.PreProcessRevNo And YBM.SendForApproval = 1 
                        And YBM.IsApprove = 1 And YBM.IsAcknowledge = 1 And YBM.IsYDBNoGenerated = 0
	                    Group By YBM.YDBookingMasterID, YBM.YDBookingNo, YBM.YDBookingBy, YBM.YDBookingDate,
                        YBM.SwatchFilePath, FCM.GroupConceptNo, FCM.BookingID, YBM.PreviewTemplate, YBM.BuyerID, 
                        YBM.Remarks, YBM.IsApprove, YBM.IsAcknowledge, 
	                    FCM.IsBDS, c.ShortName,CASE WHEN ISNULL(SBM.BookingID,0) > 0 THEN 1 ELSE 0 END
                    ),
				    YBM AS
				    (
					    SELECT YBM.BookingID, YBM.YBookingNo
					    FROM YarnBookingMaster_New YBM
					    INNER JOIN M ON M.BookingID = YBM.BookingID
					    GROUP BY YBM.BookingID, YBM.YBookingNo
				    ),
                    FinalList AS
                    (
                        SELECT YDBookingMasterID, YDBookingNo, Remarks, IsAcknowledge, YDBookingBy, BuyerID, GroupConceptNo, ProgramName, 
                        YBM.YBookingNo, YDBookingDate, SwatchFilePath, PreviewTemplate, BuyerName, M.IsSample
                        FROM M 
					    LEFT JOIN YBM ON YBM.BookingID = M.BookingID
                        GROUP BY YDBookingMasterID, YDBookingNo, Remarks, IsAcknowledge, YDBookingBy, BuyerID, GroupConceptNo, ProgramName, 
                        YBM.YBookingNo, YDBookingDate, SwatchFilePath, PreviewTemplate, BuyerName, M.IsSample
                    )
                    SELECT *, Count(*) Over() TotalRows FROM FinalList";
                }
                else
                {
                    sql = $@"
                    With Concept As 
                    (
	                    SELECT FCM.GroupConceptNo,
	                    ProgramName = CASE WHEN FCM.IsBDS = 0 THEN 'Concept' 
					                       WHEN FCM.IsBDS = 1 THEN 'BDS' 
					                       WHEN FCM.IsBDS = 2 THEN 'Bulk'
					                       WHEN FCM.IsBDS = 3 THEN 'Projection'
					                       ELSE '-' End,
	                    YBookingNo = '', (Case When FCM.IsBDS = 0 Then 'R&D' Else c.ShortName End) As BuyerName,
                        Max(FCMRM.ReqDate) PDate  
	                    FROM FreeConceptMRMaster FCMRM
	                    INNER JOIN FreeConceptMRChild FCMRC ON FCMRM.FCMRMasterID = FCMRC.FCMRMasterID AND FCMRC.YD = 1
	                    INNER JOIN FreeConceptMaster FCM ON FCM.ConceptID = FCMRM.ConceptID
	                    LEFT JOIN YDBookingMaster YDBM ON FCM.GroupConceptNo = YDBM.GroupConceptNo
	                    Left Join {DbNames.EPYSL}..Contacts c On c.ContactID = FCM.BuyerID   
	                    WHERE YDBM.GroupConceptNo IS NULL AND FCMRM.IsComplete=1 AND FCM.IsBDS IN (0,1)
	                    GROUP BY FCM.GroupConceptNo, FCM.IsBDS, c.ShortName 
                    ),
                    YBulk As 
                    (
	                    SELECT GroupConceptNo = FBA.BookingNo, 'Bulk' As ProgramName, YBM.YBookingNo, 
	                    BuyerName = c.ShortName, PDate = Max(YBM.YBookingDate)  
	                    From YarnBookingChildItem_New YCI
	                    INNER JOIN YarnBookingChild_New YBC ON YBC.YBChildID = YCI.YBChildID
	                    INNER JOIN YarnBookingMaster_New YBM ON YBM.YBookingID = YBC.YBookingID
	                    INNER JOIN FBookingAcknowledgeChild FBC ON FBC.BookingChildID = YBC.BookingChildID
	                    INNER JOIN FBookingAcknowledge FBA ON FBA.FBAckID = FBC.AcknowledgeID
	                    LEFT JOIN {DbNames.EPYSL}..Contacts  C ON C.ContactID = YBM.BuyerID
	                    LEFT JOIN YDBookingMaster YDM ON YDM.GroupConceptNo = FBA.BookingNo
	                    Where YCI.YD = 1 AND ISNULL(FBA.IsApprovedByPMC,0) = 1 AND YDM.YDBookingMasterID IS NULL
	                    Group By YBM.YBookingNo, c.ShortName, FBA.BookingNo
                    ),
                    TOTALROW AS
                    (
                        SELECT * FROM Concept
	                    UNION
	                    SELECT * FROM YBulk
                    )
                    SELECT *, Count(*) Over() TotalRows FROM TOTALROW ";

                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "ORDER BY PDate DESC" : paginationInfo.OrderBy;
                }

            }
            else if (status == Status.PartiallyCompleted)
            {
                sql = $@"
                ;WITH M As 
                (
                    Select YBM.YDBookingMasterID, YBM.YDBookingNo, YBM.YDBookingBy, YBM.YDBookingDate, 
                    YBM.SwatchFilePath, FCM.GroupConceptNo, YBM.PreviewTemplate, YBM.BuyerID, 
                    YBM.Remarks, YBM.IsApprove, YBM.IsAcknowledge, FCM.BookingID,
	                (Case When FCM.IsBDS = 0 Then 'R&D' Else c.ShortName End) As BuyerName,  
                    ProgramName = CASE WHEN FCM.IsBDS = 0 THEN 'Concept' 
					            WHEN FCM.IsBDS = 1 THEN 'BDS' 
					            WHEN FCM.IsBDS = 2 THEN 'Bulk'
					            WHEN FCM.IsBDS = 3 THEN 'Projection'
					            ELSE '-' End
                    From YDBookingMaster YBM
	                Inner JOIN FreeConceptMaster FCM ON FCM.GroupConceptNo = YBM.GroupConceptNo
                    Inner JOIN FreeConceptMRMaster FCMR ON FCMR.ConceptID = FCM.ConceptID
	                LEFT JOIN {DbNames.EPYSL}..Contacts C ON YBM.BuyerID = C.ContactID
                    Where FCMR.RevisionNo = YBM.PreProcessRevNo And YBM.SendForApproval = 0 
	                Group By YBM.YDBookingMasterID, YBM.YDBookingNo, YBM.YDBookingBy, YBM.YDBookingDate,
                    YBM.SwatchFilePath, FCM.GroupConceptNo, YBM.PreviewTemplate, YBM.BuyerID, 
                    YBM.Remarks, YBM.IsApprove, YBM.IsAcknowledge, FCM.BookingID, 
	                FCM.IsBDS, c.ShortName 
                ),
				YBM AS
				(
					SELECT YBM.BookingID, YBM.YBookingNo
					FROM YarnBookingMaster_New YBM
					INNER JOIN M ON M.BookingID = YBM.BookingID
					GROUP BY YBM.BookingID, YBM.YBookingNo
				),
                FinalList AS
                (
                    SELECT YDBookingMasterID, YDBookingNo, Remarks, IsAcknowledge, YDBookingBy, BuyerID, GroupConceptNo, ProgramName, 
                    YDBookingDate, SwatchFilePath, PreviewTemplate, BuyerName, YBM.YBookingNo
                    FROM M 
					LEFT JOIN YBM ON YBM.BookingID = M.BookingID
                    GROUP BY YDBookingMasterID, YDBookingNo, Remarks, IsAcknowledge, YDBookingBy, BuyerID, GroupConceptNo, ProgramName, 
                    YDBookingDate, SwatchFilePath, PreviewTemplate, BuyerName, YBM.YBookingNo
                )
                SELECT *,Count(*) Over() TotalRows FROM FinalList ";
            }
            else if (status == Status.Proposed)
            {
                sql = $@"
                ;WITH M As 
                (
                    Select YBM.YDBookingMasterID, YBM.YDBookingNo, YBM.YDBookingBy, YBM.YDBookingDate,
                    YBM.SwatchFilePath, FCM.GroupConceptNo, FCM.BookingId, YBM.PreviewTemplate, YBM.BuyerID, 
                    YBM.Remarks, YBM.IsApprove, YBM.IsAcknowledge, --YBM.YBookingID,
	                (Case When FCM.IsBDS = 0 Then 'R&D' Else c.ShortName End) As BuyerName,  
                    ProgramName = CASE WHEN FCM.IsBDS = 0 THEN 'Concept' 
					                WHEN FCM.IsBDS = 1 THEN 'BDS' 
					                WHEN FCM.IsBDS = 2 THEN 'Bulk'
					                WHEN FCM.IsBDS = 3 THEN 'Projection'
					                ELSE '-' End
                    From YDBookingMaster YBM
	                Inner JOIN FreeConceptMaster FCM ON FCM.GroupConceptNo = YBM.GroupConceptNo
                    Inner JOIN FreeConceptMRMaster FCMR ON FCMR.ConceptID = FCM.ConceptID
	                LEFT JOIN {DbNames.EPYSL}..Contacts C ON YBM.BuyerID = C.ContactID
                    Where FCMR.RevisionNo = YBM.PreProcessRevNo And YBM.SendForApproval = 1 AND YBM.IsApprove = 0 
	                Group By YBM.YDBookingMasterID, YBM.YDBookingNo, YBM.YDBookingBy, YBM.YDBookingDate,
                    YBM.SwatchFilePath, FCM.GroupConceptNo, FCM.BookingId, YBM.PreviewTemplate, YBM.BuyerID, 
                    YBM.Remarks, YBM.IsApprove, YBM.IsAcknowledge, 
	                FCM.IsBDS, c.ShortName 
                ),
                YBM AS
				(
					SELECT YBM.BookingID, YBM.YBookingNo
					FROM YarnBookingMaster_New YBM
					INNER JOIN M ON M.BookingID = YBM.BookingID
					GROUP BY YBM.BookingID, YBM.YBookingNo
				),
                FinalList AS
                (
                    SELECT YDBookingMasterID, YDBookingNo, Remarks, IsAcknowledge, YDBookingBy, BuyerID, GroupConceptNo, ProgramName, 
                    YBM.YBookingNo, YDBookingDate, SwatchFilePath, PreviewTemplate, BuyerName
                    FROM M 
                    LEFT JOIN YBM ON YBM.BookingID = M.BookingID
                    GROUP BY YDBookingMasterID, YDBookingNo, Remarks, IsAcknowledge, YDBookingBy, BuyerID, GroupConceptNo, ProgramName, 
                    YBM.YBookingNo, YDBookingDate, SwatchFilePath, PreviewTemplate, BuyerName
                )
                SELECT *,Count(*) Over() TotalRows FROM FinalList";
            }
            else if (status == Status.Approved)
            {
                sql = $@"
                ;WITH M As 
                (
                    Select YBM.YDBookingMasterID, YBM.YDBookingNo, YBM.YDBookingBy, YBM.YDBookingDate,
                    YBM.SwatchFilePath, FCM.GroupConceptNo, YBM.PreviewTemplate, YBM.BuyerID, 
                    YBM.Remarks, YBM.IsApprove, YBM.IsAcknowledge,
                    IsSample = CASE WHEN ISNULL(SBM.BookingID,0) > 0 THEN 1 ELSE 0 END,
	                (Case When FCM.IsBDS = 0 Then 'R&D' Else c.ShortName End) As BuyerName,  
                      ProgramName = CASE WHEN FCM.IsBDS = 0 THEN 'Concept' 
					                WHEN FCM.IsBDS = 1 THEN 'BDS' 
					                WHEN FCM.IsBDS = 2 THEN 'Bulk'
					                WHEN FCM.IsBDS = 3 THEN 'Projection'
					                ELSE '-' End ,FCM.BookingID
                    From YDBookingMaster YBM
	                Inner JOIN FreeConceptMaster FCM ON FCM.GroupConceptNo = YBM.GroupConceptNo
                    Inner JOIN FreeConceptMRMaster FCMR ON FCMR.ConceptID = FCM.ConceptID
	                LEFT JOIN {DbNames.EPYSL}..Contacts C ON YBM.BuyerID = C.ContactID
                    LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = FCM.BookingID
                    Where FCMR.RevisionNo = YBM.PreProcessRevNo And SendForApproval = 1 
                    And YBM.IsApprove = 1 And YBM.IsAcknowledge = 0
	                Group By YBM.YDBookingMasterID, YBM.YDBookingNo, YBM.YDBookingBy, YBM.YDBookingDate, 
                    YBM.SwatchFilePath, FCM.GroupConceptNo, YBM.PreviewTemplate, YBM.BuyerID, 
                    YBM.Remarks, YBM.IsApprove, YBM.IsAcknowledge, 
	                FCM.IsBDS, c.ShortName ,FCM.BookingID,
                    CASE WHEN ISNULL(SBM.BookingID,0) > 0 THEN 1 ELSE 0 END
                ),
                YBM AS
				(
					SELECT YBM.BookingID, YBM.YBookingNo
					FROM YarnBookingMaster_New YBM
					INNER JOIN M ON M.BookingID = YBM.BookingID
					GROUP BY YBM.BookingID, YBM.YBookingNo
				),
                FinalList AS
                (
	                SELECT YDBookingMasterID, YDBookingNo, Remarks, IsAcknowledge, YDBookingBy, BuyerID, GroupConceptNo, ProgramName, 
	                YBM.YBookingNo, YDBookingDate, SwatchFilePath, PreviewTemplate, BuyerName, M.IsSample
	                FROM M 
                    LEFT JOIN YBM ON YBM.BookingID = M.BookingID
                    GROUP BY YDBookingMasterID, YDBookingNo, Remarks, IsAcknowledge, YDBookingBy, BuyerID, GroupConceptNo, ProgramName, 
	                YBM.YBookingNo, YDBookingDate, SwatchFilePath, PreviewTemplate, BuyerName, M.IsSample
                )
                SELECT * INTO #TempTable{tempGuid} FROM FinalList
				SELECT *, Count(*) Over() TotalRows FROM #TempTable{tempGuid}";

                isNeedImage = true;
            }
            else if (status == Status.Revise)
            {
                sql = $@"
                ;WITH M As 
                (
                    Select YBM.YDBookingMasterID, YBM.YDBookingNo, YBM.YDBookingBy, YBM.YDBookingDate, 
                    YBM.SwatchFilePath, FCM.GroupConceptNo, FCM.BookingID, YBM.PreviewTemplate, YBM.BuyerID, 
                    YBM.Remarks, YBM.IsApprove, YBM.IsAcknowledge, --YBM.YBookingID,
	                (Case When FCM.IsBDS = 0 Then 'R&D' Else c.ShortName End) As BuyerName,  
                    ProgramName = CASE WHEN FCM.IsBDS = 0 THEN 'Concept' 
					                WHEN FCM.IsBDS = 1 THEN 'BDS' 
					                WHEN FCM.IsBDS = 2 THEN 'Bulk'
					                WHEN FCM.IsBDS = 3 THEN 'Projection'
					                ELSE '-' End
                    From YDBookingMaster YBM
	                Inner JOIN FreeConceptMaster FCM ON FCM.GroupConceptNo = YBM.GroupConceptNo
                    Inner JOIN FreeConceptMRMaster FCMR ON FCMR.ConceptID = FCM.ConceptID
	                LEFT JOIN {DbNames.EPYSL}..Contacts C ON YBM.BuyerID = C.ContactID
                    Where FCMR.RevisionNo != YBM.PreProcessRevNo
	                Group By YBM.YDBookingMasterID, YBM.YDBookingNo, YBM.YDBookingBy, YBM.YDBookingDate,
                    YBM.SwatchFilePath, FCM.GroupConceptNo, FCM.BookingID, YBM.PreviewTemplate, YBM.BuyerID, 
                    YBM.Remarks, YBM.IsApprove, YBM.IsAcknowledge, 
	                FCM.IsBDS, c.ShortName 
                ),
                YBM AS
                (
	                SELECT YBM.BookingID, YBM.YBookingNo
	                FROM YarnBookingMaster_New YBM
	                INNER JOIN M ON M.BookingID = YBM.BookingID
	                GROUP BY YBM.BookingID, YBM.YBookingNo
                ),
                FinalList AS
                (
                    SELECT YDBookingMasterID, YDBookingNo, Remarks, IsAcknowledge, YDBookingBy, BuyerID, GroupConceptNo, ProgramName, 
                    YBM.YBookingNo, YDBookingDate, SwatchFilePath, PreviewTemplate, BuyerName
                    FROM M 
	                LEFT JOIN YBM ON YBM.BookingID = M.BookingID
                    GROUP BY YDBookingMasterID, YDBookingNo, Remarks, IsAcknowledge, YDBookingBy, BuyerID, GroupConceptNo, ProgramName, 
                    YBM.YBookingNo, YDBookingDate, SwatchFilePath, PreviewTemplate, BuyerName
                )
                SELECT * INTO #TempTable{tempGuid} FROM FinalList
                SELECT *, Count(*) Over() TotalRows FROM #TempTable{tempGuid}";
            }
            else if (status == Status.UnAcknowledge)
            {
                sql = $@"
                ;WITH M As 
                (
                    Select YBM.YDBookingMasterID, YBM.YDBookingNo, YBM.YDBookingBy, YBM.YDBookingDate, 
                    YBM.SwatchFilePath, FCM.GroupConceptNo, FCM.BookingID, YBM.PreviewTemplate, YBM.BuyerID, 
                    YBM.Remarks, YBM.IsApprove, YBM.IsAcknowledge,
                    IsSample = CASE WHEN ISNULL(SBM.BookingID,0) > 0 THEN 1 ELSE 0 END,
	                (Case When FCM.IsBDS = 0 Then 'R&D' Else c.ShortName End) As BuyerName,  
                    ProgramName = CASE WHEN FCM.IsBDS = 0 THEN 'Concept' 
					                                WHEN FCM.IsBDS = 1 THEN 'BDS' 
					                                WHEN FCM.IsBDS = 2 THEN 'Bulk'
					                                WHEN FCM.IsBDS = 3 THEN 'Projection'
					                                ELSE '-' End,YBM.UnAckReason 
                    From YDBookingMaster YBM
	                Inner JOIN FreeConceptMaster FCM ON FCM.GroupConceptNo = YBM.GroupConceptNo
                    Inner JOIN FreeConceptMRMaster FCMR ON FCMR.ConceptID = FCM.ConceptID
	                LEFT JOIN {DbNames.EPYSL}..Contacts C ON YBM.BuyerID = C.ContactID
                    LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = FCM.BookingID
                    Where FCMR.RevisionNo = YBM.PreProcessRevNo And SendForApproval = 1 
                    And YBM.IsApprove = 1 And YBM.IsUnAcknowledge = 1
	                Group By YBM.YDBookingMasterID, YBM.YDBookingNo, YBM.YDBookingBy, YBM.YDBookingDate,
                    YBM.SwatchFilePath, FCM.GroupConceptNo, FCM.BookingID, YBM.PreviewTemplate, YBM.BuyerID, 
                    YBM.Remarks, YBM.IsApprove, YBM.IsAcknowledge, 
	                FCM.IsBDS, c.ShortName ,YBM.UnAckReason,CASE WHEN ISNULL(SBM.BookingID,0) > 0 THEN 1 ELSE 0 END
                ),
                YBM AS
                (
	                SELECT YBM.BookingID, YBM.YBookingNo
	                FROM YarnBookingMaster_New YBM
	                INNER JOIN M ON M.BookingID = YBM.BookingID
	                GROUP BY YBM.BookingID, YBM.YBookingNo
                ),
                FinalList AS
                (
                    SELECT YDBookingMasterID, YDBookingNo, Remarks, IsAcknowledge, YDBookingBy, BuyerID, GroupConceptNo, ProgramName, 
                    YBM.YBookingNo, YDBookingDate, SwatchFilePath, PreviewTemplate, BuyerName,UnAckReason, M.IsSample
                    FROM M 
	                LEFT JOIN YBM ON YBM.BookingID = M.BookingID
                    GROUP BY YDBookingMasterID, YDBookingNo, Remarks, IsAcknowledge, YDBookingBy, BuyerID, GroupConceptNo, ProgramName, 
                    YBM.YBookingNo, YDBookingDate, SwatchFilePath, PreviewTemplate, BuyerName,UnAckReason, M.IsSample
                )
                SELECT *, Count(*) Over() TotalRows FROM FinalList";

                isNeedImage = true;
            }
            else if (status == Status.Acknowledge)
            {
                sql = $@"
                ;WITH M As 
                (
                    Select YBM.YDBookingMasterID, YBM.YDBookingNo, YBM.YDBookingBy, YBM.YDBookingDate, 
                    YBM.SwatchFilePath, FCM.GroupConceptNo, FCM.BookingID, YBM.PreviewTemplate, YBM.BuyerID, 
                    YBM.Remarks, YBM.IsApprove, YBM.IsAcknowledge, 
                    IsSample = CASE WHEN ISNULL(SBM.BookingID,0) > 0 THEN 1 ELSE 0 END,
	                (Case When FCM.IsBDS = 0 Then 'R&D' Else c.ShortName End) As BuyerName,  
                    ProgramName = CASE WHEN FCM.IsBDS = 0 THEN 'Concept' 
					                WHEN FCM.IsBDS = 1 THEN 'BDS' 
					                WHEN FCM.IsBDS = 2 THEN 'Bulk'
					                WHEN FCM.IsBDS = 3 THEN 'Projection'
					                ELSE '-' End 
                    From YDBookingMaster YBM
	                Inner JOIN FreeConceptMaster FCM ON FCM.GroupConceptNo = YBM.GroupConceptNo
                    Inner JOIN FreeConceptMRMaster FCMR ON FCMR.ConceptID = FCM.ConceptID
	                LEFT JOIN {DbNames.EPYSL}..Contacts C ON YBM.BuyerID = C.ContactID
                    LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = FCM.BookingID
                    Where FCMR.RevisionNo = YBM.PreProcessRevNo And YBM.SendForApproval = 1 
                    And YBM.IsApprove = 1 And YBM.IsAcknowledge = 1 And YBM.IsYDBNoGenerated = 0
	                Group By YBM.YDBookingMasterID, YBM.YDBookingNo, YBM.YDBookingBy, YBM.YDBookingDate,
                    YBM.SwatchFilePath, FCM.GroupConceptNo, FCM.BookingID, YBM.PreviewTemplate, YBM.BuyerID, 
                    YBM.Remarks, YBM.IsApprove, YBM.IsAcknowledge, 
	                FCM.IsBDS, c.ShortName,CASE WHEN ISNULL(SBM.BookingID,0) > 0 THEN 1 ELSE 0 END
                ),
				YBM AS
				(
					SELECT YBM.BookingID, YBM.YBookingNo
					FROM YarnBookingMaster_New YBM
					INNER JOIN M ON M.BookingID = YBM.BookingID
					GROUP BY YBM.BookingID, YBM.YBookingNo
				),
                FinalList AS
                (
                    SELECT YDBookingMasterID, YDBookingNo, Remarks, IsAcknowledge, YDBookingBy, BuyerID, GroupConceptNo, ProgramName, 
                    YBM.YBookingNo, YDBookingDate, SwatchFilePath, PreviewTemplate, BuyerName, M.IsSample
                    FROM M 
					LEFT JOIN YBM ON YBM.BookingID = M.BookingID
                    GROUP BY YDBookingMasterID, YDBookingNo, Remarks, IsAcknowledge, YDBookingBy, BuyerID, GroupConceptNo, ProgramName, 
                    YBM.YBookingNo, YDBookingDate, SwatchFilePath, PreviewTemplate, BuyerName, M.IsSample
                )
                SELECT *, Count(*) Over() TotalRows FROM FinalList";
            }
            else if (status == Status.Completed)
            {
                sql = $@"
                ;WITH M As 
                (
                    Select YBM.YDBookingMasterID, YBM.YDBookingNo, YBM.YDBookingBy, YBM.YDBookingDate, 
                    YBM.SwatchFilePath, FCM.GroupConceptNo, FCM.BookingID, YBM.PreviewTemplate, YBM.BuyerID, 
                    YBM.Remarks, YBM.IsApprove, YBM.IsAcknowledge, 
                    IsSample = CASE WHEN ISNULL(SBM.BookingID,0) > 0 THEN 1 ELSE 0 END,
	                (Case When FCM.IsBDS = 0 Then 'R&D' Else c.ShortName End) As BuyerName,  
                    ProgramName = CASE WHEN FCM.IsBDS = 0 THEN 'Concept' 
					                WHEN FCM.IsBDS = 1 THEN 'BDS' 
					                WHEN FCM.IsBDS = 2 THEN 'Bulk'
					                WHEN FCM.IsBDS = 3 THEN 'Projection'
					                ELSE '-' End
                    ,YBM.YDBNo,YBM.IsYDBNoGenerated
                    From YDBookingMaster YBM
	                Inner JOIN FreeConceptMaster FCM ON FCM.GroupConceptNo = YBM.GroupConceptNo
                    Inner JOIN FreeConceptMRMaster FCMR ON FCMR.ConceptID = FCM.ConceptID
	                LEFT JOIN {DbNames.EPYSL}..Contacts C ON YBM.BuyerID = C.ContactID
                    LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = FCM.BookingID
                    Where FCMR.RevisionNo = YBM.PreProcessRevNo And YBM.SendForApproval = 1 
                    And YBM.IsApprove = 1 And YBM.IsAcknowledge = 1 And YBM.IsYDBNoGenerated = 1
	                Group By YBM.YDBookingMasterID, YBM.YDBookingNo, YBM.YDBookingBy, YBM.YDBookingDate,
                    YBM.SwatchFilePath, FCM.GroupConceptNo, FCM.BookingID, YBM.PreviewTemplate, YBM.BuyerID, 
                    YBM.Remarks, YBM.IsApprove, YBM.IsAcknowledge, 
	                FCM.IsBDS, c.ShortName,CASE WHEN ISNULL(SBM.BookingID,0) > 0 THEN 1 ELSE 0 END, YBM.YDBNo,YBM.IsYDBNoGenerated
                ),
                YBM AS
                (
	                SELECT YBM.BookingID, YBM.YBookingNo
	                FROM YarnBookingMaster_New YBM
	                INNER JOIN M ON M.BookingID = YBM.BookingID
	                GROUP BY YBM.BookingID, YBM.YBookingNo
                ),
                MRS AS
                (
	                SELECT YDBM.YDBookingMasterID,SUM(YDRC.BookingQty) BookingQty,SUM(ReqQty) ReqQty 
	                FROM YDReqChild YDRC 
	                INNER JOIN YDReqMaster YDRM ON YDRM.YDReqMasterID = YDRC.YDReqMasterID
	                INNER JOIN M YDBM ON YDBM.YDBookingMasterID = YDRM.YDBookingMasterID
	                GROUP BY YDBM.YDBookingMasterID
                ),
                RRS AS
                (
	                SELECT 
		                A.YDBookingMasterID,BTotal = COALESCE(A.TotalColour, 0), RTotal = COALESCE(B.TotalColour, 0),
		                ColourDifference = COALESCE(A.TotalColour, 0) - COALESCE(B.TotalColour, 0)
	                FROM
		                (
			                SELECT YDBookingMasterID, TotalColour = COUNT(ColorId)
			                FROM (
				                SELECT YDBC.YDBookingMasterID, YDBC.ColorId
				                FROM YDBookingChild YDBC
				                INNER JOIN M YDBM ON YDBM.YDBookingMasterID = YDBC.YDBookingMasterID
				                GROUP BY YDBC.YDBookingMasterID, YDBC.ColorId
			                ) V
			                GROUP BY V.YDBookingMasterID
		                ) A
	                LEFT JOIN
		                (
			                SELECT YDBookingMasterID, TotalColour = COUNT(ColorId)
			                FROM (
				                SELECT YDBC.YDBookingMasterID, YDRRM.ColorId
				                FROM YDRecipeRequestMaster YDRRM
				                INNER JOIN YDBookingChild YDBC ON YDBC.YDBookingChildID = YDRRM.YDBookingChildID
				                GROUP BY YDBC.YDBookingMasterID, YDRRM.ColorId
			                ) V
			                GROUP BY V.YDBookingMasterID
		                ) B
	                ON A.YDBookingMasterID = B.YDBookingMasterID
                ),
                R AS
                (
	                SELECT YDBC.YDBookingMasterID, ReceiveQty = SUM(YDRcvC.ReceiveQty), BookingQty = SUM(YDBC.BookingQty)
	                FROM YDReceiveChild YDRcvC
	                INNER JOIN YDReqChild YDRC ON YDRC.YDReqChildID = YDRcvC.YDReqChildID
	                INNER JOIN YDReqMaster YDRM ON YDRM.YDReqMasterID = YDRC.YDReqMasterID
	                INNER JOIN YDBookingChild YDBC ON YDBC.YDBookingMasterID = YDRM.YDBookingMasterID
	                GROUP BY YDBC.YDBookingMasterID
                ),
                FinalList AS
                (
                    SELECT M.YDBookingMasterID, YDBookingNo, Remarks, IsAcknowledge, YDBookingBy, BuyerID, GroupConceptNo, ProgramName, 
                    YBM.YBookingNo, YDBookingDate, SwatchFilePath, PreviewTemplate, BuyerName, M.IsSample,M.YDBNo,M.IsYDBNoGenerated,
	                CASE 
		                WHEN ISNULL(MRS.ReqQty,0) >= ISNULL(MRS.BookingQty,0) THEN 'Done' 
		                WHEN ISNULL(MRS.ReqQty,0) < ISNULL(MRS.BookingQty,0) THEN ('Partially Done ('+ Convert(varchar,MRS.ReqQty) +' kg out of '+ convert(varchar,MRS.BookingQty) +' kg)') 
		                WHEN ISNULL(MRS.ReqQty,0) = 0 THEN ('Not Started')
		                ELSE ''
	                END MRStatus,
	                CASE 
		                WHEN RRS.BTotal <= RRS.RTotal THEN 'Done' 
		                WHEN RRS.BTotal > RRS.RTotal AND RRS.RTotal != 0 THEN ('Partially Done ('+ Convert(varchar,RRS.RTotal) +' out of '+ convert(varchar,RRS.BTotal) +' )') 
		                WHEN RRS.BTotal = RRS.ColourDifference THEN ('Not Started')
		                ELSE ''
	                END RRStatus,
	                CASE 
		                WHEN ISNULL(R.ReceiveQty,0) >= ISNULL(R.BookingQty,0) THEN 'Done' 
		                WHEN ISNULL(R.ReceiveQty,0) < ISNULL(R.BookingQty,0) THEN ('Partially Done ('+ Convert(varchar,R.ReceiveQty) +' kg out of '+ convert(varchar,R.BookingQty) +' kg)') 
		                WHEN ISNULL(R.ReceiveQty,0) = 0 THEN ('Not Started')
		                ELSE ''
	                END RStatus
                    FROM M 
	                LEFT JOIN YBM ON YBM.BookingID = M.BookingID
	                LEFT JOIN MRS ON MRS.YDBookingMasterID = M.YDBookingMasterID
	                LEFT JOIN RRS ON RRS.YDBookingMasterID = M.YDBookingMasterID
	                LEFT JOIN R ON R.YDBookingMasterID = M.YDBookingMasterID
                    GROUP BY M.YDBookingMasterID, YDBookingNo, Remarks, IsAcknowledge, YDBookingBy, BuyerID, GroupConceptNo, ProgramName, 
                    YBM.YBookingNo, YDBookingDate, SwatchFilePath, PreviewTemplate, BuyerName, M.IsSample,M.YDBNo,M.IsYDBNoGenerated,
	                MRS.BookingQty,MRS.ReqQty,RRS.ColourDifference,RRS.RTotal,RRS.BTotal,R.ReceiveQty,R.BookingQty
                )
                SELECT *, Count(*) Over() TotalRows FROM FinalList";
            }
            else
            {
                sql = $@"
                ;WITH M As 
                (
                    Select YBM.YDBookingMasterID, YBM.YDBookingNo, YBM.YDBookingBy, YBM.YDBookingDate, 
                    YBM.SwatchFilePath, FCM.GroupConceptNo, FCM.BookingID, YBM.PreviewTemplate, YBM.BuyerID, 
                    YBM.Remarks, YBM.IsApprove, YBM.IsAcknowledge, 
                    IsSample = CASE WHEN ISNULL(SBM.BookingID,0) > 0 THEN 1 ELSE 0 END,
	                (Case When FCM.IsBDS = 0 Then 'R&D' Else c.ShortName End) As BuyerName,  
                    ProgramName = CASE WHEN FCM.IsBDS = 0 THEN 'Concept' 
					                WHEN FCM.IsBDS = 1 THEN 'BDS' 
					                WHEN FCM.IsBDS = 2 THEN 'Bulk'
					                WHEN FCM.IsBDS = 3 THEN 'Projection'
					                ELSE '-' End 
                    From YDBookingMaster YBM
	                Inner JOIN FreeConceptMaster FCM ON FCM.GroupConceptNo = YBM.GroupConceptNo
                    Inner JOIN FreeConceptMRMaster FCMR ON FCMR.ConceptID = FCM.ConceptID
	                LEFT JOIN {DbNames.EPYSL}..Contacts C ON YBM.BuyerID = C.ContactID
                    LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = FCM.BookingID
                    Where FCMR.RevisionNo = YBM.PreProcessRevNo And YBM.SendForApproval = 1 
                    And YBM.IsApprove = 1 And YBM.IsAcknowledge = 1
	                Group By YBM.YDBookingMasterID, YBM.YDBookingNo, YBM.YDBookingBy, YBM.YDBookingDate,
                    YBM.SwatchFilePath, FCM.GroupConceptNo, FCM.BookingID, YBM.PreviewTemplate, YBM.BuyerID, 
                    YBM.Remarks, YBM.IsApprove, YBM.IsAcknowledge, 
	                FCM.IsBDS, c.ShortName,CASE WHEN ISNULL(SBM.BookingID,0) > 0 THEN 1 ELSE 0 END
                ),
				YBM AS
				(
					SELECT YBM.BookingID, YBM.YBookingNo
					FROM YarnBookingMaster_New YBM
					INNER JOIN M ON M.BookingID = YBM.BookingID
					GROUP BY YBM.BookingID, YBM.YBookingNo
				),
                FinalList AS
                (
                    SELECT YDBookingMasterID, YDBookingNo, Remarks, IsAcknowledge, YDBookingBy, BuyerID, GroupConceptNo, ProgramName, 
                    YBM.YBookingNo, YDBookingDate, SwatchFilePath, PreviewTemplate, BuyerName, M.IsSample
                    FROM M 
					LEFT JOIN YBM ON YBM.BookingID = M.BookingID
                    GROUP BY YDBookingMasterID, YDBookingNo, Remarks, IsAcknowledge, YDBookingBy, BuyerID, GroupConceptNo, ProgramName, 
                    YBM.YBookingNo, YDBookingDate, SwatchFilePath, PreviewTemplate, BuyerName, M.IsSample
                )
                SELECT *, Count(*) Over() TotalRows FROM FinalList";

                isNeedImage = true;
            }

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            if (status == Status.Approved || status == Status.Revise)
            {
                sql += $@" DROP TABLE #TempTable{tempGuid} ";
            }

            var ydBookings = await _service.GetDataAsync<YDBookingMaster>(sql);

            if (isNeedImage)
            {
                string bookingNos = string.Join("','", ydBookings.Where(x => x.ProgramName == "Bulk").Select(x => x.GroupConceptNo).Distinct());
                if (bookingNos.IsNotNullOrEmpty())
                {
                    ydBookings.Where(x => x.ProgramName == "Bulk" && x.ImagePath.IsNotNullOrEmpty()).ToList().ForEach(x => x.ImagePath = "");
                    ydBookings.Where(x => x.ProgramName == "Bulk" && x.ImagePath1.IsNotNullOrEmpty()).ToList().ForEach(x => x.ImagePath1 = "");

                    var fBookingAcknowledgeImages = await _service.GetDataAsync<FBookingAcknowledge>(CommonQueries.GetImagePathQuery(bookingNos, "TP"));
                    fBookingAcknowledgeImages.ForEach(x =>
                    {
                        var obj = ydBookings.Find(y => y.ProgramName == "Bulk" && y.GroupConceptNo == x.BookingNo);
                        if (obj.IsNotNull()) ydBookings.Find(y => y.ProgramName == "Bulk" && y.GroupConceptNo == x.BookingNo).ImagePath = x.ImagePath;
                    });
                    fBookingAcknowledgeImages = await _service.GetDataAsync<FBookingAcknowledge>(CommonQueries.GetImagePathQuery(bookingNos, "BK"));
                    fBookingAcknowledgeImages.ForEach(x =>
                    {
                        var obj = ydBookings.Find(y => y.ProgramName == "Bulk" && y.GroupConceptNo == x.BookingNo);
                        if (obj.IsNotNull()) ydBookings.Find(y => y.ProgramName == "Bulk" && y.GroupConceptNo == x.BookingNo).ImagePath1 = x.ImagePath;
                    });
                }
            }
            return ydBookings;
        }
        public async Task<YDBookingMaster> GetNew(string id, string pName)
        {
            var segmentNames = new
            {
                SegmentNames = new string[]
                {
                    ItemSegmentNameConstants.FABRIC_COLOR
                }
            };
            string query;
            if (pName == "Concept" || pName == "BDS")
            {
                query = $@"
                --master
                SELECT FCM.GroupConceptNo, FCM.BuyerID, (Case When FCM.IsBDS = 0 Then 'R&D' Else c.ShortName End) As BuyerName, 
                FCM.BuyerTeamID, FCM.IsBDS, 
                Max(FCMRR.RevisionNo)PreProcessRevNo,FCM.ConceptID,
                ReqTypeID = CAST( CASE WHEN FCM.IsBDS = 0 THEN {EnumReqType.YD_CONCEPT} 
						               WHEN FCM.IsBDS = 1 THEN {EnumReqType.YD_SAMPLE} 
						               WHEN FCM.IsBDS = 2 THEN {EnumReqType.YD_BULK}
						               --WHEN FCM.IsBDS = 3 THEN 'YD_PROJECTION'
						               ELSE 0 End As int)
                FROM FreeConceptMaster FCM
                Inner Join FreeConceptMRMaster FCMRR On FCMRR.ConceptID = FCM.ConceptID
                Left Join {DbNames.EPYSL}..Contacts c On c.ContactID = FCM.BuyerID   
                WHERE FCM.GroupConceptNo = '{id}'
                Group By FCM.GroupConceptNo, FCM.BuyerID, c.ShortName, FCM.BuyerTeamID, FCM.IsBDS,FCM.ConceptID;

                -- Child
                With FCM As 
                (
	                SELECT FCM.GroupConceptNo, FCM.ConceptID
	                FROM FreeConceptMaster FCM
                    Where FCM.GroupConceptNo = '{id}'
                ),
                MR AS (
	                Select FCM.GroupConceptNo, b.ItemMasterId, b.YD, b.YDItem, Sum(b.ReqQty) ReqQty, max(b.ReqCone) ReqCone, 
	                Max(a.RevisionNo) RevisionNo, b.YBChildItemID, b.FCMRChildID, b.YarnCategory, b.ShadeCode, 
	                YSS.PhysicalCount, LotNo = YSS.YarnLotNo, YSS.SpinnerId, SpinnerName = SP.ShortName
	                From FCM  
	                Inner Join FreeConceptMRMaster a On a.ConceptID = FCM.ConceptID
	                Inner Join FreeConceptMRChild b on b.FCMRMasterID = a.FCMRMasterID
	                LEFT JOIN YarnStockSet YSS ON YSS.YarnStockSetId = b.YarnStockSetId
	                LEFT JOIN {DbNames.EPYSL}..Contacts SP ON SP.ContactID = YSS.SpinnerId
	                Where b.YD = 1  
	                Group by FCM.GroupConceptNo, b.ItemMasterId, b.YD, b.YDItem, b.YBChildItemID, b.FCMRChildID, b.YarnCategory, b.ShadeCode,
                    YSS.PhysicalCount,YSS.YarnLotNo,YSS.SpinnerId,SP.ShortName
                ) 
                Select ROW_NUMBER() OVER(ORDER BY MR.ItemMasterID) YDBookingChildID, ROW_NUMBER() OVER(ORDER BY MR.ItemMasterID) YDBookingChild_DemoID,
                '' As ColorName, '' As ColorCode, 0 As IsTwisting,
                0 As IsWaxing, ReqQty As BookingQty, ReqCone As NoOfCone, UN.DisplayUnitDesc, UN.UnitID, MR.YD, MR.YDItem,
                MR.ItemMasterID, ISV1.SegmentValueID Segment1ValueId, ISV2.SegmentValueID Segment2ValueId, ISV3.SegmentValueID Segment3ValueId, 
                ISV4.SegmentValueID Segment4ValueId, ISV5.SegmentValueID Segment5ValueId, ISV6.SegmentValueID Segment6ValueId, 
                ISV7.SegmentValueID Segment7ValueId, ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, 
                ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, 
                ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,
                MR.YBChildItemID, MR.FCMRChildID, MR.YarnCategory, MR.ShadeCode,
                MR.ShadeCode, MR.PhysicalCount, MR.LotNo, MR.SpinnerId, MR.SpinnerName
                FROM MR INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = MR.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT JOIN {DbNames.EPYSL}..Unit UN ON IM.DefaultTranUnitID = UN.UnitID;

                -- Item Segments
                {CommonQueries.GetItemSegmentValuesBySegmentNamesWithSegmentName()};

                -- YarnDyeingFor
                {CommonQueries.GetYarnDyeingFor()};

                -- Color childs
                ;Select C.CCColorID, C.ColorID, C.ColorCode, ISV.SegmentValue ColorName, FCBS.RGBOrHex, C.Remarks
                From FreeConceptChildColor C 
                Inner Join FreeConceptMaster FCM On C.ConceptID = FCM.ConceptID
                LEFT Join {DbNames.EPYSL}..FabricColorBookSetup FCBS ON FCBS.ColorID = C.ColorID
                LEFT Join {DbNames.EPYSL}..ItemSegmentValue ISV On FCBS.ColorID = ISV.SegmentValueID
                where FCM.GroupConceptNo = '{id}'
                Group By C.CCColorID, C.ColorID, C.ColorCode, ISV.SegmentValue, FCBS.RGBOrHex, C.Remarks;

                -- Spinner
                Select Cast(C.ContactID As varchar) [id], C.Name [text]
                From {DbNames.EPYSL}..Contacts C
                Inner Join {DbNames.EPYSL}..ContactCategoryChild CCC On C.ContactID = CCC.ContactID
                Inner Join {DbNames.EPYSL}..ContactCategoryHK CC ON CC.ContactCategoryID = CCC.ContactCategoryID
                Where CC.ContactCategoryName = '{ContactCategoryNames.SPINNER}'
                Union
                Select Cast(ContactID As varchar) [id], Name [text] from {DbNames.EPYSL}..Contacts;

                -- Uses in
                SELECT Cast(U.FUPartID As varchar) [id], U.PartName [text]
                FROM {DbNames.EPYSL}..FabricUsedPart U;

                -- Shade book
                {CommonQueries.GetYarnShadeBooks()};
                ";
            }
            else
            {
                query = $@"
                 SELECT YBM.YBookingNo, FBA.BookingNo As GroupConceptNo, YBM.BuyerID, C.ShortName BuyerName, 
                YBM.BuyerTeamID, YBM.ExportOrderID,  Max(YBM.RevisionNo)PreProcessRevNo, IsBDS = 2, FCM.ConceptID,
                ReqTypeID = CAST( CASE WHEN FCM.IsBDS = 0 THEN {EnumReqType.YD_CONCEPT} 
						               WHEN FCM.IsBDS = 1 THEN {EnumReqType.YD_SAMPLE} 
						               WHEN FCM.IsBDS = 2 THEN {EnumReqType.YD_BULK}
						               --WHEN FCM.IsBDS = 3 THEN 'YD_PROJECTION'
						               ELSE 0 End As int)
                FROM YarnBookingMaster_New YBM
                LEFT JOIN {DbNames.EPYSL}..Contacts C ON YBM.BuyerID=C.ContactID
				INNER JOIN FBookingAcknowledge FBA ON FBA.BookingID = YBM.BookingID
				INNER JOIN FreeConceptMaster FCM ON FCM.BookingID = YBM.BookingID 
                WHERE YBM.YBookingNo LIKE '%{id}%'
                Group By YBM.YBookingNo, FBA.BookingNo, YBM.BuyerID, C.ShortName, YBM.BuyerTeamID, YBM.ExportOrderID, FCM.ConceptID,
                        CASE WHEN FCM.IsBDS = 0 THEN {EnumReqType.YD_CONCEPT} 
						     WHEN FCM.IsBDS = 1 THEN {EnumReqType.YD_SAMPLE} 
						     WHEN FCM.IsBDS = 2 THEN {EnumReqType.YD_BULK}
						     --WHEN FCM.IsBDS = 3 THEN 'YD_PROJECTION'
						     ELSE 0 End;

                  ;With 
                FBAFinalApp AS
                (
	                SELECT FBA.BookingID
	                FROM FBookingAcknowledge FBA
	                WHERE FBA.IsApprovedByPMC = 1
	                GROUP BY FBA.BookingID
                ),
                YA AS
                (
	                SELECT YACI.YarnStockSetId, FB.BookingID, YBCI.YBChildItemID, YACI.AllocationChildItemID
	                FROM YarnAllocationChildItem YACI
	                INNER JOIN YarnAllocationChild YAC ON YAC.AllocationChildID = YACI.AllocationChildID
	                INNER JOIN YarnAllocationMaster YAM ON YAM.YarnAllocationID = YAC.AllocationID
	                INNER JOIN YarnBookingChildItem_New YBCI ON YBCI.YBChildItemID = YAC.YBChildItemID
	                INNER JOIN YarnBookingChild_New YBC ON YBC.YBChildID = YBCI.YBChildID
	                INNER JOIN YarnBookingMaster_New YBM ON YBM.YBookingID = YBC.YBookingID
	                LEFT JOIN FBAFinalApp FB ON FB.BookingID = YBM.BookingID
	                WHERE YAC.YBookingNo LIKE '%{id}%' AND YBCI.YD = 1 AND YACI.Acknowledge = 1
	                GROUP BY YACI.YarnStockSetId, FB.BookingID, YBCI.YBChildItemID,YACI.AllocationChildItemID
                ),--Select * FROM YA,
                A AS 
                (
                    SELECT YSS.YarnStockSetId, YBM.BookingID, YBM.YBookingNo, 
					ItemMasterID = Case When Isnull(YSS.ItemMasterId,0)>0 Then YSS.ItemMasterId Else YBCI.YItemMasterID END, 
					IM.ItemName,SUM(YBCI.NetYarnReqQty) BookingQty, YBCI.UnitID, YBCI.YD, YA.AllocationChildItemID,
					(CASE WHEN YBM.ExportOrderID = 0 THEN 'BDS' ELSE 'BULK' END) AS ProgramName,
                    UN.DisplayUnitDesc, YBM.BuyerID, YBM.ExportOrderID, C.ShortName BuyerName, SUM(ISNULL(YDBC.BookingQty,0)) AS YDBookingQty, 
					YarnCategory = Case When Isnull(YSS.ItemMasterId,0)>0 Then YSS.YarnCategory Else YBCI.YarnCategory END, 
					PhysicalCount = CASE WHEN ISNULL(YSS.YarnStockSetId,0) > 0 THEN YSS.PhysicalCount ELSE ISV6.SegmentValue END,
	                LotNo = CASE WHEN ISNULL(YSS.YarnStockSetId,0) > 0 THEN YSS.YarnLotNo ELSE YBCI.YarnLotNo END,
	                SpinnerId = CASE WHEN ISNULL(YSS.YarnStockSetId,0) > 0 THEN YSS.SpinnerId ELSE YBCI.SpinnerId END,


                    ISV1.SegmentValueID Segment1ValueId, ISV2.SegmentValueID Segment2ValueId, ISV3.SegmentValueID Segment3ValueId, 
                    ISV4.SegmentValueID Segment4ValueId, ISV5.SegmentValueID Segment5ValueId, ISV6.SegmentValueID Segment6ValueId, 
                    ISV7.SegmentValueID Segment7ValueId, ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, 
                    ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, 
                    ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc, YBCI.ShadeCode, YBCI.YBChildItemID
                    FROM YarnBookingChildItem_New YBCI
	                INNER JOIN YarnBookingChild_New YBC ON YBC.YBChildID = YBCI.YBChildID
                    INNER JOIN YarnBookingMaster_New YBM ON YBM.YBookingID = YBC.YBookingID
                    INNER JOIN FBAFinalApp FAPP ON FAPP.BookingID = YBM.BookingID
	                LEFT JOIN YA ON YA.YBChildItemID = YBCI.YBChildItemID
	                LEFT JOIN YarnStockSet YSS ON YSS.YarnStockSetId = YA.YarnStockSetId
                    LEFT JOIN {DbNames.EPYSL}..Contacts C ON YBM.BuyerID = C.ContactID
                    LEFT JOIN YDBookingChild YDBC ON YBCI.YBChildItemID = YDBC.YBChildItemID
                    LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = Case When Isnull(YSS.ItemMasterId,0)>0 Then YSS.ItemMasterId Else YBCI.YItemMasterID END
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                    LEFT JOIN {DbNames.EPYSL}..Unit UN ON YBCI.UnitID = UN.UnitID
                    WHERE YBCI.YD = 1
	                AND YBM.YBookingNo LIKE '%{id}%'
                    GROUP BY YSS.YarnStockSetId, YBM.BookingID, Case When Isnull(YSS.ItemMasterId,0)>0 Then YSS.ItemMasterId Else YBCI.YItemMasterID END,
					IM.ItemName, YBM.YBookingNo, YBCI.UnitID,ProgramName,UN.DisplayUnitDesc,YBM.BuyerID, YA.AllocationChildItemID,
					Case When Isnull(YSS.ItemMasterId,0)>0 Then YSS.YarnCategory Else YBCI.YarnCategory END, 
					YBM.ExportOrderID,C.ShortName,ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue, ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue,YBCI.YD,
                    ISV7.SegmentValue, YBCI.ShadeCode, ISV1.SegmentValueID, ISV2.SegmentValueID, ISV3.SegmentValueID, ISV4.SegmentValueID,	ISV5.SegmentValueID, 
	                ISV6.SegmentValueID, ISV7.SegmentValueID, YBCI.YBChildItemID,CASE WHEN ISNULL(YSS.YarnStockSetId,0) > 0 THEN YSS.PhysicalCount ELSE ISV6.SegmentValue END,
	                CASE WHEN ISNULL(YSS.YarnStockSetId,0) > 0 THEN YSS.YarnLotNo ELSE YBCI.YarnLotNo END,
	                CASE WHEN ISNULL(YSS.YarnStockSetId,0) > 0 THEN YSS.SpinnerId ELSE YBCI.SpinnerId END
                )
                SELECT ROW_NUMBER() OVER(ORDER BY BookingID) YDBookingChildID, YarnStockSetId, BookingID YBookingID, ItemMasterID,ItemName,BookingQty AS FBookingQty,(BookingQty-YDBookingQty) AS BookingQty, PhysicalCount,
                UnitID =28, ProgramName,DisplayUnitDesc = 'Kg',BuyerID,ExportOrderID, BuyerName,YDBookingQty,Segment1ValueDesc, Segment2ValueDesc, Segment3ValueDesc, Segment4ValueDesc, Segment5ValueDesc, YD,
                Segment6ValueDesc, Segment7ValueDesc,ShadeCode, Segment1ValueId, Segment2ValueId, Segment3ValueId, Segment4ValueId, Segment5ValueId, Segment6ValueId, Segment7ValueId,YBChildItemID,YarnCategory,
                SpinnerId, LotNo, PhysicalCount, AllocationChildItemID,
                ROW_NUMBER() OVER(ORDER BY BookingID) YDBookingChild_DemoID
                FROM A 
                WHERE BookingQty > YDBookingQty;

                -- Item Segments
                {CommonQueries.GetItemSegmentValuesBySegmentNamesWithSegmentName()};

                -- YarnDyeingFor
                {CommonQueries.GetYarnDyeingFor()};

                -- Spinner
                Select Cast(C.ContactID As varchar) [id], C.Name [text]
                From {DbNames.EPYSL}..Contacts C
                Inner Join {DbNames.EPYSL}..ContactCategoryChild CCC On C.ContactID = CCC.ContactID
                Inner Join {DbNames.EPYSL}..ContactCategoryHK CC ON CC.ContactCategoryID = CCC.ContactCategoryID
                Where CC.ContactCategoryName = '{ContactCategoryNames.SPINNER}'
                Union
                Select Cast(ContactID As varchar) [id], Name [text] from {DbNames.EPYSL}..Contacts;

                -- Uses in
                SELECT Cast(U.FUPartID As varchar) [id], U.PartName [text]
                FROM {DbNames.EPYSL}..FabricUsedPart U; 

                -- Shade book
                {CommonQueries.GetYarnShadeBooks()};
                ";
            }

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query, segmentNames);
                YDBookingMaster data = records.Read<YDBookingMaster>().FirstOrDefault();
                data.YDBookingChilds = records.Read<YDBookingChild>().ToList();

                int fcMrChildId = 1;
                data.YDBookingChilds.ForEach(x =>
                {
                    x.SpinnerName = x.SpinnerID == 0 ? "" : x.SpinnerName;
                    if (x.FCMRChildID <= 0)
                    {
                        x.FCMRChildID = fcMrChildId++;
                    }
                });


                List<YDBookingChildTwisting> YDBookingChildTwistings = new List<YDBookingChildTwisting>();
                data.YDBookingChildTwistings = YDBookingChildTwistings;
                List<YDBookingChildTwistingColor> YDBookingChildTwistingColors = new List<YDBookingChildTwistingColor>();
                data.YDBookingChildTwistingColors = YDBookingChildTwistingColors;

                data.YarnColorList = await records.ReadAsync<Select2OptionModel>();
                data.YarnDyeingForList = await records.ReadAsync<Select2OptionModel>();
                if (pName == "Concept" || pName == "BDS") data.ColorChilds = records.Read<FreeConceptChildColor>().ToList();
                data.SpinnerList = await records.ReadAsync<Select2OptionModel>();
                data.UsesInList = await records.ReadAsync<Select2OptionModel>();
                data.YarnShadeBooks = await records.ReadAsync<Select2OptionModel>();
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
        public async Task<YDBookingMaster> GetAsync(int id, string pName)
        {
            var segmentNames = new
            {
                SegmentNames = new string[]
               {
                        ItemSegmentNameConstants.FABRIC_COLOR
               }
            };
            string query;
            if (pName == "Concept")
            {
                query =
                $@"
                -- Master Data
                SELECT YDB.YDBookingMasterID, YDB.YDBookingNo, YDB.YDBookingDate, YDB.Remarks, YDB.ExportOrderID, YDB.BuyerID,
                (Case When FCM.IsBDS = 0 Then 'R&D' Else c.ShortName End) As BuyerName, 
                FCM.GroupConceptNo, YDB.BuyerTeamID, FCM.IsBDS, 
                --Max(YDB.RevisionNo) As PreProcessRevNo,
                Max(FCMR.RevisionNo) As PreProcessRevNo,YDB.YDBNo,YDB.IsYDBNoGenerated
                FROM YDBookingMaster YDB
                INNER JOIN FreeConceptMaster FCM ON FCM.GroupConceptNo = YDB.GroupConceptNo
                Inner JOIN FreeConceptMRMaster FCMR ON FCMR.ConceptID = FCM.ConceptID
                Left Join {DbNames.EPYSL}..Contacts c On c.ContactID = FCM.BuyerID   
                WHERE YDBookingMasterID = {id} 
                Group By YDB.YDBookingMasterID, YDB.YDBookingNo, YDB.YDBookingDate, YDB.Remarks, YDB.ExportOrderID, YDB.BuyerID,
                c.ShortName, FCM.GroupConceptNo, YDB.BuyerTeamID, FCM.IsBDS,YDB.YDBNo,YDB.IsYDBNoGenerated;

                -- Childs Data
                With YDBC As 
                (
	                Select * From YDBookingChild WHERE YDBookingMasterID = {id} 
                )
                SELECT YDBC.YDBookingChildID, YDBC.YDBookingMasterID, YDBC.ItemMasterID, YDBC.BookingQty,
                IM.DefaultTranUnitID UnitID, UN.DisplayUnitDesc, YDBC.PrintedDensity,
                YDBC.ProgramName, YDBC.NoOfThread, YDBC.Remarks, YDBC.NoOfCone, YDBC.BookingFor, YDBC.IsTwisting, 
                YDBC.IsWaxing, YDBC.UsesIn, YDBC.ShadeCode, YDBC.IsAdditionalItem, YDBC.LotNo, YDBC.PhysicalCount, 
                YDBC.SpinnerID, SpinnerName = CASE WHEN ISNULL(YDBC.SpinnerID,0) > 0 THEN Spinner.ShortName ELSE '' END, YDBC.YDBookingChildID YDBookingChild_DemoID,  
                Color.SegmentValue AS ColorName, YDBC.ColorId, YDBC.ColorCode,
                ISV1.SegmentValueID Segment1ValueId, 
                ISV2.SegmentValueID Segment2ValueId, ISV3.SegmentValueID Segment3ValueId, ISV4.SegmentValueID Segment4ValueId,
                ISV5.SegmentValueID Segment5ValueId, ISV6.SegmentValueID Segment6ValueId, ISV7.SegmentValueID Segment7ValueId,
                ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, 
                ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, 
                ISV7.SegmentValue Segment7ValueDesc, DF.YDyeingFor BookingForName, 
                Max(Isnull(YPC.YDProductionMasterID,0))YDProductionMasterID, YDBC.ColorBatchRefID, YDBC.ColorBatchRef 
                FROM YDBC 
                LEFT Join {DbNames.EPYSL}..Contacts Spinner On Spinner.ContactID = YDBC.SpinnerID  
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = YDBC.ColorId
                LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YDBC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT JOIN {DbNames.EPYSL}..Unit UN ON YDBC.UnitID = UN.UnitID
                LEFT JOIN YarnDyeingFor_HK DF ON DF.YDyeingForID = YDBC.BookingFor 
                Left Join YDProductionMaster YPC On YPC.YDBookingMasterID = YDBC.YDBookingMasterID 
                Group By YDBC.YDBookingChildID, YDBC.YDBookingMasterID, YDBC.ItemMasterID, YDBC.BookingQty,
                IM.DefaultTranUnitID, UN.DisplayUnitDesc, YDBC.PrintedDensity,
                YDBC.ProgramName, YDBC.NoOfThread, YDBC.Remarks, YDBC.NoOfCone, YDBC.BookingFor, YDBC.IsTwisting, 
                YDBC.IsWaxing, YDBC.UsesIn, YDBC.ShadeCode, YDBC.IsAdditionalItem, YDBC.LotNo, YDBC.PhysicalCount, 
                YDBC.SpinnerID, CASE WHEN ISNULL(YDBC.SpinnerID,0) > 0 THEN Spinner.ShortName ELSE '' END, YDBC.YDBookingChildID,  
                Color.SegmentValue, YDBC.ColorId, YDBC.ColorCode,
                ISV1.SegmentValueID, 
                ISV2.SegmentValueID, ISV3.SegmentValueID, ISV4.SegmentValueID,
                ISV5.SegmentValueID, ISV6.SegmentValueID, ISV7.SegmentValueID,
                ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue, 
                ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue, 
                ISV7.SegmentValue, DF.YDyeingFor, YDBC.ColorBatchRefID, YDBC.ColorBatchRef;    

                -- Twisted Child
                With YDBC As 
                (
	                Select * From YDBookingChildTwisting WHERE YDBookingMasterID = {id} 
                )
                SELECT YDBC.YDBCTwistingID, YDBC.YDBookingMasterID, YDBC.ItemMasterID, YDBC.BookingQty, 
                IM.DefaultTranUnitID UnitID, UN.DisplayUnitDesc, YDBC.ProgramName, YDBC.NoOfThread,
                YDBC.Remarks, YDBC.NoOfCone, YDBC.IsTwisting, YDBC.IsWaxing, YDBC.UsesIn, YDBC.ShadeCode, YDBC.PrintedDensity, 
                YDBC.TPI,
                Color.SegmentValue AS ColorName,YDBC.ColorId,YDBC.ColorCode, ISV1.SegmentValueID Segment1ValueId, 
                ISV2.SegmentValueID Segment2ValueId, ISV3.SegmentValueID Segment3ValueId, ISV4.SegmentValueID Segment4ValueId,
                ISV5.SegmentValueID Segment5ValueId, ISV6.SegmentValueID Segment6ValueId, ISV7.SegmentValueID Segment7ValueId,
                ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, 
                ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, 
                ISV7.SegmentValue Segment7ValueDesc,YDBC.PhysicalCount, Isnull(YPC.YDProductionMasterID,0)YDProductionMasterID

                FROM YDBC  
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = YDBC.ColorId
                LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YDBC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT JOIN {DbNames.EPYSL}..Unit UN ON YDBC.UnitID = UN.UnitID
                Left Join YDProductionChild YPC On YPC.YDBCTwistingID = YDBC.YDBCTwistingID 
 
                Group By YDBC.YDBCTwistingID, YDBC.YDBookingMasterID, YDBC.ItemMasterID, YDBC.BookingQty, 
                IM.DefaultTranUnitID, UN.DisplayUnitDesc, YDBC.ProgramName, YDBC.NoOfThread,
                YDBC.Remarks, YDBC.NoOfCone, YDBC.IsTwisting, YDBC.IsWaxing, YDBC.UsesIn, YDBC.ShadeCode, YDBC.PrintedDensity, 
                YDBC.TPI,
                Color.SegmentValue, YDBC.ColorId, YDBC.ColorCode, ISV1.SegmentValueID, 
                ISV2.SegmentValueID, ISV3.SegmentValueID, ISV4.SegmentValueID,
                ISV5.SegmentValueID, ISV6.SegmentValueID, ISV7.SegmentValueID,
                ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue, 
                ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue, 
                ISV7.SegmentValue, YDBC.PhysicalCount, Isnull(YPC.YDProductionMasterID,0);

                -- Twisted Color 
                Select YBCTC.YDBookingChildID As PrimaryTwistingColorID, YBCTC.YDBCTwistingColorID, YBCTC.YDBCTwistingID,
                YBCTC.YDBookingChildID, YBCTC.ColorId As ColorID, Color.SegmentValue AS ColorName, YBCTC.ColorCode,
                YBC.PhysicalCount, YDBC.LotNo, YDBC.BookingFor,BookingForName = DF.YDyeingFor, YBC.TPI, YBC.BookingQty,
                YDBC.ItemMasterID, ISV1.SegmentValue As Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc,
                ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc,
                ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc,
                ISV7.SegmentValue Segment7ValueDesc, YBCTC.TwistingColorQty, YBCTC.TwistingColorQty AS AssignQty, YBCTC.YDBookingMasterID
                From YDBookingChildTwistingColors YBCTC
                LEFT JOIN YDBookingChildTwisting YBC ON YBC.YDBCTwistingID = YBCTC.YDBCTwistingID
                Inner JOIN YDBookingChild YDBC On YBCTC.YDBookingChildID = YDBC.YDBookingChildID
                LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YDBC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = YBCTC.ColorId
                LEFT JOIN YarnDyeingFor_HK DF ON DF.YDyeingForID = YDBC.BookingFor
                Where YDBC.YDBookingMasterID = {id}
                Group By YBCTC.YDBookingChildID, YBCTC.YDBCTwistingColorID, YBCTC.YDBCTwistingID, YBCTC.YDBookingChildID,
                YBCTC.ColorId, Color.SegmentValue, YBCTC.ColorCode, ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,
                YDBC.ItemMasterID, ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue, YBCTC.TwistingColorQty,
                YBCTC.YDBookingMasterID, YBC.PhysicalCount, YDBC.LotNo, YDBC.BookingFor,DF.YDyeingFor, YBC.TPI, YBC.BookingQty;

                -- Print Colors
                Select PC.*, Color.SegmentValue ColorName
                From YDBookingPrintColor PC
                INNER JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = PC.ColorID
                WHERE PC.YDBookingMasterID = {id};

                -- Uses In
                SELECT U.*, P.PartName UsesInName
                FROM YDBookingChildUsesIn U
                INNER JOIN {DbNames.EPYSL}..FabricUsedPart P ON P.FUPartID = U.UsesIn
                WHERE U.YDBookingMasterID = {id};

                -- Color childs
                Select C.CCColorID, C.ColorID, C.ColorCode, ISV.SegmentValue ColorName, FCBS.RGBOrHex, C.Remarks
                From FreeConceptChildColor C
                Inner Join FreeConceptMaster FCM On C.ConceptID = FCM.ConceptID
                LEFT Join {DbNames.EPYSL}..FabricColorBookSetup FCBS ON FCBS.ColorID = C.ColorID
                LEFT Join {DbNames.EPYSL}..ItemSegmentValue ISV On FCBS.ColorID = ISV.SegmentValueID
                Left Join YDBookingMaster YBM On YBM.GroupConceptNo = FCM.GroupConceptNo
                where YBM.YDBookingMasterID = {id}
                Group By C.CCColorID, C.ColorID, C.ColorCode, ISV.SegmentValue, FCBS.RGBOrHex, C.Remarks;

                -- Yarn Dyeing For
                Select Cast(Y.YDyeingForID As varchar) [id], Y.YDyeingFor [text]
                From YarnDyeingFor_HK Y

                -- Spinner
                Select Cast(C.ContactID As varchar) [id], C.Name [text]
                From {DbNames.EPYSL}..Contacts C
                Inner Join {DbNames.EPYSL}..ContactCategoryChild CCC On C.ContactID = CCC.ContactID
                Inner Join {DbNames.EPYSL}..ContactCategoryHK CC ON CC.ContactCategoryID = CCC.ContactCategoryID
                Where CC.ContactCategoryName = '{ContactCategoryNames.SPINNER}'
                Union
                Select Cast(ContactID As varchar) [id], Name [text] from {DbNames.EPYSL}..Contacts;

                -- Uses in List
                SELECT Cast(U.FUPartID As varchar) [id], U.PartName [text]
                FROM {DbNames.EPYSL}..FabricUsedPart U;

                -- Shade book
                {CommonQueries.GetYarnShadeBooks()}; 

                -- YDBookingChild Twisting Uses In
				SELECT TUI.*, P.PartName UsesInName
				FROM YDBookingChildTwistingUsesIn TUI
				INNER JOIN {DbNames.EPYSL}..FabricUsedPart P ON P.FUPartID = TUI.UsesIn
				WHERE TUI.YDBookingMasterID = {id}";
            }
            else
            {
                query =
                $@"
                    -- Master Data
                    WITH A AS
                    (
	                    Select YDB.YDBookingMasterID,YDB.YDBookingNo,YDB.YDBookingDate,YDB.BuyerID,C.ShortName BuyerName,
	                    YDB.Remarks, YDB.RevisionNo As PreProcessRevNo, YDB.GroupConceptNo,YDB.YDBNo,YDB.IsYDBNoGenerated
	                    From YDBookingMaster YDB
	                    LEFT JOIN {DbNames.EPYSL}..Contacts C ON YDB.BuyerID = C.ContactID
	                    Where YDBookingMasterID = {id}
                    ),
                    C AS
                    (
	                    SELECT FCM.GroupConceptNo, FCM.IsBDS
	                    FROM FreeConceptMaster FCM
	                    INNER JOIN A ON A.GroupConceptNo = FCM.GroupConceptNo
	                    GROUP BY FCM.GroupConceptNo, FCM.IsBDS
                    )
                    SELECT A.*, C.IsBDS
                    FROM A
                    LEFT JOIN C ON C.GroupConceptNo = A.GroupConceptNo

                    -- Childs Data
                   SELECT YDBC.YDBookingChildID, YDBC.YDBookingMasterID, YDBC.ItemMasterID,IM.ItemName,YDBC.BookingQty BookingQty,YDBC.BookingQty SavedQty,IM.DefaultTranUnitID UnitID,UN.DisplayUnitDesc, YDBC.PrintedDensity,
                    YDBC.ProgramName,YDBC.NoOfThread,YDBC.Remarks, YDBC.NoOfCone, FBookingQty = YDBC.BookingQty, YDBookingQty = YDBC.BookingQty,
					--YBC.BookingQty FBookingQty,(select SUM(ISNULL(BookingQty,0)) FROM YDBookingChild YC WHERE YC.YBChildItemID = YBC.YBChildItemID) AS YDBookingQty,
                    Color.SegmentValue AS ColorName,YDBC.ColorId,YDBC.ColorCode, YDBC.BookingFor, YDBC.IsTwisting, YDBC.IsWaxing, YDBC.UsesIn, YDBC.ShadeCode, YDBC.IsAdditionalItem,
                    YDBC.LotNo, YDBC.PhysicalCount, YDBC.SpinnerID, SpinnerName = CASE WHEN ISNULL(YDBC.SpinnerID,0) > 0 THEN Spinner.ShortName ELSE '' END, YDBC.YDBookingChildID YDBookingChild_DemoID,
                    ISV1.SegmentValueID Segment1ValueId, ISV2.SegmentValueID Segment2ValueId, ISV3.SegmentValueID Segment3ValueId, ISV4.SegmentValueID Segment4ValueId,
                    ISV5.SegmentValueID Segment5ValueId, ISV6.SegmentValueID Segment6ValueId, ISV7.SegmentValueID Segment7ValueId,
                    ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc,
                    ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc, 
                    DF.YDyeingFor BookingForName, --Isnull(YPC.YDProductionMasterID,0)YDProductionMasterID
                    Max(Isnull(YPC.YDProductionMasterID,0))YDProductionMasterID, YDBC.ColorBatchRefID, YDBC.ColorBatchRef
                    FROM YDBookingChild YDBC
                    LEFT Join {DbNames.EPYSL}..Contacts Spinner On Spinner.ContactID = YDBC.SpinnerID
                    INNER JOIN YDBookingMaster YDBM ON YDBM.YDBookingMasterID = YDBC.YDBookingMasterID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = YDBC.ColorId
                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON YDBC.ItemMasterID = IM.ItemMasterID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                    LEFT JOIN {DbNames.EPYSL}..Unit UN ON YDBC.UnitID = UN.UnitID
                    LEFT JOIN YarnDyeingFor_HK DF ON DF.YDyeingForID = YDBC.BookingFor 
                    Left Join YDProductionMaster YPC On YPC.YDBookingMasterID = YDBM.YDBookingMasterID
                    WHERE YDBC.YDBookingMasterID = {id}
                    Group By YDBC.YDBookingChildID, YDBC.YDBookingMasterID, YDBC.ItemMasterID,IM.ItemName,YDBC.BookingQty,
                    IM.DefaultTranUnitID,UN.DisplayUnitDesc, YDBC.PrintedDensity, --YBC.YBChildItemID, YBC.BookingQty, 
                    YDBC.ProgramName,YDBC.NoOfThread,YDBC.Remarks, YDBC.NoOfCone, 
                    Color.SegmentValue,YDBC.ColorId,YDBC.ColorCode, YDBC.BookingFor, YDBC.IsTwisting, YDBC.IsWaxing, YDBC.UsesIn, YDBC.ShadeCode, YDBC.IsAdditionalItem,
                    YDBC.LotNo, YDBC.PhysicalCount, YDBC.SpinnerID, CASE WHEN ISNULL(YDBC.SpinnerID,0) > 0 THEN Spinner.ShortName ELSE '' END, YDBC.YDBookingChildID,
                    ISV1.SegmentValueID, ISV2.SegmentValueID, ISV3.SegmentValueID, ISV4.SegmentValueID,
                    ISV5.SegmentValueID, ISV6.SegmentValueID, ISV7.SegmentValueID,
                    ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue, ISV4.SegmentValue,
                    ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue, 
                    DF.YDyeingFor, YDBC.ColorBatchRefID, YDBC.ColorBatchRef;

                    -- Twisted Child
                    SELECT YDBC.YDBCTwistingID, YDBC.YDBookingMasterID, YDBC.ItemMasterID,YDBC.BookingQty BookingQty,YDBC.BookingQty SavedQty,IM.DefaultTranUnitID UnitID,UN.DisplayUnitDesc,
                    YDBC.ProgramName,YDBC.NoOfThread,YDBC.Remarks,YDBC.NoOfCone, YDBC.IsTwisting, YDBC.IsWaxing, YDBC.UsesIn, YDBC.ShadeCode,
                    FMRC.ReqQty FBookingQty,(select SUM(ISNULL(BookingQty,0)) FROM YDBookingChild YC  where YC.FCMRChildID = FMRC.FCMRChildID) AS YDBookingQty,
                    Color.SegmentValue AS ColorName,YDBC.ColorId,YDBC.ColorCode, YDBC.PrintedDensity, YDBC.TPI,
                    ISV1.SegmentValueID, ISV2.SegmentValueID, ISV3.SegmentValueID, ISV4.SegmentValueID,
                    ISV5.SegmentValueID, ISV6.SegmentValueID, ISV7.SegmentValueID,
                    ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc,
                    ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,
                    YDBC.PhysicalCount, Isnull(YPC.YDProductionMasterID,0)YDProductionMasterID
                    FROM YDBookingChildTwisting YDBC
                    INNER JOIN YDBookingmaster YDBM ON YDBM.YDBookingMasterID=YDBC.YDBookingMasterID
                    LEFT JOIN FreeConceptMRChild FMRC ON YDBC.FCMRChildID=FMRC.FCMRChildID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = YDBC.ColorId
                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YDBC.ItemMasterID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                    LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                    LEFT JOIN {DbNames.EPYSL}..Unit UN ON YDBC.UnitID = UN.UnitID
                    Left Join YDProductionChild YPC On YPC.YDBCTwistingID = YDBC.YDBCTwistingID 
                    WHERE YDBC.YDBookingMasterID = {id};

                    -- Twisted Color 
                    Select YBCTC.YDBookingChildID As PrimaryTwistingColorID, YBCTC.YDBCTwistingColorID, YBCTC.YDBCTwistingID,
                    YBCTC.YDBookingChildID, YBCTC.ColorId As ColorID, Color.SegmentValue AS ColorName, YBCTC.ColorCode,
                    ISV1.SegmentValue As Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc,
                    ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc,
                    ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc,
                    ISV7.SegmentValue Segment7ValueDesc, YBCTC.TwistingColorQty, YBCTC.TwistingColorQty AS AssignQty, 
                    YBCTC.YDBookingMasterID,YBC.TPI, YBC.BookingQty,
                    YBC.PhysicalCount, YDBC.LotNo, YDBC.BookingFor,BookingForName = DF.YDyeingFor
                    From YDBookingChildTwistingColors YBCTC
                    LEFT JOIN YDBookingChildTwisting YBC ON YBC.YDBCTwistingID = YBCTC.YDBCTwistingID
                    Inner JOIN YDBookingChild YDBC On YBCTC.YDBookingChildID = YDBC.YDBookingChildID
                    LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YDBC.ItemMasterID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = YDBC.ColorId
                    LEFT JOIN YarnDyeingFor_HK DF ON DF.YDyeingForID = YDBC.BookingFor
                    Where YDBC.YDBookingMasterID = {id}
                    Group By YBCTC.YDBookingChildID, YBCTC.YDBCTwistingColorID, YBCTC.YDBCTwistingID, YBCTC.YDBookingChildID,
                    YBCTC.ColorId, Color.SegmentValue, YBCTC.ColorCode, ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,
                    ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue, YBCTC.TwistingColorQty,
                    YBCTC.YDBookingMasterID,YBC.TPI, YBC.BookingQty,YBC.PhysicalCount, YDBC.LotNo, YDBC.BookingFor,DF.YDyeingFor;

                    -- Print Colors
                    Select PC.*, Color.SegmentValue ColorName
                    From YDBookingPrintColor PC
                    INNER JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = PC.ColorID
                    WHERE PC.YDBookingMasterID = {id};

                    -- Uses In
                    SELECT U.*, P.PartName UsesInName
                    FROM YDBookingChildUsesIn U
                    INNER JOIN {DbNames.EPYSL}..FabricUsedPart P ON P.FUPartID = U.UsesIn
                    WHERE U.YDBookingMasterID = {id};

                    -- Yarn Dyeing For
                    Select Cast(Y.YDyeingForID As varchar) [id], Y.YDyeingFor [text]
                    From YarnDyeingFor_HK Y

                    -- Spinner
                    Select Cast(C.ContactID As varchar) [id], C.Name [text]
                    From {DbNames.EPYSL}..Contacts C
                    Inner Join {DbNames.EPYSL}..ContactCategoryChild CCC On C.ContactID = CCC.ContactID
                    Inner Join {DbNames.EPYSL}..ContactCategoryHK CC ON CC.ContactCategoryID = CCC.ContactCategoryID
                    Where CC.ContactCategoryName = '{ContactCategoryNames.SPINNER}'
                    Union
                    Select Cast(ContactID As varchar) [id], Name [text] from {DbNames.EPYSL}..Contacts;

                    -- Uses in list
                    SELECT Cast(U.FUPartID As varchar) [id], U.PartName [text]
                    FROM {DbNames.EPYSL}..FabricUsedPart U;

                    -- Shade book
                    {CommonQueries.GetYarnShadeBooks()};
                    
                    -- YDBookingChild Twisting Uses In
				    SELECT TUI.*, P.PartName UsesInName
				    FROM YDBookingChildTwistingUsesIn TUI
				    INNER JOIN {DbNames.EPYSL}..FabricUsedPart P ON P.FUPartID = TUI.UsesIn
				    WHERE TUI.YDBookingMasterID = {id}";
            }

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                YDBookingMaster data = await records.ReadFirstOrDefaultAsync<YDBookingMaster>();
                Guard.Against.NullObject(data);

                data.YDBookingChilds = records.Read<YDBookingChild>().ToList();
                data.YDBookingChildTwistings = records.Read<YDBookingChildTwisting>().ToList();
                data.YDBookingChildTwistingColors = records.Read<YDBookingChildTwistingColor>().ToList();

                data.YDBookingChildTwistings.ForEach(x =>
                {
                    x.ColorIDs = string.Join(",", data.YDBookingChildTwistingColors.Where(y => y.YDBCTwistingID == x.YDBCTwistingID).Select(y => y.ColorID).Distinct());
                });

                #region Set FCMRChildID (Set value for unique operation. Inreal FCMRChildID no need)

                int fcMrChildId = 1;
                List<int> tdBookingChildIds = new List<int>();

                data.YDBookingChildTwistings.ToList().ForEach(x =>
                {
                    x.FCMRChildID = fcMrChildId;
                    data.YDBookingChildTwistingColors.Where(y => y.YDBCTwistingID == x.YDBCTwistingID).ToList().ForEach(y =>
                    {
                        y.FCMRChildID = fcMrChildId;
                    });
                    data.YDBookingChilds.Where(z => z.ItemMasterID == x.ItemMasterID).ToList().ForEach(z =>
                    {
                        z.FCMRChildID = fcMrChildId;
                        tdBookingChildIds.Add(z.YDBookingChildID);
                    });
                    fcMrChildId++;
                });

                data.YDBookingChilds.ForEach(x =>
                {
                    if (!tdBookingChildIds.Exists(y => y == x.YDBookingChildID))
                    {
                        x.FCMRChildID = fcMrChildId;
                        fcMrChildId++;
                    }
                });

                #endregion

                foreach (var item in data.YDBookingChildTwistings)
                {
                    item.YDBookingChildTwistingColors = data.YDBookingChildTwistingColors.FindAll(x => x.YDBCTwistingID == item.YDBCTwistingID);
                    if (item.YDBookingChildTwistingColors.Count() == 1)
                    {
                        var colorName = item.YDBookingChildTwistingColors.FirstOrDefault().ColorName;
                        if (colorName.IsNotNullOrEmpty())
                        {
                            item.TwistedColors = colorName + " + " + colorName;
                        }
                        else
                        {
                            item.TwistedColors = "";
                        }
                    }
                    else
                    {
                        List<string> colorNames = new List<string>();
                        item.YDBookingChildTwistingColors.ForEach(cc =>
                        {
                            if (cc.ColorName.IsNotNullOrEmpty())
                            {
                                colorNames.Add(cc.ColorName);
                            }
                        });
                        item.TwistedColors = string.Join(" + ", colorNames);
                    }
                    item.TwistedSelectedColorIDs = string.Join(",", item.YDBookingChildTwistingColors.Select(x => x.YDBookingChildID));
                }
                List<YDBookingPrintColor> printColors = records.Read<YDBookingPrintColor>().ToList();
                List<YDBookingChildUsesIn> usesIns = records.Read<YDBookingChildUsesIn>().ToList();
                foreach (var item in data.YDBookingChilds)
                {
                    item.PrintColors = printColors.FindAll(x => x.YDBookingChildID == item.YDBookingChildID);
                    item.PrintColorIDs = string.Join(",", item.PrintColors.Select(x => x.ColorID));
                    item.YDBookingChildUsesIns = usesIns.FindAll(x => x.YDBookingChildID == item.YDBookingChildID);
                    item.UsesIns = string.Join(",", item.YDBookingChildUsesIns.Select(x => x.UsesInName));
                    item.UsesInIDs = string.Join(",", item.YDBookingChildUsesIns.Select(x => x.UsesIn));
                }

                if (pName == "Concept") data.ColorChilds = records.Read<FreeConceptChildColor>().ToList();
                data.YarnDyeingForList = records.Read<Select2OptionModel>();
                data.SpinnerList = records.Read<Select2OptionModel>();
                data.UsesInList = await records.ReadAsync<Select2OptionModel>();
                data.YarnShadeBooks = await records.ReadAsync<Select2OptionModel>();

                var YDBookingChildTwistingUsesIn = await records.ReadAsync<YDBookingChildTwistingUsesIn>();
                data.YDBookingChildTwistings.ForEach(x =>
                {
                    x.YDBCTwistingUsesIns = YDBookingChildTwistingUsesIn.Where(y => y.YDBCTwistingID == x.YDBCTwistingID).ToList();
                    x.UsesInIDs = string.Join(",", x.YDBCTwistingUsesIns.Select(y => y.UsesIn));
                    x.UsesIns = string.Join(",", x.YDBCTwistingUsesIns.Select(y => y.UsesInName));
                });

                int maxColorIDForYarnTwistBookingFor = 99999;
                foreach (var item in data.YDBookingChilds.Where(x => x.ColorID == 0 && x.BookingFor == 3).ToList())
                {
                    item.ColorID = maxColorIDForYarnTwistBookingFor++;
                    foreach (var itemTC in data.YDBookingChildTwistingColors.Where(x => x.YDBookingChildID == item.YDBookingChildID && x.ColorID == 0).ToList())
                    {
                        itemTC.ColorID = item.ColorID;
                        if (data.YDBookingChildTwistings.Count() > 0)
                        {
                            data.YDBookingChildTwistings.FirstOrDefault(x => x.YDBCTwistingID == itemTC.YDBCTwistingID).ColorID = item.ColorID;
                        }
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
        public async Task<YDBookingMaster> GetReviseAsync(int id, string GroupConceptNo, string pName)
        {
            var segmentNames = new
            {
                SegmentNames = new string[]
               {
                    ItemSegmentNameConstants.FABRIC_COLOR
               }
            };
            string query;
            if (pName == "Concept")
            {
                query =
               $@"
                -- Master Data
                SELECT YDB.YDBookingMasterID, YDB.YDBookingNo, YDB.YDBookingDate, YDB.Remarks, YDB.ExportOrderID, YDB.BuyerID,
                (Case When FCM.IsBDS = 0 Then 'R&D' Else c.ShortName End) As BuyerName, FCM.GroupConceptNo, YDB.BuyerTeamID, 
                FCM.IsBDS, Max(FCMRR.RevisionNo)PreProcessRevNo
                FROM YDBookingMaster YDB
                INNER JOIN FreeConceptMaster FCM ON FCM.GroupConceptNo = YDB.GroupConceptNo
                Inner Join FreeConceptMRMaster FCMRR On FCMRR.ConceptID = FCM.ConceptID
                Left Join {DbNames.EPYSL}..Contacts c On c.ContactID = FCM.BuyerID   
                WHERE YDBookingMasterID = {id} 
                Group By YDB.YDBookingMasterID, YDB.YDBookingNo, YDB.YDBookingDate, YDB.Remarks, YDB.ExportOrderID, 
                YDB.BuyerID, c.ShortName, FCM.GroupConceptNo, YDB.BuyerTeamID, FCM.IsBDS;

                -- Childs Data 
                ;With MR AS (
	                Select c.GroupConceptNo, b.ItemMasterId, b.YD, b.YDItem, b.UnitID, Sum(b.ReqQty) ReqQty, 
                    Max(b.ReqCone) ReqCone, Max(a.RevisionNo) RevisionNo 
	                From FreeConceptMRMaster a
	                Inner Join FreeConceptMRChild b on b.FCMRMasterID = a.FCMRMasterID
	                Inner Join FreeConceptMaster c On c.ConceptID = a.ConceptID
	                Where b.YD = 1 And C.GroupConceptNo = '{GroupConceptNo}'
	                Group by c.GroupConceptNo, b.ItemMasterId, b.YD, b.YDItem, b.UnitID
                ) 
                Select (Case when Isnull(YDBC.YDBookingChildID,0) = 0 Then ROW_NUMBER() OVER(ORDER BY MR.ItemMasterID)
		                Else Isnull(YDBC.YDBookingChildID,0) End)YDBookingChildID, YBM.YDBookingMasterID, MR.ItemMasterID, 
                MR.ReqQty BookingQty, IM.DefaultTranUnitID UnitID, UN.DisplayUnitDesc, YDBC.PrintedDensity, YDBC.ProgramName, 
                YDBC.NoOfThread, YDBC.Remarks, MR.ReqCone As NoOfCone, YDBC.BookingFor, ISNULL(YDBC.IsTwisting,0) IsTwisting, 
                ISNULL(YDBC.IsWaxing,0) IsWaxing, ISNULL(YDBC.UsesIn,0) UsesIn, YDBC.ShadeCode, YDBC.IsAdditionalItem, 
                YDBC.LotNo, YDBC.PhysicalCount, YDBC.SpinnerID, SpinnerName = CASE WHEN ISNULL(YDBC.SpinnerID,0) > 0 THEN Spinner.ShortName ELSE '' END, YDBC.YDBookingChildID YDBookingChild_DemoID, 
                Color.SegmentValue AS ColorName,YDBC.ColorId,YDBC.ColorCode,
                ISV1.SegmentValueID Segment1ValueId, ISV2.SegmentValueID Segment2ValueId, ISV3.SegmentValueID Segment3ValueId, 
                ISV4.SegmentValueID Segment4ValueId, ISV5.SegmentValueID Segment5ValueId, ISV6.SegmentValueID Segment6ValueId, 
                ISV7.SegmentValueID Segment7ValueId, ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, 
                ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, 
                ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc, DF.YDyeingFor BookingForName,
                Max(Isnull(YPC.YDProductionMasterID,0))YDProductionMasterID, YDBC.ColorBatchRefID, YDBC.ColorBatchRef 
                From MR 
                Left Join YDBookingMaster YBM on YBM.GroupConceptNo = MR.GroupConceptNo 
                Left Join YDBookingChild YDBC on YDBC.YDBookingMasterID = YBM.YDBookingMasterID And YDBC.ItemMasterID = MR.ItemMasterId
	            And YDBC.YD = MR.YD And YDBC.YDItem = MR.YDItem 
                LEFT Join {DbNames.EPYSL}..Contacts Spinner On Spinner.ContactID = YDBC.SpinnerID 
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = YDBC.ColorId
                LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = MR.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT JOIN {DbNames.EPYSL}..Unit UN ON MR.UnitID = UN.UnitID
                LEFT JOIN YarnDyeingFor_HK DF ON DF.YDyeingForID = YDBC.BookingFor 
                Left Join YDProductionMaster YPC On YPC.YDBookingMasterID = YDBC.YDBookingMasterID
                Group By YDBC.YDBookingChildID, YBM.YDBookingMasterID, MR.ItemMasterID, MR.ReqQty, IM.DefaultTranUnitID, 
                UN.DisplayUnitDesc, YDBC.PrintedDensity, YDBC.ProgramName, YDBC.NoOfThread, YDBC.Remarks, MR.ReqCone,  
                YDBC.BookingFor, ISNULL(YDBC.IsTwisting,0), ISNULL(YDBC.IsWaxing,0), ISNULL(YDBC.UsesIn,0), 
                YDBC.ShadeCode, YDBC.IsAdditionalItem, YDBC.LotNo, YDBC.PhysicalCount, YDBC.SpinnerID, Spinner.ShortName, 
                YDBC.YDBookingChildID, Color.SegmentValue, YDBC.ColorId, YDBC.ColorCode, ISV1.SegmentValueID, ISV2.SegmentValueID, ISV3.SegmentValueID, 
                ISV4.SegmentValueID, ISV5.SegmentValueID, ISV6.SegmentValueID, ISV7.SegmentValueID, ISV1.SegmentValue, ISV2.SegmentValue, 
                ISV3.SegmentValue, ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue, DF.YDyeingFor, YDBC.ColorBatchRefID, YDBC.ColorBatchRef;

                -- Twisted Child
                ;With MR AS (
	                Select c.GroupConceptNo, b.ItemMasterId, b.YD, b.YDItem, b.UnitID,  
	                Sum(b.ReqQty) ReqQty, max(b.ReqCone) ReqCone, Max(a.RevisionNo) RevisionNo 
	                From FreeConceptMRMaster a
	                Inner Join FreeConceptMRChild b on b.FCMRMasterID = a.FCMRMasterID
	                Inner Join FreeConceptMaster c On c.ConceptID = a.ConceptID
	                Where b.YD = 1 And C.GroupConceptNo = '{GroupConceptNo}'
	                Group by c.GroupConceptNo, b.ItemMasterId, b.YD, b.YDItem, b.UnitID
                ) 
                Select  (Case when Isnull(YDBC.YDBCTwistingID,0) = 0 Then ROW_NUMBER() OVER(ORDER BY MR.ItemMasterID)
		                Else Isnull(YDBC.YDBCTwistingID,0) End)YDBCTwistingID,
                YBM.YDBookingMasterID, MR.ItemMasterID, MR.ReqQty BookingQty, 
                IM.DefaultTranUnitID UnitID,UN.DisplayUnitDesc, YDBC.ProgramName,
                YDBC.NoOfThread,YDBC.Remarks, MR.ReqCone As NoOfCone, 
                ISNULL(YDBC.IsTwisting,0)IsTwisting, ISNULL(YDBC.IsWaxing,0)IsWaxing, 
                ISNULL(YDBC.UsesIn,0) UsesIn, YDBC.ShadeCode, YDBC.PrintedDensity, YDBC.TPI,  
                Color.SegmentValue AS ColorName,YDBC.ColorId,YDBC.ColorCode, ISV1.SegmentValueID Segment1ValueId, 
                ISV2.SegmentValueID Segment2ValueId, ISV3.SegmentValueID Segment3ValueId, ISV4.SegmentValueID Segment4ValueId,
                ISV5.SegmentValueID Segment5ValueId, ISV6.SegmentValueID Segment6ValueId, ISV7.SegmentValueID Segment7ValueId,
                ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, 
                ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, 
                ISV7.SegmentValue Segment7ValueDesc,YDBC.PhysicalCount,
                Max(Isnull(YPC.YDProductionMasterID,0))YDProductionMasterID 
                From MR 
                Left Join YDBookingMaster YBM on YBM.GroupConceptNo = MR.GroupConceptNo 
                Left Join YDBookingChildTwisting YDBC on YDBC.YDBookingMasterID = YBM.YDBookingMasterID And YDBC.ItemMasterID = MR.ItemMasterId  
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = YDBC.ColorId
                LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = MR.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT JOIN {DbNames.EPYSL}..Unit UN ON MR.UnitID = UN.UnitID 
                Left Join YDProductionMaster YPC On YPC.YDBookingMasterID = YDBC.YDBookingMasterID
                Where ISNULL(YDBC.IsTwisting,0) = 1
                Group By YDBC.YDBCTwistingID, YBM.YDBookingMasterID, MR.ItemMasterID, YDBC.BookingQty, 
                IM.DefaultTranUnitID,UN.DisplayUnitDesc, YDBC.ProgramName, YDBC.NoOfThread, MR.ReqCone, YDBC.Remarks, YDBC.NoOfCone, 
                ISNULL(YDBC.IsTwisting,0), ISNULL(YDBC.IsWaxing,0), ISNULL(YDBC.UsesIn,0), YDBC.ShadeCode, YDBC.PrintedDensity, 
                YDBC.TPI, MR.ReqQty, Color.SegmentValue,YDBC.ColorId, YDBC.ColorCode, ISV1.SegmentValueID, 
                ISV2.SegmentValueID, ISV3.SegmentValueID, ISV4.SegmentValueID, ISV5.SegmentValueID, ISV6.SegmentValueID, ISV7.SegmentValueID,
                ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue, ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue, 
                ISV7.SegmentValue,YDBC.PhysicalCount; 

                 -- Twisted Color 
                ;With MR AS (
	                Select c.GroupConceptNo, b.ItemMasterId, b.YD, b.YDItem, b.UnitID,  
	                Sum(b.ReqQty) ReqQty, max(b.ReqCone) ReqCone, Max(a.RevisionNo) RevisionNo 
	                From FreeConceptMRMaster a
	                Inner Join FreeConceptMRChild b on b.FCMRMasterID = a.FCMRMasterID
	                Inner Join FreeConceptMaster c On c.ConceptID = a.ConceptID
	                Where b.YD = 1 And C.GroupConceptNo = '{GroupConceptNo}'
	                Group by c.GroupConceptNo, b.ItemMasterId, b.YD, b.YDItem, b.UnitID
                ) 
                Select  (Case when Isnull(YBCTC.YDBookingChildID,0) = 0 Then ROW_NUMBER() OVER(ORDER BY MR.ItemMasterID)
		                Else Isnull(YBCTC.YDBookingChildID,0) End)PrimaryTwistingColorID,
                YBCTC.YDBCTwistingColorID, YBCTC.YDBCTwistingID,
                YBCTC.YDBookingChildID, YBCTC.ColorId As ColorID, Color.SegmentValue AS ColorName, YBCTC.ColorCode,
                ISV1.SegmentValue As Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc,
                ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc,
                ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc,
                ISV7.SegmentValue Segment7ValueDesc, YBCTC.TwistingColorQty, YBCTC.YDBookingMasterID,
				YDBC.PhysicalCount, YDBC.LotNo, YDBC.BookingFor,BookingForName = DF.YDyeingFor
                From MR 
                Left Join YDBookingMaster YBM on YBM.GroupConceptNo = MR.GroupConceptNo 
                Left Join YDBookingChild YDBC on YDBC.YDBookingMasterID = YBM.YDBookingMasterID And YDBC.ItemMasterID = MR.ItemMasterId And ISNULL(YDBC.YD,0) = ISNULL(MR.YD,0) And ISNULL(YDBC.YDItem,0) = ISNULL(MR.YDItem,0)
                Left Join YDBookingChildTwistingColors YBCTC on YBCTC.YDBookingMasterID = YBM.YDBookingMasterID And YBCTC.YDBookingChildID = YDBC.YDBookingChildID 
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = YDBC.ColorId
                LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = MR.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
				LEFT JOIN YarnDyeingFor_HK DF ON DF.YDyeingForID = YDBC.BookingFor
                LEFT JOIN {DbNames.EPYSL}..Unit UN ON MR.UnitID = UN.UnitID 
                Left Join YDProductionMaster YPC On YPC.YDBookingMasterID = YDBC.YDBookingMasterID
                Group By YBCTC.YDBookingChildID, YBCTC.YDBCTwistingColorID, YBCTC.YDBCTwistingID, MR.ItemMasterID,
                YBCTC.ColorId, Color.SegmentValue, YBCTC.ColorCode, ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,
                ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue, YBCTC.TwistingColorQty,
                YBCTC.YDBookingMasterID,YDBC.PhysicalCount, YDBC.LotNo, YDBC.BookingFor,DF.YDyeingFor;

                -- Print Colors
				Select PC.*, Color.SegmentValue ColorName
				From YDBookingPrintColor PC
				INNER JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = PC.ColorID
                WHERE PC.YDBookingMasterID = {id};

                -- Uses In
				SELECT U.*, P.PartName UsesInName
                FROM YDBookingChildUsesIn U
                INNER JOIN {DbNames.EPYSL}..FabricUsedPart P ON P.FUPartID = U.UsesIn
                WHERE U.YDBookingMasterID = {id};

                -- Color childs
                ;Select C.CCColorID, C.ColorID, C.ColorCode, ISV.SegmentValue ColorName, FCBS.RGBOrHex, C.Remarks
                From FreeConceptChildColor C 
                Inner Join FreeConceptMaster FCM On C.ConceptID = FCM.ConceptID
                LEFT Join {DbNames.EPYSL}..FabricColorBookSetup FCBS ON FCBS.ColorID = C.ColorID
                LEFT Join {DbNames.EPYSL}..ItemSegmentValue ISV On FCBS.ColorID = ISV.SegmentValueID
                where FCM.GroupConceptNo = '{GroupConceptNo}'
                Group By C.CCColorID, C.ColorID, C.ColorCode, ISV.SegmentValue, FCBS.RGBOrHex, C.Remarks;

                -- Yarn Dyeing For
                Select Cast(Y.YDyeingForID As varchar) [id], Y.YDyeingFor [text]
                From YarnDyeingFor_HK Y

                -- Spinner
                Select Cast(C.ContactID As varchar) [id], C.Name [text]
                From {DbNames.EPYSL}..Contacts C
                Inner Join {DbNames.EPYSL}..ContactCategoryChild CCC On C.ContactID = CCC.ContactID
                Inner Join {DbNames.EPYSL}..ContactCategoryHK CC ON CC.ContactCategoryID = CCC.ContactCategoryID
                Where CC.ContactCategoryName = '{ContactCategoryNames.SPINNER}'
                Union
                Select Cast(ContactID As varchar) [id], Name [text] from {DbNames.EPYSL}..Contacts;

                -- Uses in List
                SELECT Cast(U.FUPartID As varchar) [id], U.PartName [text]
                FROM {DbNames.EPYSL}..FabricUsedPart U;

                -- Shade book
                {CommonQueries.GetYarnShadeBooks()};
                ";
            }
            else
            {
                query =
                $@"
                -- Master Data
                Select YDB.YDBookingMasterID,YDB.YDBookingNo,YDB.YDBookingDate,YDB.BuyerID,C.ShortName BuyerName,YDB.Remarks,
                YDB.RevisionNo As PreProcessRevNo
                From YDBookingMaster  YDB
                LEFT JOIN {DbNames.EPYSL}..Contacts C ON YDB.BuyerID=C.ContactID
                Where YDBookingMasterID = {id}

                -- Childs Data
                SELECT YDBC.YDBookingChildID, YDBC.YDBookingMasterID, YDBC.ItemMasterID,IM.ItemName,
                Sum(YDBC.BookingQty)BookingQty, Sum(YDBC.BookingQty) SavedQty,IM.DefaultTranUnitID UnitID,UN.DisplayUnitDesc, 
                YDBC.PrintedDensity, YDBC.ProgramName,YDBC.NoOfThread,YDBC.Remarks, YDBC.NoOfCone, Sum(YDBC.BookingQty) FBookingQty,(select SUM(ISNULL(BookingQty,0)) FROM YDBookingChild YC WHERE YC.YBChildItemID = YBC.YBChildItemID) AS YDBookingQty,
                Color.SegmentValue AS ColorName,YDBC.ColorId,YDBC.ColorCode, YDBC.BookingFor, YDBC.IsTwisting, YDBC.IsWaxing, YDBC.UsesIn, YDBC.ShadeCode, YDBC.IsAdditionalItem,
                YDBC.LotNo, YDBC.PhysicalCount, YDBC.SpinnerID, SpinnerName = CASE WHEN ISNULL(YDBC.SpinnerID,0) > 0 THEN Spinner.ShortName ELSE '' END, YDBC.YDBookingChildID YDBookingChild_DemoID,
                ISV1.SegmentValueID Segment1ValueId, ISV2.SegmentValueID Segment2ValueId, ISV3.SegmentValueID Segment3ValueId, ISV4.SegmentValueID Segment4ValueId,
                ISV5.SegmentValueID Segment5ValueId, ISV6.SegmentValueID Segment6ValueId, ISV7.SegmentValueID Segment7ValueId,
                ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc,
                ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc, 
                DF.YDyeingFor BookingForName, --Isnull(YPC.YDProductionMasterID,0)YDProductionMasterID
                Max(Isnull(YPC.YDProductionMasterID,0))YDProductionMasterID, YDBC.ColorBatchRefID, YDBC.ColorBatchRef 
                FROM YDBookingChild YDBC
                LEFT Join {DbNames.EPYSL}..Contacts Spinner On Spinner.ContactID = YDBC.SpinnerID
                INNER JOIN YDBookingmaster YDBM ON YDBM.YDBookingMasterID=YDBC.YDBookingMasterID
                INNER JOIN YarnBookingChildItem_New YBC ON YDBC.YBChildItemID=YBC.YBChildItemID
                INNER JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = YDBC.ColorId
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON YDBC.ItemMasterID = IM.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT JOIN {DbNames.EPYSL}..Unit UN ON YDBC.UnitID = UN.UnitID
                LEFT JOIN YarnDyeingFor_HK DF ON DF.YDyeingForID = YDBC.BookingFor 
                Left Join YDProductionMaster YPC On YPC.YDBookingMasterID = YDBM.YDBookingMasterID
                WHERE YDBC.YDBookingMasterID = {id}
                Group By YDBC.YDBookingChildID, YDBC.YDBookingMasterID, YDBC.ItemMasterID,IM.ItemName,
                IM.DefaultTranUnitID,UN.DisplayUnitDesc, 
                YDBC.PrintedDensity, YDBC.ProgramName,YDBC.NoOfThread,YDBC.Remarks, YDBC.NoOfCone, 
                YBC.YBChildItemID,
                Color.SegmentValue,YDBC.ColorId,YDBC.ColorCode, YDBC.BookingFor, YDBC.IsTwisting, YDBC.IsWaxing, YDBC.UsesIn, YDBC.ShadeCode, YDBC.IsAdditionalItem,
                YDBC.LotNo, YDBC.PhysicalCount, YDBC.SpinnerID, CASE WHEN ISNULL(YDBC.SpinnerID,0) > 0 THEN Spinner.ShortName ELSE '' END, YDBC.YDBookingChildID,
                ISV1.SegmentValueID, ISV2.SegmentValueID, ISV3.SegmentValueID, ISV4.SegmentValueID,
                ISV5.SegmentValueID, ISV6.SegmentValueID, ISV7.SegmentValueID,
                ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue, ISV4.SegmentValue,
                ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue, 
                DF.YDyeingFor, YDBC.ColorBatchRefID, YDBC.ColorBatchRef ;

                -- Twisted Child
                SELECT YDBC.YDBCTwistingID, YDBC.YDBookingMasterID, YDBC.ItemMasterID, YBC.BookingQty,
                YDBC.BookingQty SavedQty,IM.DefaultTranUnitID UnitID,UN.DisplayUnitDesc,
				YDBC.ProgramName,YDBC.NoOfThread,YDBC.Remarks,YDBC.NoOfCone, YDBC.IsTwisting, YDBC.IsWaxing, YDBC.UsesIn, YDBC.ShadeCode,
				FMRC.ReqQty FBookingQty,(select SUM(ISNULL(BookingQty,0)) FROM YDBookingChild YC  where YC.FCMRChildID = FMRC.FCMRChildID) AS YDBookingQty,
				Color.SegmentValue AS ColorName,YDBC.ColorId,YDBC.ColorCode, YDBC.PrintedDensity, YDBC.TPI,
                ISV1.SegmentValueID, ISV2.SegmentValueID, ISV3.SegmentValueID, ISV4.SegmentValueID,
	            ISV5.SegmentValueID, ISV6.SegmentValueID, ISV7.SegmentValueID,
				ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc,
	            ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,
                YDBC.PhysicalCount, Isnull(YPC.YDProductionMasterID,0)YDProductionMasterID
                FROM YDBookingChildTwisting YDBC
                INNER JOIN YDBookingmaster YDBM ON YDBM.YDBookingMasterID=YDBC.YDBookingMasterID
                INNER JOIN YarnBookingChildItem_New YBC ON YDBC.YBChildItemID=YBC.YBChildItemID
				LEFT JOIN FreeConceptMRChild FMRC ON YDBC.FCMRChildID=FMRC.FCMRChildID
				INNER JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = YDBC.ColorId
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YDBC.ItemMasterID
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
				LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
				LEFT JOIN {DbNames.EPYSL}..Unit UN ON YDBC.UnitID = UN.UnitID
                Left Join YDProductionChild YPC On YPC.YDBCTwistingID = YDBC.YDBCTwistingID 
                WHERE YDBC.YDBookingMasterID={id};

                -- Twisted Color
                /*;SELECT COL.YDBCTwistingColorID, COL.YDBCTwistingID, COL.YDBookingChildID, COL.YDBookingMasterID, COL.ColorId, COL.ColorCode, Color.SegmentValue ColorName, COL.TwistingColorQty
                FROM YDBookingChildTwistingColors COL
                INNER JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = COL.ColorID
                WHERE COL.YDBookingMasterID = {id};*/

                Select YBCTC.YDBookingChildID As PrimaryTwistingColorID, YBCTC.YDBCTwistingColorID, YBCTC.YDBCTwistingID,
                YBCTC.YDBookingChildID, YBCTC.ColorId As ColorID, Color.SegmentValue AS ColorName, YBCTC.ColorCode,
                ISV1.SegmentValue As Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc,
                ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc,
                ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc,
                ISV7.SegmentValue Segment7ValueDesc, YBCTC.TwistingColorQty, YBCTC.YDBookingMasterID,
				YDBC.PhysicalCount, YDBC.LotNo, YDBC.BookingFor,BookingForName = DF.YDyeingFor
                From YDBookingChildTwistingColors YBCTC
                Inner JOIN YDBookingChild YDBC On YBCTC.YDBookingChildID = YDBC.YDBookingChildID
                LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YDBC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = YDBC.ColorId
				LEFT JOIN YarnDyeingFor_HK DF ON DF.YDyeingForID = YDBC.BookingFor
                Where YDBC.YDBookingMasterID = {id}
                Group By YBCTC.YDBookingChildID, YBCTC.YDBCTwistingColorID, YBCTC.YDBCTwistingID, YBCTC.YDBookingChildID,
                YBCTC.ColorId, Color.SegmentValue, YBCTC.ColorCode, ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue,
                ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue, YBCTC.TwistingColorQty,
                YBCTC.YDBookingMasterID,YDBC.PhysicalCount, YDBC.LotNo, YDBC.BookingFor,DF.YDyeingFor;

                -- Print Colors
				Select PC.*, Color.SegmentValue ColorName
				From YDBookingPrintColor PC
				INNER JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = PC.ColorID
                WHERE PC.YDBookingMasterID = {id};

                -- Uses In
				SELECT U.*, P.PartName UsesInName
                FROM YDBookingChildUsesIn U
                INNER JOIN {DbNames.EPYSL}..FabricUsedPart P ON P.FUPartID = U.UsesIn
                WHERE U.YDBookingMasterID = {id};

                -- Yarn Dyeing For
                Select Cast(Y.YDyeingForID As varchar) [id], Y.YDyeingFor [text]
                From YarnDyeingFor_HK Y

                -- Spinner
                Select Cast(C.ContactID As varchar) [id], C.Name [text]
                From {DbNames.EPYSL}..Contacts C
                Inner Join {DbNames.EPYSL}..ContactCategoryChild CCC On C.ContactID = CCC.ContactID
                Inner Join {DbNames.EPYSL}..ContactCategoryHK CC ON CC.ContactCategoryID = CCC.ContactCategoryID
                Where CC.ContactCategoryName = '{ContactCategoryNames.SPINNER}'
                Union
                Select Cast(ContactID As varchar) [id], Name [text] from {DbNames.EPYSL}..Contacts;

                -- Uses in list
                SELECT Cast(U.FUPartID As varchar) [id], U.PartName [text]
                FROM {DbNames.EPYSL}..FabricUsedPart U;

                -- Shade book
                {CommonQueries.GetYarnShadeBooks()};
                ";
            }

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                YDBookingMaster data = await records.ReadFirstOrDefaultAsync<YDBookingMaster>();
                Guard.Against.NullObject(data);

                data.YDBookingChilds = records.Read<YDBookingChild>().ToList();
                data.YDBookingChildTwistings = records.Read<YDBookingChildTwisting>().ToList();
                data.YDBookingChildTwistingColors = records.Read<YDBookingChildTwistingColor>().ToList();
                foreach (var item in data.YDBookingChildTwistings)
                {
                    item.YDBookingChildTwistingColors = data.YDBookingChildTwistingColors.FindAll(x => x.YDBCTwistingID == item.YDBCTwistingID);
                    if (item.YDBookingChildTwistingColors.Count() == 1)
                    {
                        var colorName = item.YDBookingChildTwistingColors.FirstOrDefault().ColorName;
                        if (colorName.IsNotNullOrEmpty())
                        {
                            item.TwistedColors = colorName + " + " + colorName;
                        }
                        else
                        {
                            item.TwistedColors = "";
                        }
                    }
                    else
                    {
                        List<string> colorNames = new List<string>();
                        item.YDBookingChildTwistingColors.ForEach(cc =>
                        {
                            if (cc.ColorName.IsNotNullOrEmpty())
                            {
                                colorNames.Add(cc.ColorName);
                            }
                        });
                        item.TwistedColors = string.Join(" + ", colorNames);
                    }
                    item.TwistedSelectedColorIDs = string.Join(",", item.YDBookingChildTwistingColors.Select(x => x.YDBookingChildID));
                }

                #region Set FCMRChildID (Set value for unique operation. Inreal FCMRChildID no need)

                int fcMrChildId = 1;
                List<int> tdBookingChildIds = new List<int>();

                data.YDBookingChildTwistings.ToList().ForEach(x =>
                {
                    x.FCMRChildID = fcMrChildId;
                    data.YDBookingChildTwistingColors.Where(y => y.YDBCTwistingID == x.YDBCTwistingID).ToList().ForEach(y =>
                    {
                        y.FCMRChildID = fcMrChildId;
                    });
                    data.YDBookingChilds.Where(z => z.ItemMasterID == x.ItemMasterID).ToList().ForEach(z =>
                    {
                        z.FCMRChildID = fcMrChildId;
                        tdBookingChildIds.Add(z.YDBookingChildID);
                    });
                    fcMrChildId++;
                });

                data.YDBookingChilds.ForEach(x =>
                {
                    if (!tdBookingChildIds.Exists(y => y == x.YDBookingChildID))
                    {
                        x.FCMRChildID = fcMrChildId;
                        fcMrChildId++;
                    }
                });

                #endregion

                List<YDBookingPrintColor> printColors = records.Read<YDBookingPrintColor>().ToList();
                List<YDBookingChildUsesIn> usesIns = records.Read<YDBookingChildUsesIn>().ToList();
                foreach (var item in data.YDBookingChilds)
                {
                    item.PrintColors = printColors.FindAll(x => x.YDBookingChildID == item.YDBookingChildID);
                    item.PrintColorIDs = string.Join(",", item.PrintColors.Select(x => x.ColorID));
                    item.YDBookingChildUsesIns = usesIns.FindAll(x => x.YDBookingChildID == item.YDBookingChildID);
                    item.UsesIns = string.Join(",", item.YDBookingChildUsesIns.Select(x => x.UsesInName));
                    item.UsesInIDs = string.Join(",", item.YDBookingChildUsesIns.Select(x => x.UsesIn));
                }

                if (pName == "Concept") data.ColorChilds = records.Read<FreeConceptChildColor>().ToList();
                data.YarnDyeingForList = records.Read<Select2OptionModel>();
                data.SpinnerList = records.Read<Select2OptionModel>();
                data.UsesInList = await records.ReadAsync<Select2OptionModel>();
                data.YarnShadeBooks = await records.ReadAsync<Select2OptionModel>();

                int maxColorIDForYarnTwistBookingFor = 99999;
                foreach (var item in data.YDBookingChilds.Where(x => x.ColorID == 0 && x.BookingFor == 3).ToList())
                {
                    item.ColorID = maxColorIDForYarnTwistBookingFor++;
                    foreach (var itemTC in data.YDBookingChildTwistingColors.Where(x => x.YDBookingChildID == item.YDBookingChildID && x.ColorID == 0).ToList())
                    {
                        itemTC.ColorID = item.ColorID;
                        data.YDBookingChildTwistings.FirstOrDefault(x => x.YDBCTwistingID == itemTC.YDBCTwistingID).ColorID = item.ColorID;
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
        public async Task<YDBookingMaster> GetAllAsync(int id)
        {
            string sql = $@"
            ;Select * From YDBookingMaster Where YDBookingMasterID = {id}

            ;Select * From YDBookingChild Where YDBookingMasterID = {id}

            ;SELECT YDBC.*,
                ISV1.SegmentValueID Segment1ValueId, ISV2.SegmentValueID Segment2ValueId, ISV3.SegmentValueID Segment3ValueId, ISV4.SegmentValueID Segment4ValueId,
	            ISV5.SegmentValueID Segment5ValueId, ISV6.SegmentValueID Segment6ValueId, ISV7.SegmentValueID Segment7ValueId,
				ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc,
	            ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc
                FROM YDBookingChildTwisting YDBC
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YDBC.ItemMasterID
				LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
				LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
				LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
				LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
				LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
				LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
				LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                WHERE YDBC.YDBookingMasterID= {id}

            ;Select * From YDBookingChildTwistingColors Where YDBookingMasterID = {id}

            ;Select * From YDBookingPrintColor Where YDBookingMasterID = {id}

            ;Select * From YDBookingChildUsesIn Where YDBookingMasterID = {id}

            ;SELECT TUI.*
			FROM YDBookingChildTwistingUsesIn TUI
			WHERE TUI.YDBookingMasterID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);

                YDBookingMaster data = await records.ReadFirstOrDefaultAsync<YDBookingMaster>();
                Guard.Against.NullObject(data);

                data.YDBookingChilds = records.Read<YDBookingChild>().ToList();
                data.YDBookingChildTwistings = records.Read<YDBookingChildTwisting>().ToList();

                var twistingColorList = records.Read<YDBookingChildTwistingColor>().ToList();
                var printColorList = records.Read<YDBookingPrintColor>().ToList();
                var usesColorList = records.Read<YDBookingChildUsesIn>().ToList();
                var twistingUsesInList = records.Read<YDBookingChildTwistingUsesIn>().ToList();

                foreach (var item in data.YDBookingChilds)
                {
                    item.PrintColors = printColorList.FindAll(x => x.YDBookingChildID == item.YDBookingChildID);
                    item.YDBookingChildUsesIns = usesColorList.FindAll(x => x.YDBookingChildID == item.YDBookingChildID);
                }

                foreach (YDBookingChildTwisting twistedChild in data.YDBookingChildTwistings)
                {
                    twistedChild.YDBookingChildTwistingColors = twistingColorList.FindAll(x => x.YDBCTwistingID == twistedChild.YDBCTwistingID);
                    twistedChild.TwistedColors = string.Join(",", twistedChild.YDBookingChildTwistingColors);

                    twistedChild.YDBCTwistingUsesIns = twistingUsesInList.FindAll(x => x.YDBCTwistingID == twistedChild.YDBCTwistingID);
                    twistedChild.UsesIns = string.Join(",", twistedChild.YDBCTwistingUsesIns);
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

        public async Task SaveAsync(YDBookingMaster entity)
        {
            List<YDBookingChildTwisting> YDBookingChildTwistinList = entity.YDBookingChildTwistings;
            _itemMasterRepository.GenerateItem(AppConstants.ITEM_SUB_GROUP_YARN_NEW, ref YDBookingChildTwistinList);

            entity.YDBookingChildTwistings = YDBookingChildTwistinList;

            SqlTransaction transaction = null;
            SqlTransaction transactionGmt = null;
            try
            {
                //Backup table data save before YDBookingMaster data update.
                await _service.ExecuteAsync("spBackupYDBooking", new { YDBookingMasterID = entity.YDBookingMasterID }, 30, CommandType.StoredProcedure);

                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _connectionGmt.OpenAsync();
                transactionGmt = _connectionGmt.BeginTransaction();

                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity = await AddAsync(entity,transactionGmt);
                        break;

                    case EntityState.Modified:
                        entity = await UpdateAsync(entity,transactionGmt);
                        break;

                    default:
                        break;
                }

                await _service.SaveSingleAsync(entity, transaction);

                //Child
                List<YDBookingChild> YBChilds = new List<YDBookingChild>();
                List<YDBookingChild> delYBChilds = new List<YDBookingChild>();
                YBChilds.AddRange(entity.YDBookingChilds.Where(x => x.EntityState != EntityState.Deleted));
                delYBChilds.AddRange(entity.YDBookingChilds.Where(x => x.EntityState == EntityState.Deleted));

                await _service.SaveAsync(YBChilds, transaction);
                await _service.SaveAsync(entity.YDBookingChildTwistings, transaction);

                List<YDBookingPrintColor> printColors = new List<YDBookingPrintColor>();
                entity.YDBookingChilds.ForEach(x => printColors.AddRange(x.PrintColors));
                await _service.SaveAsync(printColors, transaction);

                List<YDBookingChildUsesIn> usesColors = new List<YDBookingChildUsesIn>();
                entity.YDBookingChilds.ForEach(x => usesColors.AddRange(x.YDBookingChildUsesIns));
                await _service.SaveAsync(usesColors, transaction);

                List<YDBookingChildTwistingColor> twistingColors = new List<YDBookingChildTwistingColor>();
                entity.YDBookingChildTwistings.ForEach(x => twistingColors.AddRange(x.YDBookingChildTwistingColors));
                await _service.SaveAsync(twistingColors, transaction);

                List<YDBookingChildTwistingUsesIn> twistingUsesIns = new List<YDBookingChildTwistingUsesIn>();
                entity.YDBookingChildTwistings.ForEach(x => twistingUsesIns.AddRange(x.YDBCTwistingUsesIns));
                await _service.SaveAsync(twistingUsesIns, transaction);

                //Delete YDBookingChild
                await _service.SaveAsync(delYBChilds, transaction);

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
                _connectionGmt.Close();
            }
        }
        public async Task<string> SaveYDBNoAsync(YDBookingMaster entity)
        {
            SqlTransaction transaction = null;
            SqlTransaction transactionGmt = null;
            string message = "";
            try
            {
                var isYDBNoExist = await YDBNoExistenceCheck(entity.YDBookingMasterID, entity.YDBNo);
                if (isYDBNoExist)
                {
                    message = "This YDB No (" + entity.YDBNo + ") already exist.";
                }
                else
                {
                    await _connection.OpenAsync();
                    transaction = _connection.BeginTransaction();

                    await _connectionGmt.OpenAsync();
                    transactionGmt = _connectionGmt.BeginTransaction();
                    await _service.SaveSingleAsync(entity, transaction);
                    transaction.Commit();
                    transactionGmt.Commit();
                    message = "ok";
                }
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                if (transactionGmt != null) transactionGmt.Rollback();
                message = ex.Message;
                throw ex;
            }
            finally
            {
                _connection.Close();
                _connectionGmt.Close();
            }
            return message;
        }
        public async Task<YDBookingMaster> AddAsync(YDBookingMaster entity, SqlTransaction transactionGmt)
        {
            entity.YDBookingMasterID = await _service.GetMaxIdAsync(TableNames.YD_BOOKING_MASTER, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            if (entity.IsAdditional)
            {
                string parentYDBookingNo = entity.parentYDBookingNo;
                int maxCount = await this.GetMaxYDBookingNo(parentYDBookingNo);
                entity.YDBookingNo = maxCount > 0 ? parentYDBookingNo + "-Add-" + maxCount : parentYDBookingNo;
            }

            int maxChildId = await _service.GetMaxIdAsync(TableNames.YDBookingChild, entity.YDBookingChilds.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            int maxBookingRefId = await _service.GetMaxIdAsync(TableNames.YDBookingRef, entity.YDBookingRefs.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            int maxChildTwistingId = await _service.GetMaxIdAsync(TableNames.YARN_DYEING_BOOKING_CHILD_TWISTING, entity.YDBookingChildTwistings.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

            var newTwistingColorCount = entity.YDBookingChildTwistings.Sum(x => x.YDBookingChildTwistingColors.Count);
            int maxChildTwistingColorId = await _service.GetMaxIdAsync(TableNames.YARN_DYEING_BOOKING_CHILD_TWISTING_COLOR, newTwistingColorCount, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

            var newTwistingUsesInCount = entity.YDBookingChildTwistings.Sum(x => x.YDBCTwistingUsesIns.Count);
            int maxChildTwistingUsesInId = await _service.GetMaxIdAsync(TableNames.YARN_DYEING_BOOKING_CHILD_TWISTING_USES_IN, newTwistingUsesInCount, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

            var newPrintColorCount = entity.YDBookingChilds.Sum(x => x.PrintColors.Count);
            int maxPrintColorId = await _service.GetMaxIdAsync(TableNames.YD_BOOKING_PRINT_COLOR, newPrintColorCount, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

            var newUsesCount = entity.YDBookingChilds.Sum(x => x.YDBookingChildUsesIns.Count);
            int maxChildUsesId = await _service.GetMaxIdAsync(TableNames.YD_BOOKING_CHILD_USES_IN, newUsesCount, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

            foreach (YDBookingChild child in entity.YDBookingChilds)
            {
                int tempYDBookingChildID = CommonFunction.DeepClone(child.YDBookingChildID);

                child.YDBookingChildID = maxChildId++;
                child.YDBookingMasterID = entity.YDBookingMasterID;

                foreach (YDBookingPrintColor printColor in child.PrintColors)
                {
                    printColor.PrintColorID = maxPrintColorId++;
                    printColor.YDBookingChildID = child.YDBookingChildID;
                    printColor.YDBookingMasterID = entity.YDBookingMasterID;
                }
                foreach (YDBookingChildUsesIn childUses in child.YDBookingChildUsesIns)
                {
                    childUses.YDBCUsesInID = maxChildUsesId++;
                    childUses.YDBookingChildID = child.YDBookingChildID;
                    childUses.YDBookingMasterID = entity.YDBookingMasterID;
                }

                entity.YDBookingChildTwistings.ForEach(x =>
                {
                    x.YDBookingChildTwistingColors.Where(y => y.YDBookingChildID == tempYDBookingChildID).ToList().ForEach(y =>
                    {
                        y.YDBookingChildID = child.YDBookingChildID;
                    });
                });
            }

            foreach (YDBookingChildTwisting child in entity.YDBookingChildTwistings)
            {
                child.YDBCTwistingID = maxChildTwistingId++;
                child.YDBookingMasterID = entity.YDBookingMasterID;

                foreach (YDBookingChildTwistingColor childColor in child.YDBookingChildTwistingColors)
                {
                    childColor.YDBCTwistingColorID = maxChildTwistingColorId++;
                    childColor.YDBCTwistingID = child.YDBCTwistingID;
                    childColor.YDBookingMasterID = entity.YDBookingMasterID;
                    childColor.TwistingColorQty = childColor.AssignQty;
                }

                foreach (YDBookingChildTwistingUsesIn childUsesIn in child.YDBCTwistingUsesIns)
                {
                    childUsesIn.YDBCTwistingUsesInID = maxChildTwistingUsesInId++;
                    childUsesIn.YDBCTwistingID = child.YDBCTwistingID;
                    childUsesIn.YDBookingMasterID = entity.YDBookingMasterID;
                }
            }

            return entity;
        }
        private async Task<int> GetMaxYDBookingNo(string rnDReqNo)
        {
            //string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["TexConnection"].ConnectionString;
            string connectionString = _connection.ConnectionString;
            var queryString = $"SELECT MaxValue=COUNT(*) FROM YDBookingMaster WHERE YDBookingNo LIKE '{rnDReqNo}%'";

            int maxNo = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        maxNo = Convert.ToInt32(reader["MaxValue"]);
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
            return maxNo;
        }

        private async Task<YDBookingMaster> UpdateAsync(YDBookingMaster entity, SqlTransaction transactionGmt)
        {
            int maxChildId = await _service.GetMaxIdAsync(TableNames.YDBookingChild, entity.YDBookingChilds.Where(x => x.EntityState == EntityState.Added).Count(), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            int maxChildTwistingId = await _service.GetMaxIdAsync(TableNames.YARN_DYEING_BOOKING_CHILD_TWISTING, entity.YDBookingChildTwistings.Where(x => x.EntityState == EntityState.Added).Count(), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

            var newTwistingColorCount = entity.YDBookingChildTwistings.Sum(x => x.YDBookingChildTwistingColors.Where(y => y.EntityState == EntityState.Added).Count());
            int maxChildTwistingColorId = await _service.GetMaxIdAsync(TableNames.YARN_DYEING_BOOKING_CHILD_TWISTING_COLOR, newTwistingColorCount, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

            var newTwistingUsesInCount = entity.YDBookingChildTwistings.Sum(x => x.YDBCTwistingUsesIns.Where(y => y.EntityState == EntityState.Added).Count());
            int maxChildTwistingUsesInId = await _service.GetMaxIdAsync(TableNames.YARN_DYEING_BOOKING_CHILD_TWISTING_USES_IN, newTwistingUsesInCount, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

            var newPrintColorCount = entity.YDBookingChilds.Sum(x => x.PrintColors.Count);
            int maxPrintColorId = await _service.GetMaxIdAsync(TableNames.YD_BOOKING_PRINT_COLOR, newPrintColorCount, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

            var newUsesCount = entity.YDBookingChilds.Sum(x => x.YDBookingChildUsesIns.Count);
            int maxChildUsesId = await _service.GetMaxIdAsync(TableNames.YD_BOOKING_CHILD_USES_IN, newUsesCount, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

            foreach (YDBookingChild child in entity.YDBookingChilds)
            {
                int tempYDBookingChildID = CommonFunction.DeepClone(child.YDBookingChildID);

                switch (child.EntityState)
                {
                    case EntityState.Added:

                        child.YDBookingChildID = maxChildId++;
                        child.YDBookingMasterID = entity.YDBookingMasterID;

                        foreach (var printColor in child.PrintColors.FindAll(x => x.EntityState == EntityState.Added))
                        {
                            printColor.PrintColorID = maxPrintColorId++;
                            printColor.YDBookingChildID = child.YDBookingChildID;
                            printColor.YDBookingMasterID = entity.YDBookingMasterID;
                        }

                        foreach (var usesColor in child.YDBookingChildUsesIns.FindAll(x => x.EntityState == EntityState.Added))
                        {
                            usesColor.YDBCUsesInID = maxChildUsesId++;
                            usesColor.YDBookingChildID = child.YDBookingChildID;
                            usesColor.YDBookingMasterID = entity.YDBookingMasterID;
                        }

                        entity.YDBookingChildTwistings.ForEach(x =>
                        {
                            x.YDBookingChildTwistingColors.Where(y => y.YDBookingChildID == tempYDBookingChildID && y.EntityState == EntityState.Added).ToList().ForEach(y =>
                            {
                                y.YDBookingChildID = child.YDBookingChildID;
                            });
                        });

                        break;

                    case EntityState.Modified:

                        foreach (var printColor in child.PrintColors.FindAll(x => x.EntityState == EntityState.Added))
                        {
                            printColor.PrintColorID = maxPrintColorId++;
                            printColor.YDBookingChildID = child.YDBookingChildID;
                            printColor.YDBookingMasterID = entity.YDBookingMasterID;
                        }

                        foreach (var usesColor in child.YDBookingChildUsesIns.FindAll(x => x.EntityState == EntityState.Added))
                        {
                            usesColor.YDBCUsesInID = maxChildUsesId++;
                            usesColor.YDBookingChildID = child.YDBookingChildID;
                            usesColor.YDBookingMasterID = entity.YDBookingMasterID;
                        }
                        break;

                    default:
                        break;
                }
            }

            foreach (YDBookingChildTwisting child in entity.YDBookingChildTwistings)
            {
                switch (child.EntityState)
                {
                    case EntityState.Added:
                        child.YDBCTwistingID = maxChildTwistingId++;
                        child.YDBookingMasterID = entity.YDBookingMasterID;
                        foreach (YDBookingChildTwistingColor childColor in child.YDBookingChildTwistingColors.FindAll(x => x.EntityState == EntityState.Added))
                        {
                            childColor.YDBCTwistingColorID = maxChildTwistingColorId++;
                            childColor.YDBCTwistingID = child.YDBCTwistingID;
                            childColor.YDBookingMasterID = entity.YDBookingMasterID;
                            childColor.TwistingColorQty = childColor.AssignQty;
                        }
                        foreach (YDBookingChildTwistingUsesIn childUsesIn in child.YDBCTwistingUsesIns.FindAll(x => x.EntityState == EntityState.Added))
                        {
                            childUsesIn.YDBCTwistingUsesInID = maxChildTwistingUsesInId++;
                            childUsesIn.YDBCTwistingID = child.YDBCTwistingID;
                            childUsesIn.YDBookingMasterID = entity.YDBookingMasterID;
                        }
                        break;

                    case EntityState.Modified:
                        foreach (YDBookingChildTwistingColor childColor in child.YDBookingChildTwistingColors.FindAll(x => x.EntityState == EntityState.Added))
                        {
                            childColor.YDBCTwistingColorID = maxChildTwistingColorId++;
                            childColor.YDBCTwistingID = child.YDBCTwistingID;
                            childColor.YDBookingMasterID = entity.YDBookingMasterID;
                            childColor.TwistingColorQty = childColor.AssignQty;
                        }
                        foreach (YDBookingChildTwistingUsesIn childUsesIn in child.YDBCTwistingUsesIns.FindAll(x => x.EntityState == EntityState.Added))
                        {
                            childUsesIn.YDBCTwistingUsesInID = maxChildTwistingUsesInId++;
                            childUsesIn.YDBCTwistingID = child.YDBCTwistingID;
                            childUsesIn.YDBookingMasterID = entity.YDBookingMasterID;
                        }
                        break;

                    default:
                        break;
                }
            }

            return entity;
        }

        public async Task UpdateEntityAsync(YDBookingMaster entity)
        {
            SqlTransaction transaction = null;
            SqlTransaction transactionGmt = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _connectionGmt.OpenAsync();
                transactionGmt = _connectionGmt.BeginTransaction();


                await _service.SaveSingleAsync(entity, transaction);

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
                _connectionGmt.Close();
            }
        }
        private async Task<bool> YDBNoExistenceCheck(int YDBookingMasterID, string YDBNo)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["TexConnection"].ConnectionString;
            var queryString = $"";
            if (YDBookingMasterID > 0)
            {
                queryString = $"SELECT YDBNo FROM YDBookingMaster WHERE YDBNo = '{YDBNo}' AND YDBookingMasterID != {YDBookingMasterID}";
            }
            else
            {
                queryString = $"SELECT YDBNo FROM YDBookingMaster WHERE YDBNo = '{YDBNo}'";
            }
            bool isExist = false;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                connection.Open();
                SqlDataReader reader = await command.ExecuteReaderAsync();
                isExist = reader.HasRows;
                reader.Close();
            }
            return isExist;
        }
    }
}
