import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { RangePill } from "./RangePill";

const options = [
  { value: "weeks", label: "Last 8 weeks" },
  { value: "half", label: "Last 6 months" },
] as const;

describe("range pill", () => {
  it("shows the range in use, and reports the one picked", async () => {
    const onChange = vi.fn();
    render(
      <RangePill
        label="Range"
        value="weeks"
        options={options}
        onChange={onChange}
      />,
    );
    await userEvent.click(
      screen.getByRole("button", { name: "Range: Last 8 weeks" }),
    );
    expect(
      screen.getByRole("option", { name: "Last 8 weeks" }),
    ).toHaveAttribute("aria-selected", "true");
    await userEvent.click(
      screen.getByRole("option", { name: "Last 6 months" }),
    );
    expect(onChange).toHaveBeenCalledWith("half");
    expect(screen.queryByRole("listbox")).not.toBeInTheDocument();
  });
});
