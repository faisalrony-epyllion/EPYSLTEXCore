using Dapper;
using EPYSLTEX.Core.Statics;
using System.Data;
using System.Data.Entity;
using Microsoft.Data.SqlClient;
using EPYSLTEXCore.Application.Interfaces.RND;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Entities;

namespace EPYSLTEX.Infrastructure.Services
{
    internal class LabTestResultService : ILabTestResultService
    {
        private readonly IDapperCRUDService<LabTestRequisitionMaster> _service;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _gmtConnection;

        public LabTestResultService(IDapperCRUDService<LabTestRequisitionMaster> service)
        {
            _service = service;
            _service.Connection = _service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
            _gmtConnection = service.GetConnection(AppConstants.GMT_CONNECTION);
        }

        public async Task<List<LabTestRequisitionMaster>> GetPagedAsync(Status status, int offset = 0, int limit = 10, string filterBy = null, string orderBy = null)
        {
            string tempGuid = CommonFunction.GetNewGuid();
            bool isUsedTemp = false;
            var sql = string.Empty;

            if (status == Status.Completed)
            {
                sql += $@"
                WITH LTB As(
                    SELECT LTReqMasterID 
                    FROM {TableNames.LAB_TEST_REQUISITION_BUYER}  
                    WHERE IsSend = 0
                    Group By LTReqMasterID
                ), M AS (
                    SELECT	LTRM.* 
                    FROM {TableNames.LAB_TEST_REQUISITION_MASTER}  LTRM 
                    Inner Join LTB On LTB.LTReqMasterID = LTRM.LTReqMasterID
                    WHERE LTRM.IsApproved = 1 AND LTRM.IsAcknowledge = 1
                ),
                TNI AS
                (
	                SELECT LTR.LTReqMasterID, LTR.TestNatureID, TN.TestNatureName
	                FROM {TableNames.LAB_TEST_REQUISITION_BUYER_PARAMETER} LTR
	                INNER JOIN M ON M.LTReqMasterID = LTR.LTReqMasterID
	                LEFT JOIN TestNature TN ON TN.TestNatureID = LTR.TestNatureID
	                GROUP BY LTR.LTReqMasterID, LTR.TestNatureID, TN.TestNatureName
                ),
                FinalList AS
                (
	                SELECT M.LTReqMasterID, M.ReqNo, M.ReqDate, M.DBatchID, M.ConceptID, M.BookingID, M.ExportOrderID, CM.BuyerID, CM.BuyerTeamID, M.ItemMasterID,
	                M.ColorID, M.FabricQty, M.UnitID, M.Remarks,M.[FileName], M.ImagePath, M.PreviewTemplate, Batch.DBatchNo, Batch.DBatchDate, CM.ConceptNo, BM.BookingNo, COL.SegmentValue ColorName,
	                M.IsApproved,M.IsAcknowledge,
	                BuyerName = CASE WHEN CM.BuyerID > 0 THEN CTO.ShortName ELSE CASE WHEN M.ConceptID > 0 THEN 'R&D' ELSE '-' END END, 
                    BuyerTeamName = CASE WHEN CM.BuyerTeamID > 0 THEN CCT.TeamName ELSE CASE WHEN M.ConceptID > 0 THEN 'R&D' ELSE '-' END END,
                    LabTestStatus = CASE WHEN M.IsProduction = 1 THEN 'Production' ELSE '' END,
                    TNI.TestNatureName
	                FROM M
	                INNER JOIN {TableNames.DYEING_BATCH_MASTER} Batch ON Batch.DBatchID = M.DBatchID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
	                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} CM ON CM.ConceptID = M.ConceptID
	                LEFT JOIN {DbNames.EPYSL}..BookingMaster BM ON BM.BookingID = M.BookingID
	                LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = CM.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = M.BuyerTeamID
                    LEFT JOIN TNI ON TNI.LTReqMasterID = M.LTReqMasterID
                )
                SELECT * INTO #TempData{tempGuid} FROM FinalList
				SELECT *,Count(*) Over() TotalRows FROM #TempData{tempGuid}";

                isUsedTemp = true;


                orderBy = string.IsNullOrEmpty(orderBy) ? "ORDER BY LTReqMasterID DESC" : orderBy;
            }
            else if (status == Status.Proposed)
            {
                sql += $@"
                WITH LTB As(
                    SELECT LTReqMasterID 
                    FROM {TableNames.LAB_TEST_REQUISITION_BUYER}   
                    WHERE IsSend = 1 AND IsApproved = 0 AND IsAcknowledge = 0
                    Group By LTReqMasterID
                ), M AS (
                    SELECT	LTRM.* 
                    FROM {TableNames.LAB_TEST_REQUISITION_MASTER} LTRM 
                    Inner Join LTB On LTB.LTReqMasterID = LTRM.LTReqMasterID
                    WHERE LTRM.IsApproved = 1 AND LTRM.IsAcknowledge = 1
                ),
                TNI AS
                (
	                SELECT LTR.LTReqMasterID, LTR.TestNatureID, TN.TestNatureName
	                FROM {TableNames.LAB_TEST_REQUISITION_BUYER_PARAMETER} LTR
	                INNER JOIN M ON M.LTReqMasterID = LTR.LTReqMasterID
	                LEFT JOIN TestNature TN ON TN.TestNatureID = LTR.TestNatureID
	                GROUP BY LTR.LTReqMasterID, LTR.TestNatureID, TN.TestNatureName
                ),
                FinalList AS (
	                SELECT M.LTReqMasterID, M.ReqNo, M.ReqDate, M.DBatchID, M.ConceptID, M.BookingID, M.ExportOrderID, CM.BuyerID, CM.BuyerTeamID, M.ItemMasterID,
	                M.ColorID, M.FabricQty, M.UnitID, M.Remarks,M.[FileName], M.ImagePath, M.PreviewTemplate, Batch.DBatchNo, Batch.DBatchDate, CM.ConceptNo, BM.BookingNo, COL.SegmentValue ColorName,
	                M.IsApproved,M.IsAcknowledge,
	                BuyerName = CASE WHEN CM.BuyerID > 0 THEN CTO.ShortName ELSE CASE WHEN M.ConceptID > 0 THEN 'R&D' ELSE '-' END END, 
                    BuyerTeamName = CASE WHEN CM.BuyerTeamID > 0 THEN CCT.TeamName ELSE CASE WHEN M.ConceptID > 0 THEN 'R&D' ELSE '-' END END,
                    LabTestStatus = CASE WHEN M.IsProduction = 1 THEN 'Production' ELSE '' END,
                    TNI.TestNatureName
	                FROM M
	                INNER JOIN {TableNames.DYEING_BATCH_MASTER} Batch ON Batch.DBatchID = M.DBatchID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
	                LEFT JOIN FreeConceptMaster CM ON CM.ConceptID = M.ConceptID
	                LEFT JOIN {DbNames.EPYSL}..BookingMaster BM ON BM.BookingID = M.BookingID
	                LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = CM.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = CM.BuyerTeamID
                    LEFT JOIN TNI ON TNI.LTReqMasterID = M.LTReqMasterID
                )
                SELECT * INTO #TempData{tempGuid} FROM FinalList
				SELECT *,Count(*) Over() TotalRows FROM #TempData{tempGuid}";

                isUsedTemp = true;

                orderBy = string.IsNullOrEmpty(orderBy) ? "ORDER BY LTReqMasterID DESC" : orderBy;
            }
            else if (status == Status.Approved)
            {
                sql += $@"
                WITH LTB As(
                    SELECT LTReqMasterID 
                    FROM {TableNames.LAB_TEST_REQUISITION_BUYER}   
                    WHERE IsSend = 1 AND IsApproved = 1 AND IsAcknowledge = 0
                    Group By LTReqMasterID
                ), M AS (
                    SELECT	LTRM.* 
                    FROM {TableNames.LAB_TEST_REQUISITION_MASTER} LTRM 
                    Inner Join LTB On LTB.LTReqMasterID = LTRM.LTReqMasterID
                    WHERE LTRM.IsApproved = 1 AND LTRM.IsAcknowledge = 1
                ),
                TNI AS
                (
	                SELECT LTR.LTReqMasterID, LTR.TestNatureID, TN.TestNatureName
	                FROM {TableNames.LAB_TEST_REQUISITION_BUYER_PARAMETER} LTR
	                INNER JOIN M ON M.LTReqMasterID = LTR.LTReqMasterID
	                LEFT JOIN TestNature TN ON TN.TestNatureID = LTR.TestNatureID
	                GROUP BY LTR.LTReqMasterID, LTR.TestNatureID, TN.TestNatureName
                ),
                FinalList AS
                (
	                SELECT M.LTReqMasterID, M.ReqNo, M.ReqDate, M.DBatchID, M.ConceptID, M.BookingID, M.ExportOrderID, CM.BuyerID, CM.BuyerTeamID, M.ItemMasterID,
	                M.ColorID, M.FabricQty, M.UnitID, M.Remarks,M.[FileName], M.ImagePath, M.PreviewTemplate, Batch.DBatchNo, Batch.DBatchDate, CM.ConceptNo, BM.BookingNo, COL.SegmentValue ColorName,
	                M.IsApproved,M.IsAcknowledge,
	                BuyerName = CASE WHEN CM.BuyerID > 0 THEN CTO.ShortName ELSE CASE WHEN M.ConceptID > 0 THEN 'R&D' ELSE '-' END END, 
                    BuyerTeamName = CASE WHEN CM.BuyerTeamID > 0 THEN CCT.TeamName ELSE CASE WHEN M.ConceptID > 0 THEN 'R&D' ELSE '-' END END,
                    LabTestStatus = CASE WHEN M.IsProduction = 1 THEN 'Production' ELSE '' END,
                    TNI.TestNatureName
	                FROM M
	                INNER JOIN {TableNames.DYEING_BATCH_MASTER} Batch ON Batch.DBatchID = M.DBatchID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
	                LEFT JOIN FreeConceptMaster CM ON CM.ConceptID = M.ConceptID
	                LEFT JOIN {DbNames.EPYSL}..BookingMaster BM ON BM.BookingID = M.BookingID
	                LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = CM.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = CM.BuyerTeamID
                    LEFT JOIN TNI ON TNI.LTReqMasterID = M.LTReqMasterID
                )
                SELECT * INTO #TempData{tempGuid} FROM FinalList
				SELECT *,Count(*) Over() TotalRows FROM #TempData{tempGuid}";

                isUsedTemp = true;

                orderBy = string.IsNullOrEmpty(orderBy) ? "ORDER BY LTReqMasterID DESC" : orderBy;
            }
            else if (status == Status.Acknowledge)
            {
                sql += $@"
                WITH LTB As(
                    SELECT LTReqMasterID 
                    FROM {TableNames.LAB_TEST_REQUISITION_BUYER}   
                    WHERE IsSend = 1 AND IsApproved = 1 AND IsAcknowledge = 1
                    Group By LTReqMasterID
                ), M AS (
                    SELECT	LTRM.* 
                    FROM {TableNames.LAB_TEST_REQUISITION_MASTER} LTRM 
                    Inner Join LTB On LTB.LTReqMasterID = LTRM.LTReqMasterID
                    WHERE LTRM.IsApproved = 1 AND LTRM.IsAcknowledge = 1
                ),
                TNI AS
                (
	                SELECT LTR.LTReqMasterID, LTR.TestNatureID, TN.TestNatureName
	                FROM {TableNames.LAB_TEST_REQUISITION_BUYER_PARAMETER} LTR
	                INNER JOIN M ON M.LTReqMasterID = LTR.LTReqMasterID
	                LEFT JOIN TestNature TN ON TN.TestNatureID = LTR.TestNatureID
	                GROUP BY LTR.LTReqMasterID, LTR.TestNatureID, TN.TestNatureName
                ),
                FinalList AS
                (
	                SELECT M.LTReqMasterID, M.ReqNo, M.ReqDate, M.DBatchID, M.ConceptID, M.BookingID, M.ExportOrderID, CM.BuyerID, CM.BuyerTeamID, M.ItemMasterID,
	                M.ColorID, M.FabricQty, M.UnitID, M.Remarks,M.[FileName], M.ImagePath, M.PreviewTemplate, Batch.DBatchNo, Batch.DBatchDate, CM.ConceptNo, BM.BookingNo, COL.SegmentValue ColorName,
	                M.IsApproved,M.IsAcknowledge,
	                BuyerName = CASE WHEN CM.BuyerID > 0 THEN CTO.ShortName ELSE CASE WHEN M.ConceptID > 0 THEN 'R&D' ELSE '-' END END, 
                    BuyerTeamName = CASE WHEN CM.BuyerTeamID > 0 THEN CCT.TeamName ELSE CASE WHEN M.ConceptID > 0 THEN 'R&D' ELSE '-' END END,
                    LabTestStatus = CASE WHEN M.IsProduction = 1 THEN 'Production' ELSE '' END,
                    TNI.TestNatureName
	                FROM M
	                INNER JOIN {TableNames.DYEING_BATCH_MASTER} Batch ON Batch.DBatchID = M.DBatchID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
	                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} CM ON CM.ConceptID = M.ConceptID
	                LEFT JOIN {DbNames.EPYSL}..BookingMaster BM ON BM.BookingID = M.BookingID
	                LEFT JOIN {DbNames.EPYSL}..Contacts CTO ON CTO.ContactID = CM.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = CM.BuyerTeamID
                    LEFT JOIN TNI ON TNI.LTReqMasterID = M.LTReqMasterID
                )
                SELECT * INTO #TempData{tempGuid} FROM FinalList
				SELECT *,Count(*) Over() TotalRows FROM #TempData{tempGuid}";

