import type { CSSProperties } from "react";
import { LinkButton } from "@/ui";
import { heroShot } from "../content";
import { PrimaryCta } from "./Cta";
import { ShotWindow } from "./ShotWindow";
import styles from "./Hero.module.css";

const delay = (ms: number) => ({ "--delay": `${ms}ms` }) as CSSProperties;

export function Hero() {
  return (
    <section className={styles.hero}>
      <div className={`${styles.glow} anim-fade`} aria-hidden="true" />
      <h1 className={`${styles.title} anim-rise`}>
        Everything your church runs on, in one place.
      </h1>
      <p className={`${styles.lead} anim-rise`} style={delay(120)}>
        Members, attendance, giving, finances, events and projects, in one calm
        place. Built in Ghana for congregations that want to spend less time on
        paperwork and more time with their people.
      </p>
      <div className={`${styles.actions} anim-rise`} style={delay(220)}>
        <PrimaryCta />
        <LinkButton to="/join" variant="secondary" size="xl" pill>
          I have an invitation
        </LinkButton>
      </div>
      <div className={`${styles.shot} anim-rise`} style={delay(340)}>
        <ShotWindow shot={heroShot} priority />
      </div>
    </section>
  );
}
