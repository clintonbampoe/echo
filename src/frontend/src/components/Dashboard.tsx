import React, { useEffect } from 'react';
import { useLayout } from '../context/LayoutContext';

const Dashboard: React.FC = () => {
  const { setTitle, setCtas } = useLayout();

  useEffect(() => {
    setTitle('Dashboard');
    setCtas([
      { type: 'search', placeholder: 'Search...' },
      { type: 'avatar' },
      { type: 'button', label: 'Add', icon: 'plus', variant: 'primary', onClick: () => console.log('Add clicked') }
    ]);
  }, [setTitle, setCtas]);

  const stats = [
    { label: 'Total Members', value: '428', trend: '+12 this month', color: '#34c759' },
    { label: 'Avg Weekly Attendance', value: '214', trend: '+10 this month', color: '#34c759' },
    { label: 'Monthly Offering', value: '$ 45,900', trend: '+54 this month', color: '#34c759' },
    { label: 'Active Members', value: '178', trend: '+21 this month', color: '#34c759' },
  ];

  const recentActivity = [
    { id: 1, user: 'John Doe', action: 'added a new member', time: '2 hours ago' },
    { id: 2, user: 'Mary Smith', action: 'recorded a tithe payment', time: '5 hours ago' },
    { id: 3, user: 'Robert Johnson', action: 'updated attendance for Sunday', time: '1 day ago' },
  ];

  return (
    <div style={styles.container}>
      <div style={styles.statsGrid}>
        {stats.map((stat, idx) => (
          <div key={idx} style={styles.statCard}>
            <span style={styles.statLabel}>{stat.label}</span>
            <div style={styles.statValue}>{stat.value}</div>
            <div style={{ ...styles.statTrend, color: stat.color }}>
              <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round" style={{ marginRight: '4px' }}>
                <polyline points="23 6 13.5 15.5 8.5 10.5 1 18"></polyline>
                <polyline points="17 6 23 6 23 12"></polyline>
              </svg>
              {stat.trend}
            </div>
          </div>
        ))}
      </div>

      <div style={styles.dashboardGrid}>
        {/* Financial Overview Mockup */}
        <div style={styles.mainCard}>
          <div style={styles.cardHeader}>
            <h3 style={styles.cardTitle}>Financial Overview</h3>
            <div style={styles.cardActions}>
              <button style={styles.ghostButton}>This Year</button>
            </div>
          </div>
          <div style={styles.chartContainer}>
            <div style={styles.chartYAxis}>
              <span>$50k</span>
              <span>$25k</span>
              <span>0</span>
            </div>
            <div style={styles.chartBars}>
              {[60, 40, 85, 70, 95, 50, 75, 90, 65, 80, 55, 70].map((height, i) => (
                <div key={i} style={styles.chartBarWrapper}>
                  <div style={{ ...styles.chartBar, height: `${height}%` }}></div>
                  <span style={styles.chartLabel}>{['J', 'F', 'M', 'A', 'M', 'J', 'J', 'A', 'S', 'O', 'N', 'D'][i]}</span>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Recent Activity */}
        <div style={styles.sideCard}>
          <div style={styles.cardHeader}>
            <h3 style={styles.cardTitle}>Recent Activity</h3>
          </div>
          <div style={styles.activityList}>
            {recentActivity.map(activity => (
              <div key={activity.id} style={styles.activityItem}>
                <div style={styles.activityAvatar}></div>
                <div style={styles.activityContent}>
                  <div style={styles.activityText}>
                    <strong>{activity.user}</strong> {activity.action}
                  </div>
                  <div style={styles.activityTime}>{activity.time}</div>
                </div>
              </div>
            ))}
          </div>
          <button style={styles.viewAllButton}>View all activity</button>
        </div>
      </div>
    </div>
  );
};

const styles: { [key: string]: React.CSSProperties } = {
  container: {
    padding: '32px 40px',
    display: 'flex',
    flexDirection: 'column',
    gap: '32px',
  },
  statsGrid: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))',
    gap: '24px',
  },
  statCard: {
    backgroundColor: 'white',
    padding: '24px',
    borderRadius: '16px',
    border: '1px solid var(--border)',
    boxShadow: '0 1px 3px rgba(0,0,0,0.02)',
  },
  statLabel: {
    fontSize: '13px',
    fontWeight: '500',
    color: 'var(--text-muted)',
    display: 'block',
    marginBottom: '12px',
  },
  statValue: {
    fontSize: '28px',
    fontWeight: '700',
    color: 'black',
    marginBottom: '8px',
  },
  statTrend: {
    fontSize: '12px',
    fontWeight: '600',
    display: 'flex',
    alignItems: 'center',
  },
  dashboardGrid: {
    display: 'grid',
    gridTemplateColumns: '2fr 1fr',
    gap: '24px',
  },
  mainCard: {
    backgroundColor: 'white',
    borderRadius: '16px',
    border: '1px solid var(--border)',
    padding: '24px',
  },
  sideCard: {
    backgroundColor: 'white',
    borderRadius: '16px',
    border: '1px solid var(--border)',
    padding: '24px',
    display: 'flex',
    flexDirection: 'column',
  },
  cardHeader: {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: '24px',
  },
  cardTitle: {
    fontSize: '18px',
    fontWeight: '700',
  },
  cardActions: {},
  ghostButton: {
    backgroundColor: 'transparent',
    border: '1px solid var(--border)',
    borderRadius: '6px',
    padding: '6px 12px',
    fontSize: '12px',
    fontWeight: '600',
  },
  chartContainer: {
    height: '240px',
    display: 'flex',
    gap: '16px',
    paddingTop: '20px',
  },
  chartYAxis: {
    display: 'flex',
    flexDirection: 'column',
    justifyContent: 'space-between',
    color: 'var(--text-muted)',
    fontSize: '11px',
    paddingBottom: '20px',
  },
  chartBars: {
    flex: 1,
    display: 'flex',
    alignItems: 'flex-end',
    justifyContent: 'space-between',
    gap: '8px',
  },
  chartBarWrapper: {
    flex: 1,
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    height: '100%',
    justifyContent: 'flex-end',
  },
  chartBar: {
    width: '100%',
    maxWidth: '32px',
    backgroundColor: 'black',
    borderRadius: '4px 4px 0 0',
    transition: 'height 0.3s ease',
  },
  chartLabel: {
    marginTop: '8px',
    fontSize: '11px',
    color: 'var(--text-muted)',
  },
  activityList: {
    display: 'flex',
    flexDirection: 'column',
    gap: '20px',
    flex: 1,
  },
  activityItem: {
    display: 'flex',
    gap: '12px',
  },
  activityAvatar: {
    width: '32px',
    height: '32px',
    backgroundColor: '#f2f2f7',
    borderRadius: '50%',
    flexShrink: 0,
  },
  activityContent: {
    fontSize: '13px',
  },
  activityText: {
    color: 'var(--text-main)',
    marginBottom: '2px',
  },
  activityTime: {
    color: 'var(--text-muted)',
    fontSize: '11px',
  },
  viewAllButton: {
    marginTop: '24px',
    backgroundColor: 'transparent',
    border: 'none',
    color: 'var(--text-muted)',
    fontSize: '13px',
    fontWeight: '600',
    textAlign: 'center',
    width: '100%',
    padding: '8px',
  },
};

export default Dashboard;
