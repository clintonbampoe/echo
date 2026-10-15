import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import type { Paged } from "@/lib/paged";
import { ListBody } from "./ListBody";

const list = (over: Partial<Paged<string>>): Paged<string> =>
  ({
    rows: [],
    loading: false,
    failed: false,
    hasMore: false,
    loadingMore: false,
    more: vi.fn(),
    retry: vi.fn(),
    ...over,
  }) as Paged<string>;

describe("ListBody", () => {
  it("shows a card with an icon and a message when nothing is there yet", () => {
    render(
      <ListBody
        list={list({})}
        icon="tithes"
        noun={["tithe", "tithes"]}
        emptyText="Record one."
        filtered={false}
        onClear={vi.fn()}
      >
        {() => null}
      </ListBody>,
    );
    expect(screen.getByRole("status")).toHaveTextContent("No tithes yet");
    expect(screen.getByText("Record one.")).toBeInTheDocument();
  });

  it("takes its own heading for the empty card", () => {
    render(
      <ListBody
        list={list({})}
        icon="tithes"
        noun={["tithe", "tithes"]}
        emptyTitle="No tithes collected this week"
        emptyText="Record one."
        filtered={false}
        onClear={vi.fn()}
      >
        {() => null}
      </ListBody>,
    );
    expect(
      screen.getByText("No tithes collected this week"),
    ).toBeInTheDocument();
  });

  it("offers to clear the filters when they leave nothing", async () => {
    const onClear = vi.fn();
    render(
      <ListBody
        list={list({})}
        icon="tithes"
        noun={["tithe", "tithes"]}
        emptyText="x"
        filtered
        onClear={onClear}
      >
        {() => null}
      </ListBody>,
    );
    expect(screen.getByText("No tithes match")).toBeInTheDocument();
    await userEvent.click(
      screen.getByRole("button", { name: "Clear filters" }),
    );
    expect(onClear).toHaveBeenCalled();
  });
});
