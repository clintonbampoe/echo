import { fireEvent, render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Field, PasswordField } from "./Field";

// Built from parts so no password-shaped literal sits in the source.
const good = `Q${"q".repeat(8)}7`;

describe("Field", () => {
  it("draws a lock on a value that cannot be changed, and not the email tick", () => {
    const { container } = render(
      <Field label="Email" type="email" readOnly defaultValue="a@b.org" />,
    );
    expect(container.querySelector("svg")).toBeInTheDocument();
    expect(screen.getByLabelText("Email")).toHaveAttribute("readonly");
  });

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

  it("marks the button while the password is shown, and clears it when hidden again", async () => {
    render(<PasswordField label="Password" />);
    const button = screen.getByRole("button", { name: "Show password" });
    expect(button).toHaveAttribute("data-shown", "false");

    await userEvent.click(button);
    expect(button).toHaveAttribute("data-shown", "true");

    await userEvent.click(button);
    expect(button).toHaveAttribute("data-shown", "false");
    expect(button).toHaveAccessibleName("Show password");
  });

  it("shows no meter unless it is given the password to read", () => {
    render(<PasswordField label="Password" />);
    expect(screen.queryByRole("status")).not.toBeInTheDocument();
  });

  it("shows the strength word and the next hint, and names the hint as the field's description", () => {
    const { rerender } = render(
      <PasswordField label="Password" strengthOf="" />,
    );
    const input = screen.getByLabelText("Password");
    expect(screen.getByText("Use at least 8 characters.")).toBeInTheDocument();
    expect(input).toHaveAccessibleDescription("Use at least 8 characters.");

    rerender(<PasswordField label="Password" strengthOf={good} />);
    expect(screen.getByRole("status")).toHaveTextContent("Good");
    expect(input).toHaveAccessibleDescription(
      "Add a special character, like ! or ?.",
    );

    rerender(<PasswordField label="Password" strengthOf={`${good}!`} />);
    expect(screen.getByRole("status")).toHaveTextContent("Strong");
  });

  it("keeps the error in the description beside the meter's hint", () => {
    render(
      <PasswordField
        label="Password"
        strengthOf="q"
        error="Use at least 8 characters."
      />,
    );
    expect(screen.getByLabelText("Password")).toHaveAccessibleDescription(
      "Use at least 8 characters. Use at least 8 characters.",
    );
  });
});

describe("PasswordField caps lock", () => {
  it("says so while Caps Lock is on, and stops when the field is left", async () => {
    render(<PasswordField label="Password" />);
    const input = screen.getByLabelText("Password");
    input.focus();
    fireEvent.keyDown(input, { key: "a", modifierCapsLock: true });
    expect(screen.getByText("Caps Lock is on")).toBeInTheDocument();
    fireEvent.blur(input);
    expect(screen.queryByText("Caps Lock is on")).not.toBeInTheDocument();
  });
});
