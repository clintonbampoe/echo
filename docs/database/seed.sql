-- ============================================================
-- Echo Church Management System - Seed Data
-- Congregation: Grace Community Church
-- All data scoped to one congregation.
--
-- Dev password for all Users: Echo@2024
-- CongregationId: a0000000-0000-4000-8000-000000000001
-- ============================================================

BEGIN;

-- ============================================================
-- CONGREGATION
-- ============================================================
INSERT INTO "Congregations" ("CongregationId", "Name")
VALUES ('a0000000-0000-4000-8000-000000000001', 'Grace Community Church');


-- ============================================================
-- ASSET CATEGORIES (10)
-- ============================================================
INSERT INTO "AssetCategories" ("CongregationId", "Name") VALUES
('a0000000-0000-4000-8000-000000000001', 'Musical Instruments'),
('a0000000-0000-4000-8000-000000000001', 'Furniture'),
('a0000000-0000-4000-8000-000000000001', 'Electronics'),
('a0000000-0000-4000-8000-000000000001', 'Vehicles'),
('a0000000-0000-4000-8000-000000000001', 'Sound Equipment'),
('a0000000-0000-4000-8000-000000000001', 'Office Equipment'),
('a0000000-0000-4000-8000-000000000001', 'Kitchen Equipment'),
('a0000000-0000-4000-8000-000000000001', 'Generator & Power'),
('a0000000-0000-4000-8000-000000000001', 'Projection & Media'),
('a0000000-0000-4000-8000-000000000001', 'Security Equipment');


-- ============================================================
-- ATTENDANCE TYPES (6)
-- ============================================================
INSERT INTO "AttendanceTypes" ("CongregationId", "Name") VALUES
('a0000000-0000-4000-8000-000000000001', 'Sunday Service'),
('a0000000-0000-4000-8000-000000000001', 'Midweek Prayer'),
('a0000000-0000-4000-8000-000000000001', 'Bible Study'),
('a0000000-0000-4000-8000-000000000001', 'Youth Service'),
('a0000000-0000-4000-8000-000000000001', 'Women Fellowship'),
('a0000000-0000-4000-8000-000000000001', 'Men Fellowship');


-- ============================================================
-- PROJECT CATEGORIES (8)
-- ============================================================
INSERT INTO "ProjectCategories" ("CongregationId", "Name") VALUES
('a0000000-0000-4000-8000-000000000001', 'Building & Construction'),
('a0000000-0000-4000-8000-000000000001', 'Outreach & Missions'),
('a0000000-0000-4000-8000-000000000001', 'Youth Development'),
('a0000000-0000-4000-8000-000000000001', 'Education & Scholarships'),
('a0000000-0000-4000-8000-000000000001', 'Healthcare'),
('a0000000-0000-4000-8000-000000000001', 'Welfare & Support'),
('a0000000-0000-4000-8000-000000000001', 'Technology & Media'),
('a0000000-0000-4000-8000-000000000001', 'Special Events');


-- ============================================================
-- TRANSACTION CATEGORIES (12)
-- ============================================================
INSERT INTO "TransactionCategories" ("CongregationId", "Name", "CategoryType") VALUES
('a0000000-0000-4000-8000-000000000001', 'Sunday Offerings',      'Income'),
('a0000000-0000-4000-8000-000000000001', 'Tithes',                'Income'),
('a0000000-0000-4000-8000-000000000001', 'Special Donations',     'Income'),
('a0000000-0000-4000-8000-000000000001', 'Fundraising',           'Income'),
('a0000000-0000-4000-8000-000000000001', 'Grants',                'Income'),
('a0000000-0000-4000-8000-000000000001', 'Rental Income',         'Income'),
('a0000000-0000-4000-8000-000000000001', 'Utilities',             'Expense'),
('a0000000-0000-4000-8000-000000000001', 'Salaries & Benefits',   'Expense'),
('a0000000-0000-4000-8000-000000000001', 'Maintenance & Repairs', 'Expense'),
('a0000000-0000-4000-8000-000000000001', 'Events & Programs',     'Expense'),
('a0000000-0000-4000-8000-000000000001', 'Outreach & Welfare',    'Expense'),
('a0000000-0000-4000-8000-000000000001', 'Office & Administration','Expense');


-- ============================================================
-- ORGANIZATIONS (15)
-- ============================================================
INSERT INTO "Organizations" ("Id", "CongregationId", "Name", "Description") VALUES
('b0000000-0000-4000-8000-000000000001', 'a0000000-0000-4000-8000-000000000001', 'Youth Ministry',        'Serving members between 13–35 years'),
('b0000000-0000-4000-8000-000000000002', 'a0000000-0000-4000-8000-000000000001', 'Women Fellowship',      'Fellowship for women of all ages'),
('b0000000-0000-4000-8000-000000000003', 'a0000000-0000-4000-8000-000000000001', 'Men Fellowship',        'Brotherhood and accountability for men'),
('b0000000-0000-4000-8000-000000000004', 'a0000000-0000-4000-8000-000000000001', 'Choir',                 'Praise and worship team'),
('b0000000-0000-4000-8000-000000000005', 'a0000000-0000-4000-8000-000000000001', 'Ushering Ministry',     'Front-of-house and welcome team'),
('b0000000-0000-4000-8000-000000000006', 'a0000000-0000-4000-8000-000000000001', 'Media Ministry',        'Sound, projection and live streaming'),
('b0000000-0000-4000-8000-000000000007', 'a0000000-0000-4000-8000-000000000001', 'Prayer Team',           'Intercession and prayer warriors'),
('b0000000-0000-4000-8000-000000000008', 'a0000000-0000-4000-8000-000000000001', 'Welfare Committee',     'Supporting members in need'),
('b0000000-0000-4000-8000-000000000009', 'a0000000-0000-4000-8000-000000000001', 'Evangelism Team',       'Outreach and soul winning'),
('b0000000-0000-4000-8000-000000000010', 'a0000000-0000-4000-8000-000000000001', 'Sunday School',         'Children and young adults education'),
('b0000000-0000-4000-8000-000000000011', 'a0000000-0000-4000-8000-000000000001', 'Finance Committee',     'Financial oversight and planning'),
('b0000000-0000-4000-8000-000000000012', 'a0000000-0000-4000-8000-000000000001', 'Hospitality Team',      'Event hosting and catering'),
('b0000000-0000-4000-8000-000000000013', 'a0000000-0000-4000-8000-000000000001', 'Discipleship Team',     'New member orientation and mentoring'),
('b0000000-0000-4000-8000-000000000014', 'a0000000-0000-4000-8000-000000000001', 'Sports Ministry',       'Sports and recreation programs'),
('b0000000-0000-4000-8000-000000000015', 'a0000000-0000-4000-8000-000000000001', 'Building Committee',    'Overseeing construction and renovation projects');


