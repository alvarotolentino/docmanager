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

namespace Infrastructure.Persistence.Repositories
{
    public class DocumentRepositoryAsync : IDocumentRepositoryAsync, IDisposable
    {
        private NpgsqlConnection connection;

        public DocumentRepositoryAsync(DbConnection dbConnection)
        {
            this.connection = (NpgsqlConnection)dbConnection;
        }

        public async Task<bool> DeleteDocumentById(Document document, CancellationToken cancellationToken)
        {
            using (var cmd = new NpgsqlCommand("CALL \"usp_delete_document\" (@p_id)", connection))
            {
                connection.Open();
                cmd.Parameters.Add(new NpgsqlParameter("@p_id", DbType.Int32) { Value = document.Id, Direction = ParameterDirection.InputOutput });
                await cmd.ExecuteNonQueryAsync(cancellationToken);
                var result = (int)cmd.Parameters["@p_id"].Value;
                connection.Close();
                return result > -1;
            }
        }

        public async Task<Document> GetDocumentDataById(UserDocument userDocument, CancellationToken cancellationToken)
        {
            using (var cmd = new NpgsqlCommand("udf_get_document_data_by_id", connection))
            {
                connection.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("p_documentid", userDocument.DocumentId);
                cmd.Parameters.AddWithValue("p_userid", userDocument.UserId);
                cmd.Prepare();
                Document document = null;
                using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    if (reader.HasRows)
                    {
                        document = new Document();
                        reader.Read();
                        document.Name = reader["name"].ToString();
                        document.ContentType = reader["content_type"].ToString();
                        document.Length = (long)reader["length"];
                        document.Data = (byte[])reader["data"];
                    }
                }
                connection.Close();
                return document;
            }
        }

        public async Task<Document> GetDocumentInfoById(UserDocument userDocument, CancellationToken cancellationToken)
        {
            using (var cmd = new NpgsqlCommand("udf_get_document_info_by_id", connection))
            {
                connection.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("p_documentid", userDocument.DocumentId);
                cmd.Parameters.AddWithValue("p_userid", userDocument.UserId);
                cmd.Prepare();
                Document document = null;
                using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        document = new Document();
                        document.Id = (int)reader["id"];
                        document.Name = reader["name"].ToString();
                        document.Description = reader["description"].ToString();
                        document.Category = reader["category"].ToString();
                        document.ContentType = reader["content_type"].ToString();
                        document.Length = (long)reader["length"];
                        document.CreatedBy = (int)reader["created_by"];
                        document.CreatedAt = (DateTime)reader["created_at"];
                        document.UpdatedBy = (int)reader["updated_by"];
                        document.UpdatedAt = (DateTime)reader["updated_at"];
                    }
                }
                connection.Close();
                return document;
            }
        }

        public async Task<IReadOnlyList<Document>> GetDocuments(GetUserDocumentsPaginated userDocumentsPaginated, CancellationToken cancellationToken)
        {
            using (var cmd = new NpgsqlCommand("udf_get_documents_by_page_number_size", connection))
            {
                connection.Open();
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
                connection.Close();
                return documents;
            }
        }

        public async Task<int> SaveDocument(Document document, CancellationToken cancellationToken)
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
                cmd.Parameters.AddWithValue("@p_created_at", document.CreatedAt);
                cmd.Parameters.AddWithValue("@p_created_by", document.CreatedBy);
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
