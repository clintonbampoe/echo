import { screen } from "@testing-library/react";
import { json, renderCard, signIn, asked } from "../../testing/dashboardPage";
import { NumberCards } from "./NumberCards";
type Answers = Record<string, Record<string, unknown>>;

// What the server says, by path and by the days asked for ("from&to"). "Last week" is 11 to 17 October
// and the stretch before it starts on 4 October; the church total runs from 2000 to 2100, so it is told apart.
const key = (from: string, to: string) => `${from}&${to}`;
const fresh = (): Answers => ({
  "/v1/members/summary": {
    [key("2000-01-01", "2100-12-31")]: { totalMembers: 1284, ageGroups: [] },
    [key("2026-10-11", "2026-10-17")]: {
      joinedInWindow: 9,
      joinedInPreviousWindow: 7,
    },
  },
  // The stretch before comes in the same answer, so the strip asks once for each.
  "/v1/attendance/summary": {
    [key("2026-10-11", "2026-10-17")]: {
      totalPresent: 912,
      previousTotalPresent: 828,
    },
  },
  "/v1/tithes/summary": {
    [key("2026-10-11", "2026-10-17")]: {
      totalCollected: 11300,
      previousTotalCollected: 10640,
    },
  },
  // The stretch before comes in the same answer.
  "/v1/transactions/summary": {
    [key("2026-10-11", "2026-10-17")]: {
      net: 5120,
      previousTotalIncome: 9000,
      previousTotalExpenses: 3700,
    },
  },
});

let answers: Answers;
beforeEach(() => {
  answers = fresh();
  signIn();
  vi.stubGlobal(
    "fetch",
    vi.fn(async (url: string) => {
      const { path, query } = asked(url);
      const body =
        answers[path]?.[key(query.get("from") ?? "", query.get("to") ?? "")];
      return body === undefined
        ? json({ title: "Server error", status: 500 }, 500)
        : json(body);
    }),
  );
});
afterEach(() => vi.unstubAllGlobals());

const show = () =>
  renderCard(<NumberCards period="last-week" today="2026-10-19" />);

describe("number cards", () => {
  it("shows the four numbers in one strip, each against the week before", async () => {
    show();
    expect(await screen.findByText("1.28k")).toBeInTheDocument();
    expect(screen.getByText("9 new")).toBeInTheDocument();
    expect(screen.getByText("912")).toBeInTheDocument();
    expect(screen.getByText("10%")).toBeInTheDocument();
    expect(screen.getByText("11.3k")).toBeInTheDocument();
    expect(screen.getByText("6%")).toBeInTheDocument();
    expect(screen.getByText("5.12k")).toBeInTheDocument();
    expect(screen.getByText("3%")).toBeInTheDocument();
  });

  it("says so on the one that could not load, and the others carry on", async () => {
    answers["/v1/attendance/summary"] = {};
    show();
    expect(await screen.findByText("11.3k")).toBeInTheDocument();
    expect(screen.getByText("Could not load")).toBeInTheDocument();
  });

  it("shows no change when the stretch before had nothing to compare with", async () => {
    answers["/v1/tithes/summary"] = {
      [key("2026-10-11", "2026-10-17")]: { totalCollected: 11300 },
      [key("2026-10-04", "2026-10-10")]: { totalCollected: 0 },
    };
    show();
    expect(await screen.findByText("11.3k")).toBeInTheDocument();
    expect(screen.getByText("3%")).toBeInTheDocument();
    expect(screen.queryByText("6%")).not.toBeInTheDocument();
  });
});
