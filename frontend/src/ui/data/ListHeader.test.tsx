import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { ListHeader } from "./ListHeader";

describe("ListHeader", () => {
  it("shows the title and the summary", () => {
    render(<ListHeader title="All members" summary="24" />);
    expect(
      screen.getByRole("heading", { name: "All members" }),
    ).toBeInTheDocument();
    expect(screen.getByText("24")).toBeInTheDocument();
  });

  it("leaves the summary out when there is none", () => {
    const { container } = render(<ListHeader title="Leadership" />);
    expect(container.querySelectorAll("span")).toHaveLength(0);
  });
});
