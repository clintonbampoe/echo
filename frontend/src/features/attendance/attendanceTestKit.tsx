// Shared by the attendance page tests: sample sessions and a fetch that answers like the server.
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render } from "@testing-library/react";
import type { ReactNode } from "react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { saveTokens } from "@/lib/tokens";
import type { CheckIn, Session, SessionLabel } from "./api";

export const json = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), { status });
export const page = (data: unknown[]) =>
  json({ data, hasMore: false, next: null });

export const service: SessionLabel = {
  id: 3,
  name: "Sunday Service",
  category: "Service",
};

export const session = (over: Partial<Session> = {}): Session => ({
  id: "s1",
  sessionLabelId: 3,
  sessionLabelName: "Sunday Service",
  category: "Service",
  date: "2026-10-11",
  name: null,
  topic: "Walking in Faith",
  startsAt: "08:00:00",
  endsAt: "10:30:00",
  status: "Completed",
  createdByName: "Ama Serwaa Boateng",
  createdAt: "",
  attendanceCount: 2,
  ...over,
});

export const checkIn = (over: Partial<CheckIn> = {}): CheckIn => ({
  id: "c1",
  sessionId: "s1",
  sessionDate: "2026-10-11",
  sessionLabelName: "Sunday Service",
  checkedInAt: "2026-10-11T09:04:00",
  personId: "m1",
  personName: "Kwame Asante",
  personKind: "Member",
  ...over,
});

export const summary = {
  totalPresent: 2,
  membersPresent: 1,
  visitorsPresent: 1,
  firstTimeVisitors: 1,
};

export const asked = (fetchMock: ReturnType<typeof vi.fn>) =>
  fetchMock.mock.calls.map((c) => ({
    url: new URL((c as [string])[0], "http://x"),
    init: (c as [string, RequestInit?])[1],
  }));

export function signIn() {
  saveTokens({
    accessToken: "a",
    accessTokenExpiresAt: "2099-01-01T00:00:00Z",
    refreshToken: "r",
    refreshTokenExpiresAt: "2099-01-01T00:00:00Z",
  });
}

export function renderAt(address: string, routes: ReactNode) {
  return render(
    <QueryClientProvider
      client={
        new QueryClient({ defaultOptions: { queries: { retry: false } } })
      }
    >
      <MemoryRouter initialEntries={[address]}>
        <Routes>{routes}</Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

export const route = (path: string, element: ReactNode) => (
  <Route path={path} element={element} />
);
