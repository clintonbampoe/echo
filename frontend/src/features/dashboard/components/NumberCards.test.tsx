import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, within } from "@testing-library/react";
import { saveTokens } from "@/lib/tokens";
import { NumberCards } from "./NumberCards";

const json = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), { status });

type Answers = Record<string, Record<string, unknown>>;

// What the server says, by path and by the first day asked for. "Last week" is 11 to 17 October
// and the stretch before it starts on 4 October; the church total starts in 2000.
const fresh = (): Answers => ({
  "/v1/members/summary": {
    "2000-01-01": { totalMembers: 1284 },
    "2026-10-11": { totalMembers: 9 },
  },
  "/v1/attendance/summary": {
    "2026-10-11": { totalPresent: 912 },
    "2026-10-04": { totalPresent: 828 },
  },
  "/v1/tithes/summary": {
    "2026-10-11": { totalCollected: 11300 },
    "2026-10-04": { totalCollected: 10640 },
  },
  "/v1/transactions/summary": {
    "2026-10-11": { net: 5120 },
    "2026-10-04": { net: 5300 },
  },
});

let answers: Answers;
beforeEach(() => {
  answers = fresh();
  saveTokens({
    accessToken: "a",
    accessTokenExpiresAt: "2099-01-01T00:00:00Z",
    refreshToken: "r",
    refreshTokenExpiresAt: "2099-01-01T00:00:00Z",
  });
  vi.stubGlobal(
    "fetch",
    vi.fn(async (url: string) => {
      const u = new URL(url, "http://x");
      const body = answers[u.pathname]?.[u.searchParams.get("from") ?? ""];
      return body === undefined
        ? json({ title: "Server error", status: 500 }, 500)
        : json(body);
    }),
  );
});
afterEach(() => vi.unstubAllGlobals());

const show = () =>
  render(
    <QueryClientProvider
      client={
        new QueryClient({ defaultOptions: { queries: { retry: false } } })
      }
    >
      <NumberCards period="last-week" today="2026-10-19" />
    </QueryClientProvider>,
  );

const card = (name: string) => within(screen.getByRole("group", { name }));

describe("number cards", () => {
  it("shows the four numbers, each against the week before", async () => {
    show();
    expect(await card("Members").findByText("1,284")).toBeInTheDocument();
    expect(await card("Members").findByText("9 new")).toBeInTheDocument();
    expect(await card("Present").findByText("912")).toBeInTheDocument();
    expect(await card("Present").findByText("10.1%")).toBeInTheDocument();
    expect(await card("Tithes").findByText("GH₵ 11,300")).toBeInTheDocument();
    expect(await card("Tithes").findByText("6.2%")).toBeInTheDocument();
    expect(
      await card("Net income").findByText("GH₵ 5,120"),
    ).toBeInTheDocument();
    expect(await card("Net income").findByText("3.4%")).toBeInTheDocument();
  });

  it("says so on the one card that could not load, and the others carry on", async () => {
    answers["/v1/attendance/summary"] = {};
    show();
    expect(
      await card("Present").findByText("Could not load"),
    ).toBeInTheDocument();
    expect(
      card("Present").getByRole("button", { name: "Try again" }),
    ).toBeInTheDocument();
    expect(await card("Tithes").findByText("GH₵ 11,300")).toBeInTheDocument();
  });

  it("shows no change when the stretch before had nothing to compare with", async () => {
    answers["/v1/tithes/summary"] = {
      "2026-10-11": { totalCollected: 11300 },
      "2026-10-04": { totalCollected: 0 },
    };
    show();
    expect(await card("Net income").findByText("3.4%")).toBeInTheDocument();
    expect(await card("Tithes").findByText("GH₵ 11,300")).toBeInTheDocument();
    expect(card("Tithes").queryByText(/%/)).not.toBeInTheDocument();
  });
});
