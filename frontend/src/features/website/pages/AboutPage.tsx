import { LinkRows } from "../components/chrome/LinkRows";
import { Intro } from "../components/chrome/Intro";
import { RuleList } from "../components/chrome/RuleList";
import { Split } from "../components/chrome/Split";
import { TextLink } from "../components/chrome/TextLink";
import { ABOUT } from "../content/pages";
import { GITHUB_URL } from "../content/site";
import styles from "./AboutPage.module.css";

const PLACES = [
  {
    title: "Help build it",
    text: "Open source. Fix a bug, shape what comes next.",
    to: GITHUB_URL,
    external: true,
  },
  {
    title: "Support Echo",
    text: "Gifts go into the servers and the time behind each improvement.",
    to: "/community#support",
  },
  {
    title: "Echo Sanctuary",
    text: "Your own servers, set up and looked after by us.",
    to: "/contact?topic=private",
  },
] as const;

export function AboutPage() {
  return (
    <>
      <Intro kicker="About Echo" title={ABOUT.title}>
        {ABOUT.lead}
      </Intro>
      <Split kicker="What we promise" title="What you can count on.">
        <RuleList rules={ABOUT.promises} openFirst />
      </Split>
      <Split kicker="Why Echo exists" title="Churches run on goodwill.">
        {ABOUT.why.map((text) => (
          <p key={text} className={styles.why}>
            {text}
          </p>
        ))}
        <p className={styles.more}>
          <TextLink to="/contact">Tell us what you need →</TextLink>
        </p>
      </Split>
      <Split kicker="Be part of it" title="There is a place for you.">
        <LinkRows rows={PLACES} />
      </Split>
    </>
  );
}
