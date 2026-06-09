BEGIN;

-- ============================================================
-- 1. CONGREGATIONS (Target: ~60 records)
-- ============================================================
CREATE TEMP TABLE temp_congregations AS
WITH ins AS (
  INSERT INTO "Congregations" ("Name")
  SELECT
    prefix[floor(random() * array_length(prefix, 1)) + 1] || ' ' ||
    locs[floor(random() * array_length(locs, 1)) + 1] || ' Assembly'
  FROM generate_series(1, 60) g
  CROSS JOIN (
    SELECT
      ARRAY['Grace', 'Lighthouse', 'New Life', 'Victory', 'Salvation', 'Redemption', 'Faith', 'Hope', 'Charity', 'Covenant'] AS prefix,
      ARRAY[
        'Accra Central', 'Bantama', 'Adum', 'Anaji', 'Airport Residential', 'Legon', 'Dansoman', 'Spintex', 'Labadi', 'Osu',
        'Kwesimintsim', 'Fijai', 'Nhyiaeso', 'Amakom', 'Asokwa', 'Tafo', 'Suame', 'Madina', 'Tema Community 1', 'Tema Community 6',
        'Ashaley Botwe', 'East Legon', 'West Legon', 'Achimota', 'Tesano', 'Kaneshie', 'Lapaz', 'Abeka', 'Chorkor', 'Mamprobi',
        'Korle Bu', 'James Town', 'Teshie', 'Nungua', 'Lashibi', 'Sakumono', 'Gbawe', 'Mallam', 'Weija', 'Kasoa',
        'Kwabenya', 'Adenta', 'Dome', 'Taifa', 'Oyarifa', 'Abokobi', 'Pantang', 'Dodowa', 'Prampram', 'Ada',
        'Koforidua', 'Ho', 'Cape Coast Central', 'Tamale Central', 'Sunyani Central', 'Techiman', 'Bolgatanga', 'Wa', 'Dambai'
      ] AS locs
  ) arrays
  RETURNING "CongregationId"
)
SELECT "CongregationId" FROM ins;

-- ============================================================
-- 2. USERS (Strict Boundary: Single System Administrator)
-- ============================================================
INSERT INTO "Users" ("CongregationId", "Name", "EmailAddress", "PasswordHash")
SELECT "CongregationId", 'Kwame Mensah', 'admin@echoapp.gh', '$2a$12$KIXJmTzZAWyIkDpIQ7p1.uNhf1yJEAqDEdSYG.Rk9a1NaJYplzpbO'
FROM temp_congregations
LIMIT 1;

-- ============================================================
-- 3. ASSET CATEGORIES (Target: ~420 cross-product records)
-- ============================================================
CREATE TEMP TABLE temp_asset_categories AS
WITH ins AS (
  INSERT INTO "AssetCategories" ("CongregationId", "Name")
  SELECT
    c."CongregationId",
    cats[g]
  FROM generate_series(1, 7) g
  CROSS JOIN (
    SELECT ARRAY['Audio Equipment', 'Musical Instruments', 'Furniture', 'IT Assets', 'Vehicles', 'Lighting', 'Kitchen Appliances'] AS cats
  ) arrays
  CROSS JOIN temp_congregations c
  RETURNING "Id", "CongregationId"
)
SELECT "Id" AS "CategoryId", "CongregationId" FROM ins;

