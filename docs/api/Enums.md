# Enums

Enums are stored as strings in the database, not integers. This makes the database human-readable and prevents data corruption if enum values are ever reordered in code.

| Enum                       | Values                                      |
| -------------------------- | ------------------------------------------- |
| `Gender`                   | Male, Female                                |
| `MaritalStatus`            | Single, Married, Divorced, Widowed          |
| `MemberActivityStatus`     | Active, Inactive                            |
| `MemberOrganizationalRole` | Member, Leader                              |
| `Region`                   | All 16 regions of Ghana                     |
| `MonthOfYear`              | January – December                          |
| `PaymentMethod`            | Cash, Cheque, CreditCard, MobileMoney       |
| `TransactionType`          | Income, Expenditure                         |
| `ProjectStatus`            | Planning, OnTrack, AtRisk, Complete, Missed |
| `AssetStatus`              | Active, UnderMaintenance, Decommissioned    |
| `ChurchServiceType`        | Sunday service types and midweek services   |
| `AttendeeType`             | Member, Visitor, Guest                      |
