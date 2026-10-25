import type { CSSProperties } from "react";
import { Link } from "react-router-dom";
import { useMedia } from "../hooks/useMedia";
import { Icon } from "../layout/Icon";
import { PHONE_NODES, lastReached, type TrackNode } from "./track";
import styles from "./HistoryTrack.module.css";

type Props = {
  // Names the track for screen readers.
  label: string;
  title: string;
  summary: string;
  nodes: TrackNode[];
  // How many nodes the loading state draws.
  slots: number;
  status: "loading" | "failed" | "ready";
  // The first and last node's date, shown under the track.
  ends?: [string, string];
};

// A row of nodes telling a story at a glance: which of the last sessions someone came to, which months of the year they
// gave. Oldest on the left. Nodes in a run are joined by an ink rail, so a streak reads as one line. Every node is a
// data door: it lifts on hover, the others step back, and a pill names it.
export function HistoryTrack({
  label,
  title,
  summary,
  nodes,
  slots,
  status,
  ends,
}: Props) {
  const phone = useMedia("(max-width: 600px)");
  const shown = phone ? lastReached(nodes) : nodes;
  return (
    <section
      aria-label={label}
      aria-busy={status === "loading"}
      className={styles.track}
    >
      <header className={styles.head}>
        <h3 className={styles.title}>{title}</h3>
        <p className={styles.count}>
          {status === "ready"
            ? summary
            : status === "failed"
              ? "Could not load"
              : ""}
        </p>
      </header>
      {status === "ready" ? (
        <Nodes nodes={shown} ends={phone ? undefined : ends} />
      ) : (
        <Bones slots={phone ? Math.min(slots, PHONE_NODES) : slots} />
      )}
    </section>
  );
}

function Bones({ slots }: { slots: number }) {
  return (
    <div className={styles.nodes} aria-hidden="true">
      {Array.from({ length: slots }, (_, i) => (
        <span key={i} className={styles.slot}>
          <span
            className={`${styles.node} ${styles.bone}`}
            style={{ "--i": i } as CSSProperties}
          />
        </span>
      ))}
    </div>
  );
}

function Node({ node, index }: { node: TrackNode; index: number }) {
  const className = `${styles.node} ${styles[node.state]}`;
  const stagger = { "--i": index } as CSSProperties;
  const inner = (
    <>
      <span className={`${styles.tip} glass-tip`} aria-hidden="true">
        {node.tip ?? node.name}
      </span>
      {node.state === "on" ? (
        <Icon name="check" size={16} />
      ) : node.state === "off" ? (
        <i className={styles.dot} />
      ) : null}
    </>
  );
  if (node.href)
    return (
      <Link
        to={node.href}
        className={className}
        aria-label={node.name}
        style={stagger}
      >
        {inner}
      </Link>
    );
  return (
    <span
      className={className}
      role="img"
      aria-label={node.name}
      style={stagger}
    >
      {inner}
    </span>
  );
}

function Nodes({
  nodes,
  ends,
}: {
  nodes: TrackNode[];
  ends?: [string, string];
}) {
  return (
    <>
      <ul className={styles.nodes}>
        {nodes.map((n, i) => (
          <li
            key={n.id}
            className={styles.slot}
            // The rail from the node before runs through this one when both are on.
            data-joined={n.state === "on" && nodes[i - 1]?.state === "on"}
          >
            <Node node={n} index={i} />
            {n.caption && (
              <span className={styles.caption} aria-hidden="true">
                {n.caption}
              </span>
            )}
          </li>
        ))}
      </ul>
      {ends && (
        <p className={styles.ends}>
          <span>{ends[0]}</span>
          <span>{ends[1]}</span>
        </p>
      )}
    </>
  );
}
