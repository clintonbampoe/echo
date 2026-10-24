import { ClosingCta } from "../components/chrome/ClosingCta";
import { Section } from "../components/chrome/Section";
import { LEGAL } from "../content/pages";
import styles from "./LegalPage.module.css";

type Props = { document: "privacy" | "terms" };

// Privacy and Terms share this shape: a title, a list of sections, and the text of each.
// The words themselves are placeholders until they have been reviewed.
export function LegalPage({ document }: Props) {
  const { title, sections } = LEGAL[document];
  const id = (name: string) => name.toLowerCase().replace(/\s+/g, "-");
  return (
    <>
      <Section top={72}>
        <p className={styles.updated}>{LEGAL.updated}</p>
        <h1 className={styles.title}>{title}</h1>
        <div className={styles.layout}>
          <nav aria-label="On this page" className={styles.toc}>
            {sections.map((name) => (
              <a key={name} href={`#${id(name)}`} className={styles.tocLink}>
                {name}
              </a>
            ))}
          </nav>
          <div className={styles.body}>
            {sections.map((name) => (
              <section key={name} id={id(name)} className={styles.section}>
                <h2>{name}</h2>
                <p>{LEGAL.placeholder}</p>
              </section>
            ))}
          </div>
        </div>
      </Section>
      <ClosingCta />
    </>
  );
}
