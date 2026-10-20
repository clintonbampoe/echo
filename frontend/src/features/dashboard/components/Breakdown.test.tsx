import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Breakdown, type BreakdownSlice } from "./Breakdown";

const slices: BreakdownSlice[] = [
  {
    key: "p1",
    name: "Borehole",
    value: 12000,
    text: "GH₵ 12,000",
    percent: 75,
    color: "#1F2A3A",
  },
  {
    key: "p2",
    name: "Church roof",
    value: 4000,
    text: "GH₵ 4,000",
    percent: 25,
    color: "#4F6580",
  },
];

const draw = () =>
  render(
    <Breakdown
      label="Raised by project"
      total="GH₵ 16,000"
      totalLabel="Total raised"
      note="Total raised, all time"
      slices={slices}
      ready
      empty="Nothing raised yet"
    />,
  );

describe("breakdown", () => {
  it("reads a slice on hover and lets a click pick it", async () => {
    const user = userEvent.setup();
    draw();
    const row = screen.getByRole("button", { name: /Borehole/ });
    expect(screen.getByText("Total raised, all time")).toBeInTheDocument();

    await user.hover(row);
    expect(screen.getByText("Borehole · GH₵ 12,000")).toBeInTheDocument();

    await user.click(row);
    expect(screen.getByText("75% of total raised")).toBeInTheDocument();
    expect(row).toHaveAttribute("aria-pressed", "true");

    await user.click(row);
    expect(row).toHaveAttribute("aria-pressed", "false");
    expect(screen.queryByText("75% of total raised")).not.toBeInTheDocument();
  });
});
