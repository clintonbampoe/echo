import { screen, within } from "@testing-library/react";
import { json, renderCard, signIn } from "../dashboardTestKit";
import { ProjectsCard } from "./ProjectsCard";

const project = (id: string, name: string, raisedAmount: number) => ({
  id,
  name,
  raisedAmount,
});

let answer: () => Response;
beforeEach(() => {
  signIn();
  vi.stubGlobal(
    "fetch",
    vi.fn(async () => answer()),
  );
});
afterEach(() => vi.unstubAllGlobals());

describe("projects card", () => {
  it("lists what each project has raised, with its share", async () => {
    answer = () =>
      json({
        data: [
          project("p1", "Borehole", 12000),
          project("p2", "Church roof", 4000),
        ],
        hasMore: false,
        next: null,
      });
    renderCard(<ProjectsCard />);
    const rows = within(await screen.findByRole("list"));
    expect(rows.getByText("Borehole")).toBeInTheDocument();
    expect(rows.getByText("75%")).toBeInTheDocument();
    expect(rows.getByText("GH₵ 4,000")).toBeInTheDocument();
  });

  it("says so when nothing has been raised", async () => {
    answer = () =>
      json({
        data: [project("p1", "Borehole", 0)],
        hasMore: false,
        next: null,
      });
    renderCard(<ProjectsCard />);
    expect(await screen.findByText("Nothing raised yet")).toBeInTheDocument();
  });

  it("says so when it could not load", async () => {
    answer = () => json({ title: "Server error", status: 500 }, 500);
    renderCard(<ProjectsCard />);
    expect(await screen.findByText("Could not load")).toBeInTheDocument();
  });
});
