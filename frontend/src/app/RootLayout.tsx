import { Outlet } from "react-router-dom";
import { NavProgress } from "./NavProgress";

// The layer under every page, public or signed in: the bar that shows while the next page loads.
export function RootLayout() {
  return (
    <>
      <NavProgress />
      <Outlet />
    </>
  );
}
