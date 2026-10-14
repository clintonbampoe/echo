import { useMedia } from "./useMedia";

// Below this width the details no longer fit beside the history, so a profile page stacks.
export const useNarrowProfile = () => useMedia("(max-width: 1180px)");
