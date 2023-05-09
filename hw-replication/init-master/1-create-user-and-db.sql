DO
$do$
BEGIN
	IF NOT EXISTS (SELECT FROM   pg_catalog.pg_roles WHERE rolname = 'dbuser') THEN
    	CREATE ROLE dbuser WITH LOGIN PASSWORD 'dbpassword';
        CREATE ROLE replicator with login replication password 'reppassword';
	END IF;
END
$do$;

CREATE DATABASE otusdb
	WITH OWNER = dbuser
		ENCODING = 'utf8'
		TABLESPACE = pg_default
		LC_COLLATE = 'en_US.utf8'
		LC_CTYPE = 'en_US.utf8'
		CONNECTION LIMIT = -1;

GRANT CONNECT, TEMPORARY ON DATABASE otusdb TO public;
GRANT ALL ON DATABASE otusdb TO dbuser;

ALTER ROLE dbuser SUPERUSER;
