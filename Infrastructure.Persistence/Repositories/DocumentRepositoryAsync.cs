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

namespace Infrastructure.Persistence.Repositories
{
    public class DocumentRepositoryAsync : IDocumentRepositoryAsync, IDisposable
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

        public async Task<bool> DeleteDocumentById(int id, CancellationToken cancellationToken)
        {
            using (var cmd = new NpgsqlCommand("CALL \"usp_delete_document\" (@p_id)", connection))
            {
                connection.Open();
                cmd.Parameters.Add(new NpgsqlParameter("@p_id", DbType.Int32) { Value = id, Direction = ParameterDirection.InputOutput });
                await cmd.ExecuteNonQueryAsync(cancellationToken);
                var result = (int)cmd.Parameters["@p_id"].Value;
                connection.Close();
                return result > -1;
            }
        }

        public async Task<Documents> GetDocumentDataById(int id, CancellationToken cancellationToken)
        {
            using (var cmd = new NpgsqlCommand("udf_get_document_data_by_id", connection))
            {
                connection.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("p_id", id);
                cmd.Parameters.AddWithValue("p_userid", this.authenticatedUserService.UserId);
                cmd.Prepare();
                Documents documents = null;
                using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    if (reader.HasRows)
                    {
                        documents = new Documents();
                        reader.Read();
                        documents.Name = reader["name"].ToString();
                        documents.ContentType = reader["content_type"].ToString();
                        documents.Length = (long)reader["length"];
                        documents.Data = (byte[])reader["data"];
                    }
                }
                connection.Close();
                return documents;
            }
        }

        public async Task<Documents> GetDocumentInfoById(int id, CancellationToken cancellationToken)
        {
            using (var cmd = new NpgsqlCommand("udf_get_document_info_by_id", connection))
            {
                connection.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("p_id", id);
                cmd.Parameters.AddWithValue("p_userid", this.authenticatedUserService.UserId);
                cmd.Prepare();
                Documents documents = null;
                using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        documents = new Documents();
                        documents.Id = (int)reader["id"];
                        documents.Name = reader["name"].ToString();
                        documents.Description = reader["description"].ToString();
                        documents.Category = reader["category"].ToString();
                        documents.ContentType = reader["content_type"].ToString();
                        documents.Length = (long)reader["length"];
                        documents.CreatedBy = (int)reader["created_by"];
                        documents.CreatedAt = (DateTime)reader["created_at"];
                        documents.UpdatedBy = (int)reader["updated_by"];
                        documents.UpdatedAt = (DateTime)reader["updated_at"];
                    }
                }
                connection.Close();
                return documents;
            }
        }

        public async Task<IReadOnlyList<Documents>> GetDocuments(int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            using (var cmd = new NpgsqlCommand("udf_get_documents_by_page_number_size", connection))
            {
                connection.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("p_number", pageNumber);
                cmd.Parameters.AddWithValue("p_size", pageSize);
                cmd.Parameters.AddWithValue("p_userid", this.authenticatedUserService.UserId);
                cmd.Prepare();

                List<Documents> documents = null;
                using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    if (reader.HasRows)
                    {
                        documents = new List<Documents>();
                        while (reader.Read())
                        {
                            var document = new Documents
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
                connection.Close();
                return documents;
            }
        }

        public async Task<int> SaveDocument(Domain.Entities.Documents document, CancellationToken cancellationToken)
        {
            using (var cmd = new NpgsqlCommand("CALL \"usp_insert_document\" (@p_id, @p_name, @p_description, @p_dategory, @p_content_type, @p_length, @p_data, @p_created_at, @p_created_by)", connection))
            {
                connection.Open();
                cmd.Parameters.Add(new NpgsqlParameter("@p_id", DbType.Int32) { Value = -1, Direction = ParameterDirection.InputOutput });
                cmd.Parameters.AddWithValue("@p_name", document.Name);
                cmd.Parameters.AddWithValue("@p_description", document.Description);
                cmd.Parameters.AddWithValue("@p_dategory", document.Category);
                cmd.Parameters.AddWithValue("@p_content_type", document.ContentType);
                cmd.Parameters.AddWithValue("@p_length", document.Length);
                cmd.Parameters.AddWithValue("@p_data", document.Data);
                cmd.Parameters.AddWithValue("@p_created_at", this.dateTimeService.UtcDateTime);
                cmd.Parameters.AddWithValue("@p_created_by", this.authenticatedUserService.UserId);
                cmd.Prepare();
                await cmd.ExecuteNonQueryAsync(cancellationToken);
                var id = (int)cmd.Parameters["@p_id"].Value;
                connection.Close();
                return id;
            }

        }

        public void Dispose()
        {
            if (this.connection.State == ConnectionState.Open)
            {
                this.connection.Close();
            }
        }

        public async Task<UserDocument> AssingUserPermissionAsync(UserDocument userDocument, CancellationToken cancellationToken)
        {
            using (var cmd = new NpgsqlCommand("udf_assig_user_document_permission", connection))
            {
                connection.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("p_userid", userDocument.UserId);
                cmd.Parameters.AddWithValue("p_documentid", userDocument.DocumentId);

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
            using (var cmd = new NpgsqlCommand("udf_assig_group_document_permission", connection))
            {
                connection.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("p_groupid", groupDocument.GroupId);
                cmd.Parameters.AddWithValue("p_documentid", groupDocument.DocumentId);

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
    }
}
