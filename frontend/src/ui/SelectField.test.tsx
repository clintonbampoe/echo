import { useState } from "react";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, describe, expect, it, vi } from "vitest";
import { SelectField } from "./SelectField";

const options = [
  { value: "north", label: "Northern" },
  { value: "volta", label: "Volta" },
  { value: "west", label: "Western" },
];

function Harness({ error }: { error?: string }) {
  const [value, setValue] = useState("north");
  return (
    <>
      <SelectField
        label="Region"
        options={options}
        value={value}
        onChange={setValue}
        error={error}
      />
      <output>{value}</output>
    </>
  );
}

afterEach(() => vi.unstubAllGlobals());

describe("SelectField with a mouse", () => {
  it("opens, picks an option with a click, then closes", async () => {
    const user = userEvent.setup();
    render(<Harness />);
    const box = screen.getByRole("combobox", { name: "Region" });
    expect(box).toHaveTextContent("Northern");
    await user.click(box);
    expect(screen.getAllByRole("option")).toHaveLength(3);
    expect(screen.getByRole("option", { name: "Northern" })).toHaveAttribute(
      "aria-selected",
      "true",
    );
    await user.click(screen.getByRole("option", { name: "Volta" }));
    expect(screen.queryByRole("listbox")).toBeNull();
    expect(box).toHaveTextContent("Volta");
    expect(screen.getByRole("status")).toHaveTextContent("volta");
  });

  it("works from the keyboard and Escape leaves the value alone", async () => {
    const user = userEvent.setup();
    render(<Harness />);
    screen.getByRole("combobox").focus();
    await user.keyboard("{ArrowDown}{ArrowDown}{Enter}");
    expect(screen.getByRole("combobox")).toHaveTextContent("Volta");
    await user.keyboard("{ArrowDown}{ArrowDown}{Escape}");
    expect(screen.queryByRole("listbox")).toBeNull();
    expect(screen.getByRole("combobox")).toHaveTextContent("Volta");
  });

  it("jumps to a matching option when typing while closed", async () => {
    const user = userEvent.setup();
    render(<Harness />);
    screen.getByRole("combobox").focus();
    await user.keyboard("w");
    expect(screen.getByRole("combobox")).toHaveTextContent("Western");
  });

  it("marks the control invalid when there is an error", () => {
    render(<Harness error="Pick a region" />);
    expect(screen.getByRole("combobox")).toHaveAttribute(
      "aria-invalid",
      "true",
    );
    expect(screen.getByRole("alert")).toHaveTextContent("Pick a region");
  });
});

describe("SelectField on a touch screen", () => {
  it("uses the phone's own picker", async () => {
    vi.stubGlobal("matchMedia", () => ({
      matches: false,
      addEventListener() {},
      removeEventListener() {},
    }));
    const user = userEvent.setup();
    render(<Harness />);
    const select = screen.getByLabelText("Region");
    expect(select.tagName).toBe("SELECT");
    await user.selectOptions(select, "west");
    expect(screen.getByRole("status")).toHaveTextContent("west");
  });
});
