using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Web.Extends.Helpers;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.Application.Interfaces.SCD;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.Entity;

namespace EPYSLTEXCore.API.Contollers.SCD
{

    [Route("api/ybblcproposal")]
    public class YarnBBLCProposalController : ApiBaseController
    {
        private readonly IYarnBBLCProposalService _service;
        private readonly ICommonHelpers _commonHelpers;

        public YarnBBLCProposalController(IYarnBBLCProposalService service
            , ICommonHelpers commonHelpers
            , IUserService userService) : base(userService)
        {
            _service = service;
            _commonHelpers = commonHelpers;
        }
        [Route("listdata")]
        [HttpGet]
        public async Task<IActionResult> Getlistdata(Status status, bool isCDAPage)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<YarnBBLCProposalMaster> records = await _service.GetListAsync(status, isCDAPage, paginationInfo);
            var totalCount = records.FirstOrDefault() == null ? 0 : records.FirstOrDefault().TotalRows;
            var response = new TableResponseModel(records, paginationInfo.GridType);

            return Ok(response);

        }
        [Route("list")]
        [HttpGet]
        public async Task<IActionResult> Get(Status status, bool isCDAPage)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<YarnBBLCProposalMaster> records = await _service.GetListAsync(status, isCDAPage, paginationInfo);
            var totalCount = records.FirstOrDefault() == null ? 0 : records.FirstOrDefault().TotalRows;
            var response = new TableResponseModel(totalCount, records);

