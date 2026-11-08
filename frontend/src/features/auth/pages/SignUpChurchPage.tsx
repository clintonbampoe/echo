import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { Navigate, useNavigate } from "react-router-dom";
import type { ApiError } from "@/lib/api";
import { handleSubmitError } from "@/lib/forms";
import { Alert, useMedia } from "@/ui";
import { AuthLinks } from "../components/AuthLinks";
import { AuthScreen } from "../components/AuthScreen";
import {
  useRegisterCongregation,
  useSignInAfterRegister,
} from "../hooks/mutations";
import {
  churchSchema,
  type AccountValues,
  type ChurchValues,
} from "../forms/schema";
import { ChurchFields } from "./ChurchFields";
import { ChurchButtons } from "../components/ChurchButtons";
import { CHURCH_PARTS } from "../model/churchParts";
import { useSignUp } from "../hooks/signUp";
import styles from "./form.module.css";

// Where the API's field paths land on this form.
const FIELD_MAP = {
  "congregationDto.name": "name",
  "congregationDto.orgType": "orgType",
  "congregationDto.phoneNumber": "phoneNumber",
  "congregationDto.emailAddress": "churchEmail",
  "congregationDto.region": "region",
  "congregationDto.city": "city",
  "congregationDto.town": "town",
  "congregationDto.gpsAddress": "gpsAddress",
} as const;

export function SignUpChurchPage() {
  const { account } = useSignUp();
  // Someone who opened this step directly (or reloaded) has no step 1 to go with it.
  if (!account) return <Navigate to="/signup" replace />;
  return <ChurchForm account={account} />;
}

function ChurchForm({ account }: { account: AccountValues }) {
  const navigate = useNavigate();
  const register = useRegisterCongregation();
  const signInAfterRegister = useSignInAfterRegister();
  const [problem, setProblem] = useState<ApiError | null>(null);
  const form = useForm<ChurchValues>({
    resolver: zodResolver(churchSchema),
    defaultValues: { orgType: "Church", region: "GreaterAccra" },
  });

  // On a phone the form is three short parts, one at a time.
  const phone = useMedia("(max-width: 599px)");
  const [part, setPart] = useState(0);
  const last = CHURCH_PARTS.length - 1;
  const partial = phone && part < last;

  async function next() {
    const ok = await form.trigger(CHURCH_PARTS[part]);
    if (ok) setPart(part + 1);
  }

  async function onSubmit(v: ChurchValues) {
    setProblem(null);
    try {
      await register.mutateAsync({
        congregationDto: {
          name: v.name,
          orgType: v.orgType,
          phoneNumber: v.phoneNumber,
          emailAddress: v.churchEmail,
          region: v.region,
          city: v.city,
          town: v.town,
          gpsAddress: v.gpsAddress,
        },
        userDto: {
          firstName: account.firstName,
          lastName: account.lastName,
          emailAddress: account.email,
          password: account.password,
        },
      });
      const signedIn = await signInAfterRegister(
        account.email,
        account.password,
      );
      navigate("/verify", { state: { email: account.email, signedIn } });
    } catch (e) {
      handleSubmitError(e, form.setError, FIELD_MAP, setProblem);
      // If the API objected to something in an earlier part, take them back to it.
      const bad = CHURCH_PARTS.findIndex((names) =>
        names.some((n) => form.getFieldState(n).error),
      );
      if (bad >= 0) setPart(bad);
    }
  }

  return (
    <AuthScreen
      title="Tell us about your church"
      text={
        phone
          ? `Step 2 of 2, part ${part + 1} of ${CHURCH_PARTS.length}. You can change all of this later.`
          : "Step 2 of 2. You can change all of this later."
      }
      links={
        <AuthLinks
          heading="Need to step back?"
          items={[
            { to: "/signup", label: phone ? "Back to step 1" : "Back" },
            { to: "/signin", label: "Sign in" },
          ]}
        />
      }
    >
      {problem && (
        <Alert title="We could not create your account">
          {problem.message}
        </Alert>
      )}
      <form
        onSubmit={
          partial
            ? (e) => {
                e.preventDefault();
                void next();
              }
            : form.handleSubmit(onSubmit)
        }
        noValidate
        className={`${styles.form} stagger`}
      >
        <ChurchFields form={form} part={phone ? part : undefined} />
        <ChurchButtons
          part={part}
          partial={partial}
          phone={phone}
          loading={register.isPending}
          onBack={() => setPart(part - 1)}
        />
      </form>
    </AuthScreen>
  );
}
