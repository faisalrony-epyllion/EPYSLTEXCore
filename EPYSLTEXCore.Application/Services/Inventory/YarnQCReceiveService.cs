using Dapper;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Application.Interfaces.Inventory;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Newtonsoft.Json;
using System.ComponentModel.Design;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;

namespace EPYSLTEXCore.Application.Services
{
    public class YarnQCReceiveService : IYarnQCReceiveService
    {
        private readonly IDapperCRUDService<YarnQCReceiveMaster> _service;

        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;

        public YarnQCReceiveService(IDapperCRUDService<YarnQCReceiveMaster> service)
            //, IDapperCRUDService<YarnQCReceiveChild> itemMasterRepository)
        {
            _service = service;
            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
        }

        public async Task<List<YarnQCReceiveMaster>> GetPagedAsync(Status status, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By QCReceiveMasterID Desc" : paginationInfo.OrderBy;

            string sql;
            if (status == Status.Pending)
            {
                sql =
                    $@"With 
                    M As (
                        Select QCIssueMasterID, QCIssueNo, QCIssueBy, QCReqMasterID, CONVERT(DATETIME, CONVERT(CHAR(8), QCIssueDate, 112) + ' ' + CONVERT(CHAR(8), DateAdded, 108)) QCIssueDate,
                    Approve, Reject, LocationId, RCompanyId, CompanyId, SupplierId, SpinnerId From {TableNames.YARN_QC_ISSUE_MASTER} Where Approve = 1 AND QCIssueMasterID NOT IN (SELECT QCIssueMasterID FROM {TableNames.YARN_QC_RECEIVE_MASTER})
                    )
                    SELECT M.QCIssueMasterID, M.QCIssueNo, M.QCIssueDate, M.QCIssueBy, M.QCReqMasterID, 
                    M.Approve, M.Reject, M.LocationId, M.RCompanyId, M.CompanyId, M.SupplierId, M.SpinnerId,
                    IU.Name QCIssueByUser, RM.QCReqNo, RM.QCReqDate, RM.QCForID, RU.Name QCReqByUser, QCReqFor.ValueName QCReqFor ,
                    Status = CASE WHEN RM.RetestQCReqMasterID > 0 THEN 'Retest' ELSE '' END,
                    Count(*) Over() TotalRows
                    FROM M
                    Inner Join {DbNames.EPYSL}..LoginUser IU On IU.UserCode = M.QCIssueBy
                    Inner Join {TableNames.YARN_QC_REQ_MASTER} RM On RM.QCReqMasterID = M.QCReqMasterID
                    Inner Join {DbNames.EPYSL}..LoginUser RU On RU.UserCode = RM.QCReqBy 
                    Left Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On RM.QCForID = QCReqFor.ValueID";

                orderBy = "ORDER BY QCIssueMasterID DESC";

                //sql =
                //    $@"With 
                //    M As (
                //        Select QCReqMasterID, QCReqNo, QCReqDate, AddedBy,QCForID,QCReqBy, 
                //    LocationId, RCompanyId, CompanyId, SupplierId, SpinnerId From YarnQCReqMaster
                //    Where QCReqMasterID NOT IN (SELECT QCReqMasterID FROM YarnQCReceiveMaster)
                //    )
                //    SELECT M.QCReqMasterID, M.QCReqNo, M.QCReqDate, M.AddedBy,M.QCReqBy, 
                //    M.LocationId, M.RCompanyId, M.CompanyId, M.SupplierId, M.SpinnerId,
                //    IU.Name QCIssueByUser, M.QCForID, RU.Name QCReqByUser, QCReqFor.ValueName QCReqFor , Count(*) Over() TotalRows
                //    FROM M
                //    Inner Join {DbNames.EPYSL}..LoginUser IU On IU.UserCode = M.QCReqBy
                //    Inner Join {DbNames.EPYSL}..LoginUser RU On RU.UserCode = M.QCReqBy 
                //    Left Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On M.QCForID = QCReqFor.ValueID";

                //orderBy = "ORDER BY QCReqMasterID DESC";
            }
            //else if (status == Status.All)
            //{
            //    sql = $@";
            //    ;WITH RC AS
            //    (
            //     SELECT YRC.ReceiveID, IsNoTest 
            //     FROM YarnReceiveChild YRC
            //     WHERE YRC.IsNoTest = 1
            //     GROUP BY YRC.ReceiveID, IsNoTest
            //    ),
            //    FinalList AS
            //    (
            //     SELECT YRM.ReceiveID, QCRM.QCRemarksMasterID, QCR.QCReceiveMasterID, QCI.QCIssueMasterID, QCReq.QCReqMasterID, QCReq.IsApprove, 
            //     ItemMasterID = MAX(QRC.ItemMasterID), 
            //     QRC.ChallanLot, QCReq.QCReqNo, EU.EmployeeName QCReqByUser, QCReq.QCReqDate, QCReqFor.ValueName QCReqFor, 
            //     QCReq.IsAcknowledge, QCReq.PhysicalCount, QCReq.LotNo, RM.ChallanNo, RM.ChallanDate,RM.ReceiveNo, RM.ReceiveDate, 
            //     [Status] = CASE WHEN ISNULL(RC.IsNoTest,0) = 1 THEN 'No Test'
            //         WHEN ISNULL(QCRM.QCRemarksMasterID,0) > 0 THEN 'Remarks Done' 
            //         WHEN ISNULL(QCR.QCReceiveMasterID,0) > 0 THEN 'Waiting for Yarn Assessment'
            //         WHEN ISNULL(QCI.QCIssueMasterID,0) > 0 THEN 'Waiting for Yarn Receive'
            //         WHEN ISNULL(QCReq.IsApprove,0) = 1 THEN 'Waiting for Yarn Issue'
            //         WHEN ISNULL(QCReq.QCReqMasterID,0) > 0 THEN 'Waiting for Requisition Approval'
            //         ELSE 'Waiting for Requisition' END,
            //     ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, 
            //        ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc
            //     FROM YarnReceiveMaster YRM
            //     LEFT JOIN RC ON RC.ReceiveID = YRM.ReceiveID
            //     LEFT JOIN YarnQCRemarksMaster QCRM ON QCRM.ReceiveID = YRM.ReceiveID
            //     LEFT JOIN YarnQCReceiveMaster QCR ON QCR.ReceiveID = YRM.ReceiveID
            //     LEFT JOIN YarnQCIssueMaster QCI ON QCI.ReceiveID = YRM.ReceiveID
            //     LEFT JOIN YarnQCReqMaster QCReq ON QCReq.ReceiveID = YRM.ReceiveID
            //     LEFT JOIN YarnReceiveMaster RM ON RM.ReceiveID = QCReq.ReceiveID
            //     LEFT JOIN YarnQCReqChild QRC On QRC.QCReqMasterID=QCReq.QCReqMasterID
            //     LEFT Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On QCReq.QCForID = QCReqFor.ValueID
            //                     LEFT Join {DbNames.EPYSL}..LoginUser U On QCReq.QCReqBy = U.UserCode
            //         LEFT JOIN {DbNames.EPYSL}..Employee EU ON EU.EmployeeCode = U.EmployeeCode
            //     INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = QRC.ItemMasterID 
            //        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
            //        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
            //        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
            //        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
            //        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
            //        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
            //        LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID 
            //     Group By YRM.ReceiveID, QCRM.QCRemarksMasterID, QCR.QCReceiveMasterID, QCI.QCIssueMasterID, QCReq.QCReqMasterID, QCReq.IsApprove, 
            //     QRC.ChallanLot, QCReq.QCReqNo, EU.EmployeeName, QCReq.QCReqDate, QCReqFor.ValueName, 
            //     QCReq.IsAcknowledge, QCReq.PhysicalCount, QCReq.LotNo, RM.ChallanNo, RM.ChallanDate,RM.ReceiveNo, RM.ReceiveDate, RC.IsNoTest,
            //     ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue, 
            //     ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue
            //    )
            //    SELECT *,Count(*) Over() TotalRows FROM FinalList 

            //    ";

            //    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By QCReqMasterID Desc" : paginationInfo.OrderBy;
            //}
            else
            {
                sql = $@"
                With 
                M As (
                     Select QCReceiveMasterID, QCReceiveNo, QCReceivedBy, QCReqMasterID, QCIssueMasterID,
					CONVERT(DATETIME, CONVERT(CHAR(8), QCReceiveDate, 112) + ' ' + CONVERT(CHAR(8), DateAdded, 108)) QCReceiveDate,
                   LocationId, RCompanyId, CompanyId, SupplierId, SpinnerId From {TableNames.YARN_QC_RECEIVE_MASTER}
                     )
                SELECT M.QCReceiveMasterID, M.QCReceiveNo, M.QCReceivedBy, M.QCReceiveDate, M.QCReqMasterID, M.QCIssueMasterID, 
                 M.LocationId, M.RCompanyId, M.CompanyId, M.SupplierId, M.SpinnerId, IM.QCIssueNo,
				CONVERT(DATETIME, CONVERT(CHAR(8), IM.QCIssueDate, 112) + ' ' + CONVERT(CHAR(8), IM.DateAdded, 108)) QCIssueDate, ER.EmployeeName QCReceivedByUser,
                EU.EmployeeName QCIssueByUser, RM.QCReqNo, RM.QCReqDate, RM.QCForID, ERU.EmployeeName QCReqByUser, QCReqFor.ValueName QCReqFor ,
                Status = CASE WHEN RM.RetestQCReqMasterID > 0 THEN 'Retest' ELSE '' END,
                Count(*) Over() TotalRows
                FROM M
                Inner Join {TableNames.YARN_QC_REQ_MASTER} RM On RM.QCReqMasterID = M.QCReqMasterID
                Inner Join {TableNames.YARN_QC_ISSUE_MASTER} IM On IM.QCIssueMasterID = M.QCIssueMasterID
                Inner Join {DbNames.EPYSL}..LoginUser IR On IR.UserCode = M.QCReceivedBy
				Inner Join {DbNames.EPYSL}..Employee ER On ER.EmployeeCode=IR.EmployeeCode
                Inner Join {DbNames.EPYSL}..LoginUser IU On IU.UserCode = IM.QCIssueBy
				Inner Join {DbNames.EPYSL}..Employee EU On EU.EmployeeCode=IU.EmployeeCode
                Inner Join {DbNames.EPYSL}..LoginUser RU On RU.UserCode = RM.QCReqBy 
				Inner Join {DbNames.EPYSL}..Employee ERU On ERU.EmployeeCode=RU.EmployeeCode
                Left Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On RM.QCForID = QCReqFor.ValueID";
                orderBy = string.IsNullOrEmpty(orderBy) ? "ORDER BY QCReceiveMasterID DESC" : orderBy;
            }

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<YarnQCReceiveMaster>(sql);
        }
        public async Task<YarnQCReceiveMaster> GetNewAsync(int qcIssueMasterId)
        //public async Task<YarnQCReceiveMaster> GetNewAsync(int QCReqMasterId)
        {
            var query =
                $@"
                With 
                M As (
                    Select * From {TableNames.YARN_QC_ISSUE_MASTER} Where QCIssueMasterID = {qcIssueMasterId}
                )
                SELECT M.QCIssueMasterID, M.QCIssueNo, M.QCIssueDate, M.QCIssueBy, M.QCReqMasterID, 
                M.Approve, M.Reject, M.LocationId, M.RCompanyId, M.CompanyId, M.SupplierId, M.SpinnerId,
                IU.Name QCIssueByUser, RM.QCReqNo, RM.QCReqDate, RM.QCForID, RU.Name QCReqByUser, QCReqFor.ValueName QCReqFor,RM.ReceiveID
                FROM M
                Inner Join {DbNames.EPYSL}..LoginUser IU On IU.UserCode = M.QCIssueBy
                Inner Join {TableNames.YARN_QC_REQ_MASTER} RM On RM.QCReqMasterID = M.QCReqMasterID
                Inner Join {DbNames.EPYSL}..LoginUser RU On RU.UserCode = RM.QCReqBy 
                Left Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On RM.QCForID = QCReqFor.ValueID;

                --Child
                ;Select YRC.QCIssueChildID, YRC.QCIssueMasterID, YRC.ReceiveChildID, YRC.LotNo, YRC.ChallanLot, YRC.ReqQty, YRC.ReqQtyCone, YRC.ReqQtyCarton, YRC.IssueQty, YRC.IssueQtyCone, YRC.IssueQtyCarton,
                YRC.ItemMasterID, YRC.UnitID, YRC.Rate,YRC.YarnProgramId, YRC.ChallanCount, YRC.POCount, YRC.PhysicalCount, YRC.YarnCategory, YRC.NoOfThread,
                UU.DisplayUnitDesc Uom, YRC.IssueQty ReceiveQty, YRC.IssueQtyCone ReceiveQtyCone, YRC.IssueQtyCarton ReceiveQtyCarton,ISV1.SegmentValue Segment1ValueDesc,
                ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc,ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc,
                ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,YRC.ShadeCode,YC.YarnCategory, SS.[Name] Spinner,
                Status = CASE WHEN YQRM.RetestQCReqMasterID > 0 THEN 'Retest' ELSE 'New' END, YQRC.QCReqRemarks
                From {TableNames.YARN_QC_ISSUE_CHILD} YRC
				LEFT JOIN {TableNames.YARN_RECEIVE_CHILD} YC ON YC.ChildID=YRC.ReceiveChildID
				LEFT JOIN {TableNames.YARN_QC_REQ_CHILD} YQRC ON YQRC.QCReqChildID=YRC.QCReqChildID
				LEFT JOIN {TableNames.YARN_QC_REQ_MASTER} YQRM ON YQRM.QCReqMasterID=YQRC.QCReqMasterID
                LEFT JOIN {DbNames.EPYSL}..Contacts SS ON SS.ContactID = YC.SpinnerID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YRC.ItemMasterID 
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT Join {DbNames.EPYSL}..Unit AS UU On UU.UnitID = YRC.UnitID
                Where YRC.QCIssueMasterID = {qcIssueMasterId}
                Group By YRC.QCIssueChildID, YRC.QCIssueMasterID, YRC.ReceiveChildID, YRC.LotNo, YRC.ChallanLot, YRC.ReqQty, YRC.ReqQtyCone, YRC.ReqQtyCarton, YRC.IssueQty, YRC.IssueQtyCone, 
                YRC.IssueQtyCarton,YRC.ItemMasterID, YRC.UnitID, YRC.Rate,YRC.YarnProgramId, YRC.ChallanCount, YRC.POCount, YRC.PhysicalCount, YRC.YarnCategory, 
                YRC.NoOfThread, UU.DisplayUnitDesc,ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue, ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue, 
                ISV7.SegmentValue,YRC.ShadeCode,YRC.ReqQty, YRC.ReqQtyCone, YRC.ReqQtyCarton, YC.YarnCategory, SS.[Name],YQRM.RetestQCReqMasterID, YQRC.QCReqRemarks

                --Machine Type
                ;SELECT CAST(a.MachineSubClassID AS varchar) [id], b.SubClassName [text], b.TypeID [desc], c.TypeName additionalValue
                FROM {TableNames.KNITTING_MACHINE} a
                INNER JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} b ON b.SubClassID = a.MachineSubClassID
                Inner Join {TableNames.KNITTING_MACHINE_TYPE} c On c.TypeID = b.TypeID
                --Where c.TypeName != 'Flat Bed'
                GROUP BY a.MachineSubClassID, b.SubClassName, b.TypeID, c.TypeName;

