import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { saveTokens } from "@/lib/tokens";
import type { Tithe } from "../api";
import { MemberTithes } from "./MemberTithes";

const json = (body: unknown) =>
  new Response(JSON.stringify(body), { status: 200 });
const row = (
  id: string,
  amount: number,
  forMonth: string,
  paymentMethod: Tithe["paymentMethod"],
): Tithe => ({
  id,
  memberId: "m1",
  memberName: "Kwame Asante",
  amount,
  forYear: 2026,
  forMonth,
  paymentMethod,
  collectionDate: "2026-10-04",
  createdAt: "",
});

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
    if (u.pathname.endsWith("/tithes/summary/monthly"))
      return json([
        { year: 2026, month: 10, totalCollected: 800, uniqueTithers: 1 },
      ]);
    if (u.pathname.endsWith("/tithes/summary"))
      return json({
        totalCollected: u.searchParams.get("paymentMethod") ? 800 : 1600,
        uniqueTithers: 1,
        averagePerMember: 0,
      });
    const cash = u.searchParams.get("paymentMethod") === "Cash";
    return json({
      data: cash
        ? [row("t1", 800, "October", "Cash")]
        : [
            row("t1", 800, "October", "Cash"),
            row("t2", 800, "September", "MobileMoney"),
          ],
      hasMore: false,
      next: null,
    });
  });
  vi.stubGlobal("fetch", fetchMock);
});
afterEach(() => vi.unstubAllGlobals());

const open = () =>
  render(
    <QueryClientProvider
      client={
        new QueryClient({ defaultOptions: { queries: { retry: false } } })
      }
    >
      <MemoryRouter>
        <MemberTithes memberId="m1" today="2026-10-14" />
      </MemoryRouter>
    </QueryClientProvider>,
  );

describe("a member's tithes", () => {
  it("lists every tithe with the method in words and the total under them", async () => {
    open();
    expect(await screen.findByText("September 2026")).toBeInTheDocument();
    expect(screen.getByText("Mobile money")).toBeInTheDocument();
    expect(screen.getByRole("status")).toHaveTextContent("2 tithes recorded");
    expect(await screen.findByText("GH₵ 2k")).toBeInTheDocument();
    expect(
      screen.getByRole("region", { name: "Tithes this year" }),
    ).toBeInTheDocument();
  });
});
