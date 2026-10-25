import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { FormSheet } from "./FormSheet";

function setup() {
  const onClose = vi.fn();
  render(
    <FormSheet
      title="New member"
      onClose={onClose}
      formId="f"
      onSubmit={(e) => e.preventDefault()}
      submitLabel="Save"
      pending={false}
      failures={0}
    >
      <input aria-label="Name" />
    </FormSheet>,
  );
  return { onClose, user: userEvent.setup() };
}

describe("FormSheet", () => {
  it("closes at once when nothing was typed", async () => {
    const { onClose, user } = setup();
    await user.click(screen.getByRole("button", { name: "Close" }));
    expect(onClose).toHaveBeenCalled();
  });

  it("asks before throwing away what was typed", async () => {
    const { onClose, user } = setup();
    await user.type(screen.getByLabelText("Name"), "Ama");
    await user.click(screen.getByRole("button", { name: "Close" }));
    expect(onClose).not.toHaveBeenCalled();
    expect(screen.getByRole("alertdialog")).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Discard" }));
    expect(onClose).toHaveBeenCalled();
  });

  it("stays open when the person keeps editing", async () => {
    const { onClose, user } = setup();
    await user.type(screen.getByLabelText("Name"), "Ama");
    await user.click(screen.getByRole("button", { name: "Close" }));
    await user.click(screen.getByRole("button", { name: "Keep editing" }));
    expect(onClose).not.toHaveBeenCalled();
  });
});
