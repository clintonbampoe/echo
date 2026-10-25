import { useState } from "react";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Wheel } from "./Wheel";

const minutes = Array.from({ length: 60 }, (_, i) =>
  String(i).padStart(2, "0"),
).map((v) => ({ value: v, label: v }));

function Harness({ start = "05" }: { start?: string }) {
  const [value, setValue] = useState(start);
  return (
    <>
      <Wheel
        label="Minute"
        options={minutes}
        value={value}
        onChange={setValue}
      />
      <output>{value}</output>
    </>
  );
}

describe("Wheel", () => {
  it("is a list that names the picked row", () => {
    render(<Harness />);
    expect(screen.getByRole("listbox", { name: "Minute" })).toBeInTheDocument();
    expect(screen.getAllByRole("option")).toHaveLength(60);
    expect(screen.getByRole("option", { name: "05" })).toHaveAttribute(
      "aria-selected",
      "true",
    );
  });

  it("picks the row that is tapped", async () => {
    render(<Harness />);
    await userEvent.click(screen.getByRole("option", { name: "42" }));
    expect(screen.getByRole("status")).toHaveTextContent("42");
    expect(screen.getByRole("option", { name: "42" })).toHaveAttribute(
      "aria-selected",
      "true",
    );
  });

  it("moves a row with the arrow keys, ten with the page keys, and to the ends", async () => {
    render(<Harness />);
    screen.getByRole("listbox").focus();
    await userEvent.keyboard("{ArrowDown}{ArrowDown}");
    expect(screen.getByRole("status")).toHaveTextContent("07");
    await userEvent.keyboard("{ArrowUp}");
    expect(screen.getByRole("status")).toHaveTextContent("06");
    await userEvent.keyboard("{End}");
    expect(screen.getByRole("status")).toHaveTextContent("59");
    await userEvent.keyboard("{Home}");
    expect(screen.getByRole("status")).toHaveTextContent("00");
  });

  it("jumps to the row that starts with the digits typed", async () => {
    render(<Harness />);
    screen.getByRole("listbox").focus();
    await userEvent.keyboard("4");
    expect(screen.getByRole("status")).toHaveTextContent("40");
    await userEvent.keyboard("{Escape}");
    await new Promise((resolve) => setTimeout(resolve, 650));
    await userEvent.keyboard("2");
    expect(screen.getByRole("status")).toHaveTextContent("20");
  });

  it("does not run past either end", async () => {
    render(<Harness start="00" />);
    screen.getByRole("listbox").focus();
    await userEvent.keyboard("{ArrowUp}");
    expect(screen.getByRole("status")).toHaveTextContent("00");
  });
});
