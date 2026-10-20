import { useState } from "react";
import { Pager } from "@/ui";
import {
  DEFAULT_MONTHS,
  MONTH_RANGES,
  monthChoices,
  type MonthRange,
} from "../money";
import { CategoryCard } from "./CategoryCard";
import { IncomeExpensesCard } from "./IncomeExpensesCard";
import { RangeTabs } from "./RangeTabs";

// Income and expenses beside where the money came from. One range for both, picked on the first card.
export function MoneyRow({ today }: { today: string }) {
  const [months, setMonths] = useState<MonthRange>(DEFAULT_MONTHS);
  const pick = (value: string) =>
    setMonths(MONTH_RANGES.find((m) => String(m) === value) ?? DEFAULT_MONTHS);
  return (
    <Pager
      label="Money"
      titles={["Income and expenses", "By category"]}
      columns="3fr 2fr"
    >
      <IncomeExpensesCard
        today={today}
        months={months}
        control={
          <RangeTabs
            label="Range"
            value={String(months)}
            options={monthChoices}
            onChange={pick}
          />
        }
      />
      <CategoryCard today={today} months={months} />
    </Pager>
  );
}
