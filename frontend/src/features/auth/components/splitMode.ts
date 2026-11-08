import { createContext, useContext } from "react";

// Wide screens put the form on the left and the other places to go in a pane on the right.
// Tablet and phone show less: the form and one link. AuthLayout decides; the pieces read it.
export const SplitContext = createContext(false);
export const SPLIT_QUERY = "(min-width: 1000px)";
export const useSplit = () => useContext(SplitContext);
