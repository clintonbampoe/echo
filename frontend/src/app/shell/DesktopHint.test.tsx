import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { DesktopHint } from "./DesktopHint";

// jsdom has no screen: say whether the window is narrow.
function narrow(on: boolean) {
  vi.stubGlobal("matchMedia", (query: string) => ({
    matches: on,
    media: query,
    addEventListener: () => {},
    removeEventListener: () => {},
  }));
}

beforeEach(() => localStorage.clear());
afterEach(() => vi.unstubAllGlobals());

describe("DesktopHint", () => {
  it("says Echo is best on a desktop on a small screen", () => {
    narrow(true);
    render(<DesktopHint />);
    expect(screen.getByText("Best on a desktop")).toBeInTheDocument();
  });

  it("says nothing on a wide screen", () => {
    narrow(false);
    render(<DesktopHint />);
    expect(screen.queryByText("Best on a desktop")).toBeNull();
  });

  it("goes away when dismissed, and stays away for a day", async () => {
    narrow(true);
    const first = render(<DesktopHint />);
    await userEvent.click(screen.getByRole("button", { name: "Got it" }));
    expect(screen.queryByText("Best on a desktop")).toBeNull();
    first.unmount();
    render(<DesktopHint />);
    expect(screen.queryByText("Best on a desktop")).toBeNull();
  });

  it("comes back once the quiet day is over", async () => {
    narrow(true);
    localStorage.setItem(
      "echo.desktopHint.shownAt",
      String(Date.now() - 2 * 86_400_000),
    );
    render(<DesktopHint />);
    expect(screen.getByText("Best on a desktop")).toBeInTheDocument();
  });
});
