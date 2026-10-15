import { render, screen } from "@testing-library/react";
import { StatStrip } from "./StatStrip";

describe("stat strip", () => {
  it("keeps the full text of a long value and note on hover, since the tile cuts it short", () => {
    const name = "Reverend Doctor Kofi Mensah-Asamoah Boateng";
    render(
      <StatStrip
        label="Numbers"
        stats={[
          { label: "Led by", value: name, note: "and Ama Serwaa Boateng" },
        ]}
      />,
    );
    expect(screen.getByText(name)).toHaveAttribute("title", name);
    expect(screen.getByText("and Ama Serwaa Boateng")).toHaveAttribute(
      "title",
      "and Ama Serwaa Boateng",
    );
  });
});
