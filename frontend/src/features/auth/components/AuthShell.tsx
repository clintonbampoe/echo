import { useState } from "react";
import { Outlet } from "react-router-dom";
import type { SignUpContext } from "../pages/signUpContext";
import type { AccountValues } from "../forms/schema";
import { AuthLayout } from "./AuthLayout";

// The route-level frame for every auth page. It stays mounted while the person moves
// between pages, so the light on the left does not restart on each step. For the same
// reason it also holds sign-up step 1 while step 2 is filled in: AuthLayout remounts each
// page (to replay its entrance), so anything stored lower down would be lost on the way.
export function AuthShell() {
  const [account, setAccount] = useState<AccountValues | null>(null);
  return (
    <AuthLayout>
      <Outlet context={{ account, setAccount } satisfies SignUpContext} />
    </AuthLayout>
  );
}
