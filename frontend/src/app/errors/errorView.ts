import { isRouteErrorResponse } from "react-router-dom";
import { isApiError } from "@/lib/api";
import type { IconName } from "@/ui";

export type ErrorKind =
  "notFound" | "forbidden" | "offline" | "server" | "stale" | "crash";

// A new version of the app replaces the files an open tab still points at; loading one of them then fails.
const STALE = /dynamically imported module|importing a module script failed/i;

// The kind for an HTTP status, from either source of one.
function byStatus(status: number): ErrorKind {
  if (status === 0) return "offline";
  if (status === 404) return "notFound";
  if (status === 403) return "forbidden";
  return status >= 500 ? "server" : "crash";
}

// Which of our screens an error deserves. Anything we do not know is a crash.
export function classify(error: unknown): ErrorKind {
  if (isRouteErrorResponse(error)) return byStatus(error.status);
  if (isApiError(error)) return byStatus(error.status);
  if (error instanceof Error && STALE.test(error.message)) return "stale";
  return "crash";
}

export type ErrorView = {
  icon: IconName;
  broken: boolean;
  title: string;
  text: string;
  // Reloading fixes it (a broken page, no network, an old version), or the way out is to go somewhere else.
  fix: "reload" | "leave";
  reload: string;
};

export const errorViews: Record<ErrorKind, ErrorView> = {
  notFound: {
    icon: "search",
    broken: false,
    title: "We could not find that page",
    text: "The link may be old, or the address may have a typo.",
    fix: "leave",
    reload: "",
  },
  forbidden: {
    icon: "shield",
    broken: false,
    title: "You do not have access to this",
    text: "Your role cannot open this page. Ask an admin of your church if you need it.",
    fix: "leave",
    reload: "",
  },
  offline: {
    icon: "link",
    broken: true,
    title: "You seem to be offline",
    text: "Check your connection, then try again. Nothing you have saved is lost.",
    fix: "reload",
    reload: "Try again",
  },
  server: {
    icon: "alert",
    broken: true,
    title: "Something went wrong on our side",
    text: "It is not you. Give it a moment and try again.",
    fix: "reload",
    reload: "Try again",
  },
  stale: {
    icon: "clock",
    broken: false,
    title: "Echo has been updated",
    text: "Reload to get the latest version. It takes a second.",
    fix: "reload",
    reload: "Reload",
  },
  crash: {
    icon: "alert",
    broken: true,
    title: "This page hit a problem",
    text: "Your data is safe. Try again, and if it keeps happening, tell your church admin.",
    fix: "reload",
    reload: "Try again",
  },
};
