using EPYSLTEX.Core.Interfaces;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEXCore.API.Contollers.APIBaseController;
using EPYSLTEXCore.API.Extends.Filters;
using EPYSLTEXCore.Infrastructure.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Tex.HouseKeeping;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.Entity;

namespace EPYSLTEX.Web.Controllers.Apis
{
    [Authorize]
    [Route("api/lab-test-requisition")]
    public class LabTestRequisitionController : ApiBaseController
    {
        private readonly ILabTestRequisitionService _LabTestRequisitionService;
        private readonly ISelect2Service _select2Service;
        private readonly ICommonHelperService _commonService;
        public LabTestRequisitionController(ILabTestRequisitionService LabTestRequisitionService, IUserService userService
        , ISelect2Service select2Service
             , ICommonHelperService commonService) : base(userService)
        {
            _LabTestRequisitionService = LabTestRequisitionService;
            _select2Service = select2Service;
            _commonService = commonService;
        }

        [AllowAnonymous]
        [Route("list")]
        [HttpGet]
        public async Task<IActionResult> GetList(int isBDS, Status status)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<LabTestRequisitionMaster> records = await _LabTestRequisitionService.GetPagedAsync(isBDS, status, paginationInfo);
            return Ok(new TableResponseModel(records, paginationInfo.GridType));
        }

        [HttpGet]
        [Route("new/{newId}/{conceptId}/{subGroupId}/{buyerId}")]
        public async Task<IActionResult> GetNew(int newId, int conceptId, int subGroupId, int buyerId)
        {
            LabTestRequisitionMaster data = await _LabTestRequisitionService.GetNewAsync(newId, conceptId, subGroupId, buyerId);
            data.ContactPersonID = AppUser.EmployeeCode;
            data.ContactPersonName = AppUser.EmployeeName;
            return Ok(data);
        }

        [Route("{id}/{isretestflag}/{buyerId}")]
        [HttpGet]
        public async Task<IActionResult> Get(int id, bool isretestflag, int buyerId)
        {
            LabTestRequisitionMaster record = await _LabTestRequisitionService.GetAsync(id, isretestflag, buyerId);
            Guard.Against.NullObject(id, record);
            foreach (LabTestRequisitionBuyer child in record.LabTestRequisitionBuyers)
            {
                child.LabTestRequisitionBuyerParameters = record.LabTestRequisitionBuyers[0].LabTestRequisitionBuyerParameters.Where(x => x.LTReqBuyerID == child.LTReqBuyerID).ToList();
            }
            return Ok(record);
        }

        [HttpGet]
        [Route("buyer-parameter/{ids}")]
        public async Task<IActionResult> GetBuyerParameterByBuyerId(string ids)
        {
            List<LabTestRequisitionBuyer> labTestreqBuyers = new List<LabTestRequisitionBuyer>();
            var data = await _select2Service.GetContactNamesByCintactIdsAsync(ContactCategoryConstants.CONTACT_CATEGORY_BUYER, ids);
            foreach (var child in data)
            {
                LabTestRequisitionBuyer labTestReqBuyer = new LabTestRequisitionBuyer
                {
                    BuyerID = Convert.ToInt32(child.id),
                    BuyerName = child.text
                };
                labTestreqBuyers.Add(labTestReqBuyer);
            }
            foreach (LabTestRequisitionBuyer child in labTestreqBuyers)
            {
                child.LabTestRequisitionBuyerParameters = await _LabTestRequisitionService.GetBuyerParameterByBuyerId(child.BuyerID);
            }
            return Ok(labTestreqBuyers);
        }

