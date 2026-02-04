# Requirements: Attendance

## Service & Event Tracking

- **AT-ST001:** The system must record attendance across services and events, with fields for event name, date, type (service, fellowship, outreach), and participant list. *// “Participant list” can be either individual members (linked to the Member Registry) or aggregate counts when names are not available. This ensures flexibility in how attendance is captured.*

- **AT-ST002:** The system must link attendance records to individual members in the **Member Registry** for accountability and participation tracking. 
  
  *// This ensures attendance isn’t just a headcount but tied to actual members, enabling analysis of engagement patterns.*

- **AT-ST003:** The system must support recording attendance for both recurring events (weekly services) and one‑time events (special programs).

- **AT-ST004:** The system must provide search and filter options for attendance records by member, group, event type, or date.

- **AT-ST005:** The system may maintain audit logs of attendance entries, showing who recorded them and when. 
  
  *// Note: This is optional. It adds transparency but may not be critical for MVP. A lightweight log (user + timestamp) could be sufficient.*

## Engagement Analysis

- **AT-EA001:** The system must generate statistics on attendance trends (e.g., average attendance per service, participation rates by group).

- **AT-EA002:** The system should link attendance data to contribution patterns, enabling correlation analysis between participation and giving. 
  
  *// This is a stretch requirement. It’s complex to implement but valuable for leadership insights. At minimum, the data model should support linking attendance and contributions for future expansion.*

- **AT-EA003:** The system must provide summaries of attendance by member, group, or event type over configurable periods (weekly, monthly, yearly).

- **AT-EA004:** The system must support exporting attendance reports in standard formats (PDF, Excel, CSV).
