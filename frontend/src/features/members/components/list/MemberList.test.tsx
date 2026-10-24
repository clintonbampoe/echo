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
    renderApp(
      <MemberList members={[kwame, abena]} onEdit={vi.fn()} onDelete={vi.fn()} />,
    );
    expect(screen.getByRole("link", { name: "Kwame Asante" })).toHaveAttribute(
      "href",
      "/members/m1",
    );
    expect(screen.getAllByText("024 412 7788 · Accra")).toHaveLength(2);
    expect(screen.getByText("Inactive")).toBeInTheDocument();
    expect(screen.queryByText("Active")).toBeNull();
    expect(screen.getByText("12 Mar 2019")).toBeInTheDocument();
  });

  it("opens a row menu with Edit member and Delete, and hands the member to the chosen action", async () => {
    const user = userEvent.setup();
    const onDelete = vi.fn();
    renderApp(
      <MemberList members={[kwame]} onEdit={vi.fn()} onDelete={onDelete} />,
    );
    await user.click(
      screen.getByRole("button", { name: "Actions for Kwame Asante" }),
    );
    expect(screen.getAllByRole("menuitem").map((i) => i.textContent)).toEqual([
      "Edit member",
      "Delete",
    ]);
    await user.click(screen.getByRole("menuitem", { name: "Delete" }));
    expect(onDelete).toHaveBeenCalledWith(kwame);
    expect(screen.queryByRole("menu")).toBeNull();
  });

  it("closes the menu on Escape", async () => {
    const user = userEvent.setup();
    renderApp(
      <MemberList members={[kwame]} onEdit={vi.fn()} onDelete={vi.fn()} />,
    );
    await user.click(
      screen.getByRole("button", { name: "Actions for Kwame Asante" }),
    );
    await user.keyboard("{Escape}");
    expect(screen.queryByRole("menu")).toBeNull();
  });
});