-- ============================================================
-- MEMBERS (100)
-- Name is a generated column — do not include it in INSERT.
-- ============================================================
DO $$
DECLARE
  cid UUID := 'a0000000-0000-4000-8000-000000000001';
  male_first   TEXT[] := ARRAY['Kwame','Kofi','Kweku','Kwabena','Yaw','Fiifi','Kwesi','Kojo','Emmanuel','Daniel',
                                'Joseph','Samuel','Michael','Richard','Isaac','Francis','Benjamin','Solomon','David','Eric',
                                'George','Albert','Frederick','Patrick','Stephen','Philip','Theophilus','Anthony','Nana Kofi','Nana Yaw'];
  female_first TEXT[] := ARRAY['Ama','Akosua','Abena','Akua','Yaa','Efua','Adwoa','Araba','Grace','Joyce',
                                'Esther','Ruth','Mary','Elizabeth','Margaret','Patricia','Rebecca','Gifty','Abigail','Comfort',
                                'Theodora','Felicia','Vivian','Naomi','Christiana','Eunice','Dorcas','Hagar','Miriam','Priscilla'];
  last_names   TEXT[] := ARRAY['Mensah','Asante','Owusu','Boateng','Agyemang','Adjei','Amoah','Darko','Frimpong','Agyei',
                                'Antwi','Appiah','Asare','Badu','Barimah','Bonsu','Buah','Danso','Duah','Fordjour',
                                'Gyimah','Kwarteng','Ntim','Osei','Poku','Sarpong','Tawiah','Wiredu','Yeboah','Acheampong'];
  cities       TEXT[] := ARRAY['Accra','Kumasi','Takoradi','Tamale','Cape Coast','Sunyani','Ho','Koforidua',
                                'Tema','Kasoa','Madina','Ashaiman','Obuasi','Techiman','Bolgatanga'];
  regions      TEXT[] := ARRAY['Greater Accra','Ashanti','Western','Northern','Central','Bono','Volta','Eastern',
                                'Upper East','Upper West','Oti','Savannah','North East','Western North','Ahafo'];
  marital_st   TEXT[] := ARRAY['Single','Married','Divorced','Widowed'];
  activity_st  TEXT[] := ARRAY['Active','Inactive','Transferred'];
  i            INT;
  gender       TEXT;
  fname        TEXT;
  lname        TEXT;
  city         TEXT;
  region       TEXT;
  dob          DATE;
  joined       DATE;
  phone        TEXT;
  ephone       TEXT;
  kin_name     TEXT;
  emg_name     TEXT;
BEGIN
  FOR i IN 1..100 LOOP
    -- Roughly 60% female, 40% male (typical church demographic)
    gender := CASE WHEN (i * 7) % 10 < 6 THEN 'Female' ELSE 'Male' END;

    IF gender = 'Male' THEN
      fname := male_first  [1 + ((i - 1) % array_length(male_first,   1))];
    ELSE
      fname := female_first[1 + ((i - 1) % array_length(female_first, 1))];
    END IF;

    lname  := last_names[1 + ((i - 1) % array_length(last_names, 1))];
    city   := cities    [1 + ((i - 1) % array_length(cities,     1))];
    region := regions   [1 + ((i - 1) % array_length(regions,    1))];

    -- DOB: spread ages 15–75
    dob    := (CURRENT_DATE - INTERVAL '75 years') + (((i * 223) % (60 * 365)) || ' days')::INTERVAL;
    -- JoinedDate: spread over 2010–2023
    joined := DATE '2010-01-15' + (((i * 113) % 4745) || ' days')::INTERVAL;
    phone  := '024' || LPAD(((i * 7919 + 1000000) % 10000000)::TEXT, 7, '0');
    ephone := '020' || LPAD(((i * 6271 + 2000000) % 10000000)::TEXT, 7, '0');

    kin_name := last_names[1 + ((i * 2) % array_length(last_names,   1))] || ' ' ||
                male_first [1 + ((i * 3) % array_length(male_first,  1))];
    emg_name := last_names [1 + ((i * 3) % array_length(last_names,  1))] || ' ' ||
                female_first[1 + ((i * 2) % array_length(female_first,1))];

    INSERT INTO "Members" (
      "CongregationId", "FirstName", "LastName", "OtherNames",
      "EmailAddress", "PhoneNumber", "DateOfBirth", "JoinedDate",
      "Gender", "ResidentialAddress", "City", "Hometown",
      "Region", "GpsAddress", "MaritalStatus",
      "NextOfKin", "EmergencyContactName", "EmergencyContactPhoneNumber",
      "MemberActivityStatus"
    ) VALUES (
      cid,
      fname,
      lname,
      CASE WHEN i % 3 = 0 THEN NULL ELSE 'Nana' END,
      CASE WHEN i % 5 = 0 THEN NULL
           ELSE lower(regexp_replace(fname, '\s+', '', 'g')) || '.' || lower(lname) || i || '@gmail.com'
      END,
      phone,
      dob,
      CASE WHEN i % 12 = 0 THEN NULL ELSE joined END,
      gender,
      (i * 3)::TEXT || ' ' || city || ' Road, ' || city,
      city,
      cities[1 + ((i * 7) % array_length(cities, 1))],
      region,
      CASE WHEN i % 4 = 0 THEN 'GH-' || (100 + i)::TEXT || '-' || (1000 + i * 3)::TEXT ELSE NULL END,
      marital_st [1 + ((i - 1) % array_length(marital_st,  1))],
      kin_name,
      emg_name,
      ephone,
      activity_st[1 + ((i - 1) % array_length(activity_st, 1))]
    );
  END LOOP;
