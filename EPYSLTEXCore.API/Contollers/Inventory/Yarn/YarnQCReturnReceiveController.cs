using Azure.Core;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.API.Extends.Filters;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Mvc;
using System.Data.Entity;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EnumRackBinOperationType = EPYSLTEXCore.Infrastructure.Static.EnumRackBinOperationType;
using Newtonsoft.Json;
using EPYSLTEX.Web.Extends.Helpers;


namespace EPYSLTEXCore.API.Contollers.Inventory.Yarn
{

    [Route("api/yarn-qc-returnreceive")]
    public class YarnQCReturnReceiveController : ApiBaseController
    {

        private readonly IYarnQCReturnReceiveService _service;
        private readonly IYarnRackBinAllocationService _serviceRackBin;
        private readonly ICommonHelpers _commonHelpers;


        public YarnQCReturnReceiveController(ICommonHelpers commonHelpers, IUserService userService, IYarnQCReturnReceiveService service,
                                                IYarnRackBinAllocationService serviceRackBin) : base(userService)
        {
            _commonHelpers = commonHelpers;
            _service = service;
            _serviceRackBin = serviceRackBin;
        }

        [Route("list")]
        [HttpGet]
        public async Task<IActionResult> GetList(Status status, int offset = 0, int limit = 10, string filter = null, string sort = null, string order = null)
        {
            var filterBy = _commonHelpers.GetFilterBy(filter);
            var orderBy = string.IsNullOrEmpty(sort) ? "" : $"ORDER BY {sort} {order}";

            var records = await _service.GetPagedAsync(status, offset, limit, filterBy, orderBy);
            var totalCount = records.FirstOrDefault() == null ? 0 : records.FirstOrDefault().TotalRows;
            var response = new TableResponseModel(totalCount, records);

            return Ok(response);
        }

        [HttpGet]
        [Route("new/{reqMasterId}")]
        public async Task<IActionResult> GetNew(int reqMasterId)
        {
            return Ok(await _service.GetNewAsync(reqMasterId));
        }

        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var record = await _service.GetAsync(id);
            Guard.Against.NullObject(id, record);

            return Ok(record);
        }

