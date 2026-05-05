#!/usr/bin/env sh
set -e

TEMPLATE="/usr/share/nginx/html/assets/config.example.json"
TARGET="/usr/share/nginx/html/assets/config.json"

if [ -f "$TEMPLATE" ]; then
  echo "INFO: Generating runtime config: $TARGET"

  envsubst < "$TEMPLATE" > "$TARGET"
else
  echo "WARN: $TEMPLATE nem található, runtime config generálás kihagyva."
fi

exec "$@"