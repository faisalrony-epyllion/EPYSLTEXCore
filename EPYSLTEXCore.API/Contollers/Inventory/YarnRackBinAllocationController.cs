using AutoMapper;
using EPYSLTEX.Core.Interfaces;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Infrastructure.Services;
using EPYSLTEX.Web.Extends.Helpers;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.API.Extends.Filters;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.Entity;


namespace EPYSLTEXCore.API.Contollers.Inventory
{
   
    [Route("api/yarn-rack-bin-allocation")]
    public class YarnRackBinAllocationController : ApiBaseController
    {
        private readonly IYarnRackBinAllocationService _service;
        private readonly ICommonHelpers _commonHelpers;
        public YarnRackBinAllocationController(IUserService userService,
             IYarnRackBinAllocationService service
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
            List<YarnReceiveMaster> records = await _service.GetPagedAsync(status, isCDAPage, paginationInfo);
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
        [Route("get-by-knitting-returnReceive/{receiveChildId}/{locationId}/{kReturnReceivedChildId}/{returnFrom}")]
        [HttpGet]
        public async Task<IActionResult> GetRackBinForKnittingReturnRcv(int receiveChildId, int locationId, int kReturnReceivedChildId, string returnFrom = null)
        {
            List<YarnReceiveChildRackBin> record = new List<YarnReceiveChildRackBin>();
            if (returnFrom == "Bulk")
            {
                record = await _service.GetRackBinForKnittingReturnRcv(receiveChildId, locationId, kReturnReceivedChildId);
            }
            else if (returnFrom == "YD")
            {
                record = await _service.GetRackBinForYDReturnRcv(receiveChildId, locationId, kReturnReceivedChildId);
            }
            else if (returnFrom == "SC")
            {
                record = await _service.GetRackBinForSCReturnRcv(receiveChildId, locationId, kReturnReceivedChildId);
            }
            else if (returnFrom == "RND")
            {
                record = await _service.GetRackBinForRNDReturnRcv(receiveChildId, locationId, kReturnReceivedChildId);
            }
            Guard.Against.NullObject(receiveChildId, record);
            return Ok(record);
        }
        [Route("get-by-knitting-returnReceive-unuseable/{receiveChildId}/{locationId}/{kReturnReceivedChildId}/{returnFrom}")]
        [HttpGet]
        public async Task<IActionResult> GetRackBinForKnittingReturnRcvUnusable(int receiveChildId, int locationId, int kReturnReceivedChildId, string returnFrom = null)
        {
            List<YarnReceiveChildRackBin> record = new List<YarnReceiveChildRackBin>();
            if (returnFrom == "Bulk")
            {
                record = await _service.GetRackBinForKnittingReturnRcvUnusable(receiveChildId, locationId, kReturnReceivedChildId);
            }
            else if (returnFrom == "YD")
            {
                record = await _service.GetRackBinForYDReturnRcvUnusable(receiveChildId, locationId, kReturnReceivedChildId);
            }
            else if (returnFrom == "SC")
            {
                record = await _service.GetRackBinForSCReturnRcvUnusable(receiveChildId, locationId, kReturnReceivedChildId);
            }
            else if (returnFrom == "RND")
            {
                record = await _service.GetRackBinForRNDReturnRcvUnusable(receiveChildId, locationId, kReturnReceivedChildId);
            }
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
        public async Task<IActionResult> Save(YarnReceiveMaster model)
        {
            YarnReceiveMaster entity = await _service.GetAllAsync(model.ReceiveID);
            entity.YarnReceiveChilds.ForEach(c =>
            {
                c.EntityState = EntityState.Unchanged;
                c.YarnReceiveChildRackBins.SetUnchanged();
            });

            if (model.YarnReceiveChilds.Sum(x => x.YarnReceiveChildRackBins.Where(y => y.EntityState != EntityState.Deleted).Count()) == 0)
            {
                throw new Exception("No Rack/Bin Allocation Found!!");
            }

            entity.ReceiveDate = model.ReceiveDate;
            entity.LCDate = model.LCDate;
            entity.PODate = model.PODate;
            entity.ChallanDate = model.ChallanDate;
            entity.EntityState = EntityState.Modified;

            entity.YarnReceiveChilds.SetUnchanged();

            YarnReceiveChild yarnReceiveChildEntity = new YarnReceiveChild();
            YarnReceiveChildRackBin yarnReceiveChildRackBinEntity = new YarnReceiveChildRackBin();

            foreach (YarnReceiveChild child in model.YarnReceiveChilds)
            {
                int indexChild = entity.YarnReceiveChilds.FindIndex(x => x.ChildID == child.ChildID);
                yarnReceiveChildEntity = entity.YarnReceiveChilds.FirstOrDefault(x => x.ChildID == child.ChildID);
                yarnReceiveChildEntity.EntityState = EntityState.Modified;

                if (indexChild > -1)
                {
                    entity.YarnReceiveChilds[indexChild] = yarnReceiveChildEntity;
                }
                foreach (YarnReceiveChildRackBin item in child.YarnReceiveChildRackBins)
                {
                    int indexRack = yarnReceiveChildEntity.YarnReceiveChildRackBins.FindIndex(x => x.ChildRackBinID == item.ChildRackBinID);

                    yarnReceiveChildRackBinEntity = yarnReceiveChildEntity.YarnReceiveChildRackBins.FirstOrDefault(x => x.ChildRackBinID == item.ChildRackBinID);

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
                            yarnReceiveChildRackBinEntity.AddedBy = AppUser.UserCode;
                            yarnReceiveChildRackBinEntity.EntityState = EntityState.Added;
                            yarnReceiveChildEntity.YarnReceiveChildRackBins.Add(yarnReceiveChildRackBinEntity);
                        }
                        else
                        {
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
                            entity.YarnReceiveChilds[indexChild].YarnReceiveChildRackBins[indexRack] = yarnReceiveChildRackBinEntity;
                        }
                    }
                }
            }

            entity.CompleteAllocation = false;
            entity.PartialAllocation = false;

            for (int i = 0; i < entity.YarnReceiveChilds.Count(); i++)
            {
                var rackBins = entity.YarnReceiveChilds[i].YarnReceiveChildRackBins;

                var totalRackCartoon = rackBins.Sum(x => x.NoOfCartoon);
                var totalRackCone = rackBins.Sum(x => x.NoOfCone);
                var totalRackReceiveQty = rackBins.Sum(x => x.ReceiveQty);

                if (totalRackCartoon < entity.YarnReceiveChilds[i].NoOfCartoon)
                {
                    entity.PartialAllocation = true;
                    break;
                }
                else if (totalRackCone < entity.YarnReceiveChilds[i].NoOfCone)
                {
                    entity.PartialAllocation = true;
                    break;
                }
                else if (totalRackCone < entity.YarnReceiveChilds[i].NoOfCone)
                {
                    entity.PartialAllocation = true;
                    break;
                }
            }
            if (!entity.PartialAllocation) entity.CompleteAllocation = true;

            await _service.SaveAsync(entity);

            return Ok();
        }
        /*
        [Route("save")]
        [HttpPost]
        [ValidateModel]
        public async Task<IHttpActionResult> Save(YarnReceiveMaster model)
        {
            YarnReceiveMaster entity = await _service.GetAllAsync(model.ReceiveID);

            foreach (YarnReceiveChild child in entity.YarnReceiveChilds)
            {
                foreach (YarnReceiveChildRackBin item in child.YarnReceiveChildRackBins)
                    item.EntityState = EntityState.Unchanged;
            }

            if (model.YarnReceiveChilds.Sum(x => x.YarnReceiveChildRackBins.Where(y => y.EntityState != EntityState.Deleted).Count()) == 0)
            {
                throw new Exception("No Rack/Bin Allocation Found!!");
            }

            //entity.CompleteAllocation = true;
            //entity.PartialAllocation = false;
            entity.ReceiveDate = model.ReceiveDate;
            entity.LCDate = model.LCDate;
            entity.PODate = model.PODate;
            entity.ChallanDate = model.ChallanDate;
            entity.EntityState = EntityState.Modified;

            entity.YarnReceiveChilds.SetUnchanged();

            YarnReceiveChild yarnReceiveChildEntity = new YarnReceiveChild();
            YarnReceiveChildRackBin yarnReceiveChildRackBinEntity = new YarnReceiveChildRackBin();

            foreach (YarnReceiveChild child in model.YarnReceiveChilds)
            {
                int indexChild = entity.YarnReceiveChilds.FindIndex(x => x.ChildID == child.ChildID);

                yarnReceiveChildEntity = entity.YarnReceiveChilds.FirstOrDefault(x => x.ChildID == child.ChildID);

                yarnReceiveChildEntity.EntityState = EntityState.Modified;
                yarnReceiveChildEntity.PhysicalCount = child.PhysicalCount;
                yarnReceiveChildEntity.LotNo = child.LotNo;
                yarnReceiveChildEntity.ChallanLot = child.ChallanLot;
                yarnReceiveChildEntity.NoOfCartoon = child.NoOfCartoon;
                yarnReceiveChildEntity.NoOfCone = child.NoOfCone;
                yarnReceiveChildEntity.ReceiveQty = child.ReceiveQty;

                if (indexChild > -1)
                {
                    entity.YarnReceiveChilds[indexChild] = yarnReceiveChildEntity;
                }

                //if (child.YarnReceiveChildRackBins.Where(x => x.EntityState != EntityState.Deleted).Count() == 0)
                //{
                //    entity.CompleteAllocation = false;
                //    entity.PartialAllocation = true;
                //}
                foreach (YarnReceiveChildRackBin item in child.YarnReceiveChildRackBins)
                {
                    int indexRack = yarnReceiveChildEntity.YarnReceiveChildRackBins.FindIndex(x => x.ChildRackBinID == item.ChildRackBinID);

                    yarnReceiveChildRackBinEntity = yarnReceiveChildEntity.YarnReceiveChildRackBins.FirstOrDefault(x => x.ChildRackBinID == item.ChildRackBinID);

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
                            yarnReceiveChildRackBinEntity.AddedBy = UserId;
                            yarnReceiveChildRackBinEntity.EntityState = EntityState.Added;
                            yarnReceiveChildEntity.YarnReceiveChildRackBins.Add(yarnReceiveChildRackBinEntity);
                        }
                        else
                        {
                            yarnReceiveChildRackBinEntity.LocationID = item.LocationID;
                            yarnReceiveChildRackBinEntity.RackID = item.RackID;
                            yarnReceiveChildRackBinEntity.BinID = item.BinID;
                            yarnReceiveChildRackBinEntity.NoOfCartoon = item.NoOfCartoon;
                            yarnReceiveChildRackBinEntity.NoOfCone = item.NoOfCone;
                            yarnReceiveChildRackBinEntity.ReceiveQty = item.ReceiveQty;
                            yarnReceiveChildRackBinEntity.Remarks = item.Remarks;
                            yarnReceiveChildRackBinEntity.EmployeeID = item.EmployeeID;
                            yarnReceiveChildRackBinEntity.EntityState = EntityState.Modified;
                            yarnReceiveChildRackBinEntity.UpdatedBy = UserId;
                            yarnReceiveChildRackBinEntity.DateUpdated = DateTime.Now;
                        }

                        if (isUpdateList && indexChild > -1 && indexRack > -1)
                        {
                            entity.YarnReceiveChilds[indexChild].YarnReceiveChildRackBins[indexRack] = yarnReceiveChildRackBinEntity;
                        }
                    }
                }
            }

            await _service.SaveAsync(entity);

            return Ok();
        }
        */
    }
}