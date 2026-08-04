import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import echoLogo from '../../assets/echo.svg';
import '../../styles/Login.css';

const ResetPassword: React.FC = () => {
  const [email, setEmail] = useState('');
  const [submitted, setSubmitted] = useState(false);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitted(true);
  };

  return (
    <div className="login-container">
      <div className="login-left">
        <div className="quote-container">
          <p className="login-quote">"He heals the brokenhearted and binds up their wounds."</p>
          <p className="quote-author">Psalm 147:3</p>
        </div>
      </div>

      <div className="login-right">
        <div className="login-card">
          <img src={echoLogo} className="login-logo" alt="Echo Logo" />
          <h1 className="login-title">Reset Password</h1>
          
          {submitted ? (
            <div style={{ textAlign: 'center' }}>
              <p className="login-subtitle">We've sent a password reset link to <strong>{email}</strong>.</p>
              <p className="login-subtitle" style={{ marginTop: '1rem' }}>Please check your inbox and follow the instructions to reset your password.</p>
              <div className="create-account" style={{ marginTop: '2rem' }}>
                <Link to="/login">Back to Sign In</Link>
              </div>
            </div>
          ) : (
            <>
              <p className="login-subtitle">Enter your email address and we'll send you a link to reset your password.</p>
              
              <form onSubmit={handleSubmit} style={{ marginTop: '1.5rem' }}>
                <div className="form-group">
                  <label className="form-label">Email Address</label>
                  <input
                    type="email"
                    placeholder="example@email.com"
                    className="login-input"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    required
                  />
                </div>

                <button type="submit" className="login-button" style={{ marginTop: '1.5rem' }}>
                  Send Reset Link
                </button>
              </form>

              <div className="create-account">
                Remembered your password? <Link to="/login">Sign In</Link>
              </div>
            </>
          )}
        </div>
      </div>
    </div>
  );
};

export default ResetPassword;
