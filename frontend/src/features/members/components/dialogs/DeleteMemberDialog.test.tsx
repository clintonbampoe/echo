import { screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { renderApp, signIn } from "@/test/kit";
import { aMember } from "../../testing/fixtures";
import { DeleteMemberDialog } from "./DeleteMemberDialog";

let fetchMock: ReturnType<typeof vi.fn>;
beforeEach(() => {
  signIn();
  fetchMock = vi.fn(async () => new Response(null, { status: 204 }));
  vi.stubGlobal("fetch", fetchMock);
});
afterEach(() => vi.unstubAllGlobals());

const deleted = () =>
  fetchMock.mock.calls.some(
    ([url, init]) =>
      init?.method === "DELETE" && url.includes("/v1/members/m1"),
  );

describe("delete member dialog", () => {
  it("says what happens in words that fit the member, and starts on the safe button", () => {
    renderApp(
      <DeleteMemberDialog
        member={aMember({
          name: "Abena Mensah",
          firstName: "Abena",
          gender: "Female",
        })}
        onClose={vi.fn()}
      />,
    );
    expect(screen.getByRole("alertdialog")).toHaveAccessibleName(
      "Delete Abena Mensah?",
    );
    expect(
      screen.getByText(
        "She leaves your lists and totals. Her tithes and attendance are kept for your records.",
      ),
    ).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Cancel" })).toHaveFocus();
  });

  it("keeps the member when asked to", async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();
    renderApp(<DeleteMemberDialog member={aMember()} onClose={onClose} />);
    await user.click(screen.getByRole("button", { name: "Cancel" }));
    expect(onClose).toHaveBeenCalled();
    expect(deleted()).toBe(false);
  });

  it("deletes, closes and says so", async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();
    renderApp(<DeleteMemberDialog member={aMember()} onClose={onClose} />);
    await user.click(screen.getByRole("button", { name: "Delete" }));
    await waitFor(() => expect(onClose).toHaveBeenCalled());
    expect(deleted()).toBe(true);
    expect(await screen.findByRole("status")).toHaveTextContent(
      "Member deleted",
    );
  });
});