-- ============================================================
-- 4. MEMBERS (Target: 400 records | Omit "Name" Generated Column)
-- ============================================================
CREATE TEMP TABLE temp_members AS
WITH ins AS (
  INSERT INTO "Members" (
    "CongregationId", "FirstName", "LastName", "OtherNames",
    "EmailAddress", "PhoneNumber", "DateOfBirth", "JoinedDate", "Gender",
    "ResidentialAddress", "City", "Hometown", "Region", "GpsAddress",
    "MaritalStatus", "NextOfKin", "EmergencyContactName", "EmergencyContactPhoneNumber",
    "MemberActivityStatus"
  )
  SELECT
    c."CongregationId",
    fn[f_idx] AS "FirstName",
    ln[l_idx] AS "LastName",
    mn[m_idx] AS "OtherNames",
    lower(fn[f_idx] || '.' || ln[l_idx] || g || '@echo-member.gh') AS "EmailAddress",
    '0' || (20 + floor(random() * 8))::text || (1000000 + floor(random() * 8999999))::text AS "PhoneNumber",
    ('1970-01-01'::date + (random() * 12000)::integer) AS "DateOfBirth",
    ('2015-01-01'::date + (random() * 3000)::integer) AS "JoinedDate",
    gender_val AS "Gender",
    'House No. ' || g || ', ' || cities[c_idx] AS "ResidentialAddress",
    cities[c_idx] AS "City",
    cities[floor(random() * array_length(cities, 1)) + 1] AS "Hometown",
    regions[r_idx] AS "Region",
    'GA-' || (100 + floor(random() * 899))::text || '-' || (1000 + floor(random() * 8999))::text AS "GpsAddress",
    marital[m_status_idx] AS "MaritalStatus",
    fn[floor(random() * array_length(fn, 1)) + 1] || ' ' || ln[l_idx] AS "NextOfKin",
    fn[floor(random() * array_length(fn, 1)) + 1] || ' ' || ln[l_idx] AS "EmergencyContactName",
    '0' || (20 + floor(random() * 8))::text || (1000000 + floor(random() * 8999999))::text AS "EmergencyContactPhoneNumber",
    activity[act_idx] AS "MemberActivityStatus"
  FROM generate_series(1, 400) g
  CROSS JOIN (
    SELECT
      ARRAY['Kwame', 'Kwasi', 'Kojo', 'Kwabena', 'Kofi', 'Yaw', 'Kweku', 'Emmanuel', 'Samuel', 'John', 'Isaac', 'Daniel', 'Ama', 'Yaa', 'Afia', 'Amma', 'Akua', 'Abena', 'Efe', 'Mary', 'Elizabeth', 'Grace', 'Theresa'] AS fn,
      ARRAY['Mensah', 'Osei', 'Appiah', 'Asare', 'Ansah', 'Gyasi', 'Agyeman', 'Boakye', 'Owusu', 'Danquah', 'Essien', 'Antwi', 'Aidoo', 'Baah', 'Donkor', 'Forjour', 'Koomson', 'Quansah'] AS ln,
      ARRAY['Asare', 'Boateng', 'Agyekum', 'Prempeh', 'Nti', 'Bonsu', 'Acheampong', 'Adu', 'Sarpong', 'Kyeremeh'] AS mn,
      ARRAY['Ahafo', 'Ashanti', 'Bono', 'BonoEast', 'Central', 'Eastern', 'GreaterAccra', 'NorthEast', 'Northern', 'Oti', 'Savannah', 'UpperEast', 'UpperWest', 'Volta', 'Western', 'WesternNorth'] AS regions,
      ARRAY['Accra', 'Kumasi', 'Takoradi', 'Tamale', 'Tema', 'Cape Coast', 'Koforidua', 'Sunyani', 'Obuasi', 'Ho'] AS cities,
      ARRAY['Single', 'Married'] AS marital,
      ARRAY['Active', 'Inactive', 'Archived'] AS activity
  ) arrays
  CROSS JOIN LATERAL (
    SELECT "CongregationId" FROM temp_congregations ORDER BY random() LIMIT 1
  ) c
  CROSS JOIN LATERAL (
    SELECT
      floor(random() * array_length(fn, 1)) + 1 AS f_idx,
      floor(random() * array_length(ln, 1)) + 1 AS l_idx,
      floor(random() * array_length(mn, 1)) + 1 AS m_idx,
      floor(random() * array_length(regions, 1)) + 1 AS r_idx,
      floor(random() * array_length(cities, 1)) + 1 AS c_idx,
      floor(random() * array_length(marital, 1)) + 1 AS m_status_idx,
      floor(random() * array_length(activity, 1)) + 1 AS act_idx,
      CASE WHEN floor(random() * 2) = 0 THEN 'Male' ELSE 'Female' END AS gender_val
  ) idx
  RETURNING "Id", "CongregationId"
)
SELECT "Id" AS "MemberId", "CongregationId" FROM ins;

-- ============================================================
-- 5. ORGANIZATIONS (Target: ~300 cross-product records)
-- ============================================================
CREATE TEMP TABLE temp_organizations AS
WITH ins AS (
  INSERT INTO "Organizations" ("CongregationId", "Name", "Description")
  SELECT
    c."CongregationId",
    org_names[g] || ' Ministry',
    'Official organization entity tracking group ' || g AS "Description"
  FROM generate_series(1, 5) g
  CROSS JOIN (
    SELECT ARRAY['Men''s Fellowship', 'Women''s Ministry', 'Youth Mass Choir', 'Evangelism Team', 'Ushering Board'] AS org_names
  ) arrays
  CROSS JOIN temp_congregations c
  RETURNING "Id", "CongregationId"
)
SELECT "Id" AS "OrganizationId", "CongregationId" FROM ins;

