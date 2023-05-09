#!/bin/bash
# allow users to connect
sed -i 's/host all all all md5//g' /var/lib/postgresql/data/pg_hba.conf
echo '# CONNECTIONS' >> /var/lib/postgresql/data/pg_hba.conf
echo 'host    otusdb      dbuser     0.0.0.0/0    md5' >> /var/lib/postgresql/data/pg_hba.conf
echo 'host    all         postgres     172.0.0.0/8    md5' >> /var/lib/postgresql/data/pg_hba.conf
echo 'host    replication         replicator     172.0.0.0/8    md5' >> /var/lib/postgresql/data/pg_hba.conf

# Update pg.conf
echo 'ssl = off' >> /var/lib/postgresql/data/postgresql.conf
echo 'wal_level = replica' >> /var/lib/postgresql/data/postgresql.conf
echo 'max_wal_senders = 4' >> /var/lib/postgresql/data/postgresql.conf

# restart postgres
pg_ctl reload
