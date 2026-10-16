import { screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import {
  asked,
  json,
  renderAt,
  route,
  service,
  signIn,
} from "../attendanceTestKit";
import { SessionTypesPage } from "./SessionTypesPage";

let fetchMock: ReturnType<typeof vi.fn>;
beforeEach(() => {
  signIn();
  fetchMock = vi.fn(async (url: string, init?: RequestInit) => {
    const u = new URL(url, "http://x");
    if (init?.method === "POST")
      return json({ id: 9, ...JSON.parse(String(init.body)) }, 201);
    if (init?.method === "PUT")
      return json({ ...service, name: JSON.parse(String(init.body)).name });
    if (init?.method === "DELETE") return new Response(null, { status: 204 });
    return u.pathname.endsWith("/attendance/labels")
      ? json([service, { id: 4, name: "Bible Study", category: "Study" }])
      : json({}, 404);
  });
  vi.stubGlobal("fetch", fetchMock);
});
afterEach(() => vi.unstubAllGlobals());

const open = () =>
  renderAt(
    "/attendance/labels",
    route("/attendance/labels", <SessionTypesPage />),
  );
const sent = (method: string) =>
  asked(fetchMock).filter((c) => c.init?.method === method);

describe("session types page", () => {
  it("lists the types with their categories", async () => {
    open();
    const list = await screen.findByRole("list", { name: "Session types" });
    expect(within(list).getByText("Sunday Service")).toBeInTheDocument();
    expect(within(list).getByText("Study")).toBeInTheDocument();
    expect(screen.getByRole("link", { name: /Attendance/ })).toHaveAttribute(
      "href",
      "/attendance",
    );
    expect(screen.getByText("2 session types")).toBeInTheDocument();
  });

  it("creates a type with a name and a category", async () => {
    open();
    await userEvent.click(
      await screen.findByRole("button", { name: "New session type" }),
    );
    const sheet = await screen.findByRole("dialog", {
      name: "New session type",
    });
    await userEvent.click(
      within(sheet).getByRole("button", { name: "Create session type" }),
    );
    expect(await within(sheet).findByText("Enter a name")).toBeInTheDocument();
    expect(within(sheet).getByText("Choose a category")).toBeInTheDocument();
    await userEvent.type(
      within(sheet).getByLabelText("Name"),
      " Youth Meeting ",
    );
    await userEvent.click(
      within(sheet).getByRole("combobox", { name: "Category" }),
    );
    await userEvent.click(
      await screen.findByRole("option", { name: "Meeting" }),
    );
    await userEvent.click(
      within(sheet).getByRole("button", { name: "Create session type" }),
    );
    await waitFor(() => expect(sent("POST")).toHaveLength(1));
    expect(JSON.parse(String(sent("POST")[0]?.init?.body))).toEqual({
      name: "Youth Meeting",
      category: "Meeting",
    });
  });

  it("renames a type but leaves its category alone", async () => {
    open();
    await userEvent.click(
      await screen.findByRole("button", { name: "Actions for Sunday Service" }),
    );
    await userEvent.click(
      await screen.findByRole("menuitem", { name: "Edit" }),
    );
    const sheet = await screen.findByRole("dialog", {
      name: "Edit Sunday Service",
    });
    expect(within(sheet).getByLabelText("Category")).toHaveAttribute(
      "readonly",
    );
    const name = within(sheet).getByLabelText("Name");
    await userEvent.clear(name);
    await userEvent.type(name, "Main Service");
    await userEvent.click(
      within(sheet).getByRole("button", { name: "Save changes" }),
    );
    await waitFor(() => expect(sent("PUT")).toHaveLength(1));
    expect(JSON.parse(String(sent("PUT")[0]?.init?.body))).toEqual({
      name: "Main Service",
    });
  });

  it("deletes a type after asking", async () => {
    open();
    await userEvent.click(
      await screen.findByRole("button", { name: "Actions for Bible Study" }),
    );
    await userEvent.click(
      await screen.findByRole("menuitem", { name: "Delete" }),
    );
    expect(
      await screen.findByText("Delete the Bible Study type?"),
    ).toBeInTheDocument();
    await userEvent.click(screen.getByRole("button", { name: "Delete type" }));
    await waitFor(() => expect(sent("DELETE")).toHaveLength(1));
    expect(sent("DELETE")[0]?.url.pathname).toMatch(/\/attendance\/labels\/4$/);
  });
});
