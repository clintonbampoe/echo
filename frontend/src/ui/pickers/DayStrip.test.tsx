import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { DayStrip, type StripDay } from "./DayStrip";

const days: StripDay[] = [
  { day: "2026-10-10", weekday: "Sat", date: 10, marks: 0, today: false },
  { day: "2026-10-11", weekday: "Sun", date: 11, marks: 2, today: false },
  { day: "2026-10-14", weekday: "Wed", date: 14, marks: 1, today: true },
];

function setup(selected = "2026-10-11") {
  const onSelect = vi.fn();
  const onShift = vi.fn();
  render(
    <DayStrip
      label="Days"
      days={days}
      selected={selected}
      onSelect={onSelect}
      onShift={onShift}
      noun={["session", "sessions"]}
    />,
  );
  return { onSelect, onShift };
}

describe("DayStrip", () => {
  it("says which day is picked, which is today and what is on each", () => {
    setup();
    expect(
      screen.getByRole("button", { name: "Sun 11, 2 sessions" }),
    ).toHaveAttribute("aria-pressed", "true");
    expect(
      screen.getByRole("button", { name: "Wed 14, today, 1 session" }),
    ).toHaveAttribute("aria-pressed", "false");
    expect(screen.getByRole("button", { name: "Sat 10" })).toBeInTheDocument();
  });

  it("tells the page which day was picked and which way the arrows go", async () => {
    const { onSelect, onShift } = setup();
    await userEvent.click(screen.getByRole("button", { name: "Sat 10" }));
    await userEvent.click(screen.getByRole("button", { name: "Next week" }));
    await userEvent.click(
      screen.getByRole("button", { name: "Previous week" }),
    );
    await waitFor(() => expect(onSelect).toHaveBeenCalledWith("2026-10-10"));
    expect(onShift.mock.calls).toEqual([[1], [-1]]);
  });

  it("moves one day for a sideways notch of the wheel, and tells the page", async () => {
    const { onSelect } = setup();
    fireEvent.wheel(screen.getByRole("list"), { deltaX: 100 });
    await waitFor(() => expect(onSelect).toHaveBeenCalledWith("2026-10-14"));
    expect(onSelect).toHaveBeenCalledTimes(1);
  });

  it("follows a trackpad swipe to the end, even when its tail turns more up than sideways", async () => {
    const { onSelect } = setup();
    const list = screen.getByRole("list");
    for (let i = 0; i < 3; i++)
      fireEvent.wheel(list, { deltaX: 30, deltaY: 2 });
    const tail = fireEvent.wheel(list, { deltaX: 3, deltaY: 6 });
    expect(tail).toBe(false); // false means the strip took the event and the page did not scroll
    await waitFor(() => expect(onSelect).toHaveBeenCalledWith("2026-10-14"));
  });

  it("leaves up and down scrolling to the page", () => {
    const { onSelect } = setup();
    const notCancelled = fireEvent.wheel(screen.getByRole("list"), {
      deltaY: 100,
    });
    expect(notCancelled).toBe(true);
    expect(onSelect).not.toHaveBeenCalled();
  });

  it("moves a day with the arrow keys, and only the picked day is a tab stop", async () => {
    const { onSelect } = setup();
    expect(
      screen.getByRole("button", { name: "Sun 11, 2 sessions" }),
    ).toHaveAttribute("tabindex", "0");
    expect(screen.getByRole("button", { name: "Sat 10" })).toHaveAttribute(
      "tabindex",
      "-1",
    );
    screen.getByRole("button", { name: "Sun 11, 2 sessions" }).focus();
    await userEvent.keyboard("{ArrowLeft}");
    await waitFor(() => expect(onSelect).toHaveBeenCalledWith("2026-10-10"));
  });
});
