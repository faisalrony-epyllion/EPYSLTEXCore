using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces.SCD;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Statics;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using EPYSLTEXCore.Infrastructure.Static;
using Dapper;
using EPYSLTEXCore.Infrastructure.Exceptions;

namespace EPYSLTEXCore.Application.Services.SCD
{
    public class YarnBBLCProposalService : IYarnBBLCProposalService
    {
        private readonly IDapperCRUDService<YarnBBLCProposalMaster> _service;
        //private readonly ISignatureRepository _signatureRepository;
        private readonly SqlConnection _connection;

        public YarnBBLCProposalService(IDapperCRUDService<YarnBBLCProposalMaster> service
            //, ISignatureRepository signatureRepository
            )
        {
            _service = service;
            //_signatureRepository = signatureRepository;
            _connection = service.Connection;
        }

        public async Task<List<YarnBBLCProposalMaster>> GetListAsync(Status status, bool isCDAPage, PaginationInfo paginationInfo)
        {
            string orderBy;
            string sql;
            if (status == Status.Pending)
            {
                sql = $@"
                With YARN As 
                (
	                Select YPIReceiveMasterID, PIDate, RevisionNo, RevisionDate, SupplierID, Remarks, ReceivePI, ReceivePIBy, 
                    ReceivePIDate, PIFilePath, AttachmentPreviewTemplate, NetPIValue, IncoTermsID, PaymentTermsID, TypeOfLCID, 
                    TenureofLC, CalculationofTenure, CreditDays, OfferValidity, ReImbursementCurrencyID, Charges, CountryOfOriginID, 
                    TransShipmentAllow, ShippingTolerance, PortofLoadingID, PortofDischargeID, ShipmentModeID, YPINo, CompanyId,
                    PONo
	                From {TableNames.YarnPIReceiveMaster} 
                    Where Accept = 1 AND IsCDA = '{isCDAPage}' 
                    And YPIReceiveMasterID Not In (Select YPIReceiveMasterID From {TableNames.YarnBBLCProposalChild} )
                ),
	            YPIRM AS 
                (
	                SELECT YARN.YPIReceiveMasterID, YARN.YPINo, YARN.PIDate, YARN.SupplierID, YARN.CompanyID, PIFilePath, 
                    PONo, SUM(YPIRC.PIQty) AS TotalQty, YARN.NetPIValue AS TotalValue --SUM(YPIRC.PIValue) AS TotalValue
	                FROM YARN
	                INNER JOIN {TableNames.YarnPIReceiveChild}  YPIRC ON YARN.YPIReceiveMasterID = YPIRC.YPIReceiveMasterID
	                GROUP BY YARN.YPIReceiveMasterID, YARN.YPINo, YARN.PIDate, YARN.SupplierID, YARN.CompanyID, 
                    PIFilePath, AttachmentPreviewTemplate, PONo,YARN.NetPIValue
                ),
                FinalList AS
                (
				    SELECT YPIRM.YPIReceiveMasterID, YPIRM.YPINo, YPIRM.PIDate, YPIRM.SupplierID, YPIRM.CompanyID, YPIRM.PIFilePath,
                    YPIRM.PONo, C.[Name] AS SupplierName, CE.ShortName CompanyName, YPIRM.TotalQty, YPIRM.TotalValue, 
                    Count(*) Over() TotalRows
		            FROM YPIRM
				    INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = YPIRM.SupplierID
	                INNER JOIN {DbNames.EPYSL}..CompanyEntity CE On CE.CompanyID = YPIRM.CompanyId
	                GROUP BY YPIRM.YPIReceiveMasterID, YPIRM.YPINo, YPIRM.PIDate, YPIRM.SupplierID, YPIRM.CompanyID, 
                    YPIRM.PIFilePath, YPIRM.PONo, C.[Name], CE.ShortName, YPIRM.TotalQty, YPIRM.TotalValue
                )
                SELECT * FROM FinalList";
                orderBy = "ORDER BY YPIReceiveMasterID DESC";
            }
            else if (status == Status.PartiallyCompleted)
            {
                sql = $@"
                        
                With
                RVSList AS
                (
	                SELECT YPI.ProposalID
	                FROM {TableNames.YarnBBLCProposalChild}  YPI
	                INNER JOIN {TableNames.YarnPIReceiveMaster} YPO ON YPO.YPIReceiveMasterID = YPI.YPIReceiveMasterID
	                WHERE YPI.RevisionNo <> YPO.RevisionNo
                ),
                M As 
                (
					Select ProposalID, ProposalNo, ProposalDate, SupplierID, CompanyID, YPINo, CashStatus,ProposeContractID, ProposeBankID,RetirementModeID,
                    RevisionNo
	                From {TableNames.YarnBBLCProposalMaster} 
	                Where IsCDA = '{isCDAPage}' AND ProposalID Not IN (Select ProposalID From {TableNames.YarnLCMaster} )
                )
				,BBLC As 
                (
					Select M.ProposalID, ProposalNo, M.ProposalDate, CE.ShortName CompanyName, CT.ShortName SupplierName, 
                    M.CompanyID, M.SupplierID, M.YPINo, M.CashStatus,M.ProposeContractID,BC.BBLCNo ProposeContract, M.ProposeBankID, BB.BranchName, M.RetirementModeID,M.RevisionNo,RetirementMode = ETV.ValueName,
                    SUM(PIC.PIQty) TotalQty,SUM(PIC.PIValue) TotalValue
					From M
					Inner Join {TableNames.YarnBBLCProposalChild}  C On C.ProposalID = M.ProposalID
					Inner Join {TableNames.YarnPIReceiveChild}  PIC ON PIC.YPIReceiveMasterID = C.YPIReceiveMasterID
                    Inner Join {TableNames.YarnPIReceiveMaster} PIM on PIM.YPIReceiveMasterID=C.YPIReceiveMasterID
                    --INNER JOIN {TableNames.YarnPIReceivePO}  YPIPO ON YPIPO.YPIReceiveMasterID=PIC.YPIReceiveMasterID
	                --INNER JOIN {TableNames.YarnPOMaster}  YPO ON YPO.YPOMasterID = YPIPO.YPOMasterID
					Inner Join {DbNames.EPYSL}..CompanyEntity CE On M.CompanyID = CE.CompanyID
					Inner Join {DbNames.EPYSL}..Contacts CT On M.SupplierID = CT.ContactID
					Left Join {DbNames.EPYSL}..ContractMAster CM On CM.ContractID = M.ProposeContractID
                    Left Join {DbNames.EPYSL}..BankBranch BB ON BB.BankBranchID = M.ProposeBankID
                    Left Join {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = M.RetirementModeID
                    LEFT JOIN RVSList RV ON RV.ProposalID = C.ProposalID
					LEFT JOIN {DbNames.EPYSL}..BBLC BC ON BC.BBLCID = M.ProposeContractID
                    WHERE C.RevisionNo = PIM.RevisionNo AND RV.ProposalID IS NULL
                    --WHERE M.RevisionNo =YPIPO.RevisionNo
					Group By M.RevisionNo,M.ProposalID, ProposalNo, M.ProposalDate, CE.ShortName, CT.ShortName,CM.ContractNo, 
                    M.CashStatus,M.ProposeContractID,BC.BBLCNo, M.ProposeBankID, BB.BranchName, M.CompanyID, M.SupplierID, M.YPINo, M.RetirementModeID,ETV.ValueName
				),
                FinalList AS
                (
                    Select ProposalID, ProposalNo, ProposalDate, CompanyName, SupplierName, CompanyID, SupplierID, 
                    YPINo, CashStatus,ProposeContractID,ProposeContract, ProposeBankID, BranchName,RetirementModeID,RetirementMode, TotalQty, TotalValue
				    From BBLC
                )
				SELECT *, Count(*) Over() TotalRows FROM FinalList";

                orderBy = "ORDER BY ProposalID DESC";

                //            sql = $@"
                //            With
                //            RVSList AS
                //            (
                //             SELECT YPI.ProposalID
                //             FROM {TableNames.YarnBBLCProposalChild}  YPI
                //             INNER JOIN {TableNames.YarnPIReceiveMaster} YPO ON YPO.YPIReceiveMasterID = YPI.YPIReceiveMasterID
                //             WHERE YPI.RevisionNo <> YPO.RevisionNo
                //            ),
                //            M As 
                //            (
                //	Select ProposalID, ProposalNo, ProposalDate, SupplierID, CompanyID, YPINo, CashStatus, ProposeBankID,RetirementModeID,
                //                RevisionNo
                //             From {TableNames.YarnBBLCProposalMaster} 
                //             Where IsCDA = '{isCDAPage}' AND ProposalID Not IN (Select ProposalID From {TableNames.YarnLCMaster} )
                //            )
                //,BBLC As 
                //            (
                //	Select M.ProposalID, ProposalNo, M.ProposalDate, CE.ShortName CompanyName, CT.ShortName SupplierName, 
                //                M.CompanyID, M.SupplierID, M.YPINo, M.CashStatus, M.ProposeBankID, BB.BranchName, M.RetirementModeID,M.RevisionNo,RetirementMode = ETV.ValueName,
                //                SUM(PIC.PIQty) TotalQty, SUM(PIC.PIValue) TotalValue
                //	From M
                //	Inner Join {TableNames.YarnBBLCProposalChild}  C On C.ProposalID = M.ProposalID
                //	Inner Join {TableNames.YarnPIReceiveChild}  PIC ON PIC.YPIReceiveMasterID = C.YPIReceiveMasterID
                //                Inner Join {TableNames.YarnPIReceiveMaster} PIM on PIM.YPIReceiveMasterID=C.YPIReceiveMasterID
                //                --INNER JOIN {TableNames.YarnPIReceivePO}  YPIPO ON YPIPO.YPIReceiveMasterID=PIC.YPIReceiveMasterID
                //             --INNER JOIN {TableNames.YarnPOMaster}  YPO ON YPO.YPOMasterID = YPIPO.YPOMasterID
                //	Inner Join {DbNames.EPYSL}..CompanyEntity CE On M.CompanyID = CE.CompanyID
                //	Inner Join {DbNames.EPYSL}..Contacts CT On M.SupplierID = CT.ContactID
                //                Left Join {DbNames.EPYSL}..BankBranch BB ON BB.BankBranchID = M.ProposeBankID
                //                Left Join {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = M.RetirementModeID
                //                LEFT JOIN RVSList RV ON RV.ProposalID = C.ProposalID
                //                WHERE C.RevisionNo = PIM.RevisionNo AND RV.ProposalID IS NULL
                //                --WHERE M.RevisionNo =YPIPO.RevisionNo
                //	Group By M.RevisionNo,M.ProposalID, ProposalNo, M.ProposalDate, CE.ShortName, CT.ShortName, 
                //                M.CashStatus, M.ProposeBankID, BB.BranchName, M.CompanyID, M.SupplierID, M.YPINo, M.RetirementModeID,ETV.ValueName
                //),
                //            FinalList AS
                //            (
                //                Select ProposalID, ProposalNo, ProposalDate, CompanyName, SupplierName, CompanyID, SupplierID, 
                //                YPINo, CashStatus, ProposeBankID, BranchName,RetirementModeID,RetirementMode, TotalQty, TotalValue
                //    From BBLC
                //            )
                //SELECT *, Count(*) Over() TotalRows FROM FinalList";

                //            orderBy = "ORDER BY ProposalID DESC";
            }
            else if (status == Status.Revise)
            {
                sql = $@"
                With M As 
                (
					Select ProposalID, ProposalNo, ProposalDate, SupplierID, CompanyID, YPINo, CashStatus, ProposeBankID ,RetirementModeID
	                From {TableNames.YarnBBLCProposalMaster} 
	                Where IsCDA = '{isCDAPage}'
                )
				, BBLC As 
                (
					Select M.ProposalID, ProposalNo, M.ProposalDate, CE.ShortName CompanyName, CT.ShortName SupplierName, 
                    M.CompanyID, M.SupplierID, M.YPINo, M.CashStatus, M.ProposeBankID, BB.BranchName, M.RetirementModeID,RetirementMode = ETV.ValueName,
                    SUM(PIC.PIQty) TotalQty, SUM(PIC.PIValue) TotalValue
                    ,PIAcceptStatus=PIM.Accept
					From M
					Inner Join {TableNames.YarnBBLCProposalChild}  C On C.ProposalID = M.ProposalID
					Inner Join {TableNames.YarnPIReceiveChild}  PIC ON PIC.YPIReceiveMasterID = C.YPIReceiveMasterID
                    Inner Join {TableNames.YarnPIReceiveMaster} PIM on PIM.YPIReceiveMasterID=C.YPIReceiveMasterID
					Inner Join {DbNames.EPYSL}..CompanyEntity CE On M.CompanyID = CE.CompanyID
					Inner Join {DbNames.EPYSL}..Contacts CT On M.SupplierID = CT.ContactID
                    Left Join {DbNames.EPYSL}..BankBranch BB ON BB.BankBranchID = M.ProposeBankID
                    Left Join {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = M.RetirementModeID
                    WHERE C.RevisionNo <>PIM.RevisionNo
					Group By M.ProposalID, ProposalNo, M.ProposalDate, CE.ShortName, CT.ShortName, 
                    M.CashStatus, M.ProposeBankID, BB.BranchName, M.CompanyID, M.SupplierID, M.YPINo, M.RetirementModeID,ETV.ValueName,PIM.Accept
				),
                FinalList AS
                (
                    Select ProposalID, ProposalNo, ProposalDate, CompanyName, SupplierName, CompanyID, SupplierID, 
                    YPINo, CashStatus, ProposeBankID, BranchName,RetirementModeID,RetirementMode, TotalQty, TotalValue,PIAcceptStatus
				    From BBLC
                )
				SELECT *, Count(*) Over() TotalRows FROM FinalList";

                orderBy = "ORDER BY ProposalID DESC";
            }
            else
            {
                sql = $@"
                With 
                RVSList AS
                (
	                SELECT YPI.ProposalID
	                FROM {TableNames.YarnBBLCProposalChild}  YPI
	                INNER JOIN {TableNames.YarnPIReceiveMaster} YPO ON YPO.YPIReceiveMasterID = YPI.YPIReceiveMasterID
	                WHERE YPI.RevisionNo <> YPO.RevisionNo
                ),
                M As 
                (
					Select ProposalID, ProposalNo, ProposalDate, SupplierID, CompanyID, YPINo, CashStatus, ProposeBankID ,RetirementModeID,
                    RevisionNo,ProposeContractID
	                From {TableNames.YarnBBLCProposalMaster} 
	                Where IsCDA = '{isCDAPage}' -- AND ProposalID Not IN (Select ProposalID From {TableNames.YarnLCMaster} )
                )
				, BBLC As 
                (
					Select M.ProposalID, ProposalNo, M.ProposalDate, CE.ShortName CompanyName, CT.ShortName SupplierName, 
                    M.CompanyID, M.SupplierID, M.YPINo, M.CashStatus, M.ProposeBankID, BB.BranchName, M.RetirementModeID,M.RevisionNo,RetirementMode = ETV.ValueName,
                    SUM(PIC.PIQty) TotalQty, SUM(PIC.PIValue) TotalValue,M.ProposeContractID,BC.BBLCNo ProposeContract
					From M
					Inner Join {TableNames.YarnBBLCProposalChild}  C On C.ProposalID = M.ProposalID
					Inner Join {TableNames.YarnPIReceiveChild}  PIC ON PIC.YPIReceiveMasterID = C.YPIReceiveMasterID
                    Inner Join {TableNames.YarnPIReceiveMaster} PIM on PIM.YPIReceiveMasterID=C.YPIReceiveMasterID
                    --INNER JOIN {TableNames.YarnPIReceivePO}  YPIPO ON YPIPO.YPIReceiveMasterID=PIC.YPIReceiveMasterID
	                --INNER JOIN {TableNames.YarnPOMaster}  YPO ON YPO.YPOMasterID = YPIPO.YPOMasterID
					Inner Join {DbNames.EPYSL}..CompanyEntity CE On M.CompanyID = CE.CompanyID
					Inner Join {DbNames.EPYSL}..Contacts CT On M.SupplierID = CT.ContactID
                    Left Join {DbNames.EPYSL}..BankBranch BB ON BB.BankBranchID = M.ProposeBankID
                    Left Join {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = M.RetirementModeID
                    LEFT JOIN RVSList RV ON RV.ProposalID = C.ProposalID
                    LEFT JOIN {DbNames.EPYSL}..BBLC BC ON BC.BBLCID = M.ProposeContractID
                    WHERE C.RevisionNo = PIM.RevisionNo AND RV.ProposalID IS NULL
                    --WHERE M.RevisionNo =YPIPO.RevisionNo
					Group By M.RevisionNo,M.ProposalID, ProposalNo, M.ProposalDate, CE.ShortName, CT.ShortName, 
                    M.CashStatus,M.ProposeContractID,BC.BBLCNo, M.ProposeBankID, BB.BranchName, M.CompanyID, M.SupplierID, M.YPINo, M.RetirementModeID,ETV.ValueName
				),
                FinalList AS
                (
                    Select ProposalID, ProposalNo, ProposalDate, CompanyName, SupplierName, CompanyID, SupplierID, 
                    YPINo, CashStatus,ProposeContractID,ProposeContract, ProposeBankID, BranchName,RetirementModeID,RetirementMode, TotalQty, TotalValue
				    From BBLC
                )
				SELECT *, Count(*) Over() TotalRows FROM FinalList";

                orderBy = "ORDER BY ProposalID DESC";
            }

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<YarnBBLCProposalMaster>(sql);
        }


        public async Task<YarnBBLCProposalMaster> GetNewAsync(int[] piReceiveMasterIdArray)

        {

            var query = $@"

                -- PI Receive Info

                ;With

                M As (

	                Select YPIReceiveMasterID, YPINo, RevisionNo,PIDate, PONo, SupplierID, CompanyId,  PIFilePath, NetPIValue 

	                From {TableNames.YarnPIReceiveMaster}

	                Where YPIReceiveMasterID In @PIReceiveMasterIds

                )

                Select M.YPIReceiveMasterID, M.RevisionNo,M.YPINo, PIDate, M.SupplierID, M.CompanyId, M.PIFilePath, 

                CE.ShortName CompanyName, CT.ShortName SupplierName, U.DisplayUnitDesc Unit, 

                Sum(C.PIQty) TotalQty,M.NetPIValue TotalValue --Sum(C.PIValue) TotalValue

                From M

                Inner Join {TableNames.YarnPIReceiveChild}  C On M.YPIReceiveMasterID = C.YPIReceiveMasterID

                Inner Join {DbNames.EPYSL}..CompanyEntity CE  On M.CompanyId = CE.CompanyID

                Inner Join {DbNames.EPYSL}..Contacts CT On CT.ContactID = M.SupplierID

                Inner Join {DbNames.EPYSL}..Unit U On C.UnitID = U.UnitID

                Group By M.YPIReceiveMasterID, M.RevisionNo,M.YPINo, PIDate, M.PONo, M.SupplierID, M.CompanyId, 

                M.PIFilePath, CE.ShortName, CT.ShortName, U.DisplayUnitDesc,M.NetPIValue;

                --ProposeBankList

                ;SELECT CAST(BB.BankBranchID AS VARCHAR) id, BB.BranchName text

                FROM {DbNames.EPYSL}..BankBranch BB  

                Group by BB.BankBranchID, BB.BranchName; 

                --RetirementModeList

                Select id= CAST(ETV.ValueID AS VARCHAR),text = ETV.ValueName

                From {DbNames.EPYSL}..EntityTypeValue ETV

                Inner Join {DbNames.EPYSL}..EntityType ET On ET.EntityTypeID = ETV.EntityTypeID                   

                Where ET.EntityTypeName = 'Retirement Mood'";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query, new
                {
                    PIReceiveMasterIds = piReceiveMasterIdArray
                });

                YarnBBLCProposalMaster data = new YarnBBLCProposalMaster();
                data.Childs = records.Read<YarnBBLCProposalChild>().ToList();
                data.CompanyID = data.Childs.First().CompanyId;
                data.SupplierID = data.Childs.First().SupplierId;
                data.CompanyName = data.Childs.First().CompanyName;
                data.SupplierName = data.Childs.First().SupplierName;
                data.ProposeBankList = records.Read<Select2OptionModel>().ToList();
                data.RetirementModeList = records.Read<Select2OptionModel>().ToList();
                return data;
            }

            catch (Exception ex)
            {
                throw ex;
            }

            finally
            {

                if (_connection.State == System.Data.ConnectionState.Open) _connection.Close();

            }

        }

        public async Task<List<YarnBBLCProposalMaster>> GetBBLCProposalsForMergeAsync(int companyId, int supplierId, bool isCDAPage)
        {
            var sql = $@"
            With M As 
            (
	            Select BBLC.ProposalID, BBLC.ProposalNo, BBLC.ProposalDate, BBLC.SupplierID, BBLC.CompanyID, BBLC.YPINo,
	            YLM.LCNo, YLM.LCDate
	            From {TableNames.YarnBBLCProposalMaster}  BBLC Left Join {TableNames.YarnLCMaster}  YLM On BBLC.ProposalID = YLM.ProposalID 
	            Where BBLC.CompanyID = {companyId} And BBLC.SupplierID = {supplierId} And BBLC.IsCDA = '{isCDAPage}' 
            )

            Select M.ProposalID, ProposalNo, M.ProposalDate, CE.ShortName CompanyName, CT.ShortName SupplierName, M.YPINo,
            M.LCNo, M.LCDate, SUM(PIC.PIQty) TotalQty, SUM(PIC.PIValue) TotalValue
            From M
            Inner Join {TableNames.YarnBBLCProposalChild}  C On C.ProposalID = M.ProposalID
            Inner Join {TableNames.YarnPIReceiveChild}  PIC ON PIC.YPIReceiveMasterID = C.YPIReceiveMasterID
            Inner Join {DbNames.EPYSL}..CompanyEntity CE On M.CompanyID = CE.CompanyID
            Inner Join {DbNames.EPYSL}..Contacts CT On M.SupplierID = CT.ContactID
            Group By M.ProposalID, ProposalNo, M.ProposalDate, CE.ShortName, CT.ShortName, M.YPINo, M.LCNo, M.LCDate ";

            return await _service.GetDataAsync<YarnBBLCProposalMaster>(sql);
        }

        public async Task<YarnBBLCProposalMaster> GetMergedDataAsync(int proposalId, int[] piReceiveMasterIdArray)
        {
            string PIReceiveMasterIds = string.Join(",", piReceiveMasterIdArray);


            var query = $@"
                -- Master Info
                ;With M AS 
                (
                    Select M.ProposalID, M.ProposalNo, M.ProposalDate, M.RevisionNo, M.Remarks, M.YPINo, M.SupplierID, M.CompanyID, 
	                M.ProposeContractID, Coalesce(LC.BBLCNo, CM.ContractNo) ProposeContract, IsNull(YLM.LCID, 0)LCID ,M.ProposeBankID,M.ProposeBankName 
	                From {TableNames.YarnBBLCProposalMaster}  M
                    Left Join {TableNames.YarnLCMaster}  YLM On M.ProposalID = YLM.ProposalID
	                Left Join {DbNames.EPYSL}..ContractMAster CM On CM.ContractID = M.ProposeContractID And M.CompanyID = 11
	                Left Join {DbNames.EPYSL}..BBLC LC On LC.BBLCID = M.ProposeContractID And M.CompanyID != 11  
	                Where M.ProposalID = {proposalId} 
                )
                Select M.ProposalID, M.ProposalNo, M.ProposalDate, M.RevisionNo, M.Remarks, M.YPINo, M.SupplierID, 
                M.CompanyID, CE.ShortName CompanyName, CT.ShortName SupplierName, M.ProposeContractID, M.ProposeContract, LCID,M.ProposeBankID,M.ProposeBankName  
                From M
                Inner Join {DbNames.EPYSL}..CompanyEntity CE On M.CompanyID = CE.CompanyID
                Inner Join {DbNames.EPYSL}..Contacts CT On M.SupplierID = CT.ContactID;
 
                -- Childs
                ;With M As 
                (
	                Select ProposalID, ProposalNo, ProposalDate, RevisionNo, Remarks, YPINo, SupplierID, CompanyID  
	                From {TableNames.YarnBBLCProposalMaster}  Where ProposalID = {proposalId}
                )
				, PIC As 
                (
	                Select 0 ChildID, 0 ProposalID, M.YPIReceiveMasterID, YPINo, PIFilePath, C.UnitID UnitID, 
                    U.DisplayUnitDesc Unit, PIDate, Sum(C.PIQty) TotalQty, Sum(C.PIValue) TotalValue
	                From {TableNames.YarnPIReceiveMaster} M
					Inner Join {TableNames.YarnPIReceiveChild}  C On M.YPIReceiveMasterID = C.YPIReceiveMasterID
					Inner Join {DbNames.EPYSL}..Unit U ON C.UnitID = U.UnitID
	                Where M.YPIReceiveMasterID In ({PIReceiveMasterIds})
					Group By M.YPIReceiveMasterID, YPINo, PIFilePath, C.UnitID, U.DisplayUnitDesc, PIDate
                )
				,LCC AS 
                (
                    Select C.ChildID, C.ProposalID, PIM.YPIReceiveMasterID, PIM.YPINo, PIM.PIFilePath, U.UnitID, 
                    U.DisplayUnitDesc Unit, PIM.PIDate, Sum(PIC.PIQty) TotalQty, Sum(PIC.PIValue) TotalValue
                    From M
                    Inner Join {TableNames.YarnBBLCProposalChild}  C On M.ProposalID = C.ProposalID 
                    Inner Join {TableNames.YarnPIReceiveMaster} PIM ON C.YPIReceiveMasterID = PIM.YPIReceiveMasterID --AND PIM.YPINo = M.YPINo
                    Inner Join {TableNames.YarnPIReceiveChild}  PIC On PIC.YPIReceiveMasterID = PIM.YPIReceiveMasterID AND C.YPIReceiveMasterID = C.YPIReceiveMasterID
                    Inner Join {DbNames.EPYSL}..CompanyEntity CE  On M.CompanyId = CE.CompanyID
                    Inner Join {DbNames.EPYSL}..Contacts CT On CT.ContactID = M.SupplierID
                    Inner Join {DbNames.EPYSL}..Unit U On PIC.UnitID = U.UnitID
                    Group By C.ChildID, C.ProposalID, PIM.YPIReceiveMasterID, PIM.YPINo, PIM.PIFilePath, CE.ShortName, 
                    CT.ShortName, U.UnitID, PIM.PIDate, U.DisplayUnitDesc
				) 
				Select ChildID, ProposalID, YPIReceiveMasterID, YPINo, PIFilePath, PIDate, UnitID, Unit, TotalQty, TotalValue
				From LCC
				Union
				Select ChildID, ProposalID, YPIReceiveMasterID, YPINo, PIFilePath, PIDate, UnitID, Unit, TotalQty, TotalValue 
				From PIC;
 
                -- Export L/C List
                ;Select Cast(BBLCID As varchar) [id], BBLCNo [text]
                From {DbNames.EPYSL}..BBLC
                Where SupplierId In 
                (
	                Select ContactID 
	                From {DbNames.EPYSL}..Contacts Where MappingCompanyID In (6, 8)
                )
                Order By BBLCDate Desc;
 
                SELECT CAST(BB.BankBranchID AS VARCHAR) id, BB.BranchName text
                FROM {DbNames.EPYSL}..BankBranch BB  
                Group by BB.BankBranchID, BB.BranchName;
 
                --RetirementModeList
                Select id= CAST(ETV.ValueID AS VARCHAR),text = ETV.ValueName
                From {DbNames.EPYSL}..EntityTypeValue ETV
                Inner Join {DbNames.EPYSL}..EntityType ET On ET.EntityTypeID = ETV.EntityTypeID                   
                Where ET.EntityTypeName = 'Retirement Mood'";


            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);

                var data = await records.ReadFirstOrDefaultAsync<YarnBBLCProposalMaster>();
                data.Childs = records.Read<YarnBBLCProposalChild>().ToList();
                data.TExportLCList = records.Read<Select2OptionModel>().ToList();
                data.ProposeBankList = records.Read<Select2OptionModel>().ToList();
                data.RetirementModeList = records.Read<Select2OptionModel>().ToList();
                data.EntityState = EntityState.Modified;
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open) _connection.Close();
            }
        }

        public async Task<YarnBBLCProposalMaster> GetAsync(int id)
        {
            var query = $@"
                -- Master Info
                ;With 
                M AS 
                (
                    Select ProposalID, ProposalNo, ProposalDate, RevisionNo, Remarks, YPINo, SupplierID, 
                    CompanyID ,ProposeContractID, CashStatus, ProposeBankID, ProposeBankName,RetirementModeID 
	                From {TableNames.YarnBBLCProposalMaster}  
	                Where ProposalID = {id} 
                )

                Select M.ProposalID, M.ProposalNo, M.ProposalDate, M.RevisionNo, M.Remarks, M.YPINo, M.SupplierID, M.CompanyID, 
                CE.ShortName CompanyName, CT.ShortName SupplierName,M.ProposeContractID, CashStatus, ProposeBankID, 
                M.ProposeBankName, Coalesce(LC.BBLCNo, CM.ContractNo) ProposeContract,M.RetirementModeID,RetirementMode = ETV.ValueName 
                From M
                Inner Join {DbNames.EPYSL}..CompanyEntity CE On M.CompanyID = CE.CompanyID
                Left Join {DbNames.EPYSL}..Contacts CT On M.SupplierID = CT.ContactID 
                Left Join {DbNames.EPYSL}..ContractMAster CM On CM.ContractID = M.ProposeContractID And M.CompanyID = 11
                Left Join {DbNames.EPYSL}..BBLC LC On LC.BBLCID = M.ProposeContractID And M.CompanyID != 11  
                Left Join {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = M.RetirementModeID;

                -- Childs
                ;With 
                M As (
	                Select C.ChildID, C.YPIReceiveMasterID, MM.ProposalID, MM.ProposalNo, MM.ProposalDate, C.RevisionNo, MM.Remarks, MM.YPINo, MM.SupplierID, 
					MM.CompanyID
	                From {TableNames.YarnBBLCProposalChild}  C 
					INNER JOIN {TableNames.YarnBBLCProposalMaster}  MM ON MM.ProposalID = C.ProposalID
					Where MM.ProposalID = {id}
                )
				SELECT M.ChildID, M.ProposalID, M.YPIReceiveMasterID, PIM.YPINo, M.RevisionNo,PIM.RevisionNo PIRevisionNo,PIM.PIFilePath, U.UnitID, U.DisplayUnitDesc Unit, PIM.PIDate, 
				Sum(PIC.PIQty) TotalQty, TotalValue=PIM.NetPIValue --Sum(PIC.PIValue) TotalValue
				FROM M
				Inner Join {TableNames.YarnPIReceiveMaster} PIM ON PIM.YPIReceiveMasterID = M.YPIReceiveMasterID
				Inner Join {TableNames.YarnPIReceiveChild}  PIC On PIC.YPIReceiveMasterID = PIM.YPIReceiveMasterID
                Inner Join {DbNames.EPYSL}..CompanyEntity CE  On M.CompanyId = CE.CompanyID
                Inner Join {DbNames.EPYSL}..Contacts CT On CT.ContactID = M.SupplierID
                Inner Join {DbNames.EPYSL}..Unit U On PIC.UnitID = U.UnitID
				GROUP BY M.ChildID, M.ProposalID, M.YPIReceiveMasterID, PIM.YPINo, M.RevisionNo,PIM.RevisionNo,PIM.PIFilePath, U.UnitID, 
                U.DisplayUnitDesc, PIM.PIDate,PIM.NetPIValue;

                SELECT CAST(BB.BankBranchID AS VARCHAR) id, BB.BranchName text
                FROM {DbNames.EPYSL}..BankBranch BB  
                Group by BB.BankBranchID, BB.BranchName;
 
                --RetirementModeList
                Select id= CAST(ETV.ValueID AS VARCHAR),text = ETV.ValueName
                From {DbNames.EPYSL}..EntityTypeValue ETV
                Inner Join {DbNames.EPYSL}..EntityType ET On ET.EntityTypeID = ETV.EntityTypeID                   
                Where ET.EntityTypeName = 'Retirement Mood'


                -- YarnLCChild
                Select YLC.* 
                From {TableNames.YarnLCChild}  YLC
                Inner Join {TableNames.YarnLCMaster}  YLM ON YLM.LCID = YLC.LCID
                Where ProposalID = {id}";

            try
            {

                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                var data = await records.ReadFirstOrDefaultAsync<YarnBBLCProposalMaster>();
                data.Childs = records.Read<YarnBBLCProposalChild>().ToList();
                data.EntityState = EntityState.Modified;
                data.ProposeBankList = records.Read<Select2OptionModel>().ToList();
                data.RetirementModeList = records.Read<Select2OptionModel>().ToList();
                data.YarnLCChilds = records.Read<YarnLcChild>().ToList();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open) _connection.Close();
            }
        }
        public async Task<YarnBBLCProposalMaster> GetNatureAsync(int id)
        {
            var sql = $@"
            ;Select BusinessNature From {DbNames.EPYSL}..CompanyEntity Where CompanyID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnBBLCProposalMaster data = await records.ReadFirstOrDefaultAsync<YarnBBLCProposalMaster>();
                Guard.Against.NullObject(data);
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open) _connection.Close();
            }
        }
        public async Task<List<YarnBBLCProposalMaster>> GetProposeContractForPCAsync(int id, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By ContractID ASC" : paginationInfo.OrderBy;
            var sql = $@"
            ;WITH A AS 
			(
				Select CM.ContractID ProposeContractID, CM.ContractID, CM.ContractNo ProposeContract,
	            BB2.BankBranchID AS ProposeBankID, BB2.BranchName As ProposeBankName
	            From {DbNames.EPYSL}..ContractMAster CM
	            Left Join {DbNames.EPYSL}..BankBranch BB2 On BB2.BankBranchID = CM.PaymentBankID
				where CompanyID= {id} And BB2.BankBranchID != 0
			)
			SELECT * FROM A 
            ";
            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";
            return await _service.GetDataAsync<YarnBBLCProposalMaster>(sql);

        }
        public async Task<List<YarnBBLCProposalMaster>> GetLCContractNoAsync(int id, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By BBLCID ASC" : paginationInfo.OrderBy;
            var sql = $@"
            ;With M AS
            (
	            Select ContactID From {DbNames.EPYSL}..Contacts  where MappingCompanyID = {id}
	        ),
            A AS 
			(
				Select LC.BBLCID ProposeContractID, LC.BBLCID, LC.BBLCNo ProposeContract,
	            BB.BankBranchID AS ProposeBankID, BB.BranchName As ProposeBankName
	            From M 
                Inner join {DbNames.EPYSL}..BBLC LC on LC.SupplierID=M.ContactID 
	            Left Join {DbNames.EPYSL}..BankBranch BB On BB.BankBranchID = LC.BankBranchID
				Where LC.BBLCNo != '' And BB.BankBranchID != 0
			)
			SELECT * FROM A ";
            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";
            return await _service.GetDataAsync<YarnBBLCProposalMaster>(sql);

        }
        public async Task<YarnBBLCProposalMaster> GetAllByIDAsync(int id)
        {
            string sql = $@"
            ;Select * From {TableNames.YarnBBLCProposalMaster}  Where ProposalID = {id}

            ;Select * From {TableNames.YarnBBLCProposalChild}  Where ProposalID = {id}

            ;Select * From {TableNames.YarnLCMaster}  Where ProposalID = {id}

            ; Select * From {TableNames.YarnLCChild}  Where LCID IN(Select LCID From {TableNames.YarnLCMaster}  Where ProposalID = {id})

            ;SELECT PIM.*
            FROM {TableNames.YarnPIReceiveMaster} PIM
            INNER JOIN {TableNames.YarnBBLCProposalChild}  PC ON PC.YPIReceiveMasterID = PIM.YPIReceiveMasterID
            WHERE PC.ProposalID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnBBLCProposalMaster data = records.Read<YarnBBLCProposalMaster>().FirstOrDefault();
                data.Childs = records.Read<YarnBBLCProposalChild>().ToList();
                data.YarnLcMasters = records.Read<YarnLcMaster>().ToList();
                data.YarnLCChilds = records.Read<YarnLcChild>().ToList();
                data.YarnPIReceives = records.Read<YarnPIReceiveMaster>().ToList();
                Guard.Against.NullObject(data);
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open) _connection.Close();
            }
        }

        public async Task SaveAsync(YarnBBLCProposalMaster entity)
        {
            SqlTransaction transaction = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                if (entity.isRevision)
                {
                    await _connection.ExecuteAsync("spBackupYarnBBLCProposalMaster_Full", new { ProposalID = entity.ProposalID, UserId = entity.AddedBy }, transaction, 30, CommandType.StoredProcedure);
                }
                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity = await AddAsync(entity);
                        break;
                    case EntityState.Modified:
                        entity = await UpdateAsync(entity);
                        break;
                    default:
                        break;
                }

                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.Childs, transaction);
                await _service.SaveAsync(entity.YarnLcMasters, transaction);
                await _service.SaveAsync(entity.YarnLCChilds, transaction);
                if (entity.isRevision)
                {
                    await _service.SaveAsync(entity.YarnPIReceives, transaction);
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }

        private async Task<YarnBBLCProposalMaster> AddAsync(YarnBBLCProposalMaster entity)
        {
            entity.ProposalID = await _service.GetMaxIdAsync(TableNames.YarnBBLCProposalMaster);
            entity.ProposalNo = await _service.GetMaxNoAsync("YarnBBLCProposalNo");
            var maxChildId = await _service.GetMaxIdAsync(TableNames.YarnBBLCProposalChild, entity.Childs.Count);

            foreach (var item in entity.Childs)
            {
                item.ChildID = maxChildId++;
                item.ProposalId = entity.ProposalID;
                item.EntityState = EntityState.Added;
            }

            return entity;
        }
        private async Task<YarnBBLCProposalMaster> UpdateAsync(YarnBBLCProposalMaster entity)
        {
            int maxChildId = await _service.GetMaxIdAsync(TableNames.YarnBBLCProposalChild, entity.Childs.Where(x => x.EntityState == EntityState.Added).Count());
            int maxYarnLCChildId = await _service.GetMaxIdAsync(TableNames.YarnLCChild, entity.YarnLCChilds.Where(x => x.EntityState == EntityState.Added).Count());

            foreach (YarnBBLCProposalChild item in entity.Childs.ToList())
            {
                switch (item.EntityState)
                {
                    case EntityState.Added:
                        item.ChildID = maxChildId++;
                        item.ProposalId = entity.ProposalID;
                        item.EntityState = EntityState.Added;
                        break;
                    case EntityState.Modified:
                        item.EntityState = EntityState.Modified;
                        break;
                    default:
                        break;
                }
            }
            //Yarn LC Child in importLC interface
            foreach (YarnLcChild item in entity.YarnLCChilds.ToList())
            {
                switch (item.EntityState)
                {
                    case EntityState.Added:
                        item.ChildID = maxYarnLCChildId++;
                        item.LCID = entity.LCID;
                        item.EntityState = EntityState.Added;
                        break;
                    case EntityState.Modified:
                        item.EntityState = EntityState.Modified;
                        break;
                    default:
                        break;
                }
            }

            return entity;
        }
    }
}
