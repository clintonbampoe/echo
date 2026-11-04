import { sleep } from 'k6';
import exec from 'k6/execution';
import { focusAccounts, otherAccounts } from './lib/accounts.js';
import { visitOne, think } from './lib/api.js';
import {
  FOCUS_ARRIVAL_MINUTES,
  FOCUS_VUS,
  LIMITS,
  STORM_RAMP_SECONDS,
  STORM_RATES,
  STORM_START_MINUTES,
  STORM_STEP_SECONDS,
} from './lib/config.js';
import { sessionFor, stormSignIn } from './lib/session.js';
import { pattern } from './patterns/index.js';
import { profiles } from './profiles/index.js';
import { shell } from './profiles/pages.js';

// A few staff of one church work from the start. The people of the other churches come in visits, at the rate the
// pattern says (patterns/): a person arrives, opens 4-7 screens and leaves, whatever the server is doing, so a slow
// server cannot slow the load down. Every virtual user is one person with one account of their own: they sign in on
// their first visit, keep the session between visits and renew it when its access token is about to expire.
// There are as many people as accounts, so no more than that can be in a visit at once.
const maxPeople = otherAccounts.length - FOCUS_VUS;
const visitSeconds = 45; // about how long a visit takes, to size the pool of virtual users
const arrival = Math.min(FOCUS_ARRIVAL_MINUTES * 60, pattern.seconds);

export const options = {
  scenarios: {
    focus: {
      executor: 'ramping-vus',
      exec: 'focus',
      startVUs: 0,
      stages: [
        { duration: `${arrival}s`, target: FOCUS_VUS },
        { duration: `${pattern.seconds - arrival}s`, target: FOCUS_VUS },
      ],
      gracefulRampDown: '30s',
    },
    background: {
      executor: 'ramping-arrival-rate',
      exec: 'background',
      startRate: 0,
      timeUnit: '1s',
      stages: pattern.stages,
      // A couple of seconds of arrivals, no more. k6 hands out its virtual users in turn, every one before any is used
      // twice, so a big pool would have every one of them sign in (bcrypt) in the first minutes, at the visit rate. The
      // pool grows as the rate rises instead, and each person signs in when they first arrive.
      preAllocatedVUs: Math.min(maxPeople, Math.ceil(pattern.stages[0].target * 2)),
      maxVUs: maxPeople,
    },
    ...(Math.max(...STORM_RATES) > 0 && {
      storm: {
        executor: 'ramping-arrival-rate',
        exec: 'storm',
        startRate: STORM_RATES[0],
        timeUnit: '1s',
        stages: STORM_RATES.flatMap((rate) => [
          { duration: `${STORM_RAMP_SECONDS}s`, target: rate },
          { duration: `${STORM_STEP_SECONDS - STORM_RAMP_SECONDS}s`, target: rate },
        ]),
        startTime: `${STORM_START_MINUTES}m`,
        preAllocatedVUs: 20,
        maxVUs: 400,
      },
    }),
  },
  // Every step of the pattern is judged on its own hold, at constant load (see LIMITS in lib/config.js): the highest
  // step that passes is what the API carries. Each step also has a stop-loss, so a run that has clearly broken ends
  // instead of grinding on. Sign-in (bcrypt) and renewal are judged separately from the data. A 429 or a sign-in that
  // failed for another reason makes the run invalid, whatever the numbers say.
  thresholds: {
    rate_limited: ['count==0'],
    session_signin_failed: ['count==0'],
    // Not a limit: puts the whole hold in the summary, for run.sh --repeat.
    'http_req_duration{kind:data,phase:hold}': ['max>=0'],
    ...Object.fromEntries(
      pattern.steps.flatMap((step) => {
        const hold = `phase:hold,step:${step}`;
        const stopLoss = { delayAbortEval: '30s', abortOnFail: true };
        return [
          [`http_req_duration{kind:data,${hold}}`, [`p(95)<${LIMITS.dataP95}`, `p(99)<${LIMITS.dataP99}`]],
          [`http_req_duration{name:POST /auth/sessions/login,${hold}}`, [`p(95)<${LIMITS.authP95}`]],
          [`http_req_duration{name:POST /auth/sessions/refresh,${hold}}`, [`p(95)<${LIMITS.authP95}`]],
          [`http_req_failed{${hold}}`, [`rate<${LIMITS.failedRate}`]],
          [`http_req_failed{step:${step}}`, [{ threshold: 'rate<0.2', ...stopLoss }]],
          [`http_req_duration{kind:data,step:${step}}`, [{ threshold: 'p(95)<1000', ...stopLoss }]],
        ];
      }),
    ),
  },
  summaryTrendStats: ['avg', 'med', 'p(90)', 'p(95)', 'p(99)', 'max'],
  tags: { testid: __ENV.TESTID || 'adhoc' },
};

export const setup = () => {
  if (focusAccounts.length < FOCUS_VUS || otherAccounts.length <= FOCUS_VUS)
    throw new Error(`Needs ${FOCUS_VUS} accounts for the focus tenant and more for the others. Run ./prepare.sh first.`);
  if (pattern.peakRate * visitSeconds > maxPeople)
    console.warn(
      `A peak of ${pattern.peakRate} visits a second needs about ${pattern.peakRate * visitSeconds} people at once, but there are ${maxPeople} accounts: visits will be dropped (dropped_iterations).`,
    );
};

// A virtual user keeps its account for the whole run: ids are unique, so no two people share one as long as there
// are no more of them than accounts (maxPeople above).
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
