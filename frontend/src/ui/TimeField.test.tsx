import { useState } from "react";
import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { pickTime } from "@/test/pickers";
import { TimeField } from "./TimeField";

function Harness({
  start = "",
  ...rest
}: { start?: string } & Partial<Parameters<typeof TimeField>[0]>) {
  const [value, setValue] = useState(start);
  return (
    <>
      <TimeField
        label="Starts at"
        {...rest}
        value={value}
        onChange={setValue}
      />
      <output>{value || "none"}</output>
    </>
  );
}

afterEach(() => {
  vi.useRealTimers();
  vi.unstubAllGlobals();
});

const field = () => screen.getByRole("button", { name: /^Starts at/ });

describe("TimeField with a mouse", () => {
  it("says what to do while empty and shows the time once picked", () => {
    const { unmount } = render(<Harness />);
    expect(field()).toHaveTextContent("Choose a time");
    unmount();
    render(<Harness start="14:05" />);
    expect(field()).toHaveTextContent("2:05 pm");
  });

  it("opens on the time it has and keeps the new one only after Done", async () => {
    const user = userEvent.setup();
    render(<Harness start="08:00" />);
    await user.click(field());
    const dialog = screen.getByRole("dialog", { name: "Starts at" });
    expect(within(dialog).getByRole("option", { name: "8" })).toHaveAttribute(
      "aria-selected",
      "true",
    );
    expect(within(dialog).getByRole("option", { name: "am" })).toHaveAttribute(
      "aria-selected",
      "true",
    );
    await user.click(within(dialog).getByRole("option", { name: "45" }));
    expect(screen.getByRole("status")).toHaveTextContent("08:00");
    await user.click(within(dialog).getByRole("button", { name: "Done" }));
    expect(screen.getByRole("status")).toHaveTextContent("08:45");
    expect(field()).toHaveFocus();
  });

  it("rolls the hour, minute and am or pm", async () => {
    const user = userEvent.setup();
    render(<Harness />);
    await pickTime(user, "Starts at", "18:40");
    expect(screen.getByRole("status")).toHaveTextContent("18:40");
    expect(field()).toHaveTextContent("6:40 pm");
  });

  it("starts at 9 am when empty, and Now sets the time on the clock", async () => {
    vi.useFakeTimers({ toFake: ["Date"] });
    vi.setSystemTime(new Date(2026, 9, 17, 7, 4));
    const user = userEvent.setup();
    render(<Harness />);
    await user.click(field());
    expect(screen.getByRole("option", { name: "9" })).toHaveAttribute(
      "aria-selected",
      "true",
    );
    await user.click(screen.getByRole("button", { name: "Now" }));
    await user.click(screen.getByRole("button", { name: "Done" }));
    expect(screen.getByRole("status")).toHaveTextContent("07:04");
  });

  it("clears only when the field is optional", async () => {
    const user = userEvent.setup();
    const { unmount } = render(<Harness start="09:00" />);
    await user.click(field());
    expect(screen.queryByRole("button", { name: "Clear" })).toBeNull();
    unmount();
    render(<Harness start="09:00" optional />);
    await user.click(field());
    await user.click(screen.getByRole("button", { name: "Clear" }));
    expect(screen.getByRole("status")).toHaveTextContent("none");
  });

  it("closes on Escape and leaves the time alone", async () => {
    const user = userEvent.setup();
    render(<Harness start="09:00" />);
    await user.click(field());
    await user.keyboard("{Escape}");
    expect(screen.queryByRole("dialog")).toBeNull();
    expect(screen.getByRole("status")).toHaveTextContent("09:00");
  });
});

describe("TimeField on a touch screen", () => {
  beforeEach(() =>
    vi.stubGlobal("matchMedia", () => ({
      matches: false,
      addEventListener() {},
      removeEventListener() {},
    })),
  );

  it("opens as a sheet with Cancel and Done", async () => {
    const user = userEvent.setup();
    render(<Harness start="09:00" />);
    await user.click(field());
    const sheet = screen.getByRole("dialog", { name: "Starts at" });
    await user.click(within(sheet).getByRole("option", { name: "30" }));
    await user.click(within(sheet).getByRole("button", { name: "Cancel" }));
    expect(screen.queryByRole("dialog")).toBeNull();
    expect(screen.getByRole("status")).toHaveTextContent("09:00");
    await pickTime(user, "Starts at", "09:30");
    expect(screen.getByRole("status")).toHaveTextContent("09:30");
  });
});
