import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { saveTokens } from "@/lib/tokens";
import type { Visitor } from "./api";
import { ConvertSheet } from "./ConvertSheet";

const json = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), { status });

const esi: Visitor = {
  id: "v1",
  name: "Esi Mensah",
  firstName: "Esi",
  lastName: "Mensah",
  phoneNumber: "055 212 3344",
  emailAddress: "esi@example.com",
  dateOfBirth: "1996-08-22",
  createdAt: "2026-09-20T09:00:00Z",
  visitCount: 3,
};

let fetchMock: ReturnType<typeof vi.fn>;
function show(answer: () => Response) {
  fetchMock = vi.fn(async () => answer());
  vi.stubGlobal("fetch", fetchMock);
  render(
    <QueryClientProvider
      client={
        new QueryClient({ defaultOptions: { queries: { retry: false } } })
      }
    >
      <MemoryRouter initialEntries={["/start"]}>
        <Routes>
          <Route
            path="/start"
            element={<ConvertSheet visitor={esi} onClose={() => {}} />}
          />
          <Route path="/visitors/:id" element={<p>Visitor page</p>} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

beforeEach(() => {
  saveTokens({
    accessToken: "a",
    accessTokenExpiresAt: "2099-01-01T00:00:00Z",
    refreshToken: "r",
    refreshTokenExpiresAt: "2099-01-01T00:00:00Z",
  });
});
afterEach(() => vi.unstubAllGlobals());

async function fillRest(user: ReturnType<typeof userEvent.setup>) {
  await user.type(screen.getByLabelText("Residential address"), "14 Palm St");
  await user.type(screen.getByLabelText("City"), "Accra");
  await user.type(screen.getByLabelText("Hometown"), "Kumasi");
  await user.type(screen.getByLabelText("Next of kin"), "Kofi Mensah");
  await user.type(screen.getByLabelText("Contact name"), "Kofi Mensah");
  await user.type(screen.getByLabelText("Contact phone"), "024 111 2222");
}

describe("ConvertSheet", () => {
  it("starts with what the visitor record knows and says they keep their history", () => {
    show(() => json({}));
    expect(screen.getByText("Make Esi Mensah a member")).toBeInTheDocument();
    expect(
      screen.getByText("Esi will keep their visit history."),
    ).toBeInTheDocument();
    expect(screen.getByLabelText("First name")).toHaveValue("Esi");
    expect(screen.getByLabelText("Phone")).toHaveValue("055 212 3344");
    expect(screen.getByLabelText("Date of birth")).toHaveValue("1996-08-22");
  });

  it("asks for the details a visitor does not have and sends nothing until they are there", async () => {
    const user = userEvent.setup();
    show(() => json({}));
    await user.click(screen.getByRole("button", { name: "Make member" }));
    expect(
      await screen.findByText("Enter a residential address"),
    ).toBeInTheDocument();
    expect(fetchMock).not.toHaveBeenCalled();
  });

  it("makes them a member and stays on their visitor page", async () => {
    const user = userEvent.setup();
    show(() => json({ id: "v1", name: "Esi Mensah" }));
    await fillRest(user);
    await user.click(screen.getByRole("button", { name: "Make member" }));
    expect(await screen.findByText("Visitor page")).toBeInTheDocument();
    const [url, init] = fetchMock.mock.calls[0] as [string, RequestInit];
    expect(url).toContain("/v1/visitors/v1/convert");
    expect(JSON.parse(init.body as string)).toMatchObject({
      firstName: "Esi",
      city: "Accra",
      status: "Active",
    });
  });

  it("says so when they were already made a member", async () => {
    const user = userEvent.setup();
    show(() => json({ title: "Conflict" }, 409));
    await fillRest(user);
    await user.click(screen.getByRole("button", { name: "Make member" }));
    await waitFor(() =>
      expect(screen.getByRole("alert")).toHaveTextContent(
        "Esi is already a member",
      ),
    );
  });
});
