using Dapper;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Booking;
using EPYSLTEXCore.Infrastructure.Entities.Tex.General.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using System.Data;
using System.Data.Entity;
using Microsoft.Data.SqlClient;
using System.Transactions;

namespace EPYSLTEXCore.Application.Services.Inventory
{
    public class YDLeftOverReturnService : IYDLeftOverReturnService
    {
        private readonly IDapperCRUDService<YDLeftOverReturnMaster> _service;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;

        public YDLeftOverReturnService(IDapperCRUDService<YDLeftOverReturnMaster> service)
        {
            _service = service;
            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
        }

        public async Task<List<YDLeftOverReturnMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By RNDReturnMasterID Desc" : paginationInfo.OrderBy;
            var sql = "";
            switch (status)
            {
                case Status.Draft:
                    sql = $@";With KIC AS(
                                    Select KRC.YDLOReturnMasterID,
                                    BookingNo =STRING_AGG(YDBM.GroupConceptNo,','), YDReqIssueNo = STRING_AGG(KIM.YDReqIssueNo,','),
									ProgramName = case when isnull(FCM.BookingID,0)>0 then 'RND' when isnull(YBM.BookingID,0)>0 then 'Bulk'  else case when YDBC.ProgramName='BULK' then 'Bulk' else 'RND' end end
                                    From {TableNames.YD_Left_Over_Return_CHILD} KRC
                                    Inner JOIN {TableNames.YD_REQ_ISSUE_CHILD} KIC On KIC.YDReqIssueChildID = KRC.YDReqIssueChildID
                                    Inner JOIN {TableNames.YD_REQ_ISSUE_MASTER} KIM On KIM.YDReqIssueMasterID = KIC.YDReqIssueMasterID
                                    Left JOIN {TableNames.YD_REQ_CHILD} RC On RC.YDReqChildID = KIC.YDReqChildID
                                    Left JOIN {TableNames.YD_REQ_MASTER} RM On RM.YDReqMasterID = RC.YDReqMasterID
									Left JOIN {TableNames.YDBookingChild} YDBC On YDBC.YDBookingChildID = RC.YDBookingChildID
									Left JOIN {TableNames.YD_BOOKING_MASTER} YDBM On YDBM.YDBookingMasterID = YDBC.YDBookingMasterID
									LEFT JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCMRC ON FCMRC.FCMRChildID = YDBC.FCMRChildID
									LEFT JOIN {TableNames.FreeConceptMRMaster} FCMRM ON FCMRM.FCMRMasterID = FCMRC.FCMRMasterID
									LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = FCMRM.ConceptID
									LEFT JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID=YDBC.YBChildItemID
									LEFT JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID=YBCI.YBChildID 
									LEFT JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID=YBC.YBookingID 
                                    Group By KRC.YDLOReturnMasterID, case when isnull(FCM.BookingID,0)>0 then 'RND' when isnull(YBM.BookingID,0)>0 then 'Bulk'  else case when YDBC.ProgramName='BULK' then 'Bulk' else 'RND' end end
                                ),
                                FinalFilter As(
                                    Select KRM.YDLOReturnMasterID,KRM.YDLOReturnNo,KRM.YDLOReturnDate ,KIC.BookingNo,KIC.YDReqIssueNo, KIC.ProgramName
                                    FROM {TableNames.YD_Left_Over_Return_MASTER} KRM
                                    Inner Join KIC On KIC.YDLOReturnMasterID = KRM.YDLOReturnMasterID
                                    Where KRM.IsSendToMCD = 0 And KRM.IsApprove=0
                                )
						        Select *, Count(*) Over() TotalRows from FinalFilter ";
                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YDLOReturnMasterID Desc" : paginationInfo.OrderBy;
                    break;
                case Status.Proposed:
                    sql = $@";With KIC AS(
                                    Select KRC.YDLOReturnMasterID,
                                    BookingNo =STRING_AGG(YDBM.GroupConceptNo,','), YDReqIssueNo = STRING_AGG(KIM.YDReqIssueNo,','),
									ProgramName = case when isnull(FCM.BookingID,0)>0 then 'RND' when isnull(YBM.BookingID,0)>0 then 'Bulk'  else case when YDBC.ProgramName='BULK' then 'Bulk' else 'RND' end end
                                    From {TableNames.YD_Left_Over_Return_CHILD} KRC
                                    Inner JOIN {TableNames.YD_REQ_ISSUE_CHILD} KIC On KIC.YDReqIssueChildID = KRC.YDReqIssueChildID
                                    Inner JOIN {TableNames.YD_REQ_ISSUE_MASTER} KIM On KIM.YDReqIssueMasterID = KIC.YDReqIssueMasterID
                                    Left JOIN {TableNames.YD_REQ_CHILD} RC On RC.YDReqChildID = KIC.YDReqChildID
                                    Left JOIN {TableNames.YD_REQ_MASTER} RM On RM.YDReqMasterID = RC.YDReqMasterID
									Left JOIN {TableNames.YDBookingChild} YDBC On YDBC.YDBookingChildID = RC.YDBookingChildID
									Left JOIN {TableNames.YD_BOOKING_MASTER} YDBM On YDBM.YDBookingMasterID = YDBC.YDBookingMasterID
									LEFT JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCMRC ON FCMRC.FCMRChildID = YDBC.FCMRChildID
									LEFT JOIN {TableNames.FreeConceptMRMaster} FCMRM ON FCMRM.FCMRMasterID = FCMRC.FCMRMasterID
									LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = FCMRM.ConceptID
									LEFT JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID=YDBC.YBChildItemID
									LEFT JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID=YBCI.YBChildID 
									LEFT JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID=YBC.YBookingID 
                                    Group By KRC.YDLOReturnMasterID, case when isnull(FCM.BookingID,0)>0 then 'RND' when isnull(YBM.BookingID,0)>0 then 'Bulk'  else case when YDBC.ProgramName='BULK' then 'Bulk' else 'RND' end end
                                ),
                                FinalFilter As(
                                    Select KRM.YDLOReturnMasterID,KRM.YDLOReturnNo,KRM.YDLOReturnDate ,KIC.BookingNo,KIC.YDReqIssueNo, KIC.ProgramName
                                    FROM {TableNames.YD_Left_Over_Return_MASTER} KRM
                                    Inner Join KIC On KIC.YDLOReturnMasterID = KRM.YDLOReturnMasterID
                                    Where KRM.IsSendToMCD = 1 And KRM.IsApprove=0
                                )
						        Select *, Count(*) Over() TotalRows from FinalFilter";
                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YDLOReturnMasterID Desc" : paginationInfo.OrderBy;
                    break;
                case Status.Approved:
                    sql = $@";With KIC AS(
                                    Select KRC.YDLOReturnMasterID,
                                    BookingNo =STRING_AGG(YDBM.GroupConceptNo,','), YDReqIssueNo = STRING_AGG(KIM.YDReqIssueNo,','),
									ProgramName = case when isnull(FCM.BookingID,0)>0 then 'RND' when isnull(YBM.BookingID,0)>0 then 'Bulk'  else case when YDBC.ProgramName='BULK' then 'Bulk' else 'RND' end end
                                    From {TableNames.YD_Left_Over_Return_CHILD} KRC
                                    Inner JOIN {TableNames.YD_REQ_ISSUE_CHILD} KIC On KIC.YDReqIssueChildID = KRC.YDReqIssueChildID
                                    Inner JOIN {TableNames.YD_REQ_ISSUE_MASTER} KIM On KIM.YDReqIssueMasterID = KIC.YDReqIssueMasterID
                                    Left JOIN {TableNames.YD_REQ_CHILD} RC On RC.YDReqChildID = KIC.YDReqChildID
                                    Left JOIN {TableNames.YD_REQ_MASTER} RM On RM.YDReqMasterID = RC.YDReqMasterID
									Left JOIN {TableNames.YDBookingChild} YDBC On YDBC.YDBookingChildID = RC.YDBookingChildID
									Left JOIN {TableNames.YD_BOOKING_MASTER} YDBM On YDBM.YDBookingMasterID = YDBC.YDBookingMasterID
									LEFT JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCMRC ON FCMRC.FCMRChildID = YDBC.FCMRChildID
									LEFT JOIN {TableNames.FreeConceptMRMaster} FCMRM ON FCMRM.FCMRMasterID = FCMRC.FCMRMasterID
									LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = FCMRM.ConceptID
									LEFT JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID=YDBC.YBChildItemID
									LEFT JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID=YBCI.YBChildID 
									LEFT JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID=YBC.YBookingID 
                                    Group By KRC.YDLOReturnMasterID, case when isnull(FCM.BookingID,0)>0 then 'RND' when isnull(YBM.BookingID,0)>0 then 'Bulk'  else case when YDBC.ProgramName='BULK' then 'Bulk' else 'RND' end end
                                ),
                                FinalFilter As(
                                    Select KRM.YDLOReturnMasterID,KRM.YDLOReturnNo,KRM.YDLOReturnDate ,KIC.BookingNo,KIC.YDReqIssueNo, KIC.ProgramName
                                    FROM {TableNames.YD_Left_Over_Return_MASTER} KRM
                                    Inner Join KIC On KIC.YDLOReturnMasterID = KRM.YDLOReturnMasterID
                                    Where KRM.IsSendToMCD = 1 And KRM.IsApprove=1
                                )
						        Select *, Count(*) Over() TotalRows from FinalFilter";
                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YDLOReturnMasterID Desc" : paginationInfo.OrderBy;
                    break;
                case Status.Acknowledge:

                    break;
                case Status.PendingReturnConfirmation:

                    break;
                default: //Status.Pending
                    sql = $@"; With RP As(
                            Select
                            KIC.YDReqIssueChildID,KIM.YDReqIssueMasterID,KIM.YDReqIssueNo, YarnDetails = IM.ItemName, KIC.PhysicalCount, YarnCount = ISV6.SegmentValue,KIC.LotNo,
                            BookingNo = STRING_AGG(YDBM.GroupConceptNo,','),'' BatchNo,
                            KIC.ItemMasterID, [Floor]=CE.ShortName,
                            ExportOrderNo = case when isnull(BM.BookingID,0)>0 then BM.ExportOrderNo when isnull(BBM.BookingID,0)>0 then BBM.ExportOrderNo  else '' end,
							ProgramName = case when isnull(FCM.BookingID,0)>0 then 'RND' when isnull(YBM.BookingID,0)>0 then 'Bulk'  else case when YDBC.ProgramName='BULK' then 'Bulk' else 'RND' end end,
							FCM.BookingID, BBMBookingID = YBM.BookingID, YDBC.YDBookingChildID,KIC.IssueQty ,
							ReturnQty = (ISnull(YLORC.UseableReturnQtyKG,0) + ISnull(YLORC.UnuseableReturnQtyKG,0))
                            FROM {TableNames.YD_REQ_ISSUE_CHILD} KIC
                            Inner JOIN {TableNames.YD_REQ_ISSUE_MASTER} KIM On KIM.YDReqIssueMasterID = KIC.YDReqIssueMasterID
                            Inner JOIN {TableNames.YD_RECEIVE_CHILD} YDRC On YDRC.YDReqIssueChildID = KIC.YDReqIssueChildID
                            Inner Join {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = KIC.ItemMasterID
                            Left Join {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                            Left JOIN {TableNames.YD_REQ_CHILD} RC On RC.YDReqChildID = KIC.YDReqChildID
                            Left JOIN {TableNames.YD_REQ_MASTER} RM On RM.YDReqMasterID = RC.YDReqMasterID
							LEFT JOIN {TableNames.YD_Left_Over_Return_CHILD} YLORC ON YLORC.YDReqIssueChildID = KIC.YDReqIssueChildID
							LEFT JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID=RM.CompanyID 
							LEFT JOIN {TableNames.YDBookingChild} YDBC ON YDBC.YDBookingChildID=RC.YDBookingChildID
							Left JOIN {TableNames.YD_BOOKING_MASTER} YDBM On YDBM.YDBookingMasterID = YDBC.YDBookingMasterID
                            LEFT JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCMRC ON FCMRC.FCMRChildID = YDBC.FCMRChildID
                            LEFT JOIN {TableNames.FreeConceptMRMaster} FCMRM ON FCMRM.FCMRMasterID = FCMRC.FCMRMasterID
							LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = FCMRM.ConceptID
							LEFT JOIN {DbNames.EPYSL}..BookingMaster BM ON BM.BookingID = FCM.BookingID
							--LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = FCM.BookingID
							--LEFT JOIN {DbNames.EPYSL}..ExportOrderMaster EOM ON EOM.ExportOrderID=SBM.ExportOrderID
                            LEFT JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID=YDBC.YBChildItemID
							LEFT JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID=YBCI.YBChildID 
							LEFT JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID=YBC.YBookingID 
							LEFT JOIN {DbNames.EPYSL}..BookingMaster BBM ON BBM.BookingID = YBM.BookingID
                            Left JOIN {TableNames.YD_Left_Over_Return_CHILD} KRC On KRC.YDReqIssueChildID = KIC.YDReqIssueChildID
                            Where KIM.IsApprove = 1 --And ISNULL(KRC.YDReqIssueChildID ,0) = 0
							GROUP BY KIC.YDReqIssueChildID,KIM.YDReqIssueMasterID,KIM.YDReqIssueNo, IM.ItemName, KIC.PhysicalCount, ISV6.SegmentValue,KIC.LotNo,
                            KIC.IssueQty,KIC.ItemMasterID,BM.BookingID,BM.ExportOrderNo,CE.ShortName,BBM.BookingID, BBM.ExportOrderNo, YDBC.FCMRChildID,YDBC.YBChildItemID,
							FCM.BookingID, YBM.BookingID, YDBC.YDBookingChildID, YDBC.ProgramName,KIC.IssueQty, ISnull(YLORC.UseableReturnQtyKG,0),ISnull(YLORC.UnuseableReturnQtyKG,0) 
                        ),
						Result As (
						Select YDReqIssueChildID, YDReqIssueMasterID, YDReqIssueNo, YarnDetails,  PhysicalCount, YarnCount, LotNo,
                            BookingNo, BatchNo, ItemMasterID, [Floor], ExportOrderNo, ProgramName, BookingID, BBMBookingID, YDBookingChildID, IssueQty ,
							BalanceQuantity = IssueQty - Sum(ReturnQty)
						FROM RP 
						GROUP BY YDReqIssueChildID, YDReqIssueMasterID, YDReqIssueNo, YarnDetails,  PhysicalCount, YarnCount, LotNo,
                            BookingNo, BatchNo, ItemMasterID, [Floor], ExportOrderNo, ProgramName, BookingID, BBMBookingID, YDBookingChildID, IssueQty 
							
						),
						FinalResult As (
						Select * FROM Result Where BalanceQuantity>0
						)
                        Select *, Count(*) Over() TotalRows from Result";
                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YDReqIssueChildID Desc" : paginationInfo.OrderBy;
                    break;
            }

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            var list = await _service.GetDataAsync<YDLeftOverReturnMaster>(sql);
            list.ForEach(x => {
                var issueNos = x.YDReqIssueNo.Split(',');
                x.YDReqIssueNo = string.Join(",", issueNos.Distinct());

                var bookingNos = x.BookingNo.Split(',');
                x.BookingNo = string.Join(",", bookingNos.Distinct());
            });
            return list;
        }

        public async Task<YDLeftOverReturnMaster> GetNewAsync(string YDReqIssueMasterID)
        {

            var query = $@"Select KIM.*,RCompanyID = RM.CompanyID,
		                    OCompanyID = RM.CompanyID, YDRM.YDBookingMasterID
		                    FROM {TableNames.YD_REQ_ISSUE_CHILD} KIC
		                    Inner JOIN {TableNames.YD_REQ_ISSUE_MASTER} KIM On KIM.YDReqIssueMasterID = KIC.YDReqIssueMasterID
							Inner JOIN {TableNames.YD_REQ_MASTER} YDRM On YDRM.YDReqMasterID = KIM.YDReqMasterID
		                    Inner Join {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = KIC.ItemMasterID
		                    Left Join {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID  = IM.Segment6ValueID
		                    Left JOIN {TableNames.YD_REQ_CHILD} RC On RC.YDReqChildID = KIC.YDReqChildID
		                    Left JOIN {TableNames.YD_REQ_MASTER} RM On RM.YDReqMasterID = RC.YDReqMasterID
		                    Where KIM.IsApprove = 1 and KIC.YDReqIssueChildID in({YDReqIssueMasterID});

                            Select 
                            KIC.YDReqIssueChildID,KIM.YDReqIssueMasterID,KIM.YDReqIssueNo,YarnCategory= IM.ItemName, KIC.PhysicalCount, YarnCount = ISV6.SegmentValue,KIC.LotNo,
                            BookingNo = STRING_AGG(YDBM.GroupConceptNo,','),'' BatchNo,
                            KIC.ItemMasterID, [Floor]=CE.ShortName,YDBC.FCMRChildID,YDBC.YBChildItemID,
							RCompanyID = RM.CompanyID,
							OCompanyID = RM.CompanyID,
							ExportOrderNo = case when isnull(FBM.BookingID,0)>0 then FBM.ExportOrderNo when isnull(YNBM.BookingID,0)>0 then YNBM.ExportOrderNo  else '' end,
                            YDBM.YDBookingMasterID,
							BalanceQuantity = KIC.IssueQty - (SUM(ISnull(YLORC.UseableReturnQtyKG,0))+SUM(ISnull(YLORC.UnuseableReturnQtyKG,0)))
                            FROM {TableNames.YD_REQ_ISSUE_CHILD} KIC
                            Inner JOIN {TableNames.YD_REQ_ISSUE_MASTER} KIM On KIM.YDReqIssueMasterID = KIC.YDReqIssueMasterID
                            Inner Join {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = KIC.ItemMasterID
                            Left Join {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID  = IM.Segment6ValueID
							Left JOIN {TableNames.YD_REQ_CHILD} RC On RC.YDReqChildID = KIC.YDReqChildID
                            Left JOIN {TableNames.YD_REQ_MASTER} RM On RM.YDReqMasterID = RC.YDReqMasterID
							LEFT JOIN {TableNames.YD_Left_Over_Return_CHILD} YLORC ON YLORC.YDReqIssueChildID = KIC.YDReqIssueChildID
							LEFT JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID=RM.CompanyID 
							LEFT JOIN {TableNames.YDBookingChild} YDBC ON YDBC.YDBookingChildID=RC.YDBookingChildID
							Left JOIN {TableNames.YD_BOOKING_MASTER} YDBM On YDBM.YDBookingMasterID = YDBC.YDBookingMasterID
                            LEFT JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCMRC ON FCMRC.FCMRChildID=YDBC.FCMRChildID
							LEFT JOIN {TableNames.FreeConceptMRMaster} FCMRM ON FCMRM.FCMRMasterID = FCMRC.FCMRMasterID
							LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = FCMRM.ConceptID
							LEFT JOIN {DbNames.EPYSL}..BookingMaster FBM ON FBM.BookingID = FCM.BookingID
							LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster FSBM ON FSBM.BookingID = FCM.BookingID
							--LEFT JOIN {DbNames.EPYSL}..ExportOrderMaster EOM ON EOM.ExportOrderID=SBM.ExportOrderID
							LEFT JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID=YDBC.YBChildItemID
							LEFT JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID=YBCI.YBChildID 
							LEFT JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID=YBC.YBookingID 
							LEFT JOIN {DbNames.EPYSL}..BookingMaster YNBM ON YNBM.BookingID = YBM.BookingID
							LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster YNSBM ON YNSBM.BookingID = YBM.BookingID
                            Where KIM.IsApprove = 1 and KIC.YDReqIssueChildID in({YDReqIssueMasterID})
							GROUP BY  KIC.YDReqIssueChildID,KIM.YDReqIssueMasterID,KIM.YDReqIssueNo,IM.ItemName, KIC.PhysicalCount, ISV6.SegmentValue,KIC.LotNo,RM.CompanyID,
                            KIC.IssueQty,KIC.ItemMasterID,FBM.BookingID, FBM.ExportOrderNo, CE.ShortName,YNBM.BookingID, YNBM.ExportOrderNo, YDBC.FCMRChildID,YDBC.YBChildItemID,
                            YDBM.YDBookingMasterID";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                //KYLOReturnMaster data = records.Read<KYLOReturnMaster>().FirstOrDefault();
                YDLeftOverReturnMaster data = records.Read<YDLeftOverReturnMaster>().FirstOrDefault();
                Guard.Against.NullObject(data);
                data.Childs = records.Read<YDLeftOverReturnChild>().ToList();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                // _connection.Close();
                if (_connection.State == System.Data.ConnectionState.Open) _connection.Close();
            }
        }

        public async Task<YDLeftOverReturnMaster> GetAsync(int id)
        {
            var query =
                $@"Select * 
                FROM {TableNames.YD_Left_Over_Return_MASTER}
                Where YDLOReturnMasterID = {id};

                Select KRC.*,KRM.YDLOReturnMasterID,KRM.YDLOReturnNo,KRM.YDLOReturnDate,KIM.YDReqIssueNo,YarnDetails= FCMRC.YarnCategory,
                KIC.YDReqIssueChildID,KIC.YDReqIssueMasterID,BookingNo = STRING_AGG(YDBM.GroupConceptNo,','),'' BatchNo,KIC.PhysicalCount, 
                YarnCount = ISV6.SegmentValue,BalanceQuantity = KIC.IssueQty,
				KIC.ItemMasterID, [Floor]=CE.ShortName,YDBC.FCMRChildID,YDBC.YBChildItemID,
				ExportOrderNo = Case When YBM.BookingID>0 Then BM.ExportOrderNo Else EOM.ExportOrderNo END
                From {TableNames.YD_Left_Over_Return_CHILD} KRC
                Inner JOIN {TableNames.YD_Left_Over_Return_MASTER} KRM On KRM.YDLOReturnMasterID = KRC.YDLOReturnMasterID
                Inner JOIN {TableNames.YD_REQ_ISSUE_CHILD} KIC On KIC.YDReqIssueChildID = KRC.YDReqIssueChildID
                Inner JOIN {TableNames.YD_REQ_ISSUE_MASTER} KIM On KIM.YDReqIssueMasterID = KIC.YDReqIssueMasterID
                Left JOIN {TableNames.YD_REQ_CHILD} RC On RC.YDReqChildID = KIC.YDReqChildID
                Left JOIN {TableNames.YD_REQ_MASTER} RM On RM.YDReqMasterID = RC.YDReqMasterID
				LEFT JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID=RM.CompanyID 
				LEFT JOIN {TableNames.YDBookingChild} YDBC ON YDBC.YDBookingChildID=RC.YDBookingChildID
				Left JOIN {TableNames.YD_BOOKING_MASTER} YDBM On YDBM.YDBookingMasterID = YDBC.YDBookingMasterID
				LEFT JOIN {TableNames.YarnBookingChildItem_New} YBCI ON YBCI.YBChildItemID=YDBC.YBChildItemID
				LEFT JOIN {TableNames.YarnBookingChild_New} YBC ON YBC.YBChildID=YBCI.YBChildID 
				LEFT JOIN {TableNames.YarnBookingMaster_New} YBM ON YBM.YBookingID=YBC.YBookingID 
                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MR_CHILD} FCMRC ON FCMRC.FCMRChildID=YDBC.FCMRChildID
				LEFT JOIN {TableNames.FreeConceptMRMaster} FCMRM ON FCMRM.FCMRMasterID = FCMRC.FCMRMasterID
				LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM ON FCM.ConceptID = FCMRM.ConceptID
				LEFT JOIN {DbNames.EPYSL}..BookingMaster BM ON BM.BookingID = YBM.BookingID
				LEFT JOIN {DbNames.EPYSL}..SampleBookingMaster SBM ON SBM.BookingID = FCM.BookingID
				LEFT JOIN {DbNames.EPYSL}..ExportOrderMaster EOM ON EOM.ExportOrderID=SBM.ExportOrderID
                Inner Join {DbNames.EPYSL}..ItemMaster IM On IM.ItemMasterID = KIC.ItemMasterID
                Left Join {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID  = IM.Segment6ValueID
                Where KRC.YDLOReturnMasterID = {id}
				GROUP BY  KRC.YDLOReturnChildID,KRC.YDLOReturnMasterID,KRC.ItemMasterId,KRC.UnitId,KRC.IssueQty,KRC.IssueCone,KRC.IssueQtyCarton,KRC.
			    UseableReturnQtyKG,KRC.UseableReturnQtyCone,KRC.UseableReturnQtyBag,KRC.UnuseableReturnQtyKG,KRC.UnuseableReturnQtyCone,KRC.UnuseableReturnQtyBag,KRC.
			    Remarks,KRC.YarnCategory,KRC.NoOfThread,KRC.LotNo,KRC.YarnProgramID,KRC.Rate,KRC.YDReqIssueChildID,
				KRM.YDLOReturnMasterID,KRM.YDLOReturnNo,KRM.YDLOReturnDate,KIM.YDReqIssueNo,FCMRC.YarnCategory,
                KIC.YDReqIssueChildID,KIC.YDReqIssueMasterID,KIC.PhysicalCount, 
                ISV6.SegmentValue,KIC.IssueQty,KIC.ItemMasterID,BM.BookingID,BM.ExportOrderNo,CE.ShortName, YDBC.FCMRChildID,YDBC.YBChildItemID,
				Case When YBM.BookingID>0 Then BM.ExportOrderNo Else EOM.ExportOrderNo END";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);

                YDLeftOverReturnMaster data = records.Read<YDLeftOverReturnMaster>().FirstOrDefault();
                Guard.Against.NullObject(data);
                data.Childs = records.Read<YDLeftOverReturnChild>().ToList();

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

        public async Task<YDLeftOverReturnMaster> GetAllByIDAsync(int id)
        {
            string sql = $@"
            ;Select * FROM {TableNames.YD_Left_Over_Return_MASTER} Where YDLOReturnMasterID = {id}

            ;Select * From {TableNames.YD_Left_Over_Return_CHILD} Where YDLOReturnMasterID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YDLeftOverReturnMaster data = records.Read<YDLeftOverReturnMaster>().FirstOrDefault();
                Guard.Against.NullObject(data);
                data.Childs = records.Read<YDLeftOverReturnChild>().ToList();
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
        public async Task SaveAsync(YDLeftOverReturnMaster entity)
        {

            SqlTransaction transaction = null;
            SqlTransaction transactionGmt = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _connectionGmt.OpenAsync();
                transactionGmt = _connectionGmt.BeginTransaction();

                int maxChildId = 0;
                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity.YDLOReturnMasterID = await _service.GetMaxIdAsync(TableNames.YD_Left_Over_Return_MASTER, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        entity.YDLOReturnNo = await _service.GetMaxNoAsync(TableNames.YD_Left_Over_Return_NO, 1, RepeatAfterEnum.NoRepeat, "00000", transactionGmt, _connectionGmt);

                        maxChildId = await _service.GetMaxIdAsync(TableNames.YD_Left_Over_Return_CHILD, entity.Childs.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        foreach (YDLeftOverReturnChild item in entity.Childs)
                        {
                            item.YDLOReturnChildID = maxChildId++;
                            item.YDLOReturnMasterID = entity.YDLOReturnMasterID;
                        }
                        break;

                    case EntityState.Modified:
                        List<YDLeftOverReturnChild> addedChilds = entity.Childs.FindAll(x => x.EntityState == EntityState.Added);
                        maxChildId = await _service.GetMaxIdAsync(TableNames.YD_Left_Over_Return_CHILD, addedChilds.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        foreach (YDLeftOverReturnChild item in addedChilds)
                        {
                            item.YDLOReturnChildID = maxChildId++;
                            item.YDLOReturnMasterID = entity.YDLOReturnMasterID;
                        }
                        break;
                    case EntityState.Unchanged:
                    case EntityState.Deleted:
                        entity.EntityState = EntityState.Deleted;
                        entity.Childs.SetDeleted();
                        break;

                    default:
                        break;
                }

                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.Childs, transaction);

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

        private async Task<YDLeftOverReturnMaster> AddAsync(YDLeftOverReturnMaster entity, SqlTransaction transactionGmt)
        {
            entity.YDLOReturnMasterID = await _service.GetMaxIdAsync(TableNames.YD_Left_Over_Return_MASTER, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            entity.YDLOReturnNo = await _service.GetMaxNoAsync(TableNames.YD_LO_Return_No, 1, RepeatAfterEnum.NoRepeat, "00000", transactionGmt, _connectionGmt);
            var maxChildId = await _service.GetMaxIdAsync(TableNames.YD_Left_Over_Return_CHILD, entity.Childs.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

            foreach (var item in entity.Childs)
            {
                item.YDLOReturnChildID = maxChildId++;
                item.YDLOReturnMasterID = entity.YDLOReturnMasterID;
            }
            return entity;
        }

        private async Task<YDLeftOverReturnMaster> UpdateAsync(YDLeftOverReturnMaster entity, SqlTransaction transactionGmt)
        {
            var maxChildId = await _service.GetMaxIdAsync(TableNames.YD_Left_Over_Return_CHILD, entity.Childs.Where(x => x.EntityState == EntityState.Added).Count(), RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
            foreach (var item in entity.Childs.ToList())
            {
                switch (item.EntityState)
                {
                    case EntityState.Added:
                        item.YDLOReturnChildID = maxChildId++;
                        item.YDLOReturnMasterID = entity.YDLOReturnMasterID;
                        break;

                    case EntityState.Modified:
                        item.EntityState = EntityState.Modified;
                        break;

                    case EntityState.Deleted:
                        item.EntityState = EntityState.Deleted;
                        break;

                    default:
                        break;
                }
            }
            return entity;
        }
    }
}
