using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.Application.Interfaces.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Inventory.Yarn;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Mvc;
using System.Data.Entity;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEX.Core.Statics;
using EPYSLTEX.Web.Extends.Helpers;
using EPYSLTEXCore.API.Extends.Filters;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Yarn;
using EPYSLTEXCore.Infrastructure.Entities.Tex;
using EPYSLTEXCore.Infrastructure.Data;
using Newtonsoft.Json;

namespace EPYSLTEXCore.API.Contollers.Inventory.Yarn
{
    [Route("api/yarn-mrir")]
    public class YarnMRIRController : ApiBaseController
    {
        private readonly IYarnMRIRService _yarnMRIRService;
        private readonly IYarnAllocationService _yarnAllocationService;
        private readonly ICommonHelpers _commonHelpers;
        private readonly IDapperCRUDService<YarnMRIRMaster> _service;
        public YarnMRIRController(IUserService userService, IYarnMRIRService YarnMRIRService
           , ICommonHelpers commonHelpers, IDapperCRUDService<YarnMRIRMaster> service
            , IYarnAllocationService yarnAllocationService
            ) : base(userService)
        {
            _yarnMRIRService = YarnMRIRService;
            _commonHelpers = commonHelpers;
            _service = service;
            _yarnAllocationService = yarnAllocationService;
        }
        [Route("list")]
        public async Task<IActionResult> GetList(Status status)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<YarnMRIRChild> records = await _yarnMRIRService.GetPagedAsync(status, paginationInfo, AppUser);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }
        [HttpGet]
        [Route("GetMRIRDetails/{MRIRMasterID}")]
        public async Task<IActionResult> GetDeliveryDetailsAsync(int MRIRMasterID)
        {
            YarnMRIRMaster data = await _yarnMRIRService.GetMRIRDetailsAsync(MRIRMasterID);
            return Ok(data);
        }
        [HttpGet]
        [Route("new/{qcRemarksMasterId}")]
        public async Task<IActionResult> GetNew(int qcRemarksMasterId)
        {
            return Ok(await _yarnMRIRService.GetNewAsync(qcRemarksMasterId));
        }

        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var record = await _yarnMRIRService.GetAsync(id);
            Guard.Against.NullObject(id, record);

