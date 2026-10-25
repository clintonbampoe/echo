import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { RankedBars } from "./RankedBars";

const rows = [
  { key: "a", label: "Offerings", value: 400, text: "GH₵ 400" },
  { key: "b", label: "Seed", value: 100, text: "GH₵ 100" },
];

describe("ranked bars", () => {
  it("lists each name with its amount, biggest first", () => {
    render(<RankedBars label="By category" rows={rows} />);
    const items = within(screen.getByRole("list", { name: "By category" }))
      .getAllByRole("listitem")
      .map((li) => li.textContent);
    expect(items.map((t) => t?.replace(/^.*%/, ""))).toEqual([
      "OfferingsGH₵ 400",
      "SeedGH₵ 100",
    ]);
  });

  it("gives each row a pill with its share", () => {
    render(<RankedBars label="By category" rows={rows} />);
    expect(screen.getByText("Offerings · GH₵ 400 · 80%")).toBeInTheDocument();
    expect(screen.getByText("Seed · GH₵ 100 · 20%")).toBeInTheDocument();
  });

  it("turns a row into a button that picks it, and picking it again hands the same key back", async () => {
    const user = userEvent.setup();
    const onPick = vi.fn();
    render(
      <RankedBars
        label="By category"
        rows={rows}
        selected="a"
        onPick={onPick}
      />,
    );
    const first = screen.getByRole("button", { name: /Offerings/ });
    expect(first).toHaveAttribute("aria-pressed", "true");
    await user.click(first);
    expect(onPick).toHaveBeenCalledWith("a");
  });

  it("leaves a row that stands for several others closed", () => {
    render(
      <RankedBars
        label="By category"
        rows={[
          ...rows,
          {
            key: "other",
            label: "Other",
            value: 10,
            text: "GH₵ 10",
            closed: true,
          },
        ]}
        onPick={() => {}}
      />,
    );
    expect(screen.getAllByRole("button")).toHaveLength(2);
  });

  it("sizes each bar against the biggest", () => {
    const { container } = render(
      <RankedBars label="By category" rows={rows} />,
    );
    const bars = container.querySelectorAll<HTMLElement>("[style]");
    expect(bars[0]?.style.width).toBe("100%");
    expect(bars[1]?.style.width).toBe("25%");
  });
});
