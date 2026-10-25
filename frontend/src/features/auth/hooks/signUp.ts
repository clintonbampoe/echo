import { useOutletContext } from "react-router-dom";
import type { AccountValues } from "../forms/schema";

export type SignUpContext = {
  account: AccountValues | null;
  setAccount: (account: AccountValues) => void;
};

export function useSignUp() {
  return useOutletContext<SignUpContext>();
}
