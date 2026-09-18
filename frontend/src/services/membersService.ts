import { apiFetch } from './api';
import type { Member, PagedResponse, MemberFilters } from '../types/member';

const BASE_PATH = '/v1/Members';

function cleanMemberPayload(data: Partial<Member>) {
  const payload: Record<string, unknown> = { ...data };

  delete payload.id;
  delete payload.name;
  delete payload.createdAt;

  // Clean empty strings so .NET nullable types (e.g. DateOnly?, EmailAddress) deserialize cleanly
  if (!payload.dateOfBirth) delete payload.dateOfBirth;
  if (!payload.joinedDate) delete payload.joinedDate;
  if (!payload.emailAddress || (typeof payload.emailAddress === 'string' && payload.emailAddress.trim() === '')) delete payload.emailAddress;
  if (!payload.otherNames || (typeof payload.otherNames === 'string' && payload.otherNames.trim() === '')) delete payload.otherNames;
  if (!payload.gpsAddress || (typeof payload.gpsAddress === 'string' && payload.gpsAddress.trim() === '')) delete payload.gpsAddress;
  if (!payload.region || (typeof payload.region === 'string' && payload.region.trim() === '')) delete payload.region;

  return payload;
}

export const membersService = {
  list: async (filters: MemberFilters = {}, pageSize: number = 24, cursor?: string): Promise<PagedResponse<Member>> => {
    const params = new URLSearchParams();
    if (filters.name) params.append('Name', filters.name);
    if (filters.status) params.append('Status', filters.status);
    if (filters.gender) params.append('Gender', filters.gender);
    params.append('PageSize', pageSize.toString());
    if (cursor) params.append('Cursor', cursor);
    
    return apiFetch<PagedResponse<Member>>(`${BASE_PATH}?${params.toString()}`);
  },

  getById: async (id: string): Promise<Member> => {
    return apiFetch<Member>(`${BASE_PATH}/${id}`);
  },

  create: async (data: Partial<Member>): Promise<Member> => {
    const payload = cleanMemberPayload(data);
    return apiFetch<Member>(BASE_PATH, {
      method: 'POST',
      body: JSON.stringify(payload),
    });
  },

  update: async (id: string, data: Partial<Member>): Promise<Member> => {
    const payload = cleanMemberPayload(data);
    return apiFetch<Member>(`${BASE_PATH}/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    });
  },

  delete: async (id: string): Promise<void> => {
    return apiFetch<void>(`${BASE_PATH}/${id}`, {
      method: 'DELETE',
    });
  },
};
