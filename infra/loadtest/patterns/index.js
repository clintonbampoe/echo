// A pattern is how the load changes over a run. It has:
//   stages    k6 stages of visits a second, for the background people
//   steps     the names of the parts of the run that are judged on their own (they become the `step` tag)
//   peakRate  the highest rate, to size the pool of virtual users
//   seconds   how long the run is
//   at(t)     { step, phase } for second t of the run; only phase "hold" is judged
// Add a pattern by adding a file here and a line below, then run it with PATTERN=<name>.
import exec from 'k6/execution';
import { PATTERN } from '../lib/config.js';
import stepped from './stepped.js';

const patterns = { stepped };

export const pattern = patterns[PATTERN];
if (!pattern) throw new Error(`No pattern "${PATTERN}". Known: ${Object.keys(patterns).join(', ')}`);

// The tags every request carries, so that results can be cut by step and phase.
export const tagsNow = () => pattern.at(exec.instance.currentTestRunDuration / 1000);
