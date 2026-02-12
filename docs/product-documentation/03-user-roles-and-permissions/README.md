# Summary

---

## Purpose

This file gives you a quick reference to the minimal role set for a single church instance.  
You'll see the instance limits and a short description of each role so you know how responsibilities are defined. Use it as your onboarding guide and follow the links to deeper role documents when you need more detail.

These roles map to the project's functional modules (see `functions.md`). At this stage the documentation is intentionally minimal so stakeholders can confirm constraints before we expand into detailed workflows and approvals.

---

## Roles

- **Administrator**  
  - **Limit:** 1 account per church instance.  
  - **Write permissions:** Full system control.  
  - **Responsibilities:** Manage users, roles, global settings, governance, and perform read/write across all modules.

- **Finance Officer / Accountant**  
  - **Limit:** 1 account per church instance.  
  - **Write permissions:** Full access to all financial domains and modules.  
  - **Responsibilities:** Post and approve transactions, perform reconciliations, process contributions and refunds, generate financial statements and reports.

- **Clerk**  
  - **Limit:** 3 accounts per church instance.  
  - **Write permissions:** Write access to Contributions, Attendance, and Membership management domains.  
  - **Responsibilities:** Record contributions, record attendance, and perform basic membership administration.

- **Viewer / Auditor**  
  - **Limit:** Unlimited accounts per church instance.  
  - **Write permissions:** None (read-only across all modules).  
  - **Responsibilities:** Inspect records and reports for review and audit purposes.

---

## Where to go next

- For role-writing rules and a template, see [guidelines](guidelines.md)
- For the CRUD matrix mapping roles to modules, see [permission-mapping](permission-mapping.md)
- For detailed responsibilities examples and UX implications, go to the `user-roles` directory.
- For sensitive operations and approval workflows, see [sensitive-actions](sensitive-actions.md) and [approval-workflows](approval-workflows.md)
