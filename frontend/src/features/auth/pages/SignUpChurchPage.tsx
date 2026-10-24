import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { Navigate, useNavigate } from "react-router-dom";
import type { ApiError } from "@/lib/api";
import { handleSubmitError } from "@/lib/forms";
import { Alert, Button } from "@/ui";
import { BackLink } from "../components/AuthLinks";
import { AuthScreen } from "../components/AuthScreen";
import { useRegisterCongregation } from "../hooks";
import { churchSchema, type AccountValues, type ChurchValues } from "../schema";
import { ChurchFields } from "./ChurchFields";
import { useSignUp } from "./signUpContext";
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
  const [problem, setProblem] = useState<ApiError | null>(null);
  const form = useForm<ChurchValues>({
    resolver: zodResolver(churchSchema),
    defaultValues: { orgType: "Church", region: "GreaterAccra" },
  });

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
      navigate("/verify", { state: { email: account.email } });
    } catch (e) {
      handleSubmitError(e, form.setError, FIELD_MAP, setProblem);
    }
  }

  return (
    <AuthScreen
      title="Tell us about your church"
      text="Step 2 of 2. You can change all of this later."
      links={<BackLink to="/signup">Back</BackLink>}
    >
      {problem && (
        <Alert title="We could not create your account">
          {problem.message}
        </Alert>
      )}
      <form
        onSubmit={form.handleSubmit(onSubmit)}
        noValidate
        className={styles.form}
      >
        <ChurchFields form={form} />
        <Button
          type="submit"
          size="lg"
          pill
          block
          loading={register.isPending}
          className={styles.submit}
        >
          Create church
        </Button>
      </form>
    </AuthScreen>
  );
}
