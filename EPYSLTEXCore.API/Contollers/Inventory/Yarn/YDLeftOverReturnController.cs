using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Web.Extends.Helpers;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.API.Extends.Filters;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NLog;
using System.Data.Entity;
using System.Reflection;

namespace EPYSLTEXCore.API.Contollers.Inventory.Yarn
{
    [Route("api/yd-left-over-return")]
    public class YDLeftOverReturnController : ApiBaseController
    {
        private readonly IYDLeftOverReturnService _YDLeftOverReturnService;
        private readonly ICommonHelpers _commonHelpers;

        public YDLeftOverReturnController(IUserService userService,
            IYDLeftOverReturnService YDLeftOverReturnService
            , ICommonHelpers commonHelpers) : base(userService)
        {
            _YDLeftOverReturnService = YDLeftOverReturnService;
            _commonHelpers = commonHelpers;
        }


        [Route("list")]
        [HttpGet]
        public async Task<IActionResult> GetList(Status status)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<YDLeftOverReturnMaster> records = await _YDLeftOverReturnService.GetPagedAsync(status, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }
        [HttpGet]
        [Route("new/{YDReqIssueMasterID}")]
        public async Task<IActionResult> GetNew(string YDReqIssueMasterID)
        {
            return Ok(await _YDLeftOverReturnService.GetNewAsync(YDReqIssueMasterID));
        }

        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var record = await _YDLeftOverReturnService.GetAsync(id);
            Guard.Against.NullObject(id, record);

            return Ok(record);
        }

        [Route("save")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Save(dynamic jsonString)
        {
            YDLeftOverReturnMaster model = JsonConvert.DeserializeObject<YDLeftOverReturnMaster>(Convert.ToString(jsonString));
            YDLeftOverReturnMaster entity = new YDLeftOverReturnMaster();
            if (model.IsModified)
            {
                entity = await _YDLeftOverReturnService.GetAllByIDAsync(model.YDLOReturnMasterID);
                entity.YDLOReturnMasterID = model.YDLOReturnMasterID;
                //entity.YDReqIssueMasterID = model.YDReqIssueMasterID;
                entity.RCompanyID = model.RCompanyID;
                entity.OCompanyID = model.OCompanyID;
                entity.LocationID = model.LocationID;
                entity.SupplierID = model.SupplierID;
                entity.SpinnerID = model.SpinnerID;
                entity.YDLOReturnNo = model.YDLOReturnNo;
                entity.YDLOReturnDate = model.YDLOReturnDate;

                entity.UpdatedBy = AppUser.UserCode;
                entity.DateUpdated = DateTime.Now;
                entity.EntityState = EntityState.Modified;

                entity.Childs.SetUnchanged();
                foreach (var item in model.Childs)
                {
                    var childEntity = entity.Childs.FirstOrDefault(x => x.YDLOReturnChildID == item.YDLOReturnChildID);
                    if (childEntity == null)
                    {
                        item.YDLOReturnMasterID = entity.YDLOReturnMasterID;
                        entity.Childs.Add(item);
                    }
                    else
                    {
                        childEntity.YDLOReturnChildID = item.YDLOReturnChildID;
                        childEntity.YDReqIssueChildID = item.YDReqIssueChildID;
                        childEntity.ItemMasterID = item.ItemMasterID;
                        childEntity.YarnCategory = item.YarnCategory;
                        childEntity.IssueQty = item.IssueQty;
                        childEntity.IssueQtyCarton = item.IssueQtyCarton;
                        childEntity.IssueCone = item.IssueCone;
                        //childEntity.ReturnQty = item.ReturnQty;
                        //childEntity.ReturnQtyCarton = item.ReturnQtyCarton;
                        //childEntity.ReturnQtyCone = item.ReturnQtyCone;
                        childEntity.UseableReturnQtyKG = item.UseableReturnQtyKG;
                        childEntity.UseableReturnQtyCone = item.UseableReturnQtyCone;
                        childEntity.UseableReturnQtyBag = item.UseableReturnQtyBag;
                        childEntity.UnuseableReturnQtyKG = item.UnuseableReturnQtyKG;
                        childEntity.UnuseableReturnQtyCone = item.UnuseableReturnQtyCone;
                        childEntity.UnuseableReturnQtyBag = item.UnuseableReturnQtyBag;
                        childEntity.YarnProgramID = item.YarnProgramID;
                        childEntity.LotNo = item.LotNo;
                        childEntity.EntityState = EntityState.Modified;
                    }
                }
                entity.Childs.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();

            }
            else
            {
                entity = model;
                entity.YDLOReturnDate = DateTime.Now;
                entity.DateAdded = DateTime.Now;
                entity.AddedBy = AppUser.UserCode;
                entity.YDLOReturnBy = AppUser.UserCode;

            }
            if (model.IsSendToMCD)
            {
                entity.IsSendToMCD = true;
                entity.SendToMCDDate = DateTime.Now;
                entity.SendToMCDBy = AppUser.UserCode;

            }
            else if (model.IsApprove)
            {
                entity.IsApprove = true;
                entity.ApproveDate = DateTime.Now;
                entity.ApproveBy = AppUser.UserCode;

            }
            await _YDLeftOverReturnService.SaveAsync(entity);

            return Ok();
        }
    }
}
