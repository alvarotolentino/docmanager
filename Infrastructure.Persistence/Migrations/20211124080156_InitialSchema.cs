using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Infrastructure.Persistence.Migrations
{
    public partial class InitialSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "documents",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    category = table.Column<string>(type: "text", nullable: true),
                    content_type = table.Column<string>(type: "text", nullable: true),
                    length = table.Column<long>(type: "bigint", nullable: false),
                    data = table.Column<byte[]>(type: "bytea", nullable: true),
                    created_by = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_documents", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "group",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_by = table.Column<long>(type: "bigint", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_group", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "role",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    first_name = table.Column<string>(type: "text", nullable: true),
                    last_name = table.Column<string>(type: "text", nullable: true),
                    user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    security_stamp = table.Column<string>(type: "text", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "role_claims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_id = table.Column<long>(type: "bigint", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_role_claims_asp_net_roles_identity_role_long_id",
                        column: x => x.role_id,
                        principalTable: "role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refresh_token",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    token = table.Column<string>(type: "text", nullable: true),
                    expires = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    created_by_ip = table.Column<string>(type: "text", nullable: true),
                    revoked = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    revoked_by_ip = table.Column<string>(type: "text", nullable: true),
                    replaced_by_token = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_token", x => x.id);
                    table.ForeignKey(
                        name: "fk_refresh_token_users_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_claims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_claims_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_group",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    group_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_group", x => new { x.user_id, x.group_id });
                    table.ForeignKey(
                        name: "fk_user_group_group_group_id",
                        column: x => x.group_id,
                        principalTable: "group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_group_users_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_logins",
                columns: table => new
                {
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    provider_key = table.Column<string>(type: "text", nullable: false),
                    provider_display_name = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_logins", x => new { x.login_provider, x.provider_key });
                    table.ForeignKey(
                        name: "fk_user_logins_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    role_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_user_roles_asp_net_roles_identity_role_long_id",
                        column: x => x.role_id,
                        principalTable: "role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_roles_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_tokens",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_tokens", x => new { x.user_id, x.login_provider, x.name });
                    table.ForeignKey(
                        name: "fk_user_tokens_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_refresh_token_user_id",
                table: "refresh_token",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "role",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_role_claims_role_id",
                table: "role_claims",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "user",
                column: "normalized_email");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "user",
                column: "normalized_user_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_claims_user_id",
                table: "user_claims",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_group_group_id",
                table: "user_group",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_logins_user_id",
                table: "user_logins",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_role_id",
                table: "user_roles",
                column: "role_id");

migrationBuilder.Sql(@"
DO $$
BEGIN
    CREATE OR REPLACE PROCEDURE usp_delete_document(p_id bigint = NULL)
    AS $BODY$
    BEGIN
        DELETE FROM documents doc WHERE doc.id = p_id;
    END
    $BODY$
    LANGUAGE plpgsql;

    CREATE OR REPLACE FUNCTION udf_get_all_documents()
    RETURNS TABLE (id bigint, name text, description text, category text, content_type text, length bigint, created_by bigint, created_at timestamp, updated_by bigint, updated_at timestamp) 
    LANGUAGE 'sql'
    AS $BODY$
        SELECT doc.id,
        doc.name, doc.description, doc.category, doc.content_type, doc.length, doc.created_by, doc.created_at, doc.updated_by, doc.updated_at
        FROM documents doc;  
    $BODY$;

    CREATE OR REPLACE FUNCTION udf_get_documents_by_page_number_size(p_number INTEGER = NULL,  p_size INTEGER = NULL)
    RETURNS TABLE (id bigint, name text, description text, category text, content_type text, length bigint, created_by bigint, created_at timestamp, updated_by bigint, updated_at timestamp) 
    AS
    $BODY$
        DECLARE
        page_offset INTEGER :=0;
        BEGIN
        
        page_offset := ((p_number-1) * p_size);
        
        RETURN QUERY
        SELECT doc.id,
        doc.name, doc.description, doc.category, doc.content_type, doc.length, doc.created_by, doc.created_at, doc.updated_by, doc.updated_at
        FROM documents doc
        ORDER BY doc.id
        OFFSET page_offset ROWS
        FETCH NEXT p_size ROWS ONLY;
        END;
    $BODY$
    LANGUAGE plpgsql;

    CREATE OR REPLACE FUNCTION udf_get_document_info_by_id(
    p_id bigint = NULL
    )
    RETURNS TABLE (id bigint, name text, description text, category text, content_type text, length bigint, created_by bigint, created_at timestamp, updated_by bigint, updated_at timestamp) 
    AS
    $BODY$
    BEGIN
        RETURN QUERY
        SELECT doc.id,
        doc.name, doc.description, doc.category, doc.content_type, doc.length, doc.created_by, doc.created_at, doc.updated_by, doc.updated_at
        FROM documents doc
        WHERE doc.id = p_id;
    END
    $BODY$
    LANGUAGE plpgsql;

    CREATE OR REPLACE FUNCTION udf_get_document_data_by_id(
    p_id bigint = NULL
    )
    RETURNS TABLE (name text, content_type text, length bigint, data bytea) 
    AS
    $BODY$
    BEGIN
        RETURN QUERY
        SELECT doc.name, doc.content_type, doc.length, doc.data
        FROM documents doc
        WHERE doc.id = p_id;
    END
    $BODY$
    LANGUAGE plpgsql;

    CREATE OR REPLACE PROCEDURE usp_insert_group(p_id INOUT bigint = NULL, p_name text = NULL, p_created_by bigint = NULL, p_created_at timestamp = NULL)
    AS $BODY$
    BEGIN
        INSERT INTO ""group"" (name, created_by, created_at, updated_by, updated_at) VALUES
		(p_name, p_created_by, p_created_at, p_created_by, p_created_at) RETURNING ""id"" INTO p_id;
    END
    $BODY$
    LANGUAGE plpgsql;

    CREATE OR REPLACE PROCEDURE usp_delete_group(p_id bigint = NULL)
    AS $BODY$
    BEGIN
        DELETE FROM ""group"" gr WHERE gr.id = p_id;
    END
    $BODY$
    LANGUAGE plpgsql;

    CREATE OR REPLACE PROCEDURE usp_update_group(p_id bigint = NULL, p_name text = NULL, p_updated_by bigint = null, p_updated_at timestamp = null)
    AS $BODY$
    BEGIN
        UPDATE ""group"" SET
		""name"" = p_name,
		updated_by = p_updated_by,
		updated_at = p_updated_at
		WHERE ""id"" = p_id;
    END
    $BODY$
    LANGUAGE plpgsql;

    CREATE OR REPLACE FUNCTION udf_get_group_by_id(
    p_id bigint = NULL
    )
    RETURNS TABLE (id bigint, name text, created_by bigint, created_at timestamp, updated_by bigint, updated_at timestamp) 
    AS
    $BODY$
    BEGIN
        RETURN QUERY
        SELECT gr.id, gr.name, gr.created_by, gr.created_at, gr.updated_by, gr.updated_at
        FROM ""group"" gr
        WHERE gr.id = p_id;
    END
    $BODY$
    LANGUAGE plpgsql;

    CREATE OR REPLACE FUNCTION udf_add_user_to_group(
    p_userid bigint = NULL, p_groupid bigint = NULL
    )
    RETURNS TABLE (id bigint, group_id bigint, user_name character varying, name text) 
    AS
    $BODY$
    BEGIN

        IF NOT EXISTS (SELECT 1 FROM user_group ug WHERE ug.user_id = p_userid AND ug.group_id = p_groupid) THEN
			INSERT INTO user_group VALUES (p_userid, p_groupid);
		END IF;

        RETURN QUERY
        SELECT u.id, ug.group_id , u.user_name, gr.name
        FROM ""user"" u
		INNER JOIN user_group ug ON u.id = ug.user_id
		INNER JOIN ""group"" gr ON ug.group_id = gr.id
        WHERE u.id = p_userid AND gr.id = p_groupid;
    END
    $BODY$
    LANGUAGE plpgsql;

    CREATE OR REPLACE PROCEDURE usp_delete_account(p_id bigint = NULL)
    AS $BODY$
    BEGIN
        DELETE FROM ""user"" u WHERE u.id = p_id;
    END
    $BODY$
    LANGUAGE plpgsql;

    CREATE OR REPLACE PROCEDURE usp_insert_document(p_id INOUT bigint = NULL,p_name text = NULL,p_description text = NULL,p_dategory text = NULL,p_content_type text = NULL,p_length bigint = NULL,p_data bytea = NULL,p_created_at timestamp = NULL,p_created_by bigint = NULL)
    AS $BODY$
    BEGIN
        INSERT INTO ""documents"" (name,description,category,content_type,length,data,created_by,created_at,updated_by,updated_at) VALUES
		(p_name,p_description,p_dategory,p_content_type,p_length,p_data,p_created_by,p_created_at,p_created_by,p_created_at) RETURNING ""id"" INTO p_id;
    END
    $BODY$
    LANGUAGE plpgsql;

    CREATE OR REPLACE FUNCTION udf_get_groups_by_page_number_size(p_number INTEGER = NULL,  p_size INTEGER = NULL)
    RETURNS TABLE (id bigint, name text, created_by bigint, created_at timestamp, updated_by bigint, updated_at timestamp) 
    AS
    $BODY$
        DECLARE
        page_offset INTEGER :=0;
        BEGIN
        
        page_offset := ((p_number-1) * p_size);
        
        RETURN QUERY
        SELECT gr.id, gr.name, gr.created_by, gr.created_at, gr.updated_by, gr.updated_at
        FROM ""group"" gr
        ORDER BY gr.id
        OFFSET page_offset ROWS
        FETCH NEXT p_size ROWS ONLY;
        END;
    $BODY$
    LANGUAGE plpgsql;

    CREATE OR REPLACE FUNCTION udf_assig_role_to_user(
    p_userid bigint = NULL, p_roleid bigint = NULL
    )
    RETURNS TABLE (id bigint, role_id bigint, user_name character varying, name character varying) 
    AS
    $BODY$
    BEGIN
		
		IF NOT EXISTS (SELECT 1 FROM user_roles ur WHERE ur.user_id = p_userid AND ur.role_id = p_roleid) THEN
			INSERT INTO user_roles VALUES (p_userid, p_roleid);
		END IF;
		
        RETURN QUERY
        SELECT u.id, ur.role_id , u.user_name, r.name
        FROM ""user"" u
		INNER JOIN user_roles ur ON u.id = ur.user_id
		INNER JOIN ""role"" r ON ur.role_id = r.id
        WHERE u.id = p_userid AND r.id = p_roleid;
    END
    $BODY$
    LANGUAGE plpgsql;

END
$$
            ");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"
DO $$
BEGIN
    DROP PROCEDURE IF EXISTS ""usp_delete_document""(bigint);
    DROP FUNCTION IF EXISTS ""udf_get_all_documents""();
    DROP FUNCTION IF EXISTS ""udf_get_documents_by_page_number_size""(integer, integer);
    DROP FUNCTION IF EXISTS ""udf_get_document_info_by_id""(bigint);
    DROP FUNCTION IF EXISTS ""udf_get_document_data_by_id""(bigint);
    DROP FUNCTION IF EXISTS ""udf_add_user_to_group""(bigint, bigint);
    DROP FUNCTION IF EXISTS ""udf_get_group_by_id""(bigint);
    DROP PROCEDURE IF EXISTS ""usp_delete_account""(bigint);
    DROP PROCEDURE IF EXISTS ""usp_insert_group""(bigint, text);
    DROP PROCEDURE IF EXISTS ""usp_delete_group""(bigint);
    DROP PROCEDURE IF EXISTS ""usp_update_group""(bigint,text);
END
$$
            ");
            
            migrationBuilder.DropTable(
                name: "documents");

            migrationBuilder.DropTable(
                name: "refresh_token");

            migrationBuilder.DropTable(
                name: "role_claims");

            migrationBuilder.DropTable(
                name: "user_claims");

            migrationBuilder.DropTable(
                name: "user_group");

            migrationBuilder.DropTable(
                name: "user_logins");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "user_tokens");

            migrationBuilder.DropTable(
                name: "group");

            migrationBuilder.DropTable(
                name: "role");

            migrationBuilder.DropTable(
                name: "user");
        }
    }
}
