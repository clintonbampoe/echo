import { screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { json, renderApp, signIn } from "@/test/kit";
import { aMember } from "../../testing/fixtures";
import { EditMemberSheet } from "./EditMemberSheet";

let fetchMock: ReturnType<typeof vi.fn>;
beforeEach(() => {
  signIn();
  fetchMock = vi.fn(async () => json(aMember()));
  vi.stubGlobal("fetch", fetchMock);
});
afterEach(() => vi.unstubAllGlobals());

describe("edit member sheet", () => {
  it("starts filled in, saves with a PUT, says so, and closes", async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();
    renderApp(<EditMemberSheet member={aMember()} onClose={onClose} />);
    expect(screen.getByLabelText("First name")).toHaveValue("Kwame");
    expect(
      screen.getByRole("button", { name: /Date of birth/ }),
    ).toHaveTextContent("14 Feb 1988");

    await user.clear(screen.getByLabelText("City"));
    await user.type(screen.getByLabelText("City"), "Tema");
    expect(screen.getByText("Unsaved changes")).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Save changes" }));

    await waitFor(() => expect(onClose).toHaveBeenCalled());
    const put = fetchMock.mock.calls.find(([, init]) => init?.method === "PUT");
    expect(put?.[0]).toContain("/v1/members/m1");
    expect(JSON.parse(put?.[1].body).city).toBe("Tema");
    expect(await screen.findByText("Changes saved")).toBeInTheDocument();
  });
});
