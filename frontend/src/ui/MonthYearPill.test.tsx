import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { useState } from "react";
import { MonthYearPill } from "./MonthYearPill";

function Harness({ start = "" }: { start?: string }) {
  const [value, setValue] = useState(start);
  return (
    <MonthYearPill
      label="Tithe for"
      value={value}
      onChange={setValue}
      today="2026-10-14"
    />
  );
}

describe("MonthYearPill", () => {
  it("opens on the current year and picks a month", async () => {
    const user = userEvent.setup();
    render(<Harness />);
    await user.click(screen.getByRole("button", { name: "Tithe for" }));
    expect(screen.getByText("2026")).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "March 2026" }));
    expect(
      screen.getByRole("button", { name: "Tithe for: March 2026" }),
    ).toBeInTheDocument();
  });

  it("steps between years and picks a whole year", async () => {
    const user = userEvent.setup();
    render(<Harness />);
    await user.click(screen.getByRole("button", { name: "Tithe for" }));
    await user.click(
      screen.getByRole("button", { name: "Tithe for: previous year" }),
    );
    await user.click(screen.getByRole("button", { name: "Whole year" }));
    expect(
      screen.getByRole("button", { name: "Tithe for: 2025" }),
    ).toBeInTheDocument();
  });

  it("clears the choice", async () => {
    const user = userEvent.setup();
    render(<Harness start="2026-03" />);
    await user.click(
      screen.getByRole("button", { name: "Tithe for: March 2026" }),
    );
    await user.click(screen.getByRole("button", { name: "Clear" }));
    expect(
      screen.getByRole("button", { name: "Tithe for" }),
    ).toBeInTheDocument();
  });
});
