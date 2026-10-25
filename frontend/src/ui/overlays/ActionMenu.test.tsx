import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { ActionMenu } from "./ActionMenu";

const items = (pick: (role: string) => void, remove: () => void) => [
  {
    label: "Leader",
    group: "Change role",
    checked: false,
    onSelect: () => pick("Leader"),
  },
  {
    label: "Member",
    group: "Change role",
    checked: true,
    onSelect: () => pick("Member"),
  },
  {
    label: "Remove from group",
    danger: true,
    separated: true,
    onSelect: remove,
  },
];

describe("action menu", () => {
  it("shows a choice as radio items, with the current one ticked and a heading above", async () => {
    const user = userEvent.setup();
    render(
      <ActionMenu
        label="Actions for Ama"
        title="Ama"
        items={items(
          () => {},
          () => {},
        )}
      />,
    );
    await user.click(screen.getByRole("button", { name: "Actions for Ama" }));
    expect(screen.getByText("Change role")).toBeInTheDocument();
    expect(
      screen.getByRole("menuitemradio", { name: "Member" }),
    ).toHaveAttribute("aria-checked", "true");
    expect(
      screen.getByRole("menuitemradio", { name: "Leader" }),
    ).toHaveAttribute("aria-checked", "false");
    expect(
      screen.getByRole("menuitem", { name: "Remove from group" }),
    ).toBeInTheDocument();
  });

  it("runs the chosen action and closes", async () => {
    const user = userEvent.setup();
    const pick = vi.fn();
    render(
      <ActionMenu
        label="Actions for Ama"
        title="Ama"
        items={items(pick, () => {})}
      />,
    );
    await user.click(screen.getByRole("button", { name: "Actions for Ama" }));
    await user.click(screen.getByRole("menuitemradio", { name: "Leader" }));
    expect(pick).toHaveBeenCalledWith("Leader");
    expect(screen.queryByRole("menu")).toBeNull();
  });

  it("moves between radio items with the arrow keys", async () => {
    const user = userEvent.setup();
    render(
      <ActionMenu
        label="Actions for Ama"
        title="Ama"
        items={items(
          () => {},
          () => {},
        )}
      />,
    );
    await user.click(screen.getByRole("button", { name: "Actions for Ama" }));
    expect(document.activeElement).toBe(
      screen.getByRole("menuitemradio", { name: "Leader" }),
    );
    await user.keyboard("{ArrowDown}");
    expect(document.activeElement).toBe(
      screen.getByRole("menuitemradio", { name: "Member" }),
    );
  });
});
