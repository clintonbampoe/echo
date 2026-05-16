import React, { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import '../styles/Login.css';

const Login: React.FC = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const { login, isLoading } = useAuth();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    try {
      await login(email, password);
    } catch (err) {
      setError('Invalid email or password. Please try again.');
    }
  };

  return (
    <div className="login-container">
      <div className="login-left">
        <div className="quote-container">
          <p className="login-quote">"For where two or more are gathered in my name, there am I among them."</p>
          <p className="quote-author">Matthew 18:20</p>
        </div>
      </div>
      
      <div className="login-right">
        <div className="login-card">
          <div className="login-logo-box"></div>
          <h1 className="login-title">Welcome Back</h1>
          <p className="login-subtitle">Sign in to access your Echo account</p>
          
          {error && <div className="login-error-message" style={{ color: 'red', marginBottom: '1rem', fontSize: '0.875rem' }}>{error}</div>}
          
          <form onSubmit={handleSubmit}>
            <div className="form-group">
              <label className="form-label">Email Address:</label>
              <input 
                type="email" 
                placeholder="example@email.com" 
                className="login-input"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                disabled={isLoading}
                required 
              />
            </div>
            
            <div className="form-group">
              <label className="form-label">Password</label>
              <div className="input-container">
                <input 
                  type={showPassword ? 'text' : 'password'} 
                  placeholder="Enter your password" 
                  className="login-input"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  disabled={isLoading}
                  required 
                />
                <button 
                  type="button" 
                  className="eye-button"
                  onClick={() => setShowPassword(!showPassword)}
                  disabled={isLoading}
                >
                  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                    <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path>
                    <circle cx="12" cy="12" r="3"></circle>
                  </svg>
                </button>
              </div>
            </div>

            <div className="form-footer">
              <label className="remember-me">
                <input type="checkbox" disabled={isLoading} /> Remember me
              </label>
              <a href="#" className="forgot-password">Forgot password?</a>
            </div>
            
            <button type="submit" className="login-button" disabled={isLoading}>
              {isLoading ? 'Signing in...' : 'Sign in'}
            </button>
          </form>

          <div className="create-account">
            Don't have an account? <a href="#">Create Account</a>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Login;

