import { act, fireEvent, render, screen } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { useLongPressMenu } from "./useLongPressMenu";

function Rows({ onMenu, onOpen }: { onMenu: () => void; onOpen: () => void }) {
  useLongPressMenu();
  return (
    <ul>
      <li>
        <a href="#row" onClick={onOpen}>
          Abena Mensah
        </a>
        <button aria-haspopup="menu" aria-label="More" onClick={onMenu} />
      </li>
    </ul>
  );
}

const touch = (el: Element, x = 10, y = 10) => ({
  touches: [{ clientX: x, clientY: y }],
  target: el,
});

describe("long press on a row", () => {
  beforeEach(() => vi.useFakeTimers());
  afterEach(() => vi.useRealTimers());

  it("opens the row's ⋯ menu after half a second, and the release does not open the row", () => {
    const onMenu = vi.fn();
    const onOpen = vi.fn();
    render(<Rows onMenu={onMenu} onOpen={onOpen} />);
    const link = screen.getByText("Abena Mensah");
    fireEvent.touchStart(link, touch(link));
    act(() => void vi.advanceTimersByTime(500));
    expect(onMenu).toHaveBeenCalledTimes(1);
    fireEvent.touchEnd(link);
    fireEvent.click(link);
    expect(onOpen).not.toHaveBeenCalled();
  });

  it("does nothing when the finger moves, which is a scroll", () => {
    const onMenu = vi.fn();
    render(<Rows onMenu={onMenu} onOpen={() => {}} />);
    const link = screen.getByText("Abena Mensah");
    fireEvent.touchStart(link, touch(link));
    fireEvent.touchMove(link, touch(link, 10, 60));
    act(() => void vi.advanceTimersByTime(600));
    expect(onMenu).not.toHaveBeenCalled();
  });

  it("does nothing for a quick tap", () => {
    const onMenu = vi.fn();
    render(<Rows onMenu={onMenu} onOpen={() => {}} />);
    const link = screen.getByText("Abena Mensah");
    fireEvent.touchStart(link, touch(link));
    act(() => void vi.advanceTimersByTime(200));
    fireEvent.touchEnd(link);
    act(() => void vi.advanceTimersByTime(600));
    expect(onMenu).not.toHaveBeenCalled();
  });
});
