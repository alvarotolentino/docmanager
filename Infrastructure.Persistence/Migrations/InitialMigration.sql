do $$
begin

    raise info 'Starting creating tables';

    create table if not exists "document" (
        id integer not null generated always as identity primary key,
        name character varying not null,
        description character varying not null,
        category character varying not null,
        content_type character varying not null,
        length bigint not null,
        data bytea not null,
        created_by integer null,
        created_at timestamp without time zone null,
        updated_by integer null,
        updated_at timestamp without time zone null
    );

    create table if not exists "role" (
        id integer not null generated always as identity primary key,
        name character varying null,
        created_by integer null,
        created_at timestamp without time zone null,
        updated_by integer null,
        updated_at timestamp without time zone null
    );

    create table if not exists "group" (
        id integer not null generated always as identity primary key,
        name character varying not null,
        created_by integer null,
        created_at timestamp without time zone null,
        updated_by integer null,
        updated_at timestamp without time zone null
    );

    create table if not exists "user" (
        id integer not null generated always as identity primary key,
        first_name character varying null,
        last_name character varying null,
        user_name character varying null,
        normalized_user_name character varying null,
        email character varying null,
        normalized_email character varying null,
        email_confirmed boolean null,
        password_hash text null,
        security_stamp text null,
        concurrency_stamp text null,
        phone_number text null,
        phone_number_confirmed boolean null,
        two_factor_enabled boolean null,
        lockout_end timestamp with time zone null,
        lockout_enabled boolean null,
        access_failed_count integer null,
        created_by integer null,
        created_at timestamp without time zone null,
        updated_by integer null,
        updated_at timestamp without time zone null
    );

    create table if not exists user_role (
        user_id integer not null,
        role_id integer not null,
        constraint "pk_user_role" primary key (user_id, role_id),
        constraint "fk_user_role_role_role_id" foreign key (role_id) references role (id) on delete cascade,
        constraint "fk_user_role_user_user_id" foreign key (user_id) references "user" (id) on delete cascade
    );

    create table if not exists user_group (
        user_id integer not null,
        group_id integer not null,
        constraint "pk_user_group" primary key (user_id, group_id),
        constraint "fk_user_group_user_id" foreign key (user_id) references "user" (id) on delete cascade,
        constraint "fk_user_group_group_id" foreign key (group_id) references "group" (id) on delete cascade
    );

    create table if not exists document_user_permission (
        document_id integer not null,
        user_id integer not null,
        constraint "pk_document_user" primary key (document_id, user_id),
        constraint "fk_document_user_document_id" foreign key (document_id) references "document" (id) on delete cascade,
        constraint "fk_document_user_user_id" foreign key (user_id) references "user" (id) on delete cascade
    );

    create table if not exists document_group_permission (
        document_id integer not null,
        group_id integer not null,
        constraint "pk_document_group" primary key (document_id, group_id),
        constraint "fk_document_group_document_id" foreign key (document_id) references "document" (id) on delete cascade,
        constraint "fk_document_group_group_id" foreign key (group_id) references "group" (id) on delete cascade
    );
    raise info 'Finishing creating tables';

    raise info 'Starting creating indexes';

    create unique index if not exists "ix_role_name_index" on "role" (name);

    alter table "role" add constraint "unique_role_name" unique using index "ix_role_name_index";

    create index if not exists "ix_email_index" on "user" (email);

    create unique index if not exists "ix_user_name_index" on "user" (user_name);

    alter table "user" add constraint "unique_user_name" unique using index "ix_user_name_index";

    create unique index if not exists "ix_user_email_index" on "user" (email);

    alter table "user" add constraint "unique_user_email" unique using index "ix_user_email_index";

    create index if not exists "ix_user_role_role_id" on user_role (role_id);

    create index if not exists "ix_document_document_type" on "document" (content_type);

    create index if not exists "ix_document_category" on "document" (category);

    create index if not exists "ix_document_name" on "document" (name);

    create index if not exists "ix_user_group_group_id" on user_group (group_id);

    create index if not exists "ix_user_group_user_id" on user_group (user_id);

    create index if not exists "ix_document_user_permission_user_id" on document_user_permission (user_id);

    create index if not exists "ix_document_user_permission_document_id" on document_user_permission (document_id);
    
    create index if not exists "ix_document_group_permission_group_id" on document_group_permission (group_id);

    create index if not exists "ix_document_group_permission_document_id" on document_group_permission (document_id);

    create unique index if not exists "ix_group_name_index" on "group" (name);

    alter table "group" add constraint "unique_group_name" unique using index "ix_group_name_index";

    raise info 'Finishing creating indexes';
    
    raise info 'Starting creating functions and procedures';

    CREATE OR REPLACE PROCEDURE usp_delete_document(p_id INOUT integer = NULL)
    AS $BODY$
    BEGIN
        WITH delete_document AS (
            DELETE FROM "document" doc 
            WHERE doc.id = p_id
            RETURNING doc.id
        ) SELECT COALESCE(
            (SELECT id FROM delete_document LIMIT 1),
            -1
        ) INTO p_id;
    END
    $BODY$
    LANGUAGE plpgsql;

	CREATE OR REPLACE FUNCTION udf_get_documents_by_page_number_size(p_number INTEGER = NULL,  p_size INTEGER = NULL, p_userid integer = NULL)
    RETURNS TABLE (id integer, name character varying, description character varying, category character varying, content_type character varying, length bigint, created_by integer, created_at timestamp, updated_by integer, updated_at timestamp) 
    AS
    $BODY$
        DECLARE
        page_offset INTEGER :=0;
        BEGIN
        
        page_offset := ((p_number-1) * p_size);

        RETURN QUERY
        WITH document_by_user AS (
            SELECT doc.id,
        		doc.name, doc.description, doc.category, doc.content_type, doc.length, doc.created_by, doc.created_at, doc.updated_by, doc.updated_at
            FROM "document" doc
            INNER JOIN document_user_permission dup ON doc.id = dup.document_id
            WHERE dup.user_id = p_userid
        ), document_by_group AS (
            SELECT doc.id,
        		doc.name, doc.description, doc.category, doc.content_type, doc.length, doc.created_by, doc.created_at, doc.updated_by, doc.updated_at
            FROM "document" doc
            INNER JOIN document_group_permission dgp ON doc.id = dgp.document_id
            INNER JOIN user_group ug ON dgp.group_id = ug.group_id
            WHERE ug.user_id = p_userid AND doc.id NOT IN (
				SELECT dbu.id FROM document_by_user dbu
			)
        ) 
		SELECT * FROM document_by_user
		UNION 
		SELECT * FROM document_by_group
        ORDER BY "id"
        OFFSET page_offset ROWS
        FETCH NEXT p_size ROWS ONLY;
        END;
    $BODY$
    LANGUAGE plpgsql;

    CREATE OR REPLACE FUNCTION udf_get_document_info_by_id(
    p_documentid integer = NULL, p_userid integer = NULL
    )
    RETURNS TABLE (id integer, name character varying, description character varying, category character varying, content_type character varying, length bigint, created_by integer, created_at timestamp, updated_by integer, updated_at timestamp) 
    AS
    $BODY$
    BEGIN

        RETURN QUERY
        WITH document_by_user AS (
            SELECT doc.id
            FROM "document" doc
            INNER JOIN document_user_permission dup ON doc.id = dup.document_id
            WHERE doc.id = p_documentid AND dup.user_id = p_userid
        ), document_by_group AS (
            SELECT doc.id
            FROM "document" doc
            INNER JOIN document_group_permission dgp ON doc.id = dgp.document_id
            INNER JOIN user_group ug ON dgp.group_id = ug.group_id
            WHERE doc.id = p_documentid AND ug.user_id = p_userid
        ) SELECT doc.id, doc.name, doc.description, doc.category, doc.content_type, doc.length, doc.created_by, doc.created_at, doc.updated_by, doc.updated_at
        FROM "document" doc
        WHERE doc.id = COALESCE((SELECT du.id FROM document_by_user du LIMIT 1), (SELECT dg.id FROM document_by_group dg LIMIT 1));

    END
    $BODY$
    LANGUAGE plpgsql;

	CREATE OR REPLACE FUNCTION udf_get_document_data_by_id(
    p_documentid integer = NULL, p_userid integer = NULL
    )
    RETURNS TABLE (name character varying, content_type character varying, length bigint, data bytea) 
    AS
    $BODY$
    BEGIN

        RETURN QUERY
        WITH document_by_user AS (
            SELECT doc.id
            FROM "document" doc
            INNER JOIN document_user_permission dup ON doc.id = dup.document_id
            WHERE doc.id = p_documentid AND dup.user_id = p_userid
        ), document_by_group AS (
            SELECT doc.id
            FROM "document" doc
            INNER JOIN document_group_permission dgp ON doc.id = dgp.document_id
            INNER JOIN user_group ug ON dgp.group_id = ug.group_id
            WHERE doc.id = p_documentid AND ug.user_id = p_userid
        ) SELECT doc.name, doc.content_type, doc.length, doc.data
        FROM "document" doc
        WHERE doc.id = COALESCE((SELECT id FROM document_by_user LIMIT 1), (SELECT id FROM document_by_group LIMIT 1));
    END
    $BODY$
    LANGUAGE plpgsql;

    CREATE OR REPLACE PROCEDURE usp_insert_group(p_id INOUT integer = NULL, p_name character varying = NULL, p_created_by integer = NULL, p_created_at timestamp with time zone = NULL)
    AS $BODY$
    BEGIN

        WITH new_group AS (
            INSERT INTO "group" (name, created_by, created_at, updated_by, updated_at) VALUES
            (p_name, p_created_by, p_created_at, p_created_by, p_created_at)
            ON CONFLICT ("name") DO NOTHING
            RETURNING id
        ) SELECT COALESCE (
            (SELECT id FROM new_group),
            (SELECT id FROM "group" WHERE "name" = p_name)
        ) INTO p_id;

    END
    $BODY$
    LANGUAGE plpgsql;

    CREATE OR REPLACE PROCEDURE usp_delete_group(p_id INOUT integer = NULL)
    AS $BODY$
    BEGIN
         WITH delete_group AS (
            DELETE FROM "group" gr 
            WHERE gr.id = p_id
            RETURNING u.id
        ) SELECT COALESCE(
            (SELECT id FROM delete_group LIMIT 1),
            -1
        ) INTO p_id;

    END
    $BODY$
    LANGUAGE plpgsql;

    CREATE OR REPLACE PROCEDURE usp_update_group(p_result INOUT INTEGER = NULL, p_id integer = NULL, p_name character varying = NULL, p_updated_by integer = null, p_updated_at timestamp with time zone = null)
    AS $BODY$
    BEGIN
        WITH group_updated AS (
            UPDATE "group" SET
            "name" = p_name,
            updated_by = p_updated_by,
            updated_at = p_updated_at
            WHERE "id" = p_id
            RETURNING id
        ) SELECT COALESCE (
            (SELECT id FROM group_updated),
            -1
        ) INTO p_result;
    END
    $BODY$
    LANGUAGE plpgsql;

	CREATE OR REPLACE FUNCTION udf_get_group_by_id(
    p_id integer = NULL
    )
    RETURNS TABLE (id integer, name character varying, created_by integer, created_at timestamp, updated_by integer, updated_at timestamp) 
    AS
    $BODY$
    BEGIN
        RETURN QUERY
        SELECT gr.id, gr.name, gr.created_by, gr.created_at, gr.updated_by, gr.updated_at
        FROM "group" gr
        WHERE gr.id = p_id;
    END
    $BODY$
    LANGUAGE plpgsql;

    CREATE OR REPLACE FUNCTION udf_add_user_to_group(
    p_userid integer = NULL, p_groupid integer = NULL
    )
    RETURNS TABLE (id integer, group_id integer, user_name character varying, name character varying) 
    AS
    $BODY$
    BEGIN
		INSERT INTO user_group (user_id, group_id)
        VALUES (p_userid, p_groupid)
        ON CONFLICT ON CONSTRAINT "pk_user_group" DO NOTHING;

        RETURN QUERY
        SELECT u.id, ug.group_id , u.user_name, gr.name
        FROM "user" u
		INNER JOIN user_group ug ON u.id = ug.user_id
		INNER JOIN "group" gr ON ug.group_id = gr.id
        WHERE u.id = p_userid AND gr.id = p_groupid;
    END
    $BODY$
    LANGUAGE plpgsql;

    CREATE OR REPLACE PROCEDURE usp_delete_account(p_id INOUT integer = NULL)
    AS $BODY$
    BEGIN
         WITH delete_user AS (
            DELETE FROM "user" u 
            WHERE u.id = p_id
            RETURNING u.id
        ) SELECT COALESCE(
            (SELECT id FROM delete_user LIMIT 1),
            -1
        ) INTO p_id;
    END
    $BODY$
    LANGUAGE plpgsql;

    CREATE OR REPLACE PROCEDURE usp_insert_document(p_id INOUT integer = NULL,p_name character varying = NULL,p_description character varying = NULL,p_dategory character varying = NULL,p_content_type character varying = NULL,p_length bigint = NULL,p_data bytea = NULL,p_created_at timestamp with time zone = NULL,p_created_by integer = NULL)
    AS $BODY$
    BEGIN
        WITH save_document AS (
            INSERT INTO "document" (name,description,category,content_type,length,data,created_by,created_at,updated_by,updated_at) VALUES
		    (p_name,p_description,p_dategory,p_content_type,p_length,p_data,p_created_by,p_created_at,p_created_by,p_created_at) RETURNING "id"
        )
        INSERT INTO document_user_permission (user_id, document_id)
        SELECT p_created_by, (SELECT id FROM save_document)
        RETURNING document_id INTO p_id;
    END
    $BODY$
    LANGUAGE plpgsql;

    CREATE OR REPLACE FUNCTION udf_get_groups_by_page_number_size(p_number INTEGER = NULL,  p_size INTEGER = NULL)
    RETURNS TABLE (id integer, name character varying, created_by integer, created_at timestamp, updated_by integer, updated_at timestamp) 
    AS
    $BODY$
        DECLARE
        page_offset INTEGER :=0;
        BEGIN
        
        page_offset := ((p_number-1) * p_size);
        
        RETURN QUERY
        SELECT gr.id, gr.name, gr.created_by, gr.created_at, gr.updated_by, gr.updated_at
        FROM "group" gr
        ORDER BY gr.id
        OFFSET page_offset ROWS
        FETCH NEXT p_size ROWS ONLY;
        END;
    $BODY$
    LANGUAGE plpgsql;

    CREATE OR REPLACE FUNCTION udf_assig_role_to_user(
    p_userid integer = NULL, p_roleid integer = NULL
    )
    RETURNS TABLE (id integer, role_id integer, user_name character varying, name character varying) 
    AS
    $BODY$
    BEGIN

		INSERT INTO user_role
        VALUES (p_userid, p_roleid)
        ON CONFLICT ON CONSTRAINT pk_user_role DO NOTHING;
		
        RETURN QUERY
        SELECT u.id, ur.role_id , u.user_name, r.name
        FROM "user" u
		INNER JOIN user_role ur ON u.id = ur.user_id
		INNER JOIN "role" r ON ur.role_id = r.id
        WHERE u.id = p_userid AND r.id = p_roleid;
    END
    $BODY$
    LANGUAGE plpgsql;

	CREATE OR REPLACE FUNCTION udf_create_account_default_role(
	 	p_id OUT integer,
        p_first_name character varying = NULL, 
        p_last_name character varying = NULL, 
        p_user_name character varying = NULL, 
        p_normalized_user_name character varying = NULL, 
        p_email character varying = NULL, 
        p_normalized_email character varying = NULL,
        p_password_hashed text = NULL,
        p_created_by integer = NULL,
        p_created_at timestamp with time zone = NULL
    )
    AS $BODY$
    DECLARE vrole_name character varying = 'Basic';
    BEGIN
        WITH new_user AS (
            INSERT INTO "user"(first_name,last_name,user_name,normalized_user_name,email,normalized_email, password_hash, created_by, created_at)
            VALUES (p_first_name, p_last_name, p_user_name, p_normalized_user_name, p_email, p_normalized_email, p_password_hashed, p_created_by, p_created_at)
            ON CONFLICT (email) DO NOTHING
            RETURNING id
        ), new_user_default_role AS (
            INSERT INTO user_role(user_id,role_id)
			SELECT u.id, (SELECT r.id FROM "role" r WHERE r.name = vrole_name LIMIT 1) FROM new_user u
            RETURNING role_id
        ) SELECT COALESCE(
            (SELECT u.id FROM new_user u LIMIT 1),
            -1
        )
        INTO p_id;
    END
    $BODY$
    LANGUAGE plpgsql;

    CREATE OR REPLACE FUNCTION udf_find_user_by_email (
        p_email character varying = NULL
    )
    RETURNS TABLE (user_id integer, first_name character varying, last_name character varying, user_name character varying, password_hash text, email character varying)
    AS $BODY$
    BEGIN 
		RETURN QUERY
        SELECT u.id AS user_id ,u.first_name,u.last_name,u.user_name,u.password_hash, u.email
        FROM "user" u
        WHERE u.email = p_email;
    END
    $BODY$
    LANGUAGE plpgsql;
	
    CREATE OR REPLACE FUNCTION udf_find_user_roles_by_email (
        p_email character varying = NULL
    )
    RETURNS TABLE (id integer, name character varying)
    AS $BODY$
    BEGIN 
		RETURN QUERY
        SELECT r.id, r.name 
		FROM "role" r
		INNER JOIN "user_role" ur ON r.id = ur.role_id
		INNER JOIN "user" u ON ur.user_id = u.id
        WHERE u.email = p_email;
    END
    $BODY$
    LANGUAGE plpgsql;
	
    CREATE OR REPLACE FUNCTION udf_find_user_groups_by_email (
        p_email character varying = NULL
    )
    RETURNS TABLE (id integer, name character varying)
    AS $BODY$
    BEGIN 
		RETURN QUERY
        SELECT g.id, g.name 
		FROM "group" g
		INNER JOIN "user_group" ug ON g.id = ug.group_id
		INNER JOIN "user" u ON ug.user_id = u.id
        WHERE u.email = p_email;
    END
    $BODY$
    LANGUAGE plpgsql;

    CREATE OR REPLACE FUNCTION udf_assig_user_document_permission(
    p_userid integer = NULL, p_documentid integer = NULL
    )
    RETURNS TABLE (user_id integer, document_id integer, user_name character varying, document_name character varying) 
    AS
    $BODY$
    BEGIN

        INSERT INTO document_user_permission (user_id, document_id)
        SELECT val.user_id, val.document_id
        FROM (VALUES (p_userid, p_documentid)) val (user_id, document_id)
        JOIN "user" u ON val.user_id = u.id
        JOIN "document" d ON val.document_id = d.id
        ON CONFLICT ON CONSTRAINT "pk_document_user" DO NOTHING;

        RETURN QUERY
        SELECT u.id AS user_id, du.document_id , u.user_name, d.name as document_name
        FROM "user" u
        INNER JOIN document_user_permission du ON u.id = du.user_id
        INNER JOIN "document" d ON du.document_id = d.id
        WHERE u.id = p_userid AND d.id = p_documentid;

    END
    $BODY$
    LANGUAGE plpgsql;

    CREATE OR REPLACE FUNCTION udf_assig_group_document_permission(
    p_groupid integer = NULL, p_documentid integer = NULL
    )
    RETURNS TABLE (group_id integer, document_id integer, group_name character varying, document_name character varying) 
    AS
    $BODY$
    BEGIN
        INSERT INTO document_group_permission (document_id, group_id) 
        SELECT val.document_id, val.group_id
		FROM (VALUES (p_documentid, p_groupid)) val (document_id, group_id)
		JOIN "document" d ON val.document_id = d.id
		JOIN "group" gr ON val.group_id = gr.id
        ON CONFLICT ON CONSTRAINT "pk_document_group" DO NOTHING;
		
        RETURN QUERY
        SELECT g.id AS group_id, d.id AS document_id, g.name AS group_name, d.name as document_name
        FROM "group" g
		INNER JOIN document_group_permission dg ON g.id = dg.group_id
		INNER JOIN "document" d ON dg.document_id = d.id
        WHERE g.id = p_groupid AND d.id = p_documentid;
    END
    $BODY$
    LANGUAGE plpgsql;

	CREATE OR REPLACE FUNCTION udf_create_role(
	 	p_id OUT integer,
        p_name character varying = NULL, 
        p_created_by integer = NULL,
        p_created_at timestamp with time zone = NULL
    )
    AS $BODY$
    BEGIN
        WITH new_role AS (
            INSERT INTO "role"(name, created_by, created_at)
            VALUES (p_name, p_created_by, p_created_at)
            ON CONFLICT (name) DO NOTHING
            RETURNING id
        ) SELECT COALESCE(
            (SELECT r.id FROM new_role r LIMIT 1),
            -1
        )
        INTO p_id;
    END
    $BODY$
    LANGUAGE plpgsql;

    CREATE OR REPLACE PROCEDURE usp_delete_role(p_id INOUT integer = NULL)
    AS $BODY$
    BEGIN
         WITH delete_role AS (
            DELETE FROM "role" u 
            WHERE u.id = p_id
            RETURNING u.id
        ) SELECT COALESCE(
            (SELECT id FROM delete_role LIMIT 1),
            -1
        ) INTO p_id;
    END
    $BODY$
    LANGUAGE plpgsql;

	CREATE OR REPLACE FUNCTION udf_get_accounts_by_page_number_size(p_number INTEGER = NULL,  p_size INTEGER = NULL)
    RETURNS TABLE (id integer, email character varying) 
    AS
    $BODY$
        DECLARE
        page_offset INTEGER :=0;
        BEGIN
        
        page_offset := ((p_number-1) * p_size);

        RETURN QUERY
        SELECT u.id, u.email
        FROM "user" u
        ORDER BY u.id
        OFFSET page_offset ROWS
        FETCH NEXT p_size ROWS ONLY;
        END;
    $BODY$
    LANGUAGE plpgsql;

    raise info 'Finished creating functions and procedures';

    raise info 'Starting Seeding Default Data';
    WITH adminrole as (
        INSERT INTO "role" (name) 
        VALUES ('Admin')
        RETURNING id
    ), adminuser AS (
        INSERT INTO "user" (user_name, email, first_name, last_name, password_hash) 
        VALUES ('adminuser','adminuser@gmail.com','admin','user','AQAAAAEAACcQAAAAEI65WyFJ04Gr2mU8DZ3PIFKh7zbtf+RmCPiiHWBtu3CPZb6ZhpiYvj+95XSz6Q9fvQ==') 
        RETURNING id
    ) 
    INSERT INTO user_role (user_id, role_id)
    SELECT (SELECT id FROM adminuser LIMIT 1), (SELECT id FROM adminrole LIMIT 1);

    WITH managerrole as (
        INSERT INTO "role" (name) 
        VALUES ('Manager')
        RETURNING id
    ), manageruser AS (
        INSERT INTO "user" (user_name, email, first_name, last_name, password_hash) 
        VALUES ('manageruser','manageruser@gmail.com','manager','user','AQAAAAEAACcQAAAAEK1fPzjouJ5mYnpXgLuRt4MhIC3vfj+E5n1GM+r28ZstSdtl40P3P8VNbu1hORkOgQ==') 
        RETURNING id
    ) 
    INSERT INTO user_role (user_id, role_id)
    SELECT (SELECT id FROM manageruser LIMIT 1), (SELECT id FROM managerrole LIMIT 1);

    WITH basicrole as (
        INSERT INTO "role" (name) 
        VALUES ('Basic')
        RETURNING id
    ), basicuser AS (
        INSERT INTO "user" (user_name, email, first_name, last_name, password_hash) 
        VALUES ('basicuser','basicuser@gmail.com','basic','user','AQAAAAEAACcQAAAAEB7yBf7+ROXY2A2+we48QIiP9Xz5FJwpaVAPcyohsboTnWqal4OOwyeSDdLVZmMaqQ==') 
        RETURNING id
    ) 
    INSERT INTO user_role (user_id, role_id)
    SELECT (SELECT id FROM basicuser LIMIT 1), (SELECT id FROM basicrole LIMIT 1);

    raise info 'Finished Seeding Default Data';


end
$$ language plpgsql;
