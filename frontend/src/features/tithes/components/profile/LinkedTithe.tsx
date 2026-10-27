import { useState } from "react";
import { useSearchParams } from "react-router-dom";
import { Alert, Button } from "@/ui";
import { useTithe } from "../../hooks/queries";
import { TitheDialogs } from "../dialogs/TitheDialogs";

// A link to one tithe (?tithe=id, from a member's page) opens it to read. The address is tidied once it is closed.
export function LinkedTithe({ today }: { today: string }) {
  const [params, setParams] = useSearchParams();
  const [id, setId] = useState(() => params.get("tithe"));
  const close = () => {
    setId(null);
    setParams(
      (p) => {
        p.delete("tithe");
        return p;
      },
      { replace: true },
    );
  };
  return id ? <OpenTithe id={id} today={today} onClose={close} /> : null;
}

function OpenTithe({
  id,
  today,
  onClose,
}: {
  id: string;
  today: string;
  onClose: () => void;
}) {
  const { data, isError } = useTithe(id);
  if (isError) {
    return (
      <Alert
        title="We could not open that tithe"
        action={
          <Button variant="secondary" size="sm" onClick={onClose}>
            Dismiss
          </Button>
        }
      >
        It may have been deleted. Check your connection if it should still be
        there.
      </Alert>
    );
  }
  return data ? (
    <TitheDialogs tithe={data} start="view" today={today} onClose={onClose} />
  ) : null;
}