END $$;


-- ============================================================
-- USERS (3) — password: Echo@2024
-- ============================================================
INSERT INTO "Users" ("Id", "CongregationId", "Name", "EmailAddress", "PasswordHash") VALUES
('c0000000-0000-4000-8000-000000000001', 'a0000000-0000-4000-8000-000000000001',
 'Admin User',            'admin@gracecommunity.org',     '$2b$12$mFT/RvG8PlAN.exi4J.gM.6OaUyIKjCPhcpAVL6RQ.YNOk8b8So.y'),
('c0000000-0000-4000-8000-000000000002', 'a0000000-0000-4000-8000-000000000001',
 'Pastor Kwame Mensah',   'pastor@gracecommunity.org',    '$2b$12$mFT/RvG8PlAN.exi4J.gM.6OaUyIKjCPhcpAVL6RQ.YNOk8b8So.y'),
('c0000000-0000-4000-8000-000000000003', 'a0000000-0000-4000-8000-000000000001',
 'Secretary Ama Asante',  'secretary@gracecommunity.org', '$2b$12$mFT/RvG8PlAN.exi4J.gM.6OaUyIKjCPhcpAVL6RQ.YNOk8b8So.y');


-- ============================================================
-- ATTENDANCE CONTEXTS (12)
-- Unique constraint: (AttendanceTypeId, Name)
-- ============================================================
INSERT INTO "AttendanceContexts" ("CongregationId", "AttendanceTypeId", "Name")
SELECT 'a0000000-0000-4000-8000-000000000001', at."Id", ctx."CtxName"
FROM "AttendanceTypes" at
JOIN (VALUES
  ('Sunday Service',  'Main Auditorium'),
  ('Sunday Service',  'Children Chapel'),
  ('Sunday Service',  'Youth Hall'),
  ('Midweek Prayer',  'Main Prayer Hall'),
  ('Midweek Prayer',  'Prayer Room'),
  ('Bible Study',     'Main Hall'),
  ('Bible Study',     'Small Group A'),
  ('Bible Study',     'Small Group B'),
  ('Youth Service',   'Youth Hall'),
  ('Women Fellowship','Women Fellowship Hall'),
  ('Men Fellowship',  'Men Conference Room'),
  ('Men Fellowship',  'Main Hall')
) AS ctx("TypeName", "CtxName") ON at."Name" = ctx."TypeName"
WHERE at."CongregationId" = 'a0000000-0000-4000-8000-000000000001';


-- ============================================================
-- ASSETS (54 across 9 categories)
-- ============================================================
DO $$
DECLARE
  cid    UUID := 'a0000000-0000-4000-8000-000000000001';
  cat_id INT;
