using EPYSLTEX.Core.Interfaces;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Application.Interfaces.Admin;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Admin;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Knitting;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Logging;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json;
using NLog;
using System.Data.Entity;

namespace EPYSLTEXCore.API.Contollers.Admin
{
    [Route("api/bank-limit")]
    public class BankLimitController : ApiBaseController
    {
        private readonly IBankLimitService _service;
        private readonly ICommonHelperService _commonService;
        private static Logger _logger;

        public BankLimitController(IUserService userService, IBankLimitService service
            , ICommonHelperService commonService) : base(userService)
        {
            _service = service;
            _logger = LogManager.GetCurrentClassLogger();
            _commonService = commonService;
        }
        [Route("list")]
        public async Task<IActionResult> GetList(Status status)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<BankLimitMaster> records = await _service.GetPagedAsync(status, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }
        [HttpGet]
        [Route("new")]
        public async Task<IActionResult> GetNew()
        {
            return Ok(await _service.GetNewAsync());
        }
        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> GetAsync(int id)
        {
            return Ok(await _service.GetDetails(id));
        }
        [Route("save")]
        [HttpPost]

        public async Task<IActionResult> Save(dynamic model1)
        {
            BankLimitMaster model = JsonConvert.DeserializeObject<BankLimitMaster>(Convert.ToString(model1));
            BankLimitMaster entity;

            if (model.BankLimitMasterID > 0)
            {
                entity = await _service.GetById(model.BankLimitMasterID);

                entity.UpdatedBy = AppUser.UserCode;
                entity.DateUpdated = DateTime.Now;
                entity.EntityState = EntityState.Modified;
                entity.CompanyID = model.CompanyID;
                entity.BankID = model.BankID;
                entity.BankFacilityTypeID = model.BankFacilityTypeID;
                entity.AccumulatedLimit = model.AccumulatedLimit;
                entity.CurrencyID = model.CurrencyID;

                entity.Childs.SetUnchanged();

                model.Childs.ForEach(modelChild =>
                {
                    var child = entity.Childs.FirstOrDefault(c => c.BankLimitChildID == modelChild.BankLimitChildID);

                    if (child.IsNull())
                    {
                        child = modelChild;
                        child.EntityState = EntityState.Added;
                        entity.Childs.Add(child);
                    }
                    else
                    {
                        child.EntityState = EntityState.Modified;
                        child.FormBankFacilityID = modelChild.FormBankFacilityID;
                        child.LiabilityTypeID = modelChild.LiabilityTypeID;
                        child.FromTenureDay = modelChild.FromTenureDay;
                        child.ToTenureDay = modelChild.ToTenureDay;
                        child.MaxLimit = modelChild.MaxLimit;
                        child.LCOpened = modelChild.LCOpened;
                        child.LCAcceptenceGiven = modelChild.LCAcceptenceGiven;
                        child.PaymentOnMaturity = modelChild.PaymentOnMaturity;
                    }
                });
                entity.Childs.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();
            }
            else
            {
                entity = model;
                entity.EntityState = EntityState.Added;
                entity.AddedBy = AppUser.UserCode;
                entity.DateAdded = DateTime.Now;

                foreach (var item in entity.Childs)
                {
                    item.EntityState = EntityState.Added;
                }
            }
            await _service.SaveAsync(entity);

            return Ok();
        }
    }
}
