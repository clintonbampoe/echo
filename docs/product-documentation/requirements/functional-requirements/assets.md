# Requirements: Assets

## Asset Register

- **AS-AR001:** The system must maintain records of all physical and financial assets with fields for name, type, acquisition date, value, and ownership.

- **AS-AR002:** The system must track acquisition details including purchase source, cost, and supporting documentation.

- **AS-AR003:** The system must support updates for depreciation, revaluation, or disposal of assets. *// Clarification: This requires consultation with staff to confirm depreciation and revaluation methods (e.g., straight-line, reducing balance).*

- **AS-AR004:** The system must provide search and filter options for assets by type, acquisition date, or value.

- **AS-AR005:** The system must maintain audit logs of all asset updates, showing who made changes and when.

## Designated Funds (Locked Funds)

- **AS-DF001:** The system must manage funds earmarked for specific purposes, with fields for fund name, designation, balance, and usage.

- **AS-DF002:** The system must track disbursements from designated funds, linking them to specific beneficiaries or projects. *// Clarification: A “disbursement” means any outgoing payment or allocation from a designated fund. Each record should capture beneficiary/project, amount, purpose, and date for full traceability.*

- **AS-DF003:** The system must maintain balances for each designated fund, automatically updating when disbursements or allocations occur. *// Clarification: “Balances” here refers to the remaining available amount in each designated fund after allocations and disbursements. It represents the current usable value of that locked fund.*

- **AS-DF004:** The system must generate periodic summaries of designated fund usage (monthly, yearly).

- **AS-DF005:** The system must provide alerts or flags when designated fund balances fall below a defined threshold. *// Clarification: Thresholds are set by administrators. Alerts notify authorized users when balances drop below this minimum safe level, prompting replenishment or spending review.*