BEGIN
  -- Musical Instruments (8)
  SELECT "Id" INTO cat_id FROM "AssetCategories" WHERE "CongregationId" = cid AND "Name" = 'Musical Instruments';
  INSERT INTO "Assets" ("CongregationId","CategoryId","Name","SerialNumber","PurchaseDate","PurchaseCost","CurrentValue","Status","Description") VALUES
  (cid,cat_id,'Yamaha Grand Piano',       'SN-MUS-0001','2018-03-10',45000,38000,'Active',      'Main sanctuary piano'),
  (cid,cat_id,'Roland Electronic Drum Kit','SN-MUS-0002','2019-07-22',18000,14000,'Active',      'Worship band drums'),
  (cid,cat_id,'Fender Bass Guitar',        'SN-MUS-0003','2020-01-15', 6500, 5200,'UnderMaintenance','Bass guitar - neck repair'),
  (cid,cat_id,'Gibson Acoustic Guitar',    'SN-MUS-0004','2019-11-05', 4200, 3500,'Active',      'Acoustic for small groups'),
  (cid,cat_id,'Trumpet',                   'SN-MUS-0005','2017-06-18', 3800, 3000,'Active',      'Brass instrument'),
  (cid,cat_id,'Trombone',                  'SN-MUS-0006','2017-06-18', 4500, 3800,'Active',      'Brass instrument'),
  (cid,cat_id,'Alto Saxophone',            'SN-MUS-0007','2021-02-28', 5200, 4500,'Active',      'Woodwind instrument'),
  (cid,cat_id,'Tenor Saxophone',           'SN-MUS-0008','2021-02-28', 5800, 5000,'Active',      'Woodwind instrument');

  -- Furniture (8)
  SELECT "Id" INTO cat_id FROM "AssetCategories" WHERE "CongregationId" = cid AND "Name" = 'Furniture';
  INSERT INTO "Assets" ("CongregationId","CategoryId","Name","SerialNumber","PurchaseDate","PurchaseCost","CurrentValue","Status","Description") VALUES
  (cid,cat_id,'Pulpit',               'SN-FRN-0001','2015-05-20', 3500, 2500,'Active','Main pulpit - carved mahogany'),
  (cid,cat_id,'Altar Table',          'SN-FRN-0002','2015-05-20', 2800, 2000,'Active','Communion altar table'),
  (cid,cat_id,'Church Pews Set A',    'SN-FRN-0003','2016-08-14',22000,15000,'Active','Left section – 200 seats'),
  (cid,cat_id,'Church Pews Set B',    'SN-FRN-0004','2016-08-14',22000,15000,'Active','Right section – 200 seats'),
  (cid,cat_id,'Pastor Chair',         'SN-FRN-0005','2018-01-10', 1200,  900,'Active','High-back pastoral chair'),
  (cid,cat_id,'Elder Chairs Set',     'SN-FRN-0006','2018-01-10', 4500, 3200,'Active','Set of 10 elder chairs'),
  (cid,cat_id,'Choir Risers',         'SN-FRN-0007','2017-03-30', 8000, 6000,'Active','3-tier choir platform'),
  (cid,cat_id,'Communion Table',      'SN-FRN-0008','2015-09-01', 1800, 1400,'Active','Communion serving table');

  -- Electronics (6)
  SELECT "Id" INTO cat_id FROM "AssetCategories" WHERE "CongregationId" = cid AND "Name" = 'Electronics';
  INSERT INTO "Assets" ("CongregationId","CategoryId","Name","SerialNumber","PurchaseDate","PurchaseCost","CurrentValue","Status","Description") VALUES
  (cid,cat_id,'Dell PowerEdge Server',     'SN-ELC-0001','2020-06-01',12000, 8000,'Active',           'Church data server'),
  (cid,cat_id,'HP EliteBook Laptop A',     'SN-ELC-0002','2021-03-15', 4500, 3000,'UnderMaintenance', 'Admin laptop – battery fault'),
  (cid,cat_id,'HP EliteBook Laptop B',     'SN-ELC-0003','2021-03-15', 4500, 2800,'Active',           'Pastor office laptop'),
  (cid,cat_id,'iPad Pro Set (10 units)',   'SN-ELC-0004','2022-09-10',15000, 9000,'Active',           'Sunday school tablets'),
  (cid,cat_id,'Samsung Smart TV 65"',      'SN-ELC-0005','2022-01-20', 8000, 6000,'Active',           'Conference room display'),
  (cid,cat_id,'Midea Air Conditioner 2HP', 'SN-ELC-0006','2021-11-01', 6500, 5000,'Active',           'Office AC unit');

  -- Vehicles (4)
  SELECT "Id" INTO cat_id FROM "AssetCategories" WHERE "CongregationId" = cid AND "Name" = 'Vehicles';
  INSERT INTO "Assets" ("CongregationId","CategoryId","Name","SerialNumber","PurchaseDate","PurchaseCost","CurrentValue","Status","Description") VALUES
  (cid,cat_id,'Toyota HiAce Bus',    'GH-1345-A','2017-04-12',180000,120000,'Active',           'Church bus – 18 seater'),
  (cid,cat_id,'Toyota Land Cruiser', 'GH-2456-B','2018-09-05',220000,160000,'Active',           'Pastoral vehicle'),
  (cid,cat_id,'Honda CB150 Motorcycle','GH-3012-C','2019-02-28', 12000,  8000,'UnderMaintenance','Evangelism motorbike'),
  (cid,cat_id,'Ford Transit Bus',    'GH-4567-D','2016-11-30',150000, 95000,'Active',           'Secondary church bus – 15 seater');

  -- Sound Equipment (8)
  SELECT "Id" INTO cat_id FROM "AssetCategories" WHERE "CongregationId" = cid AND "Name" = 'Sound Equipment';
  INSERT INTO "Assets" ("CongregationId","CategoryId","Name","SerialNumber","PurchaseDate","PurchaseCost","CurrentValue","Status","Description") VALUES
  (cid,cat_id,'Behringer X32 Mixing Console', 'SN-SND-0001','2019-08-01',18000,14000,'Active','32-channel digital mixer'),
  (cid,cat_id,'JBL PRX815 Speaker Set',        'SN-SND-0002','2019-08-01',35000,28000,'Active','Main PA speakers (pair)'),
  (cid,cat_id,'QSC GX7 Power Amplifier',       'SN-SND-0003','2020-03-10',12000, 9000,'Active','2-channel power amp'),
  (cid,cat_id,'Shure Wireless Mic Set (4ch)',   'SN-SND-0004','2021-05-14', 8500, 6500,'Active','4-channel wireless system'),
  (cid,cat_id,'Yamaha DXR12 Stage Monitor',     'SN-SND-0005','2020-07-22', 6000, 4500,'Active','Stage floor monitor'),
  (cid,cat_id,'Radial DI Box Set',              'SN-SND-0006','2019-08-01', 2500, 2000,'Active','Set of 4 DI boxes'),
  (cid,cat_id,'Audio Cables & Accessories',     'SN-SND-0007','2019-08-01', 3000, 2000,'Active','XLR, TRS, NL4 cables'),
  (cid,cat_id,'19" Audio Rack Unit',            'SN-SND-0008','2020-01-05', 4000, 3000,'Active','8U rack with patch bay');

  -- Generator & Power (4)
  SELECT "Id" INTO cat_id FROM "AssetCategories" WHERE "CongregationId" = cid AND "Name" = 'Generator & Power';
  INSERT INTO "Assets" ("CongregationId","CategoryId","Name","SerialNumber","PurchaseDate","PurchaseCost","CurrentValue","Status","Description") VALUES
  (cid,cat_id,'Lister 50KVA Generator',  'SN-GEN-0001','2016-02-10', 85000,60000,'Active','Primary backup generator'),
  (cid,cat_id,'Perkins 30KVA Generator', 'SN-GEN-0002','2018-05-19', 65000,45000,'Active','Secondary backup generator'),
  (cid,cat_id,'Solar Panel Array 30KW',  'SN-GEN-0003','2022-04-01',120000,95000,'Active','Rooftop solar with inverter'),
  (cid,cat_id,'APC Smart-UPS 10KVA',    'SN-GEN-0004','2021-09-15', 22000,15000,'Active','UPS for server and media room');

  -- Projection & Media (6)
  SELECT "Id" INTO cat_id FROM "AssetCategories" WHERE "CongregationId" = cid AND "Name" = 'Projection & Media';
  INSERT INTO "Assets" ("CongregationId","CategoryId","Name","SerialNumber","PurchaseDate","PurchaseCost","CurrentValue","Status","Description") VALUES
  (cid,cat_id,'Epson EB-L1505U Laser Projector','SN-PRJ-0001','2020-11-01',28000,22000,'Active','4K laser projector – main hall'),
  (cid,cat_id,'LED Screen 12x8ft',               'SN-PRJ-0002','2020-11-01',55000,45000,'Active','Indoor LED video wall'),
  (cid,cat_id,'Sony PXW-Z90 Livestream Camera A','SN-PRJ-0003','2021-07-12',18000,14000,'Active','4K livestream camera'),
  (cid,cat_id,'Sony PXW-Z90 Livestream Camera B','SN-PRJ-0004','2021-07-12',18000,14000,'Active','4K livestream camera'),
  (cid,cat_id,'Blackmagic ATEM Video Switcher',  'SN-PRJ-0005','2021-07-12',12000, 9000,'Active','Live production switcher'),
  (cid,cat_id,'Media Presentation Computer',     'SN-PRJ-0006','2022-02-20', 8000, 6000,'Active','ProPresenter workstation');

  -- Security Equipment (4)
  SELECT "Id" INTO cat_id FROM "AssetCategories" WHERE "CongregationId" = cid AND "Name" = 'Security Equipment';
  INSERT INTO "Assets" ("CongregationId","CategoryId","Name","SerialNumber","PurchaseDate","PurchaseCost","CurrentValue","Status","Description") VALUES
  (cid,cat_id,'Hikvision CCTV System 16ch',  'SN-SEC-0001','2022-03-14',22000,18000,'Active','16-channel IP camera system'),
  (cid,cat_id,'Hikvision NVR & HDD',         'SN-SEC-0002','2022-03-14', 8000, 6500,'Active','Network video recorder – 4TB'),
  (cid,cat_id,'Electric Fence Unit',          'SN-SEC-0003','2021-10-08',15000,13000,'Active','Perimeter electric fence'),
  (cid,cat_id,'Suprema Access Control System','SN-SEC-0004','2023-01-20',12000,10000,'Active','Biometric door access control');
