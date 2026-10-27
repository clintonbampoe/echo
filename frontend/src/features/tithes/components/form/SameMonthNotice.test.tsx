import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { SameMonthNotice } from "./SameMonthNotice";
import { aTithe } from "../../testing/fixtures";

const tithe = aTithe({
  id: "a",
  memberId: "m1",
  memberName: "Kofi Mensah",
  amount: 500,
  forYear: 2026,
  forMonth: "October",
  paymentMethod: "Cash",
  collectionDate: "2026-10-11",
  createdAt: "",
});

describe("same month notice", () => {
  it("names the month, shows the tithe it clashes with and opens it to update", async () => {
    const onUpdate = vi.fn();
    render(
      <SameMonthNotice
        memberName="Kofi Mensah"
        period="October 2026"
        tithe={tithe}
        onUpdate={onUpdate}
      />,
    );
    expect(
      screen.getByText("Kofi Mensah already has a tithe for October 2026"),
    ).toBeInTheDocument();
    expect(
      screen.getByText(/GH₵ 500, collected 11 Oct 2026/),
    ).toBeInTheDocument();
    await userEvent.click(
      screen.getByRole("button", { name: "Update that tithe" }),
    );
    expect(onUpdate).toHaveBeenCalledWith(tithe);
  });
});
