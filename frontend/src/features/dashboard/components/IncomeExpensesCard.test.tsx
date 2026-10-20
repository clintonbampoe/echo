import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { json, renderCard, signIn } from "../dashboardTestKit";
import { IncomeExpensesCard } from "./IncomeExpensesCard";

const income = [5500, 6100, 5800, 7000, 6600, 8400];
const expenses = [3100, 3500, 3300, 4000, 3800, 5200];

let broken: boolean;
beforeEach(() => {
  broken = false;
  signIn();
  vi.stubGlobal(
    "fetch",
    vi.fn(async (url: string) => {
      const u = new URL(url, "http://x");
      if (broken || u.pathname !== "/v1/transactions/summary/monthly")
        return json({ title: "Server error", status: 500 }, 500);
      return json(
        income.map((totalIncome, i) => ({
          year: 2026,
          month: 5 + i,
          totalIncome,
          totalExpenses: expenses[i],
        })),
      );
    }),
  );
});
afterEach(() => vi.unstubAllGlobals());

const show = () =>
  renderCard(
    <IncomeExpensesCard today="2026-10-19" months={6} control={null} />,
  );

const month = () =>
  within(screen.getByRole("group", { name: "Selected month" }));

describe("income and expenses card", () => {
  it("shows this month's figures and a pair of bars for each month", async () => {
    show();
    expect(await month().findByText("GH₵ 8,400")).toBeInTheDocument();
    expect(month().getByText("GH₵ 5,200")).toBeInTheDocument();
    expect(screen.getAllByRole("button", { name: /Income GH₵/ })).toHaveLength(
      6,
    );
  });

  it("shows another month's figures when its bar is picked", async () => {
    show();
    await month().findByText("GH₵ 8,400");
    await userEvent.click(screen.getByRole("button", { name: /May 2026/ }));
    expect(await month().findByText("GH₵ 5,500")).toBeInTheDocument();
    expect(month().getByText("GH₵ 3,100")).toBeInTheDocument();
  });

  it("says so when it could not load", async () => {
    broken = true;
    show();
    expect(await screen.findByText("Could not load")).toBeInTheDocument();
  });
});
