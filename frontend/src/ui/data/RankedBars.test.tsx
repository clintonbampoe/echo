import { render, screen, within } from "@testing-library/react";
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
    expect(items).toEqual(["OfferingsGH₵ 400", "SeedGH₵ 100"]);
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
