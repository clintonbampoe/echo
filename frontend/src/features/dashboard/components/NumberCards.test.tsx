import { screen, within } from "@testing-library/react";
import { json, renderCard, signIn, asked } from "../dashboardTestKit";
import { NumberCards } from "./NumberCards";
type Answers = Record<string, Record<string, unknown>>;

// What the server says, by path and by the days asked for ("from&to"). "Last week" is 11 to 17 October
// and the stretch before it starts on 4 October; the church total runs from 2000 to today, while the
// members-with-no-joined-date ask for 2000 to 2000, so the two are told apart.
const key = (from: string, to: string) => `${from}&${to}`;
const fresh = (): Answers => ({
  "/v1/members/summary": {
    [key("2000-01-01", "2000-01-01")]: { totalMembers: 2 },
    [key("2000-01-01", "2026-10-19")]: { totalMembers: 1284 },
    [key("2026-10-11", "2026-10-17")]: { totalMembers: 11 },
  },
  "/v1/attendance/summary": {
    [key("2026-10-11", "2026-10-17")]: { totalPresent: 912 },
    [key("2026-10-04", "2026-10-10")]: { totalPresent: 828 },
  },
  "/v1/tithes/summary": {
    [key("2026-10-11", "2026-10-17")]: { totalCollected: 11300 },
    [key("2026-10-04", "2026-10-10")]: { totalCollected: 10640 },
  },
  "/v1/transactions/summary": {
    [key("2026-10-11", "2026-10-17")]: { net: 5120 },
    [key("2026-10-04", "2026-10-10")]: { net: 5300 },
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
        answers[path]?.[
          key(query.get("from") ?? "", query.get("to") ?? "")
        ];
      return body === undefined
        ? json({ title: "Server error", status: 500 }, 500)
        : json(body);
    }),
  );
});
afterEach(() => vi.unstubAllGlobals());

const show = () => renderCard(<NumberCards period="last-week" today="2026-10-19" />);

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
      [key("2026-10-11", "2026-10-17")]: { totalCollected: 11300 },
      [key("2026-10-04", "2026-10-10")]: { totalCollected: 0 },
    };
    show();
    expect(await card("Net income").findByText("3.4%")).toBeInTheDocument();
    expect(await card("Tithes").findByText("GH₵ 11,300")).toBeInTheDocument();
    expect(card("Tithes").queryByText(/%/)).not.toBeInTheDocument();
  });
});