                isUsedTemp = true;

                orderBy = string.IsNullOrEmpty(orderBy) ? "ORDER BY LTReqMasterID DESC" : orderBy;
            }

            filterBy = string.IsNullOrEmpty(filterBy) ? string.Empty : "Where " + filterBy;
            var pageBy = string.Format(@"OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", offset, limit);

            sql += $@"{Environment.NewLine}{filterBy}{Environment.NewLine}{orderBy}{Environment.NewLine}{pageBy}";

            if (isUsedTemp)
            {
                sql += $@" DROP TABLE #TempData{tempGuid}";
            }

            return await _service.GetDataAsync<LabTestRequisitionMaster>(sql);
        }
        


        public async Task<LabTestRequisitionMaster> GetAsync(int id)
        {
            var sql = $@"
                    ;WITH M AS (
                        SELECT	* FROM {TableNames.LAB_TEST_REQUISITION_MASTER} WHERE LTReqMasterID = {id}
                    )
                    SELECT M.LTReqMasterID, M.ReqNo, M.ReqDate, M.DBatchID, M.ConceptID, M.BookingID, M.ExportOrderID, M.BuyerID, M.BuyerTeamID, M.ItemMasterID,
	                M.ColorID, M.FabricQty, M.UnitID, M.Remarks,M.[FileName], M.ImagePath, M.PreviewTemplate, Batch.DBatchNo, Batch.DBatchDate, CM.ConceptNo, BM.BookingNo, COL.SegmentValue ColorName,
	                M.IsApproved,M.IsAcknowledge,M.PerformanceCode,

	                M.LabTestServiceTypeID,M.IsRetest,M.KnittingUnitID,
	                COM.SegmentValue Composition,T.TechnicalName,MSC.SubClassName,
	                M.ContactPersonID, M.CareInstruction, E.EmployeeName ContactPersonName,

                    BuyerName = CASE WHEN CM.BuyerID > 0 THEN B.ShortName ELSE CASE WHEN M.ConceptID > 0 THEN 'R&D' ELSE '-' END END, 
                    BuyerTeamName = CASE WHEN CM.BuyerTeamID > 0 THEN CCT.TeamName ELSE CASE WHEN M.ConceptID > 0 THEN 'R&D' ELSE '-' END END

	                FROM M
	                INNER JOIN {TableNames.DYEING_BATCH_MASTER} Batch ON Batch.DBatchID = M.DBatchID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue COL ON COL.SegmentValueID = M.ColorID
	                LEFT JOIN {TableNames.RND_FREE_CONCEPT_MASTER} CM ON CM.ConceptID = M.ConceptID
	                LEFT JOIN {DbNames.EPYSL}..BookingMaster BM ON BM.BookingID = M.BookingID
	                LEFT JOIN {DbNames.EPYSL}..Contacts B ON B.ContactID = CM.BuyerID
	                LEFT JOIN {DbNames.EPYSL}..ContactCategoryTeam CCT ON CCT.CategoryTeamID = CM.BuyerTeamID
	                INNER JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = M.ContactPersonID
	                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue COM ON COM.SegmentValueID = CM.CompositionID
	                LEFT JOIN FabricTechnicalName T ON T.TechnicalNameId = CM.TechnicalNameId
	                LEFT JOIN KnittingMachineSubClass MSC ON MSC.SubClassID = CM.MCSubClassID
	                LEFT JOIN LaundryCareLableInstruction CI ON CI.BuyerID = M.BuyerID;

                    -----childs
                    ;WITH M AS (
                        SELECT	* FROM {TableNames.LAB_TEST_REQUISITION_BUYER}   WHERE LTReqMasterID = {id}
                    )
                    SELECT M.LTReqBuyerID, M.BuyerID, M.LTReqMasterID, Contacts.Name BuyerName, M.IsPass, M.Remarks, M.IsApproved, M.IsAcknowledge, M.IsSend
                    FROM M
                    INNER JOIN {DbNames.EPYSL}..Contacts ON Contacts.ContactID = M.BuyerID;

                    ----Buyer Parameter Group
                    ;WITH M AS (
                        SELECT	* FROM {TableNames.LAB_TEST_REQUISITION_BUYER_PARAMETER} WHERE LTReqMasterID = {id}
                    )
                    SELECT  M.LTReqMasterID, M.LTReqBuyerID, M.BuyerID, M.BPID, 
                    BWP.TestName, BWP.Requirement,
                    TestMethod = ETV.ValueName, STRING_AGG(M.BPSubID, ',') SubIDs
                    FROM M
                    INNER JOIN LabTestBuyerWiseParameter_HK BWP ON BWP.BPID = M.BPID
                    INNER JOIN {DbNames.EPYSL}..EntityTypeValue ETV on ETV.ValueID = BWP.TestMethodID
                    INNER JOIN {DbNames.EPYSL}..Contacts ON Contacts.ContactID = M.BuyerID
					GROUP BY M.LTReqMasterID, M.LTReqBuyerID, M.BuyerID, M.BPID, 
                    BWP.TestName, BWP.Requirement, BWP.SeqNo, ETV.ValueName
                    Order By BWP.SeqNo;

                    ----Buyer Parameters
                    ;WITH M AS (
                        SELECT	* FROM {TableNames.LAB_TEST_REQUISITION_BUYER_PARAMETER} WHERE LTReqMasterID = {id}
                    )
                    SELECT M.LTReqBPID, M.LTReqMasterID, M.LTReqBuyerID, M.BuyerID, M.BPID, M.BPSubID,
					M.TestValue, M.TestValue1, M.Requirement, M.Requirement1, M.IsPass, M.Addition1, M.Addition2, M.AdditionalInfo, M.Remarks, M.IsParameterPass, M.ParameterStatus, M.ParameterRemarks,
					Contacts.Name BuyerName, BWP.TestName, TestMethod = ETV.ValueName,
					SUB.SubTestName, SUB.SubSubTestName, SUB.Requirement RefValueFrom, SUB.Requirement RefValueTo
                    FROM M
                    INNER JOIN LabTestBuyerWiseParameter_HK BWP ON BWP.BPID = M.BPID
                    INNER JOIN {DbNames.EPYSL}..EntityTypeValue ETV on ETV.ValueID = BWP.TestMethodID
                    INNER JOIN {DbNames.EPYSL}..Contacts ON Contacts.ContactID = M.BuyerID
					INNER JOIN LabTestBuyerWiseParameterSub_HK SUB ON SUB.BPSubID = M.BPSubID
                    Order By SUB.SeqNo;
                    -----files
                    ;WITH M AS (
                        SELECT	LTReqImageID, LTReqMasterID, FileName, ImagePath, PreviewTemplate, ImageGroup, ImageSubGroup, BPID FROM {TableNames.LABTEST_REQUISITION_IMAGE} WHERE LTReqMasterID = {id}
	
                    )
                    SELECT * FROM M
                    ORDER BY M.LTReqImageID;

                    ----Unit
                    {CommonQueries.GetUnit()};";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                LabTestRequisitionMaster data = records.Read<LabTestRequisitionMaster>().FirstOrDefault();
                data.LabTestRequisitionBuyers = records.Read<LabTestRequisitionBuyer>().ToList();
                data.LabTestRequisitionBuyers[0].LabTestRequisitionBuyerParameters = records.Read<LabTestRequisitionBuyerParameter>().ToList();
                List<LabTestRequisitionBuyerParameter> buyerParams = records.Read<LabTestRequisitionBuyerParameter>().ToList();

                data.LabTestRequisitionImages = records.Read<LabTestRequisitionImage>().ToList();

                data.UnitList = records.Read<Select2OptionModel>().ToList();
                if (data.LabTestRequisitionBuyers[0].BuyerID == 0)
                {
                    data.LabTestRequisitionBuyers[0].BuyerName = "Epyllion Standard";
                }

                data.LabTestRequisitionBuyers[0].LabTestRequisitionBuyerParameters.ForEach(x =>
                {
                    x.BuyerParameters = buyerParams.Where(c => c.BPID == x.BPID).ToList();
                });

                data.LabTestRequisitionBuyers.ForEach(x =>
                {
                    if (x.LabTestRequisitionBuyerParameters.Count() > 0)
                    {
                        var bpList = buyerParams.FirstOrDefault(c => c.LTReqBuyerID == x.LTReqBuyerID);
                        if (bpList.IsNotNull())
                        {
                            x.LabTestRequisitionBuyerParameters.ForEach(c =>
                            {
                                c.IsParameterPass = bpList.IsParameterPass;
                                c.ParameterStatus = bpList.ParameterStatus;
                                c.ParameterRemarks = bpList.ParameterRemarks;
                            });
                        }
                    }
                });

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

        public async Task<List<LabTestRequisitionBuyerParameter>> GetBuyerParameterByBuyerId(int id)
        {
            var sql = $@"
                    ;SELECT BWP.BPID, BWP.BuyerID, BWP.TestName ParameterName, BWP.RefValueFrom, BWP.RefValueTo, Contacts.Name BuyerName, TestMethod = ETV.ValueName
                    FROM LabTestBuyerWiseParameter_HK BWP
                    INNER JOIN {DbNames.EPYSL}..EntityTypeValue ETV on ETV.ValueID = BWP.TestMethodID
                    INNER JOIN {DbNames.EPYSL}..Contacts ON Contacts.ContactID = BWP.BuyerID
                    WHERE BWP.BuyerID = {id}
                    Order By BWP.SeqNo;";

            return await _service.GetDataAsync<LabTestRequisitionBuyerParameter>(sql);
        }

        public async Task<LabTestRequisitionMaster> GetAllByIDAsync(int id)
        {
            string sql = $@"
            ;Select * From {TableNames.LAB_TEST_REQUISITION_MASTER} Where LTReqMasterID = {id}

            ;Select * From {TableNames.LAB_TEST_REQUISITION_BUYER}   Where LTReqMasterID = {id}

            ;Select * From {TableNames.LAB_TEST_REQUISITION_BUYER_PARAMETER} Where LTReqMasterID = {id}
            
            ;Select * From {TableNames.LABTEST_REQUISITION_IMAGE} Where LTReqMasterID = {id}
            ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                LabTestRequisitionMaster data = records.Read<LabTestRequisitionMaster>().FirstOrDefault();
                data.LabTestRequisitionBuyers = records.Read<LabTestRequisitionBuyer>().ToList();
                data.LabTestRequisitionBuyerParameters = records.Read<LabTestRequisitionBuyerParameter>().ToList();
                data.LabTestRequisitionImages = records.Read<LabTestRequisitionImage>().ToList();
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

        public async Task SaveAsync(LabTestRequisitionMaster entity)
        {
            SqlTransaction transaction = null;
            SqlTransaction transactionGmt = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();

                await _gmtConnection.OpenAsync();
                transactionGmt = _gmtConnection.BeginTransaction();

                switch (entity.EntityState)
                {
                    case EntityState.Modified:
                        entity = await UpdateAsync(entity, transaction, _connection, transactionGmt, _gmtConnection);
                        break;

                    default:
                        break;
                }


                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.LabTestRequisitionBuyers, transaction);
                await _service.SaveAsync(entity.LabTestRequisitionImages, transaction);
                List<LabTestRequisitionBuyerParameter> buyerParmsList = new List<LabTestRequisitionBuyerParameter>();
                entity.LabTestRequisitionBuyers.ForEach(x => buyerParmsList.AddRange(x.LabTestRequisitionBuyerParameters));
                await _service.SaveAsync(buyerParmsList, transaction);

                transaction.Commit();
                transactionGmt.Commit();

                //#region Update TNA
                //if (entity.LTReqMasterID > 0)
                //{
                //    await UpdateBDSTNA_TestReportPlanAsync(entity.LTReqMasterID);
                //}
                //#endregion

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

        public async Task UpdateBDSTNA_TestReportPlanAsync(int LTReqMasterID)
        {
            await _service.ExecuteAsync("spUpdateBDSTNA_TestReportPlan", new { LTReqMasterID = LTReqMasterID }, 30, CommandType.StoredProcedure);
        }

        private async Task<LabTestRequisitionMaster> UpdateAsync(LabTestRequisitionMaster entity, SqlTransaction transaction, SqlConnection _connection, SqlTransaction transactionGmt, SqlConnection _gmtConnection)
        {
            var maxLabTestRequisitionBuyerId = await _service.GetMaxIdAsync(TableNames.LAB_TEST_REQUISITION_BUYER, entity.LabTestRequisitionBuyers.Where(x => x.EntityState == EntityState.Added).Count(), RepeatAfterEnum.NoRepeat, transactionGmt, _gmtConnection);
            var maxLabTestRequisitionBuyerParamId = await _service.GetMaxIdAsync(TableNames.LAB_TEST_REQUISITION_BUYER_PARAMETER, entity.LabTestRequisitionBuyers.Sum(x => x.LabTestRequisitionBuyerParameters.Where(y => y.EntityState == EntityState.Added).Count()), RepeatAfterEnum.NoRepeat, transactionGmt, _gmtConnection);

            foreach (var item in entity.LabTestRequisitionBuyers.ToList())
            {
                foreach (var child in item.LabTestRequisitionBuyerParameters.ToList())
                {
                    switch (child.EntityState)
                    {
                        case EntityState.Added:
                            child.LTReqBPID = maxLabTestRequisitionBuyerParamId++;
                            child.LTReqBuyerID = item.LTReqBuyerID;
                            child.LTReqMasterID = entity.LTReqMasterID;
                            break;

                        case EntityState.Unchanged:
                        case EntityState.Deleted:
                            child.EntityState = EntityState.Deleted;
                            break;

                        case EntityState.Modified:
                            child.EntityState = EntityState.Modified;
                            break;

                        default:
                            break;
                    }
                }
                switch (item.EntityState)
                {
                    case EntityState.Added:
                        item.LTReqBuyerID = maxLabTestRequisitionBuyerId++;
                        item.LTReqMasterID = entity.LTReqMasterID;
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

            var maxLabTestRequisitionImageId = await _service.GetMaxIdAsync(TableNames.LABTEST_REQUISITION_IMAGE, entity.LabTestRequisitionImages.Where(x => x.EntityState == EntityState.Added).Count(),RepeatAfterEnum.NoRepeat, transactionGmt, _gmtConnection);
            foreach (var item in entity.LabTestRequisitionImages.ToList())
            {
                switch (item.EntityState)
                {
                    case EntityState.Added:
                        item.LTReqImageID = maxLabTestRequisitionImageId++;
                        item.LTReqMasterID = entity.LTReqMasterID;
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