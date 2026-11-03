import http from 'k6/http';
import { check, sleep } from 'k6';
import { Counter } from 'k6/metrics';
import { BASE_URL, RENEW_BEFORE_SECONDS, SEED_PASSWORD } from './config.js';
import { addressOf } from './ip.js';

// Sign-ins and renewals the server turned away (429), and sessions that had to start over.
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
    tags: { name: `POST ${path}`, profile: account.role, tenant_kind: 'session', kind: 'auth' },
  });

// One sign-in that keeps nothing, for the login storm. Its own route name keeps it out of the sign-in numbers of the
// people in the test, so the storm shows up as its own series.
export const stormSignIn = (account) =>
  http.post(`${BASE_URL}/auth/sessions/login`, JSON.stringify({ email: account.email, password: SEED_PASSWORD }), {
    headers: { 'content-type': 'application/json', 'x-forwarded-for': addressOf(account) },
    tags: { name: 'POST /auth/sessions/login (storm)', profile: account.role, tenant_kind: 'storm', kind: 'auth' },
  });

const keep = (res) => {
  const b = res.json();
  return {
    accessToken: b.accessToken,
    refreshToken: b.refreshToken,
    expiresAt: Date.parse(b.accessTokenExpiresAt),
  };
};

// Tries again a few times when the server says too many (429), as a person would by waiting a moment.
const withRetries = (rejected, attempt) => {
  let res = attempt();
  for (let i = 0; i < 4 && res.status === 429; i++) {
    rejected.add(1);
    sleep(3 + Math.random() * 5);
    res = attempt();
  }
  if (res.status === 429) rejected.add(1);
  return res;
};

const signIn = (account) => {
  const res = withRetries(signInRejected, () =>
    post(account, 'login', '/auth/sessions/login', { email: account.email, password: SEED_PASSWORD }),
  );
  if (res.status !== 200 && res.status !== 429) signInFailed.add(1);
  return check(res, { 'sign-in is 200': (r) => r.status === 200 }) ? keep(res) : null;
};

const renew = (account) => {
  const res = withRetries(refreshRejected, () =>
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
