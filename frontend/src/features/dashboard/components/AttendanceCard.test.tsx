import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { saveTokens } from "@/lib/tokens";
import { AttendanceCard } from "./AttendanceCard";
import { MemoryRouter } from "react-router-dom";

const json = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), { status });

// People present in each of the last eight weeks, by the day the week began. Any other day answers 500.
const weeks: Record<string, number> = {
  "2026-08-23": 800,
  "2026-08-30": 820,
  "2026-09-06": 810,
  "2026-09-13": 840,
  "2026-09-20": 830,
  "2026-09-27": 930,
  "2026-10-04": 860,
  "2026-10-11": 912,
};

let broken: boolean;
beforeEach(() => {
  broken = false;
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
      if (broken || u.pathname !== "/v1/attendance/summary")
        return json({ title: "Server error", status: 500 }, 500);
      return json({
        totalPresent: weeks[u.searchParams.get("from") ?? ""] ?? 500,
      });
    }),
  );
});
afterEach(() => vi.unstubAllGlobals());

const show = () =>
  render(
    <MemoryRouter>
      <QueryClientProvider
        client={
          new QueryClient({ defaultOptions: { queries: { retry: false } } })
        }
      >
        <AttendanceCard today="2026-10-19" />
      </QueryClientProvider>
    </MemoryRouter>,
  );

const summary = () => within(screen.getByRole("group", { name: "Summary" }));

describe("attendance card", () => {
  it("shows the last week, the average and the best week", async () => {
    show();
    expect(await summary().findByText("912")).toBeInTheDocument();
    expect(summary().getByText("850")).toBeInTheDocument();
    expect(summary().getByText("930")).toBeInTheDocument();
    expect(summary().getByText("Average per week")).toBeInTheDocument();
    expect(
      screen.getByRole("img", { name: "Week of 11 Oct: 912" }),
    ).toBeInTheDocument();
  });

  it("counts months when asked for six months", async () => {
    show();
    await userEvent.click(screen.getByRole("radio", { name: "6 months" }));
    expect(await summary().findByText("Average per month")).toBeInTheDocument();
    expect(
      await screen.findByRole("img", { name: "April 2026: 500" }),
    ).toBeInTheDocument();
  });

  it("says so when it could not load, and offers another try", async () => {
    broken = true;
    show();
    expect(await screen.findByText("Could not load")).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: "Try again" }),
    ).toBeInTheDocument();
  });
});
