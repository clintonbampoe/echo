import { useState } from "react";
import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { DateField } from "./DateField";

function Harness({
  start = "",
  ...rest
}: { start?: string } & Partial<Parameters<typeof DateField>[0]>) {
  const [value, setValue] = useState(start);
  return (
    <>
      <DateField label="Date" {...rest} value={value} onChange={setValue} />
      <output>{value || "none"}</output>
    </>
  );
}

// Only the clock is faked, so the user events keep their timers.
beforeEach(() => {
  vi.useFakeTimers({ toFake: ["Date"] });
  vi.setSystemTime(new Date("2026-10-14T10:00:00Z"));
});
afterEach(() => {
  vi.useRealTimers();
  vi.unstubAllGlobals();
});

const field = () => screen.getByRole("button", { name: /^Date/ });

describe("DateField with a mouse", () => {
  it("says what to do while empty and shows the day once picked", () => {
    const { unmount } = render(<Harness />);
    expect(field()).toHaveTextContent("Choose a date");
    unmount();
    render(<Harness start="2026-10-11" />);
    expect(field()).toHaveTextContent("11 Oct 2026");
  });

  it("opens on the picked day, picks another and closes with focus back on the field", async () => {
    const user = userEvent.setup();
    render(<Harness start="2026-10-11" />);
    await user.click(field());
    const dialog = screen.getByRole("dialog", { name: "Date" });
    expect(
      within(dialog).getByRole("button", { name: "Sunday 11 October 2026" }),
    ).toHaveAttribute("aria-pressed", "true");
    await user.click(
      within(dialog).getByRole("button", { name: "Monday 12 October 2026" }),
    );
    expect(screen.queryByRole("dialog")).toBeNull();
    expect(screen.getByRole("status")).toHaveTextContent("2026-10-12");
    expect(field()).toHaveTextContent("12 Oct 2026");
    expect(field()).toHaveFocus();
  });

  it("marks today and moves between months with the arrows", async () => {
    const user = userEvent.setup();
    render(<Harness start="2026-10-11" />);
    await user.click(field());
    expect(
      screen.getByRole("button", { name: "Wednesday 14 October 2026" }),
    ).toHaveAttribute("aria-current", "date");
    await user.click(screen.getByRole("button", { name: "Next month" }));
    expect(
      screen.getByRole("button", { name: "Sunday 1 November 2026" }),
    ).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Previous month" }));
    await user.click(screen.getByRole("button", { name: "Previous month" }));
    expect(
      screen.getByRole("button", { name: "Tuesday 1 September 2026" }),
    ).toBeInTheDocument();
  });

  it("cannot pick a day outside the limits, nor go past them", async () => {
    const user = userEvent.setup();
    render(<Harness start="2026-10-11" min="2026-10-05" max="2026-10-20" />);
    await user.click(field());
    expect(
      screen.getByRole("button", { name: "Wednesday 21 October 2026" }),
    ).toBeDisabled();
    expect(
      screen.getByRole("button", { name: "Saturday 3 October 2026" }),
    ).toBeDisabled();
    expect(
      screen.getByRole("button", { name: "Friday 9 October 2026" }),
    ).toBeEnabled();
    expect(screen.getByRole("button", { name: "Next month" })).toBeDisabled();
    expect(
      screen.getByRole("button", { name: "Previous month" }),
    ).toBeDisabled();
  });

  it("works from the keyboard: arrows move a day, Page Down a month, Enter picks", async () => {
    const user = userEvent.setup();
    render(<Harness start="2026-10-11" />);
    await user.click(field());
    expect(
      screen.getByRole("button", { name: "Sunday 11 October 2026" }),
    ).toHaveFocus();
    await user.keyboard("{ArrowRight}");
    expect(
      screen.getByRole("button", { name: "Monday 12 October 2026" }),
    ).toHaveFocus();
    await user.keyboard("{ArrowDown}");
    expect(
      screen.getByRole("button", { name: "Monday 19 October 2026" }),
    ).toHaveFocus();
    await user.keyboard("{PageDown}");
    expect(
      screen.getByRole("button", { name: "Thursday 19 November 2026" }),
    ).toHaveFocus();
    await user.keyboard("{Enter}");
    expect(screen.getByRole("status")).toHaveTextContent("2026-11-19");
  });

  it("closes on Escape and leaves the day alone", async () => {
    const user = userEvent.setup();
    render(<Harness start="2026-10-11" />);
    await user.click(field());
    await user.keyboard("{Escape}");
    expect(screen.queryByRole("dialog")).toBeNull();
    expect(screen.getByRole("status")).toHaveTextContent("2026-10-11");
  });

  it("jumps to today, and clears only when the field is optional", async () => {
    const user = userEvent.setup();
    const { unmount } = render(<Harness start="2026-03-01" />);
    await user.click(field());
    expect(screen.queryByRole("button", { name: "Clear" })).toBeNull();
    await user.click(screen.getByRole("button", { name: "Today" }));
    expect(screen.getByRole("status")).toHaveTextContent("2026-10-14");
    unmount();
    render(<Harness start="2026-03-01" optional />);
    await user.click(field());
    await user.click(screen.getByRole("button", { name: "Clear" }));
    expect(screen.getByRole("status")).toHaveTextContent("none");
  });

  it("rolls to another month and year with wheels", async () => {
    const user = userEvent.setup();
    render(<Harness start="2026-10-11" />);
    await user.click(field());
    await user.click(screen.getByRole("button", { name: "October 2026" }));
    await user.click(screen.getByRole("option", { name: "2024" }));
    await user.click(screen.getByRole("option", { name: "Mar" }));
    await user.click(screen.getByRole("button", { name: "Done" }));
    expect(
      screen.getByRole("button", { name: "March 2024" }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: "Friday 1 March 2024" }),
    ).toBeInTheDocument();
  });

  it("shows the error under the field", () => {
    render(<Harness error="Enter a date" />);
    expect(field()).toHaveAttribute("aria-invalid", "true");
    expect(screen.getByRole("alert")).toHaveTextContent("Enter a date");
  });
});

