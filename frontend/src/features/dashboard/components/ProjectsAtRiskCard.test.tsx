import { screen, within } from "@testing-library/react";
import { asked, json, renderCard, signIn } from "../dashboardTestKit";
import { ProjectsAtRiskCard } from "./ProjectsAtRiskCard";

// Of five projects, three carry the server's At Risk status.
const projects = [
  { id: "p1", name: "Church roof", raisedAmount: 9800, targetAmount: 30000 },
  { id: "p2", name: "Youth centre", raisedAmount: 7200, targetAmount: 25000 },
  {
    id: "p3",
    name: "Choir robes",
    raisedAmount: 3100,
    targetAmount: 8000,
    endDate: null,
  },
];

let broken: boolean;
beforeEach(() => {
  broken = false;
  signIn();
  vi.stubGlobal(
    "fetch",
    vi.fn(async (url: string) => {
      const { path, query } = asked(url);
      if (broken) return json({ title: "Server error", status: 500 }, 500);
      if (path === "/v1/projects/summary")
        return json({ totalProjects: 5, atRiskCount: 3 });
      if (path === "/v1/projects") {
        expect(query.get("status")).toBe("AtRisk");
        return json({ data: projects });
      }
      return json({ title: "Not found", status: 404 }, 404);
    }),
  );
});
afterEach(() => vi.unstubAllGlobals());

it("lists each project at risk, with how much of the money has come in", async () => {
  renderCard(<ProjectsAtRiskCard />);
  await screen.findByText("Church roof");
  const rows = within(screen.getByRole("list"));
  expect(
    rows.getAllByText("GH₵ 9,800 of GH₵ 30,000 raised"),
  ).toHaveLength(1);
  expect(rows.getAllByText("GH₵ 7,200 of GH₵ 25,000 raised")).toHaveLength(
    1,
  );
  expect(rows.getAllByText("GH₵ 3,100 of GH₵ 8,000 raised")).toHaveLength(1);
  expect(rows.getAllByText("At risk")).toHaveLength(3);
});

it("says how many of every project are at risk, and leads to all of them", async () => {
  renderCard(<ProjectsAtRiskCard />);
  expect(await screen.findByText("3 of 5")).toBeInTheDocument();
  expect(
    screen.getByRole("link", { name: /All projects/ }),
  ).toHaveAttribute("href", "/projects");
});

it("says so when nothing is at risk", async () => {
  vi.stubGlobal(
    "fetch",
    vi.fn(async (url: string) => {
      const { path } = asked(url);
      if (broken) return json({ title: "Server error", status: 500 }, 500);
      if (path === "/v1/projects/summary")
        return json({ totalProjects: 5, atRiskCount: 0 });
      if (path === "/v1/projects") return json({ data: [] });
      return json({ title: "Not found", status: 404 }, 404);
    }),
  );
  renderCard(<ProjectsAtRiskCard />);
  expect(
    await screen.findByText("No projects at risk"),
  ).toBeInTheDocument();
  expect(screen.queryByRole("list")).not.toBeInTheDocument();
});

it("says so when the projects could not load, with another try", async () => {
  broken = true;
  renderCard(<ProjectsAtRiskCard />);
  expect(await screen.findByText("Could not load")).toBeInTheDocument();
  expect(screen.getByRole("button", { name: "Try again" })).toBeInTheDocument();
});
