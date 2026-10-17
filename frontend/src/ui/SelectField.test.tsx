import { useState } from "react";
import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
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

const regions = [
  "Ashanti",
  "Bono",
  "Central",
  "Eastern",
  "Greater Accra",
  "Northern",
  "Volta",
  "Western",
].map((v) => ({ value: v, label: v }));

function LongHarness() {
  const [value, setValue] = useState("Volta");
  return (
    <>
      <SelectField
        label="Region"
        options={regions}
        value={value}
        onChange={setValue}
      />
      <output>{value}</output>
    </>
  );
}

describe("SelectField with a long list and a mouse", () => {
  it("has a search box that filters, and Enter picks the first match", async () => {
    const user = userEvent.setup();
    render(<LongHarness />);
    await user.click(screen.getByRole("combobox", { name: "Region" }));
    const search = screen.getByRole("searchbox", { name: "Search region" });
    expect(search).toHaveFocus();
    await user.keyboard("ern");
    expect(screen.getAllByRole("option").map((o) => o.textContent)).toEqual([
      "Eastern",
      "Northern",
      "Western",
    ]);
    await user.keyboard("{ArrowDown}{Enter}");
    expect(screen.getByRole("status")).toHaveTextContent("Northern");
    expect(screen.queryByRole("listbox")).toBeNull();
    expect(screen.getByRole("combobox")).toHaveFocus();
  });

  it("says so when nothing matches, and opens with the first letter typed on the closed field", async () => {
    const user = userEvent.setup();
    render(<LongHarness />);
    screen.getByRole("combobox").focus();
    await user.keyboard("zz");
    expect(screen.getByText("No matches")).toBeInTheDocument();
    await user.keyboard("{Escape}");
    expect(screen.getByRole("combobox")).toHaveFocus();
    await user.keyboard("b");
    expect(screen.getByRole("searchbox")).toHaveValue("b");
    expect(screen.getAllByRole("option").map((o) => o.textContent)).toEqual([
      "Bono",
    ]);
  });
});

describe("SelectField on a touch screen", () => {
  beforeEach(() =>
    vi.stubGlobal("matchMedia", () => ({
      matches: false,
      addEventListener() {},
      removeEventListener() {},
    })),
  );

  it("opens the choices in a bottom sheet and picks one", async () => {
    const user = userEvent.setup();
    render(<Harness />);
    expect(document.querySelector("select")).toBeNull();
    const field = screen.getByRole("button", { name: /Region/ });
    expect(field).toHaveTextContent("Northern");
    await user.click(field);
    const dialog = screen.getByRole("dialog", { name: "Region" });
    expect(within(dialog).getAllByRole("option")).toHaveLength(3);
    expect(
      within(dialog).getByRole("option", { name: "Northern" }),
    ).toHaveAttribute("aria-selected", "true");
    await user.click(within(dialog).getByRole("option", { name: "Western" }));
    expect(screen.queryByRole("dialog")).toBeNull();
    expect(screen.getByRole("status")).toHaveTextContent("west");
    expect(field).toHaveTextContent("Western");
  });

  it("shows a search box for a long list and forgets it when the sheet closes", async () => {
    const user = userEvent.setup();
    render(<LongHarness />);
    await user.click(screen.getByRole("button", { name: /Region/ }));
    await user.type(
      screen.getByRole("searchbox", { name: "Search region" }),
      "bo",
    );
    expect(screen.getAllByRole("option").map((o) => o.textContent)).toEqual([
      "Bono",
    ]);
    await user.keyboard("{Escape}");
    await user.click(screen.getByRole("button", { name: /Region/ }));
    expect(screen.getByRole("searchbox")).toHaveValue("");
    expect(screen.getAllByRole("option")).toHaveLength(8);
  });
});
