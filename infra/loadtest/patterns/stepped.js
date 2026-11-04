// Load that goes up in steps: the rate rises over STEP_RAMP_SECONDS, then holds for STEP_HOLD_SECONDS so there is a
// stretch of constant load to measure. Only the hold counts (phase "hold"); each step is tagged with its rate, so the
// numbers of one step can be read on their own. STEP_WARMUP_SECONDS (default 0) runs the first rate before the first
// step and judges none of it: a cold API (JIT, empty caches, the first sign-ins) is slower than a warm one, and that
// would otherwise land on the first step.
//
// A rate is visits a second: one visit is one person opening 4-7 screens (about 45 s and 25 requests). The old
// 500-person test was about 7.5 visits a second. At most as many people are in a visit at once as there are accounts.
const rates = (__ENV.STEP_RATES || '8,11,14,17,20,23,26').split(',').map(Number);
const ramp = Number(__ENV.STEP_RAMP_SECONDS || 120);
const hold = Number(__ENV.STEP_HOLD_SECONDS || 120);
const warmup = Number(__ENV.STEP_WARMUP_SECONDS || 0);

export default {
  stages: [
    ...(warmup > 0 ? [{ duration: `${warmup}s`, target: rates[0] }] : []),
    ...rates.flatMap((rate) => [
      { duration: `${ramp}s`, target: rate },
      { duration: `${hold}s`, target: rate },
    ]),
  ],
  steps: rates.map(String),
  peakRate: Math.max(...rates),
  seconds: warmup + rates.length * (ramp + hold),
  // Which step and phase a moment of the run (in seconds) belongs to.
  at: (seconds) => {
    if (seconds < warmup) return { step: 'warmup', phase: 'warmup' };
    const t = seconds - warmup;
    const i = Math.min(Math.floor(t / (ramp + hold)), rates.length - 1);
    return { step: String(rates[i]), phase: t - i * (ramp + hold) < ramp ? 'ramp' : 'hold' };
  },
};
