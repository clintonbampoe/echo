#!/usr/bin/env bash
# Adds sign-in accounts to every church, so a big run has about one account per virtual user. With the seeded ~200
# accounts, 1,000 users share one login in fives and hit the per-user rate limit, which real people would not.
#
#   ACCOUNTS_PER_CHURCH   accounts to add to each church (default 50)
#
# Each is <role>.load<N>@<the church's domain>, with the same password as that church's seeded admin (admin@123). A church
# without a seeded admin (admin@<domain>, password never changed) gets none: a hand-made church has a password of its own.
# The roles are mostly members, the rest clerks, accountants and admins. Safe to run again: accounts that exist are
# skipped. Re-seeding removes them. Run ./prepare.sh afterwards to pick them up.
set -euo pipefail

per_church=${ACCOUNTS_PER_CHURCH:-50}

docker exec database psql -U postgres -d echo_db -v ON_ERROR_STOP=1 -c "
  INSERT INTO users (id, congregation_id, email_address, password_hash, role, email_verified_at, first_name, last_name, created_at)
  SELECT gen_random_uuid(), a.congregation_id,
         lower(r.role) || '.load' || n || '@' || split_part(a.email_address, '@', 2),
         a.password_hash, r.role, a.created_at + interval '1 day', 'Load', 'User ' || n, a.created_at + interval '1 day'
  FROM (
    SELECT DISTINCT ON (congregation_id) congregation_id, email_address, password_hash, created_at
    FROM users WHERE role = 'Admin' AND deleted_at IS NULL AND email_address LIKE 'admin@%' AND password_changed_at IS NULL
    ORDER BY congregation_id, created_at
  ) a
  CROSS JOIN generate_series(1, ${per_church}) AS n
  CROSS JOIN LATERAL (SELECT (ARRAY['Member','Member','Clerk','Accountant','Admin'])[1 + n % 5] AS role) r
  ON CONFLICT (email_address) WHERE deleted_at IS NULL DO NOTHING"
