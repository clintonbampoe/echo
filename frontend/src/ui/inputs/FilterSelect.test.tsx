import { useState } from "react";
import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, describe, expect, it, vi } from "vitest";
import { FilterSelect } from "./FilterSelect";
import { plainOptions, withAny } from "@/lib/options";

const options = withAny("Any status", plainOptions(["Active", "Inactive"]));

function Harness() {
  const [value, setValue] = useState("");
  return (
    <>
      <FilterSelect
        label="Status"
        options={options}
        value={value}
        onChange={setValue}
      />
      <output>{value || "none"}</output>
    </>
  );
}

afterEach(() => vi.unstubAllGlobals());

describe("FilterSelect", () => {
  it("opens our menu with a mouse and narrows the list", async () => {
    const user = userEvent.setup();
    render(<Harness />);
    await user.click(screen.getByRole("button", { name: "Status: Any" }));
    await user.click(screen.getByRole("option", { name: "Inactive" }));
    expect(screen.getByRole("status")).toHaveTextContent("Inactive");
    expect(
      screen.getByRole("button", { name: "Status: Inactive" }),
    ).toBeInTheDocument();
  });

  it("clears an applied filter from the pill's own ✕", async () => {
    const user = userEvent.setup();
    render(<Harness />);
    expect(screen.queryByRole("button", { name: /^Clear/ })).toBeNull();
    await user.click(screen.getByRole("button", { name: "Status: Any" }));
    await user.click(screen.getByRole("option", { name: "Active" }));
    await user.click(
      screen.getByRole("button", { name: "Clear status filter" }),
    );
    expect(screen.getByRole("status")).toHaveTextContent("none");
    expect(
      screen.getByRole("button", { name: "Status: Any" }),
    ).toBeInTheDocument();
  });

  it("opens a bottom sheet on a touch screen, with no native select", async () => {
    vi.stubGlobal("matchMedia", () => ({
      matches: false,
      addEventListener() {},
      removeEventListener() {},
    }));
    const user = userEvent.setup();
    render(<Harness />);
    expect(document.querySelector("select")).toBeNull();
    await user.click(screen.getByRole("button", { name: "Status: Any" }));
    const dialog = screen.getByRole("dialog", { name: "Status" });
    await user.click(within(dialog).getByRole("option", { name: "Active" }));
    expect(screen.queryByRole("dialog")).toBeNull();
    expect(screen.getByRole("status")).toHaveTextContent("Active");
  });
});
