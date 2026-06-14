# Entities

Entities are the database models.
Every entity in this system belongs to a congregation, enforced via `ICongregationEntity`.

## Base Class: ICongregationEntity

Every entity inherits from `ICongregationEntity`, which automatically gives it:

- `Id` — a UUID v7 (time-sortable, globally unique)
- `CongregationId` — which congregation this record belongs to
- `Congregation` — navigation property to the congregation
- `CreatedAt` — when the record was created

## Interface: ISoftDeletableEntity

Entities that implement this are never physically deleted from the database.
Instead, a `DeletedAt` timestamp is set when a record is deleted.
Records with a `DeletedAt` value are filtered out automatically on all reads.

This preserves historical data and makes accidental deletions recoverable.

## Interface: ISearchableEntity

Entities that implement this have a `Name` field that can be searched via the `searchTerm` query parameter.

## Entities Overview

| Entity                | What it represents                                                        |
| --------------------- | ------------------------------------------------------------------------- |
| `Congregation`        | The church itself — the root that all other data belongs to               |
| `Member`              | A person registered in the congregation                                   |
| `Organization`        | A group or ministry within the congregation (e.g. choir, youth group)     |
| `OrganizationMember`  | A member's membership in an organization, including their role            |
| `Event`               | An event organized by a group, with date, location, and optional capacity |
| `EventRegistration`   | A member's registration for a specific event                              |
| `EventAttendance`     | Records that a member actually showed up at an event (with check-in time) |
| `AttendanceRecord`    | Tracks church service attendance for a member on a given date             |
| `Asset`               | A physical item owned by the congregation (e.g. chairs, projector)        |
| `AssetCategory`       | A category for grouping assets                                            |
| `Project`             | A congregation project with a funding target and status                   |
| `ProjectCategory`     | A category for grouping projects                                          |
| `ProjectContribution` | A payment made toward a project                                           |
| `Tithe`               | A tithe payment by a member, recorded by month and year                   |
| `Transaction`         | A general financial transaction (income or expenditure)                   |
| `TransactionCategory` | A category for classifying transactions                                   |
| `User`                | A system user (authentication-related)                                    |
