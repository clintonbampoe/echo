import { render, screen } from "@testing-library/react";
import { SubmitButton } from "./SubmitButton";

describe("SubmitButton", () => {
  it("shows its label and sends the form", () => {
    render(<SubmitButton>Sign in</SubmitButton>);
    const button = screen.getByRole("button", { name: "Sign in" });
    expect(button).toHaveAttribute("type", "submit");
    expect(button).toBeEnabled();
  });

  it("ignores clicks while it waits, without a spinner", () => {
    render(<SubmitButton loading>Sign in</SubmitButton>);
    const button = screen.getByRole("button");
    expect(button).toBeDisabled();
    expect(button).toHaveAttribute("aria-busy", "true");
    expect(button.querySelector("svg")).toBeNull();
  });

  it("says the result once it worked, and stays still", () => {
    render(<SubmitButton done="Welcome back">Sign in</SubmitButton>);
    expect(screen.getByText("Welcome back")).toBeInTheDocument();
    expect(screen.getByRole("button")).toBeDisabled();
  });
});
