using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces.SCD;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;

namespace EPYSLTEXCore.Application.Services.SCD
{
    public class ImportLCService : IImportLCService
    {
        private readonly IDapperCRUDService<YarnLcMaster> _service;
        //private readonly ISignatureRepository _signatureRepository;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _gmtConnection;

        public ImportLCService(IDapperCRUDService<YarnLcMaster> service
            //, ISignatureRepository signatureRepository
            )
        {
            _service = service;           
            _gmtConnection = service.GetConnection(AppConstants.GMT_CONNECTION);
            _service.Connection = _service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
        }

        public async Task<List<YarnLcMaster>> GetImportLCData(Status status, bool isCDAPage, PaginationInfo paginationInfo)
        {
            string orderBy = "";
            var sql = string.Empty;
            if (status == Status.Pending)
            {
                sql += $@"
                ;WITH M AS
                (
	                SELECT ProposalID, ProposalNo, ProposalDate, YPINo, SupplierID, CompanyID, ProposeContractID,
                    CashStatus, ProposeBankID,RetirementModeID 
                    FROM {TableNames.YarnBBLCProposalMaster}
	                WHERE IsCDA = '{isCDAPage}' AND ProposalID NOT IN (SELECT ProposalID FROM {TableNames.YarnLCMaster})
                ),
                D AS (
	                SELECT YC.ProposalID, SUM(YPC.PIQty) LCQty, SUM(YPC.PIValue) LCValue 
                    FROM {TableNames.YarnBBLCProposalChild} YC
	                INNER JOIN {TableNames.YarnPIReceiveChild} YPC ON YPC.YPIReceiveMasterID = YC.YPIReceiveMasterID
	                WHERE ProposalID IN (SELECT M.ProposalID FROM M)
	                GROUP BY YC.ProposalID
                ),
                FinalList AS
                (
                    SELECT M.ProposalID, M.ProposalNo, M.ProposalDate, M.YPINo PiNo, M.SupplierID, CC.[Name] SupplierName,
                    M.CompanyID, CE.ShortName CompanyName, D.LCQty, D.LCValue, M.ProposeContractID, 
                    Coalesce(LC.BBLCNo, CM.ContractNo) ProposeContractNo, M.CashStatus, M.ProposeBankID, M.RetirementModeID,RetirementMode = ETV.ValueName,
                    BB.BranchName
                    FROM M
                    INNER JOIN D ON D.ProposalID = M.ProposalID
                    INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
                    Inner Join {DbNames.EPYSL}..CompanyEntity CE On M.CompanyID = CE.CompanyID 
                    Left Join {DbNames.EPYSL}..ContractMAster CM On CM.ContractID = M.ProposeContractID And M.CompanyID = 11
                    Left Join {DbNames.EPYSL}..BBLC LC On LC.BBLCID = M.ProposeContractID And M.CompanyID != 11  
                    Left Join {DbNames.EPYSL}..BankBranch BB ON BB.BankBranchID = M.ProposeBankID
                    Left Join {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = M.RetirementModeID
                )
                SELECT *, Count(*) Over() TotalRows FROM FinalList ";

                orderBy = string.IsNullOrEmpty(orderBy) ? "ORDER BY ProposalID DESC" : orderBy;
            }
            else if (status == Status.PartiallyCompleted)
            {
                sql += $@"With YPL AS
                    (
                        SELECT LM.LCID, LM.LCNo, LM.ProposalId, YBBLM.ProposalNo, LM.LCDate, LM.LCQty, LM.LCValue, 
	                    LM.CompanyID, CE.ShortName CompanyName, LM.SupplierID, CC.Name As SupplierName, LM.Proposed, 
	                    LM.Approve, LM.LCFilePath, LM.AttachmentPreviewTemplate, Coalesce(LC.BBLCNo, CM.ContractNo) ProposeContractNo,
                        LM.CashStatus,YBBLM.RetirementModeID,RetirementMode = ETV.ValueName
                        FROM {TableNames.YarnLCMaster} LM INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = LM.CompanyID
                        Left Join {TableNames.YarnBBLCProposalMaster} YBBLM On YBBLM.ProposalId = LM.ProposalId
                        Left JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = LM.SupplierID
                        Left Join {DbNames.EPYSL}..ContractMAster CM On CM.ContractID = YBBLM.ProposeContractID And YBBLM.CompanyID = 11
                        Left Join {DbNames.EPYSL}..BBLC LC On LC.BBLCID = YBBLM.ProposeContractID And YBBLM.CompanyID != 11 
                        Left Join {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = YBBLM.RetirementModeID
                        WHERE YBBLM.RevisionNo = LM.RevisionNo And Proposed = 1 AND Approve = 0 AND LM.IsCDA = '{isCDAPage}'
                    ),
                    FinalList AS
                    (
                        SELECT LCID, LCNo,ProposalId,ProposalNo, LCDate, CompanyID, CompanyName, SupplierID, SupplierName, LCQty, LCValue,
                        LCFilePath, AttachmentPreviewTemplate, (CASE WHEN Proposed = 1 AND Approve = 0 THEN 'L/C Opening Request' WHEN Proposed = 1 
                        AND Approve = 1 THEN 'L/C' END) AS LCStatus, ProposeContractNo, CashStatus,YPL.RetirementModeID,YPL.RetirementMode  
                        FROM YPL
                    )
                    SELECT *, Count(*) Over() TotalRows FROM FinalList ";

                orderBy = string.IsNullOrEmpty(orderBy) ? "ORDER BY LCID DESC" : orderBy;
            }
            else if (status == Status.Additional)
            {
                sql += $@"With YPL AS
                        (
                            SELECT LM.LCID, LM.LCNo, LM.ProposalId, YBBLM.ProposalNo, LM.LCDate, LM.LCQty, LM.LCValue, LM.CompanyID, 
	                        CE.ShortName CompanyName, LM.SupplierID, CC.Name As SupplierName, LM.Proposed, LM.Approve, 
                            LM.LCFilePath, LM.AttachmentPreviewTemplate, Coalesce(LC.BBLCNo, CM.ContractNo) ProposeContractNo,
                            LM.CashStatus,YBBLM.RetirementModeID,RetirementMode = ETV.ValueName,LM.PreRevisionNo--,YPRM.Accept
							--,Case When YBBLM.RevisionNo = C.RevisionNo  Then 'BBLC Revised' Else 'BBLC Not Revised' END as BBLCStatus
                            FROM	{TableNames.YarnLCMaster} LM
                            INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = LM.CompanyID
                            Left Join {TableNames.YarnBBLCProposalMaster} YBBLM On YBBLM.ProposalId = LM.ProposalId
                            Left JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = LM.SupplierID
                            Left Join {DbNames.EPYSL}..ContractMAster CM On CM.ContractID = YBBLM.ProposeContractID And YBBLM.CompanyID = 11
                            Left Join {DbNames.EPYSL}..BBLC LC On LC.BBLCID = YBBLM.ProposeContractID And YBBLM.CompanyID != 11   
                            Left Join {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = YBBLM.RetirementModeID
							Left Join {TableNames.YarnLCChild} YLC ON YLC.LCID=LM.LCID
							Left Join {TableNames.YarnPIReceiveMaster} YPRM ON YPRM.YPIReceiveMasterID=YLC.YPIReceiveMasterID
							left Join {TableNames.YarnBBLCProposalChild} C On C.ProposalID = LM.ProposalID
                            WHERE  Proposed = 1 --AND Approve = 1 AND NeedAmendent=1 
							--And YBBLM.RevisionNo <> LM.PreRevisionNo  
							AND LM.PreRevisionNo<>YPRM.RevisionNo --And YPRM.Accept=1
                            AND LM.IsCDA = 'False'
                        ),
                        FinalList AS
                        (
                            SELECT LCID, LCNo,ProposalId,ProposalNo, LCDate, CompanyID, CompanyName,  SupplierID, SupplierName, LCQty, LCValue,
                            LCFilePath, AttachmentPreviewTemplate, ProposeContractNo,--Accept,
                            (CASE WHEN Proposed = 1 AND Approve = 0 THEN 'L/C Opening Request' WHEN Proposed = 1 AND Approve = 1 THEN 'L/C' END) AS LCStatus,
                            YPL.CashStatus,YPL.RetirementModeID,YPL.RetirementMode
							--,PIStatus=(Case When Accept=0 Then 'Waiting For PI Review' When Accept=1 Then 'PI Reviewed' End)--,BBLCStatus 
                            FROM YPL Group BY LCID, LCNo,ProposalId,ProposalNo, LCDate, CompanyID, CompanyName,  SupplierID, SupplierName, LCQty, LCValue,
                            LCFilePath, AttachmentPreviewTemplate, ProposeContractNo,Proposed,YPL.CashStatus,YPL.RetirementModeID,YPL.RetirementMode,Approve--,Accept--,BBLCStatus
                       
                        )
                        SELECT *, Count(*) Over() TotalRows FROM FinalList ";

                orderBy = string.IsNullOrEmpty(orderBy) ? "ORDER BY LCID DESC" : orderBy;
            }
            else if (status == Status.Completed)
            {
                sql += $@"With YPL AS
                        (
                            SELECT LM.LCID, LM.LCNo, LM.ProposalId, YBBLM.ProposalNo, LM.LCDate, LM.LCQty, LM.LCValue, LM.CompanyID, 
	                        CE.ShortName CompanyName, LM.SupplierID, CC.Name As SupplierName, LM.Proposed, LM.Approve, 
                            LM.LCFilePath, LM.AttachmentPreviewTemplate, Coalesce(LC.BBLCNo, CM.ContractNo) ProposeContractNo,
                            LM.CashStatus,YBBLM.RetirementModeID,RetirementMode = ETV.ValueName,LM.PreRevisionNo
                            FROM {TableNames.YarnLCMaster} LM
                            INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = LM.CompanyID
                            Left Join {TableNames.YarnBBLCProposalMaster} YBBLM On YBBLM.ProposalId = LM.ProposalId
                            Left JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = LM.SupplierID
                            Left Join {DbNames.EPYSL}..ContractMAster CM On CM.ContractID = YBBLM.ProposeContractID And YBBLM.CompanyID = 11
                            Left Join {DbNames.EPYSL}..BBLC LC On LC.BBLCID = YBBLM.ProposeContractID And YBBLM.CompanyID != 11 
                            Left Join {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = YBBLM.RetirementModeID
							Left Join {TableNames.YarnLCChild} YLC ON YLC.LCID=LM.LCID
							Left Join {TableNames.YarnPIReceiveMaster} YPRM ON YPRM.YPIReceiveMasterID=YLC.YPIReceiveMasterID
                            WHERE  YBBLM.RevisionNo = LM.RevisionNo And  Proposed = 1 AND Approve = 1 AND LM.IsCDA = 'False'
							AND LM.PreRevisionNo=YPRM.RevisionNo And YPRM.Accept=1
                        ),
                        FinalList AS
                        (
                            SELECT LCID, LCNo,ProposalId,ProposalNo, LCDate, CompanyID, CompanyName,  SupplierID, SupplierName, LCQty, LCValue,
                            LCFilePath, AttachmentPreviewTemplate, ProposeContractNo,
                            (CASE WHEN Proposed = 1 AND Approve = 0 THEN 'L/C Opening Request' WHEN Proposed = 1 AND Approve = 1 THEN 'L/C' END) AS LCStatus,
                            YPL.CashStatus,YPL.RetirementModeID,YPL.RetirementMode
                            FROM YPL 
                        )
                        SELECT *, Count(*) Over() TotalRows FROM FinalList ";

                orderBy = string.IsNullOrEmpty(orderBy) ? "ORDER BY LCID DESC" : orderBy;
            }
            else
            {
                sql += $@"With YPL AS
                        (
                            SELECT LM.LCID, LM.LCNo, LM.ProposalId, YBBLM.ProposalNo, LM.LCDate, LM.LCQty, LM.LCValue, LM.CompanyID, 
	                        CE.ShortName CompanyName, LM.SupplierID, CC.Name As SupplierName, LM.Proposed, LM.Approve, 
                            LM.LCFilePath, LM.AttachmentPreviewTemplate, Coalesce(LC.BBLCNo, CM.ContractNo) ProposeContractNo,
                            LM.CashStatus,YBBLM.RetirementModeID,RetirementMode = ETV.ValueName
                            FROM {TableNames.YarnLCMaster} LM
                            INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = LM.CompanyID
                            Left Join {TableNames.YarnBBLCProposalMaster} YBBLM On YBBLM.ProposalId = LM.ProposalId
                            Left JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = LM.SupplierID
                            Left Join {DbNames.EPYSL}..ContractMAster CM On CM.ContractID = YBBLM.ProposeContractID And YBBLM.CompanyID = 11
                            Left Join {DbNames.EPYSL}..BBLC LC On LC.BBLCID = YBBLM.ProposeContractID And YBBLM.CompanyID != 11  
                            Left Join {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = YBBLM.RetirementModeID
                            WHERE LM.IsCDA = '{isCDAPage}'
                        ),
                        FinalList AS
                        (
                             SELECT	LCID, LCNo,ProposalId,ProposalNo, LCDate, CompanyID, CompanyName, SupplierID, SupplierName, LCQty, LCValue, LCFilePath, AttachmentPreviewTemplate,
                            (CASE WHEN Proposed = 1 AND Approve = 0 THEN 'L/C Opening Request' WHEN Proposed = 1 AND Approve = 1 THEN 'L/C' END) AS LCStatus,
                            ProposeContractNo, YPL.CashStatus,YPL.RetirementModeID,YPL.RetirementMode
                            FROM YPL
                        )
                        SELECT *, Count(*) Over() TotalRows FROM FinalList";

                orderBy = string.IsNullOrEmpty(orderBy) ? "ORDER BY LCID DESC" : orderBy;
            }

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<YarnLcMaster>(sql);
        }

        public async Task<YarnLcMaster> GetNewAsync(int newID)
        {
            var sql = string.Empty;
            sql += $@" 
                ;WITH M AS( 
                    SELECT YBPM.ProposalID, YBPM.ProposalNo, YBPM.RevisionNo,YBPM.ProposalDate, YBPM.YPINo, YBPM.SupplierID, YBPM.CompanyID,
	                Coalesce(BB.BankBranchID, BB2.BankBranchID)IssueBankID, Coalesce(BB.BranchName, BB2.BranchName)IssueBankName,
                    Coalesce(LC.BBLCNo, CM.ContractNo) ProposeContractNo, YBPM.CashStatus, YBPM.ProposeBankID,RetirementMode = ETV.ValueName  
	                FROM {TableNames.YarnBBLCProposalMaster} YBPM 
	                Left Join {DbNames.EPYSL}..BBLC LC On LC.BBLCID = YBPM.ProposeContractID And YBPM.CompanyID != 11
	                Left Join {DbNames.EPYSL}..BankBranch BB On BB.BankBranchID = LC.BankBranchID
	                ----
	                Left Join {DbNames.EPYSL}..ContractMAster CM On CM.ContractID = YBPM.ProposeContractID And YBPM.CompanyID = 11
	                Left Join {DbNames.EPYSL}..BankBranch BB2 On BB2.BankBranchID = CM.PaymentBankID
                    Left Join {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = YBPM.RetirementModeID
	                WHERE YBPM.ProposalID = {newID}
                ),
				C AS (
					SELECT CC.ProposalID, CC.YPIReceiveMasterID
					FROM {TableNames.YarnBBLCProposalChild} CC
					INNER JOIN {TableNames.YarnPIReceiveChild} CH ON CH.YPIReceiveMasterID = CC.YPIReceiveMasterID
					WHERE ProposalID = {newID} GROUP BY CC.ProposalID, CC.YPIReceiveMasterID
				),
                D AS 
                (
	                SELECT CC.ProposalID, SUM(CH.PIQty) LCQty, SUM(CH.PIValue) LCValue
	                FROM {TableNames.YarnBBLCProposalChild} CC INNER JOIN {TableNames.YarnPIReceiveChild} CH ON CH.YPIReceiveMasterID = CC.YPIReceiveMasterID 
	                WHERE ProposalID = {newID} GROUP BY CC.ProposalID
                )
                SELECT M.ProposalID, M.ProposalNo, M.RevisionNo,M.ProposalDate, M.SupplierID, CC.[Name] SupplierName, PIM.CurrencyID, M.IssueBankID, M.IssueBankName,
                M.ProposeContractNo, PIM.PaymentTermsID As PaymentModeID, PIM.ShippingTolerance As Tolerance, PIM.CalculationofTenure As CalculationOfTenorID,
                PIM.IncoTermsID, D.LCQty, D.LCValue, M.CompanyID, CE.ShortName As CompanyName, PIM.PortofDischargeID AS DischargePortID, 
                PIM.PortofLoadingID AS LoadingPortID, PIM.TenureofLC AS TenureofLCID, M.CashStatus,M.RetirementMode
                FROM M
				INNER JOIN C ON C.ProposalID = M.ProposalID
                INNER JOIN D ON D.ProposalID = M.ProposalID
                INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
                INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = M.CompanyID
				LEFT JOIN {TableNames.YarnPIReceiveMaster} PIM ON PIM.YPIReceiveMasterID = C.YPIReceiveMasterID
				GROUP BY M.ProposalID, M.ProposalNo, M.RevisionNo,M.ProposalDate, M.SupplierID, CC.[Name], PIM.CurrencyID, M.IssueBankID, M.IssueBankName,
                M.ProposeContractNo, PIM.PaymentTermsID, PIM.ShippingTolerance, PIM.CalculationofTenure, PIM.IncoTermsID, D.LCQty, D.LCValue, M.CompanyID, 
                CE.ShortName, PIM.PortofDischargeID, PIM.PortofLoadingID, PIM.TenureofLC, M.CashStatus, M.ProposeBankID,M.RetirementMode;

                -- Supplier List
                ;{CommonQueries.GetYarnSuppliers()}; 

                --CustomerList
                ;SELECT CAST(CompanyID AS VARCHAR) AS id, CompanyName AS text
                FROM {DbNames.EPYSL}..CompanyEntity
                WHERE CompanyID IN(8,6,11)
                ORDER BY CompanyName; 

                --IncoTermsList
                ;SELECT CAST(IncoTermsID AS VARCHAR) AS id, IncoTermsName AS text
                FROM {DbNames.EPYSL}..IncoTerms
                WHERE IncoTermsID > 0
                ORDER BY IncoTermsID;

                --PaymentTermsList
                ;SELECT CAST(PaymentTermsID AS VARCHAR) AS id, PaymentTermsName AS text
                FROM {DbNames.EPYSL}..PaymentTrems
                WHERE PaymentTermsID > 0
                ORDER BY PaymentTermsID;;

                --LCTenureList
                ;SELECT CAST(ValueID AS VARCHAR) id, ValueName text
                FROM {DbNames.EPYSL}..EntityTypeValue
                WHERE EntityTypeID = {EntityTypeConstants.LC_TENURE} AND ValueName <> 'Select'
                ORDER BY ValueName;

                --CurrencyTypeList
                ;SELECT CAST(CurrencyID AS VARCHAR) AS id, CurrencyCode AS text
                FROM {DbNames.EPYSL}..Currency
                WHERE CurrencyID = 2
                ORDER BY CurrencyID DESC;

                --LCTypeList
                ;SELECT CAST(PaymentMethodID AS VARCHAR) AS id, PaymentMethodName AS text
                FROM {DbNames.EPYSL}..PaymentMethod
                WHERE PaymentMethodID > 0
                ORDER BY PaymentMethodID;

                --YarnUnitList
                ;SELECT CAST(UnitID AS VARCHAR) AS id, DisplayUnitDesc AS text
                FROM {DbNames.EPYSL}..Unit
                WHERE UnitID IN(28);

                /*--LCIssuingBankList
                ;SELECT CAST(CompanyBankID AS VARCHAR) id, BranchName text FROM {DbNames.EPYSL}..CompanyBank CB
                INNER JOIN {DbNames.EPYSL}..BankBranch BB ON BB.BankBranchID = CB.BankBranchID
                INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = CB.CompanyID; */
    
                /*--LCIssuingBankList
                ;SELECT CAST(CompanyBankID AS VARCHAR) id, BranchName text 
                FROM {DbNames.EPYSL}..CompanyBank CB
                INNER JOIN {DbNames.EPYSL}..BankBranch BB ON BB.BankBranchID = CB.BankBranchID
                INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = CB.CompanyID;
                */

                --LCIssuingBankList
                SELECT CAST(BB.BankBranchID AS VARCHAR) id, BB.BranchName text
                FROM {DbNames.EPYSL}..BankBranch BB  
                Group by BB.BankBranchID, BB.BranchName; 

                --LienBankList
                ;SELECT CAST(BB.BankBranchID AS VARCHAR) id, BranchName text 
                FROM {DbNames.EPYSL}..BankBranch BB
                INNER JOIN {DbNames.EPYSL}..BankBranchPaymentType BBPT ON BBPT.BankBranchID = BB.BankBranchID
                INNER JOIN {DbNames.EPYSL}..BankPaymentType_HK BPT ON BPT.PaymentTypeID = BBPT.PaymentTypeID
                WHERE BPT.PaymentTypeID = 4;

                --PaymentBankList
                ;SELECT CAST(BB.BankBranchID AS VARCHAR) id, BranchName text 
                FROM {DbNames.EPYSL}..BankBranch BB
                INNER JOIN {DbNames.EPYSL}..BankBranchPaymentType BBPT ON BBPT.BankBranchID = BB.BankBranchID
                INNER JOIN {DbNames.EPYSL}..BankPaymentType_HK BPT ON BPT.PaymentTypeID = BBPT.PaymentTypeID
                WHERE BPT.PaymentTypeID = 5;

                --BankAcceptanceFromList
                ;SELECT CAST(ValueID AS VARCHAR) id, ValueName text
                FROM {DbNames.EPYSL}..EntityTypeValue
                WHERE EntityTypeID = {EntityTypeConstants.BANK_ACCEPTANCE_FROM} AND ValueName <> 'Select'
                ORDER BY ValueName;

                --MaturityCalculationList
                ;SELECT CAST(ValueID AS VARCHAR) id, ValueName text
                FROM {DbNames.EPYSL}..EntityTypeValue
                WHERE EntityTypeID = {EntityTypeConstants.MATURITY_CALCULATION} AND ValueName <> 'Select'
                ORDER BY ValueName;

                --CIDeclarationList
                ;SELECT CAST(ValueID AS VARCHAR) id, ValueName text
                FROM {DbNames.EPYSL}..EntityTypeValue
                WHERE EntityTypeID = {EntityTypeConstants.CI_DECLARATION} AND ValueName <> 'Select'
                ORDER BY ValueName;

                --BIDeclarationList
                ;SELECT CAST(ValueID AS VARCHAR) id, ValueName text
                FROM {DbNames.EPYSL}..EntityTypeValue
                WHERE EntityTypeID = {EntityTypeConstants.BI_DECLARATION} AND ValueName <> 'Select'
                ORDER BY ValueName;

                --CommercialAttachmentList
                ;SELECT CAST(CDTypeID AS VARCHAR) id, CDTypeName text FROM {DbNames.EPYSL}..CommercialDocument
                WHERE CDTypeID IN(8,9,10,11,13);

                --LcChilds
                --;SELECT	CC.ChildID, CC.ProposalID, CC.YPIReceiveMasterID, YPIM.SupplierID, C.[Name] AS SupplierName, YPIM.YPINo PINo,
                --YPIM.PIDate, CC.RevisionNo,SUM(YPIC.PIQty) AS TotalQty, SUM(YPIC.PIValue) AS TotalValue, YPIM.PIFilePath
                --FROM {TableNames.YarnBBLCProposalChild} CC
                --INNER JOIN {TableNames.YarnPIReceiveMaster} YPIM ON YPIM.YPIReceiveMasterID = CC.YPIReceiveMasterID
                --INNER JOIN {TableNames.YarnPIReceiveChild} YPIC ON YPIC.YPIReceiveMasterID = CC.YPIReceiveMasterID
                --INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = YPIM.SupplierID
                --WHERE CC.ProposalID = {newID}
                --GROUP BY CC.ChildID, CC.ProposalID, CC.YPIReceiveMasterID, YPIM.SupplierID, C.[Name], YPIM.YPINo, CC.RevisionNo,YPIM.PIDate, YPIM.PIFilePath;
                
                ;SELECT	CC.ChildID, CC.ProposalID, CC.YPIReceiveMasterID, YPIM.SupplierID, C.[Name] AS SupplierName, YPIM.YPINo PINo,
                YPIM.PIDate, CC.RevisionNo,SUM(YPIC.PIQty) AS TotalQty, 
				YPIM.NetPIValue AS TotalValue, --SUM(YPIC.PIValue) AS TotalValue, 
				YPIM.PIFilePath
                FROM {TableNames.YarnBBLCProposalChild} CC
                INNER JOIN {TableNames.YarnPIReceiveMaster} YPIM ON YPIM.YPIReceiveMasterID = CC.YPIReceiveMasterID
                INNER JOIN {TableNames.YarnPIReceiveChild} YPIC ON YPIC.YPIReceiveMasterID = CC.YPIReceiveMasterID
                INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = YPIM.SupplierID
                WHERE CC.ProposalID = {newID}
                GROUP BY CC.ChildID, CC.ProposalID, CC.YPIReceiveMasterID, YPIM.SupplierID, C.[Name], YPIM.YPINo, CC.RevisionNo,YPIM.PIDate,YPIM.NetPIValue, YPIM.PIFilePath;

                --FTT/FDD List
                ;SELECT CAST(ValueID AS VARCHAR) id, ValueName text
                FROM {DbNames.EPYSL}..EntityTypeValue
                WHERE EntityTypeID = {EntityTypeConstants.TRANSPORT_AGENCY} AND ValueName <> 'Select'
                ORDER BY ValueName;

                --AvailableWith List
                ;SELECT CAST(ValueID AS VARCHAR) id, ValueName text
                FROM {DbNames.EPYSL}..EntityTypeValue
                WHERE EntityTypeID = {EntityTypeConstants.AVAILABLE_WITH} AND ValueName <> 'Select'
                ORDER BY ValueName;

                --CalculateTenure List
                ;SELECT CAST(ValueID AS VARCHAR) id, ValueName text
                FROM {DbNames.EPYSL}..EntityTypeValue
                WHERE EntityTypeID = {EntityTypeConstants.CALCULATION_OF_TENURE} AND ValueName <> 'Select'
                ORDER BY ValueName;

                --PartialShipment List
                ;SELECT CAST(ValueID AS VARCHAR) id, ValueName text
                FROM {DbNames.EPYSL}..EntityTypeValue
                WHERE EntityTypeID = {EntityTypeConstants.PARTIAL_SHIPMENT} AND ValueName <> 'Select'
                ORDER BY ValueName;

                --Transshipment List
                ;SELECT CAST(ValueID AS VARCHAR) id, ValueName text
                FROM {DbNames.EPYSL}..EntityTypeValue
                WHERE EntityTypeID = {EntityTypeConstants.TRANS_SHIPMENT_ALLOW} AND ValueName <> 'Select'
                ORDER BY ValueName;

                --LoadingPort List
                ;SELECT CAST(ValueID AS VARCHAR) id, ValueName text
                FROM {DbNames.EPYSL}..EntityTypeValue
                WHERE EntityTypeID = {EntityTypeConstants.PORT_OF_LOADING} AND ValueName <> 'Select'
                ORDER BY ValueName;

                --DischargePort List
                ;SELECT CAST(ValueID AS VARCHAR) id, ValueName text
                FROM {DbNames.EPYSL}..EntityTypeValue
                WHERE EntityTypeID = {EntityTypeConstants.PORT_OF_DISCHARGE} AND ValueName <> 'Select'
                ORDER BY ValueName;";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnLcMaster data = records.Read<YarnLcMaster>().FirstOrDefault();
                //data.CustomerList = records.Read<Select2OptionModel>().ToList(); // Supplier data load in the customer list field
                //data.RecCustomerList = records.Read<Select2OptionModel>().ToList(); //data.CustomerList;
                data.SupplierList = records.Read<Select2OptionModel>().ToList();
                data.CompanyList = records.Read<Select2OptionModel>().ToList(); //data.CompanyList;
                data.IncoTermsList = records.Read<Select2OptionModel>().ToList();
                data.PaymentTermsList = records.Read<Select2OptionModel>().ToList();
                data.TenureofLCList = records.Read<Select2OptionModel>().ToList();
                data.CurrencyList = records.Read<Select2OptionModel>().ToList();
                data.PaymentModeList = records.Read<Select2OptionModel>().ToList();
                data.LCUnitList = records.Read<Select2OptionModel>().ToList();
                data.IssueBankList = records.Read<Select2OptionModel>().ToList();
                data.LienBankList = records.Read<Select2OptionModel>().ToList();
                //data.IssueBankList = data.LienBankList;
                data.PaymentBankList = records.Read<Select2OptionModel>().ToList();
                data.BankAcceptanceFromList = records.Read<Select2OptionModel>().ToList();
                data.MaturityCalculationList = records.Read<Select2OptionModel>().ToList();
                data.CIDecList = records.Read<Select2OptionModel>().ToList();
                data.BCDecList = records.Read<Select2OptionModel>().ToList();
                data.CommercialAttachmentList = records.Read<Select2OptionModel>().ToList();
                //data.LcDocuments = records.Read<YarnLcDocument>().ToList();
                data.LcChilds = records.Read<YarnLcChild>().ToList();
                //data.YarnPoChilds = records.Read<Select2OptionModel>().ToList();
                data.TTTypeList = records.Read<Select2OptionModel>().ToList();
                data.AvailableWithList = records.Read<Select2OptionModel>().ToList();
                data.CalculationOfTenorList = records.Read<Select2OptionModel>().ToList();
                data.PartialShipmentList = records.Read<Select2OptionModel>().ToList();
                data.TransshipmentList = records.Read<Select2OptionModel>().ToList();
                data.LoadingPortList = records.Read<Select2OptionModel>().ToList();
                data.DischargePortList = records.Read<Select2OptionModel>().ToList();
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
        public async Task<YarnLcMaster> GetAsync(int id)
        {
            var sql = string.Empty;
            string removeRevisedRow = "";
            if (id == 1410)
            {
                removeRevisedRow = " AND LC.ChildID <> 2018 ";
            }

            sql += $@"
                ;WITH M AS(
	                SELECT LC.LCID, LC.LCNo, LC.LCDate, LC.LCExpiryDate, LC.ProposalID, LC.RevisionNo, LC.CompanyID, 
	                LC.CurrencyID, LC.LCValue, LC.LCQty, LC.LCUnit, LC.LCReceiveDate, LC.IssueBankID, LC.PaymentBankID, 
	                LC.LienBankID, LC.NotifyPaymentBank, LC.BBReportingNumber, LC.PaymentModeID, LC.CalculationOfTenorID, 
	                LC.DocPresentationDays, LC.BankAcceptanceFrom, LC.MaturityCalculationID, LC.Tolerance, LC.IncoTermsID, LC.CIDecID,
	                LC.BCDecID, LC.HSCode, LC.Proposed, LC.ProposedDate, LC.LCFilePath, LC.AttachmentPreviewTemplate, 
	                LC.SupplierID, LC.BankRefNo, LC.AccountNo, LC.FormOfDC,LC.TTTypeID, LC.AvailableWithID, LC.TenureofLCID,
	                LC.PartialShipmentID, LC.TransshipmentID, LC.LoadingPortID, LC.DischargePortID, LC.IsConInsWith, LC.CashStatus
	                FROM {TableNames.YarnLCMaster} LC
	                WHERE LC.LCID = {id}
                ),
				C AS (
					SELECT CC.LCID, SUM(CH.PIQty) LCQty, SUM(CH.PIValue) LCValue
					FROM {TableNames.YarnLCChild} CC
					INNER JOIN {TableNames.YarnPIReceiveChild} CH ON CH.YPIReceiveMasterID = CC.YPIReceiveMasterID
					WHERE CC.LCID = {id} GROUP BY CC.LCID
				)
                SELECT M.LCID, M.LCNo, M.LCDate, M.LCExpiryDate, M.ProposalID, M.RevisionNo, PP.RevisionNo BBLCRevisionNo,M.CompanyID, 
                M.CurrencyID, M.LCValue, M.LCQty, M.LCUnit, M.LCReceiveDate, M.IssueBankID, M.PaymentBankID, M.LienBankID, 
                M.NotifyPaymentBank, M.BBReportingNumber, M.PaymentModeID, M.CalculationOfTenorID, M.DocPresentationDays, 
                M.BankAcceptanceFrom, M.MaturityCalculationID, M.Tolerance, M.IncoTermsID, M.CIDecID, M.BCDecID, M.HSCode, 
                M.Proposed, M.ProposedDate, M.LCFilePath, M.AttachmentPreviewTemplate, M.SupplierID, PP.ProposalNo, PP.ProposalDate, 
                CU.CompanyName, M.BankRefNo, M.AccountNo, M.FormOfDC, M.TTTypeID, M.AvailableWithID, M.TenureofLCID, 
                M.PartialShipmentID, M.TransshipmentID, M.LoadingPortID, M.DischargePortID, M.IsConInsWith,
                Coalesce(LC.BBLCNo, CM.ContractNo) ProposeContractNo, M.CashStatus,RetirementMode = ETV.ValueName 
                FROM M
				INNER JOIN C ON C.LCID = M.LCID
                INNER JOIN {TableNames.YarnBBLCProposalMaster} PP ON PP.ProposalID = M.ProposalId
				INNER JOIN {DbNames.EPYSL}..CompanyEntity CU ON CU.CompanyID = M.CompanyID
                Left Join {DbNames.EPYSL}..BBLC LC On LC.BBLCID = PP.ProposeContractID And PP.CompanyID != 11
                Left Join {DbNames.EPYSL}..ContractMAster CM On CM.ContractID = PP.ProposeContractID And PP.CompanyID = 11 
                Left Join {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = PP.RetirementModeID
				GROUP BY M.LCID, M.LCNo, M.LCDate, M.LCExpiryDate, M.ProposalID, M.RevisionNo, PP.RevisionNo,M.CompanyID, 
                M.CurrencyID, M.LCValue, M.LCQty, M.LCUnit, M.LCReceiveDate, M.IssueBankID, M.PaymentBankID, M.LienBankID, 
                M.NotifyPaymentBank, M.BBReportingNumber, M.PaymentModeID, M.CalculationOfTenorID, M.DocPresentationDays, 
                M.BankAcceptanceFrom, M.MaturityCalculationID, M.Tolerance, M.IncoTermsID, M.CIDecID, M.BCDecID, M.HSCode, 
                M.Proposed, M.ProposedDate, M.LCFilePath, M.AttachmentPreviewTemplate, M.SupplierID, PP.ProposalNo, PP.ProposalDate, 
                CU.CompanyName, M.BankRefNo, M.AccountNo, M.FormOfDC, M.TTTypeID, M.AvailableWithID, M.TenureofLCID, 
                M.PartialShipmentID, M.TransshipmentID, M.LoadingPortID, M.DischargePortID, M.IsConInsWith, 
                Coalesce(LC.BBLCNo, CM.ContractNo), M.CashStatus,ETV.ValueName ;

                 -- Supplier List
                ;{CommonQueries.GetYarnSuppliers()};

                --CustomerList
                ;SELECT CAST(CompanyID AS VARCHAR) AS id, CompanyName AS text
                FROM {DbNames.EPYSL}..CompanyEntity
                WHERE CompanyID IN(8,6,11)
                ORDER BY CompanyName; 

                --IncoTermsList
                ;SELECT CAST(IncoTermsID AS VARCHAR) AS id, IncoTermsName AS text
                FROM {DbNames.EPYSL}..IncoTerms
                WHERE IncoTermsID > 0
                ORDER BY IncoTermsID;

                --PaymentTermsList
                ;SELECT CAST(PaymentTermsID AS VARCHAR) AS id, PaymentTermsName AS text
                FROM {DbNames.EPYSL}..PaymentTrems
                WHERE PaymentTermsID > 0
                ORDER BY PaymentTermsID;;

                --LCTenureList
                ;SELECT CAST(ValueID AS VARCHAR) id, ValueName text
                FROM {DbNames.EPYSL}..EntityTypeValue
                WHERE EntityTypeID = {EntityTypeConstants.LC_TENURE} AND ValueName <> 'Select'
                ORDER BY ValueName;

                --CurrencyTypeList
                ;SELECT CAST(CurrencyID AS VARCHAR) AS id, CurrencyCode AS text
                FROM {DbNames.EPYSL}..Currency
                WHERE CurrencyID = 2
                ORDER BY CurrencyID DESC;

                --LCTypeList
                ;SELECT CAST(PaymentMethodID AS VARCHAR) AS id, PaymentMethodName AS text
                FROM {DbNames.EPYSL}..PaymentMethod
                WHERE PaymentMethodID > 0
                ORDER BY PaymentMethodID;

                --YarnUnitList
                ;SELECT CAST(UnitID AS VARCHAR) AS id, DisplayUnitDesc AS text
                FROM {DbNames.EPYSL}..Unit
                WHERE UnitID IN(28);

                /*--LCIssuingBankList
                ;SELECT CAST(CompanyBankID AS VARCHAR) id, BranchName text FROM {DbNames.EPYSL}..CompanyBank CB
                INNER JOIN {DbNames.EPYSL}..BankBranch BB ON BB.BankBranchID = CB.BankBranchID
                INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = CB.CompanyID;*/

                --LCIssuingBankList
                SELECT CAST(BB.BankBranchID AS VARCHAR) id, BB.BranchName text
                FROM {DbNames.EPYSL}..BankBranch BB  
                Group by BB.BankBranchID, BB.BranchName; 

                --LienBankList
                ;SELECT CAST(BB.BankBranchID AS VARCHAR) id, BranchName text FROM {DbNames.EPYSL}..BankBranch BB
                INNER JOIN {DbNames.EPYSL}..BankBranchPaymentType BBPT ON BBPT.BankBranchID = BB.BankBranchID
                INNER JOIN {DbNames.EPYSL}..BankPaymentType_HK BPT ON BPT.PaymentTypeID = BBPT.PaymentTypeID
                WHERE BPT.PaymentTypeID = 4;

                --PaymentBankList
                ;SELECT CAST(BB.BankBranchID AS VARCHAR) id, BranchName text FROM {DbNames.EPYSL}..BankBranch BB
                INNER JOIN {DbNames.EPYSL}..BankBranchPaymentType BBPT ON BBPT.BankBranchID = BB.BankBranchID
                INNER JOIN {DbNames.EPYSL}..BankPaymentType_HK BPT ON BPT.PaymentTypeID = BBPT.PaymentTypeID
                WHERE BPT.PaymentTypeID = 5;

                --BankAcceptanceFromList
                ;SELECT CAST(ValueID AS VARCHAR) id, ValueName text
                FROM {DbNames.EPYSL}..EntityTypeValue
                WHERE EntityTypeID = {EntityTypeConstants.BANK_ACCEPTANCE_FROM} AND ValueName <> 'Select'
                ORDER BY ValueName;

                --MaturityCalculationList
                ;SELECT CAST(ValueID AS VARCHAR) id, ValueName text
                FROM {DbNames.EPYSL}..EntityTypeValue
                WHERE EntityTypeID = {EntityTypeConstants.MATURITY_CALCULATION} AND ValueName <> 'Select'
                ORDER BY ValueName;

                --CIDeclarationList
                ;SELECT CAST(ValueID AS VARCHAR) id, ValueName text
                FROM {DbNames.EPYSL}..EntityTypeValue
                WHERE EntityTypeID = {EntityTypeConstants.CI_DECLARATION} AND ValueName <> 'Select'
                ORDER BY ValueName;

                --BIDeclarationList
                ;SELECT CAST(ValueID AS VARCHAR) id, ValueName text
                FROM {DbNames.EPYSL}..EntityTypeValue
                WHERE EntityTypeID = {EntityTypeConstants.BI_DECLARATION} AND ValueName <> 'Select'
                ORDER BY ValueName;

                --CommercialAttachmentList
                ;SELECT CAST(CDTypeID AS VARCHAR) id, CDTypeName text FROM {DbNames.EPYSL}..CommercialDocument
                WHERE CDTypeID IN(8,9,10,11,13);

                --LcChilds
                ;SELECT	    LC.ChildID, LC.LCID, LC.YPIReceiveMasterID, YPIM.SupplierID, C.Name AS SupplierName, YPIM.YPINo PINo,
                CC.RevisionNo,YPIM.PIDate, SUM(YPIC.PIQty) AS TotalQty,
                YPIM.NetPIValue As TotalValue,--SUM(YPIC.PIValue) AS TotalValue,
                YPIM.PIFilePath
                ,case when CC.RevisionNo = YPIM.RevisionNo Then 'BBLC Revised' Else 'BBLC Not Revised' END as BBLCStatus,
                PIStatus=(Case When YPIM.Accept=0 Then 'Waiting For PI Review' When Accept=1 Then 'PI Reviewed' End)
                FROM        {TableNames.YarnLCChild} LC
                INNER JOIN  {TableNames.YarnLCMaster} LM ON LM.LCID = LC.LCID
                INNER JOIN  {TableNames.YarnPIReceiveMaster} YPIM ON YPIM.YPIReceiveMasterID = LC.YPIReceiveMasterID
                INNER JOIN  {TableNames.YarnPIReceiveChild} YPIC ON YPIC.YPIReceiveMasterID = YPIM.YPIReceiveMasterID
                LEFT JOIN {TableNames.YarnBBLCProposalChild} CC ON CC.YPIReceiveMasterID=LC.YPIReceiveMasterID
                INNER JOIN  {DbNames.EPYSL}..Contacts C ON C.ContactID = YPIM.SupplierID
                WHERE       LC.LCID = {id} {removeRevisedRow}
                GROUP BY    LC.ChildID, LC.LCID, LC.YPIReceiveMasterID, YPIM.YPINo, CC.RevisionNo,YPIM.PIDate,YPIM.NetPIValue, YPIM.SupplierID, C.Name, YPIM.PIFilePath,YPIM.RevisionNo,YPIM.Accept;

                --LcDocuments
                ;SELECT    LCDocID, LD.LCID, DocID, CD.CDTypeName
                FROM        {TableNames.YarnLCDocument} LD
                INNER JOIN  {TableNames.YarnLCMaster} LM ON LM.LCID = LD.LCID
                INNER JOIN  {DbNames.EPYSL}..CommercialDocument CD ON CD.CDTypeID = LD.DocID
                WHERE       LD.LCID = {id};

                 --FTT/FDD List
                ;SELECT CAST(ValueID AS VARCHAR) id, ValueName text
                FROM {DbNames.EPYSL}..EntityTypeValue
                WHERE EntityTypeID = {EntityTypeConstants.TRANSPORT_AGENCY} AND ValueName <> 'Select'
                ORDER BY ValueName;

                --AvailableWith List
                ;SELECT CAST(ValueID AS VARCHAR) id, ValueName text
                FROM {DbNames.EPYSL}..EntityTypeValue
                WHERE EntityTypeID = {EntityTypeConstants.AVAILABLE_WITH} AND ValueName <> 'Select'
                ORDER BY ValueName;

                --CalculateTenure List
                ;SELECT CAST(ValueID AS VARCHAR) id, ValueName text
                FROM {DbNames.EPYSL}..EntityTypeValue
                WHERE EntityTypeID = {EntityTypeConstants.CALCULATION_OF_TENURE} AND ValueName <> 'Select'
                ORDER BY ValueName;

                --PartialShipment List
                ;SELECT CAST(ValueID AS VARCHAR) id, ValueName text
                FROM {DbNames.EPYSL}..EntityTypeValue
                WHERE EntityTypeID = {EntityTypeConstants.PARTIAL_SHIPMENT} AND ValueName <> 'Select'
                ORDER BY ValueName;

                --Transshipment List
                ;SELECT CAST(ValueID AS VARCHAR) id, ValueName text
                FROM {DbNames.EPYSL}..EntityTypeValue
                WHERE EntityTypeID = {EntityTypeConstants.TRANS_SHIPMENT_ALLOW} AND ValueName <> 'Select'
                ORDER BY ValueName;

                --LoadingPort List
                ;SELECT CAST(ValueID AS VARCHAR) id, ValueName text
                FROM {DbNames.EPYSL}..EntityTypeValue
                WHERE EntityTypeID = {EntityTypeConstants.PORT_OF_LOADING} AND ValueName <> 'Select'
                ORDER BY ValueName;

                --DischargePort List
                ;SELECT CAST(ValueID AS VARCHAR) id, ValueName text
                FROM {DbNames.EPYSL}..EntityTypeValue
                WHERE EntityTypeID = {EntityTypeConstants.PORT_OF_DISCHARGE} AND ValueName <> 'Select'
                ORDER BY ValueName;";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnLcMaster data = records.Read<YarnLcMaster>().FirstOrDefault();
                //data.CustomerList = records.Read<Select2OptionModel>().ToList(); //Supplier data load in the customer list field
                //data.RecCustomerList = records.Read<Select2OptionModel>().ToList(); //data.CustomerList;
                data.SupplierList = records.Read<Select2OptionModel>().ToList();
                data.CompanyList = records.Read<Select2OptionModel>().ToList();
                data.IncoTermsList = records.Read<Select2OptionModel>().ToList();
                data.PaymentTermsList = records.Read<Select2OptionModel>().ToList();
                data.TenureofLCList = records.Read<Select2OptionModel>().ToList();
                data.CurrencyList = records.Read<Select2OptionModel>().ToList();
                data.PaymentModeList = records.Read<Select2OptionModel>().ToList();
                data.LCUnitList = records.Read<Select2OptionModel>().ToList();
                data.IssueBankList = records.Read<Select2OptionModel>().ToList();
                data.LienBankList = records.Read<Select2OptionModel>().ToList();
                //data.IssueBankList = data.LienBankList;
                data.PaymentBankList = records.Read<Select2OptionModel>().ToList();
                data.BankAcceptanceFromList = records.Read<Select2OptionModel>().ToList();
                data.MaturityCalculationList = records.Read<Select2OptionModel>().ToList();
                data.CIDecList = records.Read<Select2OptionModel>().ToList();
                data.BCDecList = records.Read<Select2OptionModel>().ToList();
                data.CommercialAttachmentList = records.Read<Select2OptionModel>().ToList();
                data.LcChilds = records.Read<YarnLcChild>().ToList();
                data.LcDocuments = records.Read<YarnLcDocument>().ToList();
                //data.YarnPoChilds = records.Read<Select2OptionModel>().ToList();
                data.TTTypeList = records.Read<Select2OptionModel>().ToList();
                data.AvailableWithList = records.Read<Select2OptionModel>().ToList();
                data.CalculationOfTenorList = records.Read<Select2OptionModel>().ToList();
                data.PartialShipmentList = records.Read<Select2OptionModel>().ToList();
                data.TransshipmentList = records.Read<Select2OptionModel>().ToList();
                data.LoadingPortList = records.Read<Select2OptionModel>().ToList();
                data.DischargePortList = records.Read<Select2OptionModel>().ToList();

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

        public async Task<YarnLcMaster> GetAllByIDAsync(int id)
        {
            string sql = $@"
            ;Select * From {TableNames.YarnLCMaster} Where LCID = {id}

            ;Select * From {TableNames.YarnLCChild} Where LCID = {id}

            ;Select * From {TableNames.YarnLCDocument} Where LCID = {id}

            ;select TOP 1 YPRM.RevisionNo As YarnPIRevision
            From {TableNames.YarnPIReceiveMaster} YPRM
            Inner Join {TableNames.YarnLCChild} YLC ON YLC.YPIReceiveMasterID=YPRM.YPIReceiveMasterID
            Where YPRM.YPIReceiveMasterID=(Select TOP 1 YPIReceiveMasterID From {TableNames.YarnLCChild} Where LCID = {id})";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnLcMaster data = records.Read<YarnLcMaster>().FirstOrDefault();
                data.LcChilds = records.Read<YarnLcChild>().ToList();
                data.LcDocuments = records.Read<YarnLcDocument>().ToList();
                data.YarnPIRevision = records.Read<int>().FirstOrDefault();
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

        public async Task SaveAsync(YarnLcMaster entity)
        {
            SqlTransaction transaction = null;
            SqlTransaction transactionGmt = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                await _gmtConnection.OpenAsync();
                transactionGmt = _gmtConnection.BeginTransaction();

                if (entity.isAmendentValue)
                {
                    await _connection.ExecuteAsync("spBackupYarnLCMaster_Full", new { LCID = entity.LCID, UserId = entity.AddedBy }, transaction, 30, CommandType.StoredProcedure);
                }

                //End For Backup When Revise
                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity = await AddAsync(entity, transaction, _connection, transactionGmt, _gmtConnection);
                        break;
                    case EntityState.Modified:
                        entity = await UpdateAsync(entity, transaction, _connection, transactionGmt, _gmtConnection);
                        break;
                    default:
                        break;
                }

                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.LcChilds, transaction);
                await _service.SaveAsync(entity.LcDocuments, transaction);

                transaction.Commit();
                transactionGmt.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                if (transactionGmt != null) transactionGmt.Rollback();
                throw ex;
            }
            finally
            {
                _connection.Close();
                _gmtConnection.Close();
            }
        }

        private async Task<YarnLcMaster> AddAsync(YarnLcMaster entity, SqlTransaction transaction, SqlConnection _connection, SqlTransaction transactionGmt, SqlConnection _gmtConnection)
        {
            entity.LCID = await _service.GetMaxIdAsync(TableNames.YarnLCMaster, RepeatAfterEnum.NoRepeat, transactionGmt, _gmtConnection);
           
            var maxChildId = await _service.GetMaxIdAsync(TableNames.YarnLCChild, entity.LcChilds.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _gmtConnection);
            var maxDocId = await _service.GetMaxIdAsync(TableNames.YarnLCDocument, entity.LcDocuments.Count, RepeatAfterEnum.NoRepeat, transactionGmt, _gmtConnection);

            foreach (var item in entity.LcChilds)
            {
                item.ChildID = maxChildId++;
                item.LCID = entity.LCID;
            }

            foreach (var item in entity.LcDocuments)
            {
                item.LCDocID = maxDocId++;
                item.Lcid = entity.LCID;
            }

            return entity;
        }

        private async Task<YarnLcMaster> UpdateAsync(YarnLcMaster entity, SqlTransaction transaction, SqlConnection _connection, SqlTransaction transactionGmt, SqlConnection _gmtConnection)
        {
        
            var maxChildId = await _service.GetMaxIdAsync(TableNames.YarnLCChild, entity.LcChilds.Where(x => x.EntityState == EntityState.Added).Count(), RepeatAfterEnum.NoRepeat, transactionGmt, _gmtConnection);
            var maxDocId = await _service.GetMaxIdAsync(TableNames.YarnLCDocument, entity.LcDocuments.Where(x => x.EntityState == EntityState.Added).Count(), RepeatAfterEnum.NoRepeat, transactionGmt, _gmtConnection);

            foreach (var item in entity.LcChilds.ToList())
            {
                switch (item.EntityState)
                {
                    case EntityState.Added:
                        item.ChildID = maxChildId++;
                        item.LCID = entity.LCID;
                        break;

                    case EntityState.Modified:
                        item.EntityState = EntityState.Modified;
                        break;

                    case EntityState.Unchanged:
                    case EntityState.Deleted:
                        item.EntityState = EntityState.Deleted;
                        break;

                    default:
                        break;
                }
            }

            foreach (var item in entity.LcDocuments.ToList())
            {
                switch (item.EntityState)
                {
                    case EntityState.Added:
                        item.LCDocID = maxDocId++;
                        item.Lcid = entity.LCID;
                        break;

                    case EntityState.Modified:
                        item.EntityState = EntityState.Modified;
                        break;

                    case EntityState.Unchanged:
                    case EntityState.Deleted:
                        item.EntityState = EntityState.Deleted;
                        break;

                    default:
                        break;
                }
            }

            return entity;
        }

    }
}
