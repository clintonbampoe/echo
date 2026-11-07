#!/bin/sh
# Runs a command, then tells Gatus whether it worked: heartbeat.sh <endpoint key> <command...>
key=$1
shift
if "$@"; then ok=true; else ok=false; fi
wget -q -T 5 -O /dev/null --header "Authorization: Bearer $STATUS_HEARTBEAT_TOKEN" --post-data '' \
  "http://status:8080/api/v1/endpoints/$key/external?success=$ok" || true
[ "$ok" = true ]
