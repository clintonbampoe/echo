import { ChurchHeader } from "./ChurchHeader";
import { NavList } from "./NavList";
import { UserMenu } from "./UserMenu";

type Props = { onNavigate?: () => void };

// Everything inside the sidebar. The drawer on a phone shows exactly the same thing.
export function SidebarBody({ onNavigate }: Props) {
  return (
    <>
      <ChurchHeader />
      <NavList onNavigate={onNavigate} />
      <UserMenu />
    </>
  );
}
