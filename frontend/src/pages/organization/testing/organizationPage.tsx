// What the organization page tests share: a signed-in session, a fake server, and the page drawn in its route.
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { saveTokens } from "@/lib/tokens";
import type { OrganizationMember } from "@/features/organizations";
import {
  aMembership,
  anOrganization,
} from "@/features/organizations/testing/fixtures";
import { OrganizationPage } from "../OrganizationPage";

export const json = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), { status });

export const choir = anOrganization();
const person = (
  id: string,
  memberName: string,
  role: OrganizationMember["role"],
  joinedAt: string,
) =>
  aMembership({
    id: `om-${id}`,
    memberId: id,
    memberName,
    role,
    joinedAt,
    createdAt: joinedAt,
  });
export const people = [
  person("m1", "Kofi Mensah", "Leader", "2016-01-05"),
  person("m2", "Comfort Addo", "Secretary", "2018-03-11"),
  person("m3", "Kwame Asante", "Member", "2020-01-12"),
];
export const event = {
  id: "e1",
  name: "Christmas Carols",
  startDate: "2099-12-20",
  startTime: "17:00:00",
  location: "Main Auditorium",
  organizerId: "m2",
  organizerName: "Comfort Addo",
};

export const page = (data: unknown[]) =>
  json({ data, hasMore: false, next: null });

// Adding, changing and searching: what the Members tab's menus and the Add member sheet talk to.
function changes(path: string, method?: string) {
  if (method === "PUT") return json(people[1]);
  if (method === "POST") return json(people[0]);
  if (path === "/v1/members/search")
    return json([
      { id: "m9", name: "Kojo Antwi", phoneNumber: "020 551 7733" },
    ]);
  return null;
}

export function server(status = 200) {
  return async (url: string, init?: RequestInit) => {
    if (init?.method === "DELETE") return new Response(null, { status: 204 });
    const u = new URL(url, "http://x");
    const path = u.pathname.replace("/api", "");
    const change = changes(path, init?.method);
    if (change) return change;
    if (path === "/v1/organizations/o1")
      return status === 200
        ? json(choir)
        : json({ title: "Not found" }, status);
    if (path === "/v1/organization-members/organization/o1") {
      const role = u.searchParams.get("role");
      const name = u.searchParams.get("name")?.toLowerCase();
      return page(
        people.filter(
          (p) =>
            (!role || p.role === role) &&
            (!name || p.memberName.toLowerCase().includes(name)),
        ),
      );
    }
    if (path === "/v1/events") return page([event]);
    return page([]);
  };
}

export function signedIn() {
  saveTokens({
    accessToken: "a",
    accessTokenExpiresAt: "2099-01-01T00:00:00Z",
    refreshToken: "r",
    refreshTokenExpiresAt: "2099-01-01T00:00:00Z",
  });
}

export function show(
  handler: (url: string, init?: RequestInit) => Promise<Response>,
) {
  const fetchMock = vi.fn(handler);
  vi.stubGlobal("fetch", fetchMock);
  render(
    <QueryClientProvider
      client={
        new QueryClient({ defaultOptions: { queries: { retry: false } } })
      }
    >
      <MemoryRouter initialEntries={["/organizations/o1"]}>
        <Routes>
          <Route path="/organizations/:id" element={<OrganizationPage />} />
          <Route path="/organizations" element={<p>The list</p>} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
  return fetchMock;
}

// Opens the page and then its Members tab, which is where the people are.
export async function showMembers(
  handler: (url: string, init?: RequestInit) => Promise<Response>,
) {
  const fetchMock = show(handler);
  await userEvent.click(await screen.findByRole("tab", { name: /Members/ }));
  return fetchMock;
}
