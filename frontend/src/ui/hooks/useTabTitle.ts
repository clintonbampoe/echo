import { useEffect } from "react";

// Puts a title on the browser tab while a screen is open, and puts the old one back afterwards.
// A person who switches away to open their email can see from the tab when the page is done.
export function useTabTitle(title: string) {
  useEffect(() => {
    const before = document.title;
    document.title = title;
    return () => {
      document.title = before;
    };
  }, [title]);
}
