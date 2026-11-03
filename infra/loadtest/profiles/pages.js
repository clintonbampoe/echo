// One function per screen. Each sends what the frontend sends when that screen opens (compare
// frontend/src/features/*/api), so a "page" costs the server what it costs for a real person.
import { sleep } from 'k6';
import { get, page, pick, rows } from '../lib/api.js';
import { ALL_TIME, daysAfter, daysBefore, monthStart, monthsBefore, weekStart, yearStart } from '../lib/dates.js';
import { TODAY } from '../lib/config.js';

// The dashboard's period picker (frontend/src/features/dashboard/model/periods.ts): six stretches that all end
// today, so everyone at a church who picks the same one asks for the same thing and shares one cached answer.
// The picker remembers the choice in the browser, so a person keeps theirs and only sometimes changes it.
const periods = [
  [45, 'this-week', (t) => ({ from: weekStart(t), to: t })],
  [10, 'last-week', (t) => ({ from: daysBefore(weekStart(t), 7), to: daysBefore(weekStart(t), 1) })],
  [20, 'this-month', (t) => ({ from: monthStart(t), to: t })],
  [10, 'last-3-months', (t) => ({ from: daysAfter(monthsBefore(t, 3), 1), to: t })],
  [8, 'last-6-months', (t) => ({ from: daysAfter(monthsBefore(t, 6), 1), to: t })],
  [7, 'this-year', (t) => ({ from: yearStart(t), to: t })],
];

const choosePeriod = () => {
  let roll = Math.random() * periods.reduce((sum, [w]) => sum + w, 0);
  for (const p of periods) if ((roll -= p[0]) <= 0) return p;
  return periods[0];
};

let remembered = null; // this virtual user's period, kept between iterations like the browser's storage
const stretch = () => {
  if (!remembered || Math.random() < 0.1) remembered = choosePeriod();
  return remembered[2](TODAY);
};
const months = (w) => ({ months: 12, to: w.to });
const previous = (w) => ({ ...w, includePrevious: true });

// What the app asks for once after sign-in.
export const shell = (ctx) =>
  page(ctx, [
    ['GET /users/me', '/users/me'],
    ['GET /congregation', '/congregation'],
  ]);

const attendanceCards = (w, m) => [
  ['GET /attendance/summary', '/attendance/summary', previous(w)],
  ['GET /attendance/summary', '/attendance/summary', w],
  ['GET /attendance/summary/monthly', '/attendance/summary/monthly', m],
  ['GET /attendance/sessions', '/attendance/sessions', { ...w, pageSize: 20 }],
  ['GET /attendance/sessions/summary', '/attendance/sessions/summary', w],
];

const peopleCards = (w, m) => [
  ['GET /members/summary', '/members/summary', w],
  ['GET /members/summary', '/members/summary', ALL_TIME],
  ['GET /members/summary/monthly', '/members/summary/monthly', m],
  ['GET /visitors/summary', '/visitors/summary', w],
  ['GET /visitors', '/visitors', { from: monthsBefore(TODAY, 6), to: TODAY, pageSize: 50 }],
  ['GET /events', '/events', { ...w, order: 'Asc', pageSize: 5 }],
];

const moneyCards = (w, m) => [
  ['GET /transactions/summary', '/transactions/summary', previous(w)],
  ['GET /transactions/summary', '/transactions/summary', w],
  ['GET /transactions/summary/monthly', '/transactions/summary/monthly', m],
  ['GET /tithes/summary', '/tithes/summary', previous(w)],
  ['GET /tithes/summary', '/tithes/summary', w],
  ['GET /tithes/summary/monthly', '/tithes/summary/monthly', m],
  ['GET /projects', '/projects', { ...ALL_TIME, pageSize: 50 }],
  ['GET /projects/summary', '/projects/summary'],
  ['GET /assets/summary/by-category', '/assets/summary/by-category', ALL_TIME],
];

