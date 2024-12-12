using Dapper;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Transactions;

namespace EPYSLTEXCore.Infrastructure.Services
{
    public class YarnPOService : IYarnPOService
    {
        private readonly IDapperCRUDService<YarnPOMaster> _service;
        private readonly IDapperCRUDService<YarnPOChildOrder> _servicePoChildOrder;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _gmtConnection;

        public YarnPOService(
            IDapperCRUDService<YarnPOMaster> service
            , IDapperCRUDService<YarnPOChildOrder> servicePoChildOrder
            )
        {
            _service = service;
            _servicePoChildOrder = servicePoChildOrder;
            service.Connection= service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
            _gmtConnection = service.GetConnection(AppConstants.GMT_CONNECTION);
        }

        public async Task<List<YarnPOMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo)
        {
            string tempGuid = CommonFunction.GetNewGuid();
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YPOMasterID Desc" : paginationInfo.OrderBy;

            string sql;
            if (status == Status.Pending)
            {
                sql = $@"
                ;WITH  PO As(
					Select 
					YarnPRChildID = POC.PRChildID,POQty = SUM(ISNULL(POC.POQty,0))
					From YarnPOChild POC
					INNER JOIN YarnPOMaster POM ON POM.YPOMasterID = POC.YPOMasterID
					Where ISNULL(POM.IsCancel,0) = 0 
					Group By POC.PRChildID

				), YPO As(
					Select 
					YPO.YarnPRChildID
					From PO YPO 
					INNER JOIN YarnPRChild CH ON CH.YarnPRChildID = YPO.YarnPRChildID
					Group By YPO.YarnPRChildID
					Having SUM(ISNULL(YPO.POQty,0)) >= SUM(ISNULL(CH.ReqQty,0))
				),
                PRBalance As(
                    SELECT CH.YarnPRChildID, BalanceQTY = SUM(ISNULL(CH.ReqQty,0)) - SUM(ISNULL(YPO.POQty,0)),POQty = SUM(ISNULL(YPO.POQty,0)),
                    BuyerName = STRING_AGG(B.ShortName,',')
                    FROM YarnPRMaster M
                    INNER JOIN YarnPRChild CH ON CH.YarnPRMasterID = M.YarnPRMasterID
                    LEFT JOIN FreeConceptMaster FCM on FCM.ConceptID = CH.ConceptID
                    LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = FCM.BuyerID AND FCM.BuyerID > 0
                    Left JOIN PO YPO ON YPO.YarnPRChildID = CH.YarnPRChildID
                    WHERE M.IsCPR = 1 AND M.Reject = 0 AND M.IsFPR = 1
                    --AND
                    --M.IsFPR= (
			        --    CASE 
				    --        WHEN Substring(YarnPRNo,0,4)='PB-' 
				    --        THEN 1
				    --        ELSE 0
			        --    End
		            --)
                    --M.IsFPR= (
			        --    CASE 
				    --        WHEN YarnPRFromID=4 
				    --        THEN 1
				    --        ELSE 0
			        --    End
		            --)
                    AND CH.YarnPRChildID Not In (Select YarnPRChildID From YPO)
                    Group By CH.YarnPRChildID
				),YPR AS 
                (
                    SELECT M.YarnPRMasterID, M.YarnPRDate, M.YarnPRNo, M.YarnPRRequiredDate, M.TriggerPointID, M.YarnPRBy, 
                    CH.YarnPRChildID, CH.ItemMasterID, CH.FPRCompanyID As CompanyID, CH.ShadeCode, CH.ReqQty, M.ConceptNo,
                    DayValidDurationName = CASE WHEN ISNULL(CH.DayValidDurationId,0) > 0 THEN ET.ValueName ELSE '' END
                    FROM YarnPRMaster M
                    INNER JOIN YarnPRChild CH ON CH.YarnPRMasterID = M.YarnPRMasterID
                    INNER JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = M.SubGroupID
                    LEFT JOIN DayValidDuration DVD ON DVD.DayValidDurationId = CH.DayValidDurationId
                    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ET ON ET.ValueID = DVD.LocalOrImportId
                    WHERE M.IsCPR = 1 AND M.Reject = 0 AND M.IsFPR = 1
                    --AND
                    --M.IsFPR= (
			        --   CASE 
				    --        WHEN Substring(YarnPRNo,0,4)='PB-' 
				    --       THEN 1
				    --        ELSE 0
			        --    End
		            --)
                    --M.IsFPR= (
			        --    CASE 
				    --        WHEN YarnPRFromID=4 
				    --        THEN 1
				    --        ELSE 0
			        --    End
		            --)
	                AND CH.YarnPRChildID Not In (Select YarnPRChildID From YPO) --(Select PRChildID From T_YarnPOChild)
                ),
                M AS 
                (
	                SELECT  BalanceQTY = SUM(ISNULL(PRB.BalanceQTY,0)), POQty =  SUM(ISNULL(PRB.POQty,0)), YPR.YarnPRMasterID As PRMasterID, YPR.YarnPRDate As PRDate, YPR.YarnPRNo As PRNO, 
                    YPR.YarnPRRequiredDate As PRRequiredDate, YPR.YarnPRBy, YPR.CompanyID, CE.ShortName As CompanyName, 
	                L.Name As PRByUser, YPR.YarnPRChildID, YPR.ConceptNo, YPR.ShadeCode, YPR.ReqQty,  
                    ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, 
                    ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, 
                    ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc, ISV8.SegmentValue Segment8ValueDesc,
                    PRB.BuyerName, YPR.DayValidDurationName
	                From YPR
	                INNER JOIN {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = YPR.ItemMasterID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID 
	                LEFT Join {DbNames.EPYSL}..CompanyEntity CE On YPR.CompanyID = CE.CompanyID
	                LEFT Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = YPR.TriggerPointID
	                LEFT Join {DbNames.EPYSL}..LoginUser L On YPR.YarnPRBy = L.UserCode
                    Left Join PRBalance PRB On PRB.YarnPRChildID = YPR.YarnPRChildID
	                GROUP BY YPR.YarnPRMasterID, YPR.YarnPRDate, YPR.YarnPRNo, YPR.YarnPRRequiredDate, YPR.YarnPRBy, 
                    YPR.CompanyID, CE.ShortName, L.Name, YPR.YarnPRChildID, YPR.ConceptNo, YPR.ShadeCode, YPR.ReqQty, 
                    ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue, ISV4.SegmentValue, ISV5.SegmentValue, 
                    ISV6.SegmentValue, ISV7.SegmentValue, ISV8.SegmentValue,PRB.BuyerName, YPR.DayValidDurationName

                )
                SELECT M.* INTO #TempTable{tempGuid} FROM M
                SELECT *, Count(*) Over() TotalRows FROM #TempTable{tempGuid} M ";

                orderBy = "ORDER BY PRMasterID DESC";
            }
            else if (status == Status.AwaitingPropose)
            {
                sql = $@"With 
                YPL AS(
	                SELECT YPM.YPOMasterID, CE.ShortName AS CompanyName, YPM.PONo, PODate, C.Name SupplierName, YPM.QuotationRefNo, YPF.ValueName POFor, DeliveryStartDate, DeliveryEndDate,
		                Proposed, UnApprove, Approved, SUM(YPC.POQty) TotalQty, SUM(YPC.PIValue) TotalValue,
		                CAINFO.SFToPLDays, CAINFO.PLToPDDays, CAINFO.PDToCFDays PCFDays,
		                (CAINFO.SFToPLDays + CAINFO.PLToPDDays + CAINFO.PDToCFDays) AS InHouseDays, LU.UserName, YPM.ConceptNo, AddedByName = LU.Name
	                FROM YarnPOMaster YPM
	                INNER JOIN T_YarnPOChild YPC ON YPC.YPOMasterID = YPM.YPOMasterID
	                Inner Join {DbNames.EPYSL}..Contacts C ON C.ContactID = YPM.SupplierID
	                LEFT JOIN {DbNames.EPYSL}..ContactAdditionalInfo CAINFO ON CAINFO.ContactID = YPM.SupplierID
	                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue YPF ON YPF.ValueID = YPM.POForID
	                INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = YPM.CompanyID
	                INNER JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = YPM.AddedBy
	                INNER JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = YPM.SubGroupID
	                WHERE YPM.Proposed = 0 AND YPM.Approved = 0 AND YPM.UnApprove = 0 And ISNULL(YPM.IsCancel,0) = 0 And ISG.SubGroupName = 'Yarn New' And YPM.RevisionNo=0
	                GROUP BY YPM.YPOMasterID, CE.ShortName, YPM.PONo, PODate, C.Name, YPM.QuotationRefNo, YPF.ValueName, DeliveryStartDate, DeliveryEndDate,
		                Proposed, UnApprove, Approved, CAINFO.SFToPLDays, CAINFO.PLToPDDays, CAINFO.PDToCFDays,
		                (CAINFO.SFToPLDays + CAINFO.PLToPDDays + CAINFO.PDToCFDays), LU.UserName,LU.Name, YPM.ConceptNo
                ),
				CNM AS(
				    Select YPL.YPOMasterID, YPC.BookingNo
				    FROM YarnPOChild YPC 
				    INNER JOIN YPL ON YPL.YPOMasterID = YPC.YPOMasterID
				    GROUP BY YPL.YPOMasterID, YPC.BookingNo
				),
				CN AS(
				    Select CNM.YPOMasterID, BookingNo = STRING_AGG(CNM.BookingNo,',')
				    FROM CNM 
				    GROUP BY CNM.YPOMasterID
				),
                BL AS
                (
	                SELECT YPL.YPOMasterID, BuyerName = STRING_AGG(B.ShortName,',')
	                FROM YPL
	                INNER JOIN T_YarnPOChild YPC ON YPC.YPOMasterID = YPL.YPOMasterID
	                INNER JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = YPC.BuyerID AND YPC.BuyerID > 0
	                GROUP BY YPL.YPOMasterID
                ),
                FinalList AS
                (
                    SELECT YPL.YPOMasterID, YPL.PONo, YPL.PODate, YPL.CompanyName, YPL.SupplierName, YPL.QuotationRefNo, YPL.POFor, YPL.DeliveryStartDate, YPL.DeliveryEndDate,
		                CAST(YPL.TotalQty AS DECIMAL(18,2)) TotalQty, CAST(YPL.TotalValue AS DECIMAL(18,2)) TotalValue,
		                YPL.Approved,
		                YPL.SFToPLDays, YPL.PLToPDDays, YPL.PCFDays, YPL.InHouseDays,
		                DATEADD(day, YPL.SFToPLDays, YPL.DeliveryStartDate) AS SFToPLDate,
		                DATEADD(day, YPL.PLToPDDays, DATEADD(day, YPL.SFToPLDays, YPL.DeliveryStartDate)) AS PLToPDDate,
		                DATEADD(day, YPL.PCFDays, DATEADD(day, YPL.PLToPDDays, DATEADD(day, YPL.SFToPLDays, YPL.DeliveryStartDate))) AS PCFDate,
		                DATEADD(day, (YPL.SFToPLDays + YPL.PLToPDDays + YPL.PCFDays), YPL.DeliveryStartDate) AS InHouseDate, YPL.UserName, ConceptNo = CN.BookingNo, YPL.AddedByName,
		                BL.BuyerName
                    FROM YPL
	                LEFT JOIN BL ON BL.YPOMasterID = YPL.YPOMasterID
					LEFT JOIN CN ON YPL.YPOMasterID = CN.YPOMasterID
                )
                    SELECT * INTO #TempTable{tempGuid} FROM FinalList
                    SELECT *, Count(*) Over() TotalRows FROM #TempTable{tempGuid} M ";
            }
            else if (status == Status.Proposed)
            {
                sql = $@"With YPL AS(
                    SELECT YPM.YPOMasterID, CE.ShortName AS CompanyName, YPM.PONo, PODate, C.Name SupplierName, YPM.QuotationRefNo, YPF.ValueName POFor, DeliveryStartDate, DeliveryEndDate,
			            Proposed, UnApprove, Approved, SUM(YPC.POQty) TotalQty, SUM(YPC.PIValue) TotalValue,
						CAINFO.SFToPLDays, CAINFO.PLToPDDays, CAINFO.PDToCFDays PCFDays,
						(CAINFO.SFToPLDays + CAINFO.PLToPDDays + CAINFO.PDToCFDays) AS InHouseDays, LU.UserName, YPM.ConceptNo, AddedByName = LU.Name
                    FROM YarnPOMaster YPM --ON YPM.SupplierID = Contacts.ContactID
					INNER JOIN T_YarnPOChild YPC ON YPC.YPOMasterID = YPM.YPOMasterID
					Inner Join {DbNames.EPYSL}..Contacts C ON C.ContactID = YPM.SupplierID
                    LEFT JOIN {DbNames.EPYSL}..ContactAdditionalInfo CAINFO ON CAINFO.ContactID = YPM.SupplierID
                    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue YPF ON YPF.ValueID = YPM.POForID
                    INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = YPM.CompanyID
                    INNER JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = YPM.AddedBy
                    INNER JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = YPM.SubGroupID
					WHERE YPM.Proposed = 1 AND YPM.Approved = 0 AND YPM.UnApprove = 0 And ISNULL(YPM.IsCancel,0) = 0 And ISG.SubGroupName = 'Yarn New'
					GROUP BY YPM.YPOMasterID, CE.ShortName, YPM.PONo, PODate, C.Name, YPM.QuotationRefNo, YPF.ValueName, DeliveryStartDate, DeliveryEndDate,
			            Proposed, UnApprove, Approved, CAINFO.SFToPLDays, CAINFO.PLToPDDays, CAINFO.PDToCFDays,
						(CAINFO.SFToPLDays + CAINFO.PLToPDDays + CAINFO.PDToCFDays), LU.UserName,LU.Name, YPM.ConceptNo
                    ),
				    CNM AS(
				        Select YPL.YPOMasterID, YPC.BookingNo
				        FROM T_YarnPOChild YPC 
				        INNER JOIN YPL ON YPL.YPOMasterID = YPC.YPOMasterID
				        GROUP BY YPL.YPOMasterID, YPC.BookingNo
				    ),
				    CN AS(
				        Select CNM.YPOMasterID, BookingNo = STRING_AGG(CNM.BookingNo,',')
				        FROM CNM 
				        GROUP BY CNM.YPOMasterID
				    ),
                    BL AS
                    (
	                    SELECT YPL.YPOMasterID, BuyerName = STRING_AGG(B.ShortName,',')
	                    FROM YPL
	                    INNER JOIN T_YarnPOChild YPC ON YPC.YPOMasterID = YPL.YPOMasterID
	                    INNER JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = YPC.BuyerID AND YPC.BuyerID > 0
	                    GROUP BY YPL.YPOMasterID
                    ),
                    FinalList AS
                    (
                        SELECT YPL.YPOMasterID, YPL.PONo, YPL.PODate, YPL.CompanyName, YPL.SupplierName, YPL.QuotationRefNo, YPL.POFor, YPL.DeliveryStartDate, YPL.DeliveryEndDate,
			                CAST(YPL.TotalQty AS DECIMAL(18,2)) TotalQty, CAST(YPL.TotalValue AS DECIMAL(18,2)) TotalValue,
						    YPL.Approved,
						    YPL.SFToPLDays, YPL.PLToPDDays, YPL.PCFDays, YPL.InHouseDays,
						    DATEADD(day, YPL.SFToPLDays, YPL.DeliveryStartDate) AS SFToPLDate,
						    DATEADD(day, YPL.PLToPDDays, DATEADD(day, YPL.SFToPLDays, YPL.DeliveryStartDate)) AS PLToPDDate,
						    DATEADD(day, YPL.PCFDays, DATEADD(day, YPL.PLToPDDays, DATEADD(day, YPL.SFToPLDays, YPL.DeliveryStartDate))) AS PCFDate,
						    DATEADD(day, (YPL.SFToPLDays +  YPL.PLToPDDays + YPL.PCFDays), YPL.DeliveryStartDate) AS InHouseDate, YPL.UserName, ConceptNo = CN.BookingNo, YPL.AddedByName,
                            BL.BuyerName
                        FROM YPL
                        LEFT JOIN BL ON BL.YPOMasterID = YPL.YPOMasterID
						LEFT JOIN CN ON YPL.YPOMasterID = CN.YPOMasterID
                    )
                    SELECT * INTO #TempTable{tempGuid} FROM FinalList
                    SELECT *, Count(*) Over() TotalRows FROM #TempTable{tempGuid} M";
            }
            else if (status == Status.Approved)
            {
                sql = $@"
                        With 
	                    YPL AS(
		                    SELECT YPM.YPOMasterID, CE.ShortName AS CompanyName, YPM.PONo, PODate, C.Name SupplierName, YPM.QuotationRefNo, YPF.ValueName POFor, DeliveryStartDate, DeliveryEndDate,
		                    Proposed, UnApprove, Approved, SUM(YPC.POQty) TotalQty, SUM(YPC.PIValue) TotalValue,
		                    YPC.ReceivedCompleted,
		                    CAINFO.SFToPLDays, CAINFO.PLToPDDays, CAINFO.PDToCFDays PCFDays,
		                    (CAINFO.SFToPLDays + CAINFO.PLToPDDays + CAINFO.PDToCFDays) AS InHouseDays, LU.UserName, YPM.ConceptNo,
		                    YPM.RevisionNo, YPM.RevisionDate, AddedByName = LU.Name
		                    FROM YarnPOMaster YPM --ON YPM.SupplierID = Contacts.ContactID
		                    INNER JOIN T_YarnPOChild YPC ON YPC.YPOMasterID = YPM.YPOMasterID
		                    Inner Join {DbNames.EPYSL}..Contacts C ON C.ContactID = YPM.SupplierID
		                    LEFT JOIN {DbNames.EPYSL}..ContactAdditionalInfo CAINFO ON CAINFO.ContactID = YPM.SupplierID
		                    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue YPF ON YPF.ValueID = YPM.POForID
		                    INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = YPM.CompanyID
		                    INNER JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = YPM.AddedBy
		                    INNER JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = YPM.SubGroupID
		                    WHERE YPM.Proposed = 1 AND YPM.Approved = 1 AND YPM.UnApprove = 0  And ISNULL(YPM.IsCancel,0) = 0 And ISG.SubGroupName = 'Yarn New'
		                    GROUP BY YPM.YPOMasterID, CE.ShortName, YPM.PONo, PODate, C.Name, YPM.QuotationRefNo, YPF.ValueName, DeliveryStartDate, DeliveryEndDate,
		                    Proposed, UnApprove, Approved, CAINFO.SFToPLDays, CAINFO.PLToPDDays, CAINFO.PDToCFDays,
		                    (CAINFO.SFToPLDays + CAINFO.PLToPDDays + CAINFO.PDToCFDays), LU.UserName,LU.Name, YPM.ConceptNo,YPC.ReceivedCompleted,
		                    YPM.RevisionNo, YPM.RevisionDate
                        ),
				        CNM AS(
				            Select YPL.YPOMasterID, YPC.BookingNo
				            FROM T_YarnPOChild YPC 
				            INNER JOIN YPL ON YPL.YPOMasterID = YPC.YPOMasterID
				            GROUP BY YPL.YPOMasterID, YPC.BookingNo
				        ),
				        CN AS(
				            Select CNM.YPOMasterID, BookingNo = STRING_AGG(CNM.BookingNo,',')
				            FROM CNM 
				            GROUP BY CNM.YPOMasterID
				        ),
	                    BL AS
	                    (
		                    SELECT YPL.YPOMasterID, BuyerName = STRING_AGG(B.ShortName,',')
		                    FROM YPL
		                    INNER JOIN T_YarnPOChild YPC ON YPC.YPOMasterID = YPL.YPOMasterID
		                    INNER JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = YPC.BuyerID AND YPC.BuyerID > 0
		                    GROUP BY YPL.YPOMasterID
	                    ),
	                    FinalList AS
	                    (
		                    SELECT YPL.YPOMasterID, YPL.PONo, YPL.PODate, YPL.CompanyName, YPL.SupplierName, YPL.QuotationRefNo, YPL.POFor, YPL.DeliveryStartDate, YPL.DeliveryEndDate,
		                    CAST(YPL.TotalQty AS DECIMAL(18,2)) TotalQty, CAST(YPL.TotalValue AS DECIMAL(18,2)) TotalValue,
		                    YPL.Approved,
		                    YPL.ReceivedCompleted,
		                    YPL.SFToPLDays, YPL.PLToPDDays, YPL.PCFDays, YPL.InHouseDays, 
		                    DATEADD(day, YPL.SFToPLDays, YPL.DeliveryStartDate) AS SFToPLDate,
		                    DATEADD(day, YPL.PLToPDDays, DATEADD(day, YPL.SFToPLDays, YPL.DeliveryStartDate)) AS PLToPDDate,
		                    DATEADD(day, YPL.PCFDays, DATEADD(day, YPL.PLToPDDays, DATEADD(day, YPL.SFToPLDays, YPL.DeliveryStartDate))) AS PCFDate,
		                    DATEADD(day, (YPL.SFToPLDays + YPL.PLToPDDays + YPL.PCFDays), YPL.DeliveryStartDate) AS InHouseDate, YPL.UserName, ConceptNo = CN.BookingNo,
		                    YPL.RevisionNo, YPL.RevisionDate, YPL.AddedByName,
		                    BL.BuyerName
		                    FROM YPL
		                    LEFT JOIN BL ON BL.YPOMasterID = YPL.YPOMasterID
						    LEFT JOIN CN ON YPL.YPOMasterID = CN.YPOMasterID
	                    ),
	                    PIPOINFO As(
	                        Select POM.YPOMasterID,
	                        StatusPIPO = 
	                        Case
		                        When ISNULL(YRM.POID,0)>0 And ISNULL(YPO.YPOMasterID,0)>0 Then 'Its already recieved by inventory and PI'
		                        When ISNULL(YRM.POID,0)>0 Then 'Its already recieved by inventory'
		                        When ISNULL(YPO.YPOMasterID,0)>0 Then  'Its already recieved by PI'
		                        Else '' 
	                        End
	                        From YarnPOMaster POM
	                        INNER JOIN FinalList FL ON FL.YPOMasterID =  POM.YPOMasterID
	                        LEFT JOIN YarnReceiveMaster YRM On YRM.POID = POM.YPOMasterID
	                        LEFT JOIN YarnPIReceivePO YPO On YPO.YPOMasterID = POM.YPOMasterID
	                        Group By POM.YPOMasterID,
	                        Case
		                        When ISNULL(YRM.POID,0)>0 And ISNULL(YPO.YPOMasterID,0)>0 Then 'Its already recieved by inventory and PI'
		                        When ISNULL(YRM.POID,0)>0 Then 'Its already recieved by inventory'
		                        When ISNULL(YPO.YPOMasterID,0)>0 Then  'Its already recieved by PI'
		                        Else ''
	                        End
                        ), 
	                    FFList As (
                            SELECT FL.*, PP.StatusPIPO
                            FROM FinalList FL 
                            LEFT JOIN PIPOINFO  PP ON PP.YPOMasterID = FL.YPOMasterID
                        )

	                    SELECT * INTO #TempTable{tempGuid} FROM FFList
                        SELECT *, Count(*) Over() TotalRows FROM #TempTable{tempGuid} M";
            }

            else if (status == Status.UnApproved)
            {
                sql = $@"
                    With 
                    YPL AS(
                        SELECT YPM.YPOMasterID, CE.ShortName AS CompanyName, YPM.PONo, PODate, C.Name SupplierName, YPM.QuotationRefNo, YPF.ValueName POFor, DeliveryStartDate, DeliveryEndDate,
                        Proposed, UnApprove, Approved, SUM(YPC.POQty) TotalQty, SUM(YPC.PIValue) TotalValue,
                        CAINFO.SFToPLDays, CAINFO.PLToPDDays, CAINFO.PDToCFDays PCFDays,
                        (CAINFO.SFToPLDays + CAINFO.PLToPDDays + CAINFO.PDToCFDays) AS InHouseDays, LU.UserName, YPM.ConceptNo, AddedByName = LU.Name
                        FROM YarnPOMaster YPM --ON YPM.SupplierID = Contacts.ContactID
                        INNER JOIN T_YarnPOChild YPC ON YPC.YPOMasterID = YPM.YPOMasterID
                        Inner Join {DbNames.EPYSL}..Contacts C ON C.ContactID = YPM.SupplierID
                        LEFT JOIN {DbNames.EPYSL}..ContactAdditionalInfo CAINFO ON CAINFO.ContactID = YPM.SupplierID
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue YPF ON YPF.ValueID = YPM.POForID
                        INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = YPM.CompanyID
                        INNER JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = YPM.AddedBy
                        INNER JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = YPM.SubGroupID
                        WHERE YPM.UnApprove = 1  And ISNULL(YPM.IsCancel,0) = 0 And ISG.SubGroupName = 'Yarn New'
                        GROUP BY YPM.YPOMasterID, CE.ShortName, YPM.PONo, PODate, C.Name, YPM.QuotationRefNo, YPF.ValueName, DeliveryStartDate, DeliveryEndDate,
                        Proposed, UnApprove, Approved, CAINFO.SFToPLDays, CAINFO.PLToPDDays, CAINFO.PDToCFDays,
                        (CAINFO.SFToPLDays + CAINFO.PLToPDDays + CAINFO.PDToCFDays), LU.UserName,LU.Name, YPM.ConceptNo
                    ),
				    CNM AS(
				        Select YPL.YPOMasterID, YPC.BookingNo
				        FROM T_YarnPOChild YPC 
				        INNER JOIN YPL ON YPL.YPOMasterID = YPC.YPOMasterID
				        GROUP BY YPL.YPOMasterID, YPC.BookingNo
				    ),
				    CN AS(
				        Select CNM.YPOMasterID, BookingNo = STRING_AGG(CNM.BookingNo,',')
				        FROM CNM 
				        GROUP BY CNM.YPOMasterID
				    ),
                    BL AS
                    (
	                    SELECT YPL.YPOMasterID, BuyerName = STRING_AGG(B.ShortName,',')
	                    FROM YPL
	                    INNER JOIN T_YarnPOChild YPC ON YPC.YPOMasterID = YPL.YPOMasterID
	                    INNER JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = YPC.BuyerID AND YPC.BuyerID > 0
	                    GROUP BY YPL.YPOMasterID
                    ),
                    FinalList AS
                    (
                        SELECT YPL.YPOMasterID, YPL.PONo, YPL.PODate, YPL.CompanyName, YPL.SupplierName, YPL.QuotationRefNo, YPL.POFor, YPL.DeliveryStartDate, YPL.DeliveryEndDate,
			                CAST(YPL.TotalQty AS DECIMAL(18,2)) TotalQty, CAST(YPL.TotalValue AS DECIMAL(18,2)) TotalValue,
						    YPL.Approved,
						    YPL.SFToPLDays, YPL.PLToPDDays, YPL.PCFDays, YPL.InHouseDays,
						    DATEADD(day, YPL.SFToPLDays, YPL.DeliveryStartDate) AS SFToPLDate,
						    DATEADD(day, YPL.PLToPDDays, DATEADD(day, YPL.SFToPLDays, YPL.DeliveryStartDate)) AS PLToPDDate,
						    DATEADD(day, YPL.PCFDays, DATEADD(day, YPL.PLToPDDays, DATEADD(day, YPL.SFToPLDays, YPL.DeliveryStartDate))) AS PCFDate,
						    DATEADD(day, (YPL.SFToPLDays +  YPL.PLToPDDays + YPL.PCFDays), YPL.DeliveryStartDate) AS InHouseDate, YPL.UserName, ConceptNo = CN.BookingNo,
                            YPL.AddedByName,
                            BL.BuyerName
                        FROM YPL
                        LEFT JOIN BL ON BL.YPOMasterID = YPL.YPOMasterID
						LEFT JOIN CN ON YPL.YPOMasterID = CN.YPOMasterID
                    )
                    SELECT * INTO #TempTable{tempGuid} FROM FinalList
                    SELECT *, Count(*) Over() TotalRows FROM #TempTable{tempGuid} M";
            }
            else if (status == Status.Return)
            {
                sql = $@"With 
                    YPL AS(
                        SELECT YPM.YPOMasterID, CE.ShortName AS CompanyName, YPM.PONo, PODate, C.Name SupplierName, YPM.QuotationRefNo, YPF.ValueName POFor, DeliveryStartDate, DeliveryEndDate,
                        Proposed, UnApprove, Approved, SUM(YPC.POQty) TotalQty, SUM(YPC.PIValue) TotalValue,
                        CAINFO.SFToPLDays, CAINFO.PLToPDDays, CAINFO.PDToCFDays PCFDays,
                        (CAINFO.SFToPLDays + CAINFO.PLToPDDays + CAINFO.PDToCFDays) AS InHouseDays, LU.UserName, YPM.ConceptNo, AddedByName = LU.Name
                        FROM YarnPOMaster YPM --ON YPM.SupplierID = Contacts.ContactID
                        INNER JOIN T_YarnPOChild YPC ON YPC.YPOMasterID = YPM.YPOMasterID
                        Inner Join {DbNames.EPYSL}..Contacts C ON C.ContactID = YPM.SupplierID
                        LEFT JOIN {DbNames.EPYSL}..ContactAdditionalInfo CAINFO ON CAINFO.ContactID = YPM.SupplierID
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue YPF ON YPF.ValueID = YPM.POForID
                        INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = YPM.CompanyID
                        INNER JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = YPM.AddedBy
                        INNER JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = YPM.SubGroupID
                        WHERE YPM.IsCancel = 1 And ISG.SubGroupName = 'Yarn New'
                        GROUP BY YPM.YPOMasterID, CE.ShortName, YPM.PONo, PODate, C.Name, YPM.QuotationRefNo, YPF.ValueName, DeliveryStartDate, DeliveryEndDate,
                        Proposed, UnApprove, Approved, CAINFO.SFToPLDays, CAINFO.PLToPDDays, CAINFO.PDToCFDays,
                        (CAINFO.SFToPLDays + CAINFO.PLToPDDays + CAINFO.PDToCFDays), LU.UserName,LU.Name, YPM.ConceptNo
                    ),
				    CNM AS(
				        Select YPL.YPOMasterID, YPC.BookingNo
				        FROM T_YarnPOChild YPC 
				        INNER JOIN YPL ON YPL.YPOMasterID = YPC.YPOMasterID
				        GROUP BY YPL.YPOMasterID, YPC.BookingNo
				    ),
				    CN AS(
				        Select CNM.YPOMasterID, BookingNo = STRING_AGG(CNM.BookingNo,',')
				        FROM CNM 
				        GROUP BY CNM.YPOMasterID
				    ),
                    BL AS
                    (
	                    SELECT YPL.YPOMasterID, BuyerName = STRING_AGG(B.ShortName,',')
	                    FROM YPL
	                    INNER JOIN T_YarnPOChild YPC ON YPC.YPOMasterID = YPL.YPOMasterID
	                    INNER JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = YPC.BuyerID AND YPC.BuyerID > 0
	                    GROUP BY YPL.YPOMasterID
                    ),
                    FinalList AS
                    (
                        SELECT YPL.YPOMasterID, YPL.PONo, YPL.PODate, YPL.CompanyName, YPL.SupplierName, YPL.QuotationRefNo, YPL.POFor, YPL.DeliveryStartDate, YPL.DeliveryEndDate,
			                CAST(YPL.TotalQty AS DECIMAL(18,2)) TotalQty, CAST(YPL.TotalValue AS DECIMAL(18,2)) TotalValue,
						    YPL.Approved,
						    YPL.SFToPLDays, YPL.PLToPDDays, YPL.PCFDays, YPL.InHouseDays,
						    DATEADD(day, YPL.SFToPLDays, YPL.DeliveryStartDate) AS SFToPLDate,
						    DATEADD(day, YPL.PLToPDDays, DATEADD(day, YPL.SFToPLDays, YPL.DeliveryStartDate)) AS PLToPDDate,
						    DATEADD(day, YPL.PCFDays, DATEADD(day, YPL.PLToPDDays, DATEADD(day, YPL.SFToPLDays, YPL.DeliveryStartDate))) AS PCFDate,
						    DATEADD(day, (YPL.SFToPLDays + YPL.PLToPDDays + YPL.PCFDays), YPL.DeliveryStartDate) AS InHouseDate, YPL.UserName, ConceptNo = CN.BookingNo,
                            YPL.AddedByName,
                            BL.BuyerName
                        FROM YPL
                        LEFT JOIN BL ON BL.YPOMasterID = YPL.YPOMasterID
						LEFT JOIN CN ON YPL.YPOMasterID = CN.YPOMasterID
                    )
                    SELECT * INTO #TempTable{tempGuid} FROM FinalList
                    SELECT *, Count(*) Over() TotalRows FROM #TempTable{tempGuid} M";
            }
            else if (status == Status.Revise)
            {
                sql = $@"With 
                    YPL AS(
                        SELECT YPM.YPOMasterID, CE.ShortName AS CompanyName, YPM.PONo, PODate, C.Name SupplierName, YPM.QuotationRefNo, YPF.ValueName POFor, DeliveryStartDate, DeliveryEndDate,
                        Proposed, UnApprove, Approved, SUM(YPC.POQty) TotalQty, SUM(YPC.PIValue) TotalValue,
                        CAINFO.SFToPLDays, CAINFO.PLToPDDays, CAINFO.PDToCFDays PCFDays,
                        (CAINFO.SFToPLDays + CAINFO.PLToPDDays + CAINFO.PDToCFDays) AS InHouseDays, LU.UserName, YPM.ConceptNo, AddedByName = LU.Name
                        ,YPM.IsRevision
                        FROM T_YarnPOMaster YPM --ON YPM.SupplierID = Contacts.ContactID
                        INNER JOIN T_YarnPOChild YPC ON YPC.YPOMasterID = YPM.YPOMasterID
                        Inner Join {DbNames.EPYSL}..Contacts C ON C.ContactID = YPM.SupplierID
                        LEFT JOIN {DbNames.EPYSL}..ContactAdditionalInfo CAINFO ON CAINFO.ContactID = YPM.SupplierID
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue YPF ON YPF.ValueID = YPM.POForID
                        INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = YPM.CompanyID
                        INNER JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = YPM.AddedBy
                        INNER JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = YPM.SubGroupID
                        WHERE YPM.Proposed = 0 AND YPM.Approved = 0 AND YPM.UnApprove = 0  And ISNULL(YPM.IsCancel,0) = 0 And ISG.SubGroupName = 'Yarn New' And (YPM.RevisionNo > 0 OR YPM.IsRevision = 1)
                        GROUP BY YPM.YPOMasterID, CE.ShortName, YPM.PONo, PODate, C.Name, YPM.QuotationRefNo, YPF.ValueName, DeliveryStartDate, DeliveryEndDate,
                        Proposed, UnApprove, Approved, CAINFO.SFToPLDays, CAINFO.PLToPDDays, CAINFO.PDToCFDays,
                        (CAINFO.SFToPLDays + CAINFO.PLToPDDays + CAINFO.PDToCFDays), LU.UserName,LU.Name, YPM.ConceptNo,YPM.IsRevision
                    ),
				    CNM AS(
				        Select YPL.YPOMasterID, YPC.BookingNo
				        FROM T_YarnPOChild YPC 
				        INNER JOIN YPL ON YPL.YPOMasterID = YPC.YPOMasterID
				        GROUP BY YPL.YPOMasterID, YPC.BookingNo
				    ),
				    CN AS(
				        Select CNM.YPOMasterID, BookingNo = STRING_AGG(CNM.BookingNo,',')
				        FROM CNM 
				        GROUP BY CNM.YPOMasterID
				    ),
                    BL AS
                    (
	                    SELECT YPL.YPOMasterID, BuyerName = STRING_AGG(B.ShortName,',')
	                    FROM YPL
	                    INNER JOIN T_YarnPOChild YPC ON YPC.YPOMasterID = YPL.YPOMasterID
	                    INNER JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = YPC.BuyerID AND YPC.BuyerID > 0
	                    GROUP BY YPL.YPOMasterID
                    ),
                    FinalList AS
                    (
                        SELECT YPL.YPOMasterID, YPL.PONo, YPL.PODate, YPL.CompanyName, YPL.SupplierName, YPL.QuotationRefNo, YPL.POFor, YPL.DeliveryStartDate, YPL.DeliveryEndDate,
			                CAST(YPL.TotalQty AS DECIMAL(18,2)) TotalQty, CAST(YPL.TotalValue AS DECIMAL(18,2)) TotalValue,
						    YPL.Approved,
						    YPL.SFToPLDays, YPL.PLToPDDays, YPL.PCFDays, YPL.InHouseDays,
						    DATEADD(day, YPL.SFToPLDays, YPL.DeliveryStartDate) AS SFToPLDate,
						    DATEADD(day, YPL.PLToPDDays, DATEADD(day, YPL.SFToPLDays, YPL.DeliveryStartDate)) AS PLToPDDate,
						    DATEADD(day, YPL.PCFDays, DATEADD(day, YPL.PLToPDDays, DATEADD(day, YPL.SFToPLDays, YPL.DeliveryStartDate))) AS PCFDate,
						    DATEADD(day, (YPL.SFToPLDays + YPL.PLToPDDays + YPL.PCFDays), YPL.DeliveryStartDate) AS InHouseDate, YPL.UserName, ConceptNo = CN.BookingNo, YPL.AddedByName,
                            BL.BuyerName,YPL.IsRevision
                        FROM YPL
                        LEFT JOIN BL ON BL.YPOMasterID = YPL.YPOMasterID
						LEFT JOIN CN ON YPL.YPOMasterID = CN.YPOMasterID
                    )
                    SELECT * INTO #TempTable{tempGuid} FROM FinalList
                    SELECT *, Count(*) Over() TotalRows FROM #TempTable{tempGuid} M";
            }
            else
            {
                sql = $@"With 
                    YPL AS(
                        SELECT YPM.YPOMasterID, CE.ShortName AS CompanyName, YPM.PONo, PODate, C.Name SupplierName, YPM.QuotationRefNo, YPF.ValueName POFor, DeliveryStartDate, DeliveryEndDate,
                        Proposed, UnApprove, Approved, SUM(YPC.POQty) TotalQty, SUM(YPC.PIValue) TotalValue,
                        CAINFO.SFToPLDays, CAINFO.PLToPDDays, CAINFO.PDToCFDays PCFDays,
                        (CAINFO.SFToPLDays + CAINFO.PLToPDDays + CAINFO.PDToCFDays) AS InHouseDays, LU.UserName, YPM.ConceptNo, AddedByName = LU.Name
                        FROM T_YarnPOMaster YPM --ON YPM.SupplierID = Contacts.ContactID
                        INNER JOIN T_YarnPOChild YPC ON YPC.YPOMasterID = YPM.YPOMasterID
                        Inner Join {DbNames.EPYSL}..Contacts C ON C.ContactID = YPM.SupplierID
                        LEFT JOIN {DbNames.EPYSL}..ContactAdditionalInfo CAINFO ON CAINFO.ContactID = YPM.SupplierID
                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue YPF ON YPF.ValueID = YPM.POForID
                        INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = YPM.CompanyID
                        INNER JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = YPM.AddedBy
                        INNER JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = YPM.SubGroupID
                        WHERE ISG.SubGroupName = 'Yarn New' 
                        GROUP BY YPM.YPOMasterID, CE.ShortName, YPM.PONo, PODate, C.Name, YPM.QuotationRefNo, YPF.ValueName, DeliveryStartDate, DeliveryEndDate,
                        Proposed, UnApprove, Approved, CAINFO.SFToPLDays, CAINFO.PLToPDDays, CAINFO.PDToCFDays,
                        (CAINFO.SFToPLDays + CAINFO.PLToPDDays + CAINFO.PDToCFDays), LU.UserName,LU.Name, YPM.ConceptNo
                    ),
				    CNM AS(
				        Select YPL.YPOMasterID, YPC.BookingNo
				        FROM T_YarnPOChild YPC 
				        INNER JOIN YPL ON YPL.YPOMasterID = YPC.YPOMasterID
				        GROUP BY YPL.YPOMasterID, YPC.BookingNo
				    ),
				    CN AS(
				        Select CNM.YPOMasterID, BookingNo = STRING_AGG(CNM.BookingNo,',')
				        FROM CNM 
				        GROUP BY CNM.YPOMasterID
				    ),
                    BL AS
                    (
	                    SELECT YPL.YPOMasterID, BuyerName = STRING_AGG(B.ShortName,',')
	                    FROM YPL
	                    INNER JOIN T_YarnPOChild YPC ON YPC.YPOMasterID = YPL.YPOMasterID
	                    INNER JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = YPC.BuyerID AND YPC.BuyerID > 0
	                    GROUP BY YPL.YPOMasterID
                    ),
                    FinalList AS
                    (
                        SELECT YPL.YPOMasterID, YPL.PONo, YPL.PODate, YPL.CompanyName, YPL.SupplierName, YPL.QuotationRefNo, YPL.POFor, YPL.DeliveryStartDate, YPL.DeliveryEndDate,
			                CAST(YPL.TotalQty AS DECIMAL(18,2)) TotalQty, CAST(YPL.TotalValue AS DECIMAL(18,2)) TotalValue,
						    YPL.Approved,
						    YPL.SFToPLDays, YPL.PLToPDDays, YPL.PCFDays, YPL.InHouseDays,
						    DATEADD(day, YPL.SFToPLDays, YPL.DeliveryStartDate) AS SFToPLDate,
						    DATEADD(day, YPL.PLToPDDays, DATEADD(day, YPL.SFToPLDays, YPL.DeliveryStartDate)) AS PLToPDDate,
						    DATEADD(day, YPL.PCFDays, DATEADD(day, YPL.PLToPDDays, DATEADD(day, YPL.SFToPLDays, YPL.DeliveryStartDate))) AS PCFDate,
						    DATEADD(day, (YPL.SFToPLDays + YPL.PLToPDDays + YPL.PCFDays), YPL.DeliveryStartDate) AS InHouseDate, YPL.UserName, ConceptNo = CN.BookingNo, YPL.AddedByName,
                            BL.BuyerName
                        FROM YPL
                        LEFT JOIN BL ON BL.YPOMasterID = YPL.YPOMasterID
						LEFT JOIN CN ON YPL.YPOMasterID = CN.YPOMasterID
                    )
                    SELECT * INTO #TempTable{tempGuid} FROM FinalList
                    SELECT *, Count(*) Over() TotalRows FROM #TempTable{tempGuid} M";
            }

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            sql += $@" DROP TABLE #TempTable{tempGuid} ";

            var finalList = await _service.GetDataAsync<YarnPOMaster>(sql);
            finalList.ForEach(x =>
            {
                var values = CommonFunction.GetDefaultValueWhenInvalidS(x.BookingNo).Split(',');
                x.BookingNo = string.Join(",", values.Distinct());

                values = CommonFunction.GetDefaultValueWhenInvalidS(x.BuyerName).Split(',');
                x.BuyerName = string.Join(",", values.Distinct());
            });
            return finalList;
        }

