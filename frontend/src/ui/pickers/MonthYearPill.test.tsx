import { render, screen, within } from "@testing-library/react";
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
  it("opens on the current month and year and picks another month", async () => {
    const user = userEvent.setup();
    render(<Harness />);
    await user.click(screen.getByRole("button", { name: "Tithe for" }));
    const dialog = screen.getByRole("dialog", { name: "Tithe for" });
    expect(within(dialog).getByRole("option", { name: "Oct" })).toHaveAttribute(
      "aria-selected",
      "true",
    );
    expect(
      within(dialog).getByRole("option", { name: "2026" }),
    ).toHaveAttribute("aria-selected", "true");
    await user.click(within(dialog).getByRole("option", { name: "Mar" }));
    await user.click(within(dialog).getByRole("button", { name: "Done" }));
    expect(
      screen.getByRole("button", { name: "Tithe for: March 2026" }),
    ).toBeInTheDocument();
    expect(screen.queryByRole("dialog")).toBeNull();
  });

  it("picks a whole year", async () => {
    const user = userEvent.setup();
    render(<Harness />);
    await user.click(screen.getByRole("button", { name: "Tithe for" }));
    await user.click(screen.getByRole("option", { name: "2025" }));
    await user.click(screen.getByRole("button", { name: "Whole year" }));
    expect(
      screen.getByRole("button", { name: "Tithe for: 2025" }),
    ).toBeInTheDocument();
  });

  it("opens on the chosen month and clears the choice", async () => {
    const user = userEvent.setup();
    render(<Harness start="2026-03" />);
    await user.click(
      screen.getByRole("button", { name: "Tithe for: March 2026" }),
    );
    expect(screen.getByRole("option", { name: "Mar" })).toHaveAttribute(
      "aria-selected",
      "true",
    );
    await user.click(screen.getByRole("button", { name: "Clear" }));
    expect(
      screen.getByRole("button", { name: "Tithe for" }),
    ).toBeInTheDocument();
  });

  it("closes on Escape without changing anything", async () => {
    const user = userEvent.setup();
    render(<Harness start="2026-03" />);
    await user.click(
      screen.getByRole("button", { name: "Tithe for: March 2026" }),
    );
    await user.keyboard("{Escape}");
    expect(screen.queryByRole("dialog")).toBeNull();
    expect(
      screen.getByRole("button", { name: "Tithe for: March 2026" }),
    ).toHaveFocus();
  });
});
