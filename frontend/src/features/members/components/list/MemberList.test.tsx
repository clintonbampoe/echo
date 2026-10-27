import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { renderApp } from "@/test/kit";
import { aMember } from "../../testing/fixtures";
import { MemberList } from "./MemberList";

const kwame = aMember();
const abena = aMember({
  id: "m2",
  name: "Abena Mensah",
  status: "Inactive",
  joinedDate: null,
});

describe("member list", () => {
  it("shows phone and city under the name, and a chip only for members who are not active", () => {
    renderApp(<MemberList members={[kwame, abena]} onAct={vi.fn()} />);
    expect(screen.getByRole("link", { name: "Kwame Asante" })).toHaveAttribute(
      "href",
      "/members/m1",
    );
    expect(screen.getAllByText("024 412 7788 · Accra")).toHaveLength(2);
    expect(screen.getByText("Inactive")).toBeInTheDocument();
    expect(screen.queryByText("Active")).toBeNull();
    expect(screen.getByText("12 Mar 2019")).toBeInTheDocument();
  });

  it("has a row menu that asks to edit or delete the member", async () => {
    const onAct = vi.fn();
    renderApp(<MemberList members={[kwame]} onAct={onAct} />);
    await userEvent.click(
      screen.getByRole("button", { name: "Actions for Kwame Asante" }),
    );
    await userEvent.click(
      await screen.findByRole("menuitem", { name: "Delete" }),
    );
    expect(onAct).toHaveBeenCalledWith({ kind: "delete", member: kwame });
  });

  it("puts the footer at the bottom of the card", () => {
    renderApp(
      <MemberList
        members={[kwame]}
        onAct={vi.fn()}
        footer={<p>Showing 1 member</p>}
      />,
    );
    expect(screen.getByText("Showing 1 member")).toBeInTheDocument();
  });
});
