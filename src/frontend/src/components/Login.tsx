import React, { useState } from 'react';

interface LoginProps {
  onLogin: () => void;
}

const Login: React.FC<LoginProps> = ({ onLogin }) => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [rememberMe, setRememberMe] = useState(false);
  const [showPassword, setShowPassword] = useState(false);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    console.log('Login attempt:', { email, password, rememberMe });
    onLogin(); // Switch to dashboard view
  };

  const togglePasswordVisibility = () => {
    setShowPassword(!showPassword);
  };

  return (
    <div style={styles.page}>
      {/* Left Side: Background & Quote */}
      <div style={styles.leftSide} className="login-left">
        <div style={styles.quoteContainer}>
          <p style={styles.quote}>
            "For where two or more are gathered in my name, there am I among them."
          </p>
          <p style={styles.quoteAuthor}>Matthew 18:20</p>
        </div>
      </div>

      {/* Right Side: Login Form */}
      <div style={styles.rightSide} className="login-right">

        <div style={styles.formContainer}>
          <div style={styles.header}>
            <div style={styles.logoPlaceholder}></div>
            <h1 style={styles.title}>Welcome Back</h1>
            <p style={styles.subtitle}>Sign in to access your Echo account</p>
          </div>

          <form onSubmit={handleSubmit} style={styles.form}>
            <div style={styles.inputGroup}>
              <label htmlFor="email" style={styles.label}>Email Address:</label>
              <input
                type="email"
                id="email"
                placeholder="example@email.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                style={styles.input}
                required
              />
            </div>

            <div style={styles.inputGroup}>
              <label htmlFor="password" style={styles.label}>Password</label>
              <div style={styles.passwordWrapper}>
                <input
                  type={showPassword ? "text" : "password"}
                  id="password"
                  placeholder="Enter your password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  style={styles.inputWithIcon}
                  required
                />
                <button 
                  type="button" 
                  onClick={togglePasswordVisibility} 
                  style={styles.eyeButton}
                  aria-label={showPassword ? "Hide password" : "Show password"}
                >
                  {showPassword ? (
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                      <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"></path>
                      <line x1="1" y1="1" x2="23" y2="23"></line>
                    </svg>
                  ) : (
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                      <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path>
                      <circle cx="12" cy="12" r="3"></circle>
                    </svg>
                  )}
                </button>
              </div>
            </div>


            <div style={styles.options}>
              <label style={styles.checkboxContainer}>
                <input
                  type="checkbox"
                  checked={rememberMe}
                  onChange={(e) => setRememberMe(e.target.checked)}
                  style={styles.checkbox}
                />
                <span style={styles.checkboxLabel}>Remember me</span>
              </label>
              <a href="#" style={styles.forgotPassword}>Forgot password?</a>
            </div>

            <button type="submit" style={styles.button}>
              Sign in
            </button>
          </form>

          <div style={styles.footer}>
            <p style={styles.footerText}>
              Don't have an account? <a href="#" style={styles.signUpLink}>Create Account</a>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

const styles: { [key: string]: React.CSSProperties } = {
  page: {
    display: 'flex',
    height: '100vh',
    width: '100%',
  },
  leftSide: {
    flex: 1,
    backgroundColor: 'var(--bg-left)',
    display: 'flex',
    alignItems: 'flex-end',
    padding: '60px',
    color: 'white',
  },
  quoteContainer: {
    maxWidth: '400px',
  },
  quote: {
    fontSize: '24px',
    fontWeight: '400',
    marginBottom: '8px',
    lineHeight: '1.4',
  },
  quoteAuthor: {
    fontSize: '16px',
    opacity: 0.8,
  },
  rightSide: {
    flex: 1,
    backgroundColor: 'var(--bg-right)',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    padding: '40px',
  },
  formContainer: {
    width: '100%',
    maxWidth: '400px',
  },
  header: {
    textAlign: 'center',
    marginBottom: '40px',
  },
  logoPlaceholder: {
    width: '48px',
    height: '48px',
    backgroundColor: 'var(--text-muted)',
    borderRadius: '8px',
    margin: '0 auto 24px',
  },
  title: {
    fontSize: '28px',
    fontWeight: '700',
    color: 'var(--text-main)',
    marginBottom: '8px',
  },
  subtitle: {
    color: 'var(--text-muted)',
    fontSize: '16px',
  },
  form: {
    display: 'flex',
    flexDirection: 'column',
    gap: '24px',
  },
  inputGroup: {
    display: 'flex',
    flexDirection: 'column',
    gap: '8px',
  },
  label: {
    fontSize: '14px',
    fontWeight: '600',
    color: 'var(--text-main)',
  },
  input: {
    padding: '14px 16px',
    borderRadius: '8px',
    border: 'none',
    backgroundColor: 'var(--input-bg)',
    fontSize: '14px',
    color: 'var(--text-main)',
  },
  passwordWrapper: {
    position: 'relative',
    display: 'flex',
    alignItems: 'center',
  },
  inputWithIcon: {
    padding: '14px 48px 14px 16px',
    borderRadius: '8px',
    border: 'none',
    backgroundColor: 'var(--input-bg)',
    fontSize: '14px',
    color: 'var(--text-main)',
    width: '100%',
  },
  eyeButton: {
    position: 'absolute',
    right: '12px',
    background: 'none',
    border: 'none',
    color: 'var(--text-muted)',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    padding: '4px',
  },
  options: {
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'space-between',
    fontSize: '13px',
  },
  checkboxContainer: {
    display: 'flex',
    alignItems: 'center',
    gap: '8px',
    cursor: 'pointer',
    color: 'var(--text-muted)',
  },
  checkbox: {
    width: '16px',
    height: '16px',
    borderRadius: '4px',
    cursor: 'pointer',
  },
  checkboxLabel: {
    fontSize: '13px',
  },
  forgotPassword: {
    color: 'var(--text-main)',
    textDecoration: 'none',
    fontWeight: '700',
  },
  button: {
    padding: '14px',
    backgroundColor: 'var(--primary)',
    color: 'white',
    border: 'none',
    borderRadius: '8px',
    fontSize: '16px',
    fontWeight: '700',
    marginTop: '8px',
  },
  footer: {
    marginTop: '40px',
    textAlign: 'center',
  },
  footerText: {
    fontSize: '13px',
    color: 'var(--text-muted)',
  },
  signUpLink: {
    color: 'var(--text-main)',
    textDecoration: 'none',
    fontWeight: '700',
  },
};

export default Login;
