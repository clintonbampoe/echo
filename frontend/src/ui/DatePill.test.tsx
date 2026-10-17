import { useState } from "react";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { DatePill } from "./DatePill";

function Harness({ start = "" }: { start?: string }) {
  const [from, setFrom] = useState(start);
  return (
    <DatePill
      label="From"
      value={from}
      range={{ from, to: "2026-10-17" }}
      onChange={setFrom}
    />
  );
}

describe("DatePill", () => {
  it("shows the day, picks another and clears it", async () => {
    const user = userEvent.setup();
    render(<Harness start="2026-10-05" />);
    expect(
      screen.getByRole("button", { name: "From: 05 Oct 2026" }),
    ).toHaveTextContent("From 05 Oct 2026");
    await user.click(screen.getByRole("button", { name: "From: 05 Oct 2026" }));
    await user.click(
      screen.getByRole("button", { name: "Tuesday 6 October 2026" }),
    );
    expect(
      screen.getByRole("button", { name: "From: 06 Oct 2026" }),
    ).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "From: 06 Oct 2026" }));
    await user.click(screen.getByRole("button", { name: "Clear" }));
    expect(
      screen.getByRole("button", { name: "From: any day" }),
    ).toHaveTextContent("From");
  });

  it("offers no Clear while no day is chosen", async () => {
    const user = userEvent.setup();
    render(<Harness />);
    await user.click(screen.getByRole("button", { name: "From: any day" }));
    expect(screen.queryByRole("button", { name: "Clear" })).toBeNull();
  });
});
