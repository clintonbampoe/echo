import React, { useEffect, useMemo, useState } from "react";
import { useLayout } from "../hooks/useLayout";
import { useMembers } from "../hooks/useMembers";
import { useAttendance } from "../hooks/useAttendance";
import { useTithes } from "../hooks/useTithes";
import { useTransactions } from "../hooks/useTransactions";
import { useEvents } from "../hooks/useEvents";
import "../styles/Dashboard.css";

const MONTH_NAMES = [
  "Jan",
  "Feb",
  "Mar",
  "Apr",
  "May",
  "Jun",
  "Jul",
  "Aug",
  "Sep",
  "Oct",
  "Nov",
  "Dec",
];

const getInitials = (name: string) => {
  if (!name) return "U";
  return name
    .split(" ")
    .map((word) => word[0])
    .join("")
    .toUpperCase()
    .slice(0, 2);
};

const getAvatarColor = (name: string) => {
  const colors = [
    { bg: "#e8f5e9", text: "#2e7d32" },
    { bg: "#e3f2fd", text: "#1565c0" },
    { bg: "#f3e5f5", text: "#6a1b9a" },
    { bg: "#fff3e0", text: "#e65100" },
    { bg: "#ffebee", text: "#c62828" },
    { bg: "#e0f7fa", text: "#00838f" },
  ];
  let hash = 0;
  for (let i = 0; i < name.length; i++) {
    hash = name.charCodeAt(i) + ((hash << 5) - hash);
  }
  const index = Math.abs(hash) % colors.length;
  return colors[index];
};

const formatCurrency = (amount: number): string => {
  return `₵ ${amount.toLocaleString("en-US", { minimumFractionDigits: 0, maximumFractionDigits: 0 })}`;
};

// Dashboard fetches a large page of each resource and aggregates client-side.
// The counts below are correct only while the church fits within the fetch caps.
// When the backend exposes aggregate endpoints, swap these fetches for direct
// summary reads — see the notes in the Project aggregate discussion.
const MEMBER_FETCH_CAP = 500;
const ATTENDANCE_FETCH_CAP = 500;
const TITHE_FETCH_CAP = 500;
const TRANSACTION_FETCH_CAP = 500;
const EVENT_FETCH_CAP = 500;