        public async Task<List<YarnPOChildOrder>> GetOrderListsFromCompany(PaginationInfo paginationInfo)
        {
            var sql = string.Empty;
            sql += @"With OL AS(
	                    SELECT ExportOrderID ExportOrderId, EOM.BuyerID BuyerId, Contacts.ShortName BuyerName, EOM.BuyerTeamID BuyerTeamId
		                    , CCT.TeamName BuyerTeam, ExportOrderNo, StyleMaster.StyleNo
                        FROM ExportOrderMaster EOM
		                    INNER JOIN Contacts ON Contacts.ContactID = EOM.BuyerID
		                    INNER JOIN ContactCategoryTeam CCT ON CCT.CategoryTeamID = EOM.BuyerTeamID
		                    INNER JOIN StyleMaster ON StyleMaster.StyleMasterID = EOM.StyleMasterID
                        WHERE EOM.EWOStatusID = 130
                    )

                    Select ExportOrderId, BuyerId, BuyerName, BuyerTeamId, BuyerTeam, ExportOrderNo, StyleNo, Count(*) Over() TotalRows
                    From OL";

            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By ExportOrderNo Desc" : paginationInfo.OrderBy;

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            try
            {
                await _gmtConnection.OpenAsync();
                var records = await _gmtConnection.QueryAsync<YarnPOChildOrder>(sql);
                return records.ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_gmtConnection.State == System.Data.ConnectionState.Open) _gmtConnection.Close();
                //_connection.Close();
            }
        }

        public async Task<YarnPOMaster> GetNewAsync()
        {
            var sql = $@"
                With
                CT As (
	                Select ContactID, Name, ShortName, CountryID
	                From {DbNames.EPYSL}..Contacts
                )

                SELECT CT.ContactID SupplierId, CT.Name SupplierName, CT.CountryID CountryOfOriginId, InLand, PaymentType, CreditDays, PortOfLoadingID, PortOfDischargeID, ShipmentModeID, CalculationOfTenor CalculationofTenure
                    , Tolerance AS ShippingTolerance, C.CountryName CountryOfOriginName
	                , SFToPLDays, PLToPDDays, PDToCFDays
	                , (SFToPLDays +  PLToPDDays + PDToCFDays) AS InHouseDays
	                , DATEADD(day, SFToPLDays, GETDATE()) AS SFToPLDate
	                , DATEADD(day, PLToPDDays, DATEADD(day, SFToPLDays, GETDATE())) AS PLToPDDate
	                , DATEADD(day, PDToCFDays, DATEADD(day, PLToPDDays, DATEADD(day, SFToPLDays, GETDATE()))) AS PDToCFDate
	                , DATEADD(day, (SFToPLDays +  PLToPDDays + PDToCFDays), GETDATE()) AS InHouseDate
                FROM CT
                Inner Join {DbNames.EPYSL}..ContactAdditionalInfo CAI ON CT.ContactID = CAI.ContactID
                Inner Join {DbNames.EPYSL}..Country C On CT.CountryID = C.CountryID;
              
                 -- Suppliers
                {CommonQueries.GetYarnSuppliers()};

                -- Entity Types
                {CommonQueries.GetEntityTypeValues()};
 
                -- Buyers
                {CommonQueries.GetContactsByCategoryType(ContactCategoryNames.BUYER)};

                -- PO For
                {CommonQueries.GetEntityTypesByEntityTypeName(EntityTypeNameConstants.RM_TRIGGER_POINTS)};

                -- Item Segments

                SELECT CAST(ISV.SegmentValue As varchar) [id], ISV.SegmentValue [text], CAST(ISN.SegmentNameID As varchar) [desc]
                FROM {DbNames.EPYSL}..ItemSegmentName ISN
                INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISN.SegmentNameID = ISV.SegmentNameID
                WHERE ISNULL(ISV.SegmentValue, '') <> '' AND ISN.SegmentNameID IN (111, 108, 131)
                ORDER BY ISV.SegmentValue;

                -- Company List
                {CommonQueries.GetInHouseSupplierByItemSubGroup(AppConstants.ITEM_SUB_GROUP_FABRIC)};

                -- Shade book
                {CommonQueries.GetYarnShadeBooks()};

                --Base Types
                SELECT id=ValueID,text=ValueName FROM {DbNames.EPYSL}..EntityTypeValue WHERE ValueID IN (2161,2162,2163,2164);

                -- DayValidDuration
                { CommonQueries.GetDayValidDurations()}

             ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);

                YarnPOMaster data = await records.ReadFirstOrDefaultAsync<YarnPOMaster>();
                Guard.Against.NullObject(data);
                data.SupplierList = await records.ReadAsync<Select2OptionModel>();


                var entityTypes = await records.ReadAsync<Select2OptionModel>();
                data.CalculationofTenureList = entityTypes.Where(x => x.desc == EntityTypeConstants.CALCULATION_OF_TENURE.ToString());
                data.ShipmentModeList = entityTypes.Where(x => x.desc == EntityTypeConstants.SHIPMENT_MODE.ToString());
                data.CreditDaysList = entityTypes.Where(x => x.desc == EntityTypeConstants.LC_TENURE.ToString());
                data.PortofLoadingList = entityTypes.Where(x => x.desc == EntityTypeConstants.PORT_OF_LOADING.ToString());
                data.PortofDischargeList = entityTypes.Where(x => x.desc == EntityTypeConstants.PORT_OF_DISCHARGE.ToString());
                data.OfferValidityList = entityTypes.Where(x => x.desc == EntityTypeConstants.OFFER_VALIDITY.ToString()).OrderBy(x => x.desc);
                data.QualityApprovalProcedureList = entityTypes.Where(x => x.desc == EntityTypeConstants.QUALITY_APPROVAL_PROCEDURE.ToString());
                data.CalculationofTenureList = entityTypes.Where(x => x.desc == EntityTypeConstants.CALCULATION_OF_TENURE.ToString());

                data.BuyerList = await records.ReadAsync<Select2OptionModel>();
                data.ExportOrderList = await records.ReadAsync<Select2OptionModel>();

                data.YarnProgramList = entityTypes.Where(x => x.desc == EntityTypeConstants.YARN_BRAND.ToString());
                data.YarnSubProgramList = entityTypes.Where(x => x.desc == EntityTypeConstants.YARN_SUB_BRAND.ToString());

                var itmeSegments = await records.ReadAsync<Select2OptionModel>();
                data.YarnCompositionList = itmeSegments.Where(x => x.desc == ItemSegmentConstants.YARN_COMPOSITION.ToString()).ToList();
                data.YarnTypeList = itmeSegments.Where(x => x.desc == ItemSegmentConstants.YARN_TYPE.ToString()).ToList();
                data.YarnColorList = itmeSegments.Where(x => x.desc == ItemSegmentConstants.YARN_COLOR.ToString()).ToList();
                data.CompanyList = await records.ReadAsync<Select2OptionModel>();
                data.ShadeList = await records.ReadAsync<Select2OptionModel>();
                data.BaseTypes = await records.ReadAsync<Select2OptionModel>();

                data.DayValidDurations = await records.ReadAsync<Select2OptionModel>();
                data.DayValidDurations = CommonFunction.GetDayValidDurations(data.DayValidDurations, string.Join(",", data.YarnPOChilds.Where(x => x.DayValidDurationId > 0).Select(x => x.DayValidDurationId).Distinct()));
                data.IsCheckDVD = true;

                data.YarnPOChilds.ForEach(x =>
                {
                    var dayObj = data.DayValidDurations.ToList().Find(d => d.id == x.DayValidDurationId.ToString());
                    if (dayObj.IsNotNull())
                    {
                        x.DayValidDurationName = dayObj.text;
                        x.DayDuration = Convert.ToInt32(dayObj.additionalValue);
                    }
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

        public async Task<List<YarnPOChildOrder>> GetBuyerListsFromCompany(PaginationInfo paginationInfo)
        {
            var sql = string.Empty;
            sql += @"With OL AS(
                            SELECT Contacts.ContactID AS BuyerId, Contacts.Name AS BuyerName
                            FROM Contacts
                            INNER JOIN ContactCategoryChild ON Contacts.ContactID = ContactCategoryChild.ContactID
                            INNER JOIN ContactCategoryHK ON ContactCategoryChild.ContactCategoryID = ContactCategoryHK.ContactCategoryID
                            WHERE ContactCategoryHK.ContactCategoryID = 1
                            )

                    SELECT BuyerId, BuyerName, Count(*) Over() TotalRows
                    FROM OL";

            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By BuyerName ASC" : paginationInfo.OrderBy;

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            try
            {
                await _gmtConnection.OpenAsync();
                var records = await _gmtConnection.QueryAsync<YarnPOChildOrder>(sql);
                return records.ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_gmtConnection.State == System.Data.ConnectionState.Open) _gmtConnection.Close();
                //_connection.Close();
            }
        }

        public async Task<YarnPOMaster> GetSupplierInfo(int supplierId)
        {
            var sql = $@"
                With CT As (
	                Select ContactID, Name, ShortName, CountryID
	                From {DbNames.EPYSL}..Contacts Where ContactID = {supplierId}
                )
                SELECT CT.ContactID SupplierId, CT.Name SupplierName, CT.CountryID CountryOfOriginId, InLand, PaymentType, CreditDays, PortOfLoadingID, PortOfDischargeID,
                ShipmentModeID, CalculationOfTenor CalculationofTenure, Tolerance AS ShippingTolerance, C.CountryName CountryOfOriginName, SFToPLDays, PLToPDDays, PDToCFDays PCFDays,
                (SFToPLDays +  PLToPDDays + PDToCFDays) AS InHouseDays, DATEADD(day, SFToPLDays, GETDATE()) AS SFToPLDate, DATEADD(day, PLToPDDays, DATEADD(day, SFToPLDays, GETDATE())) AS PLToPDDate,
                DATEADD(day, PDToCFDays, DATEADD(day, PLToPDDays, DATEADD(day, SFToPLDays, GETDATE()))) AS PCFDate, DATEADD(day, (SFToPLDays +  PLToPDDays + PDToCFDays), GETDATE()) AS InHouseDate
                , 102 SubGroupID
                FROM CT
                Inner Join {DbNames.EPYSL}..ContactAdditionalInfo CAI ON CT.ContactID = CAI.ContactID
                Inner Join {DbNames.EPYSL}..Country C On CT.CountryID = C.CountryID;

                -- Inco Terms
                {CommonQueries.GetIncoTermsBySupplier(supplierId)};

                -- Payment Terms
                {CommonQueries.GetPaymentTermsBySupplier(supplierId)};

                -- Payment Methods
                {CommonQueries.GetPaymentMethods(supplierId)};

                -- Entity Types
                {CommonQueries.GetEntityTypeValues()};

                -- Company List
                {CommonQueries.GetInHouseSupplierByItemSubGroup(AppConstants.ITEM_SUB_GROUP_FABRIC)};

                -- PO For
                {CommonQueries.GetEntityTypesByEntityTypeName(EntityTypeNameConstants.RM_TRIGGER_POINTS)}

                -- Shade book
                {CommonQueries.GetYarnShadeBooks()};

                --Base Types
                SELECT id=ValueID,text=ValueName FROM {DbNames.EPYSL}..EntityTypeValue WHERE ValueID IN (2161,2162,2163,2164);

                -- DayValidDuration
                { CommonQueries.GetDayValidDurations()}
                ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);

                YarnPOMaster data = await records.ReadFirstOrDefaultAsync<YarnPOMaster>();
                data.IncoTermsList = await records.ReadAsync<Select2OptionModel>();
                data.PaymentTermsList = await records.ReadAsync<Select2OptionModel>();
                data.TypeOfLcList = await records.ReadAsync<Select2OptionModel>();

                var entityTypes = await records.ReadAsync<Select2OptionModel>();
                data.ShipmentModeList = entityTypes.Where(x => x.desc == EntityTypeConstants.SHIPMENT_MODE.ToString());
                data.CreditDaysList = entityTypes.Where(x => x.desc == EntityTypeConstants.LC_TENURE.ToString());
                data.PortofLoadingList = entityTypes.Where(x => x.desc == EntityTypeConstants.PORT_OF_LOADING.ToString());
                data.PortofDischargeList = entityTypes.Where(x => x.desc == EntityTypeConstants.PORT_OF_DISCHARGE.ToString());
                data.OfferValidityList = entityTypes.Where(x => x.desc == EntityTypeConstants.OFFER_VALIDITY.ToString()).OrderBy(x => x.desc);
                data.QualityApprovalProcedureList = entityTypes.Where(x => x.desc == EntityTypeConstants.QUALITY_APPROVAL_PROCEDURE.ToString());
                data.CalculationofTenureList = entityTypes.Where(x => x.desc == EntityTypeConstants.CALCULATION_OF_TENURE.ToString());
                data.CompanyList = await records.ReadAsync<Select2OptionModel>();
                data.ExportOrderList = await records.ReadAsync<Select2OptionModel>();
                data.ShadeList = await records.ReadAsync<Select2OptionModel>();
                data.BaseTypes = await records.ReadAsync<Select2OptionModel>();

                data.DayValidDurations = await records.ReadAsync<Select2OptionModel>();
                data.DayValidDurations = CommonFunction.GetDayValidDurations(data.DayValidDurations, string.Join(",", data.YarnPOChilds.Where(x => x.DayValidDurationId > 0).Select(x => x.DayValidDurationId).Distinct()));
                data.IsCheckDVD = true;

                data.YarnPOChilds.ForEach(x =>
                {
                    var dayObj = data.DayValidDurations.ToList().Find(d => d.id == x.DayValidDurationId.ToString());
                    if (dayObj.IsNotNull())
                    {
                        x.DayValidDurationName = dayObj.text;
                        x.DayDuration = Convert.ToInt32(dayObj.additionalValue);
                    }
                });
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

        public async Task<YarnPOMaster> GetNewAsync(string purchaseReqId, string yarnPRChildID, int companyId)
        {
            var query =
                $@"
                -- Master Data
                WITH YPR AS (
	                SELECT M.YarnPRMasterID, M.YarnPRDate, M.YarnPRNo, M.YarnPRRequiredDate, M.Remarks, M.TriggerPointID, M.YarnPRBy, M.CompanyID
	                FROM YarnPRMaster M
	                WHERE M.YarnPRMasterID IN({purchaseReqId})
                )

                SELECT YarnPRMasterID PRMasterID, YarnPRDate PRDate,YarnPRNo PRNO, YarnPRRequiredDate PRRequiredDate, YPR.TriggerPointID, YPR.CompanyID,
                CE.ShortName CompanyName, L.Name PRByUser, EV.ValueName TriggerPoint, Remarks, YPR.YarnPRBy, 102 SubGroupID
                from YPR
                LEFT Join {DbNames.EPYSL}..CompanyEntity CE On YPR.CompanyID = CE.CompanyID
                LEFT Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = YPR.TriggerPointID
                LEFT Join {DbNames.EPYSL}..LoginUser L On YPR.YarnPRBy = L.UserCode;

                ----Child
                ;With YPO As(
					Select 
					CH.YarnPRChildID
					From YarnPOChild POC
                    INNER JOIN T_YarnPOMaster POM ON POM.YPOMasterID = POC.YPOMasterID
					INNER JOIN YarnPRChild CH ON CH.YarnPRChildID = POC.PRChildID
					Where CH.YarnPRMasterID IN({purchaseReqId})  And ISNULL(POM.IsCancel,0) != 1 And ISNULL(POC.POQty,0) >= ISNULL(CH.ReqQty,0)
				),
                YRC As (
	                Select C.YarnPRMasterID, C.PYBBookingChildID, C.YDMaterialRequirementChildItemID, C.AllocationChildID,
					C.YarnPRChildID, C.ReqQty, C.ShadeCode, C.ReqCone, C.SetupChildID, C.UnitID,C.Remarks, C.PurchaseQty, 
					C.AllocationQty, C.ItemMasterID, C.HSCode, C.ConceptID, C.YarnCategory, C.BaseTypeId, C.DayValidDurationId,
                    BT.ValueName, IsComInActive = ISNULL(CBS1.IsInactive,0), IsCountInActive = ISNULL(CBS2.IsInactive,0)
	                From YarnPRChild C
                    Left Join YPO POC On C.YarnPRChildID = POC.YarnPRChildID
					--Left Join YarnPOChild POC On C.YarnPRChildID = POC.PRChildID
                    LEFT JOIN {DbNames.EPYSL}..EntityTypeValue BT ON BT.ValueID = C.BaseTypeId
                    LEFT JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = C.ItemMasterID
                    LEFT JOIN YarnCompositionBasicSetup CBS1 ON CBS1.SegmentValueId = IM.Segment1ValueID
                    LEFT JOIN YarnCountBasicSetup CBS2 ON CBS2.SegmentValueId = IM.Segment6ValueID
	                Where C.YarnPRMasterID IN({purchaseReqId}) AND C.FPRCompanyID = {companyId} AND POC.YarnPRChildID IS NULL
                    And C.YarnPRChildID IN({yarnPRChildID})
                ),
                PYarnBookingBuyer AS
                (
	                SELECT TOP(1)YRC.YarnPRMasterID, B.BuyerID
	                FROM ProjectionYarnBookingItemChild PYC
	                INNER JOIN YRC ON YRC.PYBBookingChildID = PYC.PYBBookingChildID
	                INNER JOIN PYBookingBuyerAndBuyerTeam B ON B.PYBookingID = PYC.PYBookingID
	                GROUP BY YRC.YarnPRMasterID, B.BuyerID
                ),
                YDMaterialRequirementChildItemBuyer AS
                (
	                SELECT TOP(1)YRC.YarnPRMasterID, B.BuyerID
	                FROM YDMaterialRequirementChildItem PYC
	                INNER JOIN YRC ON YRC.YDMaterialRequirementChildItemID = PYC.YDMaterialRequirementChildItemID
	                INNER JOIN YDMaterialRequirementMaster B ON B.YDMaterialRequirementMasterID = PYC.YDMaterialRequirementMasterID
	                GROUP BY YRC.YarnPRMasterID, B.BuyerID
                ),
                YarnAllocationChildBuyer AS
                (
	                SELECT TOP(1)YRC.YarnPRMasterID, B.BuyerID
	                FROM YarnAllocationChild PYC
	                INNER JOIN YRC ON YRC.AllocationChildID = PYC.AllocationChildID
	                INNER JOIN YarnAllocationMaster B ON B.YarnAllocationID = PYC.AllocationID
	                GROUP BY YRC.YarnPRMasterID, B.BuyerID
                ),PO As(
					Select 
					YarnPRChildID = POC.PRChildID,POQty = SUM(ISNULL(POC.POQty,0))
					From YarnPOChild POC
					INNER JOIN T_YarnPOMaster POM ON POM.YPOMasterID = POC.YPOMasterID
					Inner Join YRC On YRC.YarnPRChildID = POC.PRChildID
					Where ISNULL(POM.IsCancel,0) = 0
					Group By POC.PRChildID

				),BalancePOQTY As(
					SELECT 
					YRC.YarnPRChildID,  POQty = SUM(ISNULL(YPO.POQty,0)), BalanceQTY = SUM(ISNULL(YRC.ReqQty,0)) - SUM(ISNULL(YPO.POQty,0))
					FROM YarnPRMaster M
					INNER JOIN YRC On YRC.YarnPRMasterID = M.YarnPRMasterID
					Left JOIN PO YPO ON YPO.YarnPRChildID = YRC.YarnPRChildID
					WHERE M.IsCPR = 1 AND M.Reject = 0 
					Group By YRC.YarnPRChildID
				),
                FinalList AS
                (
	                Select YPOChildID = YRC.YarnPRChildID,BalanceQTY = SUM(ISNULL(PRB.BalanceQTY,0)),
					MaxPOQty = Case When (YRC.ReqQty /100)*5 < 50 Then
							YRC.ReqQty - ISNULL(SUM(PRB.POQty),0) + 50
						Else
							YRC.ReqQty - ISNULL(SUM(PRB.POQty),0) + ((YRC.ReqQty /100)*5) End,
					--PR.BookingNo,
					BookingNo = CASE WHEN FBK.BookingNo IS NOT NULL AND FBK.BookingNo <>'' THEN FBK.BookingNo ELSE PR.YarnPRNo END,
					FCM.BookingID,
	                BuyerID =CASE WHEN ISNULL(PYB.BuyerID,0) > 0 THEN ISNULL(PYB.BuyerID,0)
				                  WHEN ISNULL(YDM.BuyerID,0) > 0 THEN ISNULL(YDM.BuyerID,0)
				                  WHEN ISNULL(YAB.BuyerID,0) > 0 THEN ISNULL(YAB.BuyerID,0)
								  WHEN ISNULL(BM.BuyerID,0) > 0 THEN ISNULL(BM.BuyerID,0)
								  WHEN ISNULL(SBM.BuyerID,0) > 0 THEN ISNULL(SBM.BuyerID,0)
				                  ELSE FBK.BuyerID END,
					YarnChildPoEWOs = CASE WHEN ISNULL(SBM.BookingID,0) > 0 THEN SBM.SLNo ELSE EOM.ExportOrderNo END,
	                YRC.ShadeCode,YRC.ReqCone, YRC.YarnPRChildID PRChildID, 
	                YRC.YarnPRMasterID PRMasterID, PR.YarnPRNo, YRC.SetupChildID, YRC.UnitID,YRC.Remarks, YRC.ReqQty, YRC.PurchaseQty, YRC.AllocationQty, 
	                YRC.ItemMasterID ItemMasterID, YRC.HSCode, YRC.ConceptID, 
                    ConceptNo = CASE WHEN FCM.ConceptNo is not null AND FCM.ConceptNo <> '' Then FCM.ConceptNo Else PR.ConceptNo END, 
                    YRC.YarnCategory, 
	                'Kg' AS DisplayUnitDesc, IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, 
	                IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID, IM.Segment8ValueID, IM.Segment9ValueID, 
	                IM.Segment10ValueID, IM.Segment11ValueID, IM.Segment12ValueID, IM.Segment13ValueID, IM.Segment14ValueID, 
	                IM.Segment15ValueID, ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, 
	                ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, 
	                ISV7.SegmentValue Segment7ValueDesc, ISV8.SegmentValue Segment8ValueDesc, YRC.BaseTypeId PoForId, 
	                PO.ValueName PoFor, PR.YarnPRRequiredDate, YRC.BaseTypeId,BaseTypeName = YRC.ValueName, YRC.DayValidDurationId,
                    IsInvalidItem = CASE WHEN YRC.IsComInActive = 1 OR YRC.IsCountInActive = 1 THEN 1 ELSE 0 END
	                From YRC
	                INNER JOIN YarnPRMaster PR On PR.YarnPRMasterID = YRC.YarnPRMasterID
	                LEFT JOIN PYarnBookingBuyer PYB ON PYB.YarnPRMasterID = PR.YarnPRMasterID
	                LEFT JOIN YDMaterialRequirementChildItemBuyer YDM ON YDM.YarnPRMasterID = PR.YarnPRMasterID
	                LEFT JOIN YarnAllocationChildBuyer YAB ON YAB.YarnPRMasterID = PR.YarnPRMasterID
	                INNER JOIN {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = YRC.ItemMasterID
	                LEFT JOIN FreeConceptMaster FCM On YRC.ConceptID = FCM.ConceptID
	                LEFT JOIN FBookingAcknowledge  FBK ON FCM.BookingID = FBK.BookingID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID
	                LEFT JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = YRC.UnitID
	                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue PO ON PO.ValueID = YRC.BaseTypeId
	                LEFT JOIN {DbNames.EPYSL}..Contacts CON ON CON.ContactID = FBK.BuyerID
					LEFT JOIN {DbNames.EPYSL}..BookingMaster BM ON BM.BookingNo = PR.YarnPRNo
					LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingNo = PR.YarnPRNo
					LEFT JOIN {DbNames.EPYSL}..ExportOrderMaster EOM ON EOM.ExportOrderID = BM.ExportOrderID
                    Left Join BalancePOQTY PRB On PRB.YarnPRChildID = YRC.YarnPRChildID
                    Group By  YRC.YarnPRChildID,
					CASE WHEN FBK.BookingNo IS NOT NULL AND FBK.BookingNo <>'' THEN FBK.BookingNo ELSE PR.YarnPRNo END,
					FCM.BookingID,
	                CASE WHEN ISNULL(PYB.BuyerID,0) > 0 THEN ISNULL(PYB.BuyerID,0)
				                  WHEN ISNULL(YDM.BuyerID,0) > 0 THEN ISNULL(YDM.BuyerID,0)
				                  WHEN ISNULL(YAB.BuyerID,0) > 0 THEN ISNULL(YAB.BuyerID,0)
								  WHEN ISNULL(BM.BuyerID,0) > 0 THEN ISNULL(BM.BuyerID,0)
								  WHEN ISNULL(SBM.BuyerID,0) > 0 THEN ISNULL(SBM.BuyerID,0)
				                  ELSE FBK.BuyerID END,
					CASE WHEN ISNULL(SBM.BookingID,0) > 0 THEN SBM.SLNo ELSE EOM.ExportOrderNo END,
	                YRC.ShadeCode,YRC.ReqCone, YRC.YarnPRChildID, 
	                YRC.YarnPRMasterID , PR.YarnPRNo, YRC.SetupChildID, YRC.UnitID,YRC.Remarks, YRC.ReqQty, YRC.PurchaseQty, YRC.AllocationQty, 
	                YRC.ItemMasterID , YRC.HSCode, YRC.ConceptID, 
                    CASE WHEN FCM.ConceptNo is not null AND FCM.ConceptNo <> '' Then FCM.ConceptNo Else PR.ConceptNo END, 
                    YRC.YarnCategory, 
	                IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, 
	                IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID, IM.Segment8ValueID, IM.Segment9ValueID, 
	                IM.Segment10ValueID, IM.Segment11ValueID, IM.Segment12ValueID, IM.Segment13ValueID, IM.Segment14ValueID, 
	                IM.Segment15ValueID, ISV1.SegmentValue , ISV2.SegmentValue , ISV3.SegmentValue , 
	                ISV4.SegmentValue , ISV5.SegmentValue , ISV6.SegmentValue , 
	                ISV7.SegmentValue , ISV8.SegmentValue , YRC.BaseTypeId , 
	                PO.ValueName , PR.YarnPRRequiredDate, YRC.BaseTypeId, YRC.ValueName,YRC.DayValidDurationId,
                    CASE WHEN YRC.IsComInActive = 1 OR YRC.IsCountInActive = 1 THEN 1 ELSE 0 END
                )
                SELECT FL.*,BuyerName = CON.ShortName
                FROM FinalList FL
                LEFT JOIN {DbNames.EPYSL}..Contacts CON ON CON.ContactID = FL.BuyerID;
                
                -- Suppliers
                {CommonQueries.GetYarnSuppliers()};

                -- Buyers
                {CommonQueries.GetContactsByCategoryId(ContactCategoryConstants.CONTACT_CATEGORY_BUYER)};

                -- PO For
                {CommonQueries.GetEntityTypesByEntityTypeName(EntityTypeNameConstants.RM_TRIGGER_POINTS)};
                 
                -- Company List
                {CommonQueries.GetInHouseSupplierByItemSubGroup(AppConstants.ITEM_SUB_GROUP_FABRIC)};

                -- Shade book
                {CommonQueries.GetYarnShadeBooks()}

                --Base Types
                SELECT id=ValueID,text=ValueName FROM {DbNames.EPYSL}..EntityTypeValue WHERE ValueID IN (2161,2162,2163,2164);

                -- DayValidDuration
                { CommonQueries.GetDayValidDurations()}
             ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);

                YarnPOMaster data = await records.ReadFirstOrDefaultAsync<YarnPOMaster>();

                data.CompanyId = data.CompanyId == 0 ? companyId : data.CompanyId;
               
                data.YarnPOChilds = records.Read<YarnPOChild>().ToList();

                data.SupplierList = await records.ReadAsync<Select2OptionModel>();
                data.BuyerList = await records.ReadAsync<Select2OptionModel>();
                data.PIForList = await records.ReadAsync<Select2OptionModel>();
                data.CompanyList = await records.ReadAsync<Select2OptionModel>();

                data.DeliveryStartDate = data.YarnPOChilds.OrderBy(i => i.YarnPRRequiredDate).First().YarnPRRequiredDate;
                data.DeliveryEndDate = data.YarnPOChilds.OrderBy(i => i.YarnPRRequiredDate).Last().YarnPRRequiredDate;
                data.ShadeList = await records.ReadAsync<Select2OptionModel>();

                data.BaseTypes = await records.ReadAsync<Select2OptionModel>();

                data.DayValidDurations = await records.ReadAsync<Select2OptionModel>();
                data.DayValidDurations = CommonFunction.GetDayValidDurations(data.DayValidDurations, string.Join(",", data.YarnPOChilds.Where(x => x.DayValidDurationId > 0).Select(x => x.DayValidDurationId).Distinct()));
                data.IsCheckDVD = true;

                data.YarnPOChilds.ForEach(x =>
                {
                    x.YarnChildPoBuyerIds = string.Join(",", x.BuyerID);
                    x.BuyerNames = string.Join(",", x.BuyerName);
                    x.PoForId = x.BaseTypeId;
                    x.POFor = x.BaseTypeName;

                    var dayObj = data.DayValidDurations.ToList().Find(d => d.id == x.DayValidDurationId.ToString());
                    if (dayObj.IsNotNull())
                    {
                        x.DayValidDurationName = dayObj.text;
                        x.DayDuration = Convert.ToInt32(dayObj.additionalValue);
                    }
                });
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
        public async Task<List<YarnPOMaster>> GetPRItems(int companyId, string prChildIds, PaginationInfo paginationInfo)
        {
            prChildIds = prChildIds.Trim().Length == 0 ? "0" : prChildIds;
            string tempGuid = CommonFunction.GetNewGuid();
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YPOMasterID Desc" : paginationInfo.OrderBy;

            string sql = $@";WITH PO As(
	                        Select 
	                        YarnPRChildID = POC.PRChildID,POQty = SUM(ISNULL(POC.POQty,0))
	                        From T_YarnPOChild POC
	                        INNER JOIN T_YarnPOMaster POM ON POM.YPOMasterID = POC.YPOMasterID
	                        Where ISNULL(POM.IsCancel,0) = 0 AND POM.CompanyID = {companyId}
	                        Group By POC.PRChildID
                        ), 
                        YPO As(
	                        Select 
	                        YPO.YarnPRChildID
	                        From PO YPO 
	                        INNER JOIN YarnPRChild CH ON CH.YarnPRChildID = YPO.YarnPRChildID
	                        Group By YPO.YarnPRChildID
	                        Having SUM(ISNULL(YPO.POQty,0)) >= SUM(ISNULL(CH.ReqQty,0))
                        ),
                        PRBalance As(
                            SELECT CH.YarnPRChildID, BalanceQTY = SUM(ISNULL(CH.ReqQty,0)) - SUM(ISNULL(YPO.POQty,0)),POQty = SUM(ISNULL(YPO.POQty,0)),
                            BuyerName = STRING_AGG(B.ShortName,',')
                            FROM YarnPRMaster M
                            INNER JOIN T_YarnPRChild CH ON CH.YarnPRMasterID = M.YarnPRMasterID
                            LEFT JOIN T_FreeConceptMaster FCM on FCM.ConceptID = CH.ConceptID
                            LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = FCM.BuyerID AND FCM.BuyerID > 0
                            Left JOIN PO YPO ON YPO.YarnPRChildID = CH.YarnPRChildID
                            WHERE M.IsCPR = 1 AND M.Reject = 0 AND M.CompanyID = {companyId}
                            AND CH.YarnPRChildID Not In (Select YarnPRChildID From YPO)
                            Group By CH.YarnPRChildID
                        ),
                        YPR AS 
                        (
                            SELECT M.YarnPRMasterID, M.YarnPRDate, M.YarnPRNo, M.YarnPRRequiredDate, M.TriggerPointID, M.YarnPRBy, 
                            CH.YarnPRChildID, CH.ItemMasterID, CH.FPRCompanyID As CompanyID, CH.ShadeCode, CH.ReqQty, M.ConceptNo,
                            DayValidDurationName = CASE WHEN DVD.DayValidDurationId > 0 AND DVD.DayDuration > 1 THEN CONCAT(ET.ValueName,' (',CAST(DVD.DayDuration AS VARCHAR),' days)')
                                                        WHEN DVD.DayValidDurationId > 0 AND DVD.DayDuration = 1 THEN CONCAT(ET.ValueName,' (',CAST(DVD.DayDuration AS VARCHAR),' day)')
                                                        ELSE ''
                                                        END,
                            CH.BaseTypeId, CH.ConceptID,CH.DayValidDurationId
                            FROM T_YarnPRMaster M
                            INNER JOIN T_YarnPRChild CH ON CH.YarnPRMasterID = M.YarnPRMasterID
                            INNER JOIN {DbNames.EPYSL}..ItemSubGroup ISG ON ISG.SubGroupID = M.SubGroupID
                            LEFT JOIN DayValidDuration DVD ON DVD.DayValidDurationId = CH.DayValidDurationId
                            LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ET ON ET.ValueID = DVD.LocalOrImportId
                            WHERE M.IsCPR = 1 AND M.Reject = 0 AND M.CompanyID = {companyId}
	                        AND CH.YarnPRChildID Not In (Select YarnPRChildID From YPO)
                        ),
                        M AS 
                        (
	                        SELECT BalanceQTY = SUM(ISNULL(PRB.BalanceQTY,0)), POQty =  SUM(ISNULL(PRB.POQty,0)), YPR.YarnPRMasterID As PRMasterID, YPR.YarnPRDate As PRDate, YPR.YarnPRNo As PRNO, YPR.YarnPRNo,
                            YPR.YarnPRRequiredDate As PRRequiredDate, YPR.YarnPRBy, YPR.CompanyID, CE.ShortName As CompanyName, 
	                        L.Name As PRByUser, YPR.YarnPRChildID, YPR.ConceptNo, YPR.ShadeCode, YPR.ReqQty,  
                            ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, 
                            ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, 
                            ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc, ISV8.SegmentValue Segment8ValueDesc,
                            PRB.BuyerName, YPR.DayValidDurationName, 
                            PoForId = YPR.BaseTypeId, PoFor = BT.ValueName, YPR.ConceptID,YPR.BaseTypeId, YPR.DayValidDurationId
	                        From YPR
	                        INNER JOIN {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = YPR.ItemMasterID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID 
	                        LEFT Join {DbNames.EPYSL}..CompanyEntity CE On YPR.CompanyID = CE.CompanyID
	                        LEFT Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = YPR.TriggerPointID
	                        LEFT Join {DbNames.EPYSL}..LoginUser L On YPR.YarnPRBy = L.UserCode
                            LEFT JOIN {DbNames.EPYSL}..EntityTypeValue BT On BT.ValueId = YPR.BaseTypeId
                            Left Join PRBalance PRB On PRB.YarnPRChildID = YPR.YarnPRChildID
	                        GROUP BY YPR.YarnPRMasterID, YPR.YarnPRDate, YPR.YarnPRNo, YPR.YarnPRRequiredDate, YPR.YarnPRBy, 
                            YPR.CompanyID, CE.ShortName, L.Name, YPR.YarnPRChildID, YPR.ConceptNo, YPR.ShadeCode, YPR.ReqQty, 
                            ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue, ISV4.SegmentValue, ISV5.SegmentValue, 
                            ISV6.SegmentValue, ISV7.SegmentValue, ISV8.SegmentValue,PRB.BuyerName, YPR.DayValidDurationName,YPR.BaseTypeId,BT.ValueName, 
                            YPR.ConceptID,YPR.BaseTypeId, YPR.DayValidDurationId
                        ),
                        FinalList AS
                        (
	                        SELECT BalanceQTY, POQty, PRMasterID, PRDate, PRNO, YarnPRNo,
                            PRRequiredDate, YarnPRBy, CompanyID, CompanyName, 
	                        PRByUser, YarnPRChildID, ConceptNo, ShadeCode, ReqQty,  
                            Segment1ValueDesc, Segment2ValueDesc, 
                            Segment3ValueDesc, Segment4ValueDesc, Segment5ValueDesc, 
                            Segment6ValueDesc, Segment7ValueDesc, Segment8ValueDesc,
                            BuyerName, DayValidDurationName, PoForId, PoFor, ConceptID,
                            BaseTypeId, DayValidDurationId
	                        FROM M

	                        UNION ALL

	                        SELECT BalanceQTY = SUM(ISNULL(PRB.BalanceQTY,0)), POQty =  SUM(ISNULL(PRB.POQty,0)), PRM.YarnPRMasterID As PRMasterID, PRM.YarnPRDate As PRDate, PRM.YarnPRNo As PRNO, PRM.YarnPRNo,
                            PRM.YarnPRRequiredDate As PRRequiredDate, PRM.YarnPRBy, PRM.CompanyID, CE.ShortName As CompanyName, 
	                        L.Name As PRByUser, PRC.YarnPRChildID, PRM.ConceptNo, PRC.ShadeCode, PRC.ReqQty,  
                            ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, 
                            ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, 
                            ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc, ISV8.SegmentValue Segment8ValueDesc,
                            PRB.BuyerName, DayValidDurationName = CASE WHEN DVD.DayValidDurationId > 0 AND DVD.DayDuration > 1 THEN CONCAT(ET.ValueName,' (',CAST(DVD.DayDuration AS VARCHAR),' days)')
                                                        WHEN DVD.DayValidDurationId > 0 AND DVD.DayDuration = 1 THEN CONCAT(ET.ValueName,' (',CAST(DVD.DayDuration AS VARCHAR),' day)')
                                                        ELSE ''
                                                        END, 
	                        PoForId = PRC.BaseTypeId, PoFor = BT.ValueName, PRC.ConceptID,
                            PRC.BaseTypeId, PRC.DayValidDurationId
	                        From T_YarnPRChild PRC
	                        INNER JOIN YarnPRMaster PRM ON PRM.YarnPRMasterID = PRC.YarnPRMasterID
	                        INNER JOIN {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = PRC.ItemMasterID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
	                        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID 
	                        LEFT Join {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = PRM.CompanyID
	                        LEFT Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = PRM.TriggerPointID
	                        LEFT Join {DbNames.EPYSL}..LoginUser L On L.UserCode = PRM.YarnPRBy
                            LEFT JOIN {DbNames.EPYSL}..EntityTypeValue BT On BT.ValueId = PRC.BaseTypeId
	                        LEFT JOIN DayValidDuration DVD ON DVD.DayValidDurationId = PRC.DayValidDurationId
	                        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue ET ON ET.ValueID = DVD.LocalOrImportId
                            Left Join PRBalance PRB On PRB.YarnPRChildID = PRC.YarnPRChildID
	                        LEFT JOIN M ON M.YarnPRChildID = PRC.YarnPRChildID
	                        WHERE PRC.YarnPRChildID IN ({prChildIds}) AND M.YarnPRChildID IS NULL 
	                        GROUP BY PRM.YarnPRMasterID, PRM.YarnPRDate, PRM.YarnPRNo, PRM.YarnPRRequiredDate, PRM.YarnPRBy, 
                            PRM.CompanyID, CE.ShortName, L.Name, PRC.YarnPRChildID, PRM.ConceptNo, PRC.ShadeCode, PRC.ReqQty, 
                            ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue, ISV4.SegmentValue, ISV5.SegmentValue, 
                            ISV6.SegmentValue, ISV7.SegmentValue, ISV8.SegmentValue,PRB.BuyerName, CASE WHEN ISNULL(PRC.DayValidDurationId,0) > 0 THEN ET.ValueName ELSE '' END,
	                        PRC.BaseTypeId,BT.ValueName, CASE WHEN DVD.DayValidDurationId > 0 AND DVD.DayDuration > 1 THEN CONCAT(ET.ValueName,' (',CAST(DVD.DayDuration AS VARCHAR),' days)')
                                                        WHEN DVD.DayValidDurationId > 0 AND DVD.DayDuration = 1 THEN CONCAT(ET.ValueName,' (',CAST(DVD.DayDuration AS VARCHAR),' day)')
                                                        ELSE ''
                                                        END, PRC.ConceptID,PRC.BaseTypeId, PRC.DayValidDurationId
                        )
                SELECT M.* INTO #TempTable{tempGuid} FROM FinalList M
                SELECT *,Count(*) Over() TotalRows FROM #TempTable{tempGuid}";

            orderBy = "ORDER BY PRMasterID DESC";

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            sql += $@" DROP TABLE #TempTable{tempGuid} ";

            var finalList = await _service.GetDataAsync<YarnPOMaster>(sql);
            finalList.ForEach(x =>
            {
                var values = CommonFunction.GetDefaultValueWhenInvalidS(x.BookingNo).Split(',');
                x.BookingNo = string.Join(",", values.Distinct());

                values = CommonFunction.GetDefaultValueWhenInvalidS(x.BuyerName).Split(',');
                x.BuyerName = string.Join(",", values.Distinct());
            });
            return finalList;
        }
        public async Task<List<YarnPOChildOrder>> GetExportOrderListsFromIdEdit(int yarnPoMasterId)
        {
            var sql = string.Empty;
            sql += $@"SELECT YPO.YarnPOOrderID, YPO.ExportOrderId, YPO.BuyerId, Contacts.ShortName BuyerName, YPO.BuyerTeamId
	                    , CCT.TeamName BuyerTeam, EOM.ExportOrderNo, SM.StyleNo, CustomerGroupId
                    FROM YarnPOForOrder YPO
                    INNER JOIN {DbNames.EPYSL}..Contacts ON Contacts.ContactID = YPO.BuyerID
                    INNER JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = YPO.BuyerTeamID
                    INNER JOIN {DbNames.EPYSL}..ExportOrderMaster EOM ON EOM.ExportOrderID = YPO.ExportOrderID
                    INNER JOIN {DbNames.EPYSL}..StyleMaster SM ON SM.StyleMasterID = EOM.StyleMasterID
                    WHERE YPO.YPOMasterID = {yarnPoMasterId}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryAsync<YarnPOChildOrder>(sql);
                return records.ToList();
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

        public async Task<List<YarnPOChild>> GetYarnPOItemsIdEdit(int masterId)
        {
            var sql = $@"
                SELECT YPC.YPOChildID, YPC.YPOMasterID YPOMasterID, YPC.ItemMasterID ItemMasterID, YPC.YarnCategory, YPC.UnitID, YPC.BuyerID, YPC.POQty, YPC.NoOfThread
	                , YPC.Rate, (YPC.PIValue) PIValue, YPC.Remarks, 'Kg' AS DisplayUnitDesc, YPC.YarnLotNo, YPC.HSCode, YPC.YarnShade, YPC.PoForId
                    , IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID
					, IM.Segment8ValueID, IM.Segment9ValueID, IM.Segment10ValueID, IM.Segment11ValueID, IM.Segment12ValueID, IM.Segment13ValueID
					, ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc
					, ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc, ISV8.SegmentValue Segment8ValueDesc
                FROM YarnPOChild YPC
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YPC.ItemMasterID
                LEFT JOIN  {DbNames.EPYSL}..Unit U ON U.UnitID = YPC.UnitID
				LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
				LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
				LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
				LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
				LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
				LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
				LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
				LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID
                WHERE YPC.YPOMasterID = {masterId}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryAsync<YarnPOChild>(sql);
                return records.ToList();
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

        //    public async Task<YarnPOMaster> GetAsync(int id)
        //    {
        //        var sql = $@"
        //            With
        //            M As (
        //             Select * From YarnPOMaster Where YPOMasterID = {id}
        //            )
        //            , CTI As (
        //             SELECT CT.ContactID SupplierId, CT.Name SupplierName, CT.CountryID CountryOfOriginId, InLand, PaymentType, CreditDays, PortOfLoadingID, PortOfDischargeID, ShipmentModeID, CalculationOfTenor CalculationofTenure
        //              , Tolerance AS ShippingTolerance, C.CountryName CountryOfOriginName
        //              , SFToPLDays, PLToPDDays, PDToCFDays PCFDays
        //              , (SFToPLDays +  PLToPDDays + PDToCFDays) AS InHouseDays
        //              , DATEADD(day, SFToPLDays, GETDATE()) AS SFToPLDate
        //              , DATEADD(day, PLToPDDays, DATEADD(day, SFToPLDays, GETDATE())) AS PLToPDDate
        //              , DATEADD(day, PDToCFDays, DATEADD(day, PLToPDDays, DATEADD(day, SFToPLDays, GETDATE()))) AS PCFDate
        //              , DATEADD(day, (SFToPLDays +  PLToPDDays + PDToCFDays), GETDATE()) AS InHouseDate
        //             FROM {DbNames.EPYSL}..Contacts CT
        //             Inner Join {DbNames.EPYSL}..ContactAdditionalInfo CAI ON CT.ContactID = CAI.ContactID
        //             Inner Join {DbNames.EPYSL}..Country C On CT.CountryID = C.CountryID
        //            )
        //            Select M.YPOMasterID, M.PONo, M.PODate, M.RevisionNo, M.RevisionDate, M.CompanyID, M.SupplierID, M.POForID, M.CurrencyID, M.DeliveryStartDate, M.DeliveryEndDate
        //            , M.Remarks, M.InternalNotes, M.IncoTermsID, M.PaymentTermsID, M.TypeOfLCID, M.TenureofLC, M.CalculationofTenure, M.CreditDays, M.ReImbursementCurrencyID
        //            , M.Charges, M.CountryOfOriginID, M.TransShipmentAllow, M.ShippingTolerance, M.PortofLoadingID PortofLoadingID, M.PortofDischargeID PortofDischargeID, M.ShipmentModeID, M.Proposed, M.ProposedBy
        //            , M.ProposedDate, M.UnApprove, M.UnApproveBy, M.UnApproveDate, M.UnapproveReason, M.Approved, M.ApprovedBy, M.ApprovedDate, M.AddedBy, M.UpdatedBy, M.ConceptNo
        //            , M.OfferValidity, M.QualityApprovalProcedureId, M.SignIn, M.SignInDate, M.SupplierReject, M.SupplierRejectReason, M.QuotationRefNo
        //            , M.QuotationRefDate, M.SignInBy, M.SupplierAcknowledge, M.SupplierAcknowledgeBy, M.SupplierAcknowledgeDate, M.SupplierRejectBy, M.DateAdded, M.DateUpdated
        //            , CTI.SupplierName, CTI.InLand, CTI.SFToPLDays, CTI.PLToPDDays, CTI.PCFDays, CTI.InHouseDays, CTI.SFToPLDate, CE.ShortName CompanyName
        //         , CTI.PLToPDDate, CTI.PCFDate, CTI.InHouseDate, CountryOfOriginName
        //            From M
        //            Inner Join CTI On M.SupplierID = CTI.SupplierId
        //            INNER JOIN	{DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = M.CompanyID;

        //            -- Child Data
        //            /*;With
        //            YRC As (
        //                Select PC.*, FCM.ConceptNo
        //             From YarnPOChild PC
        //	Left Join FreeConceptMaster FCM On FCM.ConceptID = PC.ConceptID
        //             Where PC.YPOMasterID = {id}
        //            )
        //            , I As (
        //             Select a.YPOChildID, a.BuyerID, C.[Name] BuyerName
        //             From YarnPOChildBuyer a
        //	LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = a.BuyerID
        //             Where a.YPOMasterID =  {id}
        //            )
        //            , YPOB As (
        //             Select YPOChildID
        //                    , STUFF(
        //                    (
        //                        SELECT ', ' + CAST(I.BuyerID as varchar) AS [text()]
        //                        FROM I
        //                        WHERE I.YPOChildID = O.YPOChildID
        //                        FOR XML PATH('')
        //                    ), 1, 2, '') As YarnChildPoBuyerIds
        //		, STUFF(
        //                    (
        //                        SELECT ', ' + CAST(I.BuyerName as varchar) AS [text()]
        //                        FROM I
        //                        WHERE I.YPOChildID = O.YPOChildID
        //                        FOR XML PATH('')
        //                    ), 1, 2, '') As BuyerNames
        //                From YarnPOChildBuyer O
        //	Where YPOMasterID = {id}
        //                Group By YPOChildID
        //            )
        //            , O As (
        //             Select YO.YPOChildID, YO.ExportOrderID, ExportOrderNo
        //             From YarnPOChildOrder YO
        //	LEFT JOIN {DbNames.EPYSL}..ExportOrderMaster EOM ON EOM.ExportOrderID = YO.ExportOrderID
        //             Where YPOMasterID =  {id}
        //            )
        //            , YPOO As (
        //             Select YPOChildID
        //              , STUFF(
        //              (
        //               SELECT ', ' + CAST(O.ExportOrderID As varchar) AS [text()]
        //               FROM O OI
        //               WHERE OI.YPOChildID = O.YPOChildID
        //               FOR XML PATH('')
        //              ), 1, 2, '') As YarnChildPoExportIds
        //		, STUFF(
        //              (
        //               SELECT ', ' + CAST(O.ExportOrderNo As varchar) AS [text()]
        //               FROM O OI
        //               WHERE OI.YPOChildID = O.YPOChildID
        //               FOR XML PATH('')
        //              ), 1, 2, '') As YarnChildPoEWOs
        //             From O
        //            )
        //            Select YRC.*,YarnPRChild.ShadeCode,YarnPRChild.ReqCone,
        //             PO.ValueName POFor,
        //                U.DisplayUnitDesc, YPOB.YarnChildPoBuyerIds, YPOB.BuyerNames, YPOO.YarnChildPoExportIds, YPOO.YarnChildPoEWOs, YarnPRMaster.YarnPRNo, YarnPRChild.ReqQty,
        //             IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID,
        //	IM.Segment8ValueID, IM.Segment9ValueID, IM.Segment10ValueID, IM.Segment11ValueID, IM.Segment12ValueID, IM.Segment13ValueID,
        //	ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc,
        //	ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc, ISV8.SegmentValue Segment8ValueDesc
        //            From YRC
        //            INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YRC.ItemMasterID
        //            Inner JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = YRC.UnitID
        //            LEFT JOIN YPOB On YPOB.YPOChildID = YRC.YPOChildID
        //            LEFT JOIN YPOO On YPOO.YPOChildID = YRC.YPOChildID
        //LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
        //LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
        //LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
        //LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
        //LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
        //LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
        //LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
        //LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID
        //LEFT JOIN {DbNames.EPYSL}..EntityTypeValue PO ON PO.ValueID = YRC.POForID
        //            LEFT JOIN YarnPRChild On YarnPRChild.YarnPRChildID = YRC.PRChildID
        //            LEFT JOIN YarnPRMaster On YarnPRMaster.YarnPRMasterID = YRC.PRMasterID; */


        //            ;With YRC As 
        //            (
        //                Select PC.*, FCM.ConceptNo
        //             From YarnPOChild PC
        //             Left Join FreeConceptMaster FCM On FCM.ConceptID = PC.ConceptID
        //             Where PC.YPOMasterID = {id}
        //            )
        //            , I As (
        //             Select a.YPOChildID, a.BuyerID, C.[Name] BuyerName
        //             From YarnPOChildBuyer a
        //             LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = a.BuyerID
        //             Where a.YPOMasterID =  {id}
        //            )
        //            , YPOB As (
        //             Select YPOChildID
        //                    , STUFF(
        //                    (
        //                        SELECT ', ' + CAST(I.BuyerID as varchar) AS [text()]
        //                        FROM I
        //                        WHERE I.YPOChildID = O.YPOChildID
        //                        FOR XML PATH('')
        //                    ), 1, 2, '') As YarnChildPoBuyerIds
        //              , STUFF(
        //                    (
        //                        SELECT ', ' + CAST(I.BuyerName as varchar) AS [text()]
        //                        FROM I
        //                        WHERE I.YPOChildID = O.YPOChildID
        //                        FOR XML PATH('')
        //                    ), 1, 2, '') As BuyerNames
        //                From YarnPOChildBuyer O
        //             Where YPOMasterID = {id}
        //                Group By YPOChildID
        //            )
        //            , CO As (
        //             Select YO.YPOChildID, YO.ExportOrderID, ExportOrderNo
        //             From YarnPOChildOrder YO
        //             LEFT JOIN {DbNames.EPYSL}..ExportOrderMaster EOM ON EOM.ExportOrderID = YO.ExportOrderID
        //             Where YPOMasterID =  {id}
        //            )
        //            , YPOO As (
        //             Select YPOChildID
        //                    , STUFF(
        //                    (
        //                        SELECT ', ' + CAST(CO.ExportOrderID As varchar) AS [text()]
        //                        FROM CO
        //                        WHERE CO.YPOChildID = O.YPOChildID --Group By O.ExportOrderID
        //                        FOR XML PATH('')
        //                    ), 1, 2, '') As YarnChildPoExportIds
        //              , STUFF(
        //                    (
        //                        SELECT ', ' + CAST(CO.ExportOrderNo As varchar) AS [text()]
        //                        FROM CO
        //                        WHERE CO.YPOChildID = O.YPOChildID
        //                        FOR XML PATH('')
        //                    ), 1, 2, '') As YarnChildPoEWOs
        //                From YarnPOChildOrder O
        //             Where YPOMasterID = {id}
        //                Group By YPOChildID  
        //            )
        //            Select YRC.*,YarnPRChild.ShadeCode,YarnPRChild.ReqCone, PO.ValueName POFor, U.DisplayUnitDesc, 
        //            YPOB.YarnChildPoBuyerIds, REPLACE(YPOB.BuyerNames,'amp;','')BuyerNames, YPOO.YarnChildPoExportIds, 
        //            YPOO.YarnChildPoEWOs, YarnPRMaster.YarnPRNo, YarnPRChild.ReqQty, IM.Segment1ValueID, IM.Segment2ValueID, 
        //            IM.Segment3ValueID, IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID,
        //            IM.Segment8ValueID, IM.Segment9ValueID, IM.Segment10ValueID, IM.Segment11ValueID, IM.Segment12ValueID, 
        //            IM.Segment13ValueID, ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, 
        //            ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, 
        //            ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc, ISV8.SegmentValue Segment8ValueDesc
        //            From YRC
        //            INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YRC.ItemMasterID
        //            Inner JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = YRC.UnitID
        //            LEFT JOIN YPOB On YPOB.YPOChildID = YRC.YPOChildID
        //            LEFT JOIN YPOO On YPOO.YPOChildID = YRC.YPOChildID
        //            LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
        //            LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
        //            LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
        //            LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
        //            LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
        //            LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
        //            LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
        //            LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID
        //            LEFT JOIN {DbNames.EPYSL}..EntityTypeValue PO ON PO.ValueID = YRC.POForID
        //            LEFT JOIN YarnPRChild On YarnPRChild.YarnPRChildID = YRC.PRChildID
        //            LEFT JOIN YarnPRMaster On YarnPRMaster.YarnPRMasterID = YRC.PRMasterID; 

        //            -- Inco Terms
        //            {CommonQueries.GetIncoTermsByPO(id)};

        //            -- Payment Terms
        //            {CommonQueries.GetPaymentTermsByPO(id)};

        //            -- Entity Types
        //            {CommonQueries.GetEntityTypeValues()};

        //            -- Buyers
        //            {CommonQueries.GetContactsByCategoryId(ContactCategoryConstants.CONTACT_CATEGORY_BUYER)};

        //            -- PO For
        //            {CommonQueries.GetEntityTypesByEntityTypeName(EntityTypeNameConstants.RM_TRIGGER_POINTS)};

        //            -- Item Segments
        //            {CommonQueries.GetItemSegments()};

        //            -- Company List
        //            {CommonQueries.GetInHouseSupplierByItemSubGroup(AppConstants.ITEM_SUB_GROUP_FABRIC)};

        //            -- Payment Methods
        //            {CommonQueries.GetPaymentMethodsByPO(id)};

        //            -- Suppliers
        //            {CommonQueries.GetYarnSuppliers()};";

        //        try
        //        {
        //            await _connection.OpenAsync();

        //            var records = await _connection.QueryMultipleAsync(sql);
        //            YarnPOMaster data = await records.ReadFirstOrDefaultAsync<YarnPOMaster>();
        //            Guard.Against.NullObject(data);

        //            data.YarnPOChilds = records.Read<YarnPOChild>().ToList();
        //            data.IncoTermsList = await records.ReadAsync<Select2OptionModel>();
        //            data.PaymentTermsList = await records.ReadAsync<Select2OptionModel>();

        //            var entityTypes = await records.ReadAsync<Select2OptionModel>();
        //            data.ShipmentModeList = entityTypes.Where(x => x.desc == EntityTypeConstants.SHIPMENT_MODE.ToString());
        //            data.CreditDaysList = entityTypes.Where(x => x.desc == EntityTypeConstants.LC_TENURE.ToString());
        //            data.PortofLoadingList = entityTypes.Where(x => x.desc == EntityTypeConstants.PORT_OF_LOADING.ToString());
        //            data.PortofDischargeList = entityTypes.Where(x => x.desc == EntityTypeConstants.PORT_OF_DISCHARGE.ToString());
        //            data.OfferValidityList = entityTypes.Where(x => x.desc == EntityTypeConstants.OFFER_VALIDITY.ToString()).OrderBy(x => x.desc);
        //            data.QualityApprovalProcedureList = entityTypes.Where(x => x.desc == EntityTypeConstants.QUALITY_APPROVAL_PROCEDURE.ToString());
        //            data.CalculationofTenureList = entityTypes.Where(x => x.desc == EntityTypeConstants.CALCULATION_OF_TENURE.ToString());

        //            data.BuyerList = await records.ReadAsync<Select2OptionModel>();



        //            data.ExportOrderList = await records.ReadAsync<Select2OptionModel>();
        //            //data.PIForList = await records.ReadAsync<Select2OptionModel>();



        //            var itmeSegments = await records.ReadAsync<Select2OptionModel>();
        //            data.FiberTypeList = itmeSegments.Where(x => x.desc == ItemSegmentConstants.FIBER_TYPE.ToString());
        //            data.BlendTypeList = itmeSegments.Where(x => x.desc == ItemSegmentConstants.BLEND_TYPE.ToString());
        //            data.YarnTypeList = itmeSegments.Where(x => x.desc == ItemSegmentConstants.YARN_TYPE.ToString());
        //            data.YarnProgramList = itmeSegments.Where(x => x.desc == ItemSegmentConstants.YARN_PROGRAM.ToString());
        //            data.YarnSubProgramList = itmeSegments.Where(x => x.desc == ItemSegmentConstants.YARN_SUB_PROGRAM.ToString());
        //            data.ManufacturingLineList = itmeSegments.Where(x => x.desc == ItemSegmentConstants.MANUFACTURING_LINE.ToString());
        //            data.ManufacturingProcessList = itmeSegments.Where(x => x.desc == ItemSegmentConstants.MANUFACTURING_PROCESS.ToString());
        //            data.ManufacturingSubProcessList = itmeSegments.Where(x => x.desc == ItemSegmentConstants.MANUFACTURING_SUB_PROCESS.ToString());
        //            data.YarnCompositionList = itmeSegments.Where(x => x.desc == ItemSegmentConstants.YARN_COMPOSITION.ToString());
        //            data.YarnColorList = itmeSegments.Where(x => x.desc == ItemSegmentConstants.YARN_COLOR.ToString());
        //            data.ColorGradeList = itmeSegments.Where(x => x.desc == ItemSegmentConstants.COLOR_GRADE.ToString());
        //            data.ShadeList = itmeSegments.Where(x => x.desc == ItemSegmentConstants.YARN_SHADE.ToString());

        //            data.CompanyList = await records.ReadAsync<Select2OptionModel>();
        //            data.TypeOfLcList = await records.ReadAsync<Select2OptionModel>();
        //            data.SupplierList = await records.ReadAsync<Select2OptionModel>();
        //            return data;
        //        }
        //        catch (Exception ex)
        //        {
        //            throw ex;
        //        }
        //        finally
        //        {
        //            if (_connection.State == System.Data.ConnectionState.Open) _connection.Close();
        //        }
        //    }

        public async Task<YarnPOMaster> GetAsync(int id)
        {
            var sql = $@"
                With
                M As (
	                Select * From YarnPOMaster Where YPOMasterID = {id}
                )
                ,PRC AS
                (
	                SELECT M.YPOMasterID, CountYarnReceive = COUNT(YRC.ChildID)
	                FROM M
	                INNER JOIN YarnPOChild POC ON POC.YPOMasterID = M.YPOMasterID
	                INNER JOIN YarnReceiveChild YRC ON YRC.POChildID = POC.YPOChildID
	                GROUP BY M.YPOMasterID
                )
                ,CTI As 
                (
	                SELECT CT.ContactID SupplierId, CT.Name SupplierName, CT.CountryID CountryOfOriginId, InLand, PaymentType, CreditDays, PortOfLoadingID, PortOfDischargeID, ShipmentModeID, CalculationOfTenor CalculationofTenure
		            , Tolerance AS ShippingTolerance, C.CountryName CountryOfOriginName
		            , SFToPLDays, PLToPDDays, PDToCFDays PCFDays
		            , (SFToPLDays +  PLToPDDays + PDToCFDays) AS InHouseDays
		            , DATEADD(day, SFToPLDays, GETDATE()) AS SFToPLDate
		            , DATEADD(day, PLToPDDays, DATEADD(day, SFToPLDays, GETDATE())) AS PLToPDDate
		            , DATEADD(day, PDToCFDays, DATEADD(day, PLToPDDays, DATEADD(day, SFToPLDays, GETDATE()))) AS PCFDate
		            , DATEADD(day, (SFToPLDays +  PLToPDDays + PDToCFDays), GETDATE()) AS InHouseDate
	                FROM {DbNames.EPYSL}..Contacts CT
	                Inner Join {DbNames.EPYSL}..ContactAdditionalInfo CAI ON CT.ContactID = CAI.ContactID
	                Inner Join {DbNames.EPYSL}..Country C On CT.CountryID = C.CountryID
                )
                Select M.YPOMasterID, M.PONo, M.PODate, M.RevisionNo, M.RevisionDate, M.CompanyID, M.SupplierID, M.POForID, M.CurrencyID, M.DeliveryStartDate, M.DeliveryEndDate
                , M.Remarks, M.InternalNotes, M.IncoTermsID, M.PaymentTermsID, M.TypeOfLCID, M.TenureofLC, M.CalculationofTenure, M.CreditDays, M.ReImbursementCurrencyID
                , M.Charges, M.CountryOfOriginID, M.TransShipmentAllow, M.ShippingTolerance, M.PortofLoadingID PortofLoadingID, M.PortofDischargeID PortofDischargeID, M.ShipmentModeID, M.Proposed, M.ProposedBy
                , M.ProposedDate, M.UnApprove, M.UnApproveBy, M.UnApproveDate, M.UnapproveReason, M.Approved, M.ApprovedBy, M.ApprovedDate, M.AddedBy, M.UpdatedBy, M.ConceptNo
                , M.OfferValidity, M.QualityApprovalProcedureId, M.SignIn, M.SignInDate, M.SupplierReject, M.SupplierRejectReason, M.QuotationRefNo
                , M.QuotationRefDate, M.SignInBy, M.SupplierAcknowledge, M.SupplierAcknowledgeBy, M.SupplierAcknowledgeDate, M.SupplierRejectBy, M.DateAdded, M.DateUpdated
                , CTI.SupplierName, CTI.InLand, CTI.SFToPLDays, CTI.PLToPDDays, CTI.PCFDays, CTI.InHouseDays, CTI.SFToPLDate, CE.ShortName CompanyName
                , IsReceivedPO = Case When YRM.POID IS NOT NULL THEN 1 ELSE 0 END
	            , CTI.PLToPDDate, CTI.PCFDate, CTI.InHouseDate, CountryOfOriginName, M.PRMasterID
                , IsYarnReceived = CASE WHEN ISNULL(PRC.CountYarnReceive,0) > 0 THEN 1 ELSE 0 END
                From M
                Left JOIN YarnReceiveMaster YRM ON YRM.POID = M.YPOMasterID
                Inner Join CTI On M.SupplierID = CTI.SupplierId
                INNER JOIN	{DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = M.CompanyID
                LEFT JOIN PRC ON PRC.YPOMasterID = M.YPOMasterID;

                --Child Data 
                ;With YRC As 
                (
                    Select PC.*, FCM.ConceptNo, BaseTypeId = ISNULL(PRC.BaseTypeId,0)
	                From YarnPOChild PC
	                Left Join FreeConceptMaster FCM On FCM.ConceptID = PC.ConceptID
	                LEFT JOIN YarnPRChild PRC ON PRC.YarnPRChildID = PC.PRChildID
	                Where PC.YPOMasterID = {id}
                ),
				AllPO As 
                (
                    Select TotalPoQty = Sum(PC.PoQty), YRC.PRChildID
	                From YarnPOChild PC
	                LEFT JOIN YRC ON YRC.PRChildID = PC.PRChildID
					Group By YRC.PRChildID
                )
                ,POCB As (
	                Select a.YPOChildID, BuyerID = MAX(a.BuyerID), C.[Name] BuyerName
	                From YarnPOChildBuyer a
	                LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = a.BuyerID
	                Where a.YPOMasterID = {id}
	                GROUP BY a.YPOChildID, C.[Name]
                ),
                CO As (
	                Select YO.YPOChildID, YO.ExportOrderID, ExportOrderNo
	                From YarnPOChildOrder YO
	                LEFT JOIN {DbNames.EPYSL}..ExportOrderMaster EOM ON EOM.ExportOrderID = YO.ExportOrderID
	                Where YPOMasterID =  {id}
                ),
                YRChild AS
                (
	                SELECT YRC1.POChildID, YRC1.ItemMasterID, ReceiveQty = SUM(YRC1.ReceiveQty)
	                FROM YarnReceiveChild YRC1
	                INNER JOIN YRC ON YRC.YPOChildID = YRC1.POChildID AND YRC.ItemMasterID = YRC1.ItemMasterID
	                GROUP BY YRC1.POChildID, YRC1.ItemMasterID
                ),
                YPIRPO AS
                ( 
	                Select YPO.ItemMasterID, ReceiveQty = SUM(YPO.PIQty)
	                From YarnPIReceivePO YPO
	                INNER JOIN YRC ON YRC.YPOMasterID = YPO.YPOMasterID AND YRC.ItemMasterID = YPO.ItemMasterID
	                Group By YPO.ItemMasterID
                )
                Select YRC.*,YarnPRChild.ReqCone, BT.ValueName POFor,YRC.POForID, U.DisplayUnitDesc,
                YarnPRMaster.YarnPRNo, YarnPRChild.ReqQty, IM.Segment1ValueID, IM.Segment2ValueID, 
                IM.Segment3ValueID, IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID,
                IM.Segment8ValueID, IM.Segment9ValueID, IM.Segment10ValueID, IM.Segment11ValueID, IM.Segment12ValueID, 
                IM.Segment13ValueID, ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, 
                ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, 
                ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc, ISV8.SegmentValue Segment8ValueDesc,
                YRC.ReceivedCompleted, IsYarnReceive = CASE WHEN ISNULL(YRC1.POChildID, 0) > 0 THEN 1 ELSE 0 END, YRC1.ReceiveQty,
                IsYarnReceiveByPI = CASE WHEN ISNULL(YPO.ItemMasterID,0)>0 Then 1 Else 0 END, YRC.BaseTypeId,
                BuyerID = CASE WHEN ISNULL(YRC.BuyerID,0) > 0 THEN YRC.BuyerID ELSE POCB.BuyerID END, 
                YRC.DayValidDurationId,
                DayValidDurationIdPR = YarnPRChild.DayValidDurationId,
				MaxPOQty = Case When (YarnPRChild.ReqQty /100)*5 < 50 Then
						YarnPRChild.ReqQty - ISNULL(AP.TotalPoQty,0) + ISNULL(YRC.PoQty,0) + 50
					Else
						YarnPRChild.ReqQty - ISNULL(AP.TotalPoQty,0) + ISNULL(YRC.PoQty,0) + ((YarnPRChild.ReqQty /100)*5) End
                From YRC
				LEFT JOIN AllPO AP ON AP.PRChildID = YRC.PRChildID 
                LEFT JOIN POCB ON POCB.YPOChildID = YRC.YPOChildID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YRC.ItemMasterID
                Inner JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = YRC.UnitID
                LEFT JOIN YRChild YRC1 ON YRC1.POChildID = YRC.YPOChildID AND YRC1.ItemMasterID = YRC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue BT ON BT.ValueID = YRC.POForID
                LEFT JOIN YPIRPO YPO ON YPO.ItemMasterID = YRC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID
                LEFT JOIN YarnPRChild On YarnPRChild.YarnPRChildID = YRC.PRChildID
                LEFT JOIN YarnPRMaster On YarnPRMaster.YarnPRMasterID = YRC.PRMasterID;

                -- Inco Terms
                {CommonQueries.GetIncoTermsByPO(id)};

                -- Payment Terms
                {CommonQueries.GetPaymentTermsByPO(id)};

                -- Entity Types
                {CommonQueries.GetEntityTypeValues()};

                -- Buyers
                {CommonQueries.GetContactsByCategoryId(ContactCategoryConstants.CONTACT_CATEGORY_BUYER)};

                -- PO For
                {CommonQueries.GetEntityTypesByEntityTypeName(EntityTypeNameConstants.RM_TRIGGER_POINTS)};

                -- Item Segments

                SELECT CAST(ISV.SegmentValue As varchar) [id], ISV.SegmentValue [text], CAST(ISN.SegmentNameID As varchar) [desc]
                FROM {DbNames.EPYSL}..ItemSegmentName ISN
                INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISN.SegmentNameID = ISV.SegmentNameID
                WHERE ISNULL(ISV.SegmentValue, '') <> '' AND ISN.SegmentNameID IN (258, 259, 260, 261, 262, 263, 264, 265, 108, 111, 131)
                ORDER BY ISV.SegmentValue;

                -- Company List
                {CommonQueries.GetInHouseSupplierByItemSubGroup(AppConstants.ITEM_SUB_GROUP_FABRIC)};

                -- Payment Methods
                {CommonQueries.GetPaymentMethodsByPO(id)};

                -- Suppliers
                {CommonQueries.GetYarnSuppliers()};

                -- POV2 IgnoreValidation
                SELECT * FROM POV2IgnoreValidation;

                -- Shade book
                {CommonQueries.GetYarnShadeBooks()}

                --Export Order
                Select * From YarnPOChildOrder Where YPOMasterID = {id};

                --Export Buyer
                Select B.*, C.ShortName BuyerName 
                From YarnPOChildBuyer B
                LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = B.BuyerID
                Where YPOMasterID = {id};

                --Base Types
                SELECT id=ValueID,text=ValueName FROM {DbNames.EPYSL}..EntityTypeValue WHERE ValueID IN (2161,2162,2163,2164);

                -- DayValidDuration
                { CommonQueries.GetDayValidDurations()}
                ";

            try
            {
                await _connection.OpenAsync();

                var records = await _connection.QueryMultipleAsync(sql);
                YarnPOMaster data = await records.ReadFirstOrDefaultAsync<YarnPOMaster>();
                Guard.Against.NullObject(data);

                data.YarnPOChilds = records.Read<YarnPOChild>().ToList();
                data.IncoTermsList = await records.ReadAsync<Select2OptionModel>();
                data.PaymentTermsList = await records.ReadAsync<Select2OptionModel>();

                var entityTypes = await records.ReadAsync<Select2OptionModel>();
                data.ShipmentModeList = entityTypes.Where(x => x.desc == EntityTypeConstants.SHIPMENT_MODE.ToString());
                data.CreditDaysList = entityTypes.Where(x => x.desc == EntityTypeConstants.LC_TENURE.ToString());
                data.PortofLoadingList = entityTypes.Where(x => x.desc == EntityTypeConstants.PORT_OF_LOADING.ToString());
                data.PortofDischargeList = entityTypes.Where(x => x.desc == EntityTypeConstants.PORT_OF_DISCHARGE.ToString());
                data.OfferValidityList = entityTypes.Where(x => x.desc == EntityTypeConstants.OFFER_VALIDITY.ToString()).OrderBy(x => x.desc);
                data.QualityApprovalProcedureList = entityTypes.Where(x => x.desc == EntityTypeConstants.QUALITY_APPROVAL_PROCEDURE.ToString());
                data.CalculationofTenureList = entityTypes.Where(x => x.desc == EntityTypeConstants.CALCULATION_OF_TENURE.ToString());

                data.BuyerList = await records.ReadAsync<Select2OptionModel>();
                data.ExportOrderList = await records.ReadAsync<Select2OptionModel>();
                //data.PIForList = await records.ReadAsync<Select2OptionModel>(); 

                var itmeSegments = await records.ReadAsync<Select2OptionModel>();
                data.FiberTypeList = itmeSegments.Where(x => x.desc == ItemSegmentConstants.FIBER_TYPE.ToString()).ToList();
                data.BlendTypeList = itmeSegments.Where(x => x.desc == ItemSegmentConstants.BLEND_TYPE.ToString()).ToList();
                data.YarnTypeList = itmeSegments.Where(x => x.desc == ItemSegmentConstants.YARN_TYPE.ToString()).ToList();
                data.YarnProgramList = itmeSegments.Where(x => x.desc == ItemSegmentConstants.YARN_PROGRAM.ToString()).ToList();
                data.YarnSubProgramList = itmeSegments.Where(x => x.desc == ItemSegmentConstants.YARN_SUB_PROGRAM.ToString()).ToList();
                data.ManufacturingLineList = itmeSegments.Where(x => x.desc == ItemSegmentConstants.MANUFACTURING_LINE.ToString()).ToList();
                data.ManufacturingProcessList = itmeSegments.Where(x => x.desc == ItemSegmentConstants.MANUFACTURING_PROCESS.ToString()).ToList();
                data.ManufacturingSubProcessList = itmeSegments.Where(x => x.desc == ItemSegmentConstants.MANUFACTURING_SUB_PROCESS.ToString()).ToList();
                data.YarnCompositionList = itmeSegments.Where(x => x.desc == ItemSegmentConstants.YARN_COMPOSITION.ToString()).ToList();
                data.YarnColorList = itmeSegments.Where(x => x.desc == ItemSegmentConstants.YARN_COLOR.ToString()).ToList();
                data.ColorGradeList = itmeSegments.Where(x => x.desc == ItemSegmentConstants.COLOR_GRADE.ToString()).ToList();

                data.CompanyList = await records.ReadAsync<Select2OptionModel>();
                data.TypeOfLcList = await records.ReadAsync<Select2OptionModel>();
                data.SupplierList = await records.ReadAsync<Select2OptionModel>();

                var ignoreValidationPO = await records.ReadAsync<YarnPOMaster>();
                if (ignoreValidationPO != null && ignoreValidationPO.Count() > 0)
                {
                    data.IgnoreValidationPOIds = ignoreValidationPO.Select(x => x.YPOMasterID).ToList();
                }
                data.ShadeList = await records.ReadAsync<Select2OptionModel>();
                var exportOrders = await records.ReadAsync<YarnPOChildOrder>();
                var exportBuyers = await records.ReadAsync<YarnPOChildBuyer>();
                data.BaseTypes = await records.ReadAsync<Select2OptionModel>();

                data.DayValidDurations = await records.ReadAsync<Select2OptionModel>();
                data.DayValidDurations = CommonFunction.GetDayValidDurations(data.DayValidDurations, string.Join(",", data.YarnPOChilds.Where(x => x.DayValidDurationId > 0).Select(x => x.DayValidDurationId).Distinct()));
                data.IsCheckDVD = data.PoDate < CommonConstent.YarnSourcingModeImplementDate ? false : true;

                data.YarnPOChilds.ForEach(x =>
                {
                    List<YarnPOChildOrder> tempList = exportOrders.Where(y => y.YPOChildID == x.YPOChildID).ToList();
                    x.YarnChildPoExportIds = string.Join(",", tempList.Select(y => y.ExportOrderId).Distinct());
                    x.YarnChildPoEWOs = string.Join(",", tempList.Select(y => y.EWONo).Distinct());

                    List<YarnPOChildBuyer> tempListB = exportBuyers.Where(y => y.YPOChildID == x.YPOChildID).ToList();
                    x.YarnChildPoBuyerIds = string.Join(",", tempListB.Select(y => y.BuyerId).Distinct());
                    x.BuyerNames = string.Join(",", tempListB.Select(y => y.BuyerName).Distinct());

                    var dayObj = data.DayValidDurations.ToList().Find(d => d.id == x.DayValidDurationId.ToString());
                    if (dayObj.IsNotNull())
                    {
                        x.DayValidDurationName = dayObj.text;
                        x.DayDuration = Convert.ToInt32(dayObj.additionalValue);
                    }

                    var dayObj_PR = data.DayValidDurations.ToList().Find(d => d.id == x.DayValidDurationIdPR.ToString());
                    if (dayObj_PR.IsNotNull())
                    {
                        x.DayValidDurationIdPRName = dayObj_PR.text;
                        x.DayDurationPR = Convert.ToInt32(dayObj_PR.additionalValue);
                    }
                });

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
        public async Task<YarnPOMaster> GetRevisionAsync(int id)
        {
            var sql = $@"
                With
                M As (
	                Select * From YarnPOMaster Where YPOMasterID = {id}
                )
                ,PRC AS
                (
	                SELECT M.YPOMasterID, CountYarnReceive = COUNT(YRC.ChildID)
	                FROM M
	                INNER JOIN YarnPOChild POC ON POC.YPOMasterID = M.YPOMasterID
	                INNER JOIN YarnReceiveChild YRC ON YRC.POChildID = POC.YPOChildID
	                GROUP BY M.YPOMasterID
                )
                ,CTI As 
                (
	                SELECT CT.ContactID SupplierId, CT.Name SupplierName, CT.CountryID CountryOfOriginId, InLand, PaymentType, CreditDays, PortOfLoadingID, PortOfDischargeID, ShipmentModeID, CalculationOfTenor CalculationofTenure
		            , Tolerance AS ShippingTolerance, C.CountryName CountryOfOriginName
		            , SFToPLDays, PLToPDDays, PDToCFDays PCFDays
		            , (SFToPLDays +  PLToPDDays + PDToCFDays) AS InHouseDays
		            , DATEADD(day, SFToPLDays, GETDATE()) AS SFToPLDate
		            , DATEADD(day, PLToPDDays, DATEADD(day, SFToPLDays, GETDATE())) AS PLToPDDate
		            , DATEADD(day, PDToCFDays, DATEADD(day, PLToPDDays, DATEADD(day, SFToPLDays, GETDATE()))) AS PCFDate
		            , DATEADD(day, (SFToPLDays +  PLToPDDays + PDToCFDays), GETDATE()) AS InHouseDate
	                FROM {DbNames.EPYSL}..Contacts CT
	                Inner Join {DbNames.EPYSL}..ContactAdditionalInfo CAI ON CT.ContactID = CAI.ContactID
	                Inner Join {DbNames.EPYSL}..Country C On CT.CountryID = C.CountryID
                )
                Select M.YPOMasterID, M.PONo, M.PODate, M.RevisionNo, M.RevisionDate, M.CompanyID, M.SupplierID, M.POForID, M.CurrencyID, M.DeliveryStartDate, M.DeliveryEndDate
                , M.Remarks, M.InternalNotes, M.IncoTermsID, M.PaymentTermsID, M.TypeOfLCID, M.TenureofLC, M.CalculationofTenure, M.CreditDays, M.ReImbursementCurrencyID
                , M.Charges, M.CountryOfOriginID, M.TransShipmentAllow, M.ShippingTolerance, M.PortofLoadingID PortofLoadingID, M.PortofDischargeID PortofDischargeID, M.ShipmentModeID, M.Proposed, M.ProposedBy
                , M.ProposedDate, M.UnApprove, M.UnApproveBy, M.UnApproveDate, M.UnapproveReason, M.Approved, M.ApprovedBy, M.ApprovedDate, M.AddedBy, M.UpdatedBy, M.ConceptNo
                , M.OfferValidity, M.QualityApprovalProcedureId, M.SignIn, M.SignInDate, M.SupplierReject, M.SupplierRejectReason, M.QuotationRefNo
                , M.QuotationRefDate, M.SignInBy, M.SupplierAcknowledge, M.SupplierAcknowledgeBy, M.SupplierAcknowledgeDate, M.SupplierRejectBy, M.DateAdded, M.DateUpdated
                , CTI.SupplierName, CTI.InLand, CTI.SFToPLDays, CTI.PLToPDDays, CTI.PCFDays, CTI.InHouseDays, CTI.SFToPLDate, CE.ShortName CompanyName
	            , CTI.PLToPDDate, CTI.PCFDate, CTI.InHouseDate, CountryOfOriginName, M.PRMasterID
                , IsYarnReceived = CASE WHEN ISNULL(PRC.CountYarnReceive,0) > 0 THEN 1 ELSE 0 END
                From M
                Inner Join CTI On M.SupplierID = CTI.SupplierId
                INNER JOIN	{DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = M.CompanyID
                LEFT JOIN PRC ON PRC.YPOMasterID = M.YPOMasterID;

                --Child Data 
                ;With YRC As 
                (
                    Select PC.YPOChildID, PC.YPOMasterID, PRC.ItemMasterID, PRC.YarnCategory, PRC.UnitID, PC.POQty
	                ,PC.Rate, PC.PIValue, PC.Remarks, PC.YarnLotNo, PC.HSCode, PC.YarnProgramId, PC.NoOfThread
	                ,PC.POForID, PRMasterID = PRC.YarnPRMasterID, PRChildID = PRC.YarnPRChildID, PRC.ShadeCode, PRC.ConceptID
	                ,PC.EWOOthers, PC.POCone, PC.QuotationRefNo, PC.QuotationRefDate, PC.BookingNo, PC.ReceivedCompleted, PC.ReceivedDate
	                ,PC.BuyerID,PC.YarnStockSetId,PRC.DayValidDurationId
	                ,FCM.ConceptNo, BaseTypeId = ISNULL(PRC.BaseTypeId,0)
	                From YarnPOChild PC
	                Left Join FreeConceptMaster FCM On FCM.ConceptID = PC.ConceptID
	                LEFT JOIN YarnPRChild PRC ON PRC.YarnPRChildID = PC.PRChildID
	                Where PC.YPOMasterID = {id}
                )
                ,POCB As (
	                Select a.YPOChildID, BuyerID = MAX(a.BuyerID), C.[Name] BuyerName
	                From YarnPOChildBuyer a
	                LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = a.BuyerID
	                Where a.YPOMasterID = {id}
	                GROUP BY a.YPOChildID, C.[Name]
                ),
                CO As (
	                Select YO.YPOChildID, YO.ExportOrderID, ExportOrderNo
	                From YarnPOChildOrder YO
	                LEFT JOIN {DbNames.EPYSL}..ExportOrderMaster EOM ON EOM.ExportOrderID = YO.ExportOrderID
	                Where YPOMasterID =  {id}
                ),
                YRChild AS
                (
	                SELECT YRC1.POChildID, YRC1.ItemMasterID, ReceiveQty = SUM(YRC1.ReceiveQty)
	                FROM YarnReceiveChild YRC1
	                INNER JOIN YRC ON YRC.YPOChildID = YRC1.POChildID AND YRC.ItemMasterID = YRC1.ItemMasterID
	                GROUP BY YRC1.POChildID, YRC1.ItemMasterID
                ),
                YPIRPO AS
                ( 
	                Select YPO.ItemMasterID, ReceiveQty = SUM(YPO.PIQty)
	                From YarnPIReceivePO YPO
	                INNER JOIN YRC ON YRC.YPOMasterID = YPO.YPOMasterID AND YRC.ItemMasterID = YPO.ItemMasterID
	                Group By YPO.ItemMasterID
                )
                Select YRC.YPOChildID, YRC.YPOMasterID, YRC.ItemMasterID, YRC.YarnCategory, YRC.UnitID, YRC.POQty
	                ,YRC.Rate, YRC.PIValue, YRC.Remarks, YRC.YarnLotNo, YRC.HSCode, YRC.YarnProgramId, YRC.NoOfThread
	                ,YRC.POForID, YRC.PRMasterID, YRC.PRChildID, YRC.ShadeCode, YRC.ConceptID
	                ,YRC.EWOOthers, YRC.POCone, YRC.QuotationRefNo, YRC.QuotationRefDate, YRC.BookingNo, YRC.ReceivedCompleted, YRC.ReceivedDate
	                ,YRC.BuyerID,YRC.YarnStockSetId,YRC.DayValidDurationId
	                ,YRC.ConceptNo, BaseTypeId = ISNULL(YRC.BaseTypeId,0)
                ,YarnPRChild.ReqCone, BT.ValueName POFor,YRC.POForID, U.DisplayUnitDesc,
                YarnPRMaster.YarnPRNo, YarnPRChild.ReqQty, IM.Segment1ValueID, IM.Segment2ValueID, 
                IM.Segment3ValueID, IM.Segment4ValueID, IM.Segment5ValueID, IM.Segment6ValueID, IM.Segment7ValueID,
                IM.Segment8ValueID, IM.Segment9ValueID, IM.Segment10ValueID, IM.Segment11ValueID, IM.Segment12ValueID, 
                IM.Segment13ValueID, ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, 
                ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, 
                ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc, ISV8.SegmentValue Segment8ValueDesc,
                YRC.ReceivedCompleted, IsYarnReceive = CASE WHEN ISNULL(YRC1.POChildID, 0) > 0 THEN 1 ELSE 0 END, YRC1.ReceiveQty,
                IsYarnReceiveByPI = CASE WHEN ISNULL(YPO.ItemMasterID,0)>0 Then 1 Else 0 END, YRC.BaseTypeId,
                BuyerID = CASE WHEN ISNULL(YRC.BuyerID,0) > 0 THEN YRC.BuyerID ELSE POCB.BuyerID END, 
                YRC.DayValidDurationId,
                DayValidDurationIdPR = YarnPRChild.DayValidDurationId
                From YRC
                LEFT JOIN POCB ON POCB.YPOChildID = YRC.YPOChildID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YRC.ItemMasterID
                Inner JOIN {DbNames.EPYSL}..Unit U ON U.UnitID = YRC.UnitID
                LEFT JOIN YRChild YRC1 ON YRC1.POChildID = YRC.YPOChildID AND YRC1.ItemMasterID = YRC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue BT ON BT.ValueID = YRC.POForID
                LEFT JOIN YPIRPO YPO ON YPO.ItemMasterID = YRC.ItemMasterID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID
                LEFT JOIN YarnPRChild On YarnPRChild.YarnPRChildID = YRC.PRChildID
                LEFT JOIN YarnPRMaster On YarnPRMaster.YarnPRMasterID = YRC.PRMasterID;

                -- Inco Terms
                {CommonQueries.GetIncoTermsByPO(id)};

                -- Payment Terms
                {CommonQueries.GetPaymentTermsByPO(id)};

                -- Entity Types
                {CommonQueries.GetEntityTypeValues()};

                -- Buyers
                {CommonQueries.GetContactsByCategoryId(ContactCategoryConstants.CONTACT_CATEGORY_BUYER)};

                -- PO For
                {CommonQueries.GetEntityTypesByEntityTypeName(EntityTypeNameConstants.RM_TRIGGER_POINTS)};

                -- Item Segments

                SELECT CAST(ISV.SegmentValue As varchar) [id], ISV.SegmentValue [text], CAST(ISN.SegmentNameID As varchar) [desc]
                FROM {DbNames.EPYSL}..ItemSegmentName ISN
                INNER JOIN {DbNames.EPYSL}..ItemSegmentValue ISV ON ISN.SegmentNameID = ISV.SegmentNameID
                WHERE ISNULL(ISV.SegmentValue, '') <> '' AND ISN.SegmentNameID IN (258, 259, 260, 261, 262, 263, 264, 265, 108, 111, 131)
                ORDER BY ISV.SegmentValue;

                -- Company List
                {CommonQueries.GetInHouseSupplierByItemSubGroup(AppConstants.ITEM_SUB_GROUP_FABRIC)};

                -- Payment Methods
                {CommonQueries.GetPaymentMethodsByPO(id)};

                -- Suppliers
                {CommonQueries.GetYarnSuppliers()};

                -- POV2 IgnoreValidation
                SELECT * FROM POV2IgnoreValidation;

                -- Shade book
                {CommonQueries.GetYarnShadeBooks()}

                --Export Order
                Select * From YarnPOChildOrder Where YPOMasterID = {id};

                --Export Buyer
                Select B.*, C.ShortName BuyerName 
                From YarnPOChildBuyer B
                LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = B.BuyerID
                Where YPOMasterID = {id};

                --Base Types
                SELECT id=ValueID,text=ValueName FROM {DbNames.EPYSL}..EntityTypeValue WHERE ValueID IN (2161,2162,2163,2164);

                -- DayValidDuration
                { CommonQueries.GetDayValidDurations()}
                ";

            try
            {
                await _connection.OpenAsync();

                var records = await _connection.QueryMultipleAsync(sql);
                YarnPOMaster data = await records.ReadFirstOrDefaultAsync<YarnPOMaster>();
                Guard.Against.NullObject(data);

                data.YarnPOChilds = records.Read<YarnPOChild>().ToList();
                data.IncoTermsList = await records.ReadAsync<Select2OptionModel>();
                data.PaymentTermsList = await records.ReadAsync<Select2OptionModel>();

                var entityTypes = await records.ReadAsync<Select2OptionModel>();
                data.ShipmentModeList = entityTypes.Where(x => x.desc == EntityTypeConstants.SHIPMENT_MODE.ToString());
                data.CreditDaysList = entityTypes.Where(x => x.desc == EntityTypeConstants.LC_TENURE.ToString());
                data.PortofLoadingList = entityTypes.Where(x => x.desc == EntityTypeConstants.PORT_OF_LOADING.ToString());
                data.PortofDischargeList = entityTypes.Where(x => x.desc == EntityTypeConstants.PORT_OF_DISCHARGE.ToString());
                data.OfferValidityList = entityTypes.Where(x => x.desc == EntityTypeConstants.OFFER_VALIDITY.ToString()).OrderBy(x => x.desc);
                data.QualityApprovalProcedureList = entityTypes.Where(x => x.desc == EntityTypeConstants.QUALITY_APPROVAL_PROCEDURE.ToString());
                data.CalculationofTenureList = entityTypes.Where(x => x.desc == EntityTypeConstants.CALCULATION_OF_TENURE.ToString());

                data.BuyerList = await records.ReadAsync<Select2OptionModel>();
                data.ExportOrderList = await records.ReadAsync<Select2OptionModel>();
                //data.PIForList = await records.ReadAsync<Select2OptionModel>(); 

                var itmeSegments = await records.ReadAsync<Select2OptionModel>();
                data.FiberTypeList = itmeSegments.Where(x => x.desc == ItemSegmentConstants.FIBER_TYPE.ToString()).ToList();
                data.BlendTypeList = itmeSegments.Where(x => x.desc == ItemSegmentConstants.BLEND_TYPE.ToString()).ToList();
                data.YarnTypeList = itmeSegments.Where(x => x.desc == ItemSegmentConstants.YARN_TYPE.ToString()).ToList();
                data.YarnProgramList = itmeSegments.Where(x => x.desc == ItemSegmentConstants.YARN_PROGRAM.ToString()).ToList();
                data.YarnSubProgramList = itmeSegments.Where(x => x.desc == ItemSegmentConstants.YARN_SUB_PROGRAM.ToString()).ToList();
                data.ManufacturingLineList = itmeSegments.Where(x => x.desc == ItemSegmentConstants.MANUFACTURING_LINE.ToString()).ToList();
                data.ManufacturingProcessList = itmeSegments.Where(x => x.desc == ItemSegmentConstants.MANUFACTURING_PROCESS.ToString()).ToList();
                data.ManufacturingSubProcessList = itmeSegments.Where(x => x.desc == ItemSegmentConstants.MANUFACTURING_SUB_PROCESS.ToString()).ToList();
                data.YarnCompositionList = itmeSegments.Where(x => x.desc == ItemSegmentConstants.YARN_COMPOSITION.ToString()).ToList();
                data.YarnColorList = itmeSegments.Where(x => x.desc == ItemSegmentConstants.YARN_COLOR.ToString()).ToList();
                data.ColorGradeList = itmeSegments.Where(x => x.desc == ItemSegmentConstants.COLOR_GRADE.ToString()).ToList();

                data.CompanyList = await records.ReadAsync<Select2OptionModel>();
                data.TypeOfLcList = await records.ReadAsync<Select2OptionModel>();
                data.SupplierList = await records.ReadAsync<Select2OptionModel>();

                var ignoreValidationPO = await records.ReadAsync<YarnPOMaster>();
                if (ignoreValidationPO != null && ignoreValidationPO.Count() > 0)
                {
                    data.IgnoreValidationPOIds = ignoreValidationPO.Select(x => x.YPOMasterID).ToList();
                }
                data.ShadeList = await records.ReadAsync<Select2OptionModel>();
                var exportOrders = await records.ReadAsync<YarnPOChildOrder>();
                var exportBuyers = await records.ReadAsync<YarnPOChildBuyer>();
                data.BaseTypes = await records.ReadAsync<Select2OptionModel>();

                data.DayValidDurations = await records.ReadAsync<Select2OptionModel>();
                data.DayValidDurations = CommonFunction.GetDayValidDurations(data.DayValidDurations, string.Join(",", data.YarnPOChilds.Where(x => x.DayValidDurationId > 0).Select(x => x.DayValidDurationId).Distinct()));
                data.IsCheckDVD = data.PoDate < CommonConstent.YarnSourcingModeImplementDate ? false : true;

                data.YarnPOChilds.ForEach(x =>
                {
                    List<YarnPOChildOrder> tempList = exportOrders.Where(y => y.YPOChildID == x.YPOChildID).ToList();
                    x.YarnChildPoExportIds = string.Join(",", tempList.Select(y => y.ExportOrderId).Distinct());
                    x.YarnChildPoEWOs = string.Join(",", tempList.Select(y => y.EWONo).Distinct());

                    List<YarnPOChildBuyer> tempListB = exportBuyers.Where(y => y.YPOChildID == x.YPOChildID).ToList();
                    x.YarnChildPoBuyerIds = string.Join(",", tempListB.Select(y => y.BuyerId).Distinct());
                    x.BuyerNames = string.Join(",", tempListB.Select(y => y.BuyerName).Distinct());

                    var dayObj = data.DayValidDurations.ToList().Find(d => d.id == x.DayValidDurationId.ToString());
                    if (dayObj.IsNotNull())
                    {
                        x.DayValidDurationName = dayObj.text;
                        x.DayDuration = Convert.ToInt32(dayObj.additionalValue);
                    }

                    var dayObj_PR = data.DayValidDurations.ToList().Find(d => d.id == x.DayValidDurationIdPR.ToString());
                    if (dayObj_PR.IsNotNull())
                    {
                        x.DayValidDurationIdPRName = dayObj_PR.text;
                        x.DayDurationPR = Convert.ToInt32(dayObj_PR.additionalValue);
                    }
                });

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
        public async Task<List<YarnPOChild>> GetPRChildList(Status status, PaginationInfo paginationInfo, string childIDs, int CompanyId)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By PRChildID Desc" : paginationInfo.OrderBy;

            string sql = $@"WITH CHILD AS (
            SELECT M.YarnPRMasterID, M.YarnPRNo, C.YarnPRChildID, C.ItemMasterID, C.ReqQty, M.TriggerPointID
            FROM YarnPRChild C
            INNER JOIN YarnPRMaster M ON M.YarnPRMasterID = C.YarnPRMasterID
            WHERE M.IsFPR = 1 AND C.FPRCompanyID = {CompanyId} AND C.YarnPRChildID Not In(Select PRChildID From YarnPOChild) AND C.YarnPRChildID Not In({childIDs})
            )
            SELECT CHILD.YarnPRMasterID PRMasterID, CHILD.YarnPRNo, CHILD.YarnPRChildID PRChildID, CHILD.ItemMasterID, CHILD.ReqQty, PO.ValueName PoFor, CHILD.TriggerPointID PoForId,
            ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc,
            ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc, ISV8.SegmentValue Segment8ValueDesc,
            Count(*) Over() TotalRows
            From CHILD
            INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = CHILD.ItemMasterID
            LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
            LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
            LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
            LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
            LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
            LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
            LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
            LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID
            LEFT JOIN {DbNames.EPYSL}..EntityTypeValue PO ON PO.ValueID = CHILD.TriggerPointID";

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryAsync<YarnPOChild>(sql);
                return records.ToList();
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

        public async Task<List<YarnPOMaster>> GetYarnPOListsShowInPIReceive(int SupplierId, PaginationInfo paginationInfo)
        {
            var sql = string.Empty;

            sql += $@"With YPL AS(
                        SELECT		YPM.YPOMasterID, YPM.PONo, CE.ShortName AS CompanyName, PODate, Contacts.Name SupplierName, YPM.QuotationRefNo, YPF.ValueName POFor, YPM.DeliveryStartDate, YPM.DeliveryEndDate,
			                        Proposed, UnApprove, Approved, SUM(YPC.POQty) TotalQty, SUM(YPC.PIValue) TotalValue,
									CAINFO.SFToPLDays, CAINFO.PLToPDDays, CAINFO.PDToCFDays PCFDays, YPM.ApprovedDate,
									(CAINFO.SFToPLDays + CAINFO.PLToPDDays + CAINFO.PDToCFDays) AS InHouseDays, LU.UserName,
                                    YPM.POForID PIForID, YPM.CurrencyID, YPM.Remarks, YPM.InternalNotes, YPM.IncoTermsID,
									YPM.PaymentTermsID, YPM.TypeOfLCID, YPM.TenureofLC, YPM.CalculationofTenure, YPM.CreditDays,
									YPM.OfferValidity, YPM.ReImbursementCurrencyID, YPM.Charges, YPM.CountryOfOriginID,
									YPM.TransShipmentAllow, YPM.ShippingTolerance, YPM.PortofLoadingID, YPM.PortofDischargeID,
									YPM.ShipmentModeID
                        FROM		{DbNames.EPYSL}..Contacts
                        INNER JOIN	{DbNames.EPYSL}..ContactCategoryChild ON Contacts.ContactID = ContactCategoryChild.ContactID
                        INNER JOIN	{DbNames.EPYSL}..ContactCategoryHK ON ContactCategoryChild.ContactCategoryID = ContactCategoryHK.ContactCategoryID
                        INNER JOIN	{DbNames.EPYSL}..ContactAdditionalInfo CAINFO ON CAINFO.ContactID = Contacts.ContactID
						INNER JOIN	YarnPOMaster YPM ON YPM.SupplierID = Contacts.ContactID
						INNER JOIN  YarnPOChild YPC ON YPC.YPOMasterID = YPM.YPOMasterID
                        LEFT JOIN	YarnPIReceiveChild YPIRC ON YPM.YPOMasterID = YPIRC.YPOMasterID
                        INNER JOIN	{DbNames.EPYSL}..EntityTypeValue YPF ON YPF.ValueID = YPM.POForID
                        INNER JOIN	{DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = YPM.CompanyID
                        INNER JOIN	{DbNames.EPYSL}..LoginUser LU ON LU.UserCode = YPM.AddedBy
						WHERE		Proposed = 1 AND Approved = 1 AND UnApprove = 0 AND SupplierID = {SupplierId} AND YPM.YPOMasterID NOT IN(SELECT YPOMasterID FROM YarnPIReceiveChild)
						GROUP BY	YPM.YPOMasterID, CE.ShortName, YPM.PONo, PODate, Contacts.Name, YPM.QuotationRefNo, YPF.POFor, YPM.DeliveryStartDate, YPM.DeliveryEndDate,
			                        Proposed, UnApprove, Approved, CAINFO.SFToPLDays, CAINFO.PLToPDDays, CAINFO.PDToCFDays, YPM.ApprovedDate,
									(CAINFO.SFToPLDays + CAINFO.PLToPDDays + CAINFO.PDToCFDays), LU.UserName,
                                    YPM.POForID, YPM.CurrencyID, YPM.Remarks, YPM.InternalNotes, YPM.IncoTermsID,
									YPM.PaymentTermsID, YPM.TypeOfLCID, YPM.TenureofLC, YPM.CalculationofTenure, YPM.CreditDays,
									YPM.OfferValidity, YPM.ReImbursementCurrencyID, YPM.Charges, YPM.CountryOfOriginID,
									YPM.TransShipmentAllow, YPM.ShippingTolerance, YPM.PortofLoadingID, YPM.PortofDischargeID,
									YPM.ShipmentModeID
                        )
                        SELECT		YPOMasterID, PONo, CompanyName, PODate, SupplierName, QuotationRefNo, POFor, DeliveryStartDate, DeliveryEndDate,
			                        Count(*) Over() TotalRows, CAST(TotalQty AS DECIMAL(18,2)) TotalQty, CAST(TotalValue AS DECIMAL(18,2)) TotalValue,
									ApprovedDate, Approved, UserName,
									SFToPLDays, PLToPDDays, PCFDays, InHouseDays,
									DATEADD(day, SFToPLDays, DeliveryStartDate) AS SFToPLDate,
							        DATEADD(day, PLToPDDays, DATEADD(day, SFToPLDays, DeliveryStartDate)) AS PLToPDDate,
							        DATEADD(day, PCFDays, DATEADD(day, PLToPDDays, DATEADD(day, SFToPLDays, DeliveryStartDate))) AS PCFDate,
							        DATEADD(day, (SFToPLDays +  PLToPDDays + PCFDays), DeliveryStartDate) AS InHouseDate,
                                    PIForID, CurrencyID, Remarks, InternalNotes, IncoTermsID,
									PaymentTermsID, TypeOfLCID, TenureofLC, CalculationofTenure, CreditDays,
									OfferValidity, ReImbursementCurrencyID, Charges, CountryOfOriginID,
									TransShipmentAllow, ShippingTolerance, PortofLoadingID PortofLoadingID, PortofDischargeID PortofDischargeID,
									ShipmentModeID
                        FROM		YPL";

            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YPOMasterID DESC" : paginationInfo.OrderBy;
            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryAsync<YarnPOMaster>(sql);
                return records.ToList();
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

        public async Task SaveAsync(YarnPOMaster entity, List<YarnPOChild> yarnPoChilds, int userId)
        {
            SqlTransaction transaction = null;            
            SqlTransaction transactionGmt = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _gmtConnection.OpenAsync();
                transactionGmt = _gmtConnection.BeginTransaction();

                if (entity.IsRevision)
                {
                    //only for revision after reject
                    await _connection.ExecuteAsync("spBackupYarnPOMaster_Full", new { PONo = entity.PoNo, UserId = entity.UpdatedBy }, transaction, 30, CommandType.StoredProcedure);
                    //end only for revision after reject
                }
                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity = await AddAsync(entity, yarnPoChilds, transaction, _connection, transactionGmt, _gmtConnection);
                        break;

                    case EntityState.Modified:
                        entity = await UpdateAsync(entity, yarnPoChilds, transaction, _connection, transactionGmt, _gmtConnection);
                        break;

                    default:
                        break;
                }

                await _service.SaveSingleAsync(entity, transaction);

                List<YarnPOChildBuyer> buyerList = new List<YarnPOChildBuyer>();
                List<YarnPOChildOrder> orderList = new List<YarnPOChildOrder>();

                List<YarnPOChildBuyer> buyerDeleteList = new List<YarnPOChildBuyer>();
                List<YarnPOChildOrder> orderDeleteList = new List<YarnPOChildOrder>();


                entity.YarnPOChilds.ForEach(x =>
                {
                    var buyers = x.YarnPOChildBuyers.Where(y => y.EntityState != EntityState.Deleted).ToList();
                    buyerList.AddRange(buyers);

                    var buyersD = x.YarnPOChildBuyers.Where(y => y.EntityState == EntityState.Deleted).ToList();
                    buyerDeleteList.AddRange(buyersD);

                    var orders = x.YarnPOChildOrders.Where(y => y.EntityState != EntityState.Deleted).ToList();
                    orderList.AddRange(orders);

                    var ordersD = x.YarnPOChildOrders.Where(y => y.EntityState == EntityState.Deleted).ToList();
                    orderDeleteList.AddRange(ordersD);
                });

                var childs = entity.YarnPOChilds.Where(x => (x.Segment5ValueDesc.ToLower() == "melange" || x.Segment5ValueDesc.ToLower() == "color melange") && (x.ShadeCode == "" || x.ShadeCode == null)).ToList();
                if (childs.Count() > 0)
                {
                    throw new Exception("Shade Code missing for melange / color melange => SaveAsync => YarnPOV2Service");
                }
                int count = entity.YarnPOChilds.Count(x => x.PoForId == 0);
                if (count > 0)
                {
                    throw new Exception("Every item must have purchase for.");
                }

                await _service.SaveAsync(buyerDeleteList, transaction);
                await _service.SaveAsync(orderDeleteList, transaction);
                entity.YarnPOChilds.ForEach(x =>
                {
                    x.PIValue = x.PoQty * x.Rate;
                });
                await _service.SaveAsync(entity.YarnPOChilds, transaction);
                foreach (YarnPOChild item in entity.YarnPOChilds)
                {
                     _service.ExecuteWithTransactionAsync(SPNames.sp_Validation_YarnPOChild, ref transaction,new
                    {
                        EntityState = item.EntityState,
                        UserId = userId,
                        PrimaryKeyId = item.YPOChildID

                    }, 30, CommandType.StoredProcedure);
                  
                }
                await _service.SaveAsync(buyerList, transaction);
                await _service.SaveAsync(orderList, transaction);
                foreach (YarnPOChildOrder item in orderList)
                {
                     _service.ExecuteWithTransactionAsync(SPNames.sp_Validation_YarnPOChildOrder, ref transaction,new
                    {
                        EntityState = item.EntityState,
                        UserId = userId,
                        PrimaryKeyId = item.YPOChildOrderID
                 
                    }, 30, CommandType.StoredProcedure);
              
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
                _gmtConnection.Close();
            }
        }

        public async Task SaveAsyncRevision(YarnPOMaster entity, List<YarnPOChild> yarnPoChilds, string PONo, int userId)
        {
            SqlTransaction transaction = null;
            SqlTransaction transactionGmt = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _gmtConnection.OpenAsync();
                transactionGmt = _gmtConnection.BeginTransaction();

                //only for revision after reject
                await _connection.ExecuteAsync("spBackupYarnPOMaster_Full", new { PONo = PONo, UserId = userId }, transaction, 30, CommandType.StoredProcedure);
                //end only for revision after reject
                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity = await AddAsync(entity, yarnPoChilds, transaction, _connection, transactionGmt, _gmtConnection);
                        break;

                    case EntityState.Modified:
                        entity = await UpdateAsync(entity, yarnPoChilds, transaction, _connection, transactionGmt, _gmtConnection);
                        break;

                    default:
                        break;
                }

                var childs = entity.YarnPOChilds.Where(x => (x.Segment5ValueDesc.ToLower() == "melange" || x.Segment5ValueDesc.ToLower() == "color melange") && (x.ShadeCode == "" || x.ShadeCode == null)).ToList();
                if (childs.Count() > 0)
                {
                    throw new Exception("Shade Code missing for melange / color melange => SaveAsyncRevision => YarnPOV2Service");
                }
                int count = entity.YarnPOChilds.Count(x => x.PoForId == 0);
                if (count > 0)
                {
                    throw new Exception("Every item must have purchase for.");
                }

                await _service.SaveSingleAsync(entity, transaction);

                entity.YarnPOChilds.ForEach(x =>
                {
                    x.PIValue = x.PoQty * x.Rate;
                });
                await _service.SaveAsync(entity.YarnPOChilds, transaction);
                foreach (YarnPOChild item in entity.YarnPOChilds)
                {
                   
                    await _service.ExecuteAsync("sp_Validation_YarnPOChild", new
                    {
                        EntityState = item.EntityState,
                        UserId = userId,
                        PrimaryKeyId = item.YPOChildID

                    }, 30, CommandType.StoredProcedure);
                }
                List<YarnPOChildBuyer> buyerList = new List<YarnPOChildBuyer>();
                List<YarnPOChildOrder> orderList = new List<YarnPOChildOrder>();
                entity.YarnPOChilds.ForEach(x =>
                {
                    buyerList.AddRange(x.YarnPOChildBuyers);
                    orderList.AddRange(x.YarnPOChildOrders);
                });
                await _service.SaveAsync(buyerList, transaction);
                await _service.SaveAsync(orderList, transaction);
                foreach (YarnPOChildOrder item in orderList)
                {
                    await _service.ExecuteAsync("sp_Validation_YarnPOChildOrder", new
                    {
                        EntityState = item.EntityState,
                        UserId = userId,
                        PrimaryKeyId = item.YPOChildOrderID

                    }, 30, CommandType.StoredProcedure);
                   
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

        #region Helpers
        private async Task<YarnPOMaster> AddAsync(YarnPOMaster entity, List<YarnPOChild> yarnPoChilds,SqlTransaction transaction, SqlConnection _connection, SqlTransaction transactionGmt, SqlConnection _gmtConnection)
        {
            

            entity.YPOMasterID = await _service.GetMaxIdAsync(TableNames.YarnPOMaster, RepeatAfterEnum.NoRepeat, transactionGmt, _gmtConnection);
            entity.PoNo = await _service.GetMaxNoAsync(TableNames.YPONo, entity.CompanyId, RepeatAfterEnum.EveryYear,"00000", transactionGmt, _gmtConnection);
            int maxYarnPOChildId =await  _service.GetMaxIdAsync(TableNames.YarnPOChild, yarnPoChilds.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _gmtConnection);

            yarnPoChilds.ForEach(x => x.YarnChildPoBuyerIdArray = Array.ConvertAll(x.YarnChildPoBuyerIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries), int.Parse));
            yarnPoChilds.ForEach(x => x.YarnChildPoBuyerIdArray.ToList().RemoveAll(y => y == 0));
            var yarnPoChildPoForBuyerCount = yarnPoChilds.Sum(x => x.YarnChildPoBuyerIdArray.Count());
            var maxYarnPOChildPOForBuyerId = await _service.GetMaxIdAsync(TableNames.YarnPOChildBuyer, yarnPoChildPoForBuyerCount, RepeatAfterEnum.NoRepeat, transactionGmt, _gmtConnection);

            yarnPoChilds.ForEach(x => x.YarnChildPoExportIdArray = Array.ConvertAll(x.YarnChildPoExportIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries), int.Parse));
            var yarnPoChildPoForExportOrderCount = yarnPoChilds.Sum(x => x.YarnChildPoExportIdArray.Count());
            var maxYarnPOChildPOForExportOrderId = await _service.GetMaxIdAsync(TableNames.YarnPOChildOrder, yarnPoChildPoForExportOrderCount, RepeatAfterEnum.NoRepeat, transactionGmt, _gmtConnection);
            entity.YarnPOChilds = new List<YarnPOChild>();

            foreach (var item in yarnPoChilds)
            {
                YarnPOChild yarnPOChildEntity = new YarnPOChild
                {
                    YPOChildID = maxYarnPOChildId++,
                    YPOMasterID = entity.YPOMasterID,
                    ItemMasterID = item.ItemMasterID,
                    PRMasterID = item.PRMasterID,
                    PRChildID = item.PRChildID,
                    YarnCategory = item.YarnCategory,
                    UnitID = item.UnitID,
                    BuyerID = item.BuyerID,
                    PoQty = item.PoQty,
                    Rate = item.Rate,
                    PIValue = item.PIValue,
                    Remarks = item.Remarks,
                    YarnLotNo = item.YarnLotNo,
                    HSCode = item.HSCode,
                    QuotationRefNo = item.QuotationRefNo,
                    QuotationRefDate = item.QuotationRefDate,
                    BookingNo = item.BookingNo,
                    YarnShade = item.Segment4ValueDesc,
                    YarnProgramId = item.YarnProgramId,
                    NoOfThread = item.NoOfThread,
                    PoForId = item.PoForId,
                    ConceptID = item.ConceptID,
                    ShadeCode = item.ShadeCode,
                    POCone = item.POCone,
                    Segment5ValueDesc = item.Segment5ValueDesc,
                    Segment5ValueId = item.Segment5ValueId,

                    DayValidDurationId = item.DayValidDurationId,
                    EntityState = EntityState.Added
                };

                foreach (int buyerId in item.YarnChildPoBuyerIdArray)
                {
                    if (buyerId > 0)
                    {
                        YarnPOChildBuyer yarnPoChildPoBuyerEntity = new YarnPOChildBuyer
                        {
                            YPOChildBuyerID = maxYarnPOChildPOForBuyerId++,
                            BuyerId = buyerId,
                            YPOMasterID = entity.YPOMasterID,
                            YPOChildID = yarnPOChildEntity.YPOChildID, //maxYarnPOChildId,//item.YPOChildID,
                            EntityState = EntityState.Added
                        };
                        yarnPOChildEntity.YarnPOChildBuyers.Add(yarnPoChildPoBuyerEntity);
                    }
                }

                List<YarnPOChildOrder> orders = new List<YarnPOChildOrder>();
                yarnPoChilds.ForEach(x =>
                {
                    orders.AddRange(x.YarnPOChildOrders);
                });

                foreach (int exportOrderId in item.YarnChildPoExportIdArray)
                {
                    if (exportOrderId > 0)
                    {
                        YarnPOChildOrder obj = orders.Find(x => x.ExportOrderId == exportOrderId);
                        YarnPOChildOrder yarnPoChildPoExportOrderEntity = new YarnPOChildOrder
                        {
                            YPOChildOrderID = maxYarnPOChildPOForExportOrderId++,
                            ExportOrderId = exportOrderId,
                            EWONo = obj != null ? obj.EWONo : "",
                            YPOMasterID = entity.YPOMasterID,
                            YPOChildID = yarnPOChildEntity.YPOChildID,//maxYarnPOChildId,//item.YPOChildID,
                            EntityState = EntityState.Added
                        };
                        yarnPOChildEntity.YarnPOChildOrders.Add(yarnPoChildPoExportOrderEntity);
                    }
                }

                entity.YarnPOChilds.Add(yarnPOChildEntity);
            }

            return entity;
        }

    

        private async Task<YarnPOMaster> UpdateAsync(YarnPOMaster entity, List<YarnPOChild> yarnPoChilds, SqlTransaction transaction, SqlConnection _connection, SqlTransaction transactionGmt, SqlConnection _gmtConnection)
        {
            #region Add/Update/Delete YarnPOchild and YarnPOChildSubProgram

            // Get Max YarnPOChildId
            var newYarnPOChilds = yarnPoChilds.FindAll(x => x.YPOMasterID == 0 && x.EntityState == EntityState.Added);
            var maxYarnPOChildId = await _service.GetMaxIdAsync(TableNames.YarnPOChild, newYarnPOChilds.Count(), RepeatAfterEnum.NoRepeat, transactionGmt, _gmtConnection);

            // Get Max YarnPOChild
            yarnPoChilds.ForEach(x => x.YarnChildPoBuyerIdArray = Array.ConvertAll(x.YarnChildPoBuyerIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries), int.Parse));
            yarnPoChilds.ForEach(x => x.YarnChildPoBuyerIdArray.ToList().RemoveAll(y => y == 0));
            var yarnPoChildBuyerCount = yarnPoChilds.Sum(x => x.YarnChildPoBuyerIdArray.Count());
            yarnPoChildBuyerCount += entity.YarnPOChilds.Sum(x => x.YarnPOChildBuyers.Where(y => y.EntityState == EntityState.Added).Count());
            var maxYarnPOChildBuyerId = await _service.GetMaxIdAsync(TableNames.YarnPOChildBuyer, yarnPoChildBuyerCount, RepeatAfterEnum.NoRepeat, transactionGmt, _gmtConnection);

            // Get Max YarnPOChildPOForExportOrderId
            yarnPoChilds.ForEach(x => x.YarnChildPoExportIdArray = Array.ConvertAll(x.YarnChildPoExportIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries), int.Parse));
            var yarnPoChildOrderCount = yarnPoChilds.Sum(x => x.YarnChildPoExportIdArray.Count());
            yarnPoChildOrderCount += entity.YarnPOChilds.Sum(x => x.YarnPOChildOrders.Where(y => y.EntityState == EntityState.Added).Count());
            var maxYarnPOChildOrderId = await _service.GetMaxIdAsync(TableNames.YarnPOChildOrder, yarnPoChildOrderCount, RepeatAfterEnum.NoRepeat, transactionGmt, _gmtConnection);

            #region Add new YarnPOChilds

            foreach (var item in newYarnPOChilds)
            {
                item.YPOChildID = maxYarnPOChildId++;
                item.YPOMasterID = entity.YPOMasterID;
                item.EntityState = EntityState.Added;

                item.YarnCategory = CommonFunction.GetYarnShortForm(item.Segment1ValueDesc, item.Segment2ValueDesc, item.Segment3ValueDesc, item.Segment4ValueDesc, item.Segment5ValueDesc, item.Segment6ValueDesc, item.ShadeCode);

                foreach (var buyerId in item.YarnChildPoBuyerIdArray)
                {
                    if (buyerId > 0)
                    {
                        var yarnPoChildPoBuyerEntity = new YarnPOChildBuyer
                        {
                            YPOChildBuyerID = maxYarnPOChildBuyerId++,
                            BuyerId = buyerId,
                            YPOMasterID = entity.YPOMasterID,
                            YPOChildID = item.YPOChildID// item.YPOChildID
                        };
                        item.YarnPOChildBuyers.Add(yarnPoChildPoBuyerEntity);
                    }
                }

                item.YarnPOChildOrders = new List<YarnPOChildOrder>();
                foreach (var exportOrderId in item.YarnChildPoExportIdArray)
                {
                    if (exportOrderId > 0)
                    {
                        var yarnPoChildPoExportOrderEntity = new YarnPOChildOrder
                        {
                            YPOChildOrderID = maxYarnPOChildOrderId++,
                            ExportOrderId = exportOrderId,
                            YPOMasterID = entity.YPOMasterID,
                            YPOChildID = item.YPOChildID //item.YPOChildID
                        };
                        item.YarnPOChildOrders.Add(yarnPoChildPoExportOrderEntity);
                    }
                }

                entity.YarnPOChilds.Add(item);
            }

            #endregion Add new YarnPOChilds

            foreach (var item in entity.YarnPOChilds.Where(x => x.EntityState != EntityState.Added).ToList())
            {
                if (item.EntityState == EntityState.Modified)
                {
                    var yarnPOChildRef = yarnPoChilds.Find(x => x.YPOChildID == item.YPOChildID);
                    if (yarnPOChildRef == null)
                        throw new Exception("Yarn PO Child not found.");

                    item.ItemMasterID = yarnPOChildRef.ItemMasterID;

                    #region Add/Update/Delete YarnPOChildYarnSubProgram

                    //foreach (var yarnPoChildYarnSubProgramEntity in item.YarnPOChildYarnSubPrograms.ToList())
                    //{
                    //    if(yarnPoChildYarnSubProgramEntity.YarnSubProgramId > 0)
                    //    {
                    //        switch (yarnPoChildYarnSubProgramEntity.EntityState)
                    //        {
                    //            case EntityState.Added:
                    //                yarnPoChildYarnSubProgramEntity.Id = maxYarnPOChildYarnSubProgramId++;
                    //                _dbSetYarnPOChildYarnSubProgram.Add(yarnPoChildYarnSubProgramEntity);
                    //                break;
                    //            case EntityState.Deleted:
                    //                _dbSetYarnPOChildYarnSubProgram.Remove(yarnPoChildYarnSubProgramEntity);
                    //                break;
                    //            default:
                    //                break;
                    //        }
                    //    }
                    //}

                    #endregion Add/Update/Delete YarnPOChildYarnSubProgram

                    #region Add/Update/Delete YarnPOChildBuyers

                    foreach (var yarnPoChildBuyerEntity in item.YarnPOChildBuyers.ToList())
                    {
                        switch (yarnPoChildBuyerEntity.EntityState)
                        {
                            case EntityState.Added:
                                yarnPoChildBuyerEntity.YPOChildBuyerID = maxYarnPOChildBuyerId++;
                                yarnPoChildBuyerEntity.EntityState = EntityState.Added;
                                break;

                            case EntityState.Deleted:
                                yarnPoChildBuyerEntity.EntityState = EntityState.Deleted;
                                break;

                            default:
                                break;
                        }
                    }

                    #endregion Add/Update/Delete YarnPOChildBuyers

                    #region Add/Update/Delete YarnPOChildYarnSubProgram

                    List<YarnPOChildOrder> orders = new List<YarnPOChildOrder>();
                    yarnPoChilds.ForEach(x =>
                    {
                        orders.AddRange(x.YarnPOChildOrders);
                    });

                    foreach (var yarnPoChildOrderEntity in item.YarnPOChildOrders.ToList())
                    {
                        switch (yarnPoChildOrderEntity.EntityState)
                        {
                            case EntityState.Added:
                                YarnPOChildOrder obj = orders.Find(x => x.ExportOrderId == yarnPoChildOrderEntity.ExportOrderId);

                                yarnPoChildOrderEntity.YPOChildOrderID = maxYarnPOChildOrderId++;
                                yarnPoChildOrderEntity.EWONo = obj != null ? obj.EWONo : "";
                                yarnPoChildOrderEntity.EntityState = EntityState.Added;
                                break;

                            case EntityState.Deleted:
                                yarnPoChildOrderEntity.EntityState = EntityState.Deleted;
                                break;

                            default:
                                break;
                        }
                    }

                    #endregion Add/Update/Delete YarnPOChildYarnSubProgram
                }
                else if (item.EntityState == EntityState.Deleted)
                {
                    item.YarnPOChildBuyers.SetDeleted();
                    item.YarnPOChildOrders.SetDeleted();
                }
            }

            #endregion Add/Update/Delete YarnPOchild and YarnPOChildSubProgram

            return entity;
        }

        #endregion Helpers

        public async Task<YarnPOMaster> GetAllByIDAsync(int id)
        {
            string sql = $@"
            ;Select * From YarnPOMaster Where YPOMasterID = {id}

            ;Select POC.*, 
            Segment1ValueDesc = ISV1.SegmentValue,
            Segment2ValueDesc = ISV2.SegmentValue,
            Segment3ValueDesc = ISV3.SegmentValue,
            Segment4ValueDesc = ISV4.SegmentValue,
            Segment5ValueDesc = ISV5.SegmentValue,
            Segment6ValueDesc = ISV6.SegmentValue,
            Segment7ValueDesc = ISV7.SegmentValue
            From YarnPOChild POC
            INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = POC.ItemMasterID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 ON ISV1.SegmentValueID = IM.Segment1ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
            LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
            Where YPOMasterID = {id}

            ;--Select * From YarnPOChildYarnSubProgram Where YPOMasterID = {id}

            ;Select * From YarnPOChildBuyer Where YPOMasterID = {id}

            ;Select * From YarnPOChildOrder Where YPOMasterID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);

                YarnPOMaster data = await records.ReadFirstOrDefaultAsync<YarnPOMaster>();
                Guard.Against.NullObject(data);

                data.YarnPOChilds = records.Read<YarnPOChild>().ToList();
                var buyers = records.Read<YarnPOChildBuyer>().ToList();
                var orders = records.Read<YarnPOChildOrder>().ToList();

                foreach (YarnPOChild child in data.YarnPOChilds)
                {
                    child.YarnPOChildBuyers = buyers.FindAll(x => x.YPOChildID == child.YPOChildID);
                    child.YarnPOChildOrders = orders.FindAll(x => x.YPOChildID == child.YPOChildID);
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

        public async Task UpdateEntityAsync(YarnPOMaster entity)
        {
            SqlTransaction transaction = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                await _service.SaveSingleAsync(entity, transaction);

                #region Stock Operation
                if (entity.Approved)
                {
                    await _connection.ExecuteAsync("spYarnStockOperation", new { MasterID = entity.YPOMasterID, FromMenuType = EnumFromMenuType.PO, UserId = entity.ApprovedBy }, transaction, 30, CommandType.StoredProcedure);
                }
                #endregion Stock Operation

                transaction.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                if (ex.Message.Contains('~')) throw new Exception(ex.Message.Split('~')[0]);
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }


    }
}