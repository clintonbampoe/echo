import { zodResolver } from "@hookform/resolvers/zod";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { useSearchParams } from "react-router-dom";
import { z } from "zod";
import { required } from "@/lib/validation";
import { Button, Field, TextAreaField } from "@/ui";
import { Kicker } from "../components/chrome/Kicker";
import { RuleList, type Rule } from "../components/chrome/RuleList";
import { Section } from "../components/chrome/Section";
import { Stop } from "../components/chrome/Stop";
import { TextLink } from "../components/chrome/TextLink";
import { CONTACT } from "../content/pages";
import { CONTACT_EMAIL } from "../content/site";
import styles from "./ContactPage.module.css";

const schema = z.object({
  name: required("Enter your name."),
  church: z.string().trim().optional(),
  email: required("Enter your email address.").pipe(
    z.email("Enter a valid email address."),
  ),
  message: required("Write a message."),
});
type Values = z.infer<typeof schema>;

const SUBJECTS: Record<string, string> = {
  support: "Supporting Echo",
  private: "Private deployment",
};

function PaperPlane() {
  return (
    <svg
      viewBox="0 0 20 20"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.8"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
      className={styles.plane}
    >
      <path d="M17 3L9 11M17 3l-5 14-3-6-6-3z" />
    </svg>
  );
}

// There is no mail service behind this page yet, so sending opens the person's own email app
// with the message already written. Nothing is sent from here.
export function ContactPage() {
  const topic = useSearchParams()[0].get("topic") ?? "";
  const subject = SUBJECTS[topic] ?? "Hello from the Echo website";
  // The paper plane flies once as the message is sent.
  const [flying, setFlying] = useState(false);
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<Values>({ resolver: zodResolver(schema) });

  function onSubmit(v: Values) {
    setFlying(true);
    setTimeout(() => setFlying(false), 800);
    const who = v.church ? `${v.name}, ${v.church}` : v.name;
    const body = `${v.message}\n\n${who}\n${v.email}`;
    window.location.assign(
      `mailto:${CONTACT_EMAIL}?subject=${encodeURIComponent(subject)}&body=${encodeURIComponent(body)}`,
    );
  }

  return (
    <Section top={72}>
      <div className={styles.layout}>
        <div>
          <Kicker>Echo Sanctuary</Kicker>
          <h1 className={styles.title}>
            Talk to us
            <Stop />
          </h1>
          <p className={styles.lead}>{CONTACT.lead}</p>
          <div className={styles.steps}>
            <RuleList rules={CONTACT.steps as readonly Rule[]} />
          </div>
        </div>
        <form
          onSubmit={handleSubmit(onSubmit)}
          noValidate
          className={styles.form}
        >
          <Field
            label="Your name"
            autoComplete="name"
            placeholder="Full name"
            error={errors.name?.message}
            {...register("name")}
          />
          <Field
            label="Church or organisation"
            autoComplete="organization"
            optional
            {...register("church")}
          />
          <Field
            label="Email address"
            type="email"
            autoComplete="email"
            placeholder="you@church.org"
            error={errors.email?.message}
            {...register("email")}
          />
          <TextAreaField
            label="What would you like from Echo?"
            placeholder="Tell us a little about your church and your servers."
            error={errors.message?.message}
            {...register("message")}
          />
          <Button
            type="submit"
            size="lg"
            pill
            block
            className={styles.send}
            data-flying={flying || undefined}
          >
            Send message
            <PaperPlane />
          </Button>
          <p className={styles.note}>
            This opens your email app with the message ready to send. Or write
            to{" "}
            <TextLink to={`mailto:${CONTACT_EMAIL}`} external>
              {CONTACT_EMAIL}
            </TextLink>
            .
          </p>
        </form>
      </div>
    </Section>
  );
}