END $$;


-- ============================================================
-- PROJECTS (20)
-- ============================================================
DO $$
DECLARE
  cid         UUID := 'a0000000-0000-4000-8000-000000000001';
  member_ids  UUID[];
  cat_ids     INT[];
  names       TEXT[] := ARRAY[
    'Sanctuary Extension Phase 1','Sanctuary Extension Phase 2',
    'Borehole Water Project','Solar Electrification',
    'Youth Computer Lab','Church Bus Purchase',
    'Welfare Emergency Fund','Community Health Screening',
    'Scholarship Fund 2023','Scholarship Fund 2024',
    'Christmas Outreach 2023','Easter Program 2024',
    'Roofing Renovation','Perimeter Wall Construction',
    'Sound System Upgrade','Livestream Studio Setup',
    'Missions Trip - Northern Ghana','Missions Trip - Volta Region',
    'New Members Orientation Hall','Parking Lot Paving'
  ];
  statuses    TEXT[] := ARRAY[
    'Completed','Active','Completed','Completed','Active','Completed',
    'Active','Completed','Completed','Active','Completed','Active',
    'Completed','Active','Active','Completed','Completed','Active','Active','Active'
  ];
  targets     NUMERIC[] := ARRAY[
    500000,800000,45000,120000,80000,180000,50000,35000,
    60000,60000,25000,30000,150000,280000,95000,75000,
    40000,40000,65000,110000
  ];
  starts      DATE[] := ARRAY[
    '2022-01-10'::DATE,'2023-06-01','2021-03-15','2022-09-01','2023-01-20','2020-08-01',
    '2021-05-01','2023-10-01','2023-01-01','2024-01-01','2023-11-01','2024-02-01',
    '2022-04-01','2023-03-15','2023-07-01','2024-01-15','2023-05-01','2023-09-01',
    '2024-03-01','2024-04-01'
  ];
  ends        DATE[] := ARRAY[
    '2023-12-31'::DATE,NULL,'2022-01-30','2023-08-31',NULL,'2021-05-30',
    NULL,'2023-12-31','2023-12-31',NULL,'2023-12-25','2024-04-10',
    '2023-03-31',NULL,NULL,'2024-06-30','2023-06-30','2023-11-30',NULL,NULL
  ];
  i INT;
BEGIN
  SELECT ARRAY(SELECT "Id" FROM "Members" WHERE "CongregationId" = cid ORDER BY "CreatedAt" LIMIT 20) INTO member_ids;
  SELECT ARRAY(SELECT "Id" FROM "ProjectCategories" WHERE "CongregationId" = cid ORDER BY "Id") INTO cat_ids;

  FOR i IN 1..20 LOOP
    INSERT INTO "Projects" ("CongregationId","CategoryId","ManagerId","Name","TargetAmount","Status","StartDate","EndDate","Description")
    VALUES (
      cid,
      cat_ids  [1 + ((i - 1) % array_length(cat_ids,     1))],
      member_ids[1 + ((i - 1) % array_length(member_ids, 1))],
      names    [i],
      targets  [i],
      statuses [i],
      starts   [i],
      ends     [i],
      'Church project: ' || names[i]
    );
  END LOOP;
END $$;


-- ============================================================
-- TITHES (150)
-- ============================================================
DO $$
DECLARE
  cid         UUID := 'a0000000-0000-4000-8000-000000000001';
  member_ids  UUID[];
  months      TEXT[] := ARRAY['January','February','March','April','May','June',
                               'July','August','September','October','November','December'];
  pay_methods TEXT[] := ARRAY['Cash','MobileMoney','BankTransfer','Cash','MobileMoney','Cash','Cheque'];
  i           INT;
  m_idx       INT;
  yr          INT;
