#!/bin/sh
host="$1"
shift
until nc -z "$host" 1433; do
  echo "⏳ Aguardando SQL Server em $host:1433..."
  sleep 2
done
echo "✅ SQL Server está pronto!"
exec "$@"
