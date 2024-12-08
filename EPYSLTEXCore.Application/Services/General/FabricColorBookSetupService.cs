using Dapper;
using EPYSLTEX.Core.Statics;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Application.Interfaces.Repositories;
using EPYSLTEXCore.Application.Interfaces.RND;
using EPYSLTEXCore.Infrastructure.Data;
using EPYSLTEXCore.Infrastructure.Entities;
using EPYSLTEXCore.Infrastructure.Entities.DTOs;
using EPYSLTEXCore.Infrastructure.Entities.Gmt.General;
using EPYSLTEXCore.Infrastructure.Entities.Tex.Knitting;
using EPYSLTEXCore.Infrastructure.Entities.Tex.RND;
using EPYSLTEXCore.Infrastructure.Exceptions;
using EPYSLTEXCore.Infrastructure.Static;
using EPYSLTEXCore.Infrastructure.Statics;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace EPYSLTEXCore.Application.Services.General
{
    public class FabricColorBookSetupService : IFabricColorBookSetupService
    {
        private readonly IDapperCRUDService<FabricColorBookSetup> _service;
        private readonly SqlConnection _connection;
        public FabricColorBookSetupService(IDapperCRUDService<FabricColorBookSetup> service)
        {
            _service = service;
            _service.Connection = service.GetConnection(AppConstants.TEXTILE_CONNECTION);
            _connection = service.Connection;
        }
        public async Task<FabricColorBookSetupDTO> GetNewAsync()
        {
            var query = $@"{CommonQueries.GetItemSegmentValuesBySegmentName(ItemSegmentNameConstants.FABRIC_COLOR)}";
            var data = new FabricColorBookSetupDTO()
            {
                ColorList = await _service.GetDataAsync<Select2OptionModel>(query)
            };

            return data;
        }

        public async Task<FabricColorBookSetup> GetAsync(int id)
        {
            var query = $@"Select * From {DbNames.EPYSL}..FabricColorBookSetup Where PTNID = {id}";
            return await _service.GetFirstOrDefaultAsync(query);
        }

        public async Task<List<FabricColorBookSetupDTO>> GetAllColorAsync(PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By LEN(ColorName), ColorName ASC" : paginationInfo.OrderBy;

            if (paginationInfo.FilterBy.Contains("ColorCode = '"))
            {
                paginationInfo.FilterBy = paginationInfo.FilterBy.Replace("ColorCode = '", " ColorCode LIKE '%");
                string[] splitData = paginationInfo.FilterBy.Split('\'');
                List<string> splitedList = new List<string>();
                for (int i = 0; i < splitData.Length; i++)
                {
                    splitedList.Add(splitData[i]);
                }
                int indexF = splitedList.FindIndex(x => x.Trim().Contains("ColorCode LIKE"));
                splitedList[indexF + 1] = splitedList[indexF + 1] + "%";
                paginationInfo.FilterBy = string.Join("'", splitedList.Select(k => k));
            }

            var query = $@"
                With F As (
                    Select ISV.SegmentValueID PTNID, ISV.SegmentValueID ColorID, FCBS.ColorSource, FCBS.ColorCode, FCBS.RGBOrHex, ISV.SegmentValue ColorName
                    From {DbNames.EPYSL}..ItemSegmentValue ISV
                    Inner Join {DbNames.EPYSL}..ItemSegmentName ISN on ISN.SegmentNameID = ISV.SegmentNameID
                    Left Join {DbNames.EPYSL}..FabricColorBookSetup FCBS On FCBS.ColorID = ISV.SegmentValueID
                    Where ISN.SegmentName = 'Fabric Color'
                )

                Select *, COUNT(*) Over() TotalRows  From F
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<FabricColorBookSetupDTO>(query);
        }

        public async Task<List<FabricColorBookSetupDTO>> GetPagedAsync(PaginationInfo paginationInfo)
        {
            string orderBy = paginationInfo.OrderBy.NullOrEmpty() ? "Order By ColorCode Desc" : paginationInfo.OrderBy;

            var query = $@"
                With F As (
	                Select FCBS.PTNID, FCBS.ColorID, FCBS.ColorSource, FCBS.ColorCode, FCBS.RGBOrHex, ISV.SegmentValue ColorName
	                From {DbNames.EPYSL}..FabricColorBookSetup FCBS
	                Inner Join {DbNames.EPYSL}..ItemSegmentValue ISV On FCBS.ColorID = ISV.SegmentValueID
                )

                Select *, COUNT(*) Over() TotalRows
                From F
                {paginationInfo.FilterBy}
                {orderBy}
                {paginationInfo.PageBy}";

            return await _service.GetDataAsync<FabricColorBookSetupDTO>(query);
        }

        public async Task<List<FabricColorBookSetupDTO>> GetAllListAsync()
        {
            var query = $@"
            Select FCBS.PTNID, FCBS.ColorID, FCBS.ColorSource, FCBS.ColorCode, FCBS.RGBOrHex, ISV.SegmentValue ColorName
	        From {DbNames.EPYSL}..FabricColorBookSetup FCBS
	        Inner Join ItemSegmentValue ISV On FCBS.ColorID = ISV.SegmentValueID";

            return await _service.GetDataAsync<FabricColorBookSetupDTO>(query);
        }

        public async Task SaveAsync(FabricColorBookSetup entity)
        {
            SqlTransaction transaction = null;
            try
            {
                await _service.Connection.OpenAsync();
                transaction = _service.Connection.BeginTransaction();

                if (entity.EntityState == System.Data.Entity.EntityState.Added)
                {
                    entity.PTNID = await _service.GetMaxIdAsync(TableNames.FABRIC_COLOR_BOOK_SETUP);
                }

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
                _service.Connection.Close();
            }
        }
    }
}
