import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { useNavigate, useParams } from "react-router-dom";
import type { ApiError } from "@/lib/api";
import { handleSubmitError } from "@/lib/forms";
import { Alert, Button, Field, LinkButton, PasswordField } from "@/ui";
import { AuthHeading } from "../components/AuthHeading";
import { AuthPrompt } from "../components/AuthLinks";
import { InvitationCard } from "../components/InvitationCard";
import { StatusScreen } from "../components/StatusScreen";
import type { InvitationInfo } from "../api";
import { useInvitation, useRegisterMember } from "../hooks";
import { joinSchema, type JoinValues } from "../schema";
import styles from "./form.module.css";

const FIELD_MAP = {
  "userInfo.firstName": "firstName",
  "userInfo.lastName": "lastName",
  "userInfo.otherNames": "otherNames",
  "userInfo.emailAddress": "email",
  "userInfo.password": "password",
} as const;

const invalid = (
  <StatusScreen
    icon="alert"
    title="This invitation is no longer valid"
    text="Invitation links expire, and each one works once. Ask your church administrator to send you a new one."
  >
    <LinkButton to="/signin" size="lg" block>
      Back to sign in
    </LinkButton>
  </StatusScreen>
);

// /join/:code. Shows which church the code is for, then asks for the person's details.
export function JoinPage() {
  const code = useParams().code ?? "";
  const invitation = useInvitation(code);
  if (invitation.isPending)
    return (
      <StatusScreen
        icon="spinner"
        title="Checking your invitation"
        text="This only takes a moment."
      />
    );
  if (invitation.isError) return invalid;
  return <JoinForm code={code} invitation={invitation.data} />;
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

  if (problem?.errorCode === "INVALID_TOKEN") return invalid;

  return (
    <>
      <AuthHeading title="Create your account">
        You have been invited to join:
      </AuthHeading>
      <InvitationCard invitation={invitation} />
      <AuthPrompt to="/join" action="Use a different code">
        Not your church?
      </AuthPrompt>
      {problem && (
        <div className={styles.alert}>
          <JoinProblem problem={problem} />
        </div>
      )}
      <form
        onSubmit={form.handleSubmit(onSubmit)}
        noValidate
        className={`${styles.form} ${styles.tight}`}
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
          block
          loading={register.isPending}
          className={styles.submit}
        >
          Create account
        </Button>
      </form>
      <AuthPrompt to="/signin" action="Sign in">
        Already have an account?
      </AuthPrompt>
    </>
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