BEGIN
  SELECT ARRAY(SELECT "Id" FROM "Members" WHERE "CongregationId" = cid ORDER BY "CreatedAt" LIMIT 80) INTO member_ids;

  FOR i IN 1..150 LOOP
    m_idx := 1 + ((i - 1) % 12);
    yr    := 2022 + ((i - 1) / 60); -- 2022, 2023, 2024

    INSERT INTO "Tithes" ("CongregationId","MemberId","Amount","ForYear","ForMonth","PaymentMethod","CollectionDate","Description")
    VALUES (
      cid,
      member_ids[1 + ((i - 1) % array_length(member_ids, 1))],
      (50 + ((i * 37) % 950))::NUMERIC,
      yr,
      months[m_idx],
      pay_methods[1 + ((i - 1) % array_length(pay_methods, 1))],
      make_date(yr, m_idx, 1 + ((i * 7) % 28)),
      CASE WHEN i % 8 = 0 THEN 'Tithe for ' || months[m_idx] || ' ' || yr ELSE NULL END
    );
  END LOOP;
END $$;


-- ============================================================
-- TRANSACTIONS (120: 70 income + 50 expense)
-- ============================================================
DO $$
DECLARE
  cid          UUID := 'a0000000-0000-4000-8000-000000000001';
  income_cats  INT[];
  expense_cats INT[];
  i            INT;
  cat_id       INT;
  t_date       DATE;
  amount       NUMERIC;
BEGIN
  SELECT ARRAY(SELECT "Id" FROM "TransactionCategories" WHERE "CongregationId" = cid AND "CategoryType" = 'Income'  ORDER BY "Id") INTO income_cats;
  SELECT ARRAY(SELECT "Id" FROM "TransactionCategories" WHERE "CongregationId" = cid AND "CategoryType" = 'Expense' ORDER BY "Id") INTO expense_cats;

  FOR i IN 1..70 LOOP
    cat_id := income_cats[1 + ((i - 1) % array_length(income_cats, 1))];
    t_date := DATE '2022-01-01' + (((i * 15) % 1000) || ' days')::INTERVAL;
    amount := (500 + ((i * 317) % 19500))::NUMERIC;
    INSERT INTO "Transactions" ("CongregationId","CategoryId","TransactionType","TransactionDate","Amount","Description")
    VALUES (cid, cat_id, 'Income', t_date, amount,
      CASE WHEN i % 6 = 0 THEN 'Income record ' || i ELSE NULL END);
  END LOOP;

  FOR i IN 1..50 LOOP
    cat_id := expense_cats[1 + ((i - 1) % array_length(expense_cats, 1))];
    t_date := DATE '2022-01-05' + (((i * 19) % 1000) || ' days')::INTERVAL;
    amount := (200 + ((i * 283) % 9800))::NUMERIC;
    INSERT INTO "Transactions" ("CongregationId","CategoryId","TransactionType","TransactionDate","Amount","Description")
    VALUES (cid, cat_id, 'Expense', t_date, amount,
      CASE WHEN i % 6 = 0 THEN 'Expense record ' || i ELSE NULL END);
  END LOOP;
END $$;


-- ============================================================
-- EVENTS (50)
-- Unique constraint: (CongregationId, Name)
-- ============================================================
DO $$
DECLARE
  cid         UUID := 'a0000000-0000-4000-8000-000000000001';
  org_ids     UUID[];
  member_ids  UUID[];
  names       TEXT[] := ARRAY[
    'Annual Thanksgiving Service','Watch Night Service 2022','Easter Sunday Service 2023',
    'Good Friday Service 2023','Founders Day Celebration','Church Anniversary 2023',
    'Youth Conference 2023','Women Convention 2023','Men Retreat 2023',
    'Children Sunday 2023','Harvest Festival 2023','Christmas Carol Service 2023',
    'Watch Night Service 2023','New Year Crossover Service 2024','Valentine Couples Night 2024',
    'Easter Sunday Service 2024','Good Friday Service 2024','Mother Day Service 2024',
    'Youth Conference 2024','Workers Convention 2024','Midyear Thanksgiving 2024',
    'Church Picnic 2024','Back to School Program 2024','Choir Concert 2023',
    'Health Walk 2024','Blood Donation Drive 2024','Community Cleanup Day 2024',
    'Evangelism Crusade - Accra Central','Evangelism Crusade - Tema',
    'Prison Ministry Visit April 2023','Hospital Visitation May 2023',
    'Orphanage Visit June 2023','Street Feeding Program July 2023',
    'Leadership Seminar 2023','Marriage Enrichment Seminar 2023',
    'Parenting Workshop 2023','Financial Stewardship Workshop 2023',
    'Youth Mentorship Day 2024','Sports Day 2024',
    'Drama & Arts Festival 2024','Choir Rehearsal Camp 2024',
    'Prayer & Fasting Week 2023','Prayer & Fasting Week 2024',
    '21 Days Fast Opening Service 2024','Dedication Service 2024',
    'Ordination Service 2024','Missions Sunday 2023',
    'Baptism Service April 2024','Baptism Service October 2023',
    'Baptism Service January 2024'
  ];
  locations   TEXT[] := ARRAY[
    'Main Auditorium','Main Auditorium','Main Auditorium','Main Auditorium',
    'Church Grounds','Main Auditorium','Youth Hall','Accra Conference Centre',
    'Aburi Mountain Resort','Main Auditorium','Church Grounds','Main Auditorium',
    'Main Auditorium','Main Auditorium','Fellowship Hall','Main Auditorium',
    'Main Auditorium','Main Auditorium','Accra International Conference Centre',
    'Main Auditorium','Main Auditorium','Laboma Beach','Church Hall',
    'Main Auditorium','Accra Sports Stadium','Korle Bu Teaching Hospital','Kaneshie Market Area',
    'Accra Central','Community 18 Tema','Nsawam Medium Security Prison',
    '37 Military Hospital','SOS Children Village Tema','Abossey Okai',
    'Church Conference Room','Fellowship Hall','Fellowship Hall',
    'Church Conference Room','Youth Hall','Church Grounds',
    'Main Auditorium','Retreat Center Aburi','Main Auditorium',
    'Main Auditorium','Main Auditorium','Church Grounds',
    'Main Auditorium','Main Auditorium','Church Baptismal Pool',
    'Church Baptismal Pool','Church Baptismal Pool'
  ];
  starts      DATE[] := ARRAY[
    '2022-11-27'::DATE,'2022-12-31','2023-04-09','2023-04-07','2023-05-20','2023-06-18',
    '2023-07-14','2023-08-10','2023-09-15','2023-10-01','2023-11-19','2023-12-17',
    '2023-12-31','2024-01-01','2024-02-14','2024-03-31','2024-03-29','2024-05-12',
    '2024-06-14','2024-07-20','2024-07-07','2024-08-11','2024-09-01','2023-12-09',
    '2024-03-17','2024-04-20','2024-05-05','2023-03-10','2023-03-17','2023-04-14',
    '2023-05-06','2023-06-03','2023-07-08','2023-08-19','2023-09-09','2023-10-07',
    '2023-11-11','2024-02-03','2024-05-18','2024-06-22','2024-07-27','2023-01-09',
    '2024-01-08','2024-01-08','2024-03-03','2024-04-14','2023-10-15','2024-04-28',
    '2023-10-22','2024-01-21'
  ];
  durations   INT[] := ARRAY[
    1,1,1,1,2,1,3,3,2,1,2,1,1,1,1,1,1,1,3,3,1,1,1,1,1,1,1,3,3,1,1,1,1,1,1,1,1,1,1,2,3,7,7,1,1,1,1,1,1,1
  ];
  capacities  INT[] := ARRAY[
    NULL,NULL,NULL,NULL,500,NULL,300,400,150,NULL,NULL,NULL,NULL,NULL,200,NULL,NULL,NULL,
    500,600,NULL,300,200,NULL,1000,NULL,NULL,NULL,NULL,200,100,80,NULL,80,60,60,80,150,300,
    400,80,NULL,NULL,NULL,NULL,NULL,NULL,200,200,200
  ];
  i INT;
