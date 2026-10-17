import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import { saveTokens } from "@/lib/tokens";
import type { Member } from "./api";
import { DeleteMemberDialog } from "./DeleteMemberDialog";
import { EditMemberSheet } from "./EditMemberSheet";
import { MemberList } from "./MemberList";

const json = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), { status });

const member: Member = {
  id: "m1",
  name: "Kwame Asante",
  firstName: "Kwame",
  lastName: "Asante",
  phoneNumber: "0244127788",
  dateOfBirth: "1988-02-14T00:00:00",
  gender: "Male",
  residentialAddress: "14 Palm St",
  city: "Accra",
  hometown: "Kumasi",
  region: "GreaterAccra",
  maritalStatus: "Single",
  nextOfKin: "Efua Asante",
  emergencyContactName: "Efua Asante",
  emergencyContactPhoneNumber: "0553014408",
  status: "Active",
  createdAt: "2026-01-01T00:00:00Z",
};

let fetchMock: ReturnType<typeof vi.fn>;
beforeEach(() => {
  saveTokens({
    accessToken: "a",
    accessTokenExpiresAt: "2099-01-01T00:00:00Z",
    refreshToken: "r",
    refreshTokenExpiresAt: "2099-01-01T00:00:00Z",
  });
  fetchMock = vi.fn(async (url: string, init?: RequestInit) => {
    if (url.includes("/congregation"))
      return json({ id: "2", name: "Grace Chapel" });
    if (init?.method === "DELETE") return new Response(null, { status: 204 });
    return json(member);
  });
  vi.stubGlobal("fetch", fetchMock);
});
afterEach(() => vi.unstubAllGlobals());

const wrap = (ui: React.ReactElement) =>
  render(
    <QueryClientProvider
      client={
        new QueryClient({ defaultOptions: { queries: { retry: false } } })
      }
    >
      <MemoryRouter>{ui}</MemoryRouter>
    </QueryClientProvider>,
  );

describe("row menu", () => {
  it("opens with Edit and Delete and hands the member to the chosen action", async () => {
    const user = userEvent.setup();
    const onEdit = vi.fn();
    const onDelete = vi.fn();
    wrap(<MemberList members={[member]} onEdit={onEdit} onDelete={onDelete} />);
    await user.click(
      screen.getByRole("button", { name: "Actions for Kwame Asante" }),
    );
    expect(screen.getAllByRole("menuitem").map((i) => i.textContent)).toEqual([
      "Edit",
      "Delete",
    ]);
    await user.click(screen.getByRole("menuitem", { name: "Delete" }));
    expect(onDelete).toHaveBeenCalledWith(member);
    expect(screen.queryByRole("menu")).toBeNull();
  });

  it("closes on Escape", async () => {
    const user = userEvent.setup();
    wrap(<MemberList members={[member]} onEdit={vi.fn()} onDelete={vi.fn()} />);
    await user.click(
      screen.getByRole("button", { name: "Actions for Kwame Asante" }),
    );
    await user.keyboard("{Escape}");
    expect(screen.queryByRole("menu")).toBeNull();
  });
});

describe("edit sheet", () => {
  it("starts filled in and saves with a PUT", async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();
    wrap(<EditMemberSheet member={member} onClose={onClose} />);
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
  });
});

describe("delete dialog", () => {
  it("keeps the member when asked to", async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();
    wrap(<DeleteMemberDialog member={member} onClose={onClose} />);
    expect(screen.getByRole("button", { name: "Keep member" })).toHaveFocus();
    await user.click(screen.getByRole("button", { name: "Keep member" }));
    expect(onClose).toHaveBeenCalled();
    expect(fetchMock.mock.calls.some(([, i]) => i?.method === "DELETE")).toBe(
      false,
    );
  });

  it("deletes and closes", async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();
    wrap(<DeleteMemberDialog member={member} onClose={onClose} />);
    await user.click(screen.getByRole("button", { name: "Delete member" }));
    await waitFor(() => expect(onClose).toHaveBeenCalled());
    expect(
      fetchMock.mock.calls.some(
        ([u, i]) => i?.method === "DELETE" && u.includes("/v1/members/m1"),
      ),
    ).toBe(true);
  });
});
