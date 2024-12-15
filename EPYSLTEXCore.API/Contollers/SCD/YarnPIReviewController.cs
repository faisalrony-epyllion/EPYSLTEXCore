using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Web.Extends.Helpers;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Statics;
using EPYSLTEXCore.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.Entity;

namespace EPYSLTEX.Web.Controllers.Apis
{
    [Authorize]
    [Route("api/yarn-pi-review")]
    public class YarnPIReviewController : ApiBaseController
    {
        private readonly IYarnPIReviewService _service;
        private readonly ICommonHelpers _commonHelpers;

        public YarnPIReviewController(IUserService userService, IYarnPIReviewService YarnPIReviewService
            , ICommonHelpers commonHelpers):base(userService)
        {
            _service = YarnPIReviewService;
            _commonHelpers = commonHelpers;
        }

        [Route("list")]
        [HttpGet]
        public async Task<IActionResult> GetList(Status status, bool isCDAPage)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<YarnPIReceiveMaster> records = await _service.GetPagedAsync(status, isCDAPage, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));

            //var paginationInfo = Request.GetPaginationInfo();
            //List<YarnPIReceiveMaster> records = await _service.GetPagedAsync(status, isCDAPage, paginationInfo);
            //var totalCount = records.FirstOrDefault() == null ? 0 : records.FirstOrDefault().TotalRows;
            //var response = new TableResponseModel(totalCount, records);

            //return Ok(response);
        }

        [HttpGet]
        [Route("getData/{id}/{supplierId}/{companyId}/{isCDAPage}")]
        public async Task<IActionResult> GetData(int id, int supplierId, int companyId, bool isCDAPage)
        {
            return Ok(await _service.GetAsync(id, supplierId, companyId, isCDAPage));
        }

        [Route("save")]
        [HttpPost]
        public async Task<IActionResult> Save(dynamic jsnString)
        {
            YarnPIReceiveMaster model = JsonConvert.DeserializeObject<YarnPIReceiveMaster>(Convert.ToString(jsnString));
            YarnPIReceiveMaster entity = await _service.GetDetailsAsync(model.YPIReceiveMasterID);

       

            if (model.Accept)
            {
                entity.Accept = true;
                entity.AcceptBy = AppUser.UserCode;
                entity.AcceptDate = DateTime.Now;
                entity.Reject = false;
                entity.RejectBy = 0;
                entity.RejectDate = null;
                entity.RejectReason = "";
            }
            else
            {
                entity.Reject = true;
                entity.RejectBy = AppUser.UserCode;
                entity.RejectDate = DateTime.Now;
                entity.RejectReason = model.RejectReason;
                entity.Accept = false;
                entity.AcceptBy = 0;
                entity.AcceptDate = null;
                entity.NeedsReview = false;
            }
            entity.EntityState = EntityState.Modified;
            

            await _service.SaveAsync(entity);
            return Ok();
        }
    }
}