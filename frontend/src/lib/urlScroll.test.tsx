import { act, render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import {
  createMemoryRouter,
  RouterProvider,
  ScrollRestoration,
} from "react-router-dom";
import { useUrlFilters } from "./useUrlFilters";
import { urlFilters } from "./urlFilters";
import { useUrlFlag } from "./useUrlFlag";

const filters = urlFilters({ name: "" });

// The app resets scroll on every page change (ScrollRestoration). A change that only edits the address's query
// string (a filter, a sheet's flag) is the same page and must leave the scroll where it is.
function Page() {
  const [, setFlag] = useUrlFlag("new");
  const { update } = useUrlFilters(filters);
  return (
    <>
      <button onClick={() => setFlag(true)}>flag</button>
      <button onClick={() => update({ name: "ama" })}>filter</button>
      <ScrollRestoration />
    </>
  );
}

describe("query-string changes", () => {
  it("do not scroll the page to the top", async () => {
    const scrollTo = vi.spyOn(window, "scrollTo");
    const router = createMemoryRouter([{ path: "/", element: <Page /> }]);
    render(<RouterProvider router={router} />);
    await act(async () => {});
    scrollTo.mockClear();

    await userEvent.click(screen.getByText("flag"));
    await userEvent.click(screen.getByText("filter"));

    expect(router.state.location.search).toContain("name=ama");
    expect(scrollTo).not.toHaveBeenCalled();
  });
});
