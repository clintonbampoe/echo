import { useState, type ReactNode } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router-dom";
import { isApiError } from "@/lib/api";
import { Button, Field } from "@/ui";
import { useSendVerification } from "../hooks";
import { emailOnlySchema, type EmailOnlyValues } from "../schema";
import { StatusScreen } from "./StatusScreen";
import styles from "./LinkExpired.module.css";

type Props = { title: string; text: string; children?: ReactNode };

// "This link has expired. Enter your email and we will send a new one."
export function LinkExpired({ title, text, children }: Props) {
  const navigate = useNavigate();
  const send = useSendVerification();
  const [problem, setProblem] = useState("");
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<EmailOnlyValues>({ resolver: zodResolver(emailOnlySchema) });

  async function onSubmit({ email }: EmailOnlyValues) {
    setProblem("");
    try {
      await send.mutateAsync(email);
      navigate("/verify", { state: { email } });
    } catch (e) {
      setProblem(
        isApiError(e) ? e.message : "Something went wrong. Try again.",
      );
    }
  }

  return (
    <StatusScreen icon="clock" title={title} text={text}>
      <form onSubmit={handleSubmit(onSubmit)} noValidate>
        <Field
          label="Email address"
          type="email"
          autoComplete="email"
          placeholder="you@yourchurch.org"
          error={errors.email?.message ?? (problem || undefined)}
          {...register("email")}
        />
        <Button
          type="submit"
          size="lg"
          block
          loading={send.isPending}
          className={styles.submit}
        >
          Send a new link
        </Button>
      </form>
      {children}
    </StatusScreen>
  );
}
