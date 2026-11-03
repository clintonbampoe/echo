import { sleep } from 'k6';
import exec from 'k6/execution';
import { focusAccounts, otherAccounts } from './lib/accounts.js';
import { visitOne, think } from './lib/api.js';
import {
  FOCUS_ARRIVAL_MINUTES,
  FOCUS_VUS,
  HOLD_MINUTES,
  PEAK_VUS,
  RAMP_MINUTES,
  STORM_MINUTES,
  STORM_RATE,
  STORM_START_MINUTES,
} from './lib/config.js';
import { sessionFor, stormSignIn } from './lib/session.js';
import { profiles } from './profiles/index.js';
import { shell } from './profiles/pages.js';

// A few staff of one church work from the start, while people from the other churches arrive slowly until
// PEAK_VUS are signed in in total. Every virtual user is one person: they sign in once, renew their session
// when its access token is about to expire, and open a few screens with a pause between them.
const backgroundPeak = PEAK_VUS - FOCUS_VUS;
const warm = Math.max(1, Math.round(RAMP_MINUTES * 0.1));
const arrival = Math.min(FOCUS_ARRIVAL_MINUTES, Math.max(1, RAMP_MINUTES - 1));
// With HOLD_MINUTES the ramp is RAMP_MINUTES long and the hold comes after it; without, the hold is the last tenth.
const hold = HOLD_MINUTES > 0 ? HOLD_MINUTES : warm;
const rampRest = HOLD_MINUTES > 0 ? RAMP_MINUTES - warm : RAMP_MINUTES - 2 * warm;
const total = warm + rampRest + hold;

export const options = {
  scenarios: {
    focus: {
      executor: 'ramping-vus',
      exec: 'focus',
      startVUs: 0,
      stages: [
        { duration: `${arrival}m`, target: FOCUS_VUS },
        { duration: `${total - arrival}m`, target: FOCUS_VUS },
      ],
      gracefulRampDown: '30s',
    },
    background: {
      executor: 'ramping-vus',
      exec: 'background',
      startVUs: 0,
      stages:
        HOLD_MINUTES > 0
          ? [
              // People arrive evenly, so the peak lands at RAMP_MINUTES and is then held at a constant load.
              { duration: `${RAMP_MINUTES}m`, target: backgroundPeak },
              { duration: `${HOLD_MINUTES}m`, target: backgroundPeak },
            ]
          : [
              { duration: `${warm}m`, target: Math.round(backgroundPeak / 5) },
              { duration: `${rampRest}m`, target: backgroundPeak },
              { duration: `${hold}m`, target: backgroundPeak },
            ],
      gracefulRampDown: '30s',
    },
    ...(STORM_RATE > 0 && {
      storm: {
        executor: 'constant-arrival-rate',
        exec: 'storm',
        rate: STORM_RATE,
        timeUnit: '1s',
        duration: `${STORM_MINUTES}m`,
        startTime: `${STORM_START_MINUTES}m`,
        preAllocatedVUs: 20,
        maxVUs: 400,
      },
    }),
  },
  // The targets (2 vCPU, 4 GB, 500 people signed in). The app's data requests: p95 under 10 ms, and 15 ms at the very
  // most (the 10 is reported by run.sh; the 15 fails the run). Sign-in is its own thing, bcrypt takes about 175 ms by
  // design: p95 under 500 ms. Renewals are not bcrypt and are judged like the data. The first minute is warm-up; with
  // HOLD_MINUTES only the hold counts, at constant load.
  thresholds: {
    http_req_failed: ['rate<0.01'],
    [`http_req_duration{kind:data,phase:${HOLD_MINUTES > 0 ? 'hold' : 'steady'}}`]: ['p(95)<15', 'p(99)<50'],
    'http_req_duration{name:POST /auth/sessions/login}': ['p(95)<500'],
    'http_req_duration{name:POST /auth/sessions/refresh}': ['p(95)<50'],
    session_refresh_rejected: ['count==0'],
    session_signin_rejected: ['count==0'],
    session_signin_failed: ['count==0'],
  },
  summaryTrendStats: ['avg', 'med', 'p(90)', 'p(95)', 'p(99)', 'max'],
  tags: { testid: __ENV.TESTID || 'adhoc' },
};

export const setup = () => {
  if (!focusAccounts.length || !otherAccounts.length)
    throw new Error('No accounts for the focus tenant or the others. Run ./prepare.sh first.');
};

const visit = (kind, accounts) => {
  const account = accounts[exec.vu.idInTest % accounts.length];
  const ctx = { account, kind };

  if (!sessionFor(account)) {
    sleep(30 + Math.random() * 30); // could not sign in: a person waits a while, they do not retry every few seconds
    return;
  }

  shell(ctx);
  const screens = 4 + Math.floor(Math.random() * 4);
  for (let i = 0; i < screens; i++) {
    think();
    visitOne(ctx, profiles[account.role]);
  }
  think();
};

// People signing in at once: a different account each time, so none of it is a repeat of a cached answer.
export const storm = () => stormSignIn(otherAccounts[exec.scenario.iterationInTest % otherAccounts.length]);

export const focus = () => visit('focus', focusAccounts);
export const background = () => visit('background', otherAccounts);
