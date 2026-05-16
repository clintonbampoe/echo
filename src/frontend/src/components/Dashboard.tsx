import React, { useEffect } from 'react';
import { useLayout } from '../context/LayoutContext';
import '../styles/Dashboard.css';

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
    <div className="dashboard-container">
      <div className="stats-grid">
        {stats.map((stat, idx) => (
          <div key={idx} className="stat-card">
            <span className="stat-label">{stat.label}</span>
            <div className="stat-value">{stat.value}</div>
            <div className="stat-trend" style={{ color: stat.color }}>
              <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round" style={{ marginRight: '4px' }}>
                <polyline points="23 6 13.5 15.5 8.5 10.5 1 18"></polyline>
                <polyline points="17 6 23 6 23 12"></polyline>
              </svg>
              {stat.trend}
            </div>
          </div>
        ))}
      </div>

      <div className="dashboard-grid">
        {/* Financial Overview Mockup */}
        <div className="main-card">
          <div className="card-header">
            <h3 className="card-title">Financial Overview</h3>
            <div className="card-actions">
              <button className="ghost-button">This Year</button>
            </div>
          </div>
          <div className="chart-container">
            <div className="chart-y-axis">
              <span>$50k</span>
              <span>$25k</span>
              <span>0</span>
            </div>
            <div className="chart-bars">
              {[60, 40, 85, 70, 95, 50, 75, 90, 65, 80, 55, 70].map((height, i) => (
                <div key={i} className="chart-bar-wrapper">
                  <div className="chart-bar" style={{ height: `${height}%` }}></div>
                  <span className="chart-label">{['J', 'F', 'M', 'A', 'M', 'J', 'J', 'A', 'S', 'O', 'N', 'D'][i]}</span>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Recent Activity */}
        <div className="side-card">
          <div className="card-header">
            <h3 className="card-title">Recent Activity</h3>
          </div>
          <div className="activity-list">
            {recentActivity.map(activity => (
              <div key={activity.id} className="activity-item">
                <div className="activity-avatar"></div>
                <div className="activity-content">
                  <div className="activity-text">
                    <strong>{activity.user}</strong> {activity.action}
                  </div>
                  <div className="activity-time">{activity.time}</div>
                </div>
              </div>
            ))}
          </div>
          <button className="view-all-button">View all activity</button>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;
