using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using System.Data.Entity;
using Microsoft.Data.SqlClient;

namespace EPYSLTEXCore.Application.Services.Inventory
{
    public class YarnQCReturnService : IYarnQCReturnService
    {

        private readonly IDapperCRUDService<YarnQCReturnMaster> _service;

        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;

        public YarnQCReturnService(IDapperCRUDService<YarnQCReturnMaster> service
            , IDapperCRUDService<YarnQCReturnChild> itemMasterRepository)
        {
            _service = service;

            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
        }

        public async Task<List<YarnQCReturnMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By QCReturnMasterID Desc" : paginationInfo.OrderBy;

            string sql;
            if (status == Status.Pending)
            {
                sql = $@"
                WITH
                PartialRcv AS
                (
	                SELECT RCC.QCReceiveChildID
	                FROM {TableNames.YARN_QC_RETURN_CHILD} RC
	                INNER JOIN {TableNames.YARN_QC_RECEIVE_CHILD} RCC ON RCC.QCReceiveChildID = RC.QCReceiveChildID
	                GROUP BY RCC.QCReceiveChildID, ISNULL(RCC.ReceiveQty,0)
	                HAVING ISNULL(SUM(RC.ReturnQty),0) < ISNULL(RCC.ReceiveQty,0)
                ),
                A AS
                (
	                SELECT QCRC.QCReceiveChildID,RM.QCReqNo,YRC.YarnCategory,YRC.PhysicalCount,YRC.LotNo,Spinner = SP.ShortName,YRC.ShadeCode,IM.QCIssueNo,YRM.ReceiveNo
	                ,QCRM.QCReceiveNo
	
	                --, QCRC.QCReceiveMasterID, QCRM.QCReceiveNo, QCRM.QCReceivedBy, 
	                --QCRM.QCReqMasterID, QCIC.QCIssueMasterID,
	                --QCReceiveDate = CONVERT(DATETIME, CONVERT(CHAR(8), QCRM.QCReceiveDate, 112) + ' ' + CONVERT(CHAR(8), QCRM.DateAdded, 108)),
	                --YRM.LocationId, YRM.RCompanyId, QCRM.CompanyId, YRM.SupplierId, YRC.SpinnerId, YRM.ReceiveNo,
	                --IM.QCIssueNo,
	                --QCIssueDate = CONVERT(DATETIME, CONVERT(CHAR(8), IM.QCIssueDate, 112) + ' ' + CONVERT(CHAR(8), IM.DateAdded, 108)), ER.EmployeeName QCReceivedByUser,
	                --QCIssueByUser = EU.EmployeeName, RM.QCReqNo, RM.QCReqDate, RM.QCForID, QCReqByUser = ERU.EmployeeName, QCReqFor = QCReqFor.ValueName,
	                --Status = CASE WHEN RM.RetestQCReqMasterID > 0 THEN 'Retest' ELSE '' END

	                FROM {TableNames.YARN_QC_RECEIVE_CHILD} QCRC
	                INNER JOIN {TableNames.YARN_QC_RECEIVE_MASTER} QCRM ON QCRM.QCReceiveMasterID = QCRC.QCReceiveMasterID
	                INNER JOIN {TableNames.YARN_QC_ISSUE_CHILD} QCIC ON QCIC.QCIssueChildID = QCRC.QCIssueChildID
	                INNER JOIN {TableNames.YARN_QC_REQ_CHILD} QCRC2 ON QCRC2.QCReqChildID = QCIC.QCReqChildID 
	                INNER JOIN {TableNames.YARN_RECEIVE_MASTER} YRM ON YRM.ReceiveID = QCRM.ReceiveID
	                INNER JOIN {TableNames.YARN_RECEIVE_CHILD} YRC ON YRC.ChildID = QCRC2.ReceiveChildID

	                INNER JOIN {TableNames.YARN_QC_REQ_MASTER} RM On RM.QCReqMasterID = QCRM.QCReqMasterID
	                INNER JOIN {TableNames.YARN_QC_ISSUE_MASTER} IM On IM.QCIssueMasterID = QCIC.QCIssueMasterID
	                INNER JOIN {DbNames.EPYSL}..LoginUser IR On IR.UserCode = QCRM.QCReceivedBy
	                INNER JOIN {DbNames.EPYSL}..Employee ER On ER.EmployeeCode=IR.EmployeeCode
	                INNER JOIN {DbNames.EPYSL}..LoginUser IU On IU.UserCode = IM.QCIssueBy
	                INNER JOIN {DbNames.EPYSL}..Employee EU On EU.EmployeeCode=IU.EmployeeCode
	                INNER JOIN {DbNames.EPYSL}..LoginUser RU On RU.UserCode = RM.QCReqBy 
	                INNER JOIN {DbNames.EPYSL}..Employee ERU On ERU.EmployeeCode=RU.EmployeeCode
	                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue QCReqFor On RM.QCForID = QCReqFor.ValueID
	                LEFT JOIN {DbNames.EPYSL}..Contacts SP ON SP.ContactID = YRC.SpinnerID
	                LEFT JOIN {TableNames.YARN_QC_RETURN_CHILD} RC ON RC.QCReceiveChildID = QCRC.QCReceiveChildID
	                WHERE RC.QCReturnChildID IS NULL
                ),
                B AS
                (
	                SELECT QCRC.QCReceiveChildID,RM.QCReqNo,YRC.YarnCategory,YRC.PhysicalCount,YRC.LotNo,Spinner = SP.ShortName,YRC.ShadeCode,IM.QCIssueNo,YRM.ReceiveNo
	                ,QCRM.QCReceiveNo
	
	                --, QCRC.QCReceiveMasterID, QCRM.QCReceiveNo, QCRM.QCReceivedBy, 
	                --QCRM.QCReqMasterID, QCIC.QCIssueMasterID,
	                --QCReceiveDate = CONVERT(DATETIME, CONVERT(CHAR(8), QCRM.QCReceiveDate, 112) + ' ' + CONVERT(CHAR(8), QCRM.DateAdded, 108)),
	                --YRM.LocationId, YRM.RCompanyId, QCRM.CompanyId, YRM.SupplierId, YRC.SpinnerId, YRM.ReceiveNo,
	                --IM.QCIssueNo,
	                --QCIssueDate = CONVERT(DATETIME, CONVERT(CHAR(8), IM.QCIssueDate, 112) + ' ' + CONVERT(CHAR(8), IM.DateAdded, 108)), ER.EmployeeName QCReceivedByUser,
	                --QCIssueByUser = EU.EmployeeName, RM.QCReqNo, RM.QCReqDate, RM.QCForID, QCReqByUser = ERU.EmployeeName, QCReqFor = QCReqFor.ValueName,
	                --Status = CASE WHEN RM.RetestQCReqMasterID > 0 THEN 'Retest' ELSE '' END

	                FROM PartialRcv A
	                INNER JOIN {TableNames.YARN_QC_RECEIVE_CHILD} QCRC ON QCRC.QCReceiveChildID = A.QCReceiveChildID
	                INNER JOIN {TableNames.YARN_QC_RECEIVE_MASTER} QCRM ON QCRM.QCReceiveMasterID = QCRC.QCReceiveMasterID
	                INNER JOIN {TableNames.YARN_QC_ISSUE_CHILD} QCIC ON QCIC.QCIssueChildID = QCRC.QCIssueChildID
	                INNER JOIN {TableNames.YARN_QC_REQ_CHILD} QCRC2 ON QCRC2.QCReqChildID = QCIC.QCReqChildID 
	                INNER JOIN {TableNames.YARN_RECEIVE_MASTER} YRM ON YRM.ReceiveID = QCRM.ReceiveID
	                INNER JOIN {TableNames.YARN_RECEIVE_CHILD} YRC ON YRC.ChildID = QCRC2.ReceiveChildID

	                INNER JOIN {TableNames.YARN_QC_REQ_MASTER} RM On RM.QCReqMasterID = QCRM.QCReqMasterID
	                INNER JOIN {TableNames.YARN_QC_ISSUE_MASTER} IM On IM.QCIssueMasterID = QCIC.QCIssueMasterID
	                INNER JOIN {DbNames.EPYSL}..LoginUser IR On IR.UserCode = QCRM.QCReceivedBy
	                INNER JOIN {DbNames.EPYSL}..Employee ER On ER.EmployeeCode=IR.EmployeeCode
	                INNER JOIN {DbNames.EPYSL}..LoginUser IU On IU.UserCode = IM.QCIssueBy
	                INNER JOIN {DbNames.EPYSL}..Employee EU On EU.EmployeeCode=IU.EmployeeCode
	                INNER JOIN {DbNames.EPYSL}..LoginUser RU On RU.UserCode = RM.QCReqBy 
	                INNER JOIN {DbNames.EPYSL}..Employee ERU On ERU.EmployeeCode=RU.EmployeeCode
	                LEFT JOIN {DbNames.EPYSL}..EntityTypeValue QCReqFor On RM.QCForID = QCReqFor.ValueID
	                LEFT JOIN {DbNames.EPYSL}..Contacts SP ON SP.ContactID = YRC.SpinnerID
	                LEFT JOIN {TableNames.YARN_QC_RETURN_CHILD} RC ON RC.QCReceiveChildID = QCRC.QCReceiveChildID
                ),
                A1 AS
                (
	                SELECT A.QCReceiveChildID,A.QCReqNo,A.YarnCategory,A.PhysicalCount,A.LotNo,A.Spinner,A.ShadeCode,A.QCIssueNo,A.ReceiveNo,A.QCReceiveNo
	                FROM A
	                GROUP BY A.QCReceiveChildID,A.QCReqNo,A.YarnCategory,A.PhysicalCount,A.LotNo,A.Spinner,A.ShadeCode,A.QCIssueNo,A.ReceiveNo,A.QCReceiveNo
                ),
                B1 AS
                (
	                SELECT A.QCReceiveChildID,A.QCReqNo,A.YarnCategory,A.PhysicalCount,A.LotNo,A.Spinner,A.ShadeCode,A.QCIssueNo,A.ReceiveNo,A.QCReceiveNo
	                FROM B A
	                GROUP BY A.QCReceiveChildID,A.QCReqNo,A.YarnCategory,A.PhysicalCount,A.LotNo,A.Spinner,A.ShadeCode,A.QCIssueNo,A.ReceiveNo,A.QCReceiveNo
                ),
                FinalList AS
                (
	                SELECT * FROM A1
	                --UNION
	                --SELECT * FROM B1
                )
                SELECT *,Count(*) Over() TotalRows FROM FinalList
                ";

                orderBy = "Order By QCReceiveChildID Desc";


                /*

                sql = $@"
                /*With
                RC As (
	                Select *
	                From YarnQCRemarksChild C
	                Where Approve = 1 OR Reject = 1
                )
                ,PR As (
	                Select M.QCRemarksMasterID, M.QCReqMasterID, REQ.QCReqNo, REQ.QCReqDate,REQ.QCReqBy,QCReqFor.ValueName as QCReqForUser, RCV.QCReceivedBy,
                    RCV.QCReceiveDate,RCV.QCReceiveNo, SUM(RCVC.ReceiveQtyCarton) ReceiveQtyCarton, SUM(RCVC.ReceiveQtyCone) ReceiveQtyCone, SUM(REQC.ReqQty) QCReqQty,
                    M.QCRemarksNo,M.QCRemarksDate
	                From RC
	                Inner Join YarnQCRemarksMaster M On RC.QCRemarksMasterID = M.QCRemarksMasterID
	                Inner Join YarnQCReqMaster REQ On REQ.QCReqMasterID = M.QCReqMasterID
	                Inner Join YarnQCReqChild REQC On REQ.QCReqMasterID = REQC.QCReqMasterID
	                Inner Join YarnQCReceiveMaster RCV On RCV.QCReceiveMasterID = M.QCReceiveMasterID
	                Inner Join YarnQCReceiveChild RCVC on RCVC.QCReceiveMasterID = RCV.QCReceiveMasterID
	                Left Join YarnQCReturnMaster RTM On M.QCRemarksMasterID = RTM.QCRemarksMasterID
					Left Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On REQ.QCForID = QCReqFor.ValueID
	                Where RTM.QCReturnMasterID IS NULL And M.IsApproved = 1
	                GROUP BY M.QCRemarksMasterID,M.QCRemarksNo,M.QCRemarksDate, M.QCReqMasterID, REQ.QCReqNo, REQ.QCReqDate,REQ.QCReqBy, RCV.QCReceivedBy,
                    RCV.QCReceiveDate,RCV.QCReceiveNo,QCReqFor.ValueName
                )

                Select QCRemarksMasterID,QCRemarksNo,QCRemarksDate, QCReqMasterID, QCReqNo, QCReqDate, QCReqBy, QCReceivedBy, QCReceiveDate, ReceiveQtyCarton, 
                ReceiveQtyCone, QCReqQty,QCReceiveNo,QCReqForUser, Count(*) Over() TotalRows


                With
                M As (
                        Select RM.QCRemarksMasterID,RM.QCRemarksNo,RM.QCRemarksDate,RC.QCReceiveMasterID, RC.QCReceiveNo, QCReceivedBy, RC.QCReqMasterID, RC.QCIssueMasterID,
                        CONVERT(DATETIME, CONVERT(CHAR(8), QCReceiveDate, 112) + ' ' + CONVERT(CHAR(8), RC.DateAdded, 108)) QCReceiveDate,
                        RC.LocationId, RC.RCompanyId, RC.CompanyId, RC.SupplierId, RC.SpinnerId, YRM.ReceiveNo
                        From YarnQCReceiveMaster RC
                        INNER JOIN YarnReceiveMaster YRM ON YRM.ReceiveID = RC.ReceiveID
                        LEFT JOIN YarnQCRemarksMaster RM ON RM.QCReqMasterID=RC.QCReqMasterID 
                        LEFT JOIN YarnQCReturnMaster RT ON RT.QCReqMasterID=RC.QCReqMasterID 
                        WHERE RT.QCReqMasterID Is Null
                     )
                SELECT M.QCRemarksMasterID,M.QCRemarksNo,M.QCRemarksDate,M.QCReceiveMasterID, M.QCReceiveNo, M.QCReceivedBy, M.QCReceiveDate, M.QCReqMasterID, M.QCIssueMasterID, 
                 M.LocationId, M.RCompanyId, M.CompanyId, M.SupplierId, M.SpinnerId, IM.QCIssueNo,
				CONVERT(DATETIME, CONVERT(CHAR(8), IM.QCIssueDate, 112) + ' ' + CONVERT(CHAR(8), IM.DateAdded, 108)) QCIssueDate, ER.EmployeeName QCReceivedByUser,
                EU.EmployeeName QCIssueByUser, RM.QCReqNo, RM.QCReqDate, RM.QCForID, ERU.EmployeeName QCReqByUser, QCReqFor.ValueName QCReqFor ,
                Status = CASE WHEN RM.RetestQCReqMasterID > 0 THEN 'Retest' ELSE '' END, M.ReceiveNo,
                Count(*) Over() TotalRows
                FROM M
                Inner Join YarnQCReqMaster RM On RM.QCReqMasterID = M.QCReqMasterID
                Inner Join YarnQCIssueMaster IM On IM.QCIssueMasterID = M.QCIssueMasterID
                Inner Join {DbNames.EPYSL}..LoginUser IR On IR.UserCode = M.QCReceivedBy
				Inner Join {DbNames.EPYSL}..Employee ER On ER.EmployeeCode=IR.EmployeeCode
                Inner Join {DbNames.EPYSL}..LoginUser IU On IU.UserCode = IM.QCIssueBy
				Inner Join {DbNames.EPYSL}..Employee EU On EU.EmployeeCode=IU.EmployeeCode
                Inner Join {DbNames.EPYSL}..LoginUser RU On RU.UserCode = RM.QCReqBy 
				Inner Join {DbNames.EPYSL}..Employee ERU On ERU.EmployeeCode=RU.EmployeeCode
                Left Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On RM.QCForID = QCReqFor.ValueID";

                */
            }
            else
            {
                sql = $@"
                
                With
                RM As (
	                Select *
	                From {TableNames.YARN_QC_RETURN_MASTER} C
	                Where IsApprove = 0 
                )
                ,PR As (
	                Select RM.QCReturnMasterID,RM.QCReturnNo,RMK.QCRemarksNo,RMK.QCRemarksDate, RU.Name as QCReturnByUser, RM.QCReturnDate, RM.QCRemarksMasterID, 
                    RM.QCReqMasterID, REQ.QCReqNo, REQ.QCReqDate,REQ.QCReqBy, RCV.QCReceivedBy,RCV.QCReceiveDate, SUM(RC.ReceiveQtyCarton) ReceiveQtyCarton, 
                    SUM(RC.ReceiveQtyCone) ReceiveQtyCone, SUM(RCVC.ReqQtyCone) ReqQty, SUM(RC.ReturnQtyCarton) ReturnQtyCarton, SUM(RC.ReturnQtyCone) ReturnQtyCone, YRM.ReceiveNo
	                From RM
	                inner JOIN {TableNames.YARN_QC_RETURN_CHILD} RC ON RM.QCReturnMasterID = RC.QCReturnMasterID
					inner JOIN {TableNames.YARN_QC_REQ_MASTER} REQ ON RM.QCReqMasterID = REQ.QCReqMasterID
					Left JOIN {TableNames.YARN_QC_REMARKS_MASTER} RMK ON RM.QCRemarksMasterID = RMK.QCRemarksMasterID
					inner JOIN {TableNames.YARN_QC_RECEIVE_MASTER} RCV ON REQ.QCReqMasterID = RCV.QCReqMasterID
					inner JOIN {TableNames.YARN_QC_RECEIVE_CHILD} RCVC ON RCV.QCReceiveMasterID = RCVC.QCReceiveMasterID
                    INNER JOIN {TableNames.YARN_RECEIVE_MASTER} YRM ON YRM.ReceiveID = RCV.ReceiveID
					Inner Join {DbNames.EPYSL}..LoginUser RU On RM.QCReturnBy = RU.UserCode

	                GROUP BY RM.QCReturnMasterID,RMK.QCRemarksNo,RMK.QCRemarksDate,RM.QCReturnNo, RU.Name , RM.QCReturnDate, RM.QCRemarksMasterID, RM.QCReqMasterID, 
                    REQ.QCReqNo, REQ.QCReqDate,REQ.QCReqBy, RCV.QCReceivedBy,RCV.QCReceiveDate, YRM.ReceiveNo
                )

                Select QCReturnMasterID,QCReturnNo, QCRemarksNo, QCRemarksDate, QCReturnByUser,QCReturnDate,QCRemarksMasterID, QCReqMasterID, QCReqNo, QCReqDate,ReqQty, 
                QCReqBy, QCReceivedBy, QCReceiveDate, ReceiveQtyCarton, ReceiveQtyCone,ReturnQtyCarton,ReturnQtyCone,ReceiveNo, Count(*) Over() TotalRows
                From PR ";
            }

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<YarnQCReturnMaster>(sql);
        }

