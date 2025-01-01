using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using System.Data;
using System.Data.Entity;
using Microsoft.Data.SqlClient;
using EPYSLTEXCore.Application.Interfaces;

namespace EPYSLTEXCore.Application.Services.Inventory
{
    public class YDReqIssueService : IYDReqIssueService
    {
        private readonly IDapperCRUDService<YDReqIssueMaster> _service;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _connectionGmt;
        private readonly IItemMasterService<YDReqIssueChild> _itemMasterRepository;
        public YDReqIssueService(
            IDapperCRUDService<YDReqIssueMaster> service,
            IItemMasterService<YDReqIssueChild> itemMasterRepository)
        {
            _service = service;
            _service.Connection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _connectionGmt = service.Connection;

            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
            _itemMasterRepository = itemMasterRepository;
        }
        public async Task<List<YDReqIssueMaster>> GetPagedAsync(Status status, string pageName, PaginationInfo paginationInfo)
        {
            string orderBy = "";

            string sql = "";
            if (status == Status.Proposed)
            {
                sql =
                $@"
               ;With M As 
                (
	                Select M.YDReqMasterID, YDReqNo, BM.YDBookingNo, LU.Name ReqByUser, YDReqDate, CT.ShortName BuyerName, SUM(C.ReqQty) ReqQty
	                FROM {TableNames.YD_REQ_MASTER} M Inner JOIN {TableNames.YD_REQ_CHILD} C On M.YDReqMasterID = C.YDReqMasterID
	                Inner JOIN {TableNames.YD_BOOKING_MASTER} BM ON M.YDBookingMasterID = BM.YDBookingMasterID
	                Inner Join {DbNames.EPYSL}..LoginUser LU On M.YDReqBy = LU.UserCode
	                Left Join {DbNames.EPYSL}..Contacts CT On M.BuyerID = CT.ContactID 
	                Where M.YDReqMasterID Not In (Select YDReqMasterID FROM {TableNames.YD_REQ_ISSUE_MASTER})
	                Group By M.YDReqMasterID, YDReqNo, BM.YDBookingNo, LU.Name, YDReqDate, CT.ShortName
                ) 
                Select YDReqMasterID, YDReqNo, YDBookingNo, ReqByUser, YDReqDate, BuyerName, ReqQty
                From M";

                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YDReqMasterID Desc" : paginationInfo.OrderBy;
            }
            else
            {
                if (pageName == "YarnDyeingRequisitionIssue")
                {
                    if (status == Status.Pending) //Requisition List
                    {
                        sql = $@"
                        ;With YRM As 
                        (
	                        Select * FROM {TableNames.YD_REQ_ISSUE_MASTER} 
	                        Where IsSendForApprove = 0
                        )
                        Select YRM.YDReqIssueMasterID, YRM.YDReqIssueNo, YRM.YDReqIssueDate, YRM.YDReqIssueBy, YRM.YDReqMasterID,
                        YRM.IsSendForApprove, RL.Name As SendForApproveName,  YRM.SendForApproveDate, YRM.IsApprove, A.Name As ApproveName, YRM.ApproveDate, 
                        YRM.IsAcknowledge, AcW.Name As AcknowledgeName, YRM.AcknowledgeDate, YRM.IsReject, R.Name As RejectName, YRM.RejectDate, YRM.AddedBy,
                        Sum(YRC.ReqQty)ReqQty, Sum(YRC.IssueQty)IssueQty, Sum(YRC.IssueQtyCarton)IssueQtyCarton, Sum(YRC.IssueQtyCone)IssueQtyCone  
                        From YRM Inner JOIN {TableNames.YD_REQ_ISSUE_CHILD} YRC ON YRC.YDReqIssueMasterID = YRM.YDReqIssueMasterID  
                        LEFT Join  {DbNames.EPYSL}..LoginUser RL On RL.UserCode = YRM.SendForApproveBy 
                        LEFT Join  {DbNames.EPYSL}..LoginUser A On A.UserCode = YRM.ApproveBy
                        LEFT Join  {DbNames.EPYSL}..LoginUser AcW On AcW.UserCode = YRM.AcknowledgeBy
                        LEFT Join  {DbNames.EPYSL}..LoginUser R On R.UserCode = YRM.RejectBy 
                        Group By YRM.YDReqIssueMasterID, YRM.YDReqIssueNo, YRM.YDReqIssueDate, YRM.YDReqIssueBy, YRM.YDReqMasterID,
                        YRM.IsSendForApprove, RL.Name,  YRM.SendForApproveDate, YRM.IsApprove, A.Name, YRM.ApproveDate, 
                        YRM.IsAcknowledge, AcW.Name, YRM.AcknowledgeDate, YRM.IsReject, R.Name, YRM.RejectDate, YRM.AddedBy";

                        orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YRM.YDReqIssueMasterID Desc" : paginationInfo.OrderBy;
                    }
                    else if (status == Status.AwaitingPropose) //Pending for Approval, Pending Approval
                    {
                        sql = $@"
                        ;With YRM As 
                        (
	                        Select * FROM {TableNames.YD_REQ_ISSUE_MASTER} 
	                        WHERE IsSendForApprove = 1
	                        AND IsApprove = 0
	                        AND IsReject = 0
                        )
                        Select YRM.YDReqIssueMasterID, YRM.YDReqIssueNo, YRM.YDReqIssueDate, YRM.YDReqIssueBy, YRM.YDReqMasterID,
                        YRM.IsSendForApprove, RL.Name As SendForApproveName,  YRM.SendForApproveDate, YRM.IsApprove, A.Name As ApproveName, YRM.ApproveDate, 
                        YRM.IsAcknowledge, AcW.Name As AcknowledgeName, YRM.AcknowledgeDate, YRM.IsReject, R.Name As RejectName, YRM.RejectDate, YRM.AddedBy,
                        Sum(YRC.ReqQty)ReqQty, Sum(YRC.IssueQty)IssueQty, Sum(YRC.IssueQtyCarton)IssueQtyCarton, Sum(YRC.IssueQtyCone)IssueQtyCone  
                        From YRM Inner JOIN {TableNames.YD_REQ_ISSUE_CHILD} YRC ON YRC.YDReqIssueMasterID = YRM.YDReqIssueMasterID  
                        LEFT Join  {DbNames.EPYSL}..LoginUser RL On RL.UserCode = YRM.SendForApproveBy 
                        LEFT Join  {DbNames.EPYSL}..LoginUser A On A.UserCode = YRM.ApproveBy
                        LEFT Join  {DbNames.EPYSL}..LoginUser AcW On AcW.UserCode = YRM.AcknowledgeBy
                        LEFT Join  {DbNames.EPYSL}..LoginUser R On R.UserCode = YRM.RejectBy 
                        Group By YRM.YDReqIssueMasterID, YRM.YDReqIssueNo, YRM.YDReqIssueDate, YRM.YDReqIssueBy, YRM.YDReqMasterID,
                        YRM.IsSendForApprove, RL.Name,  YRM.SendForApproveDate, YRM.IsApprove, A.Name, YRM.ApproveDate, 
                        YRM.IsAcknowledge, AcW.Name, YRM.AcknowledgeDate, YRM.IsReject, R.Name, YRM.RejectDate, YRM.AddedBy";

                        orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YRM.YDReqIssueMasterID Desc" : paginationInfo.OrderBy;
                    }
                    else if (status == Status.Approved) //Approve List, YD Yarn Requisiton
                    {
                        sql = $@"
                        ;With YRM As 
                        (
	                        Select * FROM {TableNames.YD_REQ_ISSUE_MASTER} 
	                        WHERE IsApprove = 1
	                        AND IsAcknowledge = 0
                        )
                        Select YRM.YDReqIssueMasterID, YRM.YDReqIssueNo, YRM.YDReqIssueDate, YRM.YDReqIssueBy, YRM.YDReqMasterID,
                        YRM.IsSendForApprove, RL.Name As SendForApproveName,  YRM.SendForApproveDate, YRM.IsApprove, A.Name As ApproveName, YRM.ApproveDate, 
                        YRM.IsAcknowledge, AcW.Name As AcknowledgeName, YRM.AcknowledgeDate, YRM.IsReject, R.Name As RejectName, YRM.RejectDate, YRM.AddedBy,
                        Sum(YRC.ReqQty)ReqQty, Sum(YRC.IssueQty)IssueQty, Sum(YRC.IssueQtyCarton)IssueQtyCarton, Sum(YRC.IssueQtyCone)IssueQtyCone  
                        From YRM Inner JOIN {TableNames.YD_REQ_ISSUE_CHILD} YRC ON YRC.YDReqIssueMasterID = YRM.YDReqIssueMasterID  
                        LEFT Join  {DbNames.EPYSL}..LoginUser RL On RL.UserCode = YRM.SendForApproveBy 
                        LEFT Join  {DbNames.EPYSL}..LoginUser A On A.UserCode = YRM.ApproveBy
                        LEFT Join  {DbNames.EPYSL}..LoginUser AcW On AcW.UserCode = YRM.AcknowledgeBy
                        LEFT Join  {DbNames.EPYSL}..LoginUser R On R.UserCode = YRM.RejectBy 
                        Group By YRM.YDReqIssueMasterID, YRM.YDReqIssueNo, YRM.YDReqIssueDate, YRM.YDReqIssueBy, YRM.YDReqMasterID,
                        YRM.IsSendForApprove, RL.Name,  YRM.SendForApproveDate, YRM.IsApprove, A.Name, YRM.ApproveDate, 
                        YRM.IsAcknowledge, AcW.Name, YRM.AcknowledgeDate, YRM.IsReject, R.Name, YRM.RejectDate, YRM.AddedBy";

                        orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YRM.YDReqIssueMasterID Desc" : paginationInfo.OrderBy;
                    }
                    else if (status == Status.Reject) //Reject List
                    {
                        sql = $@"
                        ;With YRM As 
                        (
	                        Select * FROM {TableNames.YD_REQ_ISSUE_MASTER}
	                        WHERE IsReject = 1
                        )
                        Select YRM.YDReqIssueMasterID, YRM.YDReqIssueNo, YRM.YDReqIssueDate, YRM.YDReqIssueBy, YRM.YDReqMasterID,
                        YRM.IsSendForApprove, RL.Name As SendForApproveName,  YRM.SendForApproveDate, YRM.IsApprove, A.Name As ApproveName, YRM.ApproveDate, 
                        YRM.IsAcknowledge, AcW.Name As AcknowledgeName, YRM.AcknowledgeDate, YRM.IsReject, R.Name As RejectName, YRM.RejectDate, YRM.AddedBy,
                        Sum(YRC.ReqQty)ReqQty, Sum(YRC.IssueQty)IssueQty, Sum(YRC.IssueQtyCarton)IssueQtyCarton, Sum(YRC.IssueQtyCone)IssueQtyCone  
                        From YRM Inner JOIN {TableNames.YD_REQ_ISSUE_CHILD} YRC ON YRC.YDReqIssueMasterID = YRM.YDReqIssueMasterID  
                        LEFT Join  {DbNames.EPYSL}..LoginUser RL On RL.UserCode = YRM.SendForApproveBy 
                        LEFT Join  {DbNames.EPYSL}..LoginUser A On A.UserCode = YRM.ApproveBy
                        LEFT Join  {DbNames.EPYSL}..LoginUser AcW On AcW.UserCode = YRM.AcknowledgeBy
                        LEFT Join  {DbNames.EPYSL}..LoginUser R On R.UserCode = YRM.RejectBy 
                        Group By YRM.YDReqIssueMasterID, YRM.YDReqIssueNo, YRM.YDReqIssueDate, YRM.YDReqIssueBy, YRM.YDReqMasterID,
                        YRM.IsSendForApprove, RL.Name,  YRM.SendForApproveDate, YRM.IsApprove, A.Name, YRM.ApproveDate, 
                        YRM.IsAcknowledge, AcW.Name, YRM.AcknowledgeDate, YRM.IsReject, R.Name, YRM.RejectDate, YRM.AddedBy";

                        orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YRM.YDReqIssueMasterID Desc" : paginationInfo.OrderBy;
                    }
                }
                else if (pageName == "YarnDyeingRequisitionAppove")
                {
                    if (status == Status.AwaitingPropose) //Pending for Approval, Pending Approval
                    {
                        sql = $@"
                        ;With YRM As 
                        (
	                        Select * FROM {TableNames.YD_REQ_ISSUE_MASTER} 
	                        WHERE IsSendForApprove = 1
	                        AND IsApprove = 0
	                        AND IsReject = 0
                        )
                        Select YRM.YDReqIssueMasterID, YRM.YDReqIssueNo, YRM.YDReqIssueDate, YRM.YDReqIssueBy, YRM.YDReqMasterID,
                        YRM.IsSendForApprove, RL.Name As SendForApproveName,  YRM.SendForApproveDate, YRM.IsApprove, A.Name As ApproveName, YRM.ApproveDate, 
                        YRM.IsAcknowledge, AcW.Name As AcknowledgeName, YRM.AcknowledgeDate, YRM.IsReject, R.Name As RejectName, YRM.RejectDate, YRM.AddedBy,
                        Sum(YRC.ReqQty)ReqQty, Sum(YRC.IssueQty)IssueQty, Sum(YRC.IssueQtyCarton)IssueQtyCarton, Sum(YRC.IssueQtyCone)IssueQtyCone  
                        From YRM Inner JOIN {TableNames.YD_REQ_ISSUE_CHILD} YRC ON YRC.YDReqIssueMasterID = YRM.YDReqIssueMasterID  
                        LEFT Join  {DbNames.EPYSL}..LoginUser RL On RL.UserCode = YRM.SendForApproveBy 
                        LEFT Join  {DbNames.EPYSL}..LoginUser A On A.UserCode = YRM.ApproveBy
                        LEFT Join  {DbNames.EPYSL}..LoginUser AcW On AcW.UserCode = YRM.AcknowledgeBy
                        LEFT Join  {DbNames.EPYSL}..LoginUser R On R.UserCode = YRM.RejectBy 
                        Group By YRM.YDReqIssueMasterID, YRM.YDReqIssueNo, YRM.YDReqIssueDate, YRM.YDReqIssueBy, YRM.YDReqMasterID,
                        YRM.IsSendForApprove, RL.Name,  YRM.SendForApproveDate, YRM.IsApprove, A.Name, YRM.ApproveDate, 
                        YRM.IsAcknowledge, AcW.Name, YRM.AcknowledgeDate, YRM.IsReject, R.Name, YRM.RejectDate, YRM.AddedBy";

                        orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YRM.YDReqIssueMasterID Desc" : paginationInfo.OrderBy;
                    }
                    else if (status == Status.Approved) //Approve List, YD Yarn Requisiton
                    {
                        sql = $@"
                        ;With YRM As 
                        (
	                        Select * FROM {TableNames.YD_REQ_ISSUE_MASTER} 
	                        WHERE IsApprove = 1
	                        AND IsAcknowledge = 0
                        )
                        Select YRM.YDReqIssueMasterID, YRM.YDReqIssueNo, YRM.YDReqIssueDate, YRM.YDReqIssueBy, YRM.YDReqMasterID,
                        YRM.IsSendForApprove, RL.Name As SendForApproveName,  YRM.SendForApproveDate, YRM.IsApprove, A.Name As ApproveName, YRM.ApproveDate, 
                        YRM.IsAcknowledge, AcW.Name As AcknowledgeName, YRM.AcknowledgeDate, YRM.IsReject, R.Name As RejectName, YRM.RejectDate, YRM.AddedBy,
                        Sum(YRC.ReqQty)ReqQty, Sum(YRC.IssueQty)IssueQty, Sum(YRC.IssueQtyCarton)IssueQtyCarton, Sum(YRC.IssueQtyCone)IssueQtyCone  
                        From YRM Inner JOIN {TableNames.YD_REQ_ISSUE_CHILD} YRC ON YRC.YDReqIssueMasterID = YRM.YDReqIssueMasterID  
                        LEFT Join  {DbNames.EPYSL}..LoginUser RL On RL.UserCode = YRM.SendForApproveBy 
                        LEFT Join  {DbNames.EPYSL}..LoginUser A On A.UserCode = YRM.ApproveBy
                        LEFT Join  {DbNames.EPYSL}..LoginUser AcW On AcW.UserCode = YRM.AcknowledgeBy
                        LEFT Join  {DbNames.EPYSL}..LoginUser R On R.UserCode = YRM.RejectBy 
                        Group By YRM.YDReqIssueMasterID, YRM.YDReqIssueNo, YRM.YDReqIssueDate, YRM.YDReqIssueBy, YRM.YDReqMasterID,
                        YRM.IsSendForApprove, RL.Name,  YRM.SendForApproveDate, YRM.IsApprove, A.Name, YRM.ApproveDate, 
                        YRM.IsAcknowledge, AcW.Name, YRM.AcknowledgeDate, YRM.IsReject, R.Name, YRM.RejectDate, YRM.AddedBy";

                        orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YRM.YDReqIssueMasterID Desc" : paginationInfo.OrderBy;
                    }
                    else if (status == Status.Reject) //Reject List
                    {
                        sql = $@"
                        ;With YRM As 
                        (
	                        Select * FROM {TableNames.YD_REQ_ISSUE_MASTER} 
	                        WHERE IsReject = 1
                        )
                        Select YRM.YDReqIssueMasterID, YRM.YDReqIssueNo, YRM.YDReqIssueDate, YRM.YDReqIssueBy, YRM.YDReqMasterID,
                        YRM.IsSendForApprove, RL.Name As SendForApproveName,  YRM.SendForApproveDate, YRM.IsApprove, A.Name As ApproveName, YRM.ApproveDate, 
                        YRM.IsAcknowledge, AcW.Name As AcknowledgeName, YRM.AcknowledgeDate, YRM.IsReject, R.Name As RejectName, YRM.RejectDate, YRM.AddedBy,
                        Sum(YRC.ReqQty)ReqQty, Sum(YRC.IssueQty)IssueQty, Sum(YRC.IssueQtyCarton)IssueQtyCarton, Sum(YRC.IssueQtyCone)IssueQtyCone  
                        From YRM Inner JOIN {TableNames.YD_REQ_ISSUE_CHILD} YRC ON YRC.YDReqIssueMasterID = YRM.YDReqIssueMasterID  
                        LEFT Join  {DbNames.EPYSL}..LoginUser RL On RL.UserCode = YRM.SendForApproveBy 
                        LEFT Join  {DbNames.EPYSL}..LoginUser A On A.UserCode = YRM.ApproveBy
                        LEFT Join  {DbNames.EPYSL}..LoginUser AcW On AcW.UserCode = YRM.AcknowledgeBy
                        LEFT Join  {DbNames.EPYSL}..LoginUser R On R.UserCode = YRM.RejectBy 
                        Group By YRM.YDReqIssueMasterID, YRM.YDReqIssueNo, YRM.YDReqIssueDate, YRM.YDReqIssueBy, YRM.YDReqMasterID,
                        YRM.IsSendForApprove, RL.Name,  YRM.SendForApproveDate, YRM.IsApprove, A.Name, YRM.ApproveDate, 
                        YRM.IsAcknowledge, AcW.Name, YRM.AcknowledgeDate, YRM.IsReject, R.Name, YRM.RejectDate, YRM.AddedBy";

                        orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YRM.YDReqIssueMasterID Desc" : paginationInfo.OrderBy;
                    }
                }
                else if (pageName == "YarnDyeingRequisitionAcknowledge")
                {
                    if (status == Status.Approved) //Approve List, YD Yarn Requisiton
                    {
                        sql = $@"
                        ;With YRM As 
                        (
	                        Select * FROM {TableNames.YD_REQ_ISSUE_MASTER} 
	                        WHERE IsApprove = 1
	                        AND IsAcknowledge = 0
                        )
                        Select YRM.YDReqIssueMasterID, YRM.YDReqIssueNo, YRM.YDReqIssueDate, YRM.YDReqIssueBy, YRM.YDReqMasterID,
                        YRM.IsSendForApprove, RL.Name As SendForApproveName,  YRM.SendForApproveDate, YRM.IsApprove, A.Name As ApproveName, YRM.ApproveDate, 
                        YRM.IsAcknowledge, AcW.Name As AcknowledgeName, YRM.AcknowledgeDate, YRM.IsReject, R.Name As RejectName, YRM.RejectDate, YRM.AddedBy,
                        Sum(YRC.ReqQty)ReqQty, Sum(YRC.IssueQty)IssueQty, Sum(YRC.IssueQtyCarton)IssueQtyCarton, Sum(YRC.IssueQtyCone)IssueQtyCone  
                        From YRM Inner JOIN {TableNames.YD_REQ_ISSUE_CHILD} YRC ON YRC.YDReqIssueMasterID = YRM.YDReqIssueMasterID  
                        LEFT Join  {DbNames.EPYSL}..LoginUser RL On RL.UserCode = YRM.SendForApproveBy 
                        LEFT Join  {DbNames.EPYSL}..LoginUser A On A.UserCode = YRM.ApproveBy
                        LEFT Join  {DbNames.EPYSL}..LoginUser AcW On AcW.UserCode = YRM.AcknowledgeBy
                        LEFT Join  {DbNames.EPYSL}..LoginUser R On R.UserCode = YRM.RejectBy 
                        Group By YRM.YDReqIssueMasterID, YRM.YDReqIssueNo, YRM.YDReqIssueDate, YRM.YDReqIssueBy, YRM.YDReqMasterID,
                        YRM.IsSendForApprove, RL.Name,  YRM.SendForApproveDate, YRM.IsApprove, A.Name, YRM.ApproveDate, 
                        YRM.IsAcknowledge, AcW.Name, YRM.AcknowledgeDate, YRM.IsReject, R.Name, YRM.RejectDate, YRM.AddedBy";

                        orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YRM.YDReqIssueMasterID Desc" : paginationInfo.OrderBy;
                    }
                    if (status == Status.Acknowledge) //Acknowledgement List
                    {
                        sql = $@"
                        ;With YRM As 
                        (
	                        Select * FROM {TableNames.YD_REQ_ISSUE_MASTER} 
	                        WHERE IsApprove = 1
	                        AND IsAcknowledge = 1
                        )
                        Select YRM.YDReqIssueMasterID, YRM.YDReqIssueNo, YRM.YDReqIssueDate, YRM.YDReqIssueBy, YRM.YDReqMasterID,
                        YRM.IsSendForApprove, RL.Name As SendForApproveName,  YRM.SendForApproveDate, YRM.IsApprove, A.Name As ApproveName, YRM.ApproveDate, 
                        YRM.IsAcknowledge, AcW.Name As AcknowledgeName, YRM.AcknowledgeDate, YRM.IsReject, R.Name As RejectName, YRM.RejectDate, YRM.AddedBy,
                        Sum(YRC.ReqQty)ReqQty, Sum(YRC.IssueQty)IssueQty, Sum(YRC.IssueQtyCarton)IssueQtyCarton, Sum(YRC.IssueQtyCone)IssueQtyCone  
                        From YRM Inner JOIN {TableNames.YD_REQ_ISSUE_CHILD} YRC ON YRC.YDReqIssueMasterID = YRM.YDReqIssueMasterID  
                        LEFT Join  {DbNames.EPYSL}..LoginUser RL On RL.UserCode = YRM.SendForApproveBy 
                        LEFT Join  {DbNames.EPYSL}..LoginUser A On A.UserCode = YRM.ApproveBy
                        LEFT Join  {DbNames.EPYSL}..LoginUser AcW On AcW.UserCode = YRM.AcknowledgeBy
                        LEFT Join  {DbNames.EPYSL}..LoginUser R On R.UserCode = YRM.RejectBy 
                        Group By YRM.YDReqIssueMasterID, YRM.YDReqIssueNo, YRM.YDReqIssueDate, YRM.YDReqIssueBy, YRM.YDReqMasterID,
                        YRM.IsSendForApprove, RL.Name,  YRM.SendForApproveDate, YRM.IsApprove, A.Name, YRM.ApproveDate, 
                        YRM.IsAcknowledge, AcW.Name, YRM.AcknowledgeDate, YRM.IsReject, R.Name, YRM.RejectDate, YRM.AddedBy";

                        orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YRM.YDReqIssueMasterID Desc" : paginationInfo.OrderBy;
                    }
                }
            }
            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<YDReqIssueMaster>(sql);
        }
        public async Task<List<YDReqMaster>> GetYDRequisitionsAsync(Status status, string pageName, PaginationInfo paginationInfo)
        {
            string orderBy = "";

            string sql = "";
            if (status == Status.Proposed)
            {
                sql =
                $@"
               ;With M As 
                (
	                Select M.YDReqMasterID, YDReqNo, BM.YDBookingNo, LU.Name ReqByUser, YDReqDate, CT.ShortName BuyerName, SUM(C.ReqQty) ReqQty
	                FROM {TableNames.YD_REQ_MASTER} M Inner JOIN {TableNames.YD_REQ_CHILD} C On M.YDReqMasterID = C.YDReqMasterID
	                Inner JOIN {TableNames.YD_BOOKING_MASTER} BM ON M.YDBookingMasterID = BM.YDBookingMasterID
	                Inner Join {DbNames.EPYSL}..LoginUser LU On M.YDReqBy = LU.UserCode
	                Left Join {DbNames.EPYSL}..Contacts CT On BM.BuyerID = CT.ContactID 
	                Where M.YDReqMasterID Not In (Select YDReqMasterID FROM {TableNames.YD_REQ_ISSUE_MASTER})  AND M.IsAcknowledge=1
	                Group By M.YDReqMasterID, YDReqNo, BM.YDBookingNo, LU.Name, YDReqDate, CT.ShortName
                ) 
                Select YDReqMasterID, YDReqNo, YDBookingNo, ReqByUser, YDReqDate, BuyerName, ReqQty
                From M";

                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YDReqMasterID Desc" : paginationInfo.OrderBy;
            }
            else
            {
                if (pageName == "YarnDyeingRequisitionIssue")
                {
                    if (status == Status.Pending) //Requisition List
                    {
                        sql = $@"
                        ;With YRM As 
                        (
	                        Select * FROM {TableNames.YD_REQ_ISSUE_MASTER} 
	                        Where IsSendForApprove = 0
                        )
                        Select YRM.YDReqIssueMasterID, YRM.YDReqIssueNo, YRM.YDReqIssueDate, YRM.YDReqIssueBy, YRM.YDReqMasterID,
                        YRM.IsSendForApprove, RL.Name As SendForApproveName,  YRM.SendForApproveDate, YRM.IsApprove, A.Name As ApproveName, YRM.ApproveDate, 
                        YRM.IsAcknowledge, AcW.Name As AcknowledgeName, YRM.AcknowledgeDate, YRM.IsReject, R.Name As RejectName, YRM.RejectDate, YRM.AddedBy,
                        Sum(YRC.ReqQty)ReqQty, Sum(YRC.IssueQty)IssueQty, Sum(YRC.IssueQtyCarton)IssueQtyCarton, Sum(YRC.IssueQtyCone)IssueQtyCone,  
						BM.YDBookingNo, CT.ShortName BuyerName, FM.CompanyID,FM.ConceptNo,YDRM.YDReqNo,BU.Name BookingByUser, LU.Name ReqByUser
                        From YRM Inner JOIN {TableNames.YD_REQ_ISSUE_CHILD} YRC ON YRC.YDReqIssueMasterID = YRM.YDReqIssueMasterID  
						INNER JOIN {TableNames.YD_REQ_MASTER} YDRM ON YDRM.YDReqMasterID=YRM.YDReqMasterID
						Inner JOIN {TableNames.YD_BOOKING_MASTER} BM ON YDRM.YDBookingMasterID = BM.YDBookingMasterID
						Inner JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FM ON FM.ConceptID = BM.ConceptID
						Inner Join {DbNames.EPYSL}..Contacts CT On CT.ContactID = BM.BuyerID
						Inner Join {DbNames.EPYSL}..LoginUser BU On BM.YDBookingBy = BU.UserCode
						Inner Join {DbNames.EPYSL}..LoginUser LU On YDRM.YDReqBy = LU.UserCode
                        LEFT Join  {DbNames.EPYSL}..LoginUser RL On RL.UserCode = YRM.SendForApproveBy 
                        LEFT Join  {DbNames.EPYSL}..LoginUser A On A.UserCode = YRM.ApproveBy
                        LEFT Join  {DbNames.EPYSL}..LoginUser AcW On AcW.UserCode = YRM.AcknowledgeBy
                        LEFT Join  {DbNames.EPYSL}..LoginUser R On R.UserCode = YRM.RejectBy 
                        Group By YRM.YDReqIssueMasterID, YRM.YDReqIssueNo, YRM.YDReqIssueDate, YRM.YDReqIssueBy, YRM.YDReqMasterID,
                        YRM.IsSendForApprove, RL.Name,  YRM.SendForApproveDate, YRM.IsApprove, A.Name, YRM.ApproveDate, 
                        YRM.IsAcknowledge, AcW.Name, YRM.AcknowledgeDate, YRM.IsReject, R.Name, YRM.RejectDate, YRM.AddedBy,
						BM.YDBookingNo, CT.ShortName, FM.CompanyID,FM.ConceptNo,YDRM.YDReqNo,BU.Name, LU.Name ";

                        orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YRM.YDReqIssueMasterID Desc" : paginationInfo.OrderBy;
                    }
                    else if (status == Status.AwaitingPropose) //Pending for Approval, Pending Approval
                    {
                        sql = $@"
                        ;With YRM As 
                        (
	                        Select * FROM {TableNames.YD_REQ_ISSUE_MASTER} 
	                        WHERE IsSendForApprove = 1
	                        AND IsApprove = 0
	                        AND IsReject = 0
                        )
                        Select YRM.YDReqIssueMasterID, YRM.YDReqIssueNo, YRM.YDReqIssueDate, YRM.YDReqIssueBy, YRM.YDReqMasterID,
                        YRM.IsSendForApprove, RL.Name As SendForApproveName,  YRM.SendForApproveDate, YRM.IsApprove, A.Name As ApproveName, YRM.ApproveDate, 
                        YRM.IsAcknowledge, AcW.Name As AcknowledgeName, YRM.AcknowledgeDate, YRM.IsReject, R.Name As RejectName, YRM.RejectDate, YRM.AddedBy,
                        Sum(YRC.ReqQty)ReqQty, Sum(YRC.IssueQty)IssueQty, Sum(YRC.IssueQtyCarton)IssueQtyCarton, Sum(YRC.IssueQtyCone)IssueQtyCone,  
                        BM.YDBookingNo, CT.ShortName BuyerName, FM.CompanyID,FM.ConceptNo,YDRM.YDReqNo,BU.Name BookingByUser, LU.Name ReqByUser
						From YRM Inner JOIN {TableNames.YD_REQ_ISSUE_CHILD} YRC ON YRC.YDReqIssueMasterID = YRM.YDReqIssueMasterID 
						INNER JOIN {TableNames.YD_REQ_MASTER} YDRM ON YDRM.YDReqMasterID=YRM.YDReqMasterID
						Inner JOIN {TableNames.YD_BOOKING_MASTER} BM ON YDRM.YDBookingMasterID = BM.YDBookingMasterID
						Inner JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FM ON FM.ConceptID = BM.ConceptID
						Inner Join {DbNames.EPYSL}..Contacts CT On CT.ContactID = BM.BuyerID
						Inner Join {DbNames.EPYSL}..LoginUser BU On BM.YDBookingBy = BU.UserCode
						Inner Join {DbNames.EPYSL}..LoginUser LU On YDRM.YDReqBy = LU.UserCode
                        LEFT Join  {DbNames.EPYSL}..LoginUser RL On RL.UserCode = YRM.SendForApproveBy 
                        LEFT Join  {DbNames.EPYSL}..LoginUser A On A.UserCode = YRM.ApproveBy
                        LEFT Join  {DbNames.EPYSL}..LoginUser AcW On AcW.UserCode = YRM.AcknowledgeBy
                        LEFT Join  {DbNames.EPYSL}..LoginUser R On R.UserCode = YRM.RejectBy 
                        Group By YRM.YDReqIssueMasterID, YRM.YDReqIssueNo, YRM.YDReqIssueDate, YRM.YDReqIssueBy, YRM.YDReqMasterID,
                        YRM.IsSendForApprove, RL.Name,  YRM.SendForApproveDate, YRM.IsApprove, A.Name, YRM.ApproveDate, 
                        YRM.IsAcknowledge, AcW.Name, YRM.AcknowledgeDate, YRM.IsReject, R.Name, YRM.RejectDate, YRM.AddedBy,
						BM.YDBookingNo, CT.ShortName, FM.CompanyID,FM.ConceptNo,YDRM.YDReqNo,BU.Name, LU.Name";

                        orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YRM.YDReqIssueMasterID Desc" : paginationInfo.OrderBy;
                    }
                    else if (status == Status.Approved) //Approve List, YD Yarn Requisiton
                    {
                        sql = $@"
                        ;With YRM As 
                        (
	                        Select * FROM {TableNames.YD_REQ_ISSUE_MASTER} 
	                        WHERE IsApprove = 1
	                        AND IsAcknowledge = 0
                        )
                         Select YRM.YDReqIssueMasterID, YRM.YDReqIssueNo, YRM.YDReqIssueDate, YRM.YDReqIssueBy, YRM.YDReqMasterID,
                        YRM.IsSendForApprove, RL.Name As SendForApproveName,  YRM.SendForApproveDate, YRM.IsApprove, A.Name As ApproveName, YRM.ApproveDate, 
                        YRM.IsAcknowledge, AcW.Name As AcknowledgeName, YDRM.AcknowledgeDate, YRM.IsReject, R.Name As RejectName, YRM.RejectDate, YRM.AddedBy,
                        Sum(YRC.ReqQty)ReqQty, Sum(YRC.IssueQty)IssueQty, Sum(YRC.IssueQtyCarton)IssueQtyCarton, Sum(YRC.IssueQtyCone)IssueQtyCone,  
                        BM.YDBookingNo, CT.ShortName BuyerName, FM.CompanyID,FM.ConceptNo,YDRM.YDReqNo,BU.Name BookingByUser, LU.Name ReqByUser  
                        From YRM Inner JOIN {TableNames.YD_REQ_ISSUE_CHILD} YRC ON YRC.YDReqIssueMasterID = YRM.YDReqIssueMasterID  
						INNER JOIN {TableNames.YD_REQ_MASTER} YDRM ON YDRM.YDReqMasterID=YRM.YDReqMasterID
						Inner JOIN {TableNames.YD_BOOKING_MASTER} BM ON YDRM.YDBookingMasterID = BM.YDBookingMasterID
						Inner JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FM ON FM.ConceptID = BM.ConceptID
						Inner Join {DbNames.EPYSL}..Contacts CT On CT.ContactID = BM.BuyerID
						Inner Join {DbNames.EPYSL}..LoginUser BU On BM.YDBookingBy = BU.UserCode
						Inner Join {DbNames.EPYSL}..LoginUser LU On YDRM.YDReqBy = LU.UserCode
                        LEFT Join  {DbNames.EPYSL}..LoginUser RL On RL.UserCode = YRM.SendForApproveBy 
                        LEFT Join  {DbNames.EPYSL}..LoginUser A On A.UserCode = YRM.ApproveBy
                        LEFT Join  {DbNames.EPYSL}..LoginUser AcW On AcW.UserCode = YDRM.AcknowledgeBy
                        LEFT Join  {DbNames.EPYSL}..LoginUser R On R.UserCode = YRM.RejectBy 
                        Group By YRM.YDReqIssueMasterID, YRM.YDReqIssueNo, YRM.YDReqIssueDate, YRM.YDReqIssueBy, YRM.YDReqMasterID,
                        YRM.IsSendForApprove, RL.Name,  YRM.SendForApproveDate, YRM.IsApprove, A.Name, YRM.ApproveDate, 
                        YRM.IsAcknowledge, AcW.Name, YDRM.AcknowledgeDate, YRM.IsReject, R.Name, YRM.RejectDate, YRM.AddedBy,
						BM.YDBookingNo, CT.ShortName, FM.CompanyID,FM.ConceptNo,YDRM.YDReqNo,BU.Name, LU.Name";

                        orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YRM.YDReqIssueMasterID Desc" : paginationInfo.OrderBy;
                    }
                    else if (status == Status.Reject) //Reject List
                    {
                        sql = $@"
                        ;With YRM As 
                        (
	                        Select * FROM {TableNames.YD_REQ_ISSUE_MASTER}
	                        WHERE IsReject = 1
                        )
                        Select YRM.YDReqIssueMasterID, YRM.YDReqIssueNo, YRM.YDReqIssueDate, YRM.YDReqIssueBy, YRM.YDReqMasterID,
                        YRM.IsSendForApprove, RL.Name As SendForApproveName,  YRM.SendForApproveDate, YRM.IsApprove, A.Name As ApproveName, YRM.ApproveDate, 
                        YRM.IsAcknowledge, AcW.Name As AcknowledgeName, YRM.AcknowledgeDate, YRM.IsReject, R.Name As RejectName, YRM.RejectDate, YRM.AddedBy,
                        Sum(YRC.ReqQty)ReqQty, Sum(YRC.IssueQty)IssueQty, Sum(YRC.IssueQtyCarton)IssueQtyCarton, Sum(YRC.IssueQtyCone)IssueQtyCone,  
                        BM.YDBookingNo, CT.ShortName BuyerName, FM.CompanyID,FM.ConceptNo,YDRM.YDReqNo,BU.Name BookingByUser, LU.Name ReqByUser   
                        From YRM Inner JOIN {TableNames.YD_REQ_ISSUE_CHILD} YRC ON YRC.YDReqIssueMasterID = YRM.YDReqIssueMasterID 
						INNER JOIN {TableNames.YD_REQ_MASTER} YDRM ON YDRM.YDReqMasterID=YRM.YDReqMasterID
						Inner JOIN {TableNames.YD_BOOKING_MASTER} BM ON YDRM.YDBookingMasterID = BM.YDBookingMasterID
						Inner JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FM ON FM.ConceptID = BM.ConceptID
						Inner Join {DbNames.EPYSL}..Contacts CT On CT.ContactID = BM.BuyerID
						Inner Join {DbNames.EPYSL}..LoginUser BU On BM.YDBookingBy = BU.UserCode
						Inner Join {DbNames.EPYSL}..LoginUser LU On YDRM.YDReqBy = LU.UserCode
                        LEFT Join  {DbNames.EPYSL}..LoginUser RL On RL.UserCode = YRM.SendForApproveBy 
                        LEFT Join  {DbNames.EPYSL}..LoginUser A On A.UserCode = YRM.ApproveBy
                        LEFT Join  {DbNames.EPYSL}..LoginUser AcW On AcW.UserCode = YRM.AcknowledgeBy
                        LEFT Join  {DbNames.EPYSL}..LoginUser R On R.UserCode = YRM.RejectBy 
                        Group By YRM.YDReqIssueMasterID, YRM.YDReqIssueNo, YRM.YDReqIssueDate, YRM.YDReqIssueBy, YRM.YDReqMasterID,
                        YRM.IsSendForApprove, RL.Name,  YRM.SendForApproveDate, YRM.IsApprove, A.Name, YRM.ApproveDate, 
                        YRM.IsAcknowledge, AcW.Name, YRM.AcknowledgeDate, YRM.IsReject, R.Name, YRM.RejectDate, YRM.AddedBy,
						BM.YDBookingNo, CT.ShortName, FM.CompanyID,FM.ConceptNo,YDRM.YDReqNo,BU.Name, LU.Name";

                        orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YRM.YDReqIssueMasterID Desc" : paginationInfo.OrderBy;
                    }
                }
                else if (pageName == "YarnDyeingRequisitionAppove")
                {
                    if (status == Status.AwaitingPropose) //Pending for Approval, Pending Approval
                    {
                        sql = $@"
                        ;With YRM As 
                        (
	                        Select * FROM {TableNames.YD_REQ_ISSUE_MASTER} 
	                        WHERE IsSendForApprove = 1
	                        AND IsApprove = 0
	                        AND IsReject = 0
                        )
                        Select YRM.YDReqIssueMasterID, YRM.YDReqIssueNo, YRM.YDReqIssueDate, YRM.YDReqIssueBy, YRM.YDReqMasterID,
                        YRM.IsSendForApprove, RL.Name As SendForApproveName,  YRM.SendForApproveDate, YRM.IsApprove, A.Name As ApproveName, YRM.ApproveDate, 
                        YRM.IsAcknowledge, AcW.Name As AcknowledgeName, YRM.AcknowledgeDate, YRM.IsReject, R.Name As RejectName, YRM.RejectDate, YRM.AddedBy,
                        Sum(YRC.ReqQty)ReqQty, Sum(YRC.IssueQty)IssueQty, Sum(YRC.IssueQtyCarton)IssueQtyCarton, Sum(YRC.IssueQtyCone)IssueQtyCone  
                        From YRM Inner JOIN {TableNames.YD_REQ_ISSUE_CHILD} YRC ON YRC.YDReqIssueMasterID = YRM.YDReqIssueMasterID  
                        LEFT Join  {DbNames.EPYSL}..LoginUser RL On RL.UserCode = YRM.SendForApproveBy 
                        LEFT Join  {DbNames.EPYSL}..LoginUser A On A.UserCode = YRM.ApproveBy
                        LEFT Join  {DbNames.EPYSL}..LoginUser AcW On AcW.UserCode = YRM.AcknowledgeBy
                        LEFT Join  {DbNames.EPYSL}..LoginUser R On R.UserCode = YRM.RejectBy 
                        Group By YRM.YDReqIssueMasterID, YRM.YDReqIssueNo, YRM.YDReqIssueDate, YRM.YDReqIssueBy, YRM.YDReqMasterID,
                        YRM.IsSendForApprove, RL.Name,  YRM.SendForApproveDate, YRM.IsApprove, A.Name, YRM.ApproveDate, 
                        YRM.IsAcknowledge, AcW.Name, YRM.AcknowledgeDate, YRM.IsReject, R.Name, YRM.RejectDate, YRM.AddedBy";

                        orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YRM.YDReqIssueMasterID Desc" : paginationInfo.OrderBy;
                    }
                    else if (status == Status.Approved) //Approve List, YD Yarn Requisiton
                    {
                        sql = $@"
                        ;With YRM As 
                        (
	                        Select * FROM {TableNames.YD_REQ_ISSUE_MASTER} 
	                        WHERE IsApprove = 1
	                        AND IsAcknowledge = 0
                        )
                        Select YRM.YDReqIssueMasterID, YRM.YDReqIssueNo, YRM.YDReqIssueDate, YRM.YDReqIssueBy, YRM.YDReqMasterID,
                        YRM.IsSendForApprove, RL.Name As SendForApproveName,  YRM.SendForApproveDate, YRM.IsApprove, A.Name As ApproveName, YRM.ApproveDate, 
                        YRM.IsAcknowledge, AcW.Name As AcknowledgeName, YRM.AcknowledgeDate, YRM.IsReject, R.Name As RejectName, YRM.RejectDate, YRM.AddedBy,
                        Sum(YRC.ReqQty)ReqQty, Sum(YRC.IssueQty)IssueQty, Sum(YRC.IssueQtyCarton)IssueQtyCarton, Sum(YRC.IssueQtyCone)IssueQtyCone  
                        From YRM Inner JOIN {TableNames.YD_REQ_ISSUE_CHILD} YRC ON YRC.YDReqIssueMasterID = YRM.YDReqIssueMasterID  
                        LEFT Join  {DbNames.EPYSL}..LoginUser RL On RL.UserCode = YRM.SendForApproveBy 
                        LEFT Join  {DbNames.EPYSL}..LoginUser A On A.UserCode = YRM.ApproveBy
                        LEFT Join  {DbNames.EPYSL}..LoginUser AcW On AcW.UserCode = YRM.AcknowledgeBy
                        LEFT Join  {DbNames.EPYSL}..LoginUser R On R.UserCode = YRM.RejectBy 
                        Group By YRM.YDReqIssueMasterID, YRM.YDReqIssueNo, YRM.YDReqIssueDate, YRM.YDReqIssueBy, YRM.YDReqMasterID,
                        YRM.IsSendForApprove, RL.Name,  YRM.SendForApproveDate, YRM.IsApprove, A.Name, YRM.ApproveDate, 
                        YRM.IsAcknowledge, AcW.Name, YRM.AcknowledgeDate, YRM.IsReject, R.Name, YRM.RejectDate, YRM.AddedBy";

                        orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YRM.YDReqIssueMasterID Desc" : paginationInfo.OrderBy;
                    }
                    else if (status == Status.Reject) //Reject List
                    {
                        sql = $@"
                        ;With YRM As 
                        (
	                        Select * FROM {TableNames.YD_REQ_ISSUE_MASTER} 
	                        WHERE IsReject = 1
                        )
                        Select YRM.YDReqIssueMasterID, YRM.YDReqIssueNo, YRM.YDReqIssueDate, YRM.YDReqIssueBy, YRM.YDReqMasterID,
                        YRM.IsSendForApprove, RL.Name As SendForApproveName,  YRM.SendForApproveDate, YRM.IsApprove, A.Name As ApproveName, YRM.ApproveDate, 
                        YRM.IsAcknowledge, AcW.Name As AcknowledgeName, YRM.AcknowledgeDate, YRM.IsReject, R.Name As RejectName, YRM.RejectDate, YRM.AddedBy,
                        Sum(YRC.ReqQty)ReqQty, Sum(YRC.IssueQty)IssueQty, Sum(YRC.IssueQtyCarton)IssueQtyCarton, Sum(YRC.IssueQtyCone)IssueQtyCone  
                        From YRM Inner JOIN {TableNames.YD_REQ_ISSUE_CHILD} YRC ON YRC.YDReqIssueMasterID = YRM.YDReqIssueMasterID  
                        LEFT Join  {DbNames.EPYSL}..LoginUser RL On RL.UserCode = YRM.SendForApproveBy 
                        LEFT Join  {DbNames.EPYSL}..LoginUser A On A.UserCode = YRM.ApproveBy
                        LEFT Join  {DbNames.EPYSL}..LoginUser AcW On AcW.UserCode = YRM.AcknowledgeBy
                        LEFT Join  {DbNames.EPYSL}..LoginUser R On R.UserCode = YRM.RejectBy 
                        Group By YRM.YDReqIssueMasterID, YRM.YDReqIssueNo, YRM.YDReqIssueDate, YRM.YDReqIssueBy, YRM.YDReqMasterID,
                        YRM.IsSendForApprove, RL.Name,  YRM.SendForApproveDate, YRM.IsApprove, A.Name, YRM.ApproveDate, 
                        YRM.IsAcknowledge, AcW.Name, YRM.AcknowledgeDate, YRM.IsReject, R.Name, YRM.RejectDate, YRM.AddedBy";

                        orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YRM.YDReqIssueMasterID Desc" : paginationInfo.OrderBy;
                    }
                }
                else if (pageName == "YarnDyeingRequisitionAcknowledge")
                {
                    if (status == Status.Approved) //Approve List, YD Yarn Requisiton
                    {
                        sql = $@"
                        /*;With YRM As 
                        (
	                        Select * FROM {TableNames.YD_REQ_ISSUE_MASTER} 
	                        WHERE IsApprove = 1
	                        AND IsAcknowledge = 0
                        )
                        Select YRM.YDReqIssueMasterID, YRM.YDReqIssueNo, YRM.YDReqIssueDate, YRM.YDReqIssueBy, YRM.YDReqMasterID,
                        YRM.IsSendForApprove, RL.Name As SendForApproveName,  YRM.SendForApproveDate, YRM.IsApprove, A.Name As ApproveName, YRM.ApproveDate, 
                        YRM.IsAcknowledge, AcW.Name As AcknowledgeName, YRM.AcknowledgeDate, YRM.IsReject, R.Name As RejectName, YRM.RejectDate, YRM.AddedBy,
                        Sum(YRC.ReqQty)ReqQty, Sum(YRC.IssueQty)IssueQty, Sum(YRC.IssueQtyCarton)IssueQtyCarton, Sum(YRC.IssueQtyCone)IssueQtyCone  
                        From YRM Inner JOIN {TableNames.YD_REQ_ISSUE_CHILD} YRC ON YRC.YDReqIssueMasterID = YRM.YDReqIssueMasterID  
                        LEFT Join  {DbNames.EPYSL}..LoginUser RL On RL.UserCode = YRM.SendForApproveBy 
                        LEFT Join  {DbNames.EPYSL}..LoginUser A On A.UserCode = YRM.ApproveBy
                        LEFT Join  {DbNames.EPYSL}..LoginUser AcW On AcW.UserCode = YRM.AcknowledgeBy
                        LEFT Join  {DbNames.EPYSL}..LoginUser R On R.UserCode = YRM.RejectBy 
                        Group By YRM.YDReqIssueMasterID, YRM.YDReqIssueNo, YRM.YDReqIssueDate, YRM.YDReqIssueBy, YRM.YDReqMasterID,
                        YRM.IsSendForApprove, RL.Name,  YRM.SendForApproveDate, YRM.IsApprove, A.Name, YRM.ApproveDate, 
                        YRM.IsAcknowledge, AcW.Name, YRM.AcknowledgeDate, YRM.IsReject, R.Name, YRM.RejectDate, YRM.AddedBy*/

                        ;With
                        M As (
	                    Select M.YDReqMasterID, YDReqNo, LU.Name ReqByUser, YDReqDate, BM.YDBookingNo,BU.Name BookingByUser, CT.ShortName BuyerName, SUM(C.ReqQty) ReqQty,FM.ConceptNo,BM.YDBookingDate 
	                    FROM {TableNames.YD_REQ_MASTER} M
						Inner Join {DbNames.EPYSL}..LoginUser LU On M.YDReqBy = LU.UserCode
	                    Inner JOIN {TableNames.YD_REQ_CHILD} C On M.YDReqMasterID = C.YDReqMasterID
	                    Inner JOIN {TableNames.YD_BOOKING_MASTER} BM On M.YDBookingMasterID = BM.YDBookingMasterID
                        Inner Join {DbNames.EPYSL}..LoginUser BU On BM.YDBookingBy = BU.UserCode
						Inner JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FM ON FM.ConceptID = BM.ConceptID
	                    Inner Join {DbNames.EPYSL}..Contacts CT On BM.BuyerID = CT.ContactID
						Where M.IsAcknowledge=0
	                    Group By M.YDReqMasterID, YDReqNo, LU.Name,  YDReqDate, BM.YDBookingNo,BU.Name, CT.ShortName,FM.ConceptNo,BM.YDBookingDate
                    )

                        Select YDReqMasterID, YDReqNo, ReqByUser, YDReqDate, YDBookingNo, BuyerName, ReqQty,ConceptNo,YDBookingDate,BookingByUser, Count(*) Over() TotalRows  
                        From M

                        ";

                        orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YDReqMasterID Desc" : paginationInfo.OrderBy;
                    }
                    if (status == Status.Acknowledge) //Acknowledgement List
                    {
                        sql = $@"
                        /*;With YRM As 
                        (
	                        Select * FROM {TableNames.YD_REQ_ISSUE_MASTER} 
	                        WHERE IsApprove = 1
	                        AND IsAcknowledge = 1
                        )
                        Select YRM.YDReqIssueMasterID, YRM.YDReqIssueNo, YRM.YDReqIssueDate, YRM.YDReqIssueBy, YRM.YDReqMasterID,
                        YRM.IsSendForApprove, RL.Name As SendForApproveName,  YRM.SendForApproveDate, YRM.IsApprove, A.Name As ApproveName, YRM.ApproveDate, 
                        YRM.IsAcknowledge, AcW.Name As AcknowledgeName, YRM.AcknowledgeDate, YRM.IsReject, R.Name As RejectName, YRM.RejectDate, YRM.AddedBy,
                        Sum(YRC.ReqQty)ReqQty, Sum(YRC.IssueQty)IssueQty, Sum(YRC.IssueQtyCarton)IssueQtyCarton, Sum(YRC.IssueQtyCone)IssueQtyCone  
                        From YRM Inner JOIN {TableNames.YD_REQ_ISSUE_CHILD} YRC ON YRC.YDReqIssueMasterID = YRM.YDReqIssueMasterID  
                        LEFT Join  {DbNames.EPYSL}..LoginUser RL On RL.UserCode = YRM.SendForApproveBy 
                        LEFT Join  {DbNames.EPYSL}..LoginUser A On A.UserCode = YRM.ApproveBy
                        LEFT Join  {DbNames.EPYSL}..LoginUser AcW On AcW.UserCode = YRM.AcknowledgeBy
                        LEFT Join  {DbNames.EPYSL}..LoginUser R On R.UserCode = YRM.RejectBy 
                        Group By YRM.YDReqIssueMasterID, YRM.YDReqIssueNo, YRM.YDReqIssueDate, YRM.YDReqIssueBy, YRM.YDReqMasterID,
                        YRM.IsSendForApprove, RL.Name,  YRM.SendForApproveDate, YRM.IsApprove, A.Name, YRM.ApproveDate, 
                        YRM.IsAcknowledge, AcW.Name, YRM.AcknowledgeDate, YRM.IsReject, R.Name, YRM.RejectDate, YRM.AddedBy*/
                        
                        ;With
                        M As (
	                    Select M.YDReqMasterID, YDReqNo, LU.Name ReqByUser, YDReqDate, BM.YDBookingNo,BU.Name BookingByUser, CT.ShortName BuyerName, SUM(C.ReqQty) ReqQty,FM.ConceptNo,BM.YDBookingDate 
	                    FROM {TableNames.YD_REQ_MASTER} M
						Inner Join {DbNames.EPYSL}..LoginUser LU On M.YDReqBy = LU.UserCode
	                    Inner JOIN {TableNames.YD_REQ_CHILD} C On M.YDReqMasterID = C.YDReqMasterID
	                    Inner JOIN {TableNames.YD_BOOKING_MASTER} BM On M.YDBookingMasterID = BM.YDBookingMasterID
                        Inner Join {DbNames.EPYSL}..LoginUser BU On BM.YDBookingBy = BU.UserCode
						Inner JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FM ON FM.ConceptID = BM.ConceptID
	                    Inner Join {DbNames.EPYSL}..Contacts CT On BM.BuyerID = CT.ContactID
						Where M.IsAcknowledge=1
	                    Group By M.YDReqMasterID, YDReqNo, LU.Name,  YDReqDate, BM.YDBookingNo,BU.Name, CT.ShortName,FM.ConceptNo,BM.YDBookingDate
                    )

                        Select YDReqMasterID, YDReqNo, ReqByUser, YDReqDate, YDBookingNo, BuyerName, ReqQty,ConceptNo,YDBookingDate,BookingByUser, Count(*) Over() TotalRows  
                        From M
                        ";

                        orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YDReqMasterID Desc" : paginationInfo.OrderBy;
                    }
                }
            }
            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<YDReqMaster>(sql);
        }
        public async Task<YDReqIssueMaster> GetNewAsync(int YDReqMasterID)
        {
            var segmentNames = new
            {
                SegmentNames = new string[]
               {
                    ContactCategoryNames.SUPPLIER
               }
            };

            var query =
                $@"
                -- Master Data
                Select M.YDReqMasterID, M.YDReqNo, YDReqDate, M.YDBookingMasterID, BM.BuyerID, M.Remarks, BM.YDBookingDate, 
                BM.YDBookingNo, CT.ShortName BuyerName, FM.CompanyID,FM.ConceptNo
                FROM {TableNames.YD_REQ_MASTER} M Inner JOIN {TableNames.YD_BOOKING_MASTER} BM ON M.YDBookingMasterID = BM.YDBookingMasterID
				Inner JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FM ON FM.ConceptID = BM.ConceptID
                Inner Join {DbNames.EPYSL}..Contacts CT On CT.ContactID = BM.BuyerID
                Where M.YDReqMasterID = {YDReqMasterID};

                -- Childs Data
                ;With 
                YDBC As (
	                Select * From YDReqChild Where YDReqMasterID = {YDReqMasterID}
                )

                SELECT YDBC.YDReqChildID, YDBC.YDReqMasterID, YDBC.YarnProgramID, YDBC.ItemMasterID, YDBC.UnitID, 'Kg' As DisplayUnitDesc, 
                YDBC.BookingQty, YDBC.ReqQty, YDBC.Remarks, YDBC.YarnCategory, YDBC.NoOfThread, YDBC.ShadeCode,
                IM.Segment1ValueID Segment1ValueId, IM.Segment2ValueID Segment2ValueId, IM.Segment3ValueID Segment3ValueId, 
                IM.Segment4ValueID Segment4ValueId, IM.Segment5ValueID Segment5ValueId, IM.Segment6ValueID Segment6ValueId, 
                IM.Segment7ValueID Segment7ValueId, IM.Segment8ValueID Segment8ValueId,
                ISV1.SegmentValue AS Segment1ValueDesc, ISV2.SegmentValue AS Segment2ValueDesc, ISV3.SegmentValue AS Segment3ValueDesc,
                ISV4.SegmentValue AS Segment4ValueDesc, ISV5.SegmentValue AS Segment5ValueDesc, ISV6.SegmentValue AS Segment6ValueDesc,
                ISV7.SegmentValue AS Segment7ValueDesc, ISV8.SegmentValue AS Segment8ValueDesc
                FROM YDBC INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YDBC.ItemMasterID 
                LEFT Join {DbNames.EPYSL}..EntityTypeValue EV On YDBC.YarnProgramID = EV.ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID
                LEFT JOIN {DbNames.EPYSL}..Unit UN ON YDBC.UnitID = UN.UnitID;

                -- Company List
                Select Cast (CE.CompanyID as varchar) [id] , C.Name + '(' + C.ShortName + ')' [text]
                From (select SubGroupID, ContactID from {DbNames.EPYSL}..SupplierItemGroupStatus Group By SubGroupID, ContactID) SIGS
                Inner Join {DbNames.EPYSL}..Contacts C On SIGS.ContactID = C.ContactID
                Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SIGS.SubGroupID
                Inner Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = SIGS.ContactID
                Inner Join {DbNames.EPYSL}..CompanyEntity CE On C.MappingCompanyID = CE.CompanyID
                Where ISG.SubGroupName = '{AppConstants.ITEM_SUB_GROUP_FABRIC}' And Isnull(CAI.InHouse,0) = 1
                Group by CE.CompanyID, C.Name, C.ShortName; 

                -- Return Company List
                Select Cast (CE.CompanyID as varchar) [id] , C.Name + '(' + C.ShortName + ')' [text]
                From (select SubGroupID, ContactID from {DbNames.EPYSL}..SupplierItemGroupStatus Group By SubGroupID, ContactID) SIGS
                Inner Join {DbNames.EPYSL}..Contacts C On SIGS.ContactID = C.ContactID
                Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SIGS.SubGroupID
                Inner Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = SIGS.ContactID
                Inner Join {DbNames.EPYSL}..CompanyEntity CE On C.MappingCompanyID = CE.CompanyID
                Where ISG.SubGroupName = '{AppConstants.ITEM_SUB_GROUP_FABRIC}' And Isnull(CAI.InHouse,0) = 1
                Group by CE.CompanyID, C.Name, C.ShortName; 

                -- Buyer List
                 ;{CommonQueries.GetContactsByCategoryType(ContactCategoryNames.BUYER)}

                -- Store Location List
                SELECT CAST(LocationID AS VARCHAR) id, LocationName text
                FROM {DbNames.EPYSL}..Location
                WHERE LocationID IN(3,5,10); 

                -- Supplier List
                ;{CommonQueries.GetYarnSuppliers()}

                -- Yarn Spinner List
                /* ;{CommonQueries.GetYarnSpinners()} */ 
 
                ;Select Cast(C.ContactID As varchar) [id], C.Name [text]
                From {DbNames.EPYSL}..Contacts C
                Inner Join {DbNames.EPYSL}..ContactCategoryChild CCC On C.ContactID = CCC.ContactID
                Inner Join {DbNames.EPYSL}..ContactCategoryHK CC ON CC.ContactCategoryID = CCC.ContactCategoryID
                Where CC.ContactCategoryName = '{ContactCategoryNames.SPINNER}'
                Union
                Select Cast(ContactID As varchar) [id], Name [text] from {DbNames.EPYSL}..Contacts; ";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);

                YDReqIssueMaster data = records.Read<YDReqIssueMaster>().FirstOrDefault();
                data.Childs = records.Read<YDReqIssueChild>().ToList();
                data.CompanyList = await records.ReadAsync<Select2OptionModel>();
                data.RCompanyList = await records.ReadAsync<Select2OptionModel>();
                data.BuyerList = await records.ReadAsync<Select2OptionModel>();
                data.LocationList = await records.ReadAsync<Select2OptionModel>();
                data.SupplierList = await records.ReadAsync<Select2OptionModel>();
                data.YarnBrandList = await records.ReadAsync<Select2OptionModel>();
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
        public async Task<YDReqIssueMaster> GetAsync(int id)
        {
            var query =
                $@"
                -- Master Data
                 Select IM.*,M.YDReqMasterID, M.YDReqNo, YDReqDate, M.YDBookingMasterID, BM.BuyerID, M.Remarks, BM.YDBookingDate, 
                BM.YDBookingNo, CT.ShortName BuyerName, FM.CompanyID,FM.ConceptNo FROM {TableNames.YD_REQ_ISSUE_MASTER} IM
				INNER JOIN {TableNames.YD_REQ_MASTER} M ON M.YDReqMasterID=IM.YDReqMasterID 
				Inner JOIN {TableNames.YD_BOOKING_MASTER} BM ON M.YDBookingMasterID = BM.YDBookingMasterID
				Inner JOIN {TableNames.RND_FREE_CONCEPT_MASTER} FM ON FM.ConceptID = BM.ConceptID
                Inner Join {DbNames.EPYSL}..Contacts CT On CT.ContactID = BM.BuyerID
				Where IM.YDReqIssueMasterID  = {id};

                -- Childs Data
                ;With 
                YDBC As
                (
	                Select YDReqIssueChildID, YDReqIssueMasterID, YarnProgramID, ItemMasterID, UnitID, ReqQty, IssueQty, IssueQtyCone, IssueQtyCarton,
	                Remarks, YarnCategory, NoOfThread, LotNo, PhysicalCount, YarnBrandID, Rate, ShadeCode 
	                FROM {TableNames.YD_REQ_ISSUE_CHILD}
	                Where YDReqIssueMasterID = {id}
                )

                Select YDBC.YDReqIssueChildID, YDBC.YDReqIssueMasterID, YDBC.YarnProgramID, IM.ItemMasterID, YDBC.UnitID, YDBC.ReqQty, YDBC.IssueQty, 
                YDBC.IssueQtyCone, YDBC.IssueQtyCarton, YDBC.Remarks, YDBC.YarnCategory, YDBC.NoOfThread, YDBC.LotNo,
                YDBC.PhysicalCount, YDBC.YarnBrandID, CT.ShortName YarnBrand, YDBC.Rate, YDBC.ShadeCode,

                UN.DisplayUnitDesc, 
                IM.Segment1ValueID Segment1ValueId, IM.Segment2ValueID Segment2ValueId, IM.Segment3ValueID Segment3ValueId, 
                IM.Segment4ValueID Segment4ValueId, IM.Segment5ValueID Segment5ValueId, IM.Segment6ValueID Segment6ValueId, 
                IM.Segment7ValueID Segment7ValueId, IM.Segment8ValueID Segment8ValueId,
                ISV1.SegmentValue AS Segment1ValueDesc, ISV2.SegmentValue AS Segment2ValueDesc, ISV3.SegmentValue AS Segment3ValueDesc,
                ISV4.SegmentValue AS Segment4ValueDesc, ISV5.SegmentValue AS Segment5ValueDesc, ISV6.SegmentValue AS Segment6ValueDesc,
                ISV7.SegmentValue AS Segment7ValueDesc, ISV8.SegmentValue AS Segment8ValueDesc
                From YDBC INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YDBC.ItemMasterID 
                LEFT Join {DbNames.EPYSL}..EntityTypeValue EV On YDBC.YarnProgramID = EV.ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 ON ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV8 ON ISV8.SegmentValueID = IM.Segment8ValueID
                Left Join {DbNames.EPYSL}..Contacts CT On CT.ContactID = YDBC.YarnBrandID
                LEFT JOIN {DbNames.EPYSL}..Unit UN ON YDBC.UnitID = UN.UnitID;

                -- Company List
                Select Cast (CE.CompanyID as varchar) [id] , C.Name + '(' + C.ShortName + ')' [text]
                From (select SubGroupID, ContactID from {DbNames.EPYSL}..SupplierItemGroupStatus Group By SubGroupID, ContactID) SIGS
                Inner Join {DbNames.EPYSL}..Contacts C On SIGS.ContactID = C.ContactID
                Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SIGS.SubGroupID
                Inner Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = SIGS.ContactID
                Inner Join {DbNames.EPYSL}..CompanyEntity CE On C.MappingCompanyID = CE.CompanyID
                Where ISG.SubGroupName = '{AppConstants.ITEM_SUB_GROUP_FABRIC}' And Isnull(CAI.InHouse,0) = 1
                Group by CE.CompanyID, C.Name, C.ShortName; 

                -- Return Company List
                Select Cast (CE.CompanyID as varchar) [id] , C.Name + '(' + C.ShortName + ')' [text]
                From (select SubGroupID, ContactID from {DbNames.EPYSL}..SupplierItemGroupStatus Group By SubGroupID, ContactID) SIGS
                Inner Join {DbNames.EPYSL}..Contacts C On SIGS.ContactID = C.ContactID
                Inner Join {DbNames.EPYSL}..ItemSubGroup ISG On ISG.SubGroupID = SIGS.SubGroupID
                Inner Join {DbNames.EPYSL}..ContactAdditionalInfo CAI On CAI.ContactID = SIGS.ContactID
                Inner Join {DbNames.EPYSL}..CompanyEntity CE On C.MappingCompanyID = CE.CompanyID
                Where ISG.SubGroupName = '{AppConstants.ITEM_SUB_GROUP_FABRIC}' And Isnull(CAI.InHouse,0) = 1
                Group by CE.CompanyID, C.Name, C.ShortName; 

                -- Buyer List
                 ;{CommonQueries.GetContactsByCategoryType(ContactCategoryNames.BUYER)}

                -- Store Location List
                SELECT CAST(LocationID AS VARCHAR) id, LocationName text
                FROM {DbNames.EPYSL}..Location
                WHERE LocationID IN(3,5,10); 

                -- Supplier List
                ;{CommonQueries.GetYarnSuppliers()} 

                -- Yarn Spinner List
                /* ;{CommonQueries.GetYarnSpinners()} */ 
 
                ;Select Cast(C.ContactID As varchar) [id], C.Name [text]
                From {DbNames.EPYSL}..Contacts C
                Inner Join {DbNames.EPYSL}..ContactCategoryChild CCC On C.ContactID = CCC.ContactID
                Inner Join {DbNames.EPYSL}..ContactCategoryHK CC ON CC.ContactCategoryID = CCC.ContactCategoryID
                Where CC.ContactCategoryName = '{ContactCategoryNames.SPINNER}'
                Union
                Select Cast(ContactID As varchar) [id], Name [text] from {DbNames.EPYSL}..Contacts; ";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);

                YDReqIssueMaster data = await records.ReadFirstOrDefaultAsync<YDReqIssueMaster>();
                data.Childs = records.Read<YDReqIssueChild>().ToList();
                data.CompanyList = await records.ReadAsync<Select2OptionModel>();
                data.RCompanyList = await records.ReadAsync<Select2OptionModel>();
                data.BuyerList = await records.ReadAsync<Select2OptionModel>();
                data.LocationList = await records.ReadAsync<Select2OptionModel>();
                data.SupplierList = await records.ReadAsync<Select2OptionModel>();
                data.YarnBrandList = await records.ReadAsync<Select2OptionModel>();
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
        public async Task<YDReqIssueMaster> GetAllAsync(int id)
        {
            var sql = $@"
            ;Select * FROM {TableNames.YD_REQ_ISSUE_MASTER} Where YDReqIssueMasterID = {id}

            ;Select * FROM {TableNames.YD_REQ_ISSUE_CHILD} Where YDReqIssueMasterID = {id}

            ;Select RBM.* FROM {TableNames.YD_REQ_ISSUE_CHILD}RackBinMapping RBM
            INNER JOIN {TableNames.YD_REQ_ISSUE_CHILD} RIC ON RIC.YDReqIssueChildID = RBM.YDReqIssueChildID 
            Where RIC.YDReqIssueMasterID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YDReqIssueMaster data = await records.ReadFirstOrDefaultAsync<YDReqIssueMaster>();
                Guard.Against.NullObject(data);
                data.Childs = records.Read<YDReqIssueChild>().ToList();
                var rackBinMappingList = records.Read<YDReqIssueChildRackBinMapping>().ToList();
                if (rackBinMappingList == null) rackBinMappingList = new List<YDReqIssueChildRackBinMapping>();

                data.Childs.ForEach(c =>
                {
                    c.ChildRackBins = rackBinMappingList.Where(y => y.YDReqIssueChildID == c.YDReqIssueChildID).ToList();
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
        public async Task SaveAsync(YDReqIssueMaster entity, List<YarnReceiveChildRackBin> rackBins = null)
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
                        entity.YDReqIssueMasterID = await _service.GetMaxIdAsync(TableNames.YD_REQ_ISSUE_MASTER, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        entity.YDReqIssueNo = await _service.GetMaxNoAsync(TableNames.YD_REQ_ISSUE_NO, 1, RepeatAfterEnum.NoRepeat, "00000", transactionGmt, _connectionGmt);
                        maxChildId = await _service.GetMaxIdAsync(TableNames.YD_REQ_ISSUE_CHILD, entity.Childs.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        foreach (var item in entity.Childs)
                        {
                            item.YDReqIssueChildID = maxChildId++;
                            item.YDReqIssueMasterID = entity.YDReqIssueMasterID;
                        }
                        break;

                    case EntityState.Modified:
                        var addedChilds = entity.Childs.FindAll(x => x.EntityState == EntityState.Added);
                        maxChildId = await _service.GetMaxIdAsync(TableNames.YD_REQ_ISSUE_CHILD, addedChilds.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        foreach (var item in addedChilds)
                        {
                            item.YDReqIssueChildID = maxChildId++;
                            item.YDReqIssueMasterID = entity.YDReqIssueMasterID;
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
                if (rackBins != null)
                {
                    await _service.SaveAsync(rackBins, transaction);
                }
                #region Stock Operation
                /*
                if (entity.IsApprove)
                {
                    int userId = entity.EntityState == EntityState.Added ? entity.AddedBy : entity.ApproveBy;
                    if (entity.ProgramName != null)
                    {
                        if (entity.ProgramName.ToUpper() == "RND")
                        {
                            await _connection.ExecuteAsync("spYarnStockOperation", new { MasterID = entity.YDReqIssueMasterID, FromMenuType = EnumFromMenuType.YDReqIssueRAndD, UserId = userId }, transaction, 30, CommandType.StoredProcedure);
                        }
                        else if (entity.ProgramName.ToUpper() == "BULK")
                        {
                            await _connection.ExecuteAsync("spYarnStockOperation", new { MasterID = entity.YDReqIssueMasterID, FromMenuType = EnumFromMenuType.YDReqIssueBulk, UserId = userId }, transaction, 30, CommandType.StoredProcedure);
                        }
                    }
                }
                */
                #endregion Stock Operation
                #region Stock Operation
                //if (entity.IsApprove && entity.IsGPApprove == false)
                //{
                //    if (entity.ApproveBy.IsNull()) entity.ApproveBy = 0;
                //    int userId = entity.EntityState == EntityState.Added ? entity.AddedBy : entity.ApproveBy;
                //    await _connection.ExecuteAsync("spYarnStockOperation", new { MasterID = entity.YDReqIssueMasterID, FromMenuType = EnumFromMenuType.YDIssueApp, UserId = userId }, transaction, 30, CommandType.StoredProcedure);
                //}
                #endregion Stock Operation

                transaction.Commit();
                transactionGmt.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                if (transactionGmt != null) transactionGmt.Rollback();
                if (ex.Message.Contains('~')) throw new Exception(ex.Message.Split('~')[0]);
                throw ex;
            }
            finally
            {
                if (transaction != null) transaction.Dispose();
                if (transactionGmt != null) transactionGmt.Dispose();
                _connection.Close();
                _connectionGmt.Close();
            }
        }


        public async Task SaveAsyncYD(YDReqIssueMaster entity, List<YarnReceiveChildRackBin> rackBins)
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
                int maxYDRICRBId = 0;
                entity.Childs.ForEach(x =>
                {
                    maxYDRICRBId += x.ChildRackBins.Count(y => y.EntityState == EntityState.Added);
                });
                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity.YDReqIssueMasterID = await _service.GetMaxIdAsync(TableNames.YD_REQ_ISSUE_MASTER, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        entity.YDReqIssueNo = await _service.GetMaxNoAsync(TableNames.YD_REQ_ISSUE_NO, 1, RepeatAfterEnum.NoRepeat, "00000", transactionGmt, _connectionGmt);
                        maxChildId = await _service.GetMaxIdAsync(TableNames.YD_REQ_ISSUE_CHILD, entity.Childs.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        maxYDRICRBId = await _service.GetMaxIdAsync(TableNames.YD_REQ_ISSUE_CHILD_CHILD_RACK_BIN_MAPPING, maxYDRICRBId, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        foreach (var item in entity.Childs)
                        {
                            item.YDReqIssueChildID = maxChildId++;
                            item.YDReqIssueMasterID = entity.YDReqIssueMasterID;
                            item.EntityState = EntityState.Added;

                            foreach (var itemObj in item.ChildRackBins)
                            {
                                itemObj.YDRICRBId = maxYDRICRBId++;
                                itemObj.YDReqIssueChildID = item.YDReqIssueChildID;
                                itemObj.EntityState = EntityState.Added;
                            }
                        }
                        break;

                    case EntityState.Modified:
                        var addedChilds = entity.Childs.FindAll(x => x.EntityState == EntityState.Added);
                        maxChildId = await _service.GetMaxIdAsync(TableNames.YD_REQ_ISSUE_CHILD, addedChilds.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);
                        maxYDRICRBId = await _service.GetMaxIdAsync(TableNames.YD_REQ_ISSUE_CHILD_CHILD_RACK_BIN_MAPPING, maxYDRICRBId, RepeatAfterEnum.NoRepeat, transactionGmt, _connectionGmt);

                        foreach (var item in addedChilds)
                        {
                            item.YDReqIssueChildID = maxChildId++;
                            item.YDReqIssueMasterID = entity.YDReqIssueMasterID;
                        }
                        entity.Childs.ForEach(c =>
                        {
                            if (c.EntityState == EntityState.Added)
                            {
                                c.YDReqIssueChildID = maxChildId++;
                                c.YDReqIssueMasterID = entity.YDReqIssueMasterID;
                                c.EntityState = EntityState.Added;
                            }
                            foreach (var itemObj in c.ChildRackBins.Where(x => x.EntityState == EntityState.Added).ToList())
                            {
                                itemObj.YDRICRBId = maxYDRICRBId++;
                                itemObj.YDReqIssueChildID = c.YDReqIssueChildID;
                                itemObj.EntityState = EntityState.Added;
                            }
                        });
                        break;

                    case EntityState.Unchanged:
                    case EntityState.Deleted:
                        entity.EntityState = EntityState.Deleted;
                        entity.Childs.SetDeleted();
                        entity.Childs.ForEach(x => x.ChildRackBins.SetDeleted());
                        break;

                    default:
                        break;
                }
                List<YDReqIssueChildRackBinMapping> rackBinList = new List<YDReqIssueChildRackBinMapping>();
                entity.Childs.ForEach(x =>
                {
                    rackBinList.AddRange(x.ChildRackBins);
                });

                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(rackBinList.Where(x => x.EntityState == EntityState.Deleted).ToList(), transaction);
                await _service.SaveAsync(entity.Childs, transaction);
                await _service.SaveAsync(rackBinList.Where(x => x.EntityState != EntityState.Deleted).ToList(), transaction);

                await _service.SaveAsync(rackBins, transaction);
                /*
                #region Stock Operation
                if (entity.IsApprove && entity.IsGPApprove == false)
                {
                    if (entity.ApproveBy.IsNull()) entity.ApproveBy = 0;
                    int userId = entity.EntityState == EntityState.Added ? entity.AddedBy : entity.ApproveBy;
                    await _connection.ExecuteAsync("spYarnStockOperation", new { MasterID = entity.YDReqIssueMasterID, FromMenuType = EnumFromMenuType.YDIssueApp, UserId = userId }, transaction, 30, CommandType.StoredProcedure);
                }
                #endregion Stock Operation
                */
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
                if (transaction != null) transaction.Dispose();
                if (transactionGmt != null) transactionGmt.Dispose();
                _connection.Close();
                _connectionGmt.Close();
            }
        }
        public async Task UpdateEntityAsync(YDReqIssueMaster entity)
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
