import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { Switch } from "./Switch";

describe("Switch", () => {
  it("reports its state and toggles on click", async () => {
    const onChange = vi.fn();
    render(
      <Switch label="Reduce motion" checked={false} onChange={onChange} />,
    );
    const sw = screen.getByRole("switch", { name: "Reduce motion" });
    expect(sw).toHaveAttribute("aria-checked", "false");
    await userEvent.click(sw);
    expect(onChange).toHaveBeenCalledWith(true);
  });
});
