import React, { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import echoLogo from '../../assets/echo.svg';
import { apiFetch } from '../../services/api';
import { getErrorMessage } from '../../utils/errors';
import '../../styles/Login.css';

interface VerificationResponse {
  title: string;
  status: number;
  detail: string;
  [key: string]: unknown;
}

interface LocationState {
  email?: string;
}

const VerifyEmail: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const state = location.state as LocationState;

  const [email, setEmail] = useState(state?.email || '');
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  const [loading, setLoading] = useState(false);
  const [verificationSent, setVerificationSent] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!email) {
      setError('Please enter your email address');
      return;
    }

    setLoading(true);
    try {
      // Step 1: Send verification request to get token
     
      const response = await apiFetch<VerificationResponse>('/v1/auth/verifications/account', {
        method: 'POST',
        body: JSON.stringify({ email }),
      });
      console.log('Full response:', response)

      // Parse the message if it's a stringified JSON
      let parsedMessage = response;
      if (response.message && typeof response.message === 'string') {
        try {
          parsedMessage = JSON.parse(response.message);
        } catch {
          // If not JSON, use response as is
        }
      }

      console.log('Parsed message:', parsedMessage)

      // Extract token from detail string (format: "Operation Completed Successfully. Token: {token}")
      const detailStr = parsedMessage.detail || response.detail;
      const tokenMatch = detailStr?.match(/Token:\s*(.+)$/);
      const token = tokenMatch?.[1]?.trim();
      console.log('Extracted token:', token)
      if (!token) {
        setError('Failed to retrieve verification token');
        setLoading(false);
        return;
      }

      setVerificationSent(true);

      // Step 2: Automatically verify with the token from response
      const verifyResponse = await fetch(
        `${import.meta.env.VITE_API_URL || 'http://localhost:5025/api'}/v1/auth/verifications/verify-email?token=${encodeURIComponent(token)}`,
        { method: 'POST' }
      );

      console.log('Verify response status:', verifyResponse.status);

      if (verifyResponse.status === 200) {
        setSuccess(true);
        setTimeout(() => {
          navigate('/login');
        }, 2000);
      } else {
        setError('Verification failed. Please try again.');
      }
    } catch (err: unknown) {
      setError(getErrorMessage(err, 'An error occurred. Please try again.'));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-container">
      <div className="login-left">
        <div className="quote-container">
          <p className="login-quote">"Therefore go and make disciples of all nations..."</p>
          <p className="quote-author">Matthew 28:19</p>
        </div>
      </div>

      <div className="login-right">
        <div className="login-card">
          <img src={echoLogo} className="login-logo" alt="Echo Logo" />
          <h1 className="login-title">Verify Your Email</h1>
          <p className="login-subtitle">Enter your email to activate your account</p>

          {error && (
            <div className="login-error-message" style={{ color: 'red', marginBottom: '1rem', fontSize: '0.875rem' }}>
              {error}
            </div>
          )}

          {success ? (
            <div style={{ textAlign: 'center', padding: '1.5rem 0' }}>
              <div style={{ color: '#16a34a', fontSize: '1.125rem', fontWeight: 600, marginBottom: '0.75rem' }}>
                ✓ Email Verified Successfully!
              </div>
              <p style={{ color: '#6b7280', fontSize: '0.875rem', marginBottom: '1.5rem' }}>
                Your account has been activated. Redirecting to sign in...
              </p>
            </div>
          ) : verificationSent ? (
            <div style={{ textAlign: 'center', padding: '1.5rem 0' }}>
              <div style={{ fontSize: '1.125rem', fontWeight: 600, marginBottom: '0.75rem' }}>
                ⏳ Verifying your email...
              </div>
              <p style={{ color: '#6b7280', fontSize: '0.875rem', marginBottom: '1.5rem' }}>
                Please wait while we confirm your email address.
              </p>
              <div style={{
                display: 'inline-block',
                width: '30px',
                height: '30px',
                border: '3px solid #f3f3f3',
                borderTop: '3px solid black',
                borderRadius: '50%',
                animation: 'spin 1s linear infinite'
              }} />
            </div>
          ) : (
            <form onSubmit={handleSubmit}>
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

              <button type="submit" className="login-button" style={{ marginTop: '1rem' }} disabled={loading}>
                {loading ? 'Sending verification...' : 'Verify Account'}
              </button>
            </form>
          )}

          <div className="create-account">
            Already verified? <a href="/login" style={{ color: 'black', textDecoration: 'none', fontWeight: 700 }}>Sign In</a>
          </div>
        </div>
      </div>
    </div>
  );
};

export default VerifyEmail;
