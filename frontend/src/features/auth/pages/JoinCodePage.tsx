import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router-dom";
import { isApiError } from "@/lib/api";
import { Alert, Button, Field } from "@/ui";
import { AuthHeading } from "../components/AuthHeading";
import { BackLink } from "../components/AuthLinks";
import { useCheckInvitation } from "../hooks";
import { codeSchema, type CodeValues } from "../schema";
import styles from "./form.module.css";

// "Enter your invitation code". The code is checked here, so a typo is caught on this screen.
export function JoinCodePage() {
  const navigate = useNavigate();
  const check = useCheckInvitation();
  const [problem, setProblem] = useState<"missing" | "other" | null>(null);
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<CodeValues>({ resolver: zodResolver(codeSchema) });

  async function onSubmit({ code }: CodeValues) {
    setProblem(null);
    try {
      await check(code);
      navigate(`/join/${encodeURIComponent(code)}`);
    } catch (e) {
      setProblem(isApiError(e) && e.status === 404 ? "missing" : "other");
    }
  }

  return (
    <>
      <AuthHeading icon="link" title="Enter your invitation code">
        Your church administrator gave you this code. If you were sent a link,
        just open it instead.
      </AuthHeading>
      {problem === "missing" && (
        <div className={styles.alert}>
          <Alert title="We could not find that invitation">
            Check the code and try again. If it keeps failing, ask your
            administrator for a new one.
          </Alert>
        </div>
      )}
      {problem === "other" && (
        <div className={styles.alert}>
          <Alert title="We could not check that code">
            Check your connection and try again.
          </Alert>
        </div>
      )}
      <form
        onSubmit={handleSubmit(onSubmit)}
        noValidate
        className={styles.form}
      >
        <Field
          label="Invitation code"
          autoComplete="off"
          maxLength={512}
          placeholder="Paste your code"
          error={errors.code?.message}
          {...register("code")}
        />
        <Button type="submit" size="lg" block loading={isSubmitting}>
          Continue
        </Button>
      </form>
      <BackLink />
    </>
  );
}
