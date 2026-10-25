import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm, useWatch } from "react-hook-form";
import type { MemberMatch } from "@/features/members";
import type { ApiError } from "@/lib/api";
import { handleSubmitError } from "@/lib/forms";
import { formatMoney } from "@/lib/format";
import { FormSheet } from "@/ui";
import type { Tithe } from "../../api/tithes";
import { useCreateTithe } from "../../hooks/mutations";
import { useSameMonthTithes } from "../../hooks/queries";
import {
  emptyTithe,
  nextTithe,
  titheFieldMap,
  titheSchema,
  toTitheInput,
  type TitheValues,
} from "../../forms/schema";
import { TitheFields } from "./TitheFields";
import { SameMonthNotice } from "./SameMonthNotice";
import { Sitting, type Entry } from "../profile/Sitting";

type Props = {
  open: boolean;
  today: string;
  onClose: () => void;
  onUpdate: (tithe: Tithe) => void;
};

// The "Record tithe" sheet. The form only exists while the sheet is open, so it starts fresh every time.
export function TitheSheet({ open, today, onClose, onUpdate }: Props) {
  return open ? (
    <RecordTithe today={today} onClose={onClose} onUpdate={onUpdate} />
  ) : null;
}

function RecordTithe({ today, onClose, onUpdate }: Omit<Props, "open">) {
  const create = useCreateTithe();
  const [member, setMember] = useState<MemberMatch | null>(null);
  const [problem, setProblem] = useState<ApiError | null>(null);
  const [sitting, setSitting] = useState<Entry[]>([]);
  const form = useForm<TitheValues>({
    resolver: zodResolver(titheSchema(today)),
    defaultValues: emptyTithe(today),
  });

  const [month, year] = useWatch({
    control: form.control,
    name: ["forMonth", "forYear"],
  });
  // Not while saving: the list that now holds the tithe being saved is not a clash.
  const clash = useSameMonthTithes(member?.id, Number(year), month).data
    ?.data[0];
  const existing = create.isPending ? undefined : clash;

  function choose(next: MemberMatch | null) {
    setMember(next);
    form.setValue("memberId", next?.id ?? "", {
      shouldValidate: form.formState.isSubmitted,
    });
  }

  async function save(values: TitheValues, another: boolean) {
    setProblem(null);
    try {
      const saved = await create.mutateAsync(toTitheInput(values));
      if (!another) return onClose();
      setSitting([
        ...sitting,
        { id: saved.id, name: member?.name ?? "", amount: saved.amount },
      ]);
      form.reset(nextTithe(values));
      setMember(null);
    } catch (e) {
      handleSubmitError(e, form.setError, titheFieldMap, setProblem);
    }
  }

  const last = sitting[sitting.length - 1];
  return (
    <FormSheet
      title="Record tithe"
      subtitle="One tithe from one member."
      onClose={onClose}
      formId="tithe-form"
      onSubmit={form.handleSubmit((v) => save(v, false))}
      submitLabel="Save tithe"
      pending={create.isPending}
      blocked={Boolean(member && existing)}
      failures={Object.keys(form.formState.errors).length}
      note={
        last &&
        `Recorded ${formatMoney(last.amount)} for ${last.name}. Date and method are kept for the next one.`
      }
      problem={
        problem && {
          title: "We could not record this tithe",
          message: problem.message,
        }
      }
      another={{
        label: "Save and add another",
        onClick: () => void form.handleSubmit((v) => save(v, true))(),
      }}
    >
      <Sitting entries={sitting} />
      <TitheFields
        key={sitting.length}
        form={form}
        today={today}
        member={member}
        onMember={choose}
        underMember={
          member &&
          existing && (
            <SameMonthNotice
              memberName={member.name}
              period={`${month} ${year}`}
              tithe={existing}
              onUpdate={onUpdate}
            />
          )
        }
      />
    </FormSheet>
  );
}
