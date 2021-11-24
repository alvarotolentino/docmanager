using Dapper;
using System;
using System.Data;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using Application.Enums;
using Npgsql;
using NpgsqlTypes;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Data.Common;
using Application.Exceptions;
using Infrastructure.Persistence.Context;
using Application.Interfaces.Services;
using Application.Interfaces.Repositories;

namespace Infrastructure.Persistence.Repositories
{
    public class DocumentRepositoryAsync : IDocumentRepositoryAsync
    {
        private NpgsqlConnection connection;
        private readonly IDateTimeService dateTimeService;
        private readonly IAuthenticatedUserService authenticatedUserService;

        public DocumentRepositoryAsync(DbConnection dbConnection, IAuthenticatedUserService authenticatedUserService, IDateTimeService dateTimeService)
        {
            this.connection = (NpgsqlConnection)dbConnection;
            this.dateTimeService = dateTimeService;
            this.authenticatedUserService = authenticatedUserService;
        }

        public async Task<bool> DeleteDocumentById(long id)
        {
            var param = new DynamicParameters();
            param.Add("@p_id", id);
            var affected = await this.connection.ExecuteAsync("CALL \"usp_delete_document\" (@p_id)", param);
            return true;
        }

        public async Task<Documents> GetDocumentDataById(long id)
        {
            var param = new DynamicParameters();
            param.Add("p_id", id);

            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
            var document = await this.connection.QuerySingleOrDefaultAsync<Documents>("udf_get_document_data_by_id", param, commandType: CommandType.StoredProcedure);
            return document;
        }

        public async Task<Documents> GetDocumentInfoById(long id)
        {
            var param = new DynamicParameters();
            param.Add("p_id", id);

            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
            var document = await this.connection.QuerySingleOrDefaultAsync<Documents>("udf_get_document_info_by_id", param, commandType: CommandType.StoredProcedure);
            return document;
        }

        public async Task<IReadOnlyList<Documents>> GetDocuments(int pageNumber, int pageSize)
        {
            var param = new DynamicParameters();
            param.Add("p_number", pageNumber);
            param.Add("p_size", pageSize);

            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
            var documents = await this.connection.QueryAsync<Documents>("udf_get_documents_by_page_number_size", param, commandType: CommandType.StoredProcedure);
            return documents.ToList();
        }

        public async Task<long> SaveDocument(Domain.Entities.Documents document)
        {
            var param = new DynamicParameters();
            param.Add("@p_id", direction: ParameterDirection.InputOutput);
            param.Add("@p_name", document.Name, DbType.String);
            param.Add("@p_description", document.Description, DbType.String);
            param.Add("@p_dategory", document.Category, DbType.String);
            param.Add("@p_content_type", document.ContentType, DbType.String);
            param.Add("@p_length", document.Length, DbType.Int64);
            param.Add("@p_data", document.Data, DbType.Binary);
            param.Add("@p_created_at", this.dateTimeService.UtcDateTime, DbType.DateTime);
            param.Add("@p_created_by", this.authenticatedUserService.UserId, DbType.Int64);

            await this.connection.ExecuteAsync("CALL \"usp_insert_document\" (@p_id, @p_name, @p_description, @p_dategory, @p_content_type, @p_length, @p_data, @p_created_at, @p_created_by)", param);
            var id = param.Get<long>("@p_id");
            return id;
        }
    }
}
