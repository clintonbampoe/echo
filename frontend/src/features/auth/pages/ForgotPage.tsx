import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { useLocation } from "react-router-dom";
import { isApiError } from "@/lib/api";
import { Button, Field } from "@/ui";
import { forgotPassword } from "../api/auth";
import { AuthLinks } from "../components/AuthLinks";
import { AuthScreen } from "../components/AuthScreen";
import { ResendButton } from "../components/ResendButton";
import { useForgotPassword } from "../hooks/mutations";
import { emailOnlySchema, type EmailOnlyValues } from "../forms/schema";
import styles from "./form.module.css";

function ForgotLinks() {
  return (
    <AuthLinks
      heading="Remembered it?"
      items={[
        { to: "/signin", label: "Back to sign in" },
        { to: "/signup", label: "Create a church account" },
      ]}
    />
  );
}

export function ForgotPage() {
  const forgot = useForgotPassword();
  // Someone who came from the sign-in page has already typed their address; do not make them do it twice.
  const carried = (useLocation().state as { email?: unknown } | null)?.email;
  const email = typeof carried === "string" ? carried : "";
  const [sentTo, setSentTo] = useState("");
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors },
  } = useForm<EmailOnlyValues>({
    resolver: zodResolver(emailOnlySchema),
    defaultValues: { email },
  });

  async function onSubmit({ email }: EmailOnlyValues) {
    try {
      await forgot.mutateAsync(email);
      setSentTo(email);
    } catch (e) {
      setError("email", {
        message: isApiError(e) ? e.message : "Something went wrong. Try again.",
      });
    }
  }

  // The API answers the same whether or not the address has an account, so we do too.
  if (sentTo) {
    return (
      <AuthScreen
        title="Check your inbox"
        text={
          <>
            If an account exists for <strong>{sentTo}</strong>, a reset link is
            on its way. It expires in 1 hour.
          </>
        }
        links={<ForgotLinks />}
      >
        <ResendButton send={() => forgotPassword(sentTo)} />
        <p className={styles.note}>
          Nothing in your inbox? Check spam, or{" "}
          <button
            type="button"
            className={styles.inline}
            onClick={() => setSentTo("")}
          >
            try a different email
          </button>
          .
        </p>
      </AuthScreen>
    );
  }

  return (
    <AuthScreen
      title="Forgot your password?"
      text="Enter your email and we will send a link to reset it."
      links={
        <AuthLinks
          heading="Remembered it?"
          items={[
            { to: "/signin", label: "Back to sign in" },
            { to: "/signup", label: "Create a church account" },
          ]}
        />
      }
    >
      <form
        onSubmit={handleSubmit(onSubmit)}
        noValidate
        className={`${styles.form} stagger`}
      >
        <Field
          label="Email address"
          type="email"
          autoComplete="email"
          placeholder="you@yourchurch.org"
          hint={email ? "From the sign-in page" : undefined}
          error={errors.email?.message}
          {...register("email")}
        />
        <Button
          type="submit"
          size="lg"
          pill
          block
          loading={forgot.isPending}
          className={styles.submit}
        >
          Send reset link
        </Button>
      </form>
    </AuthScreen>
  );
}
