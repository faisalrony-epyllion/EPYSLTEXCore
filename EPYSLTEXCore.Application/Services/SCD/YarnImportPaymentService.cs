using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.Tex.SCD;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using EPYSLTEXCore.Interfaces.Services;
using System.Data.Entity;
using Microsoft.Data.SqlClient;

namespace EPYSLTEX.Infrastructure.Services
{

    internal class YarnImportPaymentService : IYarnImportPaymentService
    {
        private readonly IDapperCRUDService<ImportInvoicePaymentMaster> _service;

 
        private readonly SqlConnection _connection;
        public YarnImportPaymentService(IDapperCRUDService<ImportInvoicePaymentMaster> service
           )
        {
            _service = service;
            _service.Connection = _service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
        }

        public async Task<List<ImportInvoicePaymentMaster>> GetPagedAsync(Status status, bool isCDAPage, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By BankRefNumber Desc" : paginationInfo.OrderBy;
            var sqltemp = string.Empty;
            var sqltemp1 = string.Empty;
            var sql = string.Empty;
            if (isCDAPage == true)
            {
                sqltemp = " And CM.IsCDA = 1";
                sqltemp1 = "Where IIPM.IsCDA = 1";
            }
            else
            {
                sqltemp = " And CM.IsCDA = 0";
                sqltemp1 = "Where IIPM.IsCDA = 0";
            }

            if (status == Status.Pending)
            {
                sql += $@"
                ;With IInvoice As(
					Select 
					CM.BankRefNumber, LC.CompanyID, CustomerName = CE.CompanyName , CM.SupplierId, SupplierName = C.ShortName,
					LC.PaymentBankID, PaymentBank = BB.BranchName,  LC.IssueBankID, IssueBank= IssueBank.BranchName,
					CM.BankAcceptDate, CM.MaturityDate,CIValue = AValue.AcceptedValue,
					CM.Acceptance,CM.BankAccept
					From YarnCIMaster CM
					INNER JOIN YarnLCMaster LC ON LC.LCID = CM.LCID
					INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = LC.CompanyID
					INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = CM.SupplierID
					LEFT JOIN {DbNames.EPYSL}..BankBranch BB ON BB.BankBranchID = LC.PaymentBankID
					LEFT JOIN {DbNames.EPYSL}..BankBranch CB_IssueBank ON CB_IssueBank.BankBranchID = LC.IssueBankID
					LEFT JOIN {DbNames.EPYSL}..BankBranch IssueBank ON IssueBank.BankBranchID = CB_IssueBank.BankBranchID
					outer Apply(
								Select AcceptedValue = IsNull(Sum(IsNull(CIValue,0)),0) From YarnCIMaster 
								Where BankRefNumber = CM.BankRefNumber And CompanyID = LC.CompanyID And SupplierId = CM.SupplierId
					)AValue
					--Where IsNull(CM.BankRefNumber,'') != '' And CM.Acceptance=1 --And CM.BankAccept = 1
					Where CM.Acceptance=1 And IsNull(CM.BankRefNumber,'') != '' {sqltemp}

	
				),FinalList As(
					Select IIPMasterID = 0,IIPMasterNo = '',
					II.BankRefNumber, II.CompanyID, II.CustomerName, II.SupplierId, II.SupplierName,
					II.PaymentBankID, II.PaymentBank,  II.IssueBankID, 
					II.BankAcceptDate, II.MaturityDate, II.CIValue
					From IInvoice II
					--Where II.Acceptance=1
					Group By II.BankRefNumber, II.CompanyID, II.CustomerName, II.SupplierId, II.SupplierName,
					II.PaymentBankID, II.PaymentBank,  II.IssueBankID,
					II.BankAcceptDate, II.MaturityDate, II.CIValue
				)
				SELECT *,Count(*) Over() TotalRows FROM FinalList";

                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "ORDER BY BankRefNumber DESC" : paginationInfo.OrderBy;
            }
            else if (status == Status.Edit)
            {
                sql += $@"
                    ;With IInvoice As(
	                    SELECT
	                    IIPM.IIPMasterNo,IIPM.IIPMasterID,
	                    II.BankRefNumber, IIPM.CompanyID, CustomerName = CE.CompanyName , II.SupplierId, SupplierName = C.ShortName,
	                    LCM.PaymentBankID, PaymentBank = BB.BranchName,  LCM.IssueBankID, IssueBank= IssueBank.BranchName,
	                    II.BankAcceptDate, II.MaturityDate,CIValue = AValue.AcceptedValue,
	                    II.Acceptance,II.BankAccept,
	                    PaymentedValue = isnull((select sum(isnull(PaymentValue,0)) from ImportInvoicePaymentChild where IIPMasterID <> IIPM.IIPMasterID),0)
	                    FROM ImportInvoicePaymentMaster IIPM
	                    INNER JOIN ImportInvoicePaymentChild IIPC on IIPC.IIPMasterID = IIPM.IIPMasterID
	                    INNER JOIN YarnCIMaster II on II.CIID = IIPC.InvoiceID
	                    INNER JOIN YarnLCMaster LCM ON LCM.LCID = II.LCID
	                    INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = LCM.CompanyID
	                    INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = II.SupplierID
	                    LEFT JOIN {DbNames.EPYSL}..BankBranch BB ON BB.BankBranchID = LCM.PaymentBankID
	                    LEFT JOIN {DbNames.EPYSL}..BankBranch CB_IssueBank ON CB_IssueBank.BankBranchID = LCM.IssueBankID
	                    LEFT JOIN {DbNames.EPYSL}..BankBranch IssueBank ON IssueBank.BankBranchID = CB_IssueBank.BankBranchID
	                    Outer Apply(
		                    Select AcceptedValue = IsNull(Sum(IsNull(CIValue,0)),0) From YarnCIMaster Where BankRefNumber = II.BankRefNumber
	                    )AValue
                        {sqltemp1}
                    ),FinalList As(
	                    Select 
	                    II.IIPMasterNo,II.IIPMasterID,
	                    II.BankRefNumber, II.CompanyID, II.CustomerName, II.SupplierId, II.SupplierName,
	                    II.PaymentBankID, II.PaymentBank,  II.IssueBankID, 
	                    II.BankAcceptDate, II.MaturityDate, II.CIValue,PaymentedValue= SUM(ISNULL(II.PaymentedValue,0))
	                    From IInvoice II
	                    --Where II.Acceptance=1
	                    Group By II.IIPMasterNo,II.IIPMasterID,II.BankRefNumber, II.CompanyID, II.CustomerName, II.SupplierId, II.SupplierName,
	                    II.PaymentBankID, II.PaymentBank,  II.IssueBankID,
	                    II.BankAcceptDate, II.MaturityDate, II.CIValue
                    )
                    SELECT *,Count(*) Over() TotalRows FROM FinalList";
                orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "ORDER BY BankRefNumber DESC" : paginationInfo.OrderBy;
            }
            return await _service.GetDataAsync<ImportInvoicePaymentMaster>(sql);
        }
        public async Task<ImportInvoicePaymentMaster> GetNewAsync(string BankRefNumber, int CompanyId, int SupplierId,bool isCDAPage)
        {
            string sqlTemp = string.Empty;
            var sql = string.Empty;
            if (isCDAPage == true)
            {
                sqlTemp = " And CM.IsCDA = 1";
            }
            else
            {
                sqlTemp = " And CM.IsCDA = 0";
            }
            sql += $@"
            ;With CIInfo As(
                Select 
                CM.BankRefNumber,CM.CompanyId,CM.SupplierId,CustomerName = CE.CompanyName,SupplierName = C.ShortName, 
                LC.PaymentBankID, PaymentBank = BB.BranchName,CM.BankAcceptDate,CM.MaturityDate,TotalAcceptedValue = Sum(IsNull(CM.CIValue,0))  
                From YarnCIMaster CM
                INNER JOIN YarnLCMaster LC ON LC.LCID = CM.LCID
                INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = LC.CompanyID
                INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = CM.SupplierID
                LEFT JOIN {DbNames.EPYSL}..BankBranch BB ON BB.BankBranchID = LC.PaymentBankID
                Where CM.BankRefNumber = '{BankRefNumber}' And CM.CompanyId = {CompanyId} And CM.SupplierId = {SupplierId} {sqlTemp}
                Group By CM.BankRefNumber,CM.CompanyId,CM.SupplierId,CE.CompanyName,C.ShortName, 
                LC.PaymentBankID, BB.BranchName,CM.BankAcceptDate,CM.MaturityDate
            )
            Select 
            MM.*
            From CIInfo MM;

            ----Childs
            Select 
            IIPChildID = 0, BankRefNo = CM.BankRefNumber,InvoiceID = CM.CIID,
            InvoiceNo = CM.CINo, InvoiceDate = CM.CIDate,InvoiceValue = CM.CIValue, 
            PaymentValue = 0.0, PaymentedValue=0.0, BalanceAmount=0.0
            FROM YarnCiMaster CM
            Where CM.BankRefNumber ='{BankRefNumber}' {sqlTemp}

            ----Details
            Select 
            HH.SGHeadID,HH.SGHeadName,HH.SGHeadDisplayName,HH.CTCategoryID,HH.DHeadNeed,SHeadNeed,MM.CategoryName
            From {DbNames.EPYSL}..CommercialSourceGroupHead HH
            Inner Join {DbNames.EPYSL}..CommercialTransactionCategory MM on MM.CTCategoryID = HH.CTCategoryID
            Where MM.CategoryName in ('Import Payment Source','Import Lc Charge','Import Payment Liability');

            Select CSH.SHeadID AS id, CSH.SHeadName AS text
            From {DbNames.EPYSL}..CommercialSourceHeadSetup CSHS 
            Inner Join {DbNames.EPYSL}..CommercialSourceHead CSH on CSHS.SHeadID  = CSH.SHeadID
            --where  CSHS.CompanyID = and CSHS.BankBranchID =and CSHS.SGHeadID = and CSHS.CTCategoryID =
            Group By  CSH.SHeadID,CSH.SHeadName;

            Select CSH.SHeadID  AS id, CSH.SHeadName AS text
            From {DbNames.EPYSL}..CommercialSourceHeadSetup CSHS 
            Inner Join {DbNames.EPYSL}..CommercialSourceHead CSH on CSHS.SHeadID  = CSH.SHeadID
            Inner Join {DbNames.EPYSL}..CommercialTransactionCategory CT on CT.CTCategoryID = CSHS.CTCategoryID
            --where  CSHS.CompanyID = {0} and CSHS.BankBranchID ={1}  and CSHS.SGHeadID = {2} and CT.CategoryName='Sources'
            Where CT.CategoryName='Sources'
            Group By  CSH.SHeadID,CSH.SHeadName;
            ";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                ImportInvoicePaymentMaster data = await records.ReadFirstOrDefaultAsync<ImportInvoicePaymentMaster>();
                Guard.Against.NullObject(data);
                data.IPChilds = records.Read<ImportInvoicePaymentChild>().ToList();
                data.IPDetails = records.Read<ImportInvoicePaymentDetails>().ToList();
                data.HeadDescriptionList = records.Read<Select2OptionModel>().ToList();
                data.SHeadNameList = records.Read<Select2OptionModel>().ToList();

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
        public async Task SaveAsync(ImportInvoicePaymentMaster entity, EntityState entityState)
        {
            SqlTransaction transaction = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                switch (entityState)
                {
                    case EntityState.Added:
                        entity = await AddManyAsync(entity);
                        break;

                    case EntityState.Modified:
                        entity = UpdateMany(entity);
                        break;

                    default:
                        break;
                }

                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.IPChilds, transaction);
                List<ImportInvoicePaymentDetails> SubIPDetails = new List<ImportInvoicePaymentDetails>();
                if (entityState == EntityState.Modified)
                {
                    SubIPDetails.AddRange(entity.IPDetails);

                }
                else
                {
                    entity.IPDetails.ForEach(x =>
                       SubIPDetails.AddRange(x.IPDetailSub)
                     );
                }

                await _service.SaveAsync(SubIPDetails, transaction);

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
        public async Task<ImportInvoicePaymentMaster> GetMultiDetailsAsync(string IIPMasterID, bool isCDAPage)
        {
            string sqlTemp = string.Empty;
            if (isCDAPage == true)
            {
                sqlTemp = " And PM.IsCDA = 1";
            }
            else
            {
                sqlTemp = " And PM.IsCDA = 0";
            }
            string query =
                $@"-- Master Data
                Select PM.*
                From ImportInvoicePaymentMaster PM
                Where PM.IIPMasterID = '{IIPMasterID}' {sqlTemp};

                --  Child Data
                Select PC.*
                From ImportInvoicePaymentMaster PM
                Inner Join ImportInvoicePaymentChild PC On PC.IIPMasterID = PM.IIPMasterID 
                Where PM.IIPMasterID = '{IIPMasterID}' {sqlTemp};

                --  Detail Data
                Select PC.*
                From ImportInvoicePaymentMaster PM
                Inner Join ImportInvoicePaymentDetails PC On PC.IIPMasterID = PM.IIPMasterID 
                Where PM.IIPMasterID =  {IIPMasterID} {sqlTemp};";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                ImportInvoicePaymentMaster data = await records.ReadFirstOrDefaultAsync<ImportInvoicePaymentMaster>();
                Guard.Against.NullObject(data);
                data.IPChilds = records.Read<ImportInvoicePaymentChild>().ToList();
                data.IPDetails = records.Read<ImportInvoicePaymentDetails>().ToList();

                data.IPChilds = data.IPChilds.Where(x => x.IIPMasterID == data.IIPMasterID).ToList();
                data.IPDetails = data.IPDetails.Where(x => x.IIPMasterID == data.IIPMasterID).ToList();

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _connection.Close();
            }
        }
        private async Task<ImportInvoicePaymentMaster> AddManyAsync(ImportInvoicePaymentMaster entities)
        {
            int nIIPMasterID = await _service.GetMaxIdAsync(TableNames.Import_Invoice_Payment_Master, 1);
            int nIIPMasterNo = await _service.GetMaxIdAsync(TableNames.Import_Invoice_Payment_IIPMasterNo, 1);
            int maxIIPChildID = await _service.GetMaxIdAsync(TableNames.Import_Invoice_Payment_Child, entities.IPChilds.Count);
            int maxIIPDetailsID = await _service.GetMaxIdAsync(TableNames.Import_Invoice_Payment_Details, entities.IPDetails.Sum(x => x.IPDetailSub.Count));

            int sIIPMasterNo = nIIPMasterNo++;

            entities.IIPMasterID = nIIPMasterID++;
            entities.IIPMasterNo = sIIPMasterNo.ToString();
            entities.EntityState = EntityState.Added;
            foreach (ImportInvoicePaymentChild oItem in entities.IPChilds)
            {
                oItem.IIPChildID = maxIIPChildID++;
                oItem.IIPMasterID = entities.IIPMasterID;
            }
            entities.IPDetails.ToList().ForEach(entity =>
            {

                foreach (ImportInvoicePaymentDetails oItem in entity.IPDetailSub)
                {
                    oItem.IIPDetailsID = maxIIPDetailsID++;
                    oItem.IIPMasterID = entities.IIPMasterID;
                }
            });


            return entities;
        }
        private ImportInvoicePaymentMaster UpdateMany(ImportInvoicePaymentMaster entities)
        {
            int nIIPMasterID = 0; int nIIPMasterNo = 0;
            int maxIIPChildID = _service.GetMaxId(TableNames.Import_Invoice_Payment_Child, entities.IPChilds.Where(x => x.EntityState == EntityState.Added).Count());
            int maxIIPDetailsID = _service.GetMaxId(TableNames.Import_Invoice_Payment_Details, entities.IPDetails.Sum(x => x.IPDetailSub.Where(y => y.EntityState == EntityState.Added).Count()));

            switch (entities.EntityState)
            {
                case EntityState.Added:
                    nIIPMasterID = _service.GetMaxId(TableNames.Import_Invoice_Payment_Master, 1);
                    nIIPMasterNo = _service.GetMaxId(TableNames.Import_Invoice_Payment_IIPMasterNo, 1);
                    entities.IIPMasterID = nIIPMasterID++;
                    int sIIPMasterNo = nIIPMasterNo++;
                    entities.IIPMasterNo = sIIPMasterNo.ToString();
                    entities.EntityState = EntityState.Added;
                    List<ImportInvoicePaymentChild> tempAddedChilds = entities.IPChilds.FindAll(x => x.EntityState == EntityState.Added);
                    foreach (ImportInvoicePaymentChild oItem in tempAddedChilds)
                    {
                        oItem.IIPChildID = maxIIPChildID++;
                        oItem.IIPMasterID = entities.IIPMasterID;
                    }
                    entities.IPDetails.ToList().ForEach(entity =>
                    {
                        List<ImportInvoicePaymentDetails> tempAddedChildDetails = entity.IPDetailSub.FindAll(x => x.EntityState == EntityState.Added);
                        foreach (ImportInvoicePaymentDetails oItemDetail in tempAddedChildDetails)
                        {
                            oItemDetail.IIPDetailsID = maxIIPDetailsID++;
                            oItemDetail.IIPMasterID = entities.IIPMasterID;
                        }
                    });
                    break;

                case EntityState.Modified:
                    List<ImportInvoicePaymentChild> addedChilds = entities.IPChilds.FindAll(x => x.EntityState == EntityState.Added);
                    foreach (ImportInvoicePaymentChild oItem in addedChilds)
                    {
                        oItem.IIPChildID = maxIIPChildID++;
                        oItem.IIPMasterID = entities.IIPMasterID;
                    }
                    entities.IPDetails.ToList().ForEach(entity =>
                    {
                        List<ImportInvoicePaymentDetails> addedChildDetails = entity.IPDetailSub.FindAll(x => x.EntityState == EntityState.Added);
                        foreach (ImportInvoicePaymentDetails oItemDetail in addedChildDetails)
                        {
                            oItemDetail.IIPDetailsID = maxIIPDetailsID++;
                            oItemDetail.IIPMasterID = entities.IIPMasterID;
                        }
                    });
                    break;

                default:
                    break;

            }
            return entities;
        }