            return Ok(record);
        }

        //[Route("save")]
        //[HttpPost]
        //[ValidateModel]
        //public async Task<IActionResult> SaveYarnMRIR(YarnMRIRMaster model)
        //{
        //    YarnMRIRMaster entity;


        //        entity = model;
        //        entity.AddedBy = UserId;
        //        entity.MRIRBy = UserId;

        //    await _yarnMRIRService.SaveAsync(entity);

        //    return Ok();
        //}
        private async Task<string> GetMaxMRIRNoAsync()
        {
            var id = await _service.GetMaxIdAsync(TableNames.MRIR_No, RepeatAfterEnum.EveryYear);
            var datePart = DateTime.Now.ToString("yyMMdd");
            return $@"MRIR{datePart}{id:00000}";
        }
        private async Task<string> GetMaxGRNNoAsync()
        {
            var id = await _service.GetMaxIdAsync(TableNames.GRN_No, RepeatAfterEnum.EveryYear);
            var datePart = DateTime.Now.ToString("yyMMdd");
            return $@"GRN{datePart}{id:00000}";
        }
        private async Task<string> GetMaxMRNNoAsync()
        {
            var id = await _service.GetMaxIdAsync(TableNames.MRN_No, RepeatAfterEnum.EveryYear);
            var datePart = DateTime.Now.ToString("yyMMdd");
            return $@"MRN{datePart}{id:00000}";
        }
        [Route("save")]
        [HttpPost]
        [ValidateModel]
        //public async Task<IActionResult> SaveYarnMRIR(List<YarnMRIRChild> modelList)
        public async Task<IActionResult> SaveYarnMRIR(dynamic jsonString)
        {
            //string pageName = modelDynamic.PageName;
            //string grpConceptNo = model.GroupConceptNo;
            //string statusText = model.ConceptStatus;
            List<YarnMRIRChild> modelList = JsonConvert.DeserializeObject<List<YarnMRIRChild>>(Convert.ToString(jsonString));
            string ret = "";
            YarnMRIRMaster entity = null;
            entity = new YarnMRIRMaster();

            if ((ReceiveNoteType)modelList[0].ReceiveNoteType == ReceiveNoteType.MRIR)
            {
                entity.MRIRNo = await GetMaxMRIRNoAsync();
                entity.MRIRBy = AppUser.UserCode;
                entity.MRIRDate = DateTime.Now;
                ret = entity.MRIRNo;

                modelList = await this.GenerateYarnAllocation(modelList);
            }
            if ((ReceiveNoteType)modelList[0].ReceiveNoteType == ReceiveNoteType.GRN)
            {
                entity.GRNNo = await GetMaxGRNNoAsync();
                entity.GRNBy = AppUser.EmployeeCode;
                entity.GRNDate = DateTime.Now;
                ret = entity.GRNNo;
            }
            if ((ReceiveNoteType)modelList[0].ReceiveNoteType == ReceiveNoteType.MRN)
            {
                entity.MRNNo = await GetMaxMRNNoAsync();
                entity.MRNBy = AppUser.EmployeeCode;
                entity.MRNDate = DateTime.Now;
                ret = entity.MRNNo;
            }

            entity.ReceiveNo = modelList[0].ReceiveNo;
            entity.ChallanNo = modelList[0].ChallanNo;
            entity.CompanyId = modelList[0].CompanyID;
            entity.RCompanyId = modelList[0].RCompanyID;
            entity.SupplierId = modelList[0].SupplierID;
            entity.SpinnerId = modelList[0].SpinnerID;
            entity.ReceiveNoteType = modelList[0].ReceiveNoteType;

            entity.AddedBy = AppUser.EmployeeCode;
            entity.DateAdded = DateTime.Now;
            entity.EntityState = EntityState.Added;

            foreach (YarnMRIRChild Child in modelList)
            {
                Child.EntityState = EntityState.Added;
                entity.YarnMRIRChilds.Add(Child);
            }
            await _yarnMRIRService.SaveAsync(entity);
            return Ok(ret);
        }
        [Route("SaveGRNMRIR")]
        [HttpPost]
        [ValidateModel]
        //public async Task<IActionResult> SaveGRNMRIR(YarnMRIRMaster model)
        public async Task<IActionResult> SaveGRNMRIR(dynamic jsonString)
        {
            YarnMRIRMaster model = JsonConvert.DeserializeObject<YarnMRIRMaster>(Convert.ToString(jsonString));
            string ret = "";
            YarnMRIRMaster entity = await _yarnMRIRService.GetAsync(model.MRIRMasterId); ;

            entity.MRIRNo = await GetMaxMRIRNoAsync();
            ret = entity.MRIRNo;
            entity.MRIRBy = AppUser.EmployeeCode;
            entity.MRIRDate = DateTime.Now;
            entity.ReceiveNoteType = (int)ReceiveNoteType.MRIR;
            entity.UpdatedBy = AppUser.EmployeeCode;
            entity.DateUpdated = DateTime.Now;

            entity.EntityState = EntityState.Modified;

            await _yarnMRIRService.SaveAsync(entity);
            return Ok(ret);
        }
        [Route("retest")]
        [HttpPost]
        [ValidateModel]
        //public async Task<IActionResult> Retest(YarnMRIRMaster model)
        public async Task<IActionResult> Retest(dynamic jsonString)
        {
            YarnMRIRMaster model = JsonConvert.DeserializeObject<YarnMRIRMaster>(Convert.ToString(jsonString));
            string ret = "";
            YarnMRIRMaster entity = await _yarnMRIRService.GetAsync(model.MRIRMasterId); ;

            ret = entity.MRNNo;
            entity.ReTest = true;
            entity.ReTestBy = AppUser.EmployeeCode;
            entity.ReTestDate = DateTime.Now;
            entity.ReTestReason = model.ReTestReason;

            entity.EntityState = EntityState.Modified;

            await _yarnMRIRService.SaveAsync(entity);
            return Ok(ret);
        }
        [Route("return")]
        [HttpPost]
        [ValidateModel]
        //public async Task<IActionResult> Return(YarnMRIRMaster model)
        public async Task<IActionResult> Return(dynamic jsonString)
        {
            YarnMRIRMaster model = JsonConvert.DeserializeObject<YarnMRIRMaster>(Convert.ToString(jsonString));
            string ret = "";
            YarnMRIRMaster entity = await _yarnMRIRService.GetAsync(model.MRIRMasterId); ;

            ret = entity.MRNNo;
            entity.Returned = true;
            entity.ReturnedBy = AppUser.EmployeeCode;
            entity.ReturnedDate = DateTime.Now;

            entity.EntityState = EntityState.Modified;

            await _yarnMRIRService.SaveAsync(entity);
            return Ok(ret);
        }

        private async Task<List<YarnMRIRChild>> GenerateYarnAllocation(List<YarnMRIRChild> mrChilds)
        {
            DateTime currentDateTime = DateTime.Now;

            string allcationChildIds = string.Join(",", mrChilds.Where(x => x.AllocationChildID > 0).Select(x => x.AllocationChildID).Distinct());
            string receiveChildIDs = string.Join(",", mrChilds.Where(x => x.AllocationChildID > 0).Select(x => x.ReceiveChildID).Distinct());

            if (allcationChildIds.IsNotNullOrEmpty() && receiveChildIDs.IsNotNullOrEmpty())
            {
                List<YarnReceiveChild> yarnReceiveChilds = await _yarnMRIRService.GetByYarnReceiveChildByChildIds(receiveChildIDs);
                List<YarnAllocationMaster> YarnAllocations = await _yarnAllocationService.GetByAllocationChildIds(allcationChildIds);

                foreach (var mrc in mrChilds.Where(x => x.AllocationChildID > 0).ToList())
                {
                    YarnReceiveChild yarnReceiveChild = CommonFunction.DeepClone(yarnReceiveChilds.Find(x => x.ChildID == mrc.ReceiveChildID));

                    YarnAllocationMaster yarnAllocation = new YarnAllocationMaster();
                    for (int i = 0; i < YarnAllocations.Count(); i++)
                    {
                        var child = YarnAllocations[i].Childs.Find(x => x.AllocationChildID == mrc.AllocationChildID);
                        if (child.IsNotNull())
                        {
                            yarnAllocation = YarnAllocations[i];
                            break;
                        }
                    }

                    int operationalUserId = yarnAllocation.ApproveBy;
                    //GetMaxYarnAllocationNoAsync

                    #region Allocation Master
                    mrc.YarnAllocation = new YarnAllocationMaster();
                    mrc.YarnAllocation = CommonFunction.DeepClone(yarnAllocation);
                    mrc.YarnAllocation.Childs = new List<YarnAllocationChild>();

                    mrc.YarnAllocation.MRIRChildID = mrc.MRIRChildID; //mrc.MRIRChildID = 0 cz first time saving
                    mrc.YarnAllocation.YarnAllocationNo = "";
                    mrc.YarnAllocation.YarnAllocationDate = currentDateTime;
                    mrc.YarnAllocation.Approve = true;
                    mrc.YarnAllocation.ApproveBy = operationalUserId;
                    mrc.YarnAllocation.ApproveDate = currentDateTime;
                    mrc.YarnAllocation.Acknowledge = true;
                    mrc.YarnAllocation.AcknowledgeBy = operationalUserId;
                    mrc.YarnAllocation.AcknowledgeDate = currentDateTime;
                    mrc.YarnAllocation.AddedBy = operationalUserId;

                    mrc.YarnAllocation.DateAdded = currentDateTime;
                    mrc.YarnAllocation.RevisionNo = 0;
                    mrc.YarnAllocation.IsAutoGenerate = true;
                    #endregion

                    #region Allocation Child
                    YarnAllocationChild yarnAllocationChild = new YarnAllocationChild();
                    yarnAllocationChild = CommonFunction.DeepClone(yarnAllocation.Childs.Find(x => x.AllocationChildID == mrc.AllocationChildID));
                    yarnAllocationChild.ReqQty = yarnAllocationChild.QtyForPO;
                    yarnAllocationChild.AdvanceStockAllocationQty = 0;
                    yarnAllocationChild.PipelineStockAllocationQty = 0;
                    yarnAllocationChild.QtyFromAutoGenerate = mrc.ReceiveQty;
                    yarnAllocationChild.TotalAllocationQty = yarnAllocationChild.QtyForPO;
                    yarnAllocationChild.PreRevisionNo = 0;
                    yarnAllocationChild.RevisionNo = 0;
                    yarnAllocationChild.RevisionBy = 0;
                    yarnAllocationChild.RevisionDate = null;
                    #endregion

                    #region Allocation Child Item
                    YarnAllocationChildItem yarnAllocationChildItem = new YarnAllocationChildItem();
                    yarnAllocationChildItem.YarnCategory = yarnReceiveChild.YarnCategory;
                    yarnAllocationChildItem.YarnStockSetId = yarnReceiveChild.YarnStockSetId;
                    yarnAllocationChildItem.QuarantineAllocationQty = yarnAllocationChild.QtyForPO;
                    yarnAllocationChildItem.TotalAllocationQty = yarnAllocationChild.QtyForPO;
                    yarnAllocationChildItem.Acknowledge = true;
                    yarnAllocationChildItem.AcknowledgeBy = operationalUserId;
                    yarnAllocationChildItem.AcknowledgeDate = currentDateTime;
                    yarnAllocationChild.ChildItems.Add(yarnAllocationChildItem);
                    #endregion

                    mrc.YarnAllocation.Childs.Add(yarnAllocationChild);
                }
            }
            return mrChilds;
        }

    }
}
