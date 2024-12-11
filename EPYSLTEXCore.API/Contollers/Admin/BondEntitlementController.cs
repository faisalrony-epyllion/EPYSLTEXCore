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
    [Route("api/bond-entitlement")]
    public class BondEntitlementController : ApiBaseController
    {
        private readonly IBondEntitlementService _service;

        //private readonly IEmailService _emailService;
        //private readonly IReportingService _reportingService;
        private readonly ICommonHelperService _commonService;
        private static Logger _logger;

        public BondEntitlementController(IUserService userService, IBondEntitlementService service
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
            List<BondEntitlementMaster> records = await _service.GetPagedAsync(status, paginationInfo);
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
            BondEntitlementMaster model = JsonConvert.DeserializeObject<BondEntitlementMaster>(Convert.ToString(model1));
            BondEntitlementMaster entity;

            if (model.BondEntitlementMasterID > 0)
            {
                entity = await _service.GetById(model.BondEntitlementMasterID);

                entity.UpdatedBy = AppUser.UserCode;
                entity.DateUpdated = DateTime.Now;
                entity.EntityState = EntityState.Modified;
                entity.CompanyID = model.CompanyID;
                entity.BondLicenceNo = model.BondLicenceNo;
                entity.EBINNo = model.EBINNo;
                entity.FromDate = model.FromDate;
                entity.ToDate = model.ToDate;
                entity.CurrencyID = model.CurrencyID;

                foreach (var item in entity.Childs)
                {
                    item.EntityState = EntityState.Unchanged;
                    item.ChildItems.SetUnchanged();
                }
                model.Childs.ForEach(modelChild =>
                {
                    var child = entity.Childs.FirstOrDefault(c => c.SegmentNameID == modelChild.SegmentNameID);
                    var indexChild = entity.Childs.FindIndex(c => c.SegmentNameID == modelChild.SegmentNameID);

                    if (child.IsNull())
                    {
                        child = modelChild;
                        child.EntityState = EntityState.Added;
                        entity.Childs.Add(child);
                    }
                    else
                    {
                        child.EntityState = EntityState.Modified;
                        child.HSCode = modelChild.HSCode;
                        child.UnitID = modelChild.UnitID;
                        child.BankFacilityAmount = modelChild.BankFacilityAmount;
                    }

                    modelChild.ChildItems.ForEach(modelChildItem =>
                    {
                        var childItem = child.ChildItems.FirstOrDefault(c => c.SegmentValueID == modelChildItem.SegmentValueID);
                        var indexChildItem = child.ChildItems.FindIndex(c => c.SegmentValueID == modelChildItem.SegmentValueID);

                        if (childItem.IsNull())
                        {
                            childItem = modelChildItem;
                            childItem.EntityState = EntityState.Added;
                        }
                        else
                        {
                            childItem.EntityState = EntityState.Modified;
                            childItem.HSCode = modelChildItem.HSCode;
                            childItem.BankFacilityAmount = modelChildItem.BankFacilityAmount;
                        }
                        if (indexChild > -1)
                        {
                            if (indexChildItem > -1)
                            {
                                entity.Childs[indexChild].ChildItems[indexChildItem] = childItem;
                            }
                            else
                            {
                                entity.Childs[indexChild].ChildItems.Add(childItem);
                            }
                        }
                    });
                });

                foreach (var item in entity.Childs.Where(x => x.EntityState == EntityState.Unchanged))
                {
                    item.EntityState = EntityState.Deleted;
                    item.ChildItems.SetDeleted();
                }
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
                    item.ChildItems.ForEach(x =>
                    {
                        x.EntityState = EntityState.Added;
                    });
                }
            }
            await _service.SaveAsync(entity);

            return Ok();
        }
    }
}
