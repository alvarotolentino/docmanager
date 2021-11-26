do $$
begin

    raise info 'Starting creating tables';

    create table if not exists "documents" (
        id bigint not null generated always as identity primary key,
        name character varying not null,
        description character varying not null,
        category character varying not null,
        content_type character varying not null,
        length bigint not null,
        data bytea not null,
        created_by bigint null,
        created_at timestamp without time zone null,
        updated_by bigint null,
        updated_at timestamp without time zone null
    );

    create table if not exists "role" (
        id bigint not null generated always as identity primary key,
        name character varying null
    );

    create table if not exists "group" (
        id bigint not null generated always as identity primary key,
        name character varying not null,
        created_by bigint null,
        created_at timestamp without time zone null,
        updated_by bigint null,
        updated_at timestamp without time zone null
    );

    create table if not exists "user" (
        id bigint not null generated always as identity primary key,
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
        created_by bigint null,
        created_at timestamp without time zone null,
        updated_by bigint null,
        updated_at timestamp without time zone null
    );

    create table if not exists role_claims (
        id integer not null generated always as identity primary key,
        role_id bigint not null,
        claim_type text null,
        claim_value text null,
        constraint "FK_role_claims_role_role_id" foreign key (role_id) references role (id) on delete cascade deferrable
    );

    create table if not exists refresh_token (
        id integer not null generated always as identity primary key,
        token character varying null,
        expires timestamp without time zone not null,
        created timestamp without time zone not null,
        created_by_ip character varying null,
        revoked timestamp without time zone null,
        revoked_by_ip character varying null,
        replaced_by_token character varying null,
        user_id bigint null
    );

    create table if not exists user_claim (
        id integer not null generated always as identity primary key,
        user_id bigint not null,
        claim_type text null,
        claim_value text null,
        constraint "FK_user_claim_user_user_id" foreign key (user_id) references "user" (id) on delete cascade deferrable
    );

    create table if not exists user_login (
        login_provider text not null,
        provider_key text not null,
        provider_display_name text null,
        user_id bigint not null,
        constraint "PK_user_login" primary key (login_provider, provider_key),
        constraint "FK_user_login_user_user_id" foreign key (user_id) references "user" (id) on delete cascade deferrable
    );

    create table if not exists user_role (
        user_id bigint not null,
        role_id bigint not null,
        constraint "PK_user_role" primary key (user_id, role_id),
        constraint "FK_user_role_role_role_id" foreign key (role_id) references role (id) on delete cascade deferrable,
        constraint "FK_user_role_user_user_id" foreign key (user_id) references "user" (id) on delete cascade deferrable
    );

    create table if not exists user_token (
        user_id bigint not null,
        login_provider text not null,
        name text not null,
        value text null,
        constraint "PK_user_token" primary key (user_id, login_provider, name),
        constraint "FK_user_token_user_user_id" foreign key (user_id) references "user" (id) on delete cascade deferrable
    );

    create table if not exists user_group (
        user_id bigint not null,
        group_id bigint not null,
        constraint "PK_user_group" primary key (user_id, group_id),
        constraint "FK_user_group_user_id" foreign key (user_id) references "user" (id) on delete cascade deferrable,
        constraint "FK_user_group_group_id" foreign key (group_id) references "group" (id) on delete cascade deferrable
    );

    create table if not exists document_user_permission (
        document_id bigint not null,
        user_id bigint not null,
        constraint "PK_document_user" primary key (document_id, user_id),
        constraint "FK_document_user_document_id" foreign key (document_id) references documents (id) on delete cascade deferrable,
        constraint "FK_document_user_user_id" foreign key (user_id) references "user" (id) on delete cascade deferrable
    );

    create table if not exists document_group_permission (
        document_id bigint not null,
        group_id bigint not null,
        constraint "PK_document_group" primary key (document_id, group_id),
        constraint "FK_document_group_document_id" foreign key (document_id) references documents (id) on delete cascade deferrable,
        constraint "FK_document_group_group_id" foreign key (group_id) references "group" (id) on delete cascade deferrable
    );
    raise info 'Finishing creating tables';

    raise info 'Starting creating indexes';

    create unique index if not exists "IX_role_name_index" on "role" (name);

    create index if not exists "IX_role_claims_role_id" on role_claims (role_id);

    create index if not exists "IX_email_index" on "user" (email);

    create unique index if not exists "IX_user_name_index" on "user" (user_name);

    create index if not exists "IX_user_claim_user_id" on user_claim (user_id);

    create index if not exists "IX_user_login_user_id" on user_login (user_id);

    create index if not exists "IX_user_role_role_id" on user_role (role_id);

    create index if not exists "IX_document_document_type" on documents (content_type);

    create index if not exists "IX_document_category" on documents (category);

    create index if not exists "IX_document_name" on documents (name);

    create index if not exists "IX_user_group_group_id" on user_group (group_id);

    create index if not exists "IX_user_group_user_id" on user_group (user_id);

    create index if not exists "IX_document_user_permission_user_id" on document_user_permission (user_id);

    create index if not exists "IX_document_user_permission_document_id" on document_user_permission (document_id);
    
    create index if not exists "IX_document_group_permission_group_id" on document_group_permission (group_id);

    create index if not exists "IX_document_group_permission_document_id" on document_group_permission (document_id);

    raise info 'Finishing creating indexes';
    
    raise info 'Starting creating functions and procedures';

    CREATE OR REPLACE PROCEDURE usp_delete_document(p_id bigint = NULL)
    AS $BODY$
    BEGIN
        DELETE FROM documents doc WHERE doc.id = p_id;
    END
    $BODY$
    LANGUAGE plpgsql;

    CREATE OR REPLACE FUNCTION udf_get_all_documents()
    RETURNS TABLE (id bigint, name character varying, description character varying, category character varying, content_type character varying, length bigint, created_by bigint, created_at timestamp, updated_by bigint, updated_at timestamp) 
    LANGUAGE 'sql'
    AS $BODY$
        SELECT doc.id,
        doc.name, doc.description, doc.category, doc.content_type, doc.length, doc.created_by, doc.created_at, doc.updated_by, doc.updated_at
        FROM documents doc;  
    $BODY$;

    CREATE OR REPLACE FUNCTION udf_get_documents_by_page_number_size(p_number INTEGER = NULL,  p_size INTEGER = NULL)
    RETURNS TABLE (id bigint, name character varying, description character varying, category character varying, content_type character varying, length bigint, created_by bigint, created_at timestamp, updated_by bigint, updated_at timestamp) 
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
    RETURNS TABLE (id bigint, name character varying, description character varying, category character varying, content_type character varying, length bigint, created_by bigint, created_at timestamp, updated_by bigint, updated_at timestamp) 
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
    RETURNS TABLE (name character varying, content_type character varying, length bigint, data bytea) 
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

    CREATE OR REPLACE PROCEDURE usp_insert_group(p_id INOUT bigint = NULL, p_name character varying = NULL, p_created_by bigint = NULL, p_created_at timestamp with time zone = NULL)
    AS $BODY$
    BEGIN
        INSERT INTO "group" (name, created_by, created_at, updated_by, updated_at) VALUES
		(p_name, p_created_by, p_created_at, p_created_by, p_created_at) RETURNING "id" INTO p_id;
    END
    $BODY$
    LANGUAGE plpgsql;

    CREATE OR REPLACE PROCEDURE usp_delete_group(p_id bigint = NULL)
    AS $BODY$
    BEGIN
        DELETE FROM "group" gr WHERE gr.id = p_id;
    END
    $BODY$
    LANGUAGE plpgsql;

    CREATE OR REPLACE PROCEDURE usp_update_group(p_result INOUT INTEGER = NULL, p_id bigint = NULL, p_name character varying = NULL, p_updated_by bigint = null, p_updated_at timestamp with time zone = null)
    AS $BODY$
    BEGIN
        IF EXISTS (SELECT FROM "group" WHERE "id" = p_id) THEN
            UPDATE "group" SET
            "name" = p_name,
            updated_by = p_updated_by,
            updated_at = p_updated_at
            WHERE "id" = p_id
            RETURNING 1 INTO p_result;
        ELSE
			SELECT 0 
            INTO p_result;
        END IF;
    END
    $BODY$
    LANGUAGE plpgsql;

	CREATE OR REPLACE FUNCTION udf_get_group_by_id(
    p_id bigint = NULL
    )
    RETURNS TABLE (id bigint, name character varying, created_by bigint, created_at timestamp, updated_by bigint, updated_at timestamp) 
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
    p_userid bigint = NULL, p_groupid bigint = NULL
    )
    RETURNS TABLE (id bigint, group_id bigint, user_name character varying, name character varying) 
    AS
    $BODY$
    BEGIN

        IF NOT EXISTS (SELECT 1 FROM user_group ug WHERE ug.user_id = p_userid AND ug.group_id = p_groupid) THEN
			INSERT INTO user_group VALUES (p_userid, p_groupid);
		END IF;

        RETURN QUERY
        SELECT u.id, ug.group_id , u.user_name, gr.name
        FROM "user" u
		INNER JOIN user_group ug ON u.id = ug.user_id
		INNER JOIN "group" gr ON ug.group_id = gr.id
        WHERE u.id = p_userid AND gr.id = p_groupid;
    END
    $BODY$
    LANGUAGE plpgsql;

    CREATE OR REPLACE PROCEDURE usp_delete_account(p_id bigint = NULL)
    AS $BODY$
    BEGIN
        DELETE FROM "user" u WHERE u.id = p_id;
    END
    $BODY$
    LANGUAGE plpgsql;

    CREATE OR REPLACE PROCEDURE usp_insert_document(p_id INOUT bigint = NULL,p_name character varying = NULL,p_description character varying = NULL,p_dategory character varying = NULL,p_content_type character varying = NULL,p_length bigint = NULL,p_data bytea = NULL,p_created_at timestamp with time zone = NULL,p_created_by bigint = NULL)
    AS $BODY$
    BEGIN
        INSERT INTO "documents" (name,description,category,content_type,length,data,created_by,created_at,updated_by,updated_at) VALUES
		(p_name,p_description,p_dategory,p_content_type,p_length,p_data,p_created_by,p_created_at,p_created_by,p_created_at) RETURNING "id" INTO p_id;
    END
    $BODY$
    LANGUAGE plpgsql;

    CREATE OR REPLACE FUNCTION udf_get_groups_by_page_number_size(p_number INTEGER = NULL,  p_size INTEGER = NULL)
    RETURNS TABLE (id bigint, name character varying, created_by bigint, created_at timestamp, updated_by bigint, updated_at timestamp) 
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
    p_userid bigint = NULL, p_roleid bigint = NULL
    )
    RETURNS TABLE (id bigint, role_id bigint, user_name character varying, name character varying) 
    AS
    $BODY$
    BEGIN
		
		IF NOT EXISTS (SELECT 1 FROM user_role ur WHERE ur.user_id = p_userid AND ur.role_id = p_roleid) THEN
			INSERT INTO user_role VALUES (p_userid, p_roleid);
		END IF;
		
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
	 	p_id OUT bigint,
        p_user_exists OUT boolean,
        p_first_name character varying = NULL, 
        p_last_name character varying = NULL, 
        p_user_name character varying = NULL, 
        p_normalized_user_name character varying = NULL, 
        p_email character varying = NULL, 
        p_normalized_email character varying = NULL,
        p_password_hashed text = NULL,
        p_created_by bigint = NULL,
        p_created_at timestamp with time zone = NULL
    )
    AS $BODY$
    DECLARE vuser_id bigint;
    DECLARE vrole_name character varying = 'Basic';
    BEGIN
        IF EXISTS (SELECT FROM "user" WHERE user_name = p_user_name) THEN
            SELECT 0, TRUE INTO p_id, p_user_exists;
        ELSE
            WITH new_user AS (
                INSERT INTO "user" (first_name,last_name,user_name,normalized_user_name,email,normalized_email, password_hash, created_by, created_at)
                values (p_first_name, p_last_name, p_user_name, p_normalized_user_name, p_email, p_normalized_email, p_password_hashed, p_created_by, p_created_at)
                RETURNING "id"
            ), default_role AS (
                SELECT "id" FROM "role" WHERE name = vrole_name
            )
            INSERT INTO user_role(user_id,role_id) 
            SELECT (SELECT "id" FROM new_user LIMIT 1), (SELECT "id" FROM default_role LIMIT 1)
			RETURNING "user_id", FALSE INTO p_id, p_user_exists;
        END IF;
    END
    $BODY$
    LANGUAGE plpgsql;

    CREATE OR REPLACE FUNCTION udf_find_user_by_email (
        p_email character varying = NULL
    )
    RETURNS TABLE (user_id bigint, first_name character varying, last_name character varying, user_name character varying, password_hash text, role_id bigint, role_name character varying, email character varying)
    AS $BODY$
    BEGIN 
		RETURN QUERY
        SELECT u.id AS user_id ,u.first_name,u.last_name,u.user_name,u.password_hash, r.id AS role_id, r.name AS role_name, u.email
        FROM "user" u
        INNER JOIN "user_role" ur ON u.id = ur.user_id
        INNER JOIN "role" r on ur.role_id = r.id 
        WHERE u.email = p_email;
    END
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
