import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { saveTokens } from "@/lib/tokens";
import type { VisitorSummary } from "../api/visitors";
import { VisitorsPage } from "./VisitorsPage";
import { aVisitor } from "../testing/fixtures";

const json = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), { status });

const esi = aVisitor({
  id: "v1",
  name: "Esi Mensah",
  firstName: "Esi",
  lastName: "Mensah",
  phoneNumber: "055 212 3344",
  emailAddress: "esi@example.com",
  createdAt: "2026-09-20T09:00:00Z",
  visitCount: 3,
  lastVisitedAt: "2026-10-11",
});
const kojo = aVisitor({
  id: "v2",
  name: "Kojo Antwi",
  firstName: "Kojo",
  lastName: "Antwi",
  createdAt: "2026-10-11T09:00:00Z",
  visitCount: 1,
  lastVisitedAt: "2026-10-11",
});
const yaa = aVisitor({
  id: "v3",
  name: "Yaa Pokua",
  firstName: "Yaa",
  lastName: "Pokua",
  createdAt: "2026-03-01T09:00:00Z",
  visitCount: 5,
  convertedToMemberPersonId: "v3",
  convertedToMemberName: "Yaa Pokua",
  convertedAt: "2026-10-12T10:00:00Z",
});
const summary: VisitorSummary = {
  totalVisitors: 214,
  newVisitors: 251,
  recurringVisitors: 46,
  convertedVisitors: 37,
};

let fetchMock: ReturnType<typeof vi.fn>;
beforeEach(() => {
  saveTokens({
    accessToken: "a",
    accessTokenExpiresAt: "2099-01-01T00:00:00Z",
    refreshToken: "r",
    refreshTokenExpiresAt: "2099-01-01T00:00:00Z",
  });
  fetchMock = vi.fn(async (url: string) => {
    const u = new URL(url, "http://x");
    if (u.pathname.endsWith("/summary")) return json(summary);
    const became = u.searchParams.get("converted") === "true";
    return json({
      data: became ? [yaa] : [esi, kojo],
      hasMore: false,
      next: null,
    });
  });
  vi.stubGlobal("fetch", fetchMock);
});
afterEach(() => vi.unstubAllGlobals());

const open = (address = "/visitors") =>
  render(
    <QueryClientProvider
      client={
        new QueryClient({ defaultOptions: { queries: { retry: false } } })
      }
    >
      <MemoryRouter initialEntries={[address]}>
        <VisitorsPage />
      </MemoryRouter>
    </QueryClientProvider>,
  );

describe("visitors page", () => {
  it("shows the numbers and the visitors who are not yet members", async () => {
    open();
    expect(
      await screen.findByRole("link", { name: "Esi Mensah" }),
    ).toHaveAttribute("href", "/visitors/v1");
    expect(screen.getByText("Returning")).toBeInTheDocument();
    expect(await screen.findByText("251")).toBeInTheDocument();
  });

  it("shows those who became members, and opens their visitor page", async () => {
    open("/visitors?status=became");
    expect(
      await screen.findByRole("link", { name: "Yaa Pokua" }),
    ).toHaveAttribute("href", "/visitors/v3");
    expect(screen.getByText("Joined")).toBeInTheDocument();
    expect(screen.queryByText("Last visit")).toBeNull();
    const asked = fetchMock.mock.calls.map(
      (c) => new URL((c as [string])[0]).searchParams,
    );
    expect(asked.some((p) => p.get("converted") === "true")).toBe(true);
  });
});