-- ============================================================
-- 6. PROJECT CATEGORIES (Target: ~180 cross-product records)
-- ============================================================
CREATE TEMP TABLE temp_project_categories AS
WITH ins AS (
  INSERT INTO "ProjectCategories" ("CongregationId", "Name")
  SELECT
    c."CongregationId",
    cats[g]
  FROM generate_series(1, 3) g
  CROSS JOIN (
    SELECT ARRAY['Building & Infrastructure', 'Welfare & Outreach', 'Equipment & Media Upgrade'] AS cats
  ) arrays
  CROSS JOIN temp_congregations c
  RETURNING "Id", "CongregationId"
)
SELECT "Id" AS "CategoryId", "CongregationId" FROM ins;

-- ============================================================
-- 7. TRANSACTION CATEGORIES (Target: ~240 cross-product records)
-- ============================================================
CREATE TEMP TABLE temp_transaction_categories AS
WITH ins AS (
  INSERT INTO "TransactionCategories" ("CongregationId", "Name", "CategoryType")
  SELECT
    c."CongregationId",
    cats[g],
    types[g]
  FROM generate_series(1, 4) g
  CROSS JOIN (
    SELECT
      ARRAY['Tithe & Offering', 'Donations', 'Salaries & Remuneration', 'Utility Bills'] AS cats,
      ARRAY['Income', 'Income', 'Expenditure', 'Expenditure'] AS types
  ) arrays
  CROSS JOIN temp_congregations c
  RETURNING "Id", "CongregationId", "CategoryType"
)
SELECT "Id" AS "CategoryId", "CongregationId", "CategoryType" FROM ins;

-- ============================================================
-- 8. ASSETS (Target: 100 records)
-- ============================================================
INSERT INTO "Assets" (
  "CongregationId", "CategoryId", "Name", "SerialNumber",
  "PurchaseDate", "PurchaseCost", "CurrentValue", "Status", "Description"
)
SELECT
  ac."CongregationId",
  ac."CategoryId",
  items[floor(random() * array_length(items, 1)) + 1] || ' Mk ' || g AS "Name",
  'SN-' || (100000 + floor(random() * 899999))::text AS "SerialNumber",
  ('2018-01-01'::date + (random() * 2000)::integer) AS "PurchaseDate",
  (random() * 15000 + 200)::numeric(18,2) AS "PurchaseCost",
  (random() * 10000 + 100)::numeric(18,2) AS "CurrentValue",
  astatus[floor(random() * array_length(astatus, 1)) + 1] AS "Status",
  'Standard inventory asset ' || g AS "Description"
FROM generate_series(1, 100) g
CROSS JOIN (
  SELECT
    ARRAY['Yamaha Motif', 'Behringer X32', 'Shure SM58', 'Epson Projector', 'Plastic Chair Set', 'Wooden Podium', 'HP ProLiant Server'] AS items,
    ARRAY['InUse', 'InStorage', 'UnderMaintenance', 'Liquidated'] AS astatus
) arrays
CROSS JOIN LATERAL (
  SELECT "CategoryId", "CongregationId" FROM temp_asset_categories ORDER BY random() LIMIT 1
) ac;

-- ============================================================
-- 9. PROJECTS (Target: 80 records)
-- ============================================================
CREATE TEMP TABLE temp_projects AS
WITH ins AS (
  INSERT INTO "Projects" (
    "CongregationId", "CategoryId", "ManagerId", "Name",
    "TargetAmount", "Status", "StartDate", "EndDate", "Description"
  )
  SELECT
    pc."CongregationId",
    pc."CategoryId",
    m."MemberId",
    proj[floor(random() * array_length(proj, 1)) + 1] || ' Code ' || g AS "Name",
    (random() * 100000 + 5000)::numeric(18,2) AS "TargetAmount",
    pstatus[floor(random() * array_length(pstatus, 1)) + 1] AS "Status",
    ('2025-01-01'::date + (g * 2)) AS "StartDate",
    ('2026-12-31'::date) AS "EndDate",
    'Detailed execution strategy profiles for instance line ' || g AS "Description"
  FROM generate_series(1, 80) g
  CROSS JOIN (
    SELECT
      ARRAY['Sanctuary Expansion', 'Mission House Renovation', 'Borehole Installation', 'Bus Procurement Plan'] AS proj,
      ARRAY['Planning', 'OnTrack', 'AtRisk', 'Complete', 'Missed'] AS pstatus
    ) arrays
  CROSS JOIN LATERAL (
    SELECT "CategoryId", "CongregationId" FROM temp_asset_categories ORDER BY random() LIMIT 1
  ) pc
  CROSS JOIN LATERAL (
    SELECT "MemberId" FROM temp_members WHERE "CongregationId" = pc."CongregationId" ORDER BY random() LIMIT 1
  ) m
  RETURNING "Id", "CongregationId"
)
SELECT "Id" AS "ProjectId", "CongregationId" FROM ins;

