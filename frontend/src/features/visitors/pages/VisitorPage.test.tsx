import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { saveTokens } from "@/lib/tokens";
import type { Visitor } from "../api/visitors";
import { VisitorPage } from "./VisitorPage";
import { aVisitor } from "../testing/fixtures";

const json = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), { status });

const visitor = aVisitor({
  id: "v1",
  name: "Esi Mensah",
  firstName: "Esi",
  lastName: "Mensah",
  phoneNumber: "024 555 0101",
  emailAddress: "esi@example.com",
  dateOfBirth: "1995-05-02",
  notes: "Came with her sister.",
  createdAt: "2026-09-07T10:00:00Z",
  visitCount: 3,
  lastVisitedAt: "2026-01-05T09:00:00Z",
});
// The server sends the converted date but not always the link to the member, so the page must work with the date alone.
const joined = aVisitor({ ...visitor, convertedAt: "2026-10-12T09:00:00Z" });

const empty = { data: [], hasMore: false, next: null };
const visit = {
  id: "a1",
  sessionId: "s1",
  sessionDate: "2026-10-11",
  sessionName: "Sunday Service",
  sessionLabelName: "Sunday Service",
  checkedInAt: "2026-10-11T09:05:00Z",
};

function server(v: Visitor, status = 200) {
  return async (url: string, init?: RequestInit) => {
    if (init?.method === "DELETE") return new Response(null, { status: 204 });
    const path = new URL(url, "http://x").pathname.replace("/api", "");
    if (path === "/v1/visitors/v1")
      return status === 200 ? json(v) : json({ title: "Not found" }, status);
    if (path === "/v1/attendance")
      return json({ data: [visit], hasMore: false, next: null });
    return json(empty);
  };
}

beforeEach(() => {
  saveTokens({
    accessToken: "a",
    accessTokenExpiresAt: "2099-01-01T00:00:00Z",
    refreshToken: "r",
    refreshTokenExpiresAt: "2099-01-01T00:00:00Z",
  });
});
afterEach(() => vi.unstubAllGlobals());

function show(handler: (url: string, init?: RequestInit) => Promise<Response>) {
  vi.stubGlobal("fetch", vi.fn(handler));
  render(
    <QueryClientProvider
      client={
        new QueryClient({ defaultOptions: { queries: { retry: false } } })
      }
    >
      <MemoryRouter initialEntries={["/visitors/v1"]}>
        <Routes>
          <Route path="/visitors/:id" element={<VisitorPage />} />
          <Route path="/visitors" element={<p>The list</p>} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe("VisitorPage", () => {
  it("shows the visitor, their numbers and their visits", async () => {
    show(server(visitor));
    expect(
      await screen.findByRole("heading", { name: "Esi Mensah" }),
    ).toBeInTheDocument();
    const numbers = screen.getByRole("region", { name: "Visitor numbers" });
    expect(within(numbers).getByText("As a visitor")).toBeInTheDocument();
    expect(within(numbers).getByText("Time to follow up")).toBeInTheDocument();
    expect(screen.getByText("Came with her sister.")).toBeInTheDocument();
    await userEvent.click(screen.getByRole("tab", { name: "Visits" }));
    expect(
      await screen.findByRole("link", { name: /Sunday Service/ }),
    ).toHaveAttribute("href", "/attendance/sessions/s1");
  });

  it("opens the make member form from the page", async () => {
    show(server(visitor));
    await userEvent.click(
      await screen.findByRole("button", { name: "Make a member" }),
    );
    expect(
      await screen.findByText("Make Esi Mensah a member"),
    ).toBeInTheDocument();
  });

  it("deletes the visitor and goes back to the list", async () => {
    show(server(visitor));
    await userEvent.click(
      await screen.findByRole("button", {
        name: "More actions for Esi Mensah",
      }),
    );
    await userEvent.click(screen.getByRole("menuitem", { name: "Delete" }));
    await userEvent.click(
      await screen.findByRole("button", { name: "Delete visitor" }),
    );
    expect(await screen.findByText("The list")).toBeInTheDocument();
  });

  it("links a visitor who joined to their member record and hides edit and delete", async () => {
    show(server(joined));
    expect(
      await screen.findByText(/Esi became a member on 12 Oct 2026/),
    ).toBeInTheDocument();
    for (const link of screen.getAllByRole("link", { name: "View member" }))
      expect(link).toHaveAttribute("href", "/members/v1");
    expect(
      screen.queryByRole("button", { name: "Edit" }),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByRole("button", { name: "Delete" }),
    ).not.toBeInTheDocument();
  });

  it("says so when the visitor is gone", async () => {
    show(server(visitor, 404));
    expect(
      await screen.findByText("We could not find this visitor"),
    ).toBeInTheDocument();
  });
});
