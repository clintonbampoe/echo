import { render, screen } from "@testing-library/react";
import { ProgressBar } from "./ProgressBar";

describe("progress bar", () => {
  it("says how far along it is", () => {
    render(<ProgressBar value={0.6} label="Raised against target" />);
    const bar = screen.getByRole("progressbar", {
      name: "Raised against target",
    });
    expect(bar).toHaveAttribute("aria-valuenow", "60");
    expect(bar.firstElementChild).toHaveStyle({ width: "60%" });
  });

  it("stops at full and never goes below empty", () => {
    const { rerender } = render(<ProgressBar value={1.4} label="x" />);
    expect(screen.getByRole("progressbar")).toHaveAttribute(
      "aria-valuenow",
      "100",
    );
    rerender(<ProgressBar value={-1} label="x" />);
    expect(screen.getByRole("progressbar")).toHaveAttribute(
      "aria-valuenow",
      "0",
    );
  });
});
