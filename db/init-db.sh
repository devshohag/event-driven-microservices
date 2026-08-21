#!/bin/bash
# Runs automatically the first time the postgres container's data volume is
# initialized. POSTGRES_DB (see docker-compose.yml) creates the first
# database (orderdb); this script creates the second one (inventorydb) so
# both services can share a single Postgres instance in this demo setup.
set -e

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    CREATE DATABASE inventorydb;
EOSQL
