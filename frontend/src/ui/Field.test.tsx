import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Field, PasswordField } from "./Field";

describe("Field", () => {
  it("connects the label to the input", () => {
    render(<Field label="Email address" />);
    expect(screen.getByLabelText("Email address")).toBeInTheDocument();
  });

  it("shows the error instead of the hint, and flags the input as invalid", () => {
    render(
      <Field
        label="Email"
        hint="We will email you."
        error="Enter a valid email."
      />,
    );
    const input = screen.getByLabelText("Email");
    expect(input).toHaveAttribute("aria-invalid", "true");
    expect(input).toHaveAccessibleDescription("Enter a valid email.");
    expect(screen.queryByText("We will email you.")).not.toBeInTheDocument();
  });

  it("describes the input with the hint when there is no error", () => {
    render(<Field label="Email" hint="We will email you." />);
    expect(screen.getByLabelText("Email")).toHaveAccessibleDescription(
      "We will email you.",
    );
  });
});

describe("Field unit", () => {
  it("shows the unit beside the input without making it part of the label", () => {
    render(<Field label="Amount" unit="GH₵" />);
    expect(screen.getByLabelText("Amount")).toBeInTheDocument();
    expect(screen.getByText("GH₵")).toBeInTheDocument();
  });
});

describe("PasswordField", () => {
  it("hides the password and reveals it on request", async () => {
    render(<PasswordField label="Password" />);
    const input = screen.getByLabelText("Password");
    expect(input).toHaveAttribute("type", "password");
    await userEvent.click(
      screen.getByRole("button", { name: "Show password" }),
    );
    expect(input).toHaveAttribute("type", "text");
    expect(
      screen.getByRole("button", { name: "Hide password" }),
    ).toBeInTheDocument();
  });
});
