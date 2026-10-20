import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { PeriodPill } from "./PeriodPill";

const show = (onChange = vi.fn()) => {
  render(
    <PeriodPill value="last-week" today="2026-10-19" onChange={onChange} />,
  );
  return onChange;
};

const openMenu = () =>
  userEvent.click(screen.getByRole("button", { name: "Period: Last week" }));

describe("period pill", () => {
  it("lists the six periods, each with its dates, and marks the one in use", async () => {
    show();
    await openMenu();
    const options = within(
      screen.getByRole("listbox", { name: "Period" }),
    ).getAllByRole("option");
    expect(options).toHaveLength(6);
    expect(options[1]).toHaveTextContent("Last week");
    expect(options[1]).toHaveTextContent("11 – 17 Oct");
    expect(options[3]).toHaveTextContent("20 Jul – 19 Oct");
    expect(options[1]).toHaveAttribute("aria-selected", "true");
  });

  it("reports the pick and closes", async () => {
    const onChange = show();
    await openMenu();
    await userEvent.click(
      screen.getByRole("option", { name: /Last 3 months/ }),
    );
    expect(onChange).toHaveBeenCalledWith("last-3-months");
    expect(screen.queryByRole("listbox")).not.toBeInTheDocument();
  });

  it("moves between the options with the arrow keys", async () => {
    show();
    await openMenu();
    expect(screen.getByRole("option", { name: /Last week/ })).toHaveFocus();
    await userEvent.keyboard("{ArrowDown}");
    expect(screen.getByRole("option", { name: /This month/ })).toHaveFocus();
  });
});
