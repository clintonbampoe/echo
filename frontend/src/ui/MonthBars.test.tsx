import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MonthBars, type MonthBar } from "./MonthBars";

const bars: MonthBar[] = [
  {
    key: "2026-09",
    label: "Sep",
    name: "September 2026",
    value: 100,
    text: "GH₵ 100",
  },
  {
    key: "2026-10",
    label: "Oct",
    name: "October 2026",
    value: 50,
    text: "GH₵ 50",
  },
];

describe("MonthBars", () => {
  it("names every bar with its month and value", () => {
    render(<MonthBars label="Collected per month" bars={bars} />);
    expect(
      screen.getByRole("list", { name: "Collected per month" }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("img", { name: "September 2026: GH₵ 100" }),
    ).toBeInTheDocument();
  });

  it("makes the bars buttons when they can be picked, and says which is chosen", async () => {
    const onPick = vi.fn();
    render(
      <MonthBars
        label="Collected per month"
        bars={bars}
        selected="2026-10"
        onPick={onPick}
      />,
    );
    expect(
      screen.getByRole("button", { name: "October 2026: GH₵ 50" }),
    ).toHaveAttribute("aria-pressed", "true");
    await userEvent.click(
      screen.getByRole("button", { name: "September 2026: GH₵ 100" }),
    );
    expect(onPick).toHaveBeenCalledWith("2026-09");
  });
});