                 --Technical Name
                SELECT Cast(T.TechnicalNameId As varchar) id, T.TechnicalName [text], ISNULL(ST.[Days], 0) [desc], Cast(SC.SubClassID as varchar) additionalValue
                FROM {TableNames.FabricTechnicalName} T
                LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME_KMACHINE_SUB_CLASS} SC ON SC.TechnicalNameID = T.TechnicalNameId
                LEFT JOIN {TableNames.KNITTING_MACHINE_STRUCTURE_TYPE_HK} ST ON ST.StructureTypeID = SC.StructureTypeID
                Group By T.TechnicalNameId, T.TechnicalName, ST.Days, SC.SubClassID;

                -- Buyers
                 {CommonQueries.GetContactsByCategoryType(ContactCategoryNames.BUYER)};";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                YarnQCReceiveMaster data = await records.ReadFirstOrDefaultAsync<YarnQCReceiveMaster>();
                Guard.Against.NullObject(data);
                data.YarnQCReceiveChilds = records.Read<YarnQCReceiveChild>().ToList();
                data.MCTypeForFabricList = records.Read<Select2OptionModel>().ToList();
                data.TechnicalNameList = records.Read<Select2OptionModel>().ToList();
                data.BuyerList = records.Read<Select2OptionModel>().ToList();
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

