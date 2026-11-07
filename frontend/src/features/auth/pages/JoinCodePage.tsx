import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router-dom";
import { isApiError } from "@/lib/api";
import { Alert, Button, Field } from "@/ui";
import { AuthLinks } from "../components/AuthLinks";
import { AuthScreen } from "../components/AuthScreen";
import { useCheckInvitation } from "../hooks/mutations";
import { codeSchema, type CodeValues } from "../forms/schema";
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
    <AuthScreen
      title="Enter your invitation code"
      text="Your church leader sent you a link or a code."
      links={
        <AuthLinks
          heading="No code yet?"
          items={[
            { to: "/signin", label: "Back to sign in" },
            { to: "/signup", label: "Create a church account" },
          ]}
        />
      }
    >
      {problem === "missing" && (
        <Alert title="We could not find that invitation">
          Check the code and try again. If it keeps failing, ask your
          administrator for a new one.
        </Alert>
      )}
      {problem === "other" && (
        <Alert title="We could not check that code">
          Check your connection and try again.
        </Alert>
      )}
      <form
        onSubmit={handleSubmit(onSubmit)}
        noValidate
        className={`${styles.form} stagger`}
      >
        <Field
          label="Invitation code"
          autoComplete="off"
          maxLength={512}
          placeholder="Paste your code"
          error={errors.code?.message}
          {...register("code")}
        />
        <Button
          type="submit"
          size="lg"
          pill
          block
          loading={isSubmitting}
          className={styles.submit}
        >
          Continue
        </Button>
      </form>
    </AuthScreen>
  );
}
