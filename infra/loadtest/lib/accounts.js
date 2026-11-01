import { FOCUS_TENANT } from './config.js';

// Written by prepare.sh. Opened once per run, in the init context.
export const all = JSON.parse(open('../.accounts.json'));

export const focusAccounts = all.filter((a) => a.tenant === FOCUS_TENANT);
export const otherAccounts = all.filter((a) => a.tenant !== FOCUS_TENANT);
