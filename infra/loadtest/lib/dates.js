const DAY = 24 * 60 * 60 * 1000;

const iso = (d) => d.toISOString().slice(0, 10);
const parse = (s) => new Date(`${s}T00:00:00Z`);

export const daysAfter = (date, days) => iso(new Date(parse(date).getTime() + days * DAY));

export const daysBefore = (date, days) => iso(new Date(parse(date).getTime() - days * DAY));

export const monthsBefore = (date, months) => {
  const d = parse(date);
  d.setUTCMonth(d.getUTCMonth() - months);
  return iso(d);
};

export const ALL_TIME = { from: '2000-01-01', to: '2100-01-01' };

// Weeks run Sunday to Saturday, as on the server and in the frontend.
export const weekStart = (date) => daysBefore(date, parse(date).getUTCDay());
export const monthStart = (date) => `${date.slice(0, 7)}-01`;
export const yearStart = (date) => `${date.slice(0, 4)}-01-01`;