-- ============================================================
-- 10. TITHES (Target: 200 records)
-- ============================================================
INSERT INTO "Tithes" (
  "CongregationId", "MemberId", "Decimal", "ForYear",
  "ForMonth", "PaymentMethod", "CollectionDate", "Description"
)
SELECT
  m."CongregationId",
  m."MemberId",
  (floor(random() * 1500 + 100))::int4 AS "Decimal",
  2025 AS "ForYear",
  months[floor(random() * array_length(months, 1)) + 1] AS "ForMonth",
  pmethod[floor(random() * array_length(pmethod, 1)) + 1] AS "PaymentMethod",
  ('2025-01-01'::date + (g * 2)) AS "CollectionDate",
  'Tithe financial receipt identifier ' || g AS "Description"
FROM generate_series(1, 200) g
CROSS JOIN (
  SELECT
    ARRAY['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'] AS months,
    ARRAY['Cash', 'Cheque', 'CreditCard', 'MobileMoney'] AS pmethod
) arrays
CROSS JOIN LATERAL (
  SELECT "MemberId", "CongregationId" FROM temp_members ORDER BY random() LIMIT 1
) m;

-- ============================================================
-- 11. TRANSACTIONS (Target: 200 records)
-- ============================================================
INSERT INTO "Transactions" (
  "CongregationId", "CategoryId", "TransactionType", "TransactionDate", "Amount", "Description"
)
SELECT
  tc."CongregationId",
  tc."CategoryId",
  tc."CategoryType" AS "TransactionType",
  ('2025-01-01'::date + (g * 1)) AS "TransactionDate",
  (random() * 5000 + 50)::numeric(18,2) AS "Amount",
  'General ledger balancing voucher token ' || g AS "Description"
FROM generate_series(1, 200) g
CROSS JOIN LATERAL (
  SELECT "CategoryId", "CongregationId", "CategoryType" FROM temp_transaction_categories ORDER BY random() LIMIT 1
) tc;

-- ============================================================
-- 12. ATTENDANCE RECORDS (Target: 250 unique rows)
-- ============================================================
INSERT INTO "AttendanceRecords" (
  "CongregationId", "MemberId", "ForDate",
  "ChurchServiceType", "AttendeeType", "CheckInTime", "Description"
)
SELECT
  m."CongregationId",
  m."MemberId",
  '2025-06-01'::date AS "ForDate",
  stype[floor(random() * array_length(stype, 1)) + 1] AS "ChurchServiceType",
  atype[floor(random() * array_length(atype, 1)) + 1] AS "AttendeeType",
  '08:30:00'::time AS "CheckInTime",
  'Standard service check-in index snapshot scan' AS "Description"
FROM (
  SELECT "MemberId", "CongregationId"
  FROM temp_members
  ORDER BY random()
  LIMIT 250
) m
CROSS JOIN (
  SELECT
    ARRAY['General', 'Evening', 'ChoirPractice'] AS stype,
    ARRAY['Member', 'Guest', 'Visitor'] AS atype
) arrays;

