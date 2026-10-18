import { render, screen } from "@testing-library/react";
import { Notice } from "./Notice";

describe("Notice", () => {
  it("shows the title, the text and the action, as a status and not an alert", () => {
    render(
      <Notice title="Heads up" action={<button type="button">Do it</button>}>
        Some detail.
      </Notice>,
    );
    expect(screen.getByRole("status")).toHaveTextContent("Heads up");
    expect(screen.getByText("Some detail.")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Do it" })).toBeInTheDocument();
    expect(screen.queryByRole("alert")).not.toBeInTheDocument();
  });

  it("is still a status when it warns", () => {
    render(<Notice tone="warning" title="Careful" />);
    expect(screen.getByRole("status")).toHaveTextContent("Careful");
  });

  it("can be one plain line that is still a status", () => {
    render(<Notice tone="plain" title="Apart from tithes" />);
    expect(screen.getByRole("status")).toHaveTextContent("Apart from tithes");
  });
});
