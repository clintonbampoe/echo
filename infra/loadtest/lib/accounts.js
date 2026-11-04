import { SharedArray } from 'k6/data';
import { FOCUS_TENANT } from './config.js';

// Written by prepare.sh. Read once and shared by every virtual user (a copy each would cost memory for every one of
// thousands of them). `n` numbers the accounts: it gives each person an address of their own (ip.js).
const load = () => JSON.parse(open('../.accounts.json')).map((a, n) => ({ ...a, n }));

export const focusAccounts = new SharedArray('focus accounts', () => load().filter((a) => a.tenant === FOCUS_TENANT));

const hash = (text) => {
  let h = 0;
  for (let i = 0; i < text.length; i++) h = (h * 31 + text.charCodeAt(i)) >>> 0;
  return h;
};

// Every other church, one account from each church in turn. Virtual users take accounts in this order, so a run with
// few people has a few from every church, not all of the first churches. Within a church the accounts are in a fixed
// scrambled order: the file is sorted by email, which would put all the accountants first, then the admins, and a
// run with few people would be all accountants (cheap screens) and none of the members.
export const otherAccounts = new SharedArray('other accounts', () => {
  const tenants = {};
  for (const a of load()) if (a.tenant !== FOCUS_TENANT) (tenants[a.tenant] ||= []).push(a);
  const byTenant = Object.values(tenants).map((t) => t.sort((a, b) => hash(a.email) - hash(b.email)));
  return Array.from({ length: Math.max(...byTenant.map((t) => t.length)) }, (_, i) => byTenant.map((t) => t[i]))
    .flat()
    .filter(Boolean);
});
