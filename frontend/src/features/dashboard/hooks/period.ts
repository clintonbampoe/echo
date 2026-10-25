import { useState } from "react";
import { DEFAULT_PERIOD, isPeriodKey, type PeriodKey } from "../model/periods";

const STORAGE_KEY = "echo.dashboard.period";

function stored(): PeriodKey {
  try {
    const value = localStorage.getItem(STORAGE_KEY);
    return isPeriodKey(value) ? value : DEFAULT_PERIOD;
  } catch {
    return DEFAULT_PERIOD;
  }
}

// The period picked on the dashboard, remembered in this browser for the next visit.
export function usePeriod(): [PeriodKey, (key: PeriodKey) => void] {
  const [period, setPeriod] = useState<PeriodKey>(stored);
  const pick = (key: PeriodKey) => {
    setPeriod(key);
    try {
      localStorage.setItem(STORAGE_KEY, key);
    } catch {
      // Storage is blocked (private window): the pick still works, it just is not remembered.
    }
  };
  return [period, pick];
}
