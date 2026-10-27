import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { useState } from "react";
import { SegmentedField } from "./SegmentedField";

const options = [
  { value: "a", label: "Alpha" },
  { value: "b", label: "Beta" },
] as const;

function Harness({ onChange = () => {} }: { onChange?: (v: string) => void }) {
  const [value, setValue] = useState<string>("a");
  return (
    <SegmentedField
      label="Letter"
      options={options}
      value={value}
      onChange={(v) => {
        setValue(v);
        onChange(v);
      }}
    />
  );
}

describe("segmented field", () => {
  it("is a labelled group of radio buttons with the current one chosen", () => {
    render(<Harness />);
    expect(screen.getByRole("group", { name: "Letter" })).toBeInTheDocument();
    expect(screen.getByRole("radio", { name: "Alpha" })).toBeChecked();
    expect(screen.getByRole("radio", { name: "Beta" })).not.toBeChecked();
  });

  it("tells the page which one was picked", async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();
    render(<Harness onChange={onChange} />);
    await user.click(screen.getByRole("radio", { name: "Beta" }));
    expect(onChange).toHaveBeenCalledWith("b");
    expect(screen.getByRole("radio", { name: "Beta" })).toBeChecked();
  });

  it("can be drawn as one joined bar and still be a group of radios", () => {
    render(
      <SegmentedField
        label="Show"
        options={options}
        value="a"
        onChange={() => {}}
        joined
        hideLabel
      />,
    );
    expect(screen.getByRole("group", { name: "Show" })).toBeInTheDocument();
    expect(screen.getAllByRole("radio")).toHaveLength(2);
  });

  it("tells a joined bar which choice is picked, so its thumb can slide there", async () => {
    const user = userEvent.setup();
    function Joined() {
      const [value, setValue] = useState<string>("a");
      return (
        <SegmentedField
          label="Show"
          options={options}
          value={value}
          onChange={setValue}
          joined
          hideLabel
        />
      );
    }
    render(<Joined />);
    const bar = screen
      .getByRole("radio", { name: "Alpha" })
      .closest("div") as HTMLElement;
    expect(bar.style.getPropertyValue("--n")).toBe("2");
    expect(bar.style.getPropertyValue("--i")).toBe("0");
    await user.click(screen.getByRole("radio", { name: "Beta" }));
    expect(bar.style.getPropertyValue("--i")).toBe("1");
  });
});