describe("DateField for a birthday on a touch screen", () => {
  beforeEach(() =>
    vi.stubGlobal("matchMedia", () => ({
      matches: false,
      addEventListener() {},
      removeEventListener() {},
    })),
  );

  it("opens day, month and year wheels starting 30 years back", async () => {
    const user = userEvent.setup();
    render(<Harness label="Date of birth" kind="birth" />);
    await user.click(screen.getByRole("button", { name: /^Date of birth/ }));
    const sheet = screen.getByRole("dialog", { name: "Date of birth" });
    expect(within(sheet).getByRole("option", { name: "1996" })).toHaveAttribute(
      "aria-selected",
      "true",
    );
    await user.click(within(sheet).getByRole("option", { name: "1988" }));
    await user.click(within(sheet).getByRole("option", { name: "Feb" }));
    await user.click(within(sheet).getByRole("option", { name: "14" }));
    await user.click(within(sheet).getByRole("button", { name: "Done" }));
    expect(screen.getByRole("status")).toHaveTextContent("1988-02-14");
  });

  it("takes the day back to the last of a shorter month, and Cancel keeps the old day", async () => {
    const user = userEvent.setup();
    render(<Harness label="Date of birth" kind="birth" start="1990-03-31" />);
    await user.click(screen.getByRole("button", { name: /^Date of birth/ }));
    await user.click(screen.getByRole("option", { name: "Feb" }));
    expect(screen.getByRole("option", { name: "28" })).toHaveAttribute(
      "aria-selected",
      "true",
    );
    await user.click(screen.getByRole("button", { name: "Cancel" }));
    expect(screen.getByRole("status")).toHaveTextContent("1990-03-31");
  });

  it("uses the calendar on a touch screen for a date that is not a birthday", async () => {
    const user = userEvent.setup();
    render(<Harness start="2026-10-11" />);
    await user.click(field());
    await user.click(
      screen.getByRole("button", { name: "Monday 12 October 2026" }),
    );
    expect(screen.getByRole("status")).toHaveTextContent("2026-10-12");
  });
});
