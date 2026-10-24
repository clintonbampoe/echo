import { useState } from "react";
import { ClosingCta } from "../components/chrome/ClosingCta";
import { PageHero } from "../components/chrome/PageHero";
import { Section } from "../components/chrome/Section";
import { TextLink } from "../components/chrome/TextLink";
import { DOCS } from "../content/pages";
import { GITHUB_URL } from "../content/site";
import styles from "./DocsPage.module.css";

const DOCS_URL = `${GITHUB_URL}/tree/main/docs`;

export function DocsPage() {
  const [query, setQuery] = useState("");
  const q = query.trim().toLowerCase();
  const matches = (text: string) => text.toLowerCase().includes(q);
  const topics = DOCS.topics.filter((t) => matches(`${t.title} ${t.text}`));
  const popular = DOCS.popular.filter(matches);
  return (
    <>
      <PageHero eyebrow="Docs" title={["How can we", "help?"]} />
      <Section top={56}>
        <label className={styles.search}>
          <span className={styles.hidden}>Search the docs</span>
          <input
            type="search"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            placeholder="Search the docs"
            autoComplete="off"
          />
        </label>
        {topics.length === 0 && popular.length === 0 && (
          <p className={styles.none}>
            Nothing matches “{query}”. Try fewer words, or{" "}
            <TextLink to={`${GITHUB_URL}/issues`} external>
              ask on GitHub
            </TextLink>
            .
          </p>
        )}
        <ul className={styles.topics}>
          {topics.map((topic) => (
            <li key={topic.title}>
              <a href={DOCS_URL} target="_blank" rel="noreferrer" className={styles.topic}>
                <h2>{topic.title}</h2>
                <p>{topic.text}</p>
              </a>
            </li>
          ))}
        </ul>
      </Section>
      {popular.length > 0 && (
        <Section top={96}>
          <h2 className={styles.popularTitle}>Popular</h2>
          <ul className={styles.popular}>
            {popular.map((question) => (
              <li key={question}>
                <a href={DOCS_URL} target="_blank" rel="noreferrer">
                  {question}
                </a>
              </li>
            ))}
          </ul>
          <p className={styles.ask}>
            Cannot find it?{" "}
            <TextLink to={`${GITHUB_URL}/issues`} external>
              Ask on GitHub
            </TextLink>
          </p>
        </Section>
      )}
      <ClosingCta />
    </>
  );
}
