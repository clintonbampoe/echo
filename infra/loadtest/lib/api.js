import http from 'k6/http';
import { check, sleep } from 'k6';
import exec from 'k6/execution';
import { BASE_URL, THINK_MAX, THINK_MIN, TRACE_RATIO, WARMUP_SECONDS } from './config.js';
import { addressOf } from './ip.js';
import { sessionFor } from './session.js';

const hex = (n) => {
  let s = '';
  for (let i = 0; i < n; i++) s += Math.floor(Math.random() * 16).toString(16);
  return s;
};

// The first minute is the API warming up (JIT, empty caches, connections opening). It is kept in the
// data but tagged, so runs can be compared on the steady part only.
const phase = () => (exec.instance.currentTestRunDuration < WARMUP_SECONDS * 1000 ? 'warmup' : 'steady');

// One request as the browser would send it. `name` is the route template (/members/{id}) so that every
// member does not become its own series in Prometheus; `profile` is who is asking. The traceparent header
// makes the API continue this trace when its sampled flag is set (TRACE_RATIO of requests), so those can be opened
// in Tempo by their trace id.
export const request = (ctx, name, path, query = {}) => ({
  method: 'GET',
  url: `${BASE_URL}${path}${toQuery(query)}`,
  params: {
    headers: {
      authorization: `Bearer ${sessionFor(ctx.account)?.accessToken ?? ''}`,
      'x-forwarded-for': addressOf(ctx.account.tenant),
      traceparent: `00-${hex(32)}-${hex(16)}-${Math.random() < TRACE_RATIO ? '01' : '00'}`,
    },
    tags: { name, profile: ctx.account.role, tenant_kind: ctx.kind, phase: phase(), kind: 'data' },
  },
});

const toQuery = (q) => {
  const parts = Object.entries(q)
    .filter(([, v]) => v !== undefined)
    .map(([k, v]) => `${k}=${encodeURIComponent(v)}`);
  return parts.length ? `?${parts.join('&')}` : '';
};

const checked = (res) => {
  check(res, { 'status is 2xx': (r) => r.status >= 200 && r.status < 300 });
  if (res.status >= 400 && res.status !== 429)
    console.warn(`${res.status} ${res.request.method} ${res.request.url} trace=${res.request.headers.Traceparent}`);
  return res;
};

// One request.
export const get = (ctx, name, path, query) => {
  const r = request(ctx, name, path, query);
  return checked(http.get(r.url, r.params));
};

// Requests a page sends at once, which is what a browser does when a screen opens.
export const page = (ctx, requests) =>
  http.batch(requests.map((r) => request(ctx, ...r))).map(checked);

// The `data` of a list answer, or an empty list when the request failed.
export const rows = (res) => {
  try {
    return res.json('data') || [];
  } catch (_) {
    return [];
  }
};

export const pick = (list) => list[Math.floor(Math.random() * list.length)];

export const think = () => sleep(THINK_MIN + Math.random() * (THINK_MAX - THINK_MIN));

// Picks one of the weighted pages. `pages` is [[weight, fn], ...].
export const visitOne = (ctx, pages) => {
  const total = pages.reduce((sum, [w]) => sum + w, 0);
  let roll = Math.random() * total;
  for (const [weight, fn] of pages) {
    roll -= weight;
    if (roll <= 0) return fn(ctx);
  }
};
