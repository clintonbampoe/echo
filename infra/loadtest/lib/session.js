import http from 'k6/http';
import { check } from 'k6';
import { Counter } from 'k6/metrics';
import { BASE_URL, RENEW_BEFORE_SECONDS, SEED_PASSWORD, stormRateAt } from './config.js';
import exec from 'k6/execution';
import { addressOf } from './ip.js';
import { tagsNow } from '../patterns/index.js';

// Requests the server turned away (429). The rate limits must never touch a load test: it measures what the API
// delivers, not what it refuses, so a run with any of these is not a valid run.
export const rateLimited = new Counter('rate_limited');
// The same, for sign-ins and renewals, and sessions that had to start over.
export const refreshRejected = new Counter('session_refresh_rejected');
export const signInRejected = new Counter('session_signin_rejected');
export const startedOver = new Counter('session_started_over');
// Sign-ins refused for any other reason (wrong password, unverified email): a broken account, not load.
export const signInFailed = new Counter('session_signin_failed');

// One person's session. A virtual user keeps it between iterations, like a browser tab.
let session = null;

const post = (account, name, path, body) =>
  http.post(`${BASE_URL}${path}`, JSON.stringify(body), {
    headers: { 'content-type': 'application/json', 'x-forwarded-for': addressOf(account) },
    tags: { name: `POST ${path}`, profile: account.role, tenant_kind: 'session', kind: 'auth', ...tagsNow() },
  });

// One sign-in that keeps nothing, for the login storm. Its own route name keeps it out of the sign-in numbers of the
// people in the test, so the storm shows up as its own series.
export const stormSignIn = (account) =>
  http.post(`${BASE_URL}/auth/sessions/login`, JSON.stringify({ email: account.email, password: SEED_PASSWORD }), {
    headers: { 'content-type': 'application/json', 'x-forwarded-for': addressOf(account) },
    tags: {
      name: 'POST /auth/sessions/login (storm)',
      profile: account.role,
      tenant_kind: 'storm',
      kind: 'auth',
      storm_rate: String(stormRateAt((Date.now() - exec.scenario.startTime) / 1000)),
    },
  });

const keep = (res) => {
  const b = res.json();
  return {
    accessToken: b.accessToken,
    refreshToken: b.refreshToken,
    expiresAt: Date.parse(b.accessTokenExpiresAt),
  };
};

// A 429 is counted and fails the run; the test does not wait and try again, which would hide it.
const attempt = (rejected, request) => {
  const res = request();
  if (res.status === 429) {
    rejected.add(1);
    rateLimited.add(1);
  }
  return res;
};

const signIn = (account) => {
  const res = attempt(signInRejected, () =>
    post(account, 'login', '/auth/sessions/login', { email: account.email, password: SEED_PASSWORD }),
  );
  if (res.status !== 200 && res.status !== 429) signInFailed.add(1);
  return check(res, { 'sign-in is 200': (r) => r.status === 200 }) ? keep(res) : null;
};

const renew = (account) => {
  const res = attempt(refreshRejected, () =>
    post(account, 'refresh', '/auth/sessions/refresh', { refreshToken: session.refreshToken }),
  );
  return check(res, { 'renewal is 200': (r) => r.status === 200 }) ? keep(res) : null;
};

// A session that is good for the next request: started on the first call, renewed shortly before its
// access token runs out, and started over if it cannot be renewed. Null if even that failed.
export const sessionFor = (account) => {
  if (session && Date.now() < session.expiresAt - RENEW_BEFORE_SECONDS * 1000) return session;

  if (session) {
    const renewed = renew(account);
    if (renewed) return (session = renewed);
    startedOver.add(1);
  }
  return (session = signIn(account));
};