        [HttpGet]
        [Route("get-LabTestRequisition-BuyerParameters/{buyerID}/{testNatureID}/{isProduction}")]
        public async Task<IActionResult> GetLabTestRequisitionBuyerParameters(int buyerID, int testNatureID, int isProduction)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<LabTestRequisitionBuyerParameter> list = new List<LabTestRequisitionBuyerParameter>();
            list = await _LabTestRequisitionService.GetLabTestRequisitionBuyerParameters(paginationInfo, buyerID, testNatureID, isProduction);
            return Ok(new TableResponseModel(list, paginationInfo.GridType));
        }

        [HttpGet]
        [Route("get-LabTestRequisition-BuyerParametersSet/{buyerID}/{testNatureID}/{isProduction}")]
        public async Task<IActionResult> GetLabTestRequisitionBuyerParametersForSet(int buyerID, int testNatureID, int isProduction)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<LabTestRequisitionBuyerParameter> list = new List<LabTestRequisitionBuyerParameter>();
            list = await _LabTestRequisitionService.GetLabTestRequisitionBuyerParameters(paginationInfo, buyerID, testNatureID, isProduction);
            return Ok(list);
        }
        [HttpGet]
        [Route("carelables")]
        public async Task<IActionResult> GetLaundryCareLables()
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<LaundryCareLable_HK> careLables = new List<LaundryCareLable_HK>();
            careLables = await _LabTestRequisitionService.GetAllLaundryCareLablesAsync(paginationInfo);
            return Ok(new TableResponseModel(careLables, paginationInfo.GridType));
        }
        [HttpGet]
        [Route("careLableCodes/{buyerId}")]
        public async Task<IActionResult> GetLaundryCareLableCodes(int buyerId)
        {
            var paginationInfo = Request.GetPaginationInfo();
            List<LaundryCareLableBuyerCode> list = new List<LaundryCareLableBuyerCode>();
            list = await _LabTestRequisitionService.GetLaundryCareLableCodes(buyerId, paginationInfo);
            int tempPK = 1;
            list.ForEach(x =>
            {
                x.LCLBuyerID = tempPK++;
            });
            return Ok(new TableResponseModel(list, paginationInfo.GridType));
        }
        [HttpGet]
        [Route("GetCareLebelsByCode/{careLabelCode}")]
        public async Task<IActionResult> GetCareLebelsByCode(string careLabelCode)
        {
            List<LaundryCareLableBuyerCode> list = new List<LaundryCareLableBuyerCode>();
            list = await _LabTestRequisitionService.GetCareLablesByCode(careLabelCode);
            return Ok(list);
        }
        [HttpGet]
        [Route("GetCareLebelsByCodes/multiple/{careLabelCodes}/{buyerID}")]
        public async Task<IActionResult> GetCareLebelsByCodes(string careLabelCodes, int buyerID)
        {
            List<LaundryCareLableBuyerCode> list = new List<LaundryCareLableBuyerCode>();
            list = await _LabTestRequisitionService.GetCareLebelsByCodes(careLabelCodes, buyerID);
            return Ok(list);
        }

        [Route("save")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Save(LabTestRequisitionMaster model)
        {
            LabTestRequisitionMaster entity;
            if (model.LTReqMasterID > 0)
            {
                #region For update

                #region Getting Existing / Save Data
                entity = await _LabTestRequisitionService.GetAllByIDAsync(model.LTReqMasterID);
                foreach (LabTestRequisitionBuyer reqBuyer in entity.LabTestRequisitionBuyers)
                {
                    reqBuyer.LabTestRequisitionBuyerParameters = entity.LabTestRequisitionBuyerParameters.Where(x => x.LTReqBuyerID == reqBuyer.LTReqBuyerID).ToList();
                }
                #endregion

                #region Keep Status unchange
                entity.LabTestRequisitionBuyers.SetUnchanged();
                entity.LabTestRequisitionBuyers.ForEach(x => { x.LabTestRequisitionBuyerParameters.SetUnchanged(); });
                entity.CareLabels.SetUnchanged();
                entity.Countries.SetUnchanged();
                entity.EndUses.SetUnchanged();
                entity.FinishDyeMethods.SetUnchanged();
                #endregion

                #region Add Buyer Parameter Buyer Wise from Model
                int nLTReqBuyerID = 0;

                foreach (LabTestRequisitionBuyer item in model.LabTestRequisitionBuyers)
                {
                    if (nLTReqBuyerID == 0)
                    {
                        nLTReqBuyerID = item.LTReqBuyerID;
                    }

                    List<LabTestRequisitionBuyerParameter> buyerParams = new List<LabTestRequisitionBuyerParameter>();
                    foreach (LabTestRequisitionBuyerParameter child in item.LabTestRequisitionBuyerParameters.Where(x => x.SubIDs != null).ToList())
                    {
                        buyerParams.AddRange(child.SubIDs.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(x => new LabTestRequisitionBuyerParameter
                                            {
                                                LTReqBPID = child.LTReqBPID,
                                                LTReqMasterID = child.LTReqMasterID,
                                                BPSubID = Convert.ToInt32(x),
                                                BPID = child.BPID,
                                                LTReqBuyerID = child.LTReqBuyerID,
                                                BuyerID = child.BuyerID,
                                                RefValueFrom = child.RefValueFrom,
                                                RefValueTo = child.RefValueTo,
                                                TestValue = child.TestValue,
                                                TestValue1 = child.TestValue1,
                                                Requirement = child.Requirement,
                                                Requirement1 = child.Requirement1,
                                                IsPass = child.IsPass,
                                                TestNatureID = child.TestNatureID,
                                                Remarks = child.Remarks,
                                            }));
                    }
                    item.LabTestRequisitionBuyerParameters = buyerParams;
                }
                #endregion

                entity.ReqDate = model.ReqDate;
                entity.FabricQty = model.FabricQty;
                entity.UnitID = model.UnitID;
                entity.KnittingUnitID = model.KnittingUnitID;
                entity.ContactPersonID = model.ContactPersonID;
                entity.CareInstruction = model.CareInstruction;
                entity.UpdatedBy = AppUser.UserCode;
                entity.DateUpdated = DateTime.Now;
                entity.EntityState = EntityState.Modified;
                entity.LabTestServiceTypeID = model.LabTestServiceTypeID;
                entity.IsRetest = model.IsRetest;

                #region Add Newly added Buyer & buyer Parameter
                LabTestRequisitionBuyer labTestRequisitionBuyer;
                LabTestRequisitionBuyerParameter labTestRequisitionBuyerParameter;
                foreach (LabTestRequisitionBuyer child in model.LabTestRequisitionBuyers)
                {
                    labTestRequisitionBuyer = entity.LabTestRequisitionBuyers.FirstOrDefault(x => x.LTReqBuyerID == child.LTReqBuyerID);

                    if (labTestRequisitionBuyer == null)
                    {
                        labTestRequisitionBuyer = child;
                        labTestRequisitionBuyer.EntityState = EntityState.Added;
                        entity.LabTestRequisitionBuyers.Add(labTestRequisitionBuyer);
                    }
                    else
                    {
                        labTestRequisitionBuyer.EntityState = EntityState.Modified;
                        foreach (LabTestRequisitionBuyerParameter item in child.LabTestRequisitionBuyerParameters)
                        {
                            labTestRequisitionBuyerParameter = labTestRequisitionBuyer.LabTestRequisitionBuyerParameters.FirstOrDefault(x => x.BPID == item.BPID && x.BPSubID == item.BPSubID);
                            if (labTestRequisitionBuyerParameter == null)
                            {
                                labTestRequisitionBuyerParameter = item;
                                labTestRequisitionBuyerParameter.EntityState = EntityState.Added;
                                labTestRequisitionBuyer.LabTestRequisitionBuyerParameters.Add(labTestRequisitionBuyerParameter);
                            }
                            else
                            {
                                labTestRequisitionBuyerParameter.TestValue = item.TestValue;
                                labTestRequisitionBuyerParameter.TestValue1 = item.TestValue1;
                                labTestRequisitionBuyerParameter.Requirement = item.Requirement;
                                labTestRequisitionBuyerParameter.Requirement1 = item.Requirement1;
                                labTestRequisitionBuyerParameter.TestNatureID = item.TestNatureID;
                                labTestRequisitionBuyerParameter.Remarks = item.Remarks;
                                labTestRequisitionBuyerParameter.EntityState = EntityState.Modified;
                            }
                        }
                    }
                }
                #endregion

                #region Add Newly added Care Lable
                LabTestRequisitionCareLabel careLabel = new LabTestRequisitionCareLabel();
                foreach (LabTestRequisitionCareLabel child in model.CareLabels)
                {
                    careLabel = entity.CareLabels.FirstOrDefault(x => x.LCareLableID == child.LCareLableID);

                    if (careLabel == null)
                    {
                        careLabel = child;
                        careLabel.EntityState = EntityState.Added;
                        entity.CareLabels.Add(careLabel);
                    }
                    else
                    {
                        careLabel.SeqNo = child.SeqNo;
                        careLabel.EntityState = EntityState.Modified;
                    }
                }
                #endregion

                #region LabTestRequisitionExportCountry
                LabTestRequisitionExportCountry country = new LabTestRequisitionExportCountry();
                foreach (LabTestRequisitionExportCountry child in model.Countries)
                {
                    country = entity.Countries.FirstOrDefault(x => x.CountryRegionID == child.CountryRegionID);

                    if (country == null)
                    {
                        country = child;
                        country.LTReqBuyerID = nLTReqBuyerID;
                        country.EntityState = EntityState.Added;
                        entity.Countries.Add(country);
                    }
                    else
                    {
                        country.SeqNo = child.SeqNo;
                        country.EntityState = EntityState.Modified;
                    }
                }
                entity.Countries.Where(x => x.EntityState == EntityState.Unchanged).ToList().ForEach(x => x.EntityState = EntityState.Deleted);
                #endregion

                #region LabTestRequisitionEndUse
                LabTestRequisitionEndUse endUse = new LabTestRequisitionEndUse();
                foreach (LabTestRequisitionEndUse child in model.EndUses)
                {
                    endUse = entity.EndUses.FirstOrDefault(x => x.StyleGenderID == child.StyleGenderID);

                    if (endUse == null)
                    {
                        endUse = child;
                        endUse.LTReqBuyerID = nLTReqBuyerID;
                        endUse.EntityState = EntityState.Added;
                        entity.EndUses.Add(endUse);
                    }
                    else
                    {
                        endUse.SeqNo = child.SeqNo;
                        endUse.EntityState = EntityState.Modified;
                    }
                }
                entity.EndUses.Where(x => x.EntityState == EntityState.Unchanged).ToList().ForEach(x => x.EntityState = EntityState.Deleted);
                #endregion

                #region LabTestRequisitionSpecialFinishDyeMethod
                LabTestRequisitionSpecialFinishDyeMethod finishDyeMethod = new LabTestRequisitionSpecialFinishDyeMethod();
                foreach (LabTestRequisitionSpecialFinishDyeMethod child in model.FinishDyeMethods)
                {
                    finishDyeMethod = entity.FinishDyeMethods.FirstOrDefault(x => x.FinishDyeMethodID == child.FinishDyeMethodID);

                    if (finishDyeMethod == null)
                    {
                        finishDyeMethod = child;
                        finishDyeMethod.LTReqBuyerID = nLTReqBuyerID;
                        finishDyeMethod.EntityState = EntityState.Added;
                        entity.FinishDyeMethods.Add(finishDyeMethod);
                    }
                    else
                    {
                        finishDyeMethod.SeqNo = child.SeqNo;
                        finishDyeMethod.EntityState = EntityState.Modified;
                    }
                }
                entity.FinishDyeMethods.Where(x => x.EntityState == EntityState.Unchanged).ToList().ForEach(x => x.EntityState = EntityState.Deleted);
                #endregion

                #endregion
            }
            else
            {
                #region For New
                foreach (LabTestRequisitionBuyer item in model.LabTestRequisitionBuyers)
                {
                    List<LabTestRequisitionBuyerParameter> buyerParams = new List<LabTestRequisitionBuyerParameter>();
                    foreach (LabTestRequisitionBuyerParameter child in item.LabTestRequisitionBuyerParameters.Where(x => x.SubIDs != null).ToList())
                    {
                        buyerParams.AddRange(child.SubIDs.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(x => new LabTestRequisitionBuyerParameter
                                            {
                                                LTReqBPID = child.LTReqBPID,
                                                LTReqMasterID = child.LTReqMasterID,
                                                BPSubID = Convert.ToInt32(x),
                                                BPID = child.BPID,
                                                LTReqBuyerID = child.LTReqBuyerID,
                                                BuyerID = child.BuyerID,
                                                RefValueFrom = child.RefValueFrom,
                                                RefValueTo = child.RefValueTo,
                                                TestValue = child.TestValue,
                                                TestValue1 = child.TestValue1,
                                                Requirement = child.Requirement,
                                                Requirement1 = child.Requirement1,
                                                IsPass = child.IsPass,
                                                TestNatureID = child.TestNatureID,
                                                Remarks = child.Remarks,
                                            }));

                    }
                    item.LabTestRequisitionBuyerParameters = buyerParams;
                }

                entity = model;
                entity.AddedBy = AppUser.UserCode;

                #region According to Liton sir & Salam vai requirement when create Lab Test Req, then will auto approve

                entity.IsApproved = true;
                entity.ApprovedBy = AppUser.UserCode;
                entity.ApprovedDate = DateTime.Now;

                #endregion

                #endregion For New
            }

            var obj = await _LabTestRequisitionService.SaveAsync(entity);
            await _commonService.UpdateFreeConceptStatus(InterfaceFrom.LabTestRequisition, entity.ConceptID, "", entity.BookingID, 0, 0, entity.ColorID, entity.ItemMasterID);
            return Ok(obj);
        }

        [Route("revise")]
        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> Revise(LabTestRequisitionMaster model)
        {
            LabTestRequisitionMaster entity;
            #region For update

            #region Getting Existing / Save Data
            entity = await _LabTestRequisitionService.GetAllByIDAsync(model.LTReqMasterID);
            foreach (LabTestRequisitionBuyer reqBuyer in entity.LabTestRequisitionBuyers)
            {
                reqBuyer.LabTestRequisitionBuyerParameters = entity.LabTestRequisitionBuyerParameters.Where(x => x.LTReqBuyerID == reqBuyer.LTReqBuyerID).ToList();
            }
            #endregion

            #region Keep Status unchange
            entity.LabTestRequisitionBuyers.SetUnchanged();
            entity.LabTestRequisitionBuyers.ForEach(x => { x.LabTestRequisitionBuyerParameters.SetUnchanged(); });
            entity.CareLabels.SetUnchanged();
            entity.Countries.SetUnchanged();
            entity.EndUses.SetUnchanged();
            entity.FinishDyeMethods.SetUnchanged();
            #endregion

            #region Add Buyer Parameter Buyer Wise from Model
            int nLTReqBuyerID = 0;

            foreach (LabTestRequisitionBuyer item in model.LabTestRequisitionBuyers)
            {
                if (nLTReqBuyerID == 0)
                {
                    nLTReqBuyerID = item.LTReqBuyerID;
                }

                List<LabTestRequisitionBuyerParameter> buyerParams = new List<LabTestRequisitionBuyerParameter>();
                foreach (LabTestRequisitionBuyerParameter child in item.LabTestRequisitionBuyerParameters.Where(x => x.SubIDs != null).ToList())
                {
                    buyerParams.AddRange(child.SubIDs.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(x => new LabTestRequisitionBuyerParameter
                                        {
                                            LTReqBPID = child.LTReqBPID,
                                            LTReqMasterID = child.LTReqMasterID,
                                            BPSubID = Convert.ToInt32(x),
                                            BPID = child.BPID,
                                            LTReqBuyerID = child.LTReqBuyerID,
                                            BuyerID = child.BuyerID,
                                            RefValueFrom = child.RefValueFrom,
                                            RefValueTo = child.RefValueTo,
                                            TestValue = child.TestValue,
                                            TestValue1 = child.TestValue1,
                                            Requirement = child.Requirement,
                                            Requirement1 = child.Requirement1,
                                            IsPass = child.IsPass,
                                            TestNatureID = child.TestNatureID,
                                            Remarks = child.Remarks,
                                        }));
                }
                item.LabTestRequisitionBuyerParameters = buyerParams;
            }
            #endregion

            entity.ReqDate = model.ReqDate;
            entity.FabricQty = model.FabricQty;
            entity.UnitID = model.UnitID;
            entity.KnittingUnitID = model.KnittingUnitID;
            entity.ContactPersonID = model.ContactPersonID;
            entity.CareInstruction = model.CareInstruction;
            entity.UpdatedBy = AppUser.UserCode;
            entity.DateUpdated = DateTime.Now;
            entity.EntityState = EntityState.Modified;
            entity.LabTestServiceTypeID = model.LabTestServiceTypeID;
            entity.IsRetest = model.IsRetest;
            entity.RevisionNo = entity.RevisionNo + 1;
            entity.RevisionBy = AppUser.UserCode;
            entity.RevisionDate = DateTime.Now;
            entity.RevisionReason = model.RevisionReason;
            entity.UnAcknowledge = false;


            #region Add Newly added Buyer & buyer Parameter
            LabTestRequisitionBuyer labTestRequisitionBuyer;
            LabTestRequisitionBuyerParameter labTestRequisitionBuyerParameter;
            foreach (LabTestRequisitionBuyer child in model.LabTestRequisitionBuyers)
            {
                labTestRequisitionBuyer = entity.LabTestRequisitionBuyers.FirstOrDefault(x => x.LTReqBuyerID == child.LTReqBuyerID);

                if (labTestRequisitionBuyer == null)
                {
                    labTestRequisitionBuyer = child;
                    labTestRequisitionBuyer.EntityState = EntityState.Added;
                    entity.LabTestRequisitionBuyers.Add(labTestRequisitionBuyer);
                }
                else
                {
                    labTestRequisitionBuyer.EntityState = EntityState.Modified;
                    foreach (LabTestRequisitionBuyerParameter item in child.LabTestRequisitionBuyerParameters)
                    {
                        labTestRequisitionBuyerParameter = labTestRequisitionBuyer.LabTestRequisitionBuyerParameters.FirstOrDefault(x => x.BPID == item.BPID && x.BPSubID == item.BPSubID);
                        if (labTestRequisitionBuyerParameter == null)
                        {
                            labTestRequisitionBuyerParameter = item;
                            labTestRequisitionBuyerParameter.EntityState = EntityState.Added;
                            labTestRequisitionBuyer.LabTestRequisitionBuyerParameters.Add(labTestRequisitionBuyerParameter);
                        }
                        else
                        {
                            labTestRequisitionBuyerParameter.TestValue = item.TestValue;
                            labTestRequisitionBuyerParameter.TestValue1 = item.TestValue1;
                            labTestRequisitionBuyerParameter.Requirement = item.Requirement;
                            labTestRequisitionBuyerParameter.Requirement1 = item.Requirement1;
                            labTestRequisitionBuyerParameter.TestNatureID = item.TestNatureID;
                            labTestRequisitionBuyerParameter.Remarks = item.Remarks;
                            labTestRequisitionBuyerParameter.EntityState = EntityState.Modified;
                        }
                    }
                }
            }
            #endregion

            #region Add Newly added Care Lable
            LabTestRequisitionCareLabel careLabel = new LabTestRequisitionCareLabel();
            foreach (LabTestRequisitionCareLabel child in model.CareLabels)
            {
                careLabel = entity.CareLabels.FirstOrDefault(x => x.LCareLableID == child.LCareLableID);

                if (careLabel == null)
                {
                    careLabel = child;
                    careLabel.EntityState = EntityState.Added;
                    entity.CareLabels.Add(careLabel);
                }
                else
                {
                    careLabel.SeqNo = child.SeqNo;
                    careLabel.EntityState = EntityState.Modified;
                }
            }
            #endregion

            #region LabTestRequisitionExportCountry
            LabTestRequisitionExportCountry country = new LabTestRequisitionExportCountry();
            foreach (LabTestRequisitionExportCountry child in model.Countries)
            {
                country = entity.Countries.FirstOrDefault(x => x.CountryRegionID == child.CountryRegionID);

                if (country == null)
                {
                    country = child;
                    country.LTReqBuyerID = nLTReqBuyerID;
                    country.EntityState = EntityState.Added;
                    entity.Countries.Add(country);
                }
                else
                {
                    country.SeqNo = child.SeqNo;
                    country.EntityState = EntityState.Modified;
                }
            }
            entity.Countries.Where(x => x.EntityState == EntityState.Unchanged).ToList().ForEach(x => x.EntityState = EntityState.Deleted);
            #endregion

            #region LabTestRequisitionEndUse
            LabTestRequisitionEndUse endUse = new LabTestRequisitionEndUse();
            foreach (LabTestRequisitionEndUse child in model.EndUses)
            {
                endUse = entity.EndUses.FirstOrDefault(x => x.StyleGenderID == child.StyleGenderID);

                if (endUse == null)
                {
                    endUse = child;
                    endUse.LTReqBuyerID = nLTReqBuyerID;
                    endUse.EntityState = EntityState.Added;
                    entity.EndUses.Add(endUse);
                }
                else
                {
                    endUse.SeqNo = child.SeqNo;
                    endUse.EntityState = EntityState.Modified;
                }
            }
            entity.EndUses.Where(x => x.EntityState == EntityState.Unchanged).ToList().ForEach(x => x.EntityState = EntityState.Deleted);
            #endregion

            #region LabTestRequisitionSpecialFinishDyeMethod
            LabTestRequisitionSpecialFinishDyeMethod finishDyeMethod = new LabTestRequisitionSpecialFinishDyeMethod();
            foreach (LabTestRequisitionSpecialFinishDyeMethod child in model.FinishDyeMethods)
            {
                finishDyeMethod = entity.FinishDyeMethods.FirstOrDefault(x => x.FinishDyeMethodID == child.FinishDyeMethodID);

                if (finishDyeMethod == null)
                {
                    finishDyeMethod = child;
                    finishDyeMethod.LTReqBuyerID = nLTReqBuyerID;
                    finishDyeMethod.EntityState = EntityState.Added;
                    entity.FinishDyeMethods.Add(finishDyeMethod);
                }
                else
                {
                    finishDyeMethod.SeqNo = child.SeqNo;
                    finishDyeMethod.EntityState = EntityState.Modified;
                }
            }
            entity.FinishDyeMethods.Where(x => x.EntityState == EntityState.Unchanged).ToList().ForEach(x => x.EntityState = EntityState.Deleted);
            #endregion

            #endregion

            var obj = await _LabTestRequisitionService.ReviseAsync(entity);
            await _commonService.UpdateFreeConceptStatus(InterfaceFrom.LabTestRequisition, entity.ConceptID, "", entity.BookingID, 0, 0, entity.ColorID, entity.ItemMasterID);
            return Ok(obj);
        }

        [Route("approve")]
        [HttpPost]
        public async Task<IActionResult> Approve(LabTestRequisitionMaster model)
        {
            LabTestRequisitionMaster entity;
            entity = await _LabTestRequisitionService.GetAllByIDAsync(model.LTReqMasterID);
            foreach (LabTestRequisitionBuyer reqBuyer in entity.LabTestRequisitionBuyers)
            {
                reqBuyer.LabTestRequisitionBuyerParameters = entity.LabTestRequisitionBuyerParameters.Where(x => x.LTReqBuyerID == reqBuyer.LTReqBuyerID).ToList();
            }

            if (model.IsApproved)
            {
                entity.IsApproved = true;
                entity.ApprovedBy = AppUser.UserCode;
                entity.ApprovedDate = DateTime.Now;
            }
            if (model.IsAcknowledge)
            {
                entity.IsAcknowledge = true;
                entity.AcknowledgeBy = AppUser.UserCode;
                entity.AcknowledgeDate = DateTime.Now;
                entity.UnAcknowledge = false;
            }
            if(model.UnAcknowledge)
            {
                entity.IsApproved = false;
                entity.IsAcknowledge = false;
                entity.UnAcknowledge = true;
                entity.UnAcknowledgeBy = AppUser.UserCode;
                entity.UnAcknowledgeDate = DateTime.Now;
                entity.UnAcknowledgeReason = model.UnAcknowledgeReason;
            }
            entity.EntityState = EntityState.Modified;
            await _LabTestRequisitionService.UpdateEntityAsync(entity);
            await _commonService.UpdateFreeConceptStatus(InterfaceFrom.LabTestRequisition, entity.ConceptID, "", entity.BookingID, 0, 0, entity.ColorID, entity.ItemMasterID);
            return Ok();
        }
    }
}