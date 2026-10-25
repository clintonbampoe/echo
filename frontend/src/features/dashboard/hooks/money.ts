import { getPresent } from "../api/attendance";
import { getCategories, getMonths } from "../api/money";
import { useData } from "./internals";

// What came in, what went out and how many came, over a week.
export function useWeek(from: string, to: string) {
  const categories = useData("categories", from, to, getCategories);
  const present = useData("present", from, to, getPresent);
  return {
    categories: categories.data,
    present: present.data,
    failed: categories.failed || present.failed,
    retry: () => {
      categories.retry();
      present.retry();
    },
  };
}

// The money of each month in the range.
export function useMoneyMonths(from: string, to: string) {
  const { data, failed, retry } = useData("months", from, to, getMonths);
  return { months: data, failed, retry };
}

// What each category added up to in the range.
export function useCategories(from: string, to: string) {
  const { data, failed, retry } = useData(
    "categories",
    from,
    to,
    getCategories,
  );
  return { categories: data, failed, retry };
}
