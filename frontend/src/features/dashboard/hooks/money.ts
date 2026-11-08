import { useQuery } from "@tanstack/react-query";
import { getGifts } from "../api/gifts";
import { getTitheFigures } from "../api/tithes";
import { getCategories, getMonths } from "../api/money";
import { useData } from "./internals";

// All the money of a week in one place: transactions, tithes and project gifts.
export function useWeek(from: string, to: string) {
  const categories = useData("categories", from, to, getCategories);
  const tithes = useData("tithe-figures", from, to, getTitheFigures);
  const gifts = useData("gifts", from, to, getGifts);
  return {
    categories: categories.data,
    tithes: tithes.data,
    gifts: gifts.data,
    failed: categories.failed || tithes.failed || gifts.failed,
    retry: () => {
      categories.retry();
      tithes.retry();
      gifts.retry();
    },
  };
}

// The money of each of the last `months` months, up to `to`.
export function useMoneyMonths(months: number, to: string) {
  const query = useQuery({
    queryKey: ["dashboard", "money-months", months, to],
    queryFn: ({ signal }) => getMonths(months, to, signal),
  });
  return {
    months: query.data,
    failed: query.isError,
    retry: () => void query.refetch(),
  };
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
