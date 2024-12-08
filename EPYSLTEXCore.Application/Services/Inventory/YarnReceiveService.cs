using Dapper;
using EPYSLTEX.Core.Interfaces.Repositories;
using EPYSLTEX.Core.Interfaces.Services;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Application.Services
{
    public class YarnReceiveService : IYarnReceiveService
    {
        private readonly IDapperCRUDService<YarnReceiveMaster> _service;
        private readonly SqlConnection _connection;

        public YarnReceiveService(IDapperCRUDService<YarnReceiveMaster> service)
        {
            _service = service;
            _connection = service.Connection;
        }
        public async Task<List<YarnReceiveMaster>> GetPagedAsync(Status status, bool isCDAPage, PaginationInfo paginationInfo)
        {
            string tempGuid = CommonFunction.GetNewGuid();
            bool isUsedTemp = false;

            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By ReceiveID Desc" : paginationInfo.OrderBy;
            var sql = string.Empty;
            if (status == Status.PendingReceiveCI)
            {
                sql += $@";
                WITH M AS 
                (
	                SELECT CIM.CIID, CIM.CINo, CIM.CIDate, CIM.CIValue, CIM.LCID, CIM.SupplierID, CIM.CompanyID
                    FROM {TableNames.YARN_CI_MASTER} CIM 
                    LEFT JOIN {TableNames.YARN_RECEIVE_MASTER} A On A.CIID = CIM.CIID
                    WHERE A.CIID IS NULL And CIM.Acceptance = 1 AND CIM.IsComplete = 0 AND CIM.IsCDA = '{isCDAPage}'
                    Group By CIM.CIID, CIM.CINo, CIM.CIDate, CIM.CIValue, CIM.LCID, CIM.SupplierID, CIM.CompanyID
                ),
                YLM As 
                (
                    SELECT M.CIID, M.CINo InvoiceNo, M.CIDate InvoiceDate, M.CIValue InvoiceValue, M.LCID, LM.LCNo, LM.LCDate, LM.LCQty, LM.LCValue, LM.CompanyID, M.SupplierID, CC.[Name] SupplierName,
                    M.CompanyID RCompanyId, M.CompanyID OCompanyId, CE.CompanyName CustomerName, COM.ShortName RCompany
                    FROM M
                    INNER JOIN {TableNames.YarnLCMaster} YarnLCMaster LM ON LM.LCID = M.LCID
                    INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = LM.CompanyID
			        LEFT JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
			        LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = M.CompanyID
                )
                Select *, Count(*) Over() TotalRows From YLM ";

                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By YLM.CIID Desc" : paginationInfo.OrderBy;
            }
            else if (status == Status.PendingReceivePO)
            {
                if (!isCDAPage)
                {
                    sql += $@"
                    WITH GT As(
	                    Select YPOMasterID, ISNULL(QuotationRefNo,'') QuotationRefNo
	                    From {TableNames.YarnPOChild}
	                    Where ReceivedCompleted = 0
	                    Group By YPOMasterID, QuotationRefNo 
                    ), R As(
	                    Select YPOMasterID,
	                        STUFF(
	                        (
		                        SELECT ', ' + CAST(GT.QuotationRefNo As varchar) AS [text()]
		                        FROM GT
		                        WHERE GT.YPOMasterID = FT.YPOMasterID
		                        FOR XML PATH('')
	                        ), 1, 2, '') As QuotationRefNo
	                        From GT FT
	                        Group By YPOMasterID 
                    ),
                    M AS 
                    (
	                    SELECT	PO.YPOMasterID, PO.PONo, PO.PODate, PO.CompanyID, PO.SupplierID, PO.CurrencyID, PO.ShippingTolerance, R.QuotationRefNo
	                    FROM {TableNames.YarnPOMaster} PO 
						Inner Join {DbNames.EPYSL}..ContactAdditionalInfo C ON C.ContactID = PO.SupplierID
                        Inner Join R On R.YPOMasterID = PO.YPOMasterID
                        WHERE PO.Approved = 1 AND ISNULL(PO.IsCancel,0) = 0 AND PO.SubGroupID = 102 --AND C.InLand = 1 And C.EPZ = 0
						Group By PO.YPOMasterID, PO.PONo, PO.PODate, PO.CompanyID, PO.SupplierID, PO.CurrencyID, PO.ShippingTolerance, R.QuotationRefNo
                    ), 
                    L As
                    (
	                    Select M.YPOMasterID, M.PONo, M.PODate, M.CompanyID, M.SupplierID, M.CurrencyID, M.ShippingTolerance, M.QuotationRefNo, PINo = YPIM.YPINo
	                    From M
	                    Left Join {TableNames.YarnPIReceivePO} B On B.YPOMasterID = M.YPOMasterID
	                    Left Join {TableNames.YARN_CI_CHILD_PI} F On F.YPIMasterID = B.YPIReceiveMasterID
                        Left Join {TableNames.YarnPIReceiveMaster}  YPIM On YPIM.YPIReceiveMasterID = B.YPIReceiveMasterID
	                    Where F.YPIMasterID IS NULL
	                    Group By M.YPOMasterID, M.PONo, M.PODate, M.CompanyID, M.SupplierID, M.CurrencyID, M.ShippingTolerance, M.QuotationRefNo, YPIM.YPINo
                    ),
                    LCInfo As (
						Select 
						YPC.YPOMasterID, LCNo = STRING_AGG(YLCM.LCNo,',')

						From {TableNames.YarnBBLCProposalChild} PC
						Inner Join {TableNames.YarnLCMaster} YLCM On YLCM.ProposalID = PC.ProposalID
						Inner Join {TableNames.YarnPIReceiveChild} YPIC On YPIC.YPIReceiveMasterID =PC.YPIReceiveMasterID
						Inner Join {TableNames.YarnPOChild} YPC On YPC.YPOChildID = YPIC.YPOChildID
						Inner Join L On L.YPOMasterID = YPC.YPOMasterID
						Group By YPC.YPOMasterID
					),
                    POPending As 
                    (
                        SELECT LC.LCNo, M.YPOMasterID PoId, M.PONo PoNo, M.PODate PoDate, M.CompanyID RCompanyId, M.SupplierID, M.CurrencyID, M.ShippingTolerance Tolerance, PINo = STRING_AGG(M.PINo,','), 
                        CC.[Name] SupplierName, COM.ShortName RCompany, M.QuotationRefNo
                        FROM L M
                        LEFT JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
                        LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = M.CompanyID
                        Left Join LCInfo LC On LC.YPOMasterID = M.YPOMasterID
	                    GROUP BY M.YPOMasterID, M.PONo, M.PODate, M.CompanyID, M.SupplierID, M.CurrencyID, M.ShippingTolerance, 
                        CC.[Name], COM.ShortName, M.QuotationRefNo,LC.LCNo
                    )
                    Select *, Count(*) Over() TotalRows From POPending M";

                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By M.PoId Desc" : paginationInfo.OrderBy;
                }
                else
                {
                    sql += $@"
                    WITH M AS 
                    (
	                    SELECT	PO.CDAPOMasterID, PO.PONo, PO.PODate, PO.CompanyID, PO.SupplierID, PO.CurrencyID, PO.ShippingTolerance
	                    FROM {TableNames.CDAPOMASTER} PO
                        Inner Join {DbNames.EPYSL}..ContactAdditionalInfo C ON C.ContactID = PO.SupplierID
                        LEFT Join {TableNames.YARN_RECEIVE_MASTER} R On R.POID = PO.CDAPOMasterID
                        WHERE R.POID IS NULL And C.InLand = 1 And C.EPZ = 0 And PO.Approved = 1 
                        Group by PO.CDAPOMasterID, PO.PONo, PO.PODate, PO.CompanyID, PO.SupplierID, PO.CurrencyID, PO.ShippingTolerance
                    ), 
                    L As
                    (
						Select M.CDAPOMasterID, M.PONo, M.PODate, M.CompanyID, M.SupplierID, M.CurrencyID, M.ShippingTolerance
						From M
						Left Join {TableNames.YarnPIReceivePO} B On B.YPOMasterID = M.CDAPOMasterID
						Left Join {TableNames.YARN_CI_CHILD_PI} F On F.YPIMasterID = B.YPIReceiveMasterID
						Where F.YPIMasterID IS NULL
						Group By M.CDAPOMasterID, M.PONo, M.PODate, M.CompanyID, M.SupplierID, M.CurrencyID, M.ShippingTolerance
					),LCInfo As (
						Select 
						YPC.YPOMasterID, LCNo = STRING_AGG(YLCM.LCNo,',')

						From {TableNames.YarnBBLCProposalChild} PC
						Inner Join {TableNames.YarnLCMaster} YLCM On YLCM.ProposalID = PC.ProposalID
						Inner Join {TableNames.YarnPIReceiveChild} YPIC On YPIC.YPIReceiveMasterID =PC.YPIReceiveMasterID
						Inner Join {TableNames.YarnPOChild} YPC On YPC.YPOChildID = YPIC.YPOChildID
						Inner Join L On L.YPOMasterID = YPC.YPOMasterID
						Group By YPC.YPOMasterID
					),
                    POPending As 
                    (
                        SELECT LC.LCNo, M.CDAPOMasterID PoId, M.PONo PoNo, M.PODate PoDate, M.CompanyID RCompanyId, M.SupplierID, 
                        M.CurrencyID, M.ShippingTolerance Tolerance, 
                        CC.[Name] SupplierName, COM.ShortName RCompany
                        FROM L M
                        LEFT JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
                        LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = M.CompanyID
                        Left Join LCInfo LC On LC.YPOMasterID = M.YPOMasterID
                    )
                    Select *, Count(*) Over() TotalRows From POPending M";

                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By M.PoId Desc" : paginationInfo.OrderBy;
                }
            }
            else if (status == Status.Draft)
            {
                sql += $@"With M AS
                (
                    SELECT RM.ReceiveID, RM.ReceiveDate, RM.ReceiveNo, RM.LocationID, RM.RCompanyID, RM.OCompanyID, RM.SupplierID, RM.LCNo, RM.LCDate, RM.Tolerance,
				    RM.InvoiceNo, RM.InvoiceDate, RM.BLNo, RM.BLDate, RM.BankBranchID, RM.PLNo, RM.PLDate, RM.ChallanNo, RM.ChallanDate, RM.GPNo, RM.GPDate, RM.TransportMode,
				    RM.TransportTypeID, RM.CContractorID, RM.VehicalNo, RM.ShipmentStatus, RM.Remarks, RM.SpinnerID, RM.POID, RM.CIID, RM.LCID, RM.ReceivedById, RM.GPTime,
				    RM.PONo, RM.PODate,
                    YarnReceiveType = CASE WHEN RM.IsSampleYarn = 1 THEN 'Sample' ELSE CASE WHEN RM.POID > 0 THEN 'PO' ELSE 'CI' END END
                    FROM {TableNames.YARN_RECEIVE_MASTER} RM 
                    WHERE RM.IsSendForApprove = 0 AND RM.IsApproved = 0 AND RM.IsCDA = '{isCDAPage}'
                ),
                PIWholeList AS
                (
	                SELECT M.POID, YPI.YPINo
	                FROM {TableNames.YarnPIReceivePO} YPIRPO
	                INNER JOIN M ON M.POID = YPIRPO.YPOMasterID
	                LEFT JOIN {TableNames.YarnPIReceiveMaster} YPI ON YPI.YPIReceiveMasterID = YPIRPO.YPIReceiveMasterID
	                GROUP BY M.POID, YPI.YPINo
                ),
                PIList AS
                (
	                SELECT A.POID, YPINo = STRING_AGG(A.YPINo,',')
	                FROM PIWholeList A
	                GROUP BY A.POID
                ),
                FinalList AS
                (
	                SELECT	M.ReceiveID, M.ReceiveDate, M.ReceiveNo, M.LocationID, M.RCompanyID, M.OCompanyID, M.SupplierID, 
	                M.LCNo, M.LCDate, M.Tolerance, M.InvoiceNo, M.InvoiceDate, M.BLNo, M.BLDate, M.BankBranchID, M.PLNo, 
	                M.PLDate, M.ChallanNo, M.ChallanDate, M.GPNo, M.GPDate, M.TransportMode, M.TransportTypeID, M.CContractorID, 
	                M.VehicalNo, M.ShipmentStatus, M.Remarks, M.SpinnerID, M.POID, M.CIID, M.LCID, M.ReceivedById, M.GPTime,
	                CC.[ShortName] SupplierName, CCS.[ShortName] SpinnerName,CCT.[ShortName] TransportAgencyName,
	                LocationName = L.ShortName,BB.BranchName BankBranchName, COM.ShortName RCompany, M.PONo, M.PODate, M.YarnReceiveType,
                    PINo = PIL.YPINo
	                FROM M
	                LEFT JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
	                LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = M.SpinnerID
	                LEFT JOIN {DbNames.EPYSL}..contacts CCT on CCT.ContactID = M.CContractorID
	                Left Join {DbNames.EPYSL}..Location L On L.LocationID=M.LocationID
	                LEFT JOIN {DbNames.EPYSL}..BankBranch BB ON BB.BankBranchID = M.BankBranchID
	                LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = M.RCompanyID
                    LEFT JOIN PIList PIL ON PIL.POID = M.POID
                )
                SELECT *,Count(*) Over() TotalRows FROM FinalList";
            }
            else if (status == Status.ProposedForApproval)
            {
                sql += $@"With M AS
                (
                    SELECT RM.ReceiveID, RM.ReceiveDate, RM.ReceiveNo, RM.LocationID, RM.RCompanyID, RM.OCompanyID, RM.SupplierID, RM.LCNo, RM.LCDate, RM.Tolerance,
				    RM.InvoiceNo, RM.InvoiceDate, RM.BLNo, RM.BLDate, RM.BankBranchID, RM.PLNo, RM.PLDate, RM.ChallanNo, RM.ChallanDate, RM.GPNo, RM.GPDate, RM.TransportMode,
				    RM.TransportTypeID, RM.CContractorID, RM.VehicalNo, RM.ShipmentStatus, RM.Remarks, RM.SpinnerID, RM.POID, RM.CIID, RM.LCID, RM.ReceivedById, RM.GPTime,
				    RM.PONo, RM.PODate,
                    YarnReceiveType = CASE WHEN RM.IsSampleYarn = 1 THEN 'Sample' ELSE CASE WHEN RM.POID > 0 THEN 'PO' ELSE 'CI' END END
                    FROM {TableNames.YARN_RECEIVE_MASTER} RM 
                    WHERE RM.IsSendForApprove = 1 AND RM.IsApproved = 0 AND RM.IsCDA = '{isCDAPage}'
                ),
                PIWholeList AS
                (
	                SELECT M.POID, YPI.YPINo
	                FROM YarnPIReceivePO YPIRPO
	                INNER JOIN M ON M.POID = YPIRPO.YPOMasterID
	                LEFT JOIN {TableNames.YarnPIReceiveMaster} YPI ON YPI.YPIReceiveMasterID = YPIRPO.YPIReceiveMasterID
	                GROUP BY M.POID, YPI.YPINo
                ),
                PIList AS
                (
	                SELECT A.POID, YPINo = STRING_AGG(A.YPINo,',')
	                FROM PIWholeList A
	                GROUP BY A.POID
                ),
                FinalList AS
                (
	                SELECT	M.ReceiveID, M.ReceiveDate, M.ReceiveNo, M.LocationID, M.RCompanyID, M.OCompanyID, M.SupplierID, 
	                M.LCNo, M.LCDate, M.Tolerance, M.InvoiceNo, M.InvoiceDate, M.BLNo, M.BLDate, M.BankBranchID, M.PLNo, 
	                M.PLDate, M.ChallanNo, M.ChallanDate, M.GPNo, M.GPDate, M.TransportMode, M.TransportTypeID, M.CContractorID, 
	                M.VehicalNo, M.ShipmentStatus, M.Remarks, M.SpinnerID, M.POID, M.CIID, M.LCID, M.ReceivedById, M.GPTime,
	                CC.[ShortName] SupplierName, CCS.[ShortName] SpinnerName,CCT.[ShortName] TransportAgencyName,
	                LocationName = L.ShortName,BB.BranchName BankBranchName, COM.ShortName RCompany, M.PONo, M.PODate, M.YarnReceiveType,
                    PINo = PIL.YPINo
	                FROM M
	                LEFT JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
	                LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = M.SpinnerID
	                LEFT JOIN {DbNames.EPYSL}..contacts CCT on CCT.ContactID = M.CContractorID
	                Left Join {DbNames.EPYSL}..Location L On L.LocationID=M.LocationID
	                LEFT JOIN {DbNames.EPYSL}..BankBranch BB ON BB.BankBranchID = M.BankBranchID
	                LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = M.RCompanyID
                    LEFT JOIN PIList PIL ON PIL.POID = M.POID
                )
                SELECT *,Count(*) Over() TotalRows FROM FinalList";
            }
            else if (status == Status.Approved)
            {
                sql += $@"With M AS
                (
                    SELECT RM.ReceiveID, RM.ReceiveDate, RM.ReceiveNo, RM.LocationID, RM.RCompanyID, RM.OCompanyID, RM.SupplierID, RM.LCNo, RM.LCDate, RM.Tolerance,
				    RM.InvoiceNo, RM.InvoiceDate, RM.BLNo, RM.BLDate, RM.BankBranchID, RM.PLNo, RM.PLDate, RM.ChallanNo, RM.ChallanDate, RM.GPNo, RM.GPDate, RM.TransportMode,
				    RM.TransportTypeID, RM.CContractorID, RM.VehicalNo, RM.ShipmentStatus, RM.Remarks, RM.SpinnerID, RM.POID, RM.CIID, RM.LCID, RM.ReceivedById, RM.GPTime,
				    RM.PONo, RM.PODate, RM.ApprovedDate, RM.ApprovedBy,
                    YarnReceiveType = CASE WHEN RM.IsSampleYarn = 1 THEN 'Sample' ELSE CASE WHEN RM.POID > 0 THEN 'PO' ELSE 'CI' END END
                    FROM {TableNames.YARN_RECEIVE_MASTER} RM 
                    WHERE RM.IsSendForApprove = 1 AND RM.IsApproved = 1 AND RM.IsCDA = '{isCDAPage}'
                ),
                PIWholeList AS
                (
	                SELECT M.POID, YPI.YPINo
	                FROM YarnPIReceivePO YPIRPO
	                INNER JOIN M ON M.POID = YPIRPO.YPOMasterID
	                LEFT JOIN {TableNames.YarnPIReceiveMaster} YPI ON YPI.YPIReceiveMasterID = YPIRPO.YPIReceiveMasterID
	                GROUP BY M.POID, YPI.YPINo
                ),
                PIList AS
                (
	                SELECT A.POID, YPINo = STRING_AGG(A.YPINo,',')
	                FROM PIWholeList A
	                GROUP BY A.POID
                ),
                FinalList AS
                (
	                SELECT	M.ReceiveID, M.ReceiveDate, M.ReceiveNo, M.LocationID, M.RCompanyID, M.OCompanyID, M.SupplierID, 
	                M.LCNo, M.LCDate, M.Tolerance, M.InvoiceNo, M.InvoiceDate, M.BLNo, M.BLDate, M.BankBranchID, M.PLNo, 
	                M.PLDate, M.ChallanNo, M.ChallanDate, M.GPNo, M.GPDate, M.TransportMode, M.TransportTypeID, M.CContractorID, 
	                M.VehicalNo, M.ShipmentStatus, M.Remarks, M.SpinnerID, M.POID, M.CIID, M.LCID, M.ReceivedById, M.GPTime,
	                CC.[ShortName] SupplierName, CCS.[ShortName] SpinnerName,CCT.[ShortName] TransportAgencyName,
	                LocationName = L.ShortName,BB.BranchName BankBranchName, COM.ShortName RCompany, M.PONo, M.PODate, M.YarnReceiveType,
                    PINo = PIL.YPINo, M.ApprovedDate, ApprovedByName = E.EmployeeName
	                FROM M
	                LEFT JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
	                LEFT JOIN {DbNames.EPYSL}..Contacts CCS ON CCS.ContactID = M.SpinnerID
	                LEFT JOIN {DbNames.EPYSL}..contacts CCT on CCT.ContactID = M.CContractorID
	                Left Join {DbNames.EPYSL}..Location L On L.LocationID=M.LocationID
	                LEFT JOIN {DbNames.EPYSL}..BankBranch BB ON BB.BankBranchID = M.BankBranchID
	                LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = M.RCompanyID
                    LEFT JOIN PIList PIL ON PIL.POID = M.POID

	                LEFT JOIN {DbNames.EPYSL}..LoginUser LU ON LU.UserCode = M.ApprovedBy
	                LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = LU.EmployeeCode
                )
                SELECT * INTO #TempData{tempGuid} FROM FinalList
				SELECT *,Count(*) Over() TotalRows FROM #TempData{tempGuid}";

                isUsedTemp = true;
            }
            else if (status == Status.PendingReceiveSF)
            {
                if (!isCDAPage)
                {
                    sql += $@"
                    WITH GT As(
	                    Select YPOMasterID, ISNULL(QuotationRefNo,'') QuotationRefNo
	                    From {TableNames.YarnPOChild}
	                    Where ReceivedCompleted = 0
	                    Group By YPOMasterID, QuotationRefNo 
                    ), R As(
	                    Select YPOMasterID,
	                        STUFF(
	                        (
		                        SELECT ', ' + CAST(GT.QuotationRefNo As varchar) AS [text()]
		                        FROM GT
		                        WHERE GT.YPOMasterID = FT.YPOMasterID
		                        FOR XML PATH('')
	                        ), 1, 2, '') As QuotationRefNo
	                        From GT FT
	                        Group By YPOMasterID 
                    ),
                    M AS 
                    (
	                    SELECT	PO.YPOMasterID, PO.PONo, PO.PODate, PO.CompanyID, PO.SupplierID, PO.CurrencyID, PO.ShippingTolerance, R.QuotationRefNo
	                    FROM {TableNames.YarnPOMaster} PO 
						Inner Join {DbNames.EPYSL}..ContactAdditionalInfo C ON C.ContactID = PO.SupplierID
                        Inner Join R On R.YPOMasterID = PO.YPOMasterID
                        WHERE PO.Approved = 1 AND PO.SubGroupID = 102 --AND C.InLand = 1 And C.EPZ = 0
						Group By PO.YPOMasterID, PO.PONo, PO.PODate, PO.CompanyID, PO.SupplierID, PO.CurrencyID, PO.ShippingTolerance, R.QuotationRefNo
                    ), 
                    L As
                    (
						Select M.YPOMasterID, M.PONo, M.PODate, M.CompanyID, M.SupplierID, M.CurrencyID, M.ShippingTolerance, M.QuotationRefNo
						From M
						Left Join {TableNames.YarnPIReceivePO} B On B.YPOMasterID = M.YPOMasterID
						Left Join {TableNames.YARN_CI_CHILD_PI} F On F.YPIMasterID = B.YPIReceiveMasterID
						Where F.YPIMasterID IS NULL
						Group By M.YPOMasterID, M.PONo, M.PODate, M.CompanyID, M.SupplierID, M.CurrencyID, M.ShippingTolerance, M.QuotationRefNo
					),
                    LCInfo As (
						Select 
						YPC.YPOMasterID, LCNo = STRING_AGG(YLCM.LCNo,',')

						From {TableNames.YarnBBLCProposalChild} PC
						Inner Join {TableNames.YarnLCMaster} YLCM On YLCM.ProposalID = PC.ProposalID
						Inner Join {TableNames.YarnPIReceiveChild} YPIC On YPIC.YPIReceiveMasterID =PC.YPIReceiveMasterID
						Inner Join {TableNames.YarnPOChild} YPC On YPC.YPOChildID = YPIC.YPOChildID
						Inner Join L On L.YPOMasterID = YPC.YPOMasterID
						Group By YPC.YPOMasterID
					),
                    POPending As 
                    (
                        SELECT  LC.LCNo, M.YPOMasterID PoId, M.PONo PoNo, M.PODate PoDate, M.CompanyID RCompanyId, M.SupplierID, M.CurrencyID, M.ShippingTolerance Tolerance, 
                        CC.[Name] SupplierName, COM.ShortName RCompany, M.QuotationRefNo, Count(*) Over() TotalRows
                        FROM L M
                        LEFT JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
                        LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = M.CompanyID
                        Left Join LCInfo LC On LC.YPOMasterID = M.YPOMasterID
                    )
                    Select * From POPending M";

                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By M.PoId Desc" : paginationInfo.OrderBy;
                }
                else
                {
                    sql += $@"
                    WITH M AS 
                    (
	                    SELECT	PO.CDAPOMasterID, PO.PONo, PO.PODate, PO.CompanyID, PO.SupplierID, PO.CurrencyID, PO.ShippingTolerance
	                    FROM {TableNames.CDAPOMASTER} PO
                        Inner Join {DbNames.EPYSL}..ContactAdditionalInfo C ON C.ContactID = PO.SupplierID
                        LEFT Join {TableNames.YARN_RECEIVE_MASTER} R On R.POID = PO.CDAPOMasterID
                        WHERE R.POID IS NULL And C.InLand = 1 And C.EPZ = 0 And PO.Approved = 1 
                        Group by PO.CDAPOMasterID, PO.PONo, PO.PODate, PO.CompanyID, PO.SupplierID, PO.CurrencyID, PO.ShippingTolerance
                    ), 
                    L As
                    (
						Select M.CDAPOMasterID, M.PONo, M.PODate, M.CompanyID, M.SupplierID, M.CurrencyID, M.ShippingTolerance
						From M
						Left Join {TableNames.YarnPIReceivePO} B On B.YPOMasterID = M.CDAPOMasterID
						Left Join {TableNames.YARN_CI_CHILD_PI} F On F.YPIMasterID = B.YPIReceiveMasterID
						Where F.YPIMasterID IS NULL
						Group By M.CDAPOMasterID, M.PONo, M.PODate, M.CompanyID, M.SupplierID, M.CurrencyID, M.ShippingTolerance
					),
                    LCInfo As (
						Select 
						YPC.YPOMasterID, LCNo = STRING_AGG(YLCM.LCNo,',')

						From {TableNames.YarnBBLCProposalChild} PC
						Inner Join {TableNames.YarnLCMaster} YLCM On YLCM.ProposalID = PC.ProposalID
						Inner Join {TableNames.YarnPIReceiveChild} YPIC On YPIC.YPIReceiveMasterID =PC.YPIReceiveMasterID
						Inner Join {TableNames.YarnPOChild} YPC On YPC.YPOChildID = YPIC.YPOChildID
						Inner Join L On L.YPOMasterID = YPC.YPOMasterID
						Group By YPC.YPOMasterID
					),
                    POPending As 
                    (
                        SELECT LC.LCNo, M.CDAPOMasterID PoId, M.PONo PoNo, M.PODate PoDate, M.CompanyID RCompanyId, M.SupplierID, 
                        M.CurrencyID, M.ShippingTolerance Tolerance, 
                        CC.[Name] SupplierName, COM.ShortName RCompany, Count(*) Over() TotalRows
                        FROM L M
                        LEFT JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
                        LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = M.CompanyID
                        Left Join LCInfo LC On LC.YPOMasterID = M.YPOMasterID
                    )
                    Select * From POPending M";

                    orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By M.PoId Desc" : paginationInfo.OrderBy;
                }
            }

            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            if (isUsedTemp)
            {
                sql += $@" DROP TABLE #TempData{tempGuid}";
            }

            var finalList = await _service.GetDataAsync<YarnReceiveMaster>(sql);
            finalList.ForEach(x =>
            {
                var values = CommonFunction.GetDefaultValueWhenInvalidS(x.LCNo).Split(',');
                x.LCNo = string.Join(",", values.Distinct());

                values = CommonFunction.GetDefaultValueWhenInvalidS(x.PINo).Split(',');
                x.PINo = string.Join(",", values.Distinct());

                values = CommonFunction.GetDefaultValueWhenInvalidS(x.QuotationRefNo).Split(',');
                x.QuotationRefNo = string.Join(",", values.Distinct());
            });
            return finalList;
        }

        public async Task<YarnReceiveMaster> GetNewAsync(int CiId, int PoId, bool isCDAPage)
        {
            var sql = string.Empty;
            if (CiId > 0)
            {
                sql = $@"
                ;WITH M AS (
	                SELECT CI.CIID, CI.CINo, CI.CIDate, CI.LCID, CI.SupplierID, CI.CompanyID
	                FROM {TableNames.YARN_CI_MASTER} CI 
                    WHERE CIID = {CiId}
                )
                SELECT M.CIID, M.CINo InvoiceNo, M.CIDate InvoiceDate, M.LCID, M.SupplierID, LC.LCNo, LC.LCDate, LC.Tolerance, CC.[Name] SupplierName, 
                M.CompanyID RCompanyID, M.CompanyID OCompanyID, COM.ShortName RCompany, LC.LienBankID BankBranchID, BB.BranchName BankBranchName, LC.CurrencyID
                FROM M
                INNER JOIN {TableNames.YarnLCMaster} LC ON LC.LCID = M.LCID
                INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
				INNER JOIN {DbNames.EPYSL}..BankBranch BB ON BB.BankBranchID = LC.LienBankID
                LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = M.CompanyID;

                -----childs
                ;With 
                M AS (
	                SELECT CI.ChildID, CI.CIID, CI.ItemMasterID, CI.ItemDescription, CI.UnitID, CI.NoOfCarton, CI.NoOfCone, CI.GrossWeight, CI.NetWeight, 
	                CI.PIQty, CI.InvoiceQty, CI.Rate, CI.PDValue, CI.PIValue, CI.YarnProgramID, CI.ShadeCode
	                FROM YarnCIChild CI WHERE CIID = {CiId}
                )
                SELECT InvoiceChildID = ROW_NUMBER() OVER (ORDER BY M.CIID, M.ItemMasterID, M.ItemDescription, M.UnitID, M.Rate, M.YarnProgramId, M.ShadeCode), 
                M.CIID, M.ItemMasterID, M.ItemDescription, M.UnitID, M.NoOfCarton NoOfCartoon, M.NoOfCone, 
                M.GrossWeight, M.NetWeight, M.YarnProgramID, PIQty = SUM(M.PIQty), InvoiceQty = SUM(M.InvoiceQty), M.Rate, M.PDValue, M.PIValue, UU.DisplayUnitDesc, 
                ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, 
                ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc, ISV6.SegmentValue ChallanCount, 
                ISV6.SegmentValue PhysicalCount, M.ShadeCode
                FROM M
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = M.ItemMasterID 
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                Inner Join {DbNames.EPYSL}..Unit AS UU On UU.UnitID = m.UnitID
                GROUP BY M.CIID, M.ItemMasterID, M.ItemDescription, M.UnitID, M.NoOfCarton, M.NoOfCone, 
                M.GrossWeight, M.NetWeight, M.YarnProgramID, M.Rate, M.PDValue, M.PIValue, UU.DisplayUnitDesc, 
                ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue, ISV4.SegmentValue, 
                ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue, ISV6.SegmentValue, 
                ISV6.SegmentValue, M.ShadeCode;

                ----TransportTypeList
                {CommonQueries.GetEntityTypesByEntityTypeId(EntityTypeConstants.TRANSPORT_TYPE)};

                ----TransportModeList
                {CommonQueries.GetEntityTypesByEntityTypeId(EntityTypeConstants.TRANSPORT_MODE)};

                ----TransportAgencyList
                {CommonQueries.GetContactsByCategoryType(ContactCategoryNames.CARRYING_CONTRACTOR)}; 
                /* {CommonQueries.GetEntityTypesByEntityTypeId(EntityTypeConstants.TRANSPORT_AGENCY)}; */

                -----ShipmentStatusList
                /* {CommonQueries.GetEntityTypesByEntityTypeId(EntityTypeConstants.SHIPMENT_STATUS)}; */
                SELECT 0 As id, 'Full' text 
                Union
                SELECT 1 As id, 'Partial' text 
                Union
                SELECT 2 As id, 'Last' text 

                -----LocationList
                {CommonQueries.GetLocation()};

                -----SpinnerList
                {CommonQueries.GetContactsByCategoryType(ContactCategoryNames.SPINNER)};
                
                -----SupplierList
                {CommonQueries.GetContactsByCategoryType(ContactCategoryNames.SUPPLIER)};

                -----CompanyList
                Select Cast (CE.CompanyID as varchar) [id] , CE.ShortName [text]
                FROM {DbNames.EPYSL}..CompanyEntity CE;

                --Buyers
                {CommonQueries.GetContactsByCategoryId(ContactCategoryConstants.CONTACT_CATEGORY_BUYER)};

                SELECT id = YarnPackingID, [text] = PackNo, [desc] = SpinnerID, additionalValue = Cone, additionalValue2 = NetWeight
                FROM SpinnerWiseYarnPacking_HK;

                -- POV2 IgnoreValidation
                SELECT * FROM POV2IgnoreValidation;

                --Receive For
                Select Cast (RF.ReceiveForId as varchar) [id] , RF.ReceiveForName [text] FROM ReceiveFor RF;";
            }
            else if (PoId > 0)
            {
                if (!isCDAPage)
                {
                    sql = $@"
                    ;WITH M AS (
	                    SELECT PO.YPOMasterID, PO.PONo, PO.PODate, PO.CompanyID, PO.SupplierID, PO.CurrencyID, PO.ShippingTolerance
	                    , YLCM.LCNo, YLCM.LCDate
	                    FROM {TableNames.YarnPOMaster} PO 
	                    Left Join {TableNames.YarnPIReceivePO} YPIPO On YPIPO.YPOMasterID = PO.YPOMasterID
	                    Left Join {TableNames.YarnBBLCProposalChild} YBBPC On YBBPC.YPIReceiveMasterID = YPIPO.YPIReceiveMasterID
	                    Left Join {TableNames.YarnLCMaster} YLCM ON YLCM.ProposalID = YBBPC.ProposalID
	                    WHERE PO.YPOMasterID = {PoId}
	                    GROUP BY PO.YPOMasterID, PO.PONo, PO.PODate, PO.CompanyID, PO.SupplierID, PO.CurrencyID, PO.ShippingTolerance
	                    , YLCM.LCNo, YLCM.LCDate
                    )
                    SELECT M.YPOMasterID PoId, M.PONo, M.PODate, M.SupplierID, M.CurrencyID, M.ShippingTolerance As Tolerance, CC.[Name] SupplierName, 
                    M.CompanyID RCompanyId, M.CompanyID OCompanyID, COM.ShortName RCompany, M.LCNo, M.LCDate
                    FROM M
                    INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
                    LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = M.CompanyID;
                
                     --childs
                     ;With 
                    M AS (
	                    SELECT POC.YPOChildID, POC.YPOMasterID, POC.ItemMasterID, POC.UnitID, POC.POQty, POC.Rate, POC.PIValue POValue, POC.YarnProgramId, POC.ShadeCode, POForID = ISNULL(POC.POForID,0), POForName = ETV.ValueName
	                    FROM {TableNames.YarnPOChild} POC 
						INNER JOIN {DbNames.EPYSL}..EntityTypeValue ETV ON ETV.ValueID = POC.POForID
						WHERE POC.YPOMasterID = {PoId}
                    ),
                    POChildBuyer As(
						Select a.YPOChildID, a.BuyerID, ISNULL(C.[ShortName],'') BuyerName
	                    From {TableNames.YarnPOChild} a
	                    LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = a.BuyerID and C.ContactID !=0
	                    Where a.YPOMasterID = {PoId}
					),
					C AS(
					    SELECT C.POChildID, 
                        MAX(C.ChallanCount) AS ChallanCount, 
                        MAX(C.PhysicalCount) AS PhysicalCount
                        FROM {TableNames.YARN_RECEIVE_CHILD} C
                        GROUP BY C.POChildID
					),
                    I As (
	                    Select a.YPOChildID, a.BuyerID, C.[ShortName] BuyerName
	                    From {TableNames.YarnPOChildBuyer} a
	                    LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = a.BuyerID
	                    Where a.YPOMasterID = {PoId}
                    ), YPOB As (
	                    Select YPOChildID
		                    , STUFF(
		                    (
			                    SELECT ',' + CAST(I.BuyerID as varchar) AS [text()]
			                    FROM I
			                    WHERE I.YPOChildID = O.YPOChildID
			                    FOR XML PATH('')
		                    ), 1, 1, '') As YarnChildPoBuyerIds
		                    , STUFF(
		                    (
			                    SELECT  ', ' + I.BuyerName
			                    FROM I
			                    WHERE I.YPOChildID = O.YPOChildID
			                    FOR XML PATH(''), TYPE
		                    ).value('.', 'VARCHAR(MAX)'), 1, 1, '') As BuyerNames
	                    From {TableNames.YarnPOChildBuyer} O
	                    Where YPOMasterID = {PoId}
	                    Group By YPOChildID
                    ), CO As (
	                    Select YO.YPOChildID, YO.ExportOrderID, ExportOrderNo
	                    From {TableNames.YarnPOChildOrder} YO
	                    LEFT JOIN {DbNames.EPYSL}..ExportOrderMaster EOM ON EOM.ExportOrderID = YO.ExportOrderID
	                    Where YPOMasterID = {PoId}
                    ),
                    YPOO As (
	                    Select YPOChildID
		                    , STUFF(
		                    (
			                    SELECT ',' + CAST(CO.ExportOrderID As varchar) AS [text()]
			                    FROM CO
			                    WHERE CO.YPOChildID = O.YPOChildID --Group By O.ExportOrderID
			                    FOR XML PATH('')
		                    ), 1, 1, '') As YarnChildPoExportIds
		                    , STUFF(
		                    (
			                    SELECT ',' + CAST(CO.ExportOrderNo As varchar) AS [text()]
			                    FROM CO
			                    WHERE CO.YPOChildID = O.YPOChildID
			                    FOR XML PATH('')
		                    ), 1, 1, '') As YarnChildPoEWOs
	                    From YarnPOChildOrder O
	                    Where YPOMasterID = {PoId}
	                    Group By YPOChildID  
                    ),
                    POC AS
                    (
                        SELECT POC.YPOChildID, ReceivedQty = ISNULL(SUM(YRC.ReceiveQty),0), 
                        BalanceQty = POC.POQty - ISNULL(SUM(YRC.ReceiveQty),0), ReceiveForId = ISNULL(YRC.ReceiveForId,0), RF.ReceiveForName,
						MaxReceiveQty = Case When (POC.POQty /100)*5 < 50 Then
													POC.POQty - ISNULL(SUM(YRC.ReceiveQty),0) + 50
												Else
													POC.POQty - ISNULL(SUM(YRC.ReceiveQty),0) + ((POC.POQty /100)*5) End
                        FROM {TableNames.YarnPOChild} POC
                        INNER JOIN {TableNames.YarnPOMaster} POM ON POM.YPOMasterID = POC.YPOMasterID
                        LEFT JOIN {TableNames.YARN_RECEIVE_CHILD} YRC ON YRC.POChildID = POC.YPOChildID
						LEFT JOIN ReceiveFor RF ON RF.ReceiveForId = YRC.ReceiveForId
                        WHERE POM.YPOMasterID = {PoId}
                        GROUP BY POC.YPOChildID,POC.POQty, YRC.ReceiveForId, RF.ReceiveForName
                    )
                    SELECT POChildID = M.YPOChildID, ChildID = ROW_NUMBER() OVER (ORDER BY M.YPOMasterID, M.ItemMasterID, M.UnitID, M.Rate, M.YarnProgramId, M.ShadeCode),
                    M.YPOMasterID POID, M.ItemMasterID, M.UnitID, POQty = SUM(M.POQty), M.Rate, UU.DisplayUnitDesc, M.YarnProgramId, M.ShadeCode,
                    ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, 
                    ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc, C.ChallanCount, 
                    C.PhysicalCount, 
                    YarnChildPoBuyerIds = IIF(ISNULL(YPOB.YarnChildPoBuyerIds,'') ='' , ISNULL(PB.BuyerID,'') ,ISNULL(YPOB.YarnChildPoBuyerIds,'')),
					BuyerNames = IIF(ISNULL(REPLACE(YPOB.BuyerNames,'amp;',''),'') ='' , ISNULL(PB.BuyerName,'') ,ISNULL(REPLACE(YPOB.BuyerNames,'amp;',''),'')),
                    YPOO.YarnChildPoExportIds,YPOO.YarnChildPoEWOs,
                    POC.ReceivedQty, POC.BalanceQty, M.POForID, M.POForName, POC.ReceiveForId, POC.ReceiveForName, POC.MaxReceiveQty
                    FROM M
                    LEFT JOIN YPOB On YPOB.YPOChildID = M.YPOChildID
                    LEFT JOIN POChildBuyer PB  On PB.YPOChildID = M.YPOChildID
                    LEFT JOIN YPOO On YPOO.YPOChildID = M.YPOChildID
                    INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = M.ItemMasterID 
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                    LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
                    Inner Join {DbNames.EPYSL}..Unit AS UU On UU.UnitID = M.UnitID
                    LEFT JOIN POC ON POC.YPOChildID = M.YPOChildID
					LEFT JOIN C ON C.POChildID=M.YPOChildID
                    GROUP BY M.YPOChildID, M.YPOMasterID, M.ItemMasterID, M.UnitID, M.Rate, UU.DisplayUnitDesc, M.YarnProgramId, M.ShadeCode,
                    ISV1.SegmentValue, ISV2.SegmentValue, ISV3.SegmentValue, ISV4.SegmentValue, 
                    ISV5.SegmentValue, ISV6.SegmentValue, ISV7.SegmentValue, C.ChallanCount,C.PhysicalCount,
                     --YPOB.YarnChildPoBuyerIds, REPLACE(YPOB.BuyerNames,'amp;',''),
					IIF(ISNULL(YPOB.YarnChildPoBuyerIds,'') ='' , ISNULL(PB.BuyerID,'') ,ISNULL(YPOB.YarnChildPoBuyerIds,'')),
					IIF(ISNULL(REPLACE(YPOB.BuyerNames,'amp;',''),'') ='' , ISNULL(PB.BuyerName,'') ,ISNULL(REPLACE(YPOB.BuyerNames,'amp;',''),'')),
                    YPOO.YarnChildPoExportIds,YPOO.YarnChildPoEWOs,
                    POC.ReceivedQty, POC.BalanceQty, M.POForID, M.POForName, POC.ReceiveForId, POC.ReceiveForName, POC.MaxReceiveQty;

                    ----TransportTypeList
                    {CommonQueries.GetEntityTypesByEntityTypeId(EntityTypeConstants.TRANSPORT_TYPE)};

                    ----TransportModeList
                    {CommonQueries.GetEntityTypesByEntityTypeId(EntityTypeConstants.TRANSPORT_MODE)};

                    ----TransportAgencyList
                    {CommonQueries.GetContactsByCategoryType(ContactCategoryNames.CARRYING_CONTRACTOR)}; 
                    /* {CommonQueries.GetEntityTypesByEntityTypeId(EntityTypeConstants.TRANSPORT_AGENCY)}; */

                    -----ShipmentStatusList
                    /* {CommonQueries.GetEntityTypesByEntityTypeId(EntityTypeConstants.SHIPMENT_STATUS)}; */
                    SELECT 0 As id, 'Full' text 
                    Union
                    SELECT 1 As id, 'Partial' text 
                    Union
                    SELECT 2 As id, 'Last' text 

                    -----LocationList
                    {CommonQueries.GetLocation()};

                    -----SpinnerList
                    SELECT id = CS.SpinnerID, [text] = C.ShortName
                    FROM {DbNames.EPYSL}..ContactSpinnerSetup CS
                    INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = CS.SpinnerID
                    INNER JOIN {TableNames.YarnPOMaster} PO ON PO.SupplierID = CS.ContactID
                    WHERE PO.YPOMasterID = {PoId}

                    -----SupplierList
                    SELECT Cast(C.ContactID As varchar) [id], C.ShortName [text]
                    FROM {TableNames.YarnPOMaster} PO 
                    INNER JOIN {DbNames.EPYSL}..ContactSpinnerSetup CS ON CS.ContactID = PO.SupplierID
                    INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = CS.ContactID
                    WHERE PO.YPOMasterID = {PoId}

                    Select Cast (CE.CompanyID as varchar) [id] , CE.ShortName [text]
                    FROM {DbNames.EPYSL}..CompanyEntity CE;

                    --Buyers
                    {CommonQueries.GetContactsByCategoryId(ContactCategoryConstants.CONTACT_CATEGORY_BUYER)};

                    SELECT id = YarnPackingID, [text] = PackNo, [desc] = SpinnerID, additionalValue = Cone, additionalValue2 = NetWeight
                    FROM SpinnerWiseYarnPacking_HK;

                    -- POV2 IgnoreValidation
                    SELECT * FROM POV2IgnoreValidation;

                     --Receive For
                     Select Cast (RF.ReceiveForId as varchar) [id] , RF.ReceiveForName [text] FROM ReceiveFor RF;";
                }
                else
                {
                    sql = $@"
                    ;WITH M AS (
	                    SELECT PO.CDAPOMasterID, PO.PONo, PO.PODate, PO.CompanyID, PO.SupplierID, PO.CurrencyID, PO.ShippingTolerance
	                    FROM {TableNames.CDAPOMASTER} PO 
                        WHERE PO.CDAPOMasterID = {PoId}
                    )
                    SELECT M.CDAPOMasterID POID, M.PONo, M.PODate, M.SupplierID, M.CurrencyID, M.ShippingTolerance Tolerance, CC.[Name] SupplierName, 
                    M.CompanyID RCompanyID, M.CompanyID OCompanyID, COM.ShortName RCompany
                    FROM M INNER JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
                    LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = M.CompanyID;
                
                    -----childs
                    ;With M AS (
	                    SELECT POC.CDAPOChildID, POC.CDAPOMasterID, POC.ItemMasterID, POC.UnitID As UnitID, POC.POQty, POC.Rate
	                    FROM {TableNames.CDAPOCHILD} POC 
                        WHERE POC.CDAPOMasterID = {PoId}
                    )
                    SELECT M.CDAPOChildID As ChildID, M.CDAPOMasterID POID, M.ItemMasterID, M.UnitID UnitID, M.POQty, M.Rate, UU.DisplayUnitDesc, 
                    Item.SegmentValue Segment1ValueDesc, Agent.SegmentValue Segment2ValueDesc
                    FROM M
                    Inner Join {DbNames.EPYSL}..ItemMaster AS IM On IM.ItemMasterID = M.ItemMasterID
                    Left Join {DbNames.EPYSL}..ItemSegmentValue Item On IM.Segment1ValueID = Item.SegmentValueID
                    Left Join {DbNames.EPYSL}..ItemSegmentValue  Agent On IM.Segment2ValueID = Agent.SegmentValueID
                    Inner Join {DbNames.EPYSL}..Unit AS UU On UU.UnitID = M.UnitID;

                    ----TransportTypeList
                    {CommonQueries.GetEntityTypesByEntityTypeId(EntityTypeConstants.TRANSPORT_TYPE)};

                    ----TransportModeList
                    {CommonQueries.GetEntityTypesByEntityTypeId(EntityTypeConstants.TRANSPORT_MODE)};

                    ----TransportAgencyList
                    {CommonQueries.GetContactsByCategoryType(ContactCategoryNames.CARRYING_CONTRACTOR)}; 
                 /* {CommonQueries.GetEntityTypesByEntityTypeId(EntityTypeConstants.TRANSPORT_AGENCY)}; */

                    -----ShipmentStatusList
                    /* {CommonQueries.GetEntityTypesByEntityTypeId(EntityTypeConstants.SHIPMENT_STATUS)}; */
                    SELECT 0 As id, 'Full' text 
                    Union
                    SELECT 1 As id, 'Partial' text 
                    Union
                    SELECT 2 As id, 'Last' text 

                    -----LocationList
                    {CommonQueries.GetLocation()};

                    -----SpinnerList
                    {CommonQueries.GetContactsByCategoryType(ContactCategoryNames.SPINNER)};
                    -----SupplierList
                    {CommonQueries.GetContactsByCategoryType(ContactCategoryNames.SUPPLIER)};
                    
                    Select Cast (CE.CompanyID as varchar) [id] , CE.ShortName [text]
                    FROM {DbNames.EPYSL}..CompanyEntity CE;

                    --Buyers
                    {CommonQueries.GetContactsByCategoryId(ContactCategoryConstants.CONTACT_CATEGORY_BUYER)};

                    SELECT id = YarnPackingID, [text] = PackNo, [desc] = SpinnerID, additionalValue = Cone, additionalValue2 = NetWeight
                    FROM SpinnerWiseYarnPacking_HK;

                    -- POV2 IgnoreValidation
                    SELECT * FROM POV2IgnoreValidation;

                    --Receive For
                    Select Cast (RF.ReceiveForId as varchar) [id] , RF.ReceiveForName [text] FROM ReceiveFor RF;";
                }
            }
            try
            {
                //{DbNames.EPYSL}..ContactSpinnerSetup
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnReceiveMaster data = await records.ReadFirstOrDefaultAsync<YarnReceiveMaster>();
                Guard.Against.NullObject(data);
                data.YarnReceiveChilds = records.Read<YarnReceiveChild>().ToList();
                data.TransportTypeList = records.Read<Select2OptionModel>().ToList();
                data.TransportModeList = records.Read<Select2OptionModel>().ToList();
                data.CContractorList = records.Read<Select2OptionModel>().ToList();
                data.ShipmentStatusList = records.Read<Select2OptionModel>().ToList();
                data.LocationList = records.Read<Select2OptionModel>().ToList();
                data.SpinnerList = records.Read<Select2OptionModel>().ToList();
                data.SupplierList = records.Read<Select2OptionModel>().ToList();
                data.RCompanyList = records.Read<Select2OptionModel>().ToList();
                data.BuyerList = records.Read<Select2OptionModel>().ToList();
                data.SpinnerWisePackingList = records.Read<Select2OptionModelExtended>().ToList();
                data.SpinnerWisePackingList.Insert(0, new Select2OptionModelExtended()
                {
                    id = 0.ToString(),
                    text = "Empty",
                    desc = 0.ToString(),
                    additionalValue = 0.ToString(),
                    additionalValue2 = 0.ToString()
                });

                var ignoreValidationPO = await records.ReadAsync<YarnPOMaster>();
                if (ignoreValidationPO != null && ignoreValidationPO.Count() > 0)
                {
                    data.IgnoreValidationPOIds = ignoreValidationPO.Select(x => x.YPOMasterID).ToList();
                }
                data.ReceiveForList = records.Read<Select2OptionModel>().ToList();
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
        public async Task<YarnReceiveMaster> GetNewSampleYarnAsync()
        {
            string sql = $@"
                    ----TransportTypeList
                    {CommonQueries.GetEntityTypesByEntityTypeId(EntityTypeConstants.TRANSPORT_TYPE)};

                    ----TransportModeList
                    {CommonQueries.GetEntityTypesByEntityTypeId(EntityTypeConstants.TRANSPORT_MODE)};

                    ----TransportAgencyList
                    {CommonQueries.GetContactsByCategoryType(ContactCategoryNames.CARRYING_CONTRACTOR)}; 
                    /* {CommonQueries.GetEntityTypesByEntityTypeId(EntityTypeConstants.TRANSPORT_AGENCY)}; */

                    -----ShipmentStatusList
                    /* {CommonQueries.GetEntityTypesByEntityTypeId(EntityTypeConstants.SHIPMENT_STATUS)}; */
                    SELECT 0 As id, 'Full' text 
                    Union
                    SELECT 1 As id, 'Partial' text 
                    Union
                    SELECT 2 As id, 'Last' text 

                    -----LocationList
                    {CommonQueries.GetLocation()};

                    -----SpinnerList
                    {CommonQueries.GetContactsByCategoryType(ContactCategoryNames.SPINNER)};

                    -----SupplierList
                    {CommonQueries.GetContactsByCategoryType(ContactCategoryNames.SUPPLIER)};

                    Select Cast (CE.CompanyID as varchar) [id] , CE.ShortName [text]
                    FROM {DbNames.EPYSL}..CompanyEntity CE 
                    WHERE CE.BusinessNature IN ('TEX','PC') ORDER BY CE.ShortName

                    --Buyers
                    {CommonQueries.GetContactsByCategoryId(ContactCategoryConstants.CONTACT_CATEGORY_BUYER)};

                    SELECT id = YarnPackingID, [text] = PackNo, [desc] = SpinnerID, additionalValue = Cone, additionalValue2 = NetWeight
                    FROM SpinnerWiseYarnPacking_HK;

                    -- POV2 IgnoreValidation
                    SELECT * FROM POV2IgnoreValidation;

                    --Receive For
                    Select Cast (RF.ReceiveForId as varchar) [id] , RF.ReceiveForName [text] FROM ReceiveFor RF;";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnReceiveMaster data = new YarnReceiveMaster();
                Guard.Against.NullObject(data);
                data.TransportTypeList = records.Read<Select2OptionModel>().ToList();
                data.TransportModeList = records.Read<Select2OptionModel>().ToList();
                data.CContractorList = records.Read<Select2OptionModel>().ToList();
                data.ShipmentStatusList = records.Read<Select2OptionModel>().ToList();
                data.LocationList = records.Read<Select2OptionModel>().ToList();
                data.SpinnerList = records.Read<Select2OptionModel>().ToList();
                data.SupplierList = records.Read<Select2OptionModel>().ToList();
                data.RCompanyList = records.Read<Select2OptionModel>().ToList();
                data.BuyerList = records.Read<Select2OptionModel>().ToList();
                data.SpinnerWisePackingList = records.Read<Select2OptionModelExtended>().ToList();
                data.SpinnerWisePackingList.Insert(0, new Select2OptionModelExtended()
                {
                    id = 0.ToString(),
                    text = "Empty",
                    desc = 0.ToString(),
                    additionalValue = 0.ToString(),
                    additionalValue2 = 0.ToString()
                });

                var ignoreValidationPO = await records.ReadAsync<YarnPOMaster>();
                if (ignoreValidationPO != null && ignoreValidationPO.Count() > 0)
                {
                    data.IgnoreValidationPOIds = ignoreValidationPO.Select(x => x.YPOMasterID).ToList();
                }
                data.ReceiveForList = records.Read<Select2OptionModel>().ToList();
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
        public async Task<YarnReceiveMaster> GetAsync(int id, int POID)
        {
            var sql = $@"
                ;WITH M AS (
	                SELECT RM.ReceiveID, RM.ReceiveDate, RM.ReceiveNo, RM.LocationID, RM.RCompanyID, RM.OCompanyID, RM.SupplierID, RM.LCNo, RM.LCDate, RM.Tolerance,
				    RM.InvoiceNo, RM.InvoiceDate, RM.BLNo, RM.BLDate, RM.BankBranchID, RM.PLNo, RM.PLDate, RM.ChallanNo, RM.ChallanDate, RM.GPNo, RM.GPDate, RM.TransportMode,
				    RM.TransportTypeID, RM.CContractorID, RM.VehicalNo, RM.ShipmentStatus, RM.Remarks, RM.SpinnerID, RM.POID, RM.CIID, RM.LCID, RM.ReceivedById, RM.GPTime, 
				    RM.CurrencyID, RM.PONo, RM.PODate, RM.ACompanyInvoice, RM.IsSampleYarn,RM.TruckChallanNo,RM.TruckChallanDate, RM.MushakNo, RM.MushakDate, RM.BillEntryNo
                    FROM {TableNames.YARN_RECEIVE_MASTER} RM 
                    WHERE RM.ReceiveId = {id}
                )
                SELECT	M.ReceiveID, M.ReceiveDate, M.ReceiveNo, M.LocationID, M.RCompanyID, M.OCompanyID, M.SupplierID, M.LCNo, M.LCDate, M.Tolerance,
				M.InvoiceNo, M.InvoiceDate, M.BLNo, M.BLDate, M.BankBranchID, M.PLNo, M.PLDate, M.ChallanNo, M.ChallanDate, M.GPNo, M.GPDate, M.TransportMode,
				M.TransportTypeID, M.CContractorID, M.VehicalNo, M.ShipmentStatus, M.Remarks, M.SpinnerID, M.POID, M.CIID, M.LCID, M.ReceivedById, M.GPTime,
				CC.[Name] SupplierName, BB.BranchName BankBranchName, COM.ShortName RCompany, M.CurrencyID, M.PONo, M.PODate, M.ACompanyInvoice, M.IsSampleYarn,
				M.TruckChallanNo,M.TruckChallanDate, M.MushakNo, M.MushakDate, M.BillEntryNo
		        FROM M
		        LEFT JOIN {DbNames.EPYSL}..Contacts CC ON CC.ContactID = M.SupplierID
				LEFT JOIN {DbNames.EPYSL}..BankBranch BB ON BB.BankBranchID = M.BankBranchID
				LEFT JOIN {DbNames.EPYSL}..CompanyEntity COM ON COM.CompanyID = M.RCompanyID;
                
                -----childs
                ;With 
                X AS (
	                SELECT RC.ChildID, RC.ReceiveID, RC.ItemMasterID, RC.InvoiceChildID, RC.POChildID, RC.UnitID, RC.InvoiceQty, RC.ChallanQty, RC.ShortQty, RC.ExcessQty,
	                RC.ReceiveQty, RC.Rate, RC.Remarks, RC.LotNo, RC.ChallanLot, RC.SpinnerID, RC.NoOfCartoon, RC.NoOfCone, RC.YarnPackingID, RC.POQty,
                    RC.YarnProgramId, RC.ChallanCount, RC.PhysicalCount, RC.ShadeCode, RC.YarnControlNo, RC.YarnCategory, POC.YPOMasterID,RC.ReceiveForId
	                FROM {TableNames.YARN_RECEIVE_CHILD} RC 
	                LEFT JOIN {TableNames.YarnPOChild} POC ON POC.YPOChildID = RC.POChildID
	                WHERE RC.ReceiveID = {id}
	                Group By RC.ChildID, RC.ReceiveID, RC.ItemMasterID, RC.InvoiceChildID, RC.POChildID, RC.UnitID, RC.InvoiceQty, RC.ChallanQty, RC.ShortQty, RC.ExcessQty,
	                RC.ReceiveQty, RC.Rate, RC.Remarks, RC.LotNo, RC.ChallanLot, RC.SpinnerID, RC.NoOfCartoon, RC.YarnPackingID, RC.NoOfCone, RC.POQty,
                    RC.YarnProgramId, RC.ChallanCount, RC.PhysicalCount, RC.ShadeCode, RC.YarnControlNo, RC.YarnCategory,POC.POQty,RC.ReceiveQty, POC.YPOMasterID,RC.ReceiveForId
                ),
                POC AS
                (
                    SELECT POC.YPOChildID, ReceivedQty = ISNULL(SUM(YRC.ReceiveQty),0) - ISNULL(SUM(X.ReceiveQty),0), 
                    BalanceQty = POC.POQty - ISNULL(SUM(YRC.ReceiveQty),0) + ISNULL(SUM(X.ReceiveQty),0),
					MaxReceiveQty = Case When (POC.POQty /100)*5 < 50 Then
							POC.POQty - ISNULL(SUM(YRC.ReceiveQty),0) + ISNULL(SUM(X.ReceiveQty),0) + 50
						Else
							POC.POQty - ISNULL(SUM(YRC.ReceiveQty),0) + ISNULL(SUM(X.ReceiveQty),0) + ((POC.POQty /100)*5) End
                    FROM {TableNames.YarnPOChild} POC
                    INNER JOIN {TableNames.YarnPOMaster} POM ON POM.YPOMasterID = POC.YPOMasterID
                    LEFT JOIN {TableNames.YARN_RECEIVE_CHILD} YRC ON YRC.POChildID = POC.YPOChildID
                    LEFT JOIN X ON X.POChildID = YRC.POChildID AND X.ReceiveID = YRC.ReceiveID
                    WHERE POM.YPOMasterID = {POID}
                    GROUP BY POC.YPOChildID,POC.POQty

                    /*
	                SELECT POC.YPOChildID, ReceivedQty = SUM(YRC.ReceiveQty) - SUM(X.ReceiveQty), 
                    BalanceQty = POC.POQty - SUM(YRC.ReceiveQty) + SUM(X.ReceiveQty)
	                FROM {TableNames.YARN_RECEIVE_CHILD} YRC
	                INNER JOIN {TableNames.YarnPOChild} POC ON POC.YPOChildID = YRC.POChildID
	                INNER JOIN {TableNames.YarnPOMaster} POM ON POM.YPOMasterID = POC.YPOMasterID
	                LEFT JOIN X ON X.POChildID = YRC.POChildID AND X.ReceiveID = YRC.ReceiveID
	                WHERE POC.YPOMasterID = {POID}
	                GROUP BY POC.YPOChildID,POC.POQty
                    */
                ),
                POChildBuyer As(
					Select a.YPOChildID, a.BuyerID, ISNULL(C.[ShortName],'') BuyerName
	                From {TableNames.YarnPOChild} a
	                LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = a.BuyerID and C.ContactID !=0
	                Where a.YPOMasterID = {POID}
				),
				I As (
	                Select a.YPOChildID, a.BuyerID, C.[ShortName] BuyerName
	                From {TableNames.YarnPOChildBuyer} a
	                LEFT JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = a.BuyerID
	                Where a.YPOMasterID = {POID}
                ), YPOB As (
	                Select YPOChildID
		                , STUFF(
		                (
			                SELECT ',' + CAST(I.BuyerID as varchar) AS [text()]
			                FROM I
			                WHERE I.YPOChildID = O.YPOChildID
			                FOR XML PATH('')
		                ), 1, 1, '') As YarnChildPoBuyerIds
		                , STUFF(
		                (
			                SELECT  ', ' + I.BuyerName
			                FROM I
			                WHERE I.YPOChildID = O.YPOChildID
			                FOR XML PATH(''), TYPE
		                ).value('.', 'VARCHAR(MAX)'), 1, 1, '') As BuyerNames
	                From {TableNames.YarnPOChildBuyer} O
	                Where YPOMasterID = {POID}
	                Group By YPOChildID
                )
                SELECT X.ChildID, X.ReceiveID, X.ItemMasterID, X.InvoiceChildID, X.POChildID, X.UnitID, X.InvoiceQty, X.POQty, X.ChallanQty, X.ShortQty, X.ExcessQty, X.ReceiveQty, 
                X.Rate, X.Remarks, X.LotNo, X.ChallanLot, X.SpinnerID, SpinnerName = S.ShortName ,X.NoOfCartoon, X.NoOfCone, X.YarnPackingID, PackNo = CASE WHEN X.YarnPackingID > 0 THEN P.PackNo ELSE 'Empty' END, 'Kg' DisplayUnitDesc, X.YarnProgramId, X.ChallanCount, X.PhysicalCount, X.YarnCategory,
                ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc, 
                ISV5.SegmentValue Segment5ValueDesc, ISV6.SegmentValue Segment6ValueDesc, ISV7.SegmentValue Segment7ValueDesc, X.ShadeCode, X.YarnControlNo, Segment6ValueId = IM.Segment6ValueID,
                POC.ReceivedQty, POC.BalanceQty,X.ReceiveForId,
				YarnChildPoBuyerIds = IIF(ISNULL(YPOB.YarnChildPoBuyerIds,'') ='' , ISNULL(PB.BuyerID,'') ,ISNULL(YPOB.YarnChildPoBuyerIds,'')),
			    BuyerNames = IIF(ISNULL(REPLACE(YPOB.BuyerNames,'amp;',''),'') ='' , ISNULL(REPLACE(PB.BuyerName,'amp;',''),'') ,ISNULL(REPLACE(YPOB.BuyerNames,'amp;',''),'')), POC.MaxReceiveQty
                FROM X
                LEFT JOIN POC ON POC.YPOChildID = X.POChildID
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = X.ItemMasterID 
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV5 ON ISV5.SegmentValueID = IM.Segment5ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV6 On ISV6.SegmentValueID = IM.Segment6ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV7 ON ISV7.SegmentValueID = IM.Segment7ValueID
				LEFT JOIN YPOB On YPOB.YPOChildID =X.POChildID
                LEFT JOIN POChildBuyer PB  On PB.YPOChildID = X.POChildID
                LEFT JOIN {DbNames.EPYSL}..Contacts S ON S.ContactId = X.SpinnerID
                LEFT JOIN SpinnerWiseYarnPacking_HK P ON P.YarnPackingID = X.YarnPackingID
                LEFT Join {DbNames.EPYSL}..Unit AS UU On UU.UnitID = X.UnitID;

                --YarnReceiveChildOrder
                SELECT CO.* 
                FROM {TableNames.YARN_RECEIVE_CHILD_ORDER} CO
                INNER JOIN {TableNames.YARN_RECEIVE_CHILD} RC ON RC.ChildID = CO.ReceiveChildID
                WHERE RC.ReceiveID = {id};

                --YarnReceiveChildBuyer
                SELECT CO.*, BuyerName = C.ShortName
                FROM {TableNames.YARN_RECEIVE_CHILD_BUYER} CO
                INNER JOIN {TableNames.YARN_RECEIVE_CHILD} RC ON RC.ChildID = CO.ReceiveChildID
                INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = CO.BuyerID
                WHERE RC.ReceiveID = {id};

                ----TransportTypeList
                {CommonQueries.GetEntityTypesByEntityTypeId(EntityTypeConstants.TRANSPORT_TYPE)};

                ----TransportModeList
                {CommonQueries.GetEntityTypesByEntityTypeId(EntityTypeConstants.TRANSPORT_MODE)};

                ----TransportAgencyList
                {CommonQueries.GetContactsByCategoryType(ContactCategoryNames.CARRYING_CONTRACTOR)}; 
                /* {CommonQueries.GetEntityTypesByEntityTypeId(EntityTypeConstants.TRANSPORT_AGENCY)}; */

                -----ShipmentStatusList
                /* {CommonQueries.GetEntityTypesByEntityTypeId(EntityTypeConstants.SHIPMENT_STATUS)}; */
                SELECT 0 As id, 'Full' text 
                Union
                SELECT 1 As id, 'Partial' text 
                Union
                SELECT 2 As id, 'Last' text 

                -----LocationList
                {CommonQueries.GetLocation()};

                -----SpinnerList
                SELECT id = CS.SpinnerID, [text] = C.ShortName
                FROM {DbNames.EPYSL}..ContactSpinnerSetup CS
                INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = CS.SpinnerID
                INNER JOIN {TableNames.YARN_RECEIVE_MASTER} PO ON PO.SupplierID = CS.ContactID
                WHERE PO.ReceiveID = {id};

                -----SupplierList
                SELECT Cast(C.ContactID As varchar) [id], C.ShortName [text]
                FROM {TableNames.YARN_RECEIVE_MASTER} PO 
                INNER JOIN {DbNames.EPYSL}..ContactSpinnerSetup CS ON CS.ContactID = PO.SupplierID
                INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = CS.ContactID
                WHERE PO.ReceiveID = {id};

                Select Cast (CE.CompanyID as varchar) [id] , CE.ShortName [text]
                FROM {DbNames.EPYSL}..CompanyEntity CE 
                WHERE CE.BusinessNature IN ('TEX','PC') ORDER BY CE.ShortName;

                --Buyers
                {CommonQueries.GetContactsByCategoryId(ContactCategoryConstants.CONTACT_CATEGORY_BUYER)}

                SELECT id = YarnPackingID, [text] = PackNo, [desc] = SpinnerID, additionalValue = Cone, additionalValue2 = NetWeight
                FROM SpinnerWiseYarnPacking_HK;

                -----SpinnerList
                {CommonQueries.GetContactsByCategoryType(ContactCategoryNames.SPINNER)};

                -- POV2 IgnoreValidation
                SELECT * FROM POV2IgnoreValidation;

                --Receive For
                Select Cast (RF.ReceiveForId as varchar) [id] , RF.ReceiveForName [text] FROM ReceiveFor RF;";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnReceiveMaster data = await records.ReadFirstOrDefaultAsync<YarnReceiveMaster>();
                Guard.Against.NullObject(data);
                data.YarnReceiveChilds = records.Read<YarnReceiveChild>().ToList();
                List<YarnReceiveChildOrder> orders = records.Read<YarnReceiveChildOrder>().ToList();
                List<YarnReceiveChildBuyer> buyers = records.Read<YarnReceiveChildBuyer>().ToList();

                data.YarnReceiveChilds.ForEach(c =>
                {
                    c.YarnReceiveChildOrders = orders.Where(o => o.ReceiveChildID == c.ChildID).ToList();
                    c.YarnReceiveChildBuyers = buyers.Where(o => o.ReceiveChildID == c.ChildID).ToList();

                    c.YarnChildPoBuyerIds = string.Join(",", c.YarnReceiveChildBuyers.Select(x => x.BuyerID).Distinct());
                    c.BuyerNames = string.Join(",", c.YarnReceiveChildBuyers.Select(x => x.BuyerName).Distinct());

                    c.YarnChildPoExportIds = string.Join(",", c.YarnReceiveChildOrders.Select(x => x.ExportOrderID).Distinct());
                    c.YarnChildPoEWOs = string.Join(",", c.YarnReceiveChildOrders.Select(x => x.EWONo).Distinct());
                });

                data.TransportTypeList = records.Read<Select2OptionModel>().ToList();
                data.TransportModeList = records.Read<Select2OptionModel>().ToList();
                data.CContractorList = records.Read<Select2OptionModel>().ToList();
                data.ShipmentStatusList = records.Read<Select2OptionModel>().ToList();
                data.LocationList = records.Read<Select2OptionModel>().ToList();
                data.SpinnerList = records.Read<Select2OptionModel>().ToList();
                data.SupplierList = records.Read<Select2OptionModel>().ToList();
                data.RCompanyList = records.Read<Select2OptionModel>().ToList();
                data.BuyerList = records.Read<Select2OptionModel>().ToList();
                data.SpinnerWisePackingList = records.Read<Select2OptionModelExtended>().ToList();
                data.SpinnerWisePackingList.Insert(0, new Select2OptionModelExtended()
                {
                    id = 0.ToString(),
                    text = "Empty",
                    desc = 0.ToString(),
                    additionalValue = 0.ToString(),
                    additionalValue2 = 0.ToString()
                });

                var spinners = records.Read<Select2OptionModel>().ToList();
                if (data.IsSampleYarn) data.SpinnerList = spinners;

                var ignoreValidationPO = await records.ReadAsync<YarnPOMaster>();
                if (ignoreValidationPO != null && ignoreValidationPO.Count() > 0)
                {
                    data.IgnoreValidationPOIds = ignoreValidationPO.Select(x => x.YPOMasterID).ToList();
                }
                data.ReceiveForList = records.Read<Select2OptionModel>().ToList();
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

        public async Task<YarnReceiveMaster> GetAllAsync(int id)
        {
            var sql = $@"
            ;SELECT * 
            FROM {TableNames.YARN_RECEIVE_MASTER} 
            WHERE ReceiveID = {id}

            SELECT YRC.*,PO_ItemMasterId = POC.ItemMasterID 
            FROM {TableNames.YARN_RECEIVE_CHILD} YRC
            LEFT JOIN {TableNames.YarnPOChild} POC ON POC.YPOChildID = YRC.POChildID
            WHERE YRC.ReceiveID = {id}

            ;SELECT CO.*, BuyerName = C.ShortName
            FROM {TableNames.YARN_RECEIVE_CHILD_BUYER} CO
            INNER JOIN {TableNames.YARN_RECEIVE_CHILD} RC ON RC.ChildID = CO.ReceiveChildID
            INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = CO.BuyerID
            WHERE RC.ReceiveID = {id}

            ;SELECT * 
            FROM {TableNames.YARN_RECEIVE_CHILD_ORDER} CO
            INNER JOIN {TableNames.YARN_RECEIVE_CHILD} RC ON RC.ChildID = CO.ReceiveChildID
            WHERE ReceiveID = {id}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnReceiveMaster data = await records.ReadFirstOrDefaultAsync<YarnReceiveMaster>();
                Guard.Against.NullObject(data);
                data.YarnReceiveChilds = records.Read<YarnReceiveChild>().ToList();

                if (!data.IsSampleYarn)
                {
                    data.YarnReceiveChilds.Where(x => x.ItemMasterID != x.PO_ItemMasterId).ToList().ForEach(c =>
                    {
                        c.ItemMasterID = c.PO_ItemMasterId;
                    });
                }

                List<YarnReceiveChildBuyer> buyers = records.Read<YarnReceiveChildBuyer>().ToList();
                List<YarnReceiveChildOrder> orders = records.Read<YarnReceiveChildOrder>().ToList();
                if (orders.IsNotNull())
                {
                    data.YarnReceiveChilds.ForEach(c =>
                    {
                        c.YarnReceiveChildBuyers = buyers.Where(x => x.ReceiveChildID == c.ChildID).ToList();
                        c.YarnReceiveChildOrders = orders.Where(x => x.ReceiveChildID == c.ChildID).ToList();

                        c.YarnChildPoBuyerIds = string.Join(",", c.YarnReceiveChildBuyers.Select(x => x.BuyerID).Distinct());
                        c.BuyerNames = string.Join(",", c.YarnReceiveChildBuyers.Select(x => x.BuyerName).Distinct());

                        c.YarnChildPoExportIds = string.Join(",", c.YarnReceiveChildOrders.Select(x => x.ExportOrderID).Distinct());
                        c.YarnChildPoEWOs = string.Join(",", c.YarnReceiveChildOrders.Select(x => x.EWONo).Distinct());
                    });
                }
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
        public async Task<List<YarnQCReqChild>> GetPrevReq(PaginationInfo paginationInfo, string LotNo, int ItemMasterID)
        {
            //setName = 'Pre Set' OR 'Post Set'

            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By QCReqChildID ASC" : paginationInfo.OrderBy;

            var query = $@"
                WITH
                F AS 
                (
                    Select YRM.QCReqNo, YRC.QCReqChildID, YRC.ReceiveChildID, YarnDetail = YRC.YarnCategory, 
                            QCReqRemarks = Case When YRemC.Approve=1 then 'Approve'
					                            When YRemC.CommerciallyApprove=1 then 'Commercially Approve'
					                            When YRemC.Diagnostic=1 then 'Diagnostic'
					                            When YRemC.ReTest=1 then 'ReTest' Else '' END
                            from {TableNames.YARN_QC_REQ_CHILD} YRC 
                            INNER JOIN {TableNames.YARN_QC_REQ_MASTER} YRM ON YRM.QCReqMasterID	= YRC.QCReqMasterID
                            LEFT JOIN {TableNames.YARN_QC_REMARKS_CHILD} YRemC ON YRemC.ReceiveChildID=YRC.ReceiveChildID
                            Where YRC.LotNo='{LotNo}' AND YRC.ItemMasterID={ItemMasterID}
                )

                Select *, COUNT(*) Over() TotalRows From F
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<YarnQCReqChild>(query);
        }
        public async Task SaveAsync(YarnReceiveMaster entity, int userId)
        {
            SqlTransaction transaction = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                int maxChildId = 0;
                int maxChildOrderId = 0;
                int maxChildBuyerId = 0;

                List<YarnReceiveChildOrder> yarnReceiveChildOrders = new List<YarnReceiveChildOrder>();
                List<YarnReceiveChildBuyer> yarnReceiveChildBuyers = new List<YarnReceiveChildBuyer>();

                switch (entity.EntityState)
                {
                    case EntityState.Added:

                        entity.ReceiveID = await _service.GetMaxIdAsync(TableNames.YARN_RECEIVE_MASTER);
                        entity.ReceiveNo = await _service.GetMaxNoAsync(TableNames.YARN_RECEIVE_NO, 1, RepeatAfterEnum.EveryYear);
                        //entity.GPNo = await _service.GetMaxNoAsync(TableNames.YARN_GATE_PASS_NO);

                        maxChildId = await _service.GetMaxIdAsync(TableNames.YARN_RECEIVE_CHILD, entity.YarnReceiveChilds.Count);
                        //maxSubProgramId = await _service.GetMaxIdAsync(TableNames.YARN_RECEIVE_CHILD_YARN_SUBPROGRAM, entity.YarnReceiveChilds.Sum(x => x.SubPrograms.Count()));

                        yarnReceiveChildOrders = new List<YarnReceiveChildOrder>();
                        yarnReceiveChildBuyers = new List<YarnReceiveChildBuyer>();

                        entity.YarnReceiveChilds.ForEach(c =>
                        {
                            yarnReceiveChildOrders.AddRange(c.YarnReceiveChildOrders);
                            yarnReceiveChildBuyers.AddRange(c.YarnReceiveChildBuyers);
                        });
                        maxChildOrderId = await _service.GetMaxIdAsync(TableNames.YARN_RECEIVE_CHILD_ORDER, yarnReceiveChildOrders.Count());
                        maxChildBuyerId = await _service.GetMaxIdAsync(TableNames.YARN_RECEIVE_CHILD_BUYER, yarnReceiveChildBuyers.Count());

                        foreach (var item in entity.YarnReceiveChilds)
                        {
                            item.ChildID = maxChildId++;
                            item.ReceiveID = entity.ReceiveID;
                            item.EntityState = EntityState.Added;

                            foreach (var itemOrder in item.YarnReceiveChildOrders)
                            {
                                itemOrder.YRChildOrderID = maxChildOrderId++;
                                itemOrder.ReceiveChildID = item.ChildID;
                                itemOrder.EntityState = EntityState.Added;
                            }
                            foreach (var itemBuyer in item.YarnReceiveChildBuyers)
                            {
                                itemBuyer.YRChildBuyerID = maxChildBuyerId++;
                                itemBuyer.ReceiveChildID = item.ChildID;
                                itemBuyer.EntityState = EntityState.Added;
                            }
                        }

                        break;

                    case EntityState.Modified:
                        var addedChilds = entity.YarnReceiveChilds.FindAll(x => x.EntityState == EntityState.Added);
                        maxChildId = await _service.GetMaxIdAsync(TableNames.YARN_RECEIVE_CHILD, addedChilds.Count);
                        //maxSubProgramId = await _service.GetMaxIdAsync(TableNames.YARN_RECEIVE_CHILD_YARN_SUBPROGRAM, entity.YarnReceiveChilds.Sum(x => x.SubPrograms.Count()));

                        yarnReceiveChildOrders = new List<YarnReceiveChildOrder>();
                        yarnReceiveChildBuyers = new List<YarnReceiveChildBuyer>();

                        entity.YarnReceiveChilds.ForEach(c =>
                        {
                            yarnReceiveChildOrders.AddRange(c.YarnReceiveChildOrders.Where(x => x.EntityState == EntityState.Added).ToList());
                            yarnReceiveChildBuyers.AddRange(c.YarnReceiveChildBuyers.Where(x => x.EntityState == EntityState.Added).ToList());
                        });
                        maxChildOrderId = await _service.GetMaxIdAsync(TableNames.YARN_RECEIVE_CHILD_ORDER, yarnReceiveChildOrders.Count());
                        maxChildBuyerId = await _service.GetMaxIdAsync(TableNames.YARN_RECEIVE_CHILD_BUYER, yarnReceiveChildBuyers.Count());

                        entity.YarnReceiveChilds.ForEach(c =>
                        {
                            if (c.EntityState == EntityState.Added)
                            {
                                c.ChildID = maxChildId++;
                                c.ReceiveID = entity.ReceiveID;
                                c.EntityState = EntityState.Added;
                            }
                            c.YarnReceiveChildOrders.Where(x => x.EntityState == EntityState.Added).ToList().ForEach(o =>
                            {
                                o.YRChildOrderID = maxChildOrderId++;
                                o.ReceiveChildID = c.ChildID;
                                o.EntityState = EntityState.Added;
                            });
                            c.YarnReceiveChildBuyers.Where(x => x.EntityState == EntityState.Added).ToList().ForEach(o =>
                            {
                                o.YRChildBuyerID = maxChildBuyerId++;
                                o.ReceiveChildID = c.ChildID;
                                o.EntityState = EntityState.Added;
                            });
                        });

                        break;

                    case EntityState.Unchanged:
                    case EntityState.Deleted:
                        entity.EntityState = EntityState.Deleted;
                        entity.YarnReceiveChilds.SetDeleted();
                        entity.YarnReceiveChilds.ForEach(c =>
                        {
                            c.YarnReceiveChildOrders.SetDeleted();
                            c.YarnReceiveChildBuyers.SetDeleted();
                        });
                        //entity.YarnReceiveChilds.ForEach(x => x.SubPrograms.SetDeleted());
                        break;

                    default:
                        break;
                }

                List<YarnReceiveChild> yarnReceiveChilds = await this.GetChildsWithYarnControlNo(entity.YarnReceiveChilds, entity.RCompany, entity.ChallanNo);

                yarnReceiveChildOrders = new List<YarnReceiveChildOrder>();
                yarnReceiveChildBuyers = new List<YarnReceiveChildBuyer>();

                yarnReceiveChilds.ForEach(x =>
                {
                    yarnReceiveChildOrders.AddRange(CommonFunction.DeepClone(x.YarnReceiveChildOrders));
                    yarnReceiveChildBuyers.AddRange(CommonFunction.DeepClone(x.YarnReceiveChildBuyers));
                });

                await _service.SaveSingleAsync(entity, transaction);

                await _service.SaveAsync(yarnReceiveChildOrders.Where(x => x.EntityState == EntityState.Deleted).ToList(), transaction);
                await _service.SaveAsync(yarnReceiveChildBuyers.Where(x => x.EntityState == EntityState.Deleted).ToList(), transaction);

                await _service.SaveAsync(yarnReceiveChilds, transaction);
                foreach (YarnReceiveChild item in yarnReceiveChilds)
                {
                   // await _service.ValidationSingleAsync(item, transaction, SPNames.sp_Validation_YarnReceiveChild, item.EntityState, userId, item.ChildID);
                }

                await _service.SaveAsync(yarnReceiveChildOrders.Where(x => x.EntityState != EntityState.Deleted).ToList(), transaction);
                await _service.SaveAsync(yarnReceiveChildBuyers.Where(x => x.EntityState != EntityState.Deleted).ToList(), transaction);

                //#region Stock Operation
                //if (entity.IsApproved)
                //{
                //    userId = entity.EntityState == EntityState.Added ? entity.AddedBy : entity.UpdatedBy;
                //    await _connection.ExecuteAsync("spYarnStockOperation", new { MasterID = entity.ReceiveID, FromMenuType = EnumFromMenuType.YarnReceive, UserId = userId }, transaction, 30, CommandType.StoredProcedure);
                //}
                //#endregion Stock Operation

                transaction.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                if (ex.Message.Contains('~')) throw new Exception(ex.Message.Split('~')[0]);
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }

        public async Task DeleteAsync(YarnReceiveMaster entity, int userId)
        {
            SqlTransaction transaction = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                List<YarnReceiveChildOrder> yarnReceiveChildOrders = new List<YarnReceiveChildOrder>();
                List<YarnReceiveChildBuyer> yarnReceiveChildBuyers = new List<YarnReceiveChildBuyer>();

                entity.YarnReceiveChilds.ForEach(x =>
                {
                    yarnReceiveChildOrders.AddRange(CommonFunction.DeepClone(x.YarnReceiveChildOrders));
                    yarnReceiveChildBuyers.AddRange(CommonFunction.DeepClone(x.YarnReceiveChildBuyers));
                });

                await _service.SaveAsync(yarnReceiveChildOrders, transaction);
                await _service.SaveAsync(yarnReceiveChildBuyers, transaction);
                await _service.SaveAsync(entity.YarnReceiveChilds, transaction);
                await _service.SaveSingleAsync(entity, transaction);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                if (ex.Message.Contains('~')) throw new Exception(ex.Message.Split('~')[0]);
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }

        private async Task<List<YarnReceiveChild>> GetChildsWithYarnControlNo(List<YarnReceiveChild> childs, string companyName, string challanNo)
        {
            if (childs.Count() > 0 && companyName.IsNotNullOrEmpty())
            {
                //CountId = Segment6ValueId

                #region find yarn control table name

                string comYarnControlTable = TableNames.YARN_CONTROL_NO;
                if (companyName == "EFL") comYarnControlTable = TableNames.YARN_CONTROL_NO_EFL;
                else if (companyName == "EKL") comYarnControlTable = TableNames.YARN_CONTROL_NO_EKL;
                else if (companyName == "ESL") comYarnControlTable = TableNames.YARN_CONTROL_NO_ESL;

                #endregion

                List<YarnReceiveChild> tempChilds = new List<YarnReceiveChild>();
                childs.ForEach(x =>
                {
                    YarnReceiveChild tempObj = CommonFunction.DeepClone(x);
                    tempChilds.Add(new YarnReceiveChild()
                    {
                        ChildID = tempObj.ChildID,
                        ShadeCode = tempObj.ShadeCode
                    });
                    x.ShadeCode = x.ShadeCode == "" 
                               || x.ShadeCode == null 
                               || x.ShadeCode.ToLower() == "n/a" 
                               || x.ShadeCode.ToLower() == "empty" ? "N/A" : x.ShadeCode;
                });

                if (comYarnControlTable.IsNotNullOrEmpty())
                {
                    List<YarnReceiveChild> yControlList = new List<YarnReceiveChild>();
                    childs.ForEach(x =>
                    {
                        if (x.YarnControlNo.IsNotNullOrEmpty())
                        {
                            x.YarnControlNo = companyName[1] + x.YarnControlNo.Remove(0, 1);

                            int index = yControlList.FindIndex(y => y.YarnControlNo == x.YarnControlNo);
                            if (index < 0)
                            {

                                yControlList.Add(new YarnReceiveChild()
                                {
                                    Segment6ValueId = x.Segment6ValueId, //Count
                                    ShadeCode = x.ShadeCode,
                                    PhysicalCount = x.PhysicalCount,
                                    LotNo = x.LotNo,
                                    YarnControlNo = x.YarnControlNo
                                });
                            }
                        }
                    });

                    var distinctList = childs
                                    .Select(m => new { m.Segment6ValueId, m.ShadeCode, m.PhysicalCount, m.LotNo })
                                    .Distinct()
                                    .ToList();

                    char comName = companyName[1];
                    foreach (var item in distinctList)
                    {
                        string yarnControlNo = "";
                        int index = yControlList.FindIndex(y => y.Segment6ValueId == item.Segment6ValueId && y.ShadeCode == item.ShadeCode && y.PhysicalCount == item.PhysicalCount && y.LotNo == item.LotNo);
                        if (index > -1)
                        {
                            yarnControlNo = yControlList[index].YarnControlNo;
                        }
                        else
                        {
                            yarnControlNo = await _service.GetMaxNoAsync(comYarnControlTable, 1, RepeatAfterEnum.EveryYear);
                            string yearPart = yarnControlNo.Substring(2, 2);
                            string numberPart = yarnControlNo.Substring(8, 5);
                            yarnControlNo = comName + yearPart + numberPart;

                            yControlList.Add(new YarnReceiveChild()
                            {
                                Segment6ValueId = item.Segment6ValueId, //Count
                                ShadeCode = item.ShadeCode,
                                PhysicalCount = item.PhysicalCount,
                                LotNo = item.LotNo
                            });
                        }

                        childs.Where(x => x.Segment6ValueId == item.Segment6ValueId && x.ShadeCode == item.ShadeCode && x.PhysicalCount == item.PhysicalCount && x.LotNo == item.LotNo).ToList().ForEach(x =>
                        {
                            x.YarnControlNo = yarnControlNo;
                            var tempObj = tempChilds.FirstOrDefault(y => y.ChildID == x.ChildID);
                            if (tempObj.IsNotNull()) x.ShadeCode = tempObj.ShadeCode;
                        });
                    }
                }
            }
            return childs;
        }
        public async Task<YarnReceiveChild> GetReceiveChild(int childId)
        {
            var sql = $@"
            ;Select * From YarnReceiveChild Where ChildID = {childId}";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                YarnReceiveChild data = await records.ReadFirstOrDefaultAsync<YarnReceiveChild>();
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
        public async Task<List<YarnReceiveChild>> GetReceiveChilds(string childIds)
        {
            var sql = $@"
            ;Select * From YarnReceiveChild Where ChildID IN ({childIds})";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                List<YarnReceiveChild> datas = records.Read<YarnReceiveChild>().ToList();
                return datas;
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
        public async Task UpdateChildAsync(YarnReceiveChild entityChild)
        {
            SqlTransaction transaction = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                await _service.SaveSingleAsync(entityChild, transaction);
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
    }
}
