import { paymentLabel, type PaymentMethod } from "@/features/transactions";
import { DashCard, DistributionBar } from "@/ui";
import type { Project } from "../../api/projects";
import {
  allTimeFilters,
  contributionStats,
  methodParts,
} from "../../model/contributions";
import {
  useContributionsByMethod,
  useContributionSummary,
} from "../../hooks/queries";
import { ContributionSummary } from "../contributions/ContributionSummary";
import { ProjectProgress } from "./ProjectProgress";
import styles from "./ProjectSummary.module.css";

const METHODS = Object.keys(paymentLabel) as PaymentMethod[];

type Props = {
  project: Project;
  // Choosing a way of giving takes the person to the Contributions tab, narrowed to it.
  onPickMethod: (method: string) => void;
};

// A project at a glance: how far along it is, the figures of everything given towards it, and how it was given. It
// always covers all time; the Contributions tab is where the list is narrowed.
export function ProjectSummary({ project: p, onPickMethod }: Props) {
  const filters = allTimeFilters(p.id);
  const summary = useContributionSummary(filters);
  const byMethod = useContributionsByMethod(filters, METHODS);
  const parts = methodParts(byMethod.flatMap((r) => (r.data ? [r.data] : [])));
  return (
    <div className={styles.summary}>
      <ProjectProgress project={p} />
      <ContributionSummary
        stats={contributionStats(summary.data)}
        busy={summary.isPending}
      />
      {byMethod.every((r) => r.data) && parts.length > 0 && (
        <DashCard title="How it was given">
          <DistributionBar
            label="How it was given"
            parts={parts}
            onPick={onPickMethod}
          />
        </DashCard>
      )}
    </div>
  );
}