-- ============================================================
-- 13. EVENTS (Target: 80 records)
-- ============================================================
CREATE TEMP TABLE temp_events AS
WITH ins AS (
  INSERT INTO "Events" (
    "CongregationId", "OrganizationId", "OrganizerId", "Name",
    "StartDate", "EndDate", "StartTime", "EndTime", "Location", "Capacity", "Description"
  )
  SELECT
    o."CongregationId",
    o."OrganizationId",
    m."MemberId",
    'Event Master Plan ' || g || ' - ' || substring(md5(random()::text) from 1 for 6) AS "Name",
    ('2025-07-01'::date + (g * 2)) AS "StartDate",
    ('2025-07-03'::date + (g * 2)) AS "EndDate",
    '09:00:00'::time AS "StartTime",
    '16:00:00'::time AS "EndTime",
    'Main Assembly Hall Room ' || g AS "Location",
    (100 + floor(random() * 400))::int4 AS "Capacity",
    'Automated institutional system event calendar tracking item ' || g AS "Description"
  FROM generate_series(1, 80) g
  CROSS JOIN LATERAL (
    SELECT "OrganizationId", "CongregationId" FROM temp_organizations ORDER BY random() LIMIT 1
  ) o
  CROSS JOIN LATERAL (
    SELECT "MemberId" FROM temp_members WHERE "CongregationId" = o."CongregationId" ORDER BY random() LIMIT 1
  ) m
  RETURNING "Id", "CongregationId"
)
SELECT "Id" AS "EventId", "CongregationId" FROM ins;

-- ============================================================
-- 14. ORGANIZATION MEMBERS (Target: 200 records)
-- ============================================================
INSERT INTO "OrganizationMembers" (
  "CongregationId", "OrganizationId", "MemberId", "Role", "JoinedAt"
)
SELECT
  sub."CongregationId",
  sub."OrganizationId",
  sub."MemberId",
  roles[floor(random() * array_length(roles, 1)) + 1] AS "Role",
  '2024-01-01'::date AS "JoinedAt"
FROM (
  SELECT
    o."OrganizationId",
    o."CongregationId",
    m."MemberId",
    ROW_NUMBER() OVER(PARTITION BY m."MemberId", o."OrganizationId") as rn
  FROM temp_organizations o
  JOIN temp_members m ON m."CongregationId" = o."CongregationId"
  ORDER BY random()
  LIMIT 200
) sub
CROSS JOIN (
  SELECT ARRAY['Member', 'Secretary', 'Leader'] AS roles
) arrays
WHERE sub.rn = 1;

-- ============================================================
-- 15. PROJECT CONTRIBUTIONS (Target: 150 records)
-- ============================================================
INSERT INTO "ProjectContributions" (
  "CongregationId", "ProjectId", "Amount", "DateContributed", "PaymentMethod", "Description"
)
SELECT
  p."CongregationId",
  p."ProjectId",
  (random() * 2000 + 10)::numeric(18,2) AS "Amount",
  ('2025-03-01'::date + (g * 1)) AS "DateContributed",
  pmethod[floor(random() * array_length(pmethod, 1)) + 1] AS "PaymentMethod",
  'Project programmatic financing structural allocation line ' || g AS "Description"
FROM generate_series(1, 150) g
CROSS JOIN LATERAL (
  SELECT "ProjectId", "CongregationId" FROM temp_projects ORDER BY random() LIMIT 1
) p
CROSS JOIN (
  SELECT ARRAY['Cash', 'Cheque', 'CreditCard', 'MobileMoney'] AS pmethod
) arrays;

-- ============================================================
-- 16. EVENT REGISTRATIONS (Target: 150 records)
-- ============================================================
INSERT INTO "EventRegistrations" (
  "CongregationId", "EventId", "MemberId", "RegistrationDate"
)
SELECT
  sub."CongregationId",
  sub."EventId",
  sub."MemberId",
  '2025-06-15'::date AS "RegistrationDate"
FROM (
  SELECT DISTINCT ON (e."EventId", m."MemberId")
    e."EventId",
    e."CongregationId",
    m."MemberId"
  FROM temp_events e
  JOIN temp_members m ON m."CongregationId" = e."CongregationId"
  ORDER BY e."EventId", m."MemberId", random()
  LIMIT 150
) sub;

-- ============================================================
-- 17. EVENT ATTENDANCES (Target: 150 records)
-- ============================================================
INSERT INTO "EventAttendances" (
  "CongregationId", "EventId", "MemberId", "CheckInTime"
)
SELECT
  sub."CongregationId",
  sub."EventId",
  sub."MemberId",
  '09:15:00'::time AS "CheckInTime"
FROM (
  SELECT DISTINCT ON (e."EventId", m."MemberId")
    e."EventId",
    e."CongregationId",
    m."MemberId"
  FROM temp_events e
  JOIN temp_members m ON m."CongregationId" = e."CongregationId"
  ORDER BY e."EventId", m."MemberId", random()
  LIMIT 150
) sub;

COMMIT;
