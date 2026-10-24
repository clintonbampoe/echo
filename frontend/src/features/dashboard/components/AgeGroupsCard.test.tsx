import { screen, within } from "@testing-library/react";
import { json, asked, renderCard, signIn } from "../dashboardTestKit";
import { AgeGroupsCard } from "./AgeGroupsCard";

// The members by age: five bands in the order the server sends. No one of them outside a band, so the
// shares are 13, 27, 29, 19 and 12 of a rounded hundred.
const mix = {
  totalMembers: 1284,
  maleCount: 692,
  femaleCount: 578,
  averageAge: 38.41,
  ageGroups: [
    { label: "Under 18", count: 168 },
    { label: "18–29", count: 341 },
    { label: "30–44", count: 372 },
    { label: "45–59", count: 245 },
    { label: "60+", count: 158 },
  ],
};

let broken: boolean;
beforeEach(() => {
  broken = false;
  signIn();
  vi.stubGlobal(
    "fetch",
    vi.fn(async (url: string) => {
      const { path } = asked(url);
      if (broken || path !== "/v1/members/summary")
        return json({ title: "Server error", status: 500 }, 500);
      return json(mix);
    }),
  );
});
afterEach(() => vi.unstubAllGlobals());

it("says how the ages fall, in its own words, with each count and share", async () => {
  renderCard(<AgeGroupsCard />);
  await screen.findByText("18 to 29");
  const rows = within(
    screen.getByRole("list", { name: "Members by age group" }),
  );
  expect(rows.getByText("Under 18")).toBeInTheDocument();
  expect(rows.getByText("168")).toBeInTheDocument();
  expect(rows.getByText("13%")).toBeInTheDocument();
  expect(rows.getByText("372")).toBeInTheDocument();
  expect(rows.getByText("29%")).toBeInTheDocument();
  expect(rows.getByText("158")).toBeInTheDocument();
  expect(rows.getByText("12%")).toBeInTheDocument();
});

it("says the average age in the foot", async () => {
  renderCard(<AgeGroupsCard />);
  expect(await screen.findByText("Average age 38")).toBeInTheDocument();
  expect(screen.getByText("See members")).toHaveAttribute("href", "/members");
});

it("says nothing about the average for a church without members", async () => {
  vi.stubGlobal(
    "fetch",
    vi.fn(async (url: string) => {
      const { path } = asked(url);
      if (broken || path !== "/v1/members/summary")
        return json({ title: "Server error", status: 500 }, 500);
      return json({
        ...mix,
        totalMembers: 0,
        averageAge: 0,
        ageGroups: mix.ageGroups.map((b) => ({ ...b, count: 0 })),
      });
    }),
  );
  renderCard(<AgeGroupsCard />);
  await screen.findByText("No members yet");
  expect(screen.queryByText(/Average age/)).not.toBeInTheDocument();
  expect(
    screen.queryByRole("list", { name: "Members by age group" }),
  ).not.toBeInTheDocument();
});

it("says so when the ages could not load, and offers another try", async () => {
  broken = true;
  renderCard(<AgeGroupsCard />);
  expect(await screen.findByText("Could not load")).toBeInTheDocument();
  expect(screen.getByRole("button", { name: "Try again" })).toBeInTheDocument();
});
