#!/bin/bash
touch standby.signal

# Update pg.conf
echo "primary_conninfo = 'host=pgmaster port=5432 user=replicator password=reppassword application_name=pgslave'" >> /var/lib/postgresql/data/postgresql.conf