        public async Task<YarnQCReceiveMaster> GetAsync(int id)
        {
            var query =
                $@"
                With 
                M As (
                    Select * From {TableNames.YARN_QC_RECEIVE_MASTER} WHERE QCReceiveMasterID = {id}
                )
                SELECT M.QCReceiveMasterID, M.QCReceiveNo, M.QCReceivedBy, M.QCReceiveDate, M.QCReqMasterID, M.QCIssueMasterID, 
                M.LocationId, M.RCompanyId, M.CompanyId, M.SupplierId, M.SpinnerId, IM.QCIssueNo, IM.QCIssueDate, IU.Name QCReceivedByUser,
                IU.Name QCIssueByUser, RM.QCReqNo, RM.QCReqDate, RM.QCForID, RU.Name QCReqByUser, QCReqFor.ValueName QCReqFor,M.ReceiveID
                FROM M
                Inner Join {TableNames.YARN_QC_REQ_MASTER} RM On RM.QCReqMasterID = M.QCReqMasterID
                Inner Join {TableNames.YARN_QC_ISSUE_MASTER} IM On IM.QCIssueMasterID = M.QCIssueMasterID
                Inner Join {DbNames.EPYSL}..LoginUser IR On IR.UserCode = M.QCReceivedBy
                Inner Join {DbNames.EPYSL}..LoginUser IU On IU.UserCode = IM.QCIssueBy
                Inner Join {DbNames.EPYSL}..LoginUser RU On RU.UserCode = RM.QCReqBy 
                Left Join {DbNames.EPYSL}..EntityTypeValue QCReqFor On RM.QCForID = QCReqFor.ValueID;

                --Childs
                ;Select YRC.QCReceiveChildID, YRC.QCReceiveMasterID, YRC.ReceiveChildID, YRC.LotNo, YRC.ChallanLot, YRC.ReqQty, YRC.ReqQtyCone, YRC.ReqQtyCarton, YRC.IssueQty, YRC.IssueQtyCone, YRC.IssueQtyCarton,
                YRC.ReceiveQty, YRC.ReceiveQtyCone, YRC.ReceiveQtyCarton, YRC.UnitID, YRC.ItemMasterID, YRC.Rate, YRC.YarnProgramId, YRC.ChallanCount, YRC.POCount, YRC.PhysicalCount,
                YRC.YarnCategory, YRC.NoOfThread, UU.DisplayUnitDesc Uom,ISV1.SegmentValue Segment1ValueDesc,ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc,
                ISV4.SegmentValue Segment4ValueDesc, ISV5.SegmentValue Segment5ValueDesc,ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc,YRC.ShadeCode, SS.[Name] Spinner,
                Status = CASE WHEN YQRM.RetestQCReqMasterID > 0 THEN 'Retest' ELSE 'New' END, RCC.QCReqRemarks
                From {TableNames.YARN_QC_RECEIVE_CHILD} YRC
                LEFT JOIN {TableNames.YARN_RECEIVE_CHILD} YC ON YC.ChildID=YRC.ReceiveChildID
                LEFT JOIN {TableNames.YARN_QC_RECEIVE_MASTER} YQRC ON YQRC.QCReceiveMasterID=YRC.QCReceiveMasterID
                INNER JOIN {TableNames.YARN_QC_ISSUE_CHILD} IC ON IC.QCIssueChildID = YRC.QCIssueChildID
                INNER JOIN {TableNames.YARN_QC_REQ_CHILD} RCC ON RCC.QCReqChildID = IC.QCReqChildID
                INNER JOIN {TableNames.YARN_QC_REQ_MASTER} YQRM ON YQRM.QCReqMasterID = RCC.QCReqMasterID
                LEFT JOIN {DbNames.EPYSL}..Contacts SS ON SS.ContactID = YC.SpinnerID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YRC.ItemMasterID 
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT Join {DbNames.EPYSL}..Unit AS UU On UU.UnitID = YRC.UnitID
                Where YRC.QCReceiveMasterID = {id}
                Group By YRC.QCReceiveChildID, YRC.QCReceiveMasterID, YRC.ReceiveChildID, YRC.LotNo, YRC.ChallanLot, YRC.ReqQty, YRC.ReqQtyCone, YRC.ReqQtyCarton, YRC.IssueQty, YRC.IssueQtyCone, YRC.IssueQtyCarton,
                YRC.ReceiveQty, YRC.ReceiveQtyCone, YRC.ReceiveQtyCarton, YRC.UnitID, YRC.ItemMasterID, YRC.Rate, YRC.YarnProgramId, YRC.ChallanCount, YRC.POCount, YRC.PhysicalCount,
                YRC.YarnCategory, YRC.NoOfThread, UU.DisplayUnitDesc,ISV1.SegmentValue,ISV2.SegmentValue, ISV3.SegmentValue ,ISV4.SegmentValue , ISV5.SegmentValue ,
                ISV6.SegmentValue , ISV7.SegmentValue ,YRC.ShadeCode, SS.[Name],YQRM.RetestQCReqMasterID,RCC.QCReqRemarks;

                --Machine Type
                ;SELECT CAST(a.MachineSubClassID AS varchar) [id], b.SubClassName [text], b.TypeID [desc], c.TypeName additionalValue
                FROM {TableNames.KNITTING_MACHINE} a
                INNER JOIN {TableNames.KNITTING_MACHINE_SUBCLASS} b ON b.SubClassID = a.MachineSubClassID
                Inner Join {TableNames.KNITTING_MACHINE_TYPE} c On c.TypeID = b.TypeID
                --Where c.TypeName != 'Flat Bed'
                GROUP BY a.MachineSubClassID, b.SubClassName, b.TypeID, c.TypeName;

                 --Technical Name
                SELECT Cast(T.TechnicalNameId As varchar) id, T.TechnicalName [text], ISNULL(ST.[Days], 0) [desc], Cast(SC.SubClassID as varchar) additionalValue
                FROM {TableNames.FabricTechnicalName} T
                LEFT JOIN {TableNames.FABRIC_TECHNICAL_NAME_KMACHINE_SUB_CLASS} SC ON SC.TechnicalNameID = T.TechnicalNameId
                LEFT JOIN {TableNames.KNITTING_MACHINE_STRUCTURE_TYPE_HK} ST ON ST.StructureTypeID = SC.StructureTypeID
                Group By T.TechnicalNameId, T.TechnicalName, ST.Days, SC.SubClassID;

                -- Buyers
                 {CommonQueries.GetContactsByCategoryType(ContactCategoryNames.BUYER)};";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                YarnQCReceiveMaster data = await records.ReadFirstOrDefaultAsync<YarnQCReceiveMaster>();
                Guard.Against.NullObject(data);
                data.YarnQCReceiveChilds = records.Read<YarnQCReceiveChild>().ToList();
                data.MCTypeForFabricList = records.Read<Select2OptionModel>().ToList();
                data.TechnicalNameList = records.Read<Select2OptionModel>().ToList();
                data.BuyerList = records.Read<Select2OptionModel>().ToList();
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

        public async Task<YarnQCReceiveMaster> GetAllAsync(int id)
        {
            var sql = $@"
            ;Select * From {TableNames.YARN_QC_RECEIVE_MASTER} Where QCReceiveMasterID = {id}

            ;Select * From {TableNames.YARN_QC_RECEIVE_CHILD} Where QCReceiveMasterID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnQCReceiveMaster data = await records.ReadFirstOrDefaultAsync<YarnQCReceiveMaster>();
                Guard.Against.NullObject(data);
                data.YarnQCReceiveChilds = records.Read<YarnQCReceiveChild>().ToList();
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

        public async Task SaveAsync(YarnQCReceiveMaster entity)
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
                        entity.QCReceiveMasterID = await _service.GetMaxIdAsync(TableNames.YARN_QC_RECEIVE_MASTER);
                        entity.QCReceiveNo = await _service.GetMaxNoAsync(TableNames.YARN_QC_RECEIVE_NO, 1, RepeatAfterEnum.NoRepeat, "00000", transactionGmt, _connectionGmt);

                        maxChildId = await _service.GetMaxIdAsync(TableNames.YARN_QC_RECEIVE_CHILD, entity.YarnQCReceiveChilds.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        foreach (YarnQCReceiveChild item in entity.YarnQCReceiveChilds)
                        {
                            item.QCReceiveChildID = maxChildId++;
                            item.QCReceiveMasterID = entity.QCReceiveMasterID;
                        }
                        break;

                    case EntityState.Modified:
                        var addedChilds = entity.YarnQCReceiveChilds.FindAll(x => x.EntityState == EntityState.Added);
                        maxChildId = await _service.GetMaxIdAsync(TableNames.YARN_QC_RECEIVE_CHILD, addedChilds.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        foreach (YarnQCReceiveChild item in addedChilds)
                        {
                            item.QCReceiveChildID = maxChildId++;
                            item.QCReceiveMasterID = entity.QCReceiveMasterID;
                        }
                        break;

                    case EntityState.Unchanged:
                    case EntityState.Deleted:
                        entity.EntityState = EntityState.Deleted;
                        entity.YarnQCReceiveChilds.SetDeleted();
                        break;

                    default:
                        break;
                }

                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.YarnQCReceiveChilds, transaction);

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
