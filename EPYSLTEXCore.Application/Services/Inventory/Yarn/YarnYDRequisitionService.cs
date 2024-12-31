using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using System.Data.Entity;
using Microsoft.Data.SqlClient;
using EPYSLTEXCore.Application.Interfaces;

namespace EPYSLTEXCore.Application.Services.Inventory
{
    public class YarnYDRequisitionService : IYarnYDRequisitionService
    {
        private readonly IDapperCRUDService<YarnYDReqMaster> _service;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;
        private readonly IItemMasterService<YarnYDReqChild> _itemMasterRepository;
        public YarnYDRequisitionService(IDapperCRUDService<YarnYDReqMaster> service,
            IItemMasterService<YarnYDReqChild> itemMasterRepository)
        {
            _service = service;
            _service = service;
            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
            _itemMasterRepository = itemMasterRepository;
        }

        public async Task<List<YarnYDReqMaster>> GetPagedAsync(Status status, string pageName, PaginationInfo paginationInfo)
        {
            string orderBy = "";
            int IsSendForApprove = -1, IsApprove = -1, IsAcknowledge = -1, IsReject = -1;

            if (pageName == "YDYarnRequisition")
            {
                if (status == Status.Pending) //Requisition List
                {
                    IsSendForApprove = 0;
                }
                else if (status == Status.AwaitingPropose) //Pending for Approval, Pending Approval
                {
                    IsSendForApprove = 1;
                    IsApprove = 0;
                    IsReject = 0;
                }
                else if (status == Status.Approved) //Approve List, YD Yarn Requisiton
                {
                    IsApprove = 1;
                    IsAcknowledge = 0;
                }
                else if (status == Status.Reject) //Reject List
                {
                    IsReject = 1;
                }
            }
            else if (pageName == "YDYarnApprove")
            {
                if (status == Status.AwaitingPropose) //Pending for Approval, Pending Approval
                {
                    IsSendForApprove = 1;
                    IsApprove = 0;
                    IsReject = 0;
                }
                else if (status == Status.Approved) //Approve List, YD Yarn Requisiton
                {
                    IsApprove = 1;
                    IsAcknowledge = 0;
                }
                else if (status == Status.Reject) //Reject List
                {
                    IsReject = 1;
                }
            }
            else if (pageName == "YDYarnAcknowledgement")
            {
                if (status == Status.Approved) //Approve List, YD Yarn Requisiton
                {
                    IsApprove = 1;
                    IsAcknowledge = 0;
                }
                if (status == Status.Acknowledge) //Acknowledgement List
                {
                    IsApprove = 1;
                    IsAcknowledge = 1;
                }
            }

            string sql;
            if (status == Status.Proposed)
            {
                sql =
                $@"
               ;With A AS 
                ( 
                    Select YBM.YDBookingMasterID, YBM.YDBookingNo, YBM.YDBookingDate, FM.ConceptNo,YBM.ConceptID, YBM.Remarks, 
                    YBM.TotalBookingQty
                    FROM {TableNames.YD_BOOKING_MASTER} YBM Inner JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FM ON FM.ConceptID = YBM.ConceptID
                    Left JOIN {TableNames.YARN_YD_REQ_MASTER} RM ON RM.YDBookingMasterID = YBM.YDBookingMasterID
                    where YBM.IsAcknowledge = 1 AND RM.YDBookingMasterID IS NULL
                )
                Select *, Count(*) Over() TotalRows From A ";

                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YDBookingMasterID Desc" : paginationInfo.OrderBy;
            }
            else
            {
                sql = $@"
                ;With
                YRM As 
                (
	                Select *
	                FROM {TableNames.YARN_YD_REQ_MASTER} 
	                Where IsSendForApprove = (Case when {IsSendForApprove} = -1 Then IsSendForApprove Else {IsSendForApprove} End)
	                AND IsApprove = (Case when {IsApprove} = -1 Then IsApprove Else {IsApprove} End) 
	                AND IsAcknowledge = (Case when {IsAcknowledge} = -1 Then IsAcknowledge Else {IsAcknowledge} End) 
	                AND IsReject = (Case when {IsReject} = -1 Then IsReject Else {IsReject} End) 
                )
                Select YRM.YDReqMasterID, YRM.YDBookingMasterID, YRM.YDReqNo, YRM.YDReqDate, YRM.ReqFromID, C.ShortName AS BuyerName, YRM.Remarks, 
                YRM.IsSendForApprove, RL.Name As SendForApproveName,  YRM.SendForApproveDate, YRM.IsApprove, A.Name As ApproveName, YRM.ApproveDate, 
                YRM.IsAcknowledge, AcW.Name As AcknowledgeName, YRM.AcknowledgeDate, YRM.IsReject, R.Name As RejectName, YRM.RejectDate, YRM.AddedBy,
                Sum(YRC.RequiredQty)RequiredQty, Sum(YRC.RequsitionQty)RequsitionQty
                From YRM
                Inner JOIN {TableNames.YARN_YD_REQ_CHILD} YRC ON YRC.YDReqMasterID = YRM.YDReqMasterID 
                Left join  {DbNames.EPYSL}..Contacts C ON c.ContactID = YRM.ReqFromID 
                LEFT Join  {DbNames.EPYSL}..LoginUser RL On RL.UserCode = YRM.SendForApproveBy 
                LEFT Join  {DbNames.EPYSL}..LoginUser A On A.UserCode = YRM.ApproveBy
                LEFT Join  {DbNames.EPYSL}..LoginUser AcW On AcW.UserCode = YRM.AcknowledgeBy
                LEFT Join  {DbNames.EPYSL}..LoginUser R On R.UserCode = YRM.RejectBy 
                Group By YRM.YDReqMasterID, YRM.YDBookingMasterID, YRM.YDReqNo, YRM.YDReqDate, YRM.ReqFromID, C.ShortName, YRM.Remarks, YRM.IsSendForApprove, RL.Name,
                YRM.SendForApproveDate, YRM.IsApprove, A.Name, YRM.ApproveDate, YRM.IsAcknowledge, AcW.Name, YRM.AcknowledgeDate, YRM.IsReject, 
                R.Name, YRM.RejectDate, YRM.AddedBy";

                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YRM.YDReqMasterID Desc" : paginationInfo.OrderBy;
            }
            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<YarnYDReqMaster>(sql);
        }
        public async Task<YarnYDReqMaster> GetNewAsync(int YDBookingMasterID)
        {
            var segmentNames = new
            {
                SegmentNames = new string[]
               {
                    ItemSegmentNameConstants.FABRIC_COLOR
               }
            };

            var sql =
                $@"
                SELECT DISTINCT YDBM.YDBookingMasterID, YDBM.YDBookingDate, YDBM.BuyerID, C.ShortName BuyerName, FCM.CompanyID As ReqFromID
                FROM {TableNames.YD_BOOKING_MASTER} YDBM LEFT JOIN {DbNames.EPYSL}..Contacts C ON YDBM.BuyerID=C.ContactID
                Inner JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FCM on FCM.ConceptID = YDBM.ConceptID 
                WHERE YDBM.YDBookingMasterID = {YDBookingMasterID};
 
                -- Child
                ;With 
                YDBC As (
	                Select * FROM {TableNames.YDBookingChild} Where YDBookingMasterID = {YDBookingMasterID}
                )

                SELECT YDBC.YDBookingChildID, YDBC.YDBookingMasterID, YDBC.ItemMasterID, YDBC.ShadeCode, YDBC.NoOfThread, YDBC.IsAdditionalItem, YDBC.ColorId As ColorID, 
                YDBC.ColorCode, Color.SegmentValue AS ColorName, YDBC.BookingFor, YDF.YDyeingFor As BookingForName, YDBC.IsTwisting,YDBC.IsWaxing, YDBC.BookingQty As RequiredQty, YDBC.NoOfCone, YDBC.UnitId As UnitID, 
                'Kg' As DisplayUnitDesc, YDBC.Remarks, 
                IM.Segment1ValueID Segment1ValueId, IM.Segment2ValueID Segment2ValueId, IM.Segment3ValueID Segment3ValueId, IM.Segment4ValueID Segment4ValueId,
                IM.Segment5ValueID Segment5ValueId, IM.Segment6ValueID Segment6ValueId, IM.Segment7ValueID Segment7ValueId, IM.Segment8ValueID Segment8ValueId,
                IM.Segment9ValueID Segment9ValueId, IM.Segment10ValueID Segment10ValueId, IM.Segment11ValueID Segment11ValueId, IM.Segment12ValueID Segment12ValueId,
                IM.Segment13ValueID Segment13ValueId, IM.Segment14ValueID Segment14ValueId, IM.Segment15ValueID Segment15ValueId,
                ISV1.SegmentValue AS Segment1ValueDesc, ISV2.SegmentValue AS Segment2ValueDesc, ISV3.SegmentValue AS Segment3ValueDesc,
                ISV4.SegmentValue AS Segment4ValueDesc, ISV5.SegmentValue AS Segment5ValueDesc, ISV6.SegmentValue AS Segment6ValueDesc,
                ISV7.SegmentValue AS Segment7ValueDesc, ISV8.SegmentValue AS Segment8ValueDesc  
                FROM YDBC 
                LEFT JOIN {TableNames.DyeingProcessPart_HK} DP ON DP.DPID = YDBC.DPID
                INNER JOIN {DbNames.EPYSL}..ItemSegmentValue Color ON Color.SegmentValueID = YDBC.ColorId
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YDBC.ItemMasterID

                Left JOIN {TableNames.YarnDyeingFor_HK} YDF On YDF.YDyeingForID = YDBC.BookingFor
                LEFT Join {DbNames.EPYSL}..EntityTypeValue EV On YDBC.YarnProgramId = EV.ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID
                LEFT JOIN {DbNames.EPYSL}..Unit UN ON YDBC.UnitID = UN.UnitID;

                -- CompanyList
                Select Cast (CE.CompanyID as varchar) [id] , C.Name + '(' + C.ShortName + ')' [text]
                From (select SubGroupID, ContactID from {DbNames.EPYSL}..SupplierItemGroupStatus Group By SubGroupID, ContactID) SIGS
                Inner Join {DbNames.EPYSL}..Contacts C On SIGS.ContactID = C.ContactID
                Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SIGS.SubGroupID
                Inner Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = SIGS.ContactID
                Inner Join {DbNames.EPYSL}..CompanyEntity CE On C.MappingCompanyID = CE.CompanyID
                Where ISG.SubGroupName = '{AppConstants.ITEM_SUB_GROUP_FABRIC}' And Isnull(CAI.InHouse,0) = 1
                Group by CE.CompanyID, C.Name, C.ShortName; 

                /*-- Item Segments (Color)
                {CommonQueries.GetItemSegmentValuesBySegmentNamesWithSegmentName()};

                -- YarnDyeingFor (Booking For)
                {CommonQueries.GetYarnDyeingFor()}; */ 

";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql, segmentNames);
                YarnYDReqMaster data = await records.ReadFirstOrDefaultAsync<YarnYDReqMaster>();
                data.Childs = records.Read<YarnYDReqChild>().ToList();
                data.ReqFromList = await records.ReadAsync<Select2OptionModel>();
                //data.YarnColorList = await records.ReadAsync<Select2OptionModel>();
                //data.YarnDyeingForList = await records.ReadAsync<Select2OptionModel>();
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
        public async Task<YarnYDReqMaster> GetAsync(int id)
        {
            var segmentNames = new
            {
                SegmentNames = new string[]
               {
                    ItemSegmentNameConstants.FABRIC_COLOR
               }
            };

            var query =
                $@"
                Select YRM.YDReqMasterID, YRM.YDBookingMasterID, YRM.YDReqNo, YRM.YDReqDate, YRM.ReqFromID, C.ShortName, YRM.Remarks 
                FROM {TableNames.YARN_YD_REQ_MASTER} YRM Left join  {DbNames.EPYSL}..Contacts C ON c.ContactID = YRM.ReqFromID  
                Where YRM.YDReqMasterID = {id};

                ;With
                YRC As 
                (
	                Select * FROM {TableNames.YARN_YD_REQ_CHILD} Where YDReqMasterID = {id}
                )
                Select YRC.YDReqChildID, YRC.YDBookingChildID, YRC.YDReqMasterID, YRC.YarnProgramID, YRC.ItemMasterID, YRC.PhysicalCount, YRC.Remarks, YRC.NoOfThread, 
                YRC.UnitId, YRC.DisplayUnitDesc, YRC.RequiredQty, YRC.RequsitionQty, YRC.NoOfCone, YRC.ColorId, YRC.ColorCode, YRC.BookingFor, 
                YRC.IsTwisting, YRC.IsWaxing, YRC.ShadeCode, YRC.IsAdditionalItem,
                IM.Segment1ValueID Segment1ValueId, IM.Segment2ValueID Segment2ValueId, IM.Segment3ValueID Segment3ValueId, IM.Segment4ValueID Segment4ValueId,
                IM.Segment5ValueID Segment5ValueId, IM.Segment6ValueID Segment6ValueId, IM.Segment7ValueID Segment7ValueId, IM.Segment8ValueID Segment8ValueId,
                IM.Segment9ValueID Segment9ValueId, IM.Segment10ValueID Segment10ValueId, IM.Segment11ValueID Segment11ValueId, IM.Segment12ValueID Segment12ValueId,
                IM.Segment13ValueID Segment13ValueId, IM.Segment14ValueID Segment14ValueId, IM.Segment15ValueID Segment15ValueId,
                ISV1.SegmentValue AS Segment1ValueDesc, ISV2.SegmentValue AS Segment2ValueDesc, ISV3.SegmentValue AS Segment3ValueDesc,
                ISV4.SegmentValue AS Segment4ValueDesc, ISV5.SegmentValue AS Segment5ValueDesc, ISV6.SegmentValue AS Segment6ValueDesc,
                ISV7.SegmentValue AS Segment7ValueDesc, ISV8.SegmentValue AS Segment8ValueDesc 
                From YRC LEFT JOIN   {DbNames.EPYSL}..Unit U ON U.UnitID = YRC.UnitID 
                INNER JOIN  {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YRC.ItemMasterID
                LEFT JOIN   {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN   {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN   {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN   {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN   {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN   {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN   {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT JOIN   {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID
                Group By YRC.YDReqChildID, YRC.YDBookingChildID, YRC.YDReqMasterID, YRC.YarnProgramID, YRC.ItemMasterID, YRC.PhysicalCount, YRC.Remarks, YRC.NoOfThread, 
                YRC.UnitId, YRC.DisplayUnitDesc, YRC.RequiredQty, YRC.RequsitionQty, YRC.NoOfCone, YRC.ColorId, YRC.ColorCode, 
                YRC.BookingFor, YRC.IsTwisting, YRC.IsWaxing, YRC.ShadeCode, YRC.IsAdditionalItem,
                IM.Segment1ValueID, IM.Segment2ValueID, IM.Segment3ValueID, 
                IM.Segment4ValueID, IM.Segment5ValueID , IM.Segment6ValueID , IM.Segment7ValueID , IM.Segment8ValueID ,
                IM.Segment9ValueID , IM.Segment10ValueID , IM.Segment11ValueID , IM.Segment12ValueID , IM.Segment13ValueID , IM.Segment14ValueID , 
                IM.Segment15ValueID, ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue, ISV4.SegmentValue, ISV5.SegmentValue, ISV6.SegmentValue,
                ISV7.SegmentValue, ISV8.SegmentValue; 

                -- CompanyList
                Select Cast (CE.CompanyID as varchar) [id] , C.Name + '(' + C.ShortName + ')' [text]
                From (select SubGroupID, ContactID from {DbNames.EPYSL}..SupplierItemGroupStatus Group By SubGroupID, ContactID) SIGS
                Inner Join {DbNames.EPYSL}..Contacts C On SIGS.ContactID = C.ContactID
                Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SIGS.SubGroupID
                Inner Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = SIGS.ContactID
                Inner Join {DbNames.EPYSL}..CompanyEntity CE On C.MappingCompanyID = CE.CompanyID
                Where ISG.SubGroupName = '{AppConstants.ITEM_SUB_GROUP_FABRIC}' And Isnull(CAI.InHouse,0) = 1
                Group by CE.CompanyID, C.Name, C.ShortName; 


                /*-- Item Segments (Color)
                {CommonQueries.GetItemSegmentValuesBySegmentNamesWithSegmentName()};

                -- YarnDyeingFor (Booking For)
                {CommonQueries.GetYarnDyeingFor()}; */
";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query, segmentNames);

                YarnYDReqMaster data = records.Read<YarnYDReqMaster>().FirstOrDefault();
                data.Childs = records.Read<YarnYDReqChild>().ToList();
                data.ReqFromList = await records.ReadAsync<Select2OptionModel>();
                //data.YarnColorList = await records.ReadAsync<Select2OptionModel>();
                //data.YarnDyeingForList = await records.ReadAsync<Select2OptionModel>(); 

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
        public async Task<YarnYDReqMaster> GetAllAsync(int id)
        {
            var sql = $@"
            ;Select * FROM {TableNames.YARN_YD_REQ_MASTER} Where YDReqMasterID = {id}

            ;Select * FROM {TableNames.YARN_YD_REQ_CHILD} Where YDReqMasterID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnYDReqMaster data = await records.ReadFirstOrDefaultAsync<YarnYDReqMaster>();
                Guard.Against.NullObject(data);
                data.Childs = records.Read<YarnYDReqChild>().ToList();
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
        public async Task SaveAsync(YarnYDReqMaster entity)
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
                List<YarnYDReqChild> childRecords = entity.Childs;
                _itemMasterRepository.GenerateItem(AppConstants.ITEM_SUB_GROUP_YARN_NEW, ref childRecords);

                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity.YDReqMasterID = await _service.GetMaxIdAsync(TableNames.YARN_YD_REQ_MASTER, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        entity.YDReqNo = await _service.GetMaxNoAsync(TableNames.YARN_YD_REQ_MASTER, 1, RepeatAfterEnum.NoRepeat, "00000", transactionGmt, _connectionGmt);

                        maxChildId = await _service.GetMaxIdAsync(TableNames.YARN_YD_REQ_CHILD, entity.Childs.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        foreach (var item in entity.Childs)
                        {
                            item.YDReqChildID = maxChildId++;
                            item.YDReqMasterID = entity.YDReqMasterID;
                        }
                        break;

                    case EntityState.Modified:
                        var addedChilds = entity.Childs.FindAll(x => x.EntityState == EntityState.Added);
                        maxChildId = await _service.GetMaxIdAsync(TableNames.YARN_YD_REQ_CHILD, addedChilds.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        foreach (var item in addedChilds)
                        {
                            item.YDReqChildID = maxChildId++;
                            item.YDReqMasterID = entity.YDReqMasterID;
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
        public async Task UpdateEntityAsync(YarnYDReqMaster entity)
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
    }
}
