// Who does what. Each role has a list of [weight, screen]; a person opens a few of them per visit.
// Reads only for now: writes need valid payloads and would grow the database during the run.
import * as p from './pages.js';

export const profiles = {
  Admin: [
    [30, p.dashboardAll],
    [14, p.members],
    [10, p.attendance],
    [8, p.tithes],
    [8, p.finance],
    [8, p.events],
    [5, p.projects],
    [4, p.assets],
    [4, p.organizations],
    [5, p.users],
    [4, p.visitors],
  ],
  Accountant: [
    [20, p.dashboardMoney],
    [30, p.tithes],
    [30, p.finance],
    [10, p.projects],
    [10, p.assets],
  ],
  Clerk: [
    [10, p.dashboardPeople],
    [35, p.attendance],
    [25, p.members],
    [15, p.visitors],
    [10, p.events],
    [5, p.organizations],
  ],
  Member: [
    [40, p.events],
    [25, p.organizations],
    [20, p.memberSearch],
    [15, p.members],
  ],
};