        public async Task<YarnQCReturnMaster> GetNewAsync(int qcRemarksMasterId)
        {
            var query =
                $@"
                 With R As 
                (
	                Select QCRemarksMasterID,Rec.QCReqMasterID,Rec.QCIssueMasterID,Rec.QCReceiveMasterID,QCRemarksNo,QCRemarksBy,QCRemarksDate
	                From {TableNames.YARN_QC_RECEIVE_MASTER} Rec
					LEFT JOIN {TableNames.YARN_QC_REMARKS_MASTER} Rem ON Rem.QCReceiveMasterID=Rec.QCReceiveMasterID
	                Where Rec.QCReceiveMasterID = {qcRemarksMasterId}
                )

                Select  QCRemarksMasterID,QCReqFor.ValueName QCReqFor ,QCReceiveDate ,QCReceiveNo ,QCReqNo ,QCReqDate,R.QCReqMasterID
                From R
                Inner Join {TableNames.YARN_QC_RECEIVE_MASTER} RM On R.QCReceiveMasterID = RM.QCReceiveMasterID
                Inner Join {TableNames.YARN_QC_REQ_MASTER} RQ ON R.QCReqMasterID = RQ.QCReqMasterID
                Left Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On RQ.QCForID = QCReqFor.ValueID;

                --Child

               ;Select YRC.QCRemarksChildID, YRC.QCRemarksMasterID, YRVC.ReceiveChildID, YRVC.LotNo, YRVC.ChallanLot, YRVC.ReceiveQty AS ReceiveQty, YRVC.ReceiveQtyCone AS ReceiveQtyCone, 
                YRVC.ReceiveQtyCarton AS ReceiveQtyCarton, YRVC.ItemMasterID, YRVC.UnitID, YRVC.Rate,YRVC.YarnProgramId, YRVC.ChallanCount, YRVC.POCount, 
                YRVC.PhysicalCount, YRVC.YarnCategory, YRVC.NoOfThread, UU.DisplayUnitDesc Uom,ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc,
                ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, 
                ISV7.SegmentValue Segment7ValueDesc,YRVC.ShadeCode, YRVC.ReceiveQty AS ReturnQty , YRVC.ReceiveQtyCone AS ReturnQtyCone, YRVC.ReceiveQtyCarton AS ReturnQtyCarton, SS.[Name] Spinner
                From {TableNames.YARN_QC_RECEIVE_CHILD} YRVC
				LEFT JOIN {TableNames.YARN_QC_REMARKS_CHILD} YRC ON YRC.QCReceiveChildID=YRVC.QCReceiveChildID
                LEFT JOIN {TableNames.YARN_RECEIVE_CHILD} YC ON YC.ChildID=YRVC.ReceiveChildID
				LEFT JOIN {DbNames.EPYSL}..Contacts SS ON SS.ContactID = YC.SpinnerID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YRVC.ItemMasterID 
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT Join {DbNames.EPYSL}..Unit AS UU On UU.UnitID = YRVC.UnitID
                Where YRVC.QCReceiveMasterID = {qcRemarksMasterId}
                Group By YRC.QCRemarksChildID,YRC.QCRemarksMasterID, YRVC.ReceiveChildID, YRVC.LotNo, YRVC.ChallanLot, YRVC.ReceiveQty, YRVC.ReceiveQtyCone, YRVC.ReceiveQtyCarton,
                YRVC.ItemMasterID, YRVC.UnitID, YRVC.Rate,YRVC.YarnProgramId, YRVC.ChallanCount, YRVC.POCount, YRVC.PhysicalCount, YRVC.YarnCategory, YRVC.NoOfThread,
                UU.DisplayUnitDesc,ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue, 
                ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue,
				YRVC.ShadeCode, SS.[Name] ";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                YarnQCReturnMaster data = await records.ReadFirstOrDefaultAsync<YarnQCReturnMaster>();
                Guard.Against.NullObject(data);
                data.YarnQCReturnChilds = records.Read<YarnQCReturnChild>().ToList();
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

        public async Task<YarnQCReturnMaster> GetAsync(int id)
        {
            var query =
                 $@"
                 With R As 
                (
	                Select*
	                From {TableNames.YARN_QC_RETURN_MASTER}
	                Where QCReturnMasterID = {id}
                )

                Select R.QCReturnMasterID, R.QCReturnNo, R.QCReturnDate, R.QCReturnBy,R.QCRemarksMasterID, RMK.QCReceiveMasterID,QCReqFor.ValueName QCReqFor,
                RM.QCReceiveDate ,RM.QCReceiveNo ,RQ.QCReqNo ,RQ.QCReqDate
                From R
                Left Join {TableNames.YARN_QC_REMARKS_MASTER} RMK On R.QCRemarksMasterID = RMK.QCRemarksMasterID
                Inner Join {TableNames.YARN_QC_RECEIVE_MASTER} RM On R.QCReqMasterID = RM.QCReqMasterID
                Inner Join {TableNames.YARN_QC_REQ_MASTER} RQ ON R.QCReqMasterID = RQ.QCReqMasterID
                Left Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On RQ.QCForID = QCReqFor.ValueID;

                -- Childs

               SELECT 
	                RC.QCReturnChildID, RC.QCReturnMasterID, RC.QCRemarksChildID,
	                RC.QCReceiveChildID, RC.ReceiveChildID, RC11.ReceiveQtyCarton,
	                RC11.ReceiveQtyCone, RC11.ReceiveQty, RC.ReturnQtyCarton, RC.ReturnQtyCone,
	                RC.ReturnQty, RC.Remarks,

	                RC.QCReceiveChildID, YRC.YarnCategory, Uom = 'KG',
                    YRC.ChallanLot, YRC.LotNo, YRC.PhysicalCount, Spinner = SP.ShortName,
                    ReceiveQty = RC.ReceiveQty, ReceiveQtyCone = RC.ReceiveQtyCone, ReceiveQtyCarton = RC.ReceiveQtyCarton,
                    RM11.QCReceiveNo, QCIM.QCIssueNo, YRM.ReceiveNo, RM.QCRemarksNo, QCRemarksChildID = ISNULL(RE.QCRemarksChildID,0),
                    ReceiveChildId = YRC.ChildId

                From {TableNames.YARN_QC_RETURN_CHILD} RC

                INNER JOIN {TableNames.YARN_QC_RECEIVE_CHILD} RC11 ON RC11.QCReceiveChildID = RC.QCReceiveChildID
                INNER JOIN {TableNames.YARN_QC_RECEIVE_MASTER} RM11 ON RM11.QCReceiveMasterID = RC11.QCReceiveMasterID

                INNER JOIN {TableNames.YARN_QC_ISSUE_CHILD} IC ON IC.QCIssueChildID = RC11.QCIssueChildID
                INNER JOIN {TableNames.YARN_QC_ISSUE_MASTER} QCIM ON QCIM.QCIssueMasterID = IC.QCIssueMasterID

                INNER JOIN {TableNames.YARN_QC_REQ_CHILD} RC1 ON RC1.QCReqChildID = IC.QCReqChildID
                INNER JOIN {TableNames.YARN_QC_REQ_MASTER} RM1 ON RM1.QCReqMasterID = RC1.QCReqMasterID

                INNER JOIN {TableNames.YARN_RECEIVE_CHILD} YRC ON YRC.ChildID = RC1.ReceiveChildID
                INNER JOIN {TableNames.YARN_RECEIVE_MASTER} YRM ON YRM.ReceiveID = YRC.ReceiveID

                LEFT JOIN {TableNames.YARN_QC_REMARKS_CHILD} RE ON RE.QCReceiveChildID = RC.QCReceiveChildID
                LEFT JOIN {TableNames.YARN_QC_REMARKS_MASTER} RM ON RM.QCRemarksMasterID = RE.QCRemarksMasterID


                LEFT JOIN {DbNames.EPYSL}..Contacts SP ON SP.ContactID = YRC.SpinnerID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YRC.ItemMasterID 
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                Inner Join {DbNames.EPYSL}..Unit AS UU On UU.UnitID = YRC.UnitID
                Where RC.QCReturnMasterID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                YarnQCReturnMaster data = await records.ReadFirstOrDefaultAsync<YarnQCReturnMaster>();
                Guard.Against.NullObject(data);
                data.YarnQCReturnChilds = records.Read<YarnQCReturnChild>().ToList();
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

        public async Task<YarnQCReturnMaster> GetDetailsByQCReturnChilds(string qcReceiveChildIds)
        {
            string sql = $@"
           	;WITH M AS (
	            SELECT TOP(1) RM1.QCReqMasterID, RM1.QCReqNo, RM1.QCReqDate, QCReqFor = QCReqFor.ValueName
				,QCReceiveNo = STRING_AGG(RM.QCReceiveNo,',')
	            FROM {TableNames.YARN_QC_RECEIVE_CHILD} YRC
	            INNER JOIN {TableNames.YARN_QC_RECEIVE_MASTER} RM ON RM.QCReceiveMasterID = YRC.QCReceiveMasterID
		        INNER JOIN {TableNames.YARN_QC_REQ_MASTER} RM1 ON RM1.QCReqMasterID = RM.QCReqMasterID
		        LEFT JOIN {DbNames.EPYSL}..EntityTypeValue QCReqFor On QCReqFor.ValueID = RM1.QCForID
	            WHERE YRC.QCReceiveChildID IN ({qcReceiveChildIds})
				GROUP BY RM1.QCReqMasterID, RM1.QCReqNo, RM1.QCReqDate, QCReqFor.ValueName
            )
            SELECT QCReturnDate = GETDATE(), M.QCReqMasterID, M.QCReqNo, M.QCReqDate, M.QCReceiveNo, M.QCReqFor
            FROM M
                
            --childs
            SELECT RC.QCReceiveChildID, RC.YarnCategory, Uom = 'KG',
            YRC.ChallanLot, YRC.LotNo, YRC.PhysicalCount, Spinner = SP.ShortName,
            ReceiveQty = RC.ReceiveQty, ReceiveQtyCone = RC.ReceiveQtyCone, ReceiveQtyCarton = RC.ReceiveQtyCarton,
            QCRM.QCReceiveNo, QCIM.QCIssueNo, YRM.ReceiveNo, RM.QCRemarksNo, QCRemarksChildID = ISNULL(RE.QCRemarksChildID,0),
            ReceiveChildId = YRC.ChildId
            FROM {TableNames.YARN_QC_RECEIVE_CHILD} RC
            INNER JOIN {TableNames.YARN_QC_RECEIVE_MASTER} QCRM ON QCRM.QCReceiveMasterID = RC.QCReceiveMasterID

            INNER JOIN {TableNames.YARN_QC_ISSUE_CHILD} IC ON IC.QCIssueChildID = RC.QCIssueChildID
            INNER JOIN {TableNames.YARN_QC_ISSUE_MASTER} QCIM ON QCIM.QCIssueMasterID = IC.QCIssueMasterID

            INNER JOIN {TableNames.YARN_QC_REQ_CHILD} RC1 ON RC1.QCReqChildID = IC.QCReqChildID
            INNER JOIN {TableNames.YARN_QC_REQ_MASTER} RM1 ON RM1.QCReqMasterID = RC1.QCReqMasterID

            INNER JOIN {TableNames.YARN_RECEIVE_CHILD} YRC ON YRC.ChildID = RC1.ReceiveChildID
            INNER JOIN {TableNames.YARN_RECEIVE_MASTER} YRM ON YRM.ReceiveID = YRC.ReceiveID

            LEFT JOIN {TableNames.YARN_QC_REMARKS_CHILD} RE ON RE.QCReceiveChildID = RC.QCReceiveChildID
            LEFT JOIN {TableNames.YARN_QC_REMARKS_MASTER} RM ON RM.QCRemarksMasterID = RE.QCRemarksMasterID

            LEFT JOIN {DbNames.EPYSL}..Contacts SP ON SP.ContactID = YRC.SpinnerID
            WHERE RC.QCReceiveChildID IN ({qcReceiveChildIds})";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnQCReturnMaster data = new YarnQCReturnMaster();
                List<YarnQCReturnMaster> datas = records.Read<YarnQCReturnMaster>().ToList();
                data = datas.First();
                Guard.Against.NullObject(data);
                data.YarnQCReturnChilds = records.Read<YarnQCReturnChild>().ToList();

                var nQCReturnChildID = 1;
                data.YarnQCReturnChilds.ForEach(x =>
                {
                    x.QCReturnChildID = nQCReturnChildID++;
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

        public async Task<YarnQCReturnMaster> GetAllAsync(int id)
        {
            var sql = $@"
            ;Select * From {TableNames.YARN_QC_RETURN_MASTER} Where QCReturnMasterID = {id}

            ;Select * From {TableNames.YARN_QC_RETURN_CHILD} Where QCReturnMasterID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnQCReturnMaster data = await records.ReadFirstOrDefaultAsync<YarnQCReturnMaster>();
                Guard.Against.NullObject(data);
                data.YarnQCReturnChilds = records.Read<YarnQCReturnChild>().ToList();
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

        public async Task SaveAsync(YarnQCReturnMaster entity)
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
                        entity.QCReturnMasterID = await _service.GetMaxIdAsync(TableNames.YARN_QC_RETURN_MASTER, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        entity.QCReturnNo = await _service.GetMaxNoAsync(TableNames.YARN_QC_RETURN_NO, 1, RepeatAfterEnum.NoRepeat, "00000", transactionGmt, _connectionGmt);

                        maxChildId = await _service.GetMaxIdAsync(TableNames.YARN_QC_RECEIVE_CHILD, entity.YarnQCReturnChilds.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        foreach (YarnQCReturnChild item in entity.YarnQCReturnChilds)
                        {
                            item.QCReturnChildID = maxChildId++;
                            item.QCReturnMasterID = entity.QCReturnMasterID;
                        }
                        break;

                    case EntityState.Modified:
                        var addedChilds = entity.YarnQCReturnChilds.FindAll(x => x.EntityState == EntityState.Added);
                        maxChildId = await _service.GetMaxIdAsync(TableNames.YARN_QC_RECEIVE_CHILD, addedChilds.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        foreach (YarnQCReturnChild item in addedChilds)
                        {
                            item.QCReturnChildID = maxChildId++;
                            item.QCReturnMasterID = entity.QCReturnMasterID;
                        }
                        break;

                    case EntityState.Unchanged:
                    case EntityState.Deleted:
                        entity.EntityState = EntityState.Deleted;
                        entity.YarnQCReturnChilds.SetDeleted();
                        break;

                    default:
                        break;
                }

                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.YarnQCReturnChilds, transaction);

                transaction.Commit();
                transactionGmt.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                transactionGmt.Rollback();
                throw ex;
            }
            finally
            {
                _connection.Close();
                _connectionGmt.Close();
            }
        }
    }
}