        [Route("save")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Save(dynamic jsonString)
        {

            YarnQCReturnReceivedMaster model = JsonConvert.DeserializeObject<YarnQCReturnReceivedMaster>(Convert.ToString(jsonString));
            YarnQCReturnReceivedMaster entity;
            List<YarnReceiveChildRackBin> rackBins = new List<YarnReceiveChildRackBin>();

            #region Rack Bin List
            List<int> rackBinIds = new List<int>();

            model.Childs.ForEach(c =>
            {
                rackBinIds.AddRange(c.ChildRackBins.Select(x => x.ChildRackBinID));
            });
            string sChildRackBinIDs = string.Join(",", rackBinIds.Distinct());

            if (sChildRackBinIDs.IsNotNullOrEmpty())
            {
                rackBins = await _serviceRackBin.GetRackBinById(sChildRackBinIDs);
                rackBins.SetModified();
            }
            #endregion

            if (model.IsModified)
            {
                entity = await _service.GetAllAsync(model.QCReturnReceivedMasterID);

                entity.QCReturnReceivedDate = model.QCReturnReceivedDate;
                entity.UpdatedBy = AppUser.UserCode;
                entity.DateUpdated = DateTime.Now;
                entity.EntityState = EntityState.Modified;

                entity.Childs.ForEach(x =>
                {
                    x.EntityState = EntityState.Unchanged;
                    x.ChildRackBins.SetUnchanged();
                });

                foreach (var item in model.Childs)
                {
                    var childEntity = entity.Childs.FirstOrDefault(x => x.QCReturnReceivedChildID == item.QCReturnReceivedChildID);
                    int childIndexF = entity.Childs.FindIndex(x => x.QCReturnReceivedChildID == item.QCReturnReceivedChildID);

                    if (childEntity == null)
                    {
                        childEntity = item;
                        childEntity.QCReturnReceivedMasterID = entity.QCReturnReceivedMasterID;
                        childEntity.EntityState = EntityState.Added;
                        entity.Childs.Add(childEntity);
                    }
                    else
                    {
                        entity.Childs[childIndexF].Remarks = item.Remarks;
                        entity.Childs[childIndexF].ReceiveQtyCarton = item.ReceiveQtyCarton;
                        entity.Childs[childIndexF].ReceiveQtyCone = item.ReceiveQtyCone;
                        entity.Childs[childIndexF].ReceiveQty = item.ReceiveQty;
                        entity.Childs[childIndexF].EntityState = EntityState.Modified;
                    }

                    foreach (var crbObj in item.ChildRackBins)
                    {
                        #region rack bin update
                        var childRackBins = childEntity.ChildRackBins.Where(x => x.ChildRackBinID == crbObj.ChildRackBinID).ToList();
                        int cone = 0;
                        int cartoon = 0;
                        decimal qtyKg = 0;

                        childEntity.ChildRackBins.ForEach(c =>
                        {
                            cone += c.ReceiveQtyCone;
                            cartoon += c.ReceiveCartoon;
                            qtyKg += c.ReceiveQtyKg;
                        });

                        cone = crbObj.ReceiveQtyCone - cone;
                        cartoon = crbObj.ReceiveCartoon - cartoon;
                        qtyKg = crbObj.ReceiveQtyKg - qtyKg;
                        #endregion

                        var childMapping = childEntity.ChildRackBins.FirstOrDefault(x => x.YQCRRId == crbObj.YQCRRId);
                        int indexF = childEntity.ChildRackBins.FindIndex(x => x.YQCRRId == crbObj.YQCRRId);

                        if (childMapping == null)
                        {
                            childMapping = CommonFunction.DeepClone(crbObj);
                            childMapping.QCReturnReceivedChildID = item.QCReturnReceivedChildID;
                            childMapping.EntityState = EntityState.Added;
                            childEntity.ChildRackBins.Add(childMapping);
                        }
                        else if (indexF > -1)
                        {
                            childEntity.ChildRackBins[indexF].ReceiveCartoon = crbObj.ReceiveCartoon;
                            childEntity.ChildRackBins[indexF].ReceiveQtyCone = crbObj.ReceiveQtyCone;
                            childEntity.ChildRackBins[indexF].ReceiveQtyKg = crbObj.ReceiveQtyKg;
                            childEntity.ChildRackBins[indexF].EntityState = EntityState.Modified;
                        }

                        #region rack bin update
                        rackBins = _serviceRackBin.GetRackBinWithUpdateValue(rackBins, crbObj.ChildRackBinID, EnumRackBinOperationType.Addition, cone, cartoon, qtyKg);
                        #endregion
                    }
                    if (childIndexF > -1)
                    {
                        entity.Childs[childIndexF] = childEntity;
                    }
                }

                foreach (var item in entity.Childs)
                {
                    if (item.EntityState == EntityState.Unchanged)
                    {
                        item.EntityState = EntityState.Deleted;
                    }
                    item.ChildRackBins.ForEach(rb =>
                    {
                        if (rb.EntityState == EntityState.Unchanged || item.EntityState == EntityState.Deleted)
                        {
                            rb.EntityState = EntityState.Deleted;

                            #region rack bin update
                            rackBins = _serviceRackBin.GetRackBinWithUpdateValue(rackBins, rb.ChildRackBinID, EnumRackBinOperationType.Deduction, rb.ReceiveQtyCone, rb.ReceiveCartoon, rb.ReceiveQtyKg);
                            #endregion
                        }
                    });
                }
            }
            else
            {
                entity = model;
                entity.AddedBy = AppUser.UserCode;
                entity.QCReturnReceivedBy = AppUser.UserCode;
                entity.DateAdded = DateTime.Now;

                entity.Childs.ForEach(c =>
                {
                    c.ChildRackBins.ForEach(rb =>
                    {
                        rackBins = _serviceRackBin.GetRackBinWithUpdateValue(rackBins, rb.ChildRackBinID, EnumRackBinOperationType.Addition, rb.ReceiveQtyCone, rb.ReceiveCartoon, rb.ReceiveQtyKg);
                    });
                });
            }
            entity.UserId = AppUser.UserCode;
            await _service.SaveAsync(entity, rackBins);
            return Ok();
        }
    }
}