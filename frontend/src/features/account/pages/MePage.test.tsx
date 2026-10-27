import { screen } from "@testing-library/react";
import { json, pathOf, renderApp, signIn } from "@/test/kit";
import { MePage } from "./MePage";

beforeEach(signIn);
afterEach(() => vi.unstubAllGlobals());

describe("MePage", () => {
  it("shows who is signed in", async () => {
    vi.stubGlobal(
      "fetch",
      vi.fn(async (url: string) =>
        pathOf(url) === "/v1/users/me"
          ? json({
              id: "u1",
              name: "Kwabena Ansah",
              emailAddress: "k@x.org",
              role: "Admin",
            })
          : json({ id: "c1", name: "Grace Chapel" }),
      ),
    );
    renderApp(<MePage />);
    expect(await screen.findByText("Kwabena Ansah")).toBeInTheDocument();
    expect(await screen.findByText("Grace Chapel")).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: /sign out/i }),
    ).toBeInTheDocument();
  });
});