// One range for the whole dashboard, as its period picker does.
const dashboard = (ctx, cards) => {
  const w = stretch();
  const m = months(w);
  page(ctx, cards.flatMap((c) => c(w, m)));
};

export const dashboardAll = (ctx) => dashboard(ctx, [attendanceCards, peopleCards, moneyCards]);
export const dashboardPeople = (ctx) => dashboard(ctx, [attendanceCards, peopleCards]);
export const dashboardMoney = (ctx) => dashboard(ctx, [moneyCards]);

export const members = (ctx) => {
  const list = rows(get(ctx, 'GET /members', '/members', { pageSize: 50 }));
  if (list.length) get(ctx, 'GET /members/{id}', `/members/${pick(list).id}`);
};

// Typing a name, a request after each letter group, a little apart. What a person types varies: every sequence gets to at
// least three letters (where the name index can be used), and most start with two, which is what the box sends first and
// which that index cannot serve.
const typing = [
  [2, 3, 4],
  [2, 3],
  [3, 4, 5],
  [2, 4, 6],
  [3, 6],
  [2, 3, 4, 5, 6],
];

export const memberSearch = (ctx) => {
  const list = rows(get(ctx, 'GET /members', '/members', { pageSize: 50 }));
  if (!list.length) return;
  const name = pick(list).name.toLowerCase();
  for (const length of typing[Math.floor(Math.random() * typing.length)]) {
    get(ctx, 'GET /members/search', '/members/search', { q: name.slice(0, length) });
    sleep(0.3 + Math.random() * 0.5);
  }
};

export const visitors = (ctx) => {
  const w = stretch();
  page(ctx, [
    ['GET /visitors', '/visitors', { from: monthsBefore(TODAY, 6), to: TODAY, pageSize: 50 }],
    ['GET /visitors/summary', '/visitors/summary', w],
  ]);
};

export const attendance = (ctx) => {
  const sessions = rows(get(ctx, 'GET /attendance/sessions', '/attendance/sessions', { ...stretch(), pageSize: 20 }));
  if (!sessions.length) return;
  const id = pick(sessions).id;
  page(ctx, [
    ['GET /attendance/sessions/{id}', `/attendance/sessions/${id}`],
    ['GET /attendance', '/attendance', { sessionId: id, pageSize: 50 }],
  ]);
};

export const tithes = (ctx) => {
  const w = stretch();
  page(ctx, [
    ['GET /tithes', '/tithes', { pageSize: 50 }],
    ['GET /tithes/summary', '/tithes/summary', previous(w)],
    ['GET /tithes/summary/monthly', '/tithes/summary/monthly', months(w)],
  ]);
};

export const finance = (ctx) => {
  const w = stretch();
  page(ctx, [
    ['GET /transactions', '/transactions', { pageSize: 50 }],
    ['GET /transactions/summary', '/transactions/summary', previous(w)],
    ['GET /transactions/summary/monthly', '/transactions/summary/monthly', months(w)],
    ['GET /transaction-categories', '/transaction-categories'],
  ]);
};

export const events = (ctx) => {
  const list = rows(get(ctx, 'GET /events', '/events', { ...stretch(), pageSize: 20 }));
  if (list.length) get(ctx, 'GET /events/{id}', `/events/${pick(list).id}`);
};

export const organizations = (ctx) =>
  page(ctx, [
    ['GET /organizations', '/organizations', { pageSize: 50 }],
    ['GET /organizations/summary', '/organizations/summary'],
  ]);

export const projects = (ctx) =>
  page(ctx, [
    ['GET /projects', '/projects', { ...ALL_TIME, pageSize: 50 }],
    ['GET /projects/summary', '/projects/summary'],
  ]);

export const assets = (ctx) =>
  page(ctx, [
    ['GET /assets', '/assets', { pageSize: 50 }],
    ['GET /assets/summary/by-category', '/assets/summary/by-category', ALL_TIME],
  ]);

export const users = (ctx) => get(ctx, 'GET /users', '/users', { pageSize: 50 });
