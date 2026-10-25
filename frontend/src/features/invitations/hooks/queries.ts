import { usePagedList } from "@/lib/paged";
import * as api from "../api/invitations";
import { invitationKeys } from "./keys";

// One page at a time: `more()` loads the next, `rows` is every page loaded so far.
export const useInvitations = () =>
  usePagedList(invitationKeys.list, (cursor, signal) =>
    api.listInvitations(cursor, signal),
  );
