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

            await metadataConnection.OpenAsync();
            using (var cmdDeleteMetadata = new NpgsqlCommand("udf_delete_document_metadata", metadataConnection))
            {
                cmdDeleteMetadata.CommandType = CommandType.StoredProcedure;
                cmdDeleteMetadata.Parameters.AddWithValue("p_id", document.Id);
                cmdDeleteMetadata.Prepare();

                using (var reader = await cmdDeleteMetadata.ExecuteReaderAsync(cancellationToken))
                {
                    if (!reader.HasRows) return false;
                    reader.Read();
                    externalId = (int)reader["external_id"];
                }
            }
            await metadataConnection.CloseAsync();
            if (externalId == -1) return false;

            await dataConnection.OpenAsync();
            using (var cmdDeleteData = new NpgsqlCommand("CALL \"udf_delete_document_data\" (@p_id)", dataConnection))
            {
                cmdDeleteData.Parameters.Add(new NpgsqlParameter("@p_id", DbType.Int32) { Value = externalId, Direction = ParameterDirection.InputOutput });
                cmdDeleteData.Prepare();
                await cmdDeleteData.ExecuteNonQueryAsync();
                documentId = (int)cmdDeleteData.Parameters["@p_id"].Value;
            }
            await dataConnection.CloseAsync();

            return documentId > -1 ? true : false;
        }

        public async Task<Document> GetDocumentDataById(UserDocument userDocument, CancellationToken cancellationToken)
        {
            Document document = null;
            using (var cmd = new NpgsqlCommand("udf_get_document_data_by_id", metadataConnection))
            {
                await metadataConnection.OpenAsync(cancellationToken);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("p_documentid", userDocument.DocumentId);
                cmd.Parameters.AddWithValue("p_userid", userDocument.UserId);
                await cmd.PrepareAsync(cancellationToken);
                using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    if (!reader.HasRows)
                        return null;

                    document = new Document();
                    reader.Read();
                    document.Name = reader["name"].ToString();
                    document.ContentType = reader["content_type"].ToString();
                    document.Length = (long)reader["length"];
                    document.ExternalId = (int)reader["external_id"];

                }
                await metadataConnection.CloseAsync();
            }
            if (document == null || document.ExternalId <= 0)
                return null;

            using (var cmd = new NpgsqlCommand("udf_get_document_data_by_id", dataConnection))
            {
                await dataConnection.OpenAsync(cancellationToken);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("p_documentid", document.ExternalId);
                await cmd.PrepareAsync(cancellationToken);
                using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    if (!reader.HasRows)
                        return null;

                    reader.Read();
                    document.Data = (byte[])reader["data"];

                }
                await dataConnection.CloseAsync();
            }

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
            using (var cmd = new NpgsqlCommand("udf_get_documents_by_page_number_size", metadataConnection))
            {
                metadataConnection.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("p_number", userDocumentsPaginated.PageNumber);
                cmd.Parameters.AddWithValue("p_size", userDocumentsPaginated.PageSize);
                cmd.Parameters.AddWithValue("p_userid", userDocumentsPaginated.UserId);
                cmd.Prepare();

                List<Document> documents = null;
                using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    if (reader.HasRows)
                    {
                        documents = new List<Document>();
                        while (reader.Read())
                        {
                            var document = new Document
                            {
                                Id = (int)reader["id"],
                                Name = reader["name"].ToString(),
                                Description = reader["description"].ToString(),
                                Category = reader["category"].ToString(),
                                ContentType = reader["content_type"].ToString(),
                                Length = (long)reader["length"],
                                CreatedBy = (int)reader["created_by"],
                                CreatedAt = (DateTime)reader["created_at"],
                                UpdatedBy = (int)reader["updated_by"],
                                UpdatedAt = (DateTime)reader["updated_at"]

                            };
                            documents.Add(document);
                        }
                    }
                }
                metadataConnection.Close();
                return documents;
            }
        }

        public async Task<int> SaveDocument(Document document, CancellationToken cancellationToken)
        {
            using (var cmd = new NpgsqlCommand("CALL \"usp_insert_document_data\" (@p_id, @p_data)", dataConnection))
            {
                await dataConnection.OpenAsync(cancellationToken);
                cmd.Parameters.Add(new NpgsqlParameter("@p_id", DbType.Int32) { Value = -1, Direction = ParameterDirection.InputOutput });
                cmd.Parameters.AddWithValue("@p_data", document.Data);
                await cmd.PrepareAsync(cancellationToken);
                await cmd.ExecuteNonQueryAsync(cancellationToken);
                var id = (int)cmd.Parameters["@p_id"].Value;
                await dataConnection.CloseAsync();
                if (id == -1) return id;
                document.ExternalId = id;
            }

            using (var cmd = new NpgsqlCommand("CALL \"usp_insert_document_metadata\" (@p_id, @p_name, @p_description, @p_dategory, @p_content_type, @p_length, @p_external_id, @p_created_at, @p_created_by)", metadataConnection))
            {
                await metadataConnection.OpenAsync(cancellationToken);
                cmd.Parameters.Add(new NpgsqlParameter("@p_id", DbType.Int32) { Value = -1, Direction = ParameterDirection.InputOutput });
                cmd.Parameters.AddWithValue("@p_name", document.Name);
                cmd.Parameters.AddWithValue("@p_description", document.Description);
                cmd.Parameters.AddWithValue("@p_dategory", document.Category);
                cmd.Parameters.AddWithValue("@p_content_type", document.ContentType);
                cmd.Parameters.AddWithValue("@p_length", document.Length);
                cmd.Parameters.AddWithValue("@p_external_id", document.ExternalId);
                cmd.Parameters.AddWithValue("@p_created_at", document.CreatedAt);
                cmd.Parameters.AddWithValue("@p_created_by", document.CreatedBy);
                await cmd.PrepareAsync(cancellationToken);
                await cmd.ExecuteNonQueryAsync(cancellationToken);
                var id = (int)cmd.Parameters["@p_id"].Value;
                await metadataConnection.CloseAsync();
                return id;
            }

        }

        public async Task<UserDocument> AssingUserPermissionAsync(UserDocument userDocument, CancellationToken cancellationToken)
        {
            using (var cmd = new NpgsqlCommand("udf_assig_user_document_permission", metadataConnection))
            {
                metadataConnection.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("p_userid", userDocument.UserId);
                cmd.Parameters.AddWithValue("p_documentid", userDocument.DocumentId);
                cmd.Prepare();

                using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        userDocument.UserName = reader["user_name"].ToString();
                        userDocument.DocumentName = reader["document_name"].ToString();
                        return userDocument;
                    }
                    return null;
                }
            }
        }

        public async Task<GroupDocument> AssingGroupPermissionAsync(GroupDocument groupDocument, CancellationToken cancellationToken)
        {
            using (var cmd = new NpgsqlCommand("udf_assig_group_document_permission", metadataConnection))
            {
                metadataConnection.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("p_groupid", groupDocument.GroupId);
                cmd.Parameters.AddWithValue("p_documentid", groupDocument.DocumentId);
                cmd.Prepare();

                using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        groupDocument.GroupName = reader["group_name"].ToString();
                        groupDocument.DocumentName = reader["document_name"].ToString();
                        return groupDocument;
                    }
                    return null;
                }
            }
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