        public async Task<ImportInvoicePaymentMaster> GetEditAsync(int IIPMasterID,bool isCDAPage)
        {
            var sql = string.Empty;
            string sqlTemp = string.Empty;
            if (isCDAPage == true)
            {
                sqlTemp = " And IM.IsCDA = 1";
            }
            else
            {
                sqlTemp = " And IM.IsCDA = 0";
            }
            sql += $@"
            ;With CIInfo As(
	            Select IM.IIPMasterID,IM.PaymentDate,CM.BankRefNumber,CM.CompanyId,CM.SupplierId,CustomerName = CE.CompanyName,SupplierName = C.ShortName,
	            LCM.PaymentBankID, PaymentBank = BB.BranchName,CM.BankAcceptDate,CM.MaturityDate,TotalAcceptedValue = Sum(IsNull(CM.CIValue,0)),
	            TotalPaymentedValue = Sum(IsNull(IIPC.PaymentValue,0))
	            From ImportInvoicePaymentMaster IM
	            INNER JOIN ImportInvoicePaymentChild IIPC on IIPC.IIPMasterID = IM.IIPMasterID
	            INNER JOIN YarnCIMaster CM on CM.CIID = IIPC.InvoiceID
	            INNER JOIN YarnLCMaster LCM ON LCM.LCID = CM.LCID
	            INNER JOIN {DbNames.EPYSL}..CompanyEntity CE ON CE.CompanyID = LCM.CompanyID
	            INNER JOIN {DbNames.EPYSL}..Contacts C ON C.ContactID = CM.SupplierID
	            LEFT JOIN {DbNames.EPYSL}..BankBranch BB ON BB.BankBranchID = LCM.PaymentBankID
	            LEFT JOIN {DbNames.EPYSL}..BankBranch CB_IssueBank ON CB_IssueBank.BankBranchID = LCM.IssueBankID
	            LEFT JOIN {DbNames.EPYSL}..BankBranch IssueBank ON IssueBank.BankBranchID = CB_IssueBank.BankBranchID
	            Where IM.IIPMasterID = {IIPMasterID} {sqlTemp}
	            Group By IM.IIPMasterID,IM.PaymentDate,CM.BankRefNumber,CM.CompanyId,CM.SupplierId,CE.CompanyName,C.ShortName,
	            LCM.PaymentBankID, BB.BranchName,CM.BankAcceptDate,CM.MaturityDate

            )
            Select 
            MM.*,
            TotalPaidValue = isnull((select sum(isnull(PaymentValue,0)) from ImportInvoicePaymentChild where IIPMasterID <> MM.IIPMasterID),0)
            From CIInfo MM;

            ----Childs
            ;With IMC As(
	            Select 
	            IMC.IIPChildID,BankRefNo = CM.BankRefNumber,IMC.InvoiceID,InvoiceNo = CM.CINo,InvoiceDate = CM.CIDate,
	            IMC.InvoiceValue,IMC.PaymentValue,
	            PaymentedValue = ISNULL((select sum(ISNULL(PaymentValue,0)) from ImportInvoicePaymentChild where IIPMasterID <> IM.IIPMasterID),0)
	            From ImportInvoicePaymentChild IMC
	            INNER JOIN ImportInvoicePaymentMaster IM On IM.IIPMasterID = IMC.IIPMasterID
	            INNER JOIN YarnCIMaster CM on CM.CIID = IMC.InvoiceID
	            Where IM.IIPMasterID = {IIPMasterID} {sqlTemp}
            )
            Select 
            IMC.*,
            BalanceAmount = ISNULL(IMC.InvoiceValue,0) -(ISNULL(IMC.PaymentValue,0) + ISNULL(IMC.PaymentedValue,0))
            From IMC

            ----Details
            Select 
            HH.SGHeadID,HH.SGHeadName,HH.SGHeadDisplayName,HH.CTCategoryID,HH.DHeadNeed,SHeadNeed,MM.CategoryName
            From {DbNames.EPYSL}..CommercialSourceGroupHead HH
            Inner Join {DbNames.EPYSL}..CommercialTransactionCategory MM on MM.CTCategoryID = HH.CTCategoryID
            Where MM.CategoryName in ('Import Payment Source','Import Lc Charge','Import Payment Liability');
            
            Select
            IMD.IIPDetailsID, IMD.SGHeadID, IMD.DHeadID,IMD.CalculationOn,IMD.ValueInFC,IMD.ValueInLC,IMD.CurConvRate,IMD.SHeadID
            From ImportInvoicePaymentDetails IMD
            INNER JOIN ImportInvoicePaymentMaster IM On IM.IIPMasterID = IMD.IIPMasterID
            Where IM.IIPMasterID = {IIPMasterID} {sqlTemp};


            Select CSH.SHeadID AS id, CSH.SHeadName AS text
            From {DbNames.EPYSL}..CommercialSourceHeadSetup CSHS 
            Inner Join {DbNames.EPYSL}..CommercialSourceHead CSH on CSHS.SHeadID  = CSH.SHeadID
            --where  CSHS.CompanyID = and CSHS.BankBranchID =and CSHS.SGHeadID = and CSHS.CTCategoryID =
            Group By  CSH.SHeadID,CSH.SHeadName;

            Select CSH.SHeadID  AS id, CSH.SHeadName AS text
            From {DbNames.EPYSL}..CommercialSourceHeadSetup CSHS 
            Inner Join {DbNames.EPYSL}..CommercialSourceHead CSH on CSHS.SHeadID  = CSH.SHeadID
            Inner Join {DbNames.EPYSL}..CommercialTransactionCategory CT on CT.CTCategoryID = CSHS.CTCategoryID
            --where  CSHS.CompanyID = {0} and CSHS.BankBranchID ={1}  and CSHS.SGHeadID = {2} and CT.CategoryName='Sources'
            Where CT.CategoryName='Sources'
            Group By  CSH.SHeadID,CSH.SHeadName;
            ";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                ImportInvoicePaymentMaster data = await records.ReadFirstOrDefaultAsync<ImportInvoicePaymentMaster>();
                Guard.Against.NullObject(data);
                data.IPChilds = records.Read<ImportInvoicePaymentChild>().ToList();
                data.IPDetails = records.Read<ImportInvoicePaymentDetails>().ToList();
                List<ImportInvoicePaymentDetails> DetailSubList = new List<ImportInvoicePaymentDetails>();
                DetailSubList = records.Read<ImportInvoicePaymentDetails>().ToList();
                data.IPDetails.ToList().ForEach(entity =>
                {
                    entity.IPDetailSub.AddRange(DetailSubList.FindAll(x => x.SGHeadID == entity.SGHeadID).ToList());
                });
                data.HeadDescriptionList = records.Read<Select2OptionModel>().ToList();
                data.SHeadNameList = records.Read<Select2OptionModel>().ToList();

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

    }
}