BEGIN
  SELECT ARRAY(SELECT "Id" FROM "Organizations" WHERE "CongregationId" = cid ORDER BY "Id") INTO org_ids;
  SELECT ARRAY(SELECT "Id" FROM "Members" WHERE "CongregationId" = cid AND "MemberActivityStatus" = 'Active' ORDER BY "CreatedAt" LIMIT 30) INTO member_ids;

  FOR i IN 1..50 LOOP
    INSERT INTO "Events" (
      "CongregationId","OrganizationId","OrganizerId","Name",
      "StartDate","EndDate","StartTime","EndTime",
      "Location","Capacity","Description"
    ) VALUES (
      cid,
      org_ids   [1 + ((i - 1) % array_length(org_ids,    1))],
      member_ids[1 + ((i - 1) % array_length(member_ids, 1))],
      names     [i],
      starts    [i],
      starts    [i] + (durations[i] - 1 || ' days')::INTERVAL,
      CASE WHEN i % 7 = 0 THEN NULL ELSE make_time(6 + ((i * 2) % 8), 0, 0) END,
      CASE WHEN i % 7 = 0 THEN NULL ELSE make_time(((10 + (i % 6))::INT), 30, 0) END,
      locations [i],
      capacities[i],
      'Church event: ' || names[i]
    );
  END LOOP;
END $$;


-- ============================================================
-- ORGANIZATION MEMBERS (80)
-- Unique constraint: (MemberId, OrganizationId)
-- ============================================================
DO $$
DECLARE
  cid        UUID := 'a0000000-0000-4000-8000-000000000001';
  org_ids    UUID[];
  member_ids UUID[];
  roles      TEXT[] := ARRAY['Member','Member','Member','Member','Leader','Secretary','Treasurer','President'];
  inserted   INT := 0;
  i          INT;
  org_id     UUID;
  member_id  UUID;
BEGIN
  SELECT ARRAY(SELECT "Id" FROM "Organizations" WHERE "CongregationId" = cid ORDER BY "Id") INTO org_ids;
  SELECT ARRAY(SELECT "Id" FROM "Members" WHERE "CongregationId" = cid ORDER BY "CreatedAt" LIMIT 100) INTO member_ids;

  FOR i IN 1..120 LOOP
    org_id    := org_ids   [1 + ((i - 1) % array_length(org_ids,    1))];
    member_id := member_ids[1 + ((i - 1) % array_length(member_ids, 1))];

    BEGIN
      INSERT INTO "OrganizationMembers" ("CongregationId","MemberId","OrganizationId","Role","JoinedAt")
      VALUES (
        cid, member_id, org_id,
        roles[1 + ((i - 1) % array_length(roles, 1))],
        DATE '2015-01-01' + (((i * 89) % 3000) || ' days')::INTERVAL
      );
      inserted := inserted + 1;
    EXCEPTION WHEN unique_violation THEN
      -- skip duplicate (member already in this org)
    END;

    EXIT WHEN inserted >= 80;
  END LOOP;
END $$;


-- ============================================================
-- ATTENDANCE RECORDS (150)
-- ============================================================
DO $$
DECLARE
  cid        UUID := 'a0000000-0000-4000-8000-000000000001';
  ctx_ids    INT[];
  member_ids UUID[];
  i          INT;
  ctx_id     INT;
  member_id  UUID;
  att_date   DATE;
  checkin    TIME;
