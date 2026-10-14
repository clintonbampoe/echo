// Where each kind of record lives. Lists link to these, so the address of a page is written once.
// A page that is not built yet is a placeholder in app/routes.tsx, so a link never ends on a 404.
export const paths = {
  members: "/members",
  visitors: "/visitors",
  member: (id: string) => `/members/${id}`,
  visitor: (id: string) => `/visitors/${id}`,
  event: (id: string) => `/events/${id}`,
  organization: (id: string) => `/organizations/${id}`,
  session: (id: string) => `/attendance/sessions/${id}`,
  project: (id: string) => `/projects/${id}`,
};
