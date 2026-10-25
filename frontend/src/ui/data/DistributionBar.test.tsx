import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { DistributionBar } from "./DistributionBar";

const parts = [
  { key: "momo", label: "Mobile money", value: 400, text: "GH₵ 400" },
  { key: "cash", label: "Cash", value: 400, text: "GH₵ 400" },
  { key: "empty", label: "Cheque", value: 0, text: "GH₵ 0" },
  { key: "other", label: "Other", value: 200, text: "GH₵ 200", closed: true },
];

describe("distribution bar", () => {
  it("lists each part with its amount and share, and leaves out what is empty", () => {
    render(<DistributionBar label="How it was given" parts={parts} />);
    const group = screen.getByRole("group", { name: "How it was given" });
    expect(group).toHaveTextContent("Mobile money");
    expect(group).toHaveTextContent("40%");
    expect(group).toHaveTextContent("20%");
    expect(group).not.toHaveTextContent("Cheque");
  });

  it("makes a legend row a button that picks its part, but not a closed one", async () => {
    const user = userEvent.setup();
    const onPick = vi.fn();
    render(
      <DistributionBar
        label="How it was given"
        parts={parts}
        selected="cash"
        onPick={onPick}
      />,
    );
    expect(screen.getByRole("button", { name: /Cash/ })).toHaveAttribute(
      "aria-pressed",
      "true",
    );
    await user.click(screen.getByRole("button", { name: /Mobile money/ }));
    expect(onPick).toHaveBeenCalledWith("momo");
    expect(screen.queryByRole("button", { name: /Other/ })).toBeNull();
  });

  it("gives each segment a pill with its name, amount and share", () => {
    const { container } = render(
      <DistributionBar label="How it was given" parts={parts} />,
    );
    const tips = [...container.querySelectorAll("[data-tip]")].map((e) =>
      e.getAttribute("data-tip"),
    );
    expect(tips).toEqual([
      "Mobile money · GH₵ 400 · 40%",
      "Cash · GH₵ 400 · 40%",
      "Other · GH₵ 200 · 20%",
    ]);
  });
});
