// Through nginx, the way every real request arrives (its connection handling, rate limit, compression and log are
// part of what is measured). 5025 is the API itself, for when nginx is the question.
export const BASE_URL = __ENV.BASE_URL || 'https://echo.localhost/api/v1';

// Share of requests that ask the API to record a trace. The API traces 1% by default, and it follows the caller's
// flag, so the test sends the same share a real fleet would (browsers send none, and the API's 1% applies). Tracing
// every request would spend API CPU that production does not, which matters when CPU is the limit.
export const TRACE_RATIO = Number(__ENV.TRACE_RATIO ?? 0.01);

// "Today" for the seeded data. The seeder's ReferenceNow, not the clock: nothing was recorded after it,
// so dashboard ranges ending on the real date would be empty.
export const TODAY = __ENV.REF_DATE || '2026-10-20';

// A burst of sign-ins on top of the usual load, to see how much bcrypt (about 200 ms of CPU each) takes from everything
// else. Off unless STORM_RATES (sign-ins a second, one number or steps like 2,4,6,9) or STORM_RATE is set; it starts
// STORM_START_MINUTES in and spends STORM_STEP_SECONDS on each rate (STORM_MINUTES in all when there is one rate).
export const STORM_RATES = (__ENV.STORM_RATES || __ENV.STORM_RATE || '0').split(',').map(Number);
export const STORM_MINUTES = Number(__ENV.STORM_MINUTES || 2);
export const STORM_STEP_SECONDS = Number(__ENV.STORM_STEP_SECONDS || STORM_MINUTES * 60);
export const STORM_START_MINUTES = Number(__ENV.STORM_START_MINUTES || 8);
// The rate at second t of the storm, so each sign-in can be told apart by the rate it arrived at.
export const stormRateAt = (t) => STORM_RATES[Math.min(Math.floor(t / STORM_STEP_SECONDS), STORM_RATES.length - 1)];

export const SEED_PASSWORD = __ENV.SEED_PASSWORD || 'admin@123';

export const FOCUS_TENANT = __ENV.FOCUS_TENANT || 'Grace Chapel International';

// Staff in the focus tenant. They arrive over the first FOCUS_ARRIVAL_MINUTES, the way an office signs in together.
export const FOCUS_VUS = Number(__ENV.FOCUS_VUS || 20);
export const FOCUS_ARRIVAL_MINUTES = Number(__ENV.FOCUS_ARRIVAL_MINUTES || 1);

// How the load changes over the run: the name of a file in patterns/. See patterns/index.js.
export const PATTERN = __ENV.PATTERN || 'stepped';

// What the API has to deliver, at every step of a pattern. Data is the app's own requests (not sign-in or renewal).
export const LIMITS = {
  dataP95: 25, // ms
  dataP99: 50, // ms
  authP95: 500, // ms, sign-in (bcrypt) and renewal
  failedRate: 0.01,
};

// Seconds a person looks at a page before doing the next thing.
export const THINK_MIN = Number(__ENV.THINK_MIN || 3);
export const THINK_MAX = Number(__ENV.THINK_MAX || 10);

// A session is renewed this long before its access token runs out.
export const RENEW_BEFORE_SECONDS = Number(__ENV.RENEW_BEFORE_SECONDS || 60);
