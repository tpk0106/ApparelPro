#!/bin/bash
# ---------------------------------------------------------------------------
# 1. Starts sqlservr in the background.
# 2. Polls with sqlcmd until it accepts connections.
# 3. If a .bak file is present in /var/opt/mssql/backup AND the database
#    hasn't already been restored (tracked via a marker file), restores it
#    as ${DB_NAME} using restore.sql.
# 4. Waits on the sqlservr process so the container keeps running normally.
# Runs automatically every container start; the marker file means the
# restore only actually happens once.
# ---------------------------------------------------------------------------
set -e

/opt/mssql/bin/sqlservr &
SQLPID=$!

echo "[entrypoint] Waiting for SQL Server to accept connections..."
READY=0
for i in $(seq 1 60); do
  if /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -Q "SELECT 1" > /dev/null 2>&1; then
    READY=1
    break
  fi
  sleep 2
done

if [ "$READY" -ne 1 ]; then
  echo "[entrypoint] SQL Server did not become ready in time." >&2
  wait $SQLPID
  exit 1
fi

echo "[entrypoint] SQL Server is up."

MARKER="/var/opt/mssql/data/.restored_${DB_NAME}"
BACKUP_FILE=$(ls /var/opt/mssql/backup/*.bak 2>/dev/null | head -n 1 || true)

if [ -f "$MARKER" ]; then
  echo "[entrypoint] ${DB_NAME} already restored previously (marker found) — skipping."
elif [ -z "$BACKUP_FILE" ]; then
  echo "[entrypoint] No .bak file found in db/backup/ — skipping restore. Database will start empty."
else
  echo "[entrypoint] Restoring ${DB_NAME} from ${BACKUP_FILE} ..."
  /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$MSSQL_SA_PASSWORD" \
    -v BACKUPFILE="$BACKUP_FILE" DBNAME="$DB_NAME" \
    -i /scripts/restore.sql
  touch "$MARKER"
  echo "[entrypoint] Restore complete."
fi

wait $SQLPID
