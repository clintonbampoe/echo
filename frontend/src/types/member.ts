export interface Member {
  id: string;
  firstName: string;
  lastName: string;
  email?: string;
  phoneNumber?: string;
  gender?: string;
  dateOfBirth?: string;
  status: string;
}

export interface PagedResponse<T> {
  data: T[];
  hasMore: boolean;
  nextCursor: string | null;
}

export interface MemberFilters {
  searchTerm?: string;
  status?: string;
}
