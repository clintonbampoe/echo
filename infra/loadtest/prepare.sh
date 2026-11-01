#!/usr/bin/env bash
# Writes the seeded accounts (email, role, church) to .accounts.json. Each virtual user signs in with
# its account by itself when the test starts, so nothing here talks to the API.
#
#   PER_TENANT   accounts per church, picked so every role is covered (default: all of them)
#   FOCUS_TENANT church that always gets every account (default Grace Chapel International)
#
# Run it again after re-seeding.
set -euo pipefail
cd "$(dirname "$0")"

PER_TENANT=${PER_TENANT:-1000}
FOCUS_TENANT=${FOCUS_TENANT:-Grace Chapel International}

# Each church's accounts are taken one role at a time (admin, clerk, accountant, member, admin again...), so
# a church capped at four does not end up with four admins.
docker exec database psql -U postgres -d echo_db -At -F'|' -c "
  SELECT email, role, tenant FROM (
    SELECT email, role, tenant,
           row_number() OVER (
             PARTITION BY cid
             ORDER BY turn, array_position(ARRAY['Admin','Clerk','Accountant','Member'], role), created_at
           ) AS n
    FROM (
      SELECT u.email_address AS email, u.role, c.name AS tenant, c.id AS cid, u.created_at,
             row_number() OVER (PARTITION BY c.id, u.role ORDER BY u.created_at) AS turn
      FROM users u JOIN congregations c ON c.id = u.congregation_id
      WHERE u.deleted_at IS NULL AND c.deleted_at IS NULL
        AND length(u.password_hash) = 60 AND left(u.password_hash, 1) = chr(36)
    ) ranked
  ) x
  WHERE tenant = '${FOCUS_TENANT//\'/\'\'}' OR n <= ${PER_TENANT}
  ORDER BY tenant, email" \
  | jq -R -s 'split("\n") | map(select(length > 0) | split("|") | {email: .[0], role: .[1], tenant: .[2]})' \
  > .accounts.json

echo "$(jq length .accounts.json) accounts in .accounts.json"
jq -r '.[].role' .accounts.json | sort | uniq -c
