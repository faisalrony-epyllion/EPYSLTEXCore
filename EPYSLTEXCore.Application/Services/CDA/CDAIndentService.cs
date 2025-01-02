using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.CDA;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using Microsoft.Data.SqlClient;
using System.Data.Entity;

namespace EPYSLTEXCore.Application.Services.CDA
{
    public class CDAIndentService : ICDAIndentService
    {
        private readonly IDapperCRUDService<CDAIndentMaster> _service;
        private readonly SqlConnection _connection;
        private readonly SqlConnection _gmtConnection;
        private SqlTransaction transaction;
        private SqlTransaction transactionGmt;

        public CDAIndentService(IDapperCRUDService<CDAIndentMaster> service)
        {
            _service = service;
            _service.Connection = _service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
            _gmtConnection = service.GetConnection(AppConstants.GMT_CONNECTION);
        }

        public async Task<CDAIndentMaster> GetDyesChemicalsAsync()
        {
            var query = $@"{CommonQueries.GetDyesChemicals()}";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                CDAIndentMaster data = new CDAIndentMaster
                {
                    SubGroupList = await records.ReadAsync<Select2OptionModel>()
                };
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

        public async Task<List<CDAIndentMaster>> GetPagedAsync(Status status, string pageName, PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By CDAIndentMasterID Desc" : paginationInfo.OrderBy;
            var sql = string.Empty;
            if (status == Status.Pending)
            {
                sql = $@"
                WITH Indent AS(
                    SELECT CDAM.CDAIndentMasterID, CDAM.SubGroupID, CDAM.IndentNo, CDAM.IndentDate, CDAM.TriggerPointID, EV.ValueName As TriggerPoint,
                    CDAM.CIndentBy, E.EmployeeName As CDAIndentByUser, CDAM.Remarks
	                from CDAIndentMaster CDAM
	                Left Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = CDAM.TriggerPointID
	                LEFT Join {DbNames.EPYSL}..LoginUser L On L.UserCode = CDAM.CIndentBy
	                LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
	                WHERE CDAM.SendForCheck = 0 And CDAM.SendForApproval = 0 And CDAM.IsCheck = 0 And CDAM.Approve = 0
                    AND CDAM.SendForAcknowledge = 0 And CDAM.Acknowledge = 0 And CDAM.Reject = 0
                )
                SELECT *,Count(*) Over() TotalRows FROM Indent";
            }
            else if (status == Status.Proposed)
            {
                sql = $@"
                WITH Indent AS(
                    SELECT CDAM.CDAIndentMasterID, CDAM.SubGroupID, CDAM.IndentNo, CDAM.IndentDate, CDAM.TriggerPointID, EV.ValueName As TriggerPoint,
                    CDAM.CIndentBy, E.EmployeeName As CDAIndentByUser, CDAM.Remarks
	                from CDAIndentMaster CDAM
	                Left Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = CDAM.TriggerPointID
	                LEFT Join {DbNames.EPYSL}..LoginUser L On L.UserCode = CDAM.CIndentBy
                    LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
	                WHERE CDAM.SendForCheck = 1 And CDAM.SendForApproval = 1 And CDAM.IsCheck = 0 And CDAM.Approve = 0
                    AND CDAM.SendForAcknowledge = 0 And CDAM.Acknowledge = 0 And CDAM.Reject = 0
                )
                SELECT *,Count(*) Over() TotalRows FROM Indent";
            }
            else if (status == Status.Check)
            {
                sql = $@"
                WITH Indent AS(
                    SELECT CDAM.CDAIndentMasterID, CDAM.SubGroupID, CDAM.IndentNo, CDAM.IndentDate, CDAM.TriggerPointID, EV.ValueName As TriggerPoint,
                    CDAM.CIndentBy, E.EmployeeName As CDAIndentByUser, CDAM.Remarks
	                from CDAIndentMaster CDAM
	                Left Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = CDAM.TriggerPointID
	                LEFT Join {DbNames.EPYSL}..LoginUser L On L.UserCode = CDAM.CIndentBy
                    LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
	                WHERE CDAM.SendForCheck = 1 And CDAM.SendForApproval = 1 And CDAM.IsCheck = 1  
                )
                SELECT *,Count(*) Over() TotalRows FROM Indent";
            }
            else if (status == Status.ProposedForApproval)
            {
                sql = $@"
                WITH Indent AS(
                    SELECT CDAM.CDAIndentMasterID, CDAM.SubGroupID, CDAM.IndentNo, CDAM.IndentDate, CDAM.TriggerPointID, EV.ValueName As TriggerPoint,
                    CDAM.CIndentBy, E.EmployeeName As CDAIndentByUser, CDAM.Remarks
	                from CDAIndentMaster CDAM
	                Left Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = CDAM.TriggerPointID
	                LEFT Join {DbNames.EPYSL}..LoginUser L On L.UserCode = CDAM.CIndentBy
                    LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
	                WHERE CDAM.SendForCheck = 1 And CDAM.SendForApproval = 1 And CDAM.IsCheck = 1 And CDAM.Approve = 0
                    AND CDAM.SendForAcknowledge = 0 And CDAM.Acknowledge = 0 And CDAM.Reject = 0
                )
                SELECT *,Count(*) Over() TotalRows FROM Indent";
            }
            else if (status == Status.Approved)
            {
                sql = $@"
                WITH Indent AS(
                    SELECT CDAM.CDAIndentMasterID, CDAM.SubGroupID, CDAM.IndentNo, CDAM.IndentDate, CDAM.TriggerPointID, EV.ValueName As TriggerPoint,
                    CDAM.CIndentBy, E.EmployeeName As CDAIndentByUser, CDAM.Remarks
	                from CDAIndentMaster CDAM
	                Left Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = CDAM.TriggerPointID
	                LEFT Join {DbNames.EPYSL}..LoginUser L On L.UserCode = CDAM.CIndentBy
                    LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
	                WHERE CDAM.IsCheck = 1 And CDAM.Approve = 1
                    AND CDAM.SendForAcknowledge = 1 --And CDAM.Acknowledge = 0 And CDAM.Reject = 0
                )
                SELECT *,Count(*) Over() TotalRows FROM Indent";
            }
            else if (status == Status.ProposedForAcknowledge)
            {
                sql = $@"
                WITH Indent AS(
                    SELECT CDAM.CDAIndentMasterID, CDAM.SubGroupID, CDAM.IndentNo, CDAM.IndentDate, CDAM.TriggerPointID, EV.ValueName As TriggerPoint,
                    CDAM.CIndentBy, E.EmployeeName As CDAIndentByUser, CDAM.Remarks
	                from CDAIndentMaster CDAM
	                Left Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = CDAM.TriggerPointID
	                LEFT Join {DbNames.EPYSL}..LoginUser L On L.UserCode = CDAM.CIndentBy
                    LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
	                WHERE CDAM.SendForCheck = 1 And CDAM.SendForApproval = 1 And CDAM.IsCheck = 1 And CDAM.Approve = 1
                    AND CDAM.SendForAcknowledge = 1 And CDAM.Acknowledge = 0 And CDAM.Reject = 0
                )
                SELECT *,Count(*) Over() TotalRows FROM Indent";
            }
            else if (status == Status.Acknowledge)
            {
                sql = $@"
                WITH Indent AS(
                    SELECT CDAM.CDAIndentMasterID, CDAM.SubGroupID, CDAM.IndentNo, CDAM.IndentDate, CDAM.TriggerPointID, EV.ValueName As TriggerPoint,
                    CDAM.CIndentBy, E.EmployeeName As CDAIndentByUser, CDAM.Remarks, CDAM.AcknowledgeDate
	                from CDAIndentMaster CDAM
	                Left Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = CDAM.TriggerPointID
	                LEFT Join {DbNames.EPYSL}..LoginUser L On L.UserCode = CDAM.CIndentBy
                    LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
	                WHERE CDAM.Acknowledge = 1
                )
                SELECT *,Count(*) Over() TotalRows FROM Indent";
            }
            else if (status == Status.ProposedForAcknowledgeAcceptence)
            {
                sql = $@"
                WITH Indent AS(
                    SELECT CDAM.CDAIndentMasterID, CDAM.SubGroupID, CDAM.IndentNo, CDAM.IndentDate, CDAM.TriggerPointID, EV.ValueName As TriggerPoint,
                    CDAM.CIndentBy, E.EmployeeName As CDAIndentByUser, CDAM.Remarks, CDAM.AcknowledgeDate
	                from CDAIndentMaster CDAM
	                Left Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = CDAM.TriggerPointID
	                LEFT Join {DbNames.EPYSL}..LoginUser L On L.UserCode = CDAM.CIndentBy
                    LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
	                WHERE CDAM.Acknowledge = 1 And CDAM.TexAcknowledge = 0
                )
                SELECT *,Count(*) Over() TotalRows FROM Indent";
            }
            else if (status == Status.AcknowledgeAcceptence)
            {
                sql = $@"
                WITH Indent AS(
                    SELECT CDAM.CDAIndentMasterID, CDAM.SubGroupID, CDAM.IndentNo, CDAM.IndentDate, CDAM.TriggerPointID, EV.ValueName As TriggerPoint,
                    CDAM.CIndentBy, E.EmployeeName As CDAIndentByUser, CDAM.Remarks, CDAM.AcknowledgeDate, CDAM.TexAcknowledgeDate
	                from CDAIndentMaster CDAM
	                Left Join {DbNames.EPYSL}..EntityTypeValue EV On EV.ValueID = CDAM.TriggerPointID
	                LEFT Join {DbNames.EPYSL}..LoginUser L On L.UserCode = CDAM.CIndentBy
                    LEFT JOIN {DbNames.EPYSL}..Employee E ON E.EmployeeCode = L.EmployeeCode
	                WHERE CDAM.TexAcknowledge = 1
                )
                SELECT *,Count(*) Over() TotalRows FROM Indent";
            }
            sql += $@"
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";
            return await _service.GetDataAsync<CDAIndentMaster>(sql);
        }

        public async Task<CDAIndentMaster> GetNewAsync(string SubGroupName)
        {
            var query =
                $@"
                -- Requisition By
                {CommonQueries.GetCDAIndentByUsers()};

                -- RM Trigger Points
                {CommonQueries.GetEntityTypesByEntityTypeName(EntityTypeNameConstants.RM_TRIGGER_POINTS)}

                --Items List for PopUp
                Select IM.ItemMasterID, IM.Segment1ValueID Segment1ValueId, ISV.SegmentValue As Segment1ValueDesc,
                IM.Segment2ValueID Segment2ValueId, ISV2.SegmentValue AS Segment2ValueDesc,
                IM.Segment3ValueID Segment3ValueId, ISV3.SegmentValue AS Segment3ValueDesc,
                IM.Segment4ValueID Segment4ValueId, ISV4.SegmentValue AS Segment4ValueDesc
                From {DbNames.EPYSL}..ItemMaster IM
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 On ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 On ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 On ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentName ISN On ISN.SegmentNameID = ISV.SegmentNameID
                WHERE ISN.SegmentName In ('{SubGroupName}'); ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                CDAIndentMaster data = new CDAIndentMaster
                {
                    CIndentByList = await records.ReadAsync<Select2OptionModel>(),
                    TriggerPointList = await records.ReadAsync<Select2OptionModel>(),
                    ChildsItemSegments = records.Read<CDAIndentChild>().ToList()
                };
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

        public async Task<CDAIndentMaster> GetAsync(int id, string SubGroupName)
        {
            var query =
                $@"
                -- Master Data
                Select CDAM.CDAIndentMasterID, CDAM.IndentDate, CDAM.IndentNo, CDAM.IndentStartMonth, CDAM.IndentEndMonth, 
                CDAM.Remarks, CDAM.CIndentBy, CDAM.TriggerPointID, CDAM.SubGroupID, SG.SubGroupName SubGroupName,
                CDAM.SendForApproval, CDAM.Approve, CDAM.SendForAcknowledge, CDAM.Acknowledge
                From CDAIndentMaster CDAM INNER JOIN {DbNames.EPYSL}..ITEMSUBGROUP SG ON SG.SubGroupID = CDAM.SubGroupID
                Where CDAM.CDAIndentMasterID = {id};

                --Child Data
                ;WITH YRC As (
	                 Select * From CDAIndentChild Where CDAIndentMasterID = {id}
                )
                Select YRC.CDAIndentChildID, YRC.CDAIndentMasterID, YRC.ItemMasterID, YRC.UnitID,YRC.Remarks, YRC.IndentQty, YRC.CheckQty, YRC.ApprovQty, YRC.ReqQty,
                YRC.UnitID, YRC.ItemMasterID, YRC.HSCode, YRC.CompanyID, U.DisplayUnitDesc, IM.Segment1ValueID, IM.Segment2ValueID, 
				IM.Segment3ValueID, IM.Segment4ValueID, ISV1.SegmentValue Segment1ValueDesc, ISV2.SegmentValue Segment2ValueDesc, 
				ISV3.SegmentValue Segment3ValueDesc, ISV4.SegmentValue Segment4ValueDesc
                From YRC
                INNER JOIN {DbNames.EPYSL}..ItemMaster IM ON IM.ItemMasterID = YRC.ItemMasterID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV1 On ISV1.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV2 ON ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV3 ON ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN  {DbNames.EPYSL}..ItemSegmentValue ISV4 ON ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN  {DbNames.EPYSL}..Unit U ON U.UnitID = YRC.UnitID;

                -- Child Details Information
                Select * From CDAIndentChildDetails Where CDAIndentMasterID = {id};

                -- Company
                Select Cast (CE.CompanyID as varchar) [id] , CE.ShortName [text]
                From {DbNames.EPYSL}..CompanyEntity CE 
                Where Isnull(CE.BusinessNature,'') IN ('TEX','PC')
                Group by CE.CompanyID, CE.ShortName;

                --Requisition By
                {CommonQueries.GetCDAIndentByUsers()};

                -- RM Trigger Points
                SELECT CAST(ValueID AS VARCHAR) id, ValueName text
                FROM {DbNames.EPYSL}..EntityTypeValue EV
                Inner Join {DbNames.EPYSL}..EntityType ET On EV.EntityTypeID = ET.EntityTypeID
                WHERE ET.EntityTypeName = 'RM Trigger Points' AND ValueName <> 'Select'
                ORDER BY ValueName;

                --Items List for PopUp
                Select IM.ItemMasterID, IM.Segment1ValueID Segment1ValueId, ISV.SegmentValue As Segment1ValueDesc,
                IM.Segment2ValueID Segment2ValueId, ISV2.SegmentValue AS Segment2ValueDesc,
                IM.Segment3ValueID Segment3ValueId, ISV3.SegmentValue AS Segment3ValueDesc,
                IM.Segment4ValueID Segment4ValueId, ISV4.SegmentValue AS Segment4ValueDesc
                From {DbNames.EPYSL}..ItemMaster IM
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV On ISV.SegmentValueID = IM.Segment1ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV2 On ISV2.SegmentValueID = IM.Segment2ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV3 On ISV3.SegmentValueID = IM.Segment3ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentValue ISV4 On ISV4.SegmentValueID = IM.Segment4ValueID
                LEFT JOIN {DbNames.EPYSL}..ItemSegmentName ISN On ISN.SegmentNameID = ISV.SegmentNameID
                WHERE ISN.SegmentName In ('{SubGroupName}'); ";
            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(query);
                CDAIndentMaster data = await records.ReadFirstOrDefaultAsync<CDAIndentMaster>();
                data.Childs = records.Read<CDAIndentChild>().ToList();
                data.Childs.ForEach(child =>
                {
                    child.ItemIDs += string.Join(",", data.Childs.Where(x => x.ItemMasterID == child.ItemMasterID).Select(y => y.ItemMasterID));
                });
                //var prCompanyList = records.Read<CDAIndentChildCompany>().ToList();
                
                List<CDAIndentChildDetails> cItems = records.Read<CDAIndentChildDetails>().ToList();
                data.Childs.ForEach(childDetails =>
                {
                    childDetails.ChildItems = cItems.Where(c => c.CDAIndentChildID == childDetails.CDAIndentChildID).ToList();
                });

                data.CompanyList = await records.ReadAsync<Select2OptionModel>();
                data.CIndentByList = await records.ReadAsync<Select2OptionModel>();
                data.TriggerPointList = await records.ReadAsync<Select2OptionModel>();
                data.ChildsItemSegments = records.Read<CDAIndentChild>().ToList();
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

        public async Task<CDAIndentMaster> GetAllAsync(int id)
        {
            var sql = $@"
            ;Select * From CDAIndentMaster Where CDAIndentMasterID = {id}
            ;Select * From CDAIndentChild Where CDAIndentMasterID = {id}
            ;Select * From CDAIndentChildDetails Where CDAIndentMasterID = {id}
            ;Select * From CDAIndentChildCompany Where CDAIndentMasterID = { id} ";

            try
            {
                await _connection.OpenAsync();
                var records = await _connection.QueryMultipleAsync(sql);
                CDAIndentMaster data = records.Read<CDAIndentMaster>().FirstOrDefault();
                Guard.Against.NullObject(data);
                data.Childs = records.Read<CDAIndentChild>().ToList();
                data.ChildItems = records.Read<CDAIndentChildDetails>().ToList();
                data.Childs.ForEach(x =>
                {
                    x.ChildItems = data.ChildItems.Where(c => c.CDAIndentChildID == x.CDAIndentChildID).ToList();
                });

                List<CDAIndentChildCompany> CDAPRCompanieList = records.Read<CDAIndentChildCompany>().ToList();
                data.Childs.ForEach(x =>
                {
                    x.CDAIndentCompanies = CDAPRCompanieList.Where(y => y.CDAIndentChildID == x.CDAIndentChildID).ToList();
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

        public async Task SaveAsync(CDAIndentMaster entity)
        {
            SqlTransaction transaction = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                await _gmtConnection.OpenAsync();
                transactionGmt = _gmtConnection.BeginTransaction();

                int maxChildId = 0, maxChildDetailsId = 0, maxCompanyId = 0;
                switch (entity.EntityState)
                {
                    case EntityState.Added:
                        entity.CDAIndentMasterID = await _service.GetMaxIdAsync(TableNames.CDA_INDENT_MASTER);
                        entity.IndentNo = await _service.GetMaxNoAsync(TableNames.CDA_INDENT_NO, 1, RepeatAfterEnum.NoRepeat, "00000", transactionGmt, _gmtConnection);
                        maxChildId = await _service.GetMaxIdAsync(TableNames.CDA_INDENT_CHILD, entity.Childs.Count);
                        maxChildDetailsId = await _service.GetMaxIdAsync(TableNames.CDA_INDENT_CHILD_DETAILS, entity.Childs.Sum(x => x.ChildItems.Count));
                        maxCompanyId = await _service.GetMaxIdAsync(TableNames.CDA_INDENT_CHILD_COMPANY, entity.Childs.Sum(x => x.CDAIndentCompanies.Count()));

                        foreach (CDAIndentChild child in entity.Childs)
                        {
                            child.CDAIndentChildID = maxChildId++;
                            child.CDAIndentMasterID = entity.CDAIndentMasterID;
                            foreach (CDAIndentChildDetails itemDtls in child.ChildItems)
                            {
                                itemDtls.CDAIndentChildDetailsID = maxChildDetailsId++;
                                itemDtls.CDAIndentChildID = child.CDAIndentChildID;
                                itemDtls.CDAIndentMasterID = child.CDAIndentMasterID;
                            }

                            foreach (CDAIndentChildCompany company in child.CDAIndentCompanies)
                            {
                                company.CDAIndentChildCompanyID = maxCompanyId++;
                                company.CDAIndentChildID = child.CDAIndentChildID;
                                company.CDAIndentMasterID = entity.CDAIndentMasterID;
                            }
                        }

                        break;

                    case EntityState.Modified:
                        maxChildId = await _service.GetMaxIdAsync(TableNames.CDA_INDENT_CHILD, entity.Childs.Count(x => x.EntityState == EntityState.Added));
                        maxChildDetailsId = await _service.GetMaxIdAsync(TableNames.CDA_INDENT_CHILD_DETAILS, entity.Childs.Sum(x => x.ChildItems.Where(y => y.EntityState == EntityState.Added).ToList().Count));
                        maxCompanyId = await _service.GetMaxIdAsync(TableNames.CDA_INDENT_CHILD_COMPANY, entity.Childs.Sum(x => x.CDAIndentCompanies.Where(y => y.EntityState == EntityState.Added).Count()));

                        foreach (CDAIndentChild child in entity.Childs)
                        {
                            if (child.EntityState == EntityState.Added)
                            {
                                child.CDAIndentChildID = maxChildId++;
                                child.CDAIndentMasterID = entity.CDAIndentMasterID;

                                foreach (CDAIndentChildDetails itemDtls in child.ChildItems.ToList())
                                {
                                    itemDtls.CDAIndentChildDetailsID = maxChildDetailsId++;
                                    itemDtls.CDAIndentChildID = child.CDAIndentChildID;
                                    itemDtls.CDAIndentMasterID = child.CDAIndentMasterID;
                                    child.EntityState = EntityState.Added;
                                }

                                foreach (CDAIndentChildCompany company in child.CDAIndentCompanies)
                                {
                                    company.CDAIndentChildCompanyID = maxCompanyId++;
                                    company.CDAIndentChildID = child.CDAIndentChildID;
                                    company.CDAIndentMasterID = entity.CDAIndentMasterID;
                                }
                            }
                            else if (child.EntityState == EntityState.Modified)
                            {
                                foreach (CDAIndentChildDetails itemDtls in child.ChildItems.Where(y => y.EntityState == EntityState.Added).ToList())
                                {
                                    itemDtls.CDAIndentChildDetailsID = maxChildDetailsId++;
                                    itemDtls.CDAIndentChildID = child.CDAIndentChildID;
                                    itemDtls.CDAIndentMasterID = child.CDAIndentMasterID;
                                    itemDtls.EntityState = EntityState.Added;
                                }

                                foreach (CDAIndentChildCompany company in child.CDAIndentCompanies.Where(x => x.EntityState == EntityState.Added))
                                {
                                    company.CDAIndentChildCompanyID = maxCompanyId++;
                                    company.CDAIndentChildID = child.CDAIndentChildID;
                                    company.CDAIndentMasterID = entity.CDAIndentMasterID;
                                }
                            }
                            else if (child.EntityState == EntityState.Deleted)
                            {
                                child.ChildItems.SetDeleted();
                                List<CDAIndentChildDetails> cItems = new List<CDAIndentChildDetails>();
                                entity.Childs.ForEach(x => cItems.AddRange(x.ChildItems.Where(y => y.EntityState == EntityState.Deleted)));
                                await _service.SaveAsync(cItems, transaction);

                                child.CDAIndentCompanies.SetDeleted();
                                List<CDAIndentChildCompany> CompanyDel = new List<CDAIndentChildCompany>();
                                entity.Childs.ForEach(x => CompanyDel.AddRange(x.CDAIndentCompanies.Where(y => y.EntityState == EntityState.Deleted)));
                                await _service.SaveAsync(CompanyDel, transaction);
                            }
                        }
                        break;

                    default:
                        break;
                }
                
                await _service.SaveSingleAsync(entity, transaction);
                await _service.SaveAsync(entity.Childs, transaction);
                List<CDAIndentChildDetails> childItems = new List<CDAIndentChildDetails>();
                entity.Childs.ForEach(x =>
                {
                    childItems.AddRange(x.ChildItems);
                });
                await _service.SaveAsync(childItems, transaction);

                List<CDAIndentChildCompany> companyList = new List<CDAIndentChildCompany>();
                entity.Childs.ForEach(x => companyList.AddRange(x.CDAIndentCompanies));
                await _service.SaveAsync(companyList, transaction);
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
        public async Task UpdateEntityAsync(CDAIndentMaster entity)
        {
            SqlTransaction transaction = null;
            try
            {
                await _connection.OpenAsync();
                transaction = _connection.BeginTransaction();
                await _service.SaveSingleAsync(entity, transaction);
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