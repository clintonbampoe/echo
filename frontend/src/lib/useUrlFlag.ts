import { useSearchParams } from "react-router-dom";

// A yes-or-no kept in the address ("?new=1"), so a link from another page can open a sheet that this page owns
// (the dashboard's "New member"). Setting it changes the address without adding a step to the back button.
export function useUrlFlag(name: string): [boolean, (on: boolean) => void] {
  const [params, setParams] = useSearchParams();
  const on = params.get(name) === "1";
  const set = (value: boolean) =>
    setParams(
      (current) => {
        const next = new URLSearchParams(current);
        if (value) next.set(name, "1");
        else next.delete(name);
        return next;
      },
      { replace: true },
    );
  return [on, set];
}
