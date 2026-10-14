import { useSurface } from "@/ui";
import { Closing } from "./components/Closing";
import { Features } from "./components/Features";
import { Footer } from "./components/Footer";
import { Header } from "./components/Header";
import { Hero } from "./components/Hero";
import { Steps } from "./components/Steps";
import { Trust } from "./components/Trust";
import styles from "./LandingPage.module.css";

// Pure composition: each section lives in components/ and owns its own words (content.ts) and styles.
export function LandingPage() {
  useSurface("dark");
  return (
    <div className={styles.page}>
      <Header />
      <main>
        <Hero />
        <Features />
        <Steps />
        <Trust />
        <Closing />
      </main>
      <Footer />
    </div>
  );
}
