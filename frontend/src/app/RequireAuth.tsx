import { Navigate, Outlet, useLocation } from "react-router-dom";
import { useIsSignedIn } from "@/features/auth";

// Wraps every page that needs a signed-in person. Anyone else goes to /signin, and comes
// back to the page they asked for afterwards.
export function RequireAuth() {
  const signedIn = useIsSignedIn();
  const location = useLocation();
  if (!signedIn) {
    return (
      <Navigate
        to="/signin"
        replace
        state={{ from: location.pathname + location.search }}
      />
    );
  }
  return <Outlet />;
}
