import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { useNavigate, useParams } from "react-router-dom";
import type { ApiError } from "@/lib/api";
import { handleSubmitError } from "@/lib/forms";
import { Alert, Button, Field, PasswordField } from "@/ui";
import { AuthLinks } from "../components/AuthLinks";
import { AuthScreen } from "../components/AuthScreen";
import {
  InvalidInvitation,
  InvitationGate,
} from "../components/InvitationGate";
import type { InvitationInfo } from "../api";
import { useRegisterMember } from "../hooks";
import { joinSchema, type JoinValues } from "../schema";
import styles from "./form.module.css";

const FIELD_MAP = {
  "userInfo.firstName": "firstName",
  "userInfo.lastName": "lastName",
  "userInfo.otherNames": "otherNames",
  "userInfo.emailAddress": "email",
  "userInfo.password": "password",
} as const;

// /join/:code/account. The invitation is confirmed; now ask for the person's details.
export function JoinPage() {
  const code = useParams().code ?? "";
  return (
    <InvitationGate code={code}>
      {(invitation) => <JoinForm code={code} invitation={invitation} />}
    </InvitationGate>
  );
}

function JoinForm({
  code,
  invitation,
}: {
  code: string;
  invitation: InvitationInfo;
}) {
  const navigate = useNavigate();
  const register = useRegisterMember();
  const [problem, setProblem] = useState<ApiError | null>(null);
  const form = useForm<JoinValues>({ resolver: zodResolver(joinSchema) });
  const { errors } = form.formState;

  async function onSubmit(v: JoinValues) {
    setProblem(null);
    try {
      await register.mutateAsync({
        token: code,
        userInfo: {
          firstName: v.firstName,
          lastName: v.lastName,
          otherNames: v.otherNames || undefined,
          emailAddress: v.email,
          password: v.password,
        },
      });
      navigate("/verify", { state: { email: v.email } });
    } catch (e) {
      handleSubmitError(e, form.setError, FIELD_MAP, setProblem);
    }
  }

  if (problem?.errorCode === "INVALID_TOKEN") return <InvalidInvitation />;

  return (
    <AuthScreen
      title="Create your account"
      text={
        invitation.invitedBy
          ? `${invitation.invitedBy} invited you to join ${invitation.congregationName} on Echo.`
          : `You were invited to join ${invitation.congregationName} on Echo.`
      }
      links={<AuthLinks items={[{ to: "/signin", label: "Sign in" }]} />}
    >
      {problem && <JoinProblem problem={problem} />}
      <form
        onSubmit={form.handleSubmit(onSubmit)}
        noValidate
        className={styles.form}
      >
        <Field
          label="First name"
          autoComplete="given-name"
          error={errors.firstName?.message}
          {...form.register("firstName")}
        />
        <Field
          label="Last name"
          autoComplete="family-name"
          error={errors.lastName?.message}
          {...form.register("lastName")}
        />
        <Field
          label="Other names (optional)"
          autoComplete="additional-name"
          error={errors.otherNames?.message}
          {...form.register("otherNames")}
        />
        <Field
          label="Email address"
          type="email"
          autoComplete="email"
          placeholder="you@example.com"
          hint="We will send a verification link here."
          error={errors.email?.message}
          {...form.register("email")}
        />
        <PasswordField
          label="Password"
          autoComplete="new-password"
          hint="At least 8 characters."
          error={errors.password?.message}
          {...form.register("password")}
        />
        <Button
          type="submit"
          size="lg"
          pill
          block
          loading={register.isPending}
          className={styles.submit}
        >
          Join {invitation.congregationName}
        </Button>
      </form>
    </AuthScreen>
  );
}

function JoinProblem({ problem }: { problem: ApiError }) {
  if (problem.status === 409) {
    return (
      <Alert title="This email already has an account">
        Sign in instead, or use a different email address.
      </Alert>
    );
  }
  return (
    <Alert title="We could not create your account">{problem.message}</Alert>
  );
}