BEGIN
  SELECT ARRAY(SELECT "Id" FROM "AttendanceContexts" WHERE "CongregationId" = cid ORDER BY "Id") INTO ctx_ids;
  SELECT ARRAY(SELECT "Id" FROM "Members" WHERE "CongregationId" = cid ORDER BY "CreatedAt" LIMIT 100) INTO member_ids;

  FOR i IN 1..150 LOOP
    ctx_id   := ctx_ids[1 + ((i - 1) % array_length(ctx_ids, 1))];
    att_date := DATE '2023-01-01' + (((i * 7) % 550) || ' days')::INTERVAL;
    checkin  := make_time(6 + ((i * 3) % 6), (i * 7) % 60, 0);

    IF (i % 5 = 0) THEN
      INSERT INTO "AttendanceRecords" ("CongregationId","AttendanceContextId","MemberId","GuestName","ForDate","AttendeeType","CheckInTime","Description")
      VALUES (cid, ctx_id, NULL, 'Guest Visitor ' || i, att_date, 'Guest', checkin, NULL);
    ELSE
      member_id := member_ids[1 + ((i - 1) % array_length(member_ids, 1))];
      INSERT INTO "AttendanceRecords" ("CongregationId","AttendanceContextId","MemberId","GuestName","ForDate","AttendeeType","CheckInTime","Description")
      VALUES (cid, ctx_id, member_id, NULL, att_date, 'Member', checkin, NULL);
    END IF;
  END LOOP;
END $$;


-- ============================================================
-- PROJECT CONTRIBUTIONS (80)
-- ============================================================
DO $$
DECLARE
  cid         UUID := 'a0000000-0000-4000-8000-000000000001';
  project_ids UUID[];
  pay_methods TEXT[] := ARRAY['Cash','MobileMoney','BankTransfer','Cash','MobileMoney','Cheque'];
  i           INT;
  project_id  UUID;
  contrib_date DATE;
  amount      NUMERIC;
BEGIN
  SELECT ARRAY(SELECT "Id" FROM "Projects" WHERE "CongregationId" = cid ORDER BY "StartDate") INTO project_ids;

  FOR i IN 1..80 LOOP
    project_id   := project_ids[1 + ((i - 1) % array_length(project_ids, 1))];
    contrib_date := DATE '2022-01-01' + (((i * 13) % 900) || ' days')::INTERVAL;
    amount       := (100 + ((i * 271) % 9900))::NUMERIC;

    INSERT INTO "ProjectContributions" ("CongregationId","ProjectId","Amount","DateContributed","PaymentMethod","Description")
    VALUES (
      cid, project_id, amount, contrib_date,
      pay_methods[1 + ((i - 1) % array_length(pay_methods, 1))],
      CASE WHEN i % 7 = 0 THEN 'Contribution batch ' || i ELSE NULL END
    );
  END LOOP;
END $$;


-- ============================================================
-- EVENT REGISTRATIONS (60)
-- Unique constraint: (EventId, MemberId)
-- ============================================================
DO $$
DECLARE
  cid        UUID := 'a0000000-0000-4000-8000-000000000001';
  event_ids  UUID[];
  member_ids UUID[];
  i          INT;
  event_id   UUID;
  member_id  UUID;
  reg_date   DATE;
  e_len      INT;
  m_len      INT;
BEGIN
  SELECT ARRAY(SELECT "Id" FROM "Events" WHERE "CongregationId" = cid ORDER BY "StartDate" LIMIT 30) INTO event_ids;
  SELECT ARRAY(SELECT "Id" FROM "Members" WHERE "CongregationId" = cid ORDER BY "CreatedAt" LIMIT 60) INTO member_ids;

  e_len := array_length(event_ids,  1);
  m_len := array_length(member_ids, 1);

  FOR i IN 1..80 LOOP
    event_id  := event_ids [1 + ((i - 1) % e_len)];
    -- Offset member index by a prime multiple of the cycle to minimise same (event,member) repeats
    member_id := member_ids[1 + (((i - 1) + ((i - 1) / e_len) * 7) % m_len)];
    reg_date  := (SELECT "StartDate" - INTERVAL '7 days' FROM "Events" WHERE "Id" = event_id);

    BEGIN
      INSERT INTO "EventRegistrations" ("CongregationId","MemberId","EventId","RegistrationDate")
      VALUES (cid, member_id, event_id, reg_date);
    EXCEPTION WHEN unique_violation THEN
      -- skip duplicate
    END;
  END LOOP;
END $$;


-- ============================================================
-- EVENT ATTENDANCES (60)
-- Unique constraint: (EventId, MemberId)
-- ============================================================
DO $$
DECLARE
  cid        UUID := 'a0000000-0000-4000-8000-000000000001';
  event_ids  UUID[];
  member_ids UUID[];
  i          INT;
  event_id   UUID;
  member_id  UUID;
  checkin    TIME;
  e_len      INT;
  m_len      INT;
BEGIN
  SELECT ARRAY(SELECT "Id" FROM "Events" WHERE "CongregationId" = cid ORDER BY "StartDate" LIMIT 30) INTO event_ids;
  SELECT ARRAY(SELECT "Id" FROM "Members" WHERE "CongregationId" = cid ORDER BY "CreatedAt" LIMIT 60) INTO member_ids;

  e_len := array_length(event_ids,  1);
  m_len := array_length(member_ids, 1);

  FOR i IN 1..80 LOOP
    event_id  := event_ids [1 + ((i - 1) % e_len)];
    member_id := member_ids[1 + (((i - 1) + ((i - 1) / e_len) * 11) % m_len)];
    checkin   := make_time(6 + ((i * 2) % 8), (i * 11) % 60, 0);

    BEGIN
      INSERT INTO "EventAttendances" ("CongregationId","MemberId","EventId","CheckInTime")
      VALUES (cid, member_id, event_id, checkin);
    EXCEPTION WHEN unique_violation THEN
      -- skip duplicate
    END;
  END LOOP; END $$;

COMMIT;

/*
  RECORD COUNTS (approximate — unique-constraint skips may reduce some):
  Congregations         :   1
  AssetCategories       :  10
  AttendanceTypes       :   6
  ProjectCategories     :   8
  TransactionCategories :  12
  Organizations         :  15
  Members               : 100
  Users                 :   3
  AttendanceContexts    :  12
  Assets                :  54
  Projects              :  20
  Tithes                : 150
  Transactions          : 120
  Events                :  50
  OrganizationMembers   : ~80
  AttendanceRecords     : 150
  ProjectContributions  :  80
  EventRegistrations    : ~60
  EventAttendances      : ~60
  ─────────────────────────────
  Total                 : ~993
*/
