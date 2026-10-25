import { useState } from "react";
import type { MemberMatch } from "@/features/members";
import { nowTime, todayText } from "@/lib/dates";
import { useCheckInMember, useRegisterMember } from "./mutations";
import { recordProblem, type Mode, type Problem } from "../model/record";

// Records people one at a time as they are tapped. Remembers who was recorded from this panel (so their row can say so
// at once, before the lists catch up), which person is being saved, and what went wrong.
export function useRecorder(eventId: string) {
  const register = useRegisterMember();
  const checkIn = useCheckInMember();
  const [done, setDone] = useState<Record<Mode, string[]>>({
    register: [],
    "check-in": [],
  });
  const [busy, setBusy] = useState<string | null>(null);
  const [problem, setProblem] = useState<Problem | null>(null);

  async function record(mode: Mode, member: MemberMatch) {
    setProblem(null);
    setBusy(member.id);
    try {
      if (mode === "register")
        await register.mutateAsync({
          memberId: member.id,
          eventId,
          registrationDate: todayText(),
        });
      else
        await checkIn.mutateAsync({
          memberId: member.id,
          eventId,
          checkInTime: nowTime(),
        });
      setDone((d) => ({ ...d, [mode]: [...d[mode], member.id] }));
    } catch (e) {
      setProblem(recordProblem(mode, member.name, e));
    } finally {
      setBusy(null);
    }
  }

  return { done, busy, problem, record };
}
