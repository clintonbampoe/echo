# Requirements: Membership Management

## Member Registry

- **MM-MR001:** The system must maintain records of individual members with fields for name, contact information, unique ID, and membership status.  
  
  *// Clarification: Fields are not limited to these; additional attributes such as date of birth, gender, join date, and emergency contact may be required depending on stakeholder needs.*

- **MM-MR002:** The system must support linking members to contributions, tithes, and dues for accountability.

- **MM-MR003:** The system must track member participation across services, events, and projects.

- **MM-MR004:** The system must provide search and filter options for members by name, group, contribution status, or participation level.

- **MM-MR005:** The system must maintain audit logs of member record updates, showing who made changes and when.  
  
  *// Clarification: This ensures transparency in membership data management, preventing silent edits or removals. While not crucial for MVP, stakeholders may request it for accountability.*

## Group/Organization Registry

- **MM-GR001:** The system must maintain records of groups, fellowships, committees, and organizations within the church, with fields for group name, type, leader, and membership list.

- **MM-GR002:** The system must support linking groups to contributions, projects, and attendance records.  
  
  *// Clarification: This is technically challenging with current skill level. A phased approach can be taken—start with linking groups to attendance and projects, then expand to contributions later.*

- **MM-GR003:** The system must provide search and filter options for groups by type, leader, or activity.

- **MM-GR004:** The system must maintain audit logs of group record updates, showing who made changes and when.

## Association & Roles

- **MM-AR001:** The system must allow members to be associated with multiple groups or roles (e.g., choir member, committee leader).

- **MM-AR002:** The system must track leadership roles and responsibilities within groups and projects.  

  *// Clarification: Roles should include title (e.g., “Choir Leader”), scope (group or project), and duration (start/end dates). This ensures responsibilities are explicit and traceable.*

- **MM-AR003:** The system must support hierarchical relationships (e.g., groups under fellowships, fellowships under circuits).  
  
  *// Clarification: This reflects real-world church structure, ensuring reporting and accountability flow correctly. For now, the model applies to a single church instance; expansion to circuits/dioceses can be considered later.*
