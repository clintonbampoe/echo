import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import { PersonList } from "./PersonList";

type Row = { id: string; name: string };
const rows: Row[] = [{ id: "m1", name: "Ama Boateng" }];

const show = (
  person: (
    r: Row,
  ) => Parameters<typeof PersonList<Row>>[0]["person"] extends (
    r: Row,
  ) => infer P
    ? P
    : never,
) =>
  render(
    <MemoryRouter>
      <PersonList
        label="People"
        title="People"
        rows={rows}
        rowKey={(r) => r.id}
        person={person}
        columns={[]}
      />
    </MemoryRouter>,
  );

describe("person list", () => {
  it("makes the name a link when the row opens a page", () => {
    show((r) => ({ name: r.name, href: `/members/${r.id}` }));
    expect(screen.getByRole("link", { name: "Ama Boateng" })).toHaveAttribute(
      "href",
      "/members/m1",
    );
  });

  it("makes the name a button when the row opens a panel on the same page", async () => {
    const user = userEvent.setup();
    const open = vi.fn();
    show((r) => ({ name: r.name, onOpen: open, plain: true }));
    expect(screen.queryByRole("link")).toBeNull();
    await user.click(screen.getByRole("button", { name: "Ama Boateng" }));
    expect(open).toHaveBeenCalled();
  });
});
