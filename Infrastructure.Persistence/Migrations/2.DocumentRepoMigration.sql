do $$
begin

    raise info 'Starting creating tables';

    create table if not exists "document_data" (
        id integer not null generated always as identity primary key,
        data bytea not null
    );

    raise info 'Finishing creating tables';
    raise info 'Starting creating functions and procedures';

    CREATE OR REPLACE FUNCTION udf_get_document_data_by_id(
        p_document_id integer = NULL
    )
    RETURNS TABLE (id integer, data bytea) 
    AS
    $BODY$
    BEGIN
        RETURN QUERY
        SELECT doc.id, doc.data
        FROM "document_data" doc
        WHERE doc.id = p_document_id;
    END
    $BODY$
    LANGUAGE plpgsql;

    CREATE OR REPLACE PROCEDURE usp_insert_document_data(p_id INOUT integer = NULL, p_data bytea = NULL)
    AS $BODY$
    BEGIN
        WITH document_inserted AS (
            INSERT INTO "document_data" (data)
            VALUES (p_data) 
            RETURNING "id"
        ) SELECT COALESCE(
            (SELECT id FROM document_inserted LIMIT 1),
            -1
        ) INTO p_id;
    END
    $BODY$
    LANGUAGE plpgsql;

    CREATE OR REPLACE PROCEDURE udf_delete_document_data(p_id INOUT integer = NULL)
    AS $BODY$
    BEGIN
        WITH delete_document AS (
            DELETE FROM "document_data" doc 
            WHERE doc.id = p_id
            RETURNING doc.id
        ) SELECT COALESCE(
            (SELECT id FROM delete_document LIMIT 1),
            -1
        ) INTO p_id;
    END
    $BODY$
    LANGUAGE plpgsql;

end
$$ language plpgsql;