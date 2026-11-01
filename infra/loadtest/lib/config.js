// Through nginx, the way every real request arrives (its connection handling, rate limit, compression and log are
// part of what is measured). 5025 is the API itself, for when nginx is the question.
export const BASE_URL = __ENV.BASE_URL || 'http://localhost:8080/api/v1';

// Share of requests that ask the API to record a trace. The API traces 1% by default, and it follows the caller's
// flag, so the test sends the same share a real fleet would (browsers send none, and the API's 1% applies).
export const TRACE_RATIO = Number(__ENV.TRACE_RATIO || 0.01);

// "Today" for the seeded data. The seeder's ReferenceNow, not the clock: nothing was recorded after it,
// so dashboard ranges ending on the real date would be empty.
export const TODAY = __ENV.REF_DATE || '2026-10-20';

export const SEED_PASSWORD = __ENV.SEED_PASSWORD || 'admin@123';

export const FOCUS_TENANT = __ENV.FOCUS_TENANT || 'Grace Chapel International';

// Staff in the focus tenant. They arrive over the first FOCUS_ARRIVAL_MINUTES, the way an office signs in together.
export const FOCUS_VUS = Number(__ENV.FOCUS_VUS || 20);
export const FOCUS_ARRIVAL_MINUTES = Number(__ENV.FOCUS_ARRIVAL_MINUTES || 1);

// People signed in at the end of the ramp, the focus staff included.
export const PEAK_VUS = Number(__ENV.PEAK_VUS || 500);
export const RAMP_MINUTES = Number(__ENV.RAMP_MINUTES || 15);

// Seconds a person looks at a page before doing the next thing.
export const THINK_MIN = Number(__ENV.THINK_MIN || 3);
export const THINK_MAX = Number(__ENV.THINK_MAX || 10);

// Requests in the first minute are tagged phase=warmup: the API is still warming up (JIT, caches, connections).
export const WARMUP_SECONDS = Number(__ENV.WARMUP_SECONDS || 60);

// A session is renewed this long before its access token runs out.
export const RENEW_BEFORE_SECONDS = Number(__ENV.RENEW_BEFORE_SECONDS || 60);
