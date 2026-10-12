import { apiFetch } from "./api";
import type { PagedResponse } from "../types/pagination";
import type { MemberAttendanceRecord } from "../types/memberProfile";
import type { Tithe } from "../types/tithe";
import type { OrganizationMember } from "../types/organizationMember";
import type { EventAttendance } from "../types/eventAttendance";
import type { EventRegistration } from "../types/eventRegistration";
import type { Project } from "../types/project";
import type { Event } from "../types/event";

// List endpoints default to a short window (this week / this year) when no
// range is given, so a member's full history needs an explicit wide range.
const ALL_TIME_FROM = "1900-01-01";
const ALL_TIME_TO = "2100-12-31";
const PAGE_SIZE = 25;

const buildParams = (
  filters: Record<string, string>,
  cursor?: string,
  withDateRange = false,
) => {
  const params = new URLSearchParams(filters);
  if (withDateRange) {
    params.append("From", ALL_TIME_FROM);
    params.append("To", ALL_TIME_TO);
  }
  params.append("PageSize", PAGE_SIZE.toString());
  if (cursor) params.append("Cursor", cursor);
  return params.toString();
};

export const memberProfileService = {
  attendance: (memberId: string, cursor?: string) =>
    apiFetch<PagedResponse<MemberAttendanceRecord>>(
      `/v1/attendance?${buildParams({ PersonId: memberId }, cursor, true)}`,
    ),

  tithes: (memberId: string, cursor?: string) =>
    apiFetch<PagedResponse<Tithe>>(
      `/v1/tithes?${buildParams({ MemberId: memberId }, cursor, true)}`,
    ),

  organizations: (memberId: string, cursor?: string) =>
    apiFetch<PagedResponse<OrganizationMember>>(
      `/v1/organization-members/member/${memberId}?${buildParams({}, cursor)}`,
    ),

  eventsAttended: (memberId: string, cursor?: string) =>
    apiFetch<PagedResponse<EventAttendance>>(
      `/v1/event-attendance/member/${memberId}?${buildParams({}, cursor)}`,
    ),

  eventsRegistered: (memberId: string, cursor?: string) =>
    apiFetch<PagedResponse<EventRegistration>>(
      `/v1/event-registrations/member/${memberId}?${buildParams({}, cursor)}`,
    ),

  projectsManaged: (memberId: string, cursor?: string) =>
    apiFetch<PagedResponse<Project>>(
      `/v1/projects?${buildParams({ ManagerId: memberId }, cursor, true)}`,
    ),

  eventsOrganized: (memberId: string, cursor?: string) =>
    apiFetch<PagedResponse<Event>>(
      `/v1/events?${buildParams({ OrganizerId: memberId }, cursor, true)}`,
    ),
};
