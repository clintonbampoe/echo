import exec from 'k6/execution';
import { focusAccounts, otherAccounts } from './lib/accounts.js';
import { visitOne, think } from './lib/api.js';
import { FOCUS_ARRIVAL_MINUTES, FOCUS_VUS, PEAK_VUS, RAMP_MINUTES } from './lib/config.js';
import { sessionFor } from './lib/session.js';
import { profiles } from './profiles/index.js';
import { shell } from './profiles/pages.js';

// A few staff of one church work from the start, while people from the other churches arrive slowly until
// PEAK_VUS are signed in in total. Every virtual user is one person: they sign in once, renew their session
// when its access token is about to expire, and open a few screens with a pause between them.
const backgroundPeak = PEAK_VUS - FOCUS_VUS;
const warm = Math.max(1, Math.round(RAMP_MINUTES * 0.1));
const arrival = Math.min(FOCUS_ARRIVAL_MINUTES, Math.max(1, RAMP_MINUTES - 1));

export const options = {
  scenarios: {
    focus: {
      executor: 'ramping-vus',
      exec: 'focus',
      startVUs: 0,
      stages: [
        { duration: `${arrival}m`, target: FOCUS_VUS },
        { duration: `${RAMP_MINUTES - arrival}m`, target: FOCUS_VUS },
      ],
      gracefulRampDown: '30s',
    },
    background: {
      executor: 'ramping-vus',
      exec: 'background',
      startVUs: 0,
      stages: [
        { duration: `${warm}m`, target: Math.round(backgroundPeak / 5) },
        { duration: `${RAMP_MINUTES - 2 * warm}m`, target: backgroundPeak },
        { duration: `${warm}m`, target: backgroundPeak },
      ],
      gracefulRampDown: '30s',
    },
  },
  // The target for now (2 vCPU, 4 GB): 500 people signed in, and the app's data requests answer in under 25 ms
  // for 95 in 100 and under 50 ms for 99 in 100. Sign-in is not in it: bcrypt takes about 200 ms by design.
  // The first minute is warm-up and is left out. We only raise the bar when there are more users.
  thresholds: {
    http_req_failed: ['rate<0.01'],
    'http_req_duration{kind:data,phase:steady}': ['p(95)<25', 'p(99)<50'],
    'http_req_duration{name:POST /auth/sessions/login}': ['p(95)<1000'],
    session_refresh_rejected: ['count==0'],
    session_signin_rejected: ['count==0'],
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
    think(); // could not sign in; try again next time round
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

export const focus = () => visit('focus', focusAccounts);
export const background = () => visit('background', otherAccounts);