const Dashboard: React.FC = () => {
  const { setTitle, setCtas } = useLayout();
  const [initialTimestamp] = useState(() => Date.now());

  // ── Real Queries ──────────────────────────────────────────────────────────
  const { data: membersResponse } = useMembers({}, MEMBER_FETCH_CAP);
  const members = useMemo(
    () => membersResponse?.data || [],
    [membersResponse?.data],
  );

  const { data: attendanceResponse } = useAttendance({}, ATTENDANCE_FETCH_CAP);
  const attendance = useMemo(
    () => attendanceResponse?.data || [],
    [attendanceResponse?.data],
  );

  const { data: tithesResponse } = useTithes({}, TITHE_FETCH_CAP);
  const tithes = useMemo(
    () => tithesResponse?.data || [],
    [tithesResponse?.data],
  );

  const { data: transactionsResponse } = useTransactions(
    {},
    TRANSACTION_FETCH_CAP,
  );
  const transactions = useMemo(
    () => transactionsResponse?.data || [],
    [transactionsResponse?.data],
  );

  const { data: eventsResponse } = useEvents({}, EVENT_FETCH_CAP);
  const events = useMemo(
    () => eventsResponse?.data || [],
    [eventsResponse?.data],
  );

  useEffect(() => {
    setTitle("Dashboard");
    setCtas([
      { type: "search", placeholder: "Search dashboard..." },
      { type: "avatar" },
    ]);
  }, [setTitle, setCtas]);

  // ── Computed KPIs ─────────────────────────────────────────────────────────
  const totalMembersCount = members.length;
  const activeMembersCount = useMemo(
    () => members.filter((m) => m.status === "Active").length,
    [members],
  );

  const totalTithesAmount = useMemo(
    () => tithes.reduce((sum, t) => sum + (t.amount || 0), 0),
    [tithes],
  );
  const totalIncomeTxAmount = useMemo(
    () =>
      transactions
        .filter((t) => t.transactionType === "Income")
        .reduce((sum, t) => sum + (t.amount || 0), 0),
    [transactions],
  );
  const totalIncome = totalTithesAmount + totalIncomeTxAmount;

  const totalAttendeesCount = attendance.length;

  const upcomingEventsCount = useMemo(() => {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return events.filter((e) => new Date(e.startDate) >= today).length;
  }, [events]);

  const stats = [
    {
      label: "Total Members",
      value: String(totalMembersCount),
      trend: `${activeMembersCount} active members`,
      color: "#34c759",
    },
    {
      label: "Total Attendees Logged",
      value: String(totalAttendeesCount),
      trend: "Across all services",
      color: "#007aff",
    },
    {
      label: "Total Inflow",
      value: formatCurrency(totalIncome),
      trend: `${tithes.length + transactions.length} transactions`,
      color: "#34c759",
    },
    {
      label: "Upcoming Events",
      value: String(upcomingEventsCount),
      trend: "Scheduled from today",
      color: "#af52de",
    },
  ];

  // ── Monthly Inflow Chart Calculation ──────────────────────────────────────
  const monthlyChartData = useMemo(() => {
    const currentYear = new Date().getFullYear();
    const monthlyAmounts = new Array(12).fill(0);

    transactions
      .filter((t) => t.transactionType === "Income")
      .forEach((t) => {
        const d = new Date(t.transactionDate);
        if (d.getFullYear() === currentYear) {
          monthlyAmounts[d.getMonth()] += t.amount;
        }
      });

    tithes.forEach((t) => {
      const d = new Date(t.collectionDate);
      if (d.getFullYear() === currentYear) {
        monthlyAmounts[d.getMonth()] += t.amount;
      }
    });

    const maxVal = Math.max(...monthlyAmounts, 100);

    return monthlyAmounts.map((amount, idx) => ({
      month: MONTH_NAMES[idx],
      amount,
      heightPercent: Math.max(8, (amount / maxVal) * 100),
    }));
  }, [transactions, tithes]);

  // ── Real Recent Activity Feed ─────────────────────────────────────────────
  const recentActivity = useMemo(() => {
    const activities: {
      id: string;
      user: string;
      action: string;
      time: string;
      date: Date;
    }[] = [];

    members.slice(0, 4).forEach((m) => {
      const name = m.name || `${m.firstName} ${m.lastName}`;
      activities.push({
        id: `mem-${m.id}`,
        user: name,
        action: "joined as a member",
        time: m.joinedDate
          ? new Date(m.joinedDate).toLocaleDateString()
          : "Recently",
        date: new Date(m.createdAt || m.joinedDate || initialTimestamp),
      });
    });

    tithes.slice(0, 4).forEach((t) => {
      activities.push({
        id: `tith-${t.id}`,
        user: t.memberName || "Member",
        action: `recorded tithe payment of ${formatCurrency(t.amount)}`,
        time: new Date(t.collectionDate).toLocaleDateString(),
        date: new Date(t.createdAt || t.collectionDate || initialTimestamp),
      });
    });

    events.slice(0, 3).forEach((ev) => {
      activities.push({
        id: `ev-${ev.id}`,
        user: ev.organizerName || "Admin",
        action: `created event "${ev.name}"`,
        time: new Date(ev.startDate).toLocaleDateString(),
        date: new Date(ev.createdAt || ev.startDate || initialTimestamp),
      });
    });

    transactions.slice(0, 4).forEach((tr) => {
      activities.push({
        id: `tx-${tr.id}`,
        user: tr.categoryName || "Finance",
        action: `recorded ${tr.transactionType?.toLowerCase() || "transaction"} of ${formatCurrency(tr.amount)}`,
        time: new Date(tr.transactionDate).toLocaleDateString(),
        date: new Date(tr.createdAt || tr.transactionDate || initialTimestamp),
      });
    });

    activities.sort((a, b) => b.date.getTime() - a.date.getTime());

    return activities.slice(0, 6);
  }, [members, tithes, events, transactions, initialTimestamp]);

  return (
    <div className="dashboard-container">
      <div className="stats-grid">
        {stats.map((stat, idx) => (
          <div key={idx} className="stat-card">
            <span className="stat-label">{stat.label}</span>
            <div className="stat-value">{stat.value}</div>
            <div className="stat-trend" style={{ color: stat.color }}>
              <svg
                width="12"
                height="12"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="3"
                strokeLinecap="round"
                strokeLinejoin="round"
                style={{ marginRight: "4px" }}
              >
                <polyline points="23 6 13.5 15.5 8.5 10.5 1 18"></polyline>
                <polyline points="17 6 23 6 23 12"></polyline>
              </svg>
              {stat.trend}
            </div>
          </div>
        ))}
      </div>

      <div className="dashboard-grid">
        <div className="main-card">
          <div className="card-header">
            <h3 className="card-title">Financial Overview</h3>
            <div className="card-actions">
              <span className="ghost-button">
                {new Date().getFullYear()} Overview
              </span>
            </div>
          </div>
          <div className="chart-container">
            <div className="chart-y-axis">
              <span>Max</span>
              <span>Mid</span>
              <span>0</span>
            </div>
            <div className="chart-bars">
              {monthlyChartData.map((data, i) => (
                <div key={i} className="chart-bar-wrapper">
                  <div
                    className="chart-bar"
                    style={{ height: `${data.heightPercent}%` }}
                    title={`${data.month}: ${formatCurrency(data.amount)}`}
                  ></div>
                  <span className="chart-label">{data.month}</span>
                </div>
              ))}
            </div>
          </div>
        </div>

        <div className="side-card">
          <div className="card-header">
            <h3 className="card-title">Recent Activity</h3>
          </div>
          <div className="activity-list">
            {recentActivity.length === 0 ? (
              <div
                style={{
                  padding: "24px",
                  textAlign: "center",
                  color: "var(--text-muted)",
                  fontSize: "13px",
                }}
              >
                No recent activity recorded yet.
              </div>
            ) : (
              recentActivity.map((activity) => {
                const avatarStyle = getAvatarColor(activity.user);
                return (
                  <div key={activity.id} className="activity-item">
                    <div
                      className="activity-avatar"
                      style={{
                        backgroundColor: avatarStyle.bg,
                        color: avatarStyle.text,
                      }}
                    >
                      {getInitials(activity.user)}
                    </div>
                    <div className="activity-content">
                      <div className="activity-text">
                        <strong>{activity.user}</strong> {activity.action}
                      </div>
                      <div className="activity-time">{activity.time}</div>
                    </div>
                  </div>
                );
              })
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;
