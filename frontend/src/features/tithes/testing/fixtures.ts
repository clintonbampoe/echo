import type { Tithe } from "../api/tithes";

export const aTithe = (over: Partial<Tithe> = {}): Tithe => ({
  id: "t1",
  memberId: "m1",
  memberName: "Kofi Mensah",
  amount: 500,
  forYear: 2026,
  forMonth: "October",
  paymentMethod: "Cash",
  collectionDate: "2026-10-11",
  createdAt: "",
  ...over,
});
