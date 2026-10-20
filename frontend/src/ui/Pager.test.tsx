import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Pager } from "./Pager";

describe("pager", () => {
  it("has a dot for each page and moves to the one you pick", async () => {
    render(
      <Pager label="Money" titles={["First", "Second"]}>
        <p>one</p>
        <p>two</p>
      </Pager>,
    );
    const first = screen.getByRole("button", { name: "Show First" });
    const second = screen.getByRole("button", { name: "Show Second" });
    expect(first).toHaveAttribute("aria-current", "true");
    await userEvent.click(second);
    expect(second).toHaveAttribute("aria-current", "true");
    expect(first).toHaveAttribute("aria-current", "false");
  });

  it("has no dots for a single page", () => {
    render(
      <Pager label="Money" titles={["Only"]}>
        <p>one</p>
      </Pager>,
    );
    expect(screen.queryByRole("button")).not.toBeInTheDocument();
  });
});
