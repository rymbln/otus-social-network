FROM postgres:14-alpine as base

COPY 1-create-user-and-db.sql 2-allow-user-to-connect.sh 3-create-extensions.sh 4-create-tables.sql docker-entrypoint-initdb.d/

EXPOSE 5432
