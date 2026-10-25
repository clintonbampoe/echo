import { createContext, useContext } from "react";

// True while a <Presence> is playing its children's exit animation. Sheets and dialogs read it.
export const LeavingContext = createContext(false);
export const useLeaving = () => useContext(LeavingContext);
