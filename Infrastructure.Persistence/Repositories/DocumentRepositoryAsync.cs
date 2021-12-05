using System;
using System.Data;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using Application.Enums;
using Npgsql;
using System.Data.Common;
using Application.Interfaces.Services;
using Application.Interfaces.Repositories;
using System.Threading;
using Application.Features.Documents.Queries.GetAllDocuments;
using Infrastructure.Persistence.Database;
using System.Transactions;
using System.Linq;

namespace Infrastructure.Persistence.Repositories
{
    public class DocumentRepositoryAsync : IDocumentRepositoryAsync, IDisposable
    {
        private NpgsqlConnection metadataConnection;
        private NpgsqlConnection dataConnection;

        public DocumentRepositoryAsync(DatabaseConnections docManagerConnection)
        {
            this.metadataConnection = docManagerConnection.MetadataConnection;
            this.dataConnection = docManagerConnection.DataConnection;
        }

        public async Task<bool> DeleteDocumentById(Document document, CancellationToken cancellationToken)
        {
            var documentId = -1;
            var externalId = -1;

            using var dbManagerMetadata = new DbManager(metadataConnection);
            var deletedDocMetadata = await dbManagerMetadata.ExecuteReaderAsync<dynamic>("udf_delete_document_metadata", cancellationToken, inputParam: new { Id = document.Id }, commandType: CommandType.StoredProcedure);
            if (deletedDocMetadata == null) return false;
            var kvMetadata = deletedDocMetadata as IDictionary<string, object>;
            externalId = (int)kvMetadata["ExternalId"];
            if (externalId == -1) return false;

            using var dbManagerData = new DbManager(dataConnection);
            var deletedDocData = await dbManagerData.ExecuteNonQueryAsync<dynamic>("CALL \"udf_delete_document_data\" (@p_id)", cancellationToken, outpuParam: new { Id = externalId });
            var kvData = deletedDocData as IDictionary<string, object>;
            documentId = (int)kvData["Id"];
            return documentId > -1 ? true : false;

        }

        public async Task<Document> GetDocumentDataById(UserDocument userDocument, CancellationToken cancellationToken)
        {
            using var dbManagerMetadata = new DbManager(this.metadataConnection);
            var document = await dbManagerMetadata.ExecuteReaderAsync<Document>("udf_get_document_data_by_id", cancellationToken,
            inputParam: userDocument,
            commandType: CommandType.StoredProcedure);
            if (document == null || document.ExternalId <= 0)
                return null;

            using var dbManagerData = new DbManager(this.dataConnection);
            var docData = await dbManagerData.ExecuteReaderAsync<Document>("udf_get_document_data_by_id", cancellationToken,
            inputParam: new { DocumentId = document.ExternalId },
            commandType: CommandType.StoredProcedure);
            document.Data = docData.Data;
            return document.Data != null ? document : null;
        }

        public async Task<Document> GetDocumentInfoById(UserDocument userDocument, CancellationToken cancellationToken)
        {
            using var dbManager = new DbManager(this.metadataConnection);
            var result = await dbManager.ExecuteReaderAsync<Document>("udf_get_document_info_by_id", cancellationToken, inputParam: userDocument, commandType: CommandType.StoredProcedure);
            return result;
        }

        public async Task<IReadOnlyList<Document>> GetDocuments(GetUserDocumentsPaginated userDocumentsPaginated, CancellationToken cancellationToken)
        {
            using var dbManagerMetadata = new DbManager(this.metadataConnection);
            var documents = await dbManagerMetadata.ExecuteReaderAsListAsync<Document>("udf_get_documents_by_page_number_size", cancellationToken,
            inputParam: new
            {
                Number = userDocumentsPaginated.PageNumber,
                Size = userDocumentsPaginated.PageSize,
                UserId = userDocumentsPaginated.UserId,
            },
            commandType: CommandType.StoredProcedure);

            return documents != null ? documents.ToList() : null;
        }

        public async Task<int> SaveDocument(Document document, CancellationToken cancellationToken)
        {
            using var dbManagerData = new DbManager(this.dataConnection);
            var sp = "CALL \"usp_insert_document_data\" (@p_id, @p_data)";
            var dynData = await dbManagerData.ExecuteNonQueryAsync<dynamic>(sp, cancellationToken,
            inputParam: new { Data = document.Data },
            outpuParam: new { Id = -1 });
            if (dynData == null) return -1;
            var kvData = dynData as IDictionary<string, object>;
            document.ExternalId = (int)kvData["Id"];

            var spInsertDoc = "CALL \"usp_insert_document_metadata\" (@p_id, @p_name, @p_description, @p_category, @p_content_type, @p_length, @p_external_id, @p_created_at, @p_created_by)";
            using var dbManagerMetadata = new DbManager(this.metadataConnection);
            var dynMetadata = await dbManagerMetadata.ExecuteNonQueryAsync<dynamic>(spInsertDoc, cancellationToken,
            inputParam: document,
            outpuParam: new { Id = -1 });
            if (dynMetadata == null) return -1;
            var kvMetadata = dynMetadata as IDictionary<string, object>;
            var id = (int)kvMetadata["Id"];
            return id;

        }

        public async Task<UserDocument> AssingUserPermissionAsync(UserDocument userDocument, CancellationToken cancellationToken)
        {
            using var dbManager = new DbManager(this.metadataConnection);
            var result = await dbManager.ExecuteReaderAsync<UserDocument>("udf_assig_user_document_permission", cancellationToken, inputParam: userDocument);
            return result;
        }

        public async Task<GroupDocument> AssingGroupPermissionAsync(GroupDocument groupDocument, CancellationToken cancellationToken)
        {
            using var dbManager = new DbManager(this.metadataConnection);
            var result = await dbManager.ExecuteReaderAsync<GroupDocument>("udf_assig_group_document_permission", cancellationToken, inputParam: groupDocument);
            return result;
        }

        public void Dispose()
        {
            if (this.dataConnection != null && this.dataConnection.State == ConnectionState.Open)
            {
                this.dataConnection.Close();
            }

            if (this.metadataConnection != null && this.metadataConnection.State == ConnectionState.Open)
            {
                this.metadataConnection.Close();
            }
        }
    }
}