            return Ok(response);
        }

        [HttpGet]
        [Route("proposallist")]
        public async Task<IActionResult> GetList(Status status, bool isCDAPage)
        {
            var paginationInfo = Request.GetPaginationInfo();
            var records = await _service.GetListAsync(status, isCDAPage, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [Route("new")]
        [HttpGet]
        public async Task<IActionResult> GetNew(string piReceiveMasterIds)
        {
            //return Ok(await _service.GetNewAsync(piReceiveMasterIds));

            var piReceiveMasterIdArray = Array.ConvertAll(piReceiveMasterIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries), s => int.Parse(s));
            return Ok(await _service.GetNewAsync(piReceiveMasterIdArray));
        }

        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var record = await _service.GetAsync(id);
            Guard.Against.NullObject(id, record);

            return Ok(record);
        }
        [Route("LCType/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetNature(int id)
        {
            var record = await _service.GetNatureAsync(id);
            Guard.Against.NullObject(id, record);

            return Ok(record);
        }
        [Route("ProposeContractForPC/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetProposeContractForPC(int id)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<YarnBBLCProposalMaster> records = await _service.GetProposeContractForPCAsync(id, paginationInfo);
            var response = new TableResponseModel(records, paginationInfo.GridType);

            return Ok(response);
        }
        [Route("LCContractNo/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetLCContractNo(int id)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<YarnBBLCProposalMaster> records = await _service.GetLCContractNoAsync(id, paginationInfo);
            //Guard.Against.NullObject(id, record);

            var response = new TableResponseModel(records, paginationInfo.GridType);

            return Ok(response);
        }
        [Route("list-for-merge/{companyId}/{supplierId}/{isCDAPage}")]
        [HttpGet]
        public async Task<IActionResult> GetProposalForMerge(int companyId, int supplierId, bool isCDAPage)
        {
            return Ok(await _service.GetBBLCProposalsForMergeAsync(companyId, supplierId, isCDAPage));
        }

        [Route("load-merge-data")]
        [HttpGet]
        public async Task<IActionResult> Get(int id, string piReceiveMasterIds)
        {
            //return Ok(await _service.GetMergedDataAsync(id, piReceiveMasterIds));

            var piReceiveMasterIdArray = Array.ConvertAll(piReceiveMasterIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries), s => int.Parse(s));
            return Ok(await _service.GetMergedDataAsync(id, piReceiveMasterIdArray));
        }

        [Route("save")]
        [HttpPost]
        //[ValidateModel]
        public async Task<IActionResult> SaveYarnBBLCProposal(dynamic JsonString)
        {
            YarnBBLCProposalMaster model = JsonConvert.DeserializeObject<YarnBBLCProposalMaster>(Convert.ToString(JsonString));
            YarnBBLCProposalMaster entity;
            if (model.ProposalID > 0)
            {
                entity = await _service.GetAllByIDAsync(model.ProposalID);

                entity.LCID = model.LCID;
                entity.ProposalDate = model.ProposalDate;
                entity.Remarks = model.Remarks;
                entity.UpdatedBy = AppUser.UserCode;;
                entity.DateUpdated = DateTime.Now;
                entity.ProposeContractID = model.ProposeContractID;
                entity.CashStatus = model.CashStatus;
                entity.ProposeBankID = model.ProposeBankID;
                entity.ProposeBankName = model.ProposeBankName;
                entity.RetirementModeID = model.RetirementModeID;
                if (model.isMerge)
                {
                    entity.RevisionNo = (entity.RevisionNo + 1);
                    if (entity.YarnLcMasters.Count > 0)
                        entity.YarnLcMasters.FirstOrDefault().PreRevisionNo = entity.RevisionNo;
                }
                entity.isRevision = model.isRevision;
                if (model.isRevision)
                {
                    entity.RevisionNo = (entity.RevisionNo + 1);
                }
                entity.EntityState = EntityState.Modified;
                entity.YarnLcMasters.SetModified();
                entity.Childs.SetUnchanged();
                entity.YarnLCChilds.SetUnchanged();

                if (entity.Childs.IsNull()) entity.Childs = new List<YarnBBLCProposalChild>();

                int childID = 1;
                int maxRevisionNo = entity.YarnPIReceives.Max(x => x.RevisionNo);

                if (model.isRevision)
                {
                    entity.YarnPIReceives.ToList().ForEach(pir =>
                    {
                        pir.EntityState = EntityState.Modified;
                        if (pir.RevisionNo != maxRevisionNo)
                        {
                            pir.RevisionNo = maxRevisionNo;
                            pir.RevisionBy = AppUser.UserCode;;
                            pir.RevisionDate = DateTime.Now;
                        }
                    });
                }

                YarnBBLCProposalChild childEntity;
                foreach (YarnBBLCProposalChild item in model.Childs)
                {
                    childEntity = entity.Childs.FirstOrDefault(x => x.ChildID == item.ChildID);
                    if (childEntity == null || item.ChildID == 0)
                    {
                        if (item.ChildID == 0)
                        {
                            item.ChildID = childID++;
                        }
                        childEntity = CommonFunction.DeepClone(item);
                        if (model.isRevision)
                        {
                            childEntity.RevisionNo = maxRevisionNo;
                        }
                        entity.Childs.Add(CommonFunction.DeepClone(childEntity));
                    }
                    else
                    {
                        if (model.isRevision)
                        {
                            childEntity.RevisionNo = maxRevisionNo;
                        }
                        childEntity.EntityState = EntityState.Modified;
                    }
                }
                entity.Childs.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();

                //YarnLCChild table modify which use in Import LC interface
                YarnLcChild childYarnLC;
                foreach (YarnLcChild itemYarnLC in model.YarnLCChilds)
                {
                    childYarnLC = entity.YarnLCChilds.FirstOrDefault(x => x.LCID == itemYarnLC.LCID && x.YPIReceiveMasterID == itemYarnLC.YPIReceiveMasterID);

                    if (childYarnLC == null)
                    {
                        childYarnLC = itemYarnLC;
                        entity.YarnLCChilds.Add(childYarnLC);
                    }
                    else
                    {
                        childYarnLC.EntityState = EntityState.Modified;
                    }
                }
                entity.YarnLCChilds.Where(x => x.EntityState == EntityState.Unchanged).SetDeleted();
            }
            else
            {
                entity = model;
                entity.YPINo = string.Join(",", model.Childs.Select(x => x.YPINo));
                entity.AddedBy = AppUser.UserCode;;
            }

            entity.YarnLcMasters.ForEach(x =>
            {
                x.IsContract = entity.IsContract;
            });

            await _service.SaveAsync(entity);

            return Ok();
        }
    }
}
