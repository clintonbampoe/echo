import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import echoLogo from '../../assets/echo.svg';
import { authService } from '../../services/authService';
import { getErrorMessage } from '../../utils/errors';
import '../../styles/Login.css';

const SignUp: React.FC = () => {
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [organization, setOrganization] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (password !== confirmPassword) {
      setError('Passwords do not match');
      return;
    }

    if (password.length < 8) {
      setError('Password must be at least 8 characters long and contain uppercase, lowercase, digit, and special character.');
      return;
    }

    setLoading(true);
    try {
      await authService.registerCongregation({
        congregationDto: {
          name: organization,
          orgType: 'Church',
          phoneNumber: '+233000000000',
          emailAddress: email,
          region: 'GreaterAccra',
          city: 'Accra',
          town: 'Accra',
          gpsAddress: 'GA-000-0000',
        },
        userDto: {
          firstName,
          lastName,
          emailAddress: email,
          password,
        },
      });
      setSuccess(true);
    } catch (err: unknown) {
      setError(getErrorMessage(err, 'An error occurred during sign up.'));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-container">
      <div className="login-left">
        <div className="quote-container">
          <p className="login-quote">"Let all that you do be done in love."</p>
          <p className="quote-author">1 Corinthians 16:14</p>
        </div>
      </div>

      <div className="login-right">
        <div className="login-card">
          <img src={echoLogo} className="login-logo" alt="Echo Logo" />
          <h1 className="login-title">Create Account</h1>
          <p className="login-subtitle">Join Echo to manage your church effectively</p>

          {error && <div className="login-error-message" style={{ color: 'red', marginBottom: '1rem', fontSize: '0.875rem' }}>{error}</div>}

          {success ? (
            <div style={{ textAlign: 'center', padding: '1.5rem 0' }}>
              <div style={{ color: '#16a34a', fontSize: '1.125rem', fontWeight: 600, marginBottom: '0.75rem' }}>
                Registration Successful!
              </div>
              <p style={{ color: '#6b7280', fontSize: '0.875rem', marginBottom: '1.5rem' }}>
                Your congregation account has been created. Please check your email to verify your account, then sign in.
              </p>
              <button
                type="button"
                className="login-button"
                onClick={() => navigate('/login')}
              >
                Go to Sign In
              </button>
            </div>
          ) : (
            <form onSubmit={handleSubmit}>
              <div className="form-row" style={{ display: 'flex', gap: '1rem' }}>
                <div className="form-group" style={{ flex: 1 }}>
                  <label className="form-label">First Name</label>
                  <input
                    type="text"
                    placeholder="John"
                    className="login-input"
                    value={firstName}
                    onChange={(e) => setFirstName(e.target.value)}
                    disabled={loading}
                    required
                  />
                </div>
                <div className="form-group" style={{ flex: 1 }}>
                  <label className="form-label">Last Name</label>
                  <input
                    type="text"
                    placeholder="Doe"
                    className="login-input"
                    value={lastName}
                    onChange={(e) => setLastName(e.target.value)}
                    disabled={loading}
                    required
                  />
                </div>
              </div>

              <div className="form-group">
                <label className="form-label">Church/Organization Name</label>
                <input
                  type="text"
                  placeholder="Grace Community Church"
                  className="login-input"
                  value={organization}
                  onChange={(e) => setOrganization(e.target.value)}
                  disabled={loading}
                  required
                />
              </div>

              <div className="form-group">
                <label className="form-label">Email Address</label>
                <input
                  type="email"
                  placeholder="example@email.com"
                  className="login-input"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  disabled={loading}
                  required
                />
              </div>

              <div className="form-group">
                <label className="form-label">Password</label>
                <div className="input-container">
                  <input
                    type={showPassword ? 'text' : 'password'}
                    placeholder="Create a password"
                    className="login-input"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    disabled={loading}
                    required
                  />
                  <button
                    type="button"
                    className="eye-button"
                    onClick={() => setShowPassword(!showPassword)}
                    disabled={loading}
                  >
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                      <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path>
                      <circle cx="12" cy="12" r="3"></circle>
                    </svg>
                  </button>
                </div>
              </div>

              <div className="form-group">
                <label className="form-label">Confirm Password</label>
                <input
                  type={showPassword ? 'text' : 'password'}
                  placeholder="Confirm your password"
                  className="login-input"
                  value={confirmPassword}
                  onChange={(e) => setConfirmPassword(e.target.value)}
                  disabled={loading}
                  required
                />
              </div>

              <button type="submit" className="login-button" style={{ marginTop: '1rem' }} disabled={loading}>
                {loading ? 'Creating Account...' : 'Sign Up'}
              </button>
            </form>
          )}

          <div className="create-account">
            Already have an account? <Link to="/login">Sign In</Link>
          </div>
        </div>
      </div>
    </div>
  );
};

export default SignUp;
