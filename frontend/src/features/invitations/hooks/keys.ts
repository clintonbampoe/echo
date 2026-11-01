// Every cache key invitations use, in one place.
export const invitationKeys = {
  all: ["invitations"] as const,
  list: ["invitations", "list"] as const,
  summary: ["invitations", "summary"] as const,
  one: (id: string) => ["invitations", "one", id] as const,
};
