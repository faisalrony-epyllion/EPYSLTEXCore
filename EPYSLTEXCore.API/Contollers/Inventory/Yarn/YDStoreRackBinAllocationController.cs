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
    [Route("api/yd-store-rack-bin-allocation")]
    public class YDStoreRackBinAllocationController : ApiBaseController
    {
        private readonly IYDStoreRackBinAllocationService _service;
        private readonly ICommonHelpers _commonHelpers;
        public YDStoreRackBinAllocationController(IUserService userService,
             IYDStoreRackBinAllocationService service
            , ICommonHelpers commonHelpers
           ) : base(userService)
        {
            _service = service;
            _commonHelpers = commonHelpers;
        }

        [Route("list")]
        [HttpGet]
        public async Task<IActionResult> GetList(Status status, bool isCDAPage)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<YDStoreReceiveMaster> records = await _service.GetPagedAsync(status, isCDAPage, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [Route("{id}/{companyId}")]
        [HttpGet]
        public async Task<IActionResult> Get(int id, int companyId)
        {
            var record = await _service.GetAsync(id, companyId);
            Guard.Against.NullObject(id, record);
            return Ok(record);
        }

        [Route("get-by-receive-child/{receiveChildId}/{locationId}/{qcReturnReceivedChildId}")]
        [HttpGet]
        public async Task<IActionResult> GetRackBin(int receiveChildId, int locationId, int qcReturnReceivedChildId)
        {
            var record = await _service.GetRackBin(receiveChildId, locationId, qcReturnReceivedChildId);
            Guard.Against.NullObject(receiveChildId, record);
            return Ok(record);
        }
        [HttpGet]
        [Route("all-rack-list")]
        public async Task<IActionResult> GetAllRacks()
        {
            var paginationInfo = Request.GetPaginationInfo();
            var records = await _service.GetAllRacks(paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }
        [Route("save")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Save(dynamic jsonString)
        {
            YDStoreReceiveMaster model = JsonConvert.DeserializeObject<SendToYDStoreMaster>(Convert.ToString(jsonString));
            YDStoreReceiveMaster entity = await _service.GetAllAsync(model.YDStoreReceiveMasterID);
            entity.Childs.ForEach(c =>
            {
                c.EntityState = EntityState.Unchanged;
                c.YDStoreReceiveChildRackBins.SetUnchanged();
            });

            if (model.Childs.Sum(x => x.YDStoreReceiveChildRackBins.Where(y => y.EntityState != EntityState.Deleted).Count()) == 0)
            {
                throw new Exception("No Rack/Bin Allocation Found!!");
            }

            entity.YDStoreReceiveDate = model.YDStoreReceiveDate;
            entity.EntityState = EntityState.Modified;

            entity.Childs.SetUnchanged();

            YDStoreReceiveChild yarnReceiveChildEntity = new YDStoreReceiveChild();
            YDStoreReceiveChildRackBin yarnReceiveChildRackBinEntity = new YDStoreReceiveChildRackBin();

            foreach (YDStoreReceiveChild child in model.Childs)
            {
                int indexChild = entity.Childs.FindIndex(x => x.YDStoreReceiveChildID == child.YDStoreReceiveChildID);
                yarnReceiveChildEntity = entity.Childs.FirstOrDefault(x => x.YDStoreReceiveChildID == child.YDStoreReceiveChildID);
                yarnReceiveChildEntity.EntityState = EntityState.Modified;

                if (indexChild > -1)
                {
                    entity.Childs[indexChild] = yarnReceiveChildEntity;
                }
                foreach (YDStoreReceiveChildRackBin item in child.YDStoreReceiveChildRackBins)
                {
                    int indexRack = yarnReceiveChildEntity.YDStoreReceiveChildRackBins.FindIndex(x => x.ChildRackBinID == item.ChildRackBinID);

                    yarnReceiveChildRackBinEntity = yarnReceiveChildEntity.YDStoreReceiveChildRackBins.FirstOrDefault(x => x.ChildRackBinID == item.ChildRackBinID);

                    if (item.EntityState == EntityState.Deleted)
                    {
                        yarnReceiveChildRackBinEntity.EntityState = EntityState.Deleted;
                    }
                    else
                    {
                        bool isUpdateList = true;
                        if (yarnReceiveChildRackBinEntity == null || yarnReceiveChildRackBinEntity.ChildRackBinID == 0)
                        {
                            isUpdateList = false;

                            yarnReceiveChildRackBinEntity = CommonFunction.DeepClone(item);
                            yarnReceiveChildRackBinEntity.ItemMasterID = child.ItemMasterID;
                            yarnReceiveChildRackBinEntity.YarnCategory = child.YarnCategory;
                            yarnReceiveChildRackBinEntity.SupplierID = child.SupplierID;
                            yarnReceiveChildRackBinEntity.SpinnerID = child.SpinnerID;
                            yarnReceiveChildRackBinEntity.ShadeCode = child.ShadeCode;
                            yarnReceiveChildRackBinEntity.LotNo = child.LotNo;
                            yarnReceiveChildRackBinEntity.PhysicalCount = child.PhysicalCount;
                            yarnReceiveChildRackBinEntity.ShadeCode = child.ShadeCode;
                            yarnReceiveChildRackBinEntity.AddedBy = AppUser.UserCode;
                            yarnReceiveChildRackBinEntity.EntityState = EntityState.Added;
                            yarnReceiveChildEntity.YDStoreReceiveChildRackBins.Add(yarnReceiveChildRackBinEntity);
                        }
                        else
                        {
                            yarnReceiveChildRackBinEntity.ItemMasterID = child.ItemMasterID;
                            yarnReceiveChildRackBinEntity.YarnCategory = child.YarnCategory;
                            yarnReceiveChildRackBinEntity.SupplierID = child.SupplierID;
                            yarnReceiveChildRackBinEntity.SpinnerID = child.SpinnerID;
                            yarnReceiveChildRackBinEntity.ShadeCode = child.ShadeCode;
                            yarnReceiveChildRackBinEntity.LotNo = child.LotNo;
                            yarnReceiveChildRackBinEntity.PhysicalCount = child.PhysicalCount;
                            yarnReceiveChildRackBinEntity.ShadeCode = child.ShadeCode;
                            yarnReceiveChildRackBinEntity.LocationID = item.LocationID;
                            yarnReceiveChildRackBinEntity.RackID = item.RackID;
                            yarnReceiveChildRackBinEntity.BinID = item.BinID;
                            yarnReceiveChildRackBinEntity.NoOfCartoon = item.NoOfCartoon;
                            yarnReceiveChildRackBinEntity.NoOfCone = item.NoOfCone;
                            yarnReceiveChildRackBinEntity.ReceiveQty = item.ReceiveQty;
                            yarnReceiveChildRackBinEntity.Remarks = item.Remarks;
                            yarnReceiveChildRackBinEntity.EmployeeID = item.EmployeeID;
                            yarnReceiveChildRackBinEntity.EntityState = EntityState.Modified;
                            yarnReceiveChildRackBinEntity.UpdatedBy = AppUser.UserCode;
                            yarnReceiveChildRackBinEntity.DateUpdated = DateTime.Now;
                        }
                        if (isUpdateList && indexChild > -1 && indexRack > -1)
                        {
                            entity.Childs[indexChild].YDStoreReceiveChildRackBins[indexRack] = yarnReceiveChildRackBinEntity;
                        }
                    }
                }
            }

            entity.CompleteAllocation = false;
            entity.PartialAllocation = false;

            for (int i = 0; i < entity.Childs.Count(); i++)
            {
                var rackBins = entity.Childs[i].YDStoreReceiveChildRackBins;

                var totalRackCartoon = rackBins.Sum(x => x.NoOfCartoon);
                var totalRackCone = rackBins.Sum(x => x.NoOfCone);
                var totalRackReceiveQty = rackBins.Sum(x => x.ReceiveQty);

                if (totalRackCartoon < entity.Childs[i].ReceiveCarton)
                {
                    entity.PartialAllocation = true;
                    break;
                }
                else if (totalRackCone < entity.Childs[i].ReceiveCone)
                {
                    entity.PartialAllocation = true;
                    break;
                }
                else if (totalRackCone < entity.Childs[i].ReceiveCone)
                {
                    entity.PartialAllocation = true;
                    break;
                }
            }
            if (!entity.PartialAllocation) entity.CompleteAllocation = true;

            await _service.SaveAsync(entity);

            return Ok();
        }
    }
}
