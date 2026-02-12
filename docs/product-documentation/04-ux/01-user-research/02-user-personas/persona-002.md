# Persona 002

role: **accountant**  
owner: **dev team**  
status: **draft**  

name: **Daniel Owusu**  
age: 40

## Snapshot

Daniel is the church's trusted accountant, known for his steady, meticulous approach to money matters. He's detail-driven, cautious and takes pride in keeping records clean and auditable. He often works from a desktop with spreadsheets alongside the system, cross-checking entries to make sure everything balances.

## Typical day

Daniel spends his morning reconciling deposits against bank statements, approving transactions and preparing reports for leadership. He prefers structured workflows and hates chasing missing information. In the evenings, he sometimes reviews reports at home, but most of his heavy work in done in the office.

## Top Goals

- Ensure every deposit and transaction is traceable and accurate.
- Reconcile accounts quickly without needing to chase clerks for missing details.
- Produce monthly reports that leadership can trust without extra cleanup.

## Main Frustrations

- Missing metadata (like clerk IDs or deposit sources) that blocks reconciliation.
- Approval trails that are incomplete or hard to follow.
- Slow, cluttered reconciliation tools that make batch work painful.

## Tech Environment

Desktop-first, comfortable with spreadsheets and accounting software. Needs precise data, clear filters and exportable repors. Expects the system to support batch actions and provide reliable audit trails.  

## Success & Acceptance Criteria

Daniel can reconcile deposits with minimal follow-up, approve transactions with full audit trails and generate reports that leadership accepts without question.

## Design Implications (What to build for Daniel)

- Require and include reconciliation metadata (clerk ID, deposit source, approval ID, transaction ID, etc)
- Ensure exports include complete approval trails and metadata.
- Provide batch reconciliation tools with filters and bulk actions.

**Representative quote**
If the numbers do not line up, I can't move forward -- I need the system to give me the full picture everytime.
