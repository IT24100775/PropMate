import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../context/authContext";
import logo from "../assets/logo-full.png";
import logoIcon from "../assets/logo-icon.png";
import "./Auth.css";

function Login() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      const user = await login(email, password);

      if (user.role === "Admin") {
        navigate("/admin");
      } else if (user.role === "OwnerAgent") {
        navigate("/owner");
      } else {
        navigate("/");
      }
    } catch (err) {
      setError(err.message || "Unable to sign in.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-page">
      <section className="auth-brand-panel">
        <div className="brand-content">
          <img
            src={logoIcon}
            alt="PropMate"
            className="brand-icon"
          />

          <p className="brand-eyebrow">PROPERTY INTELLIGENCE</p>

          <h1>
            Property decisions,
            <br />
            <span>made clearer.</span>
          </h1>

          <p className="brand-description">
            Create, verify and manage property listings through one
            intelligent platform built for confident decisions.
          </p>
        </div>

        <div className="verification-badge">
          <div className="verification-symbol">✓</div>

          <div>
            <strong>AI-assisted verification</strong>
            <span>Smarter checks. Human decisions.</span>
          </div>
        </div>

        <span className="building-outline building-one"></span>
        <span className="building-outline building-two"></span>
        <span className="building-outline building-three"></span>
      </section>

      <section className="auth-form-panel">
        <div className="auth-form-wrapper">
          <img src={logo} alt="PropMate" className="auth-logo" />

          <div className="auth-heading">
            <p className="auth-kicker">WELCOME BACK</p>
            <h2>Sign in to PropMate</h2>
            <p>Access your property workspace and continue where you left off.</p>
          </div>

          <form onSubmit={handleSubmit} className="auth-form">
            {error && <div className="auth-error">{error}</div>}

            <div className="form-group">
              <label htmlFor="email">Email address</label>
              <input
                id="email"
                type="email"
                placeholder="you@example.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
              />
            </div>

            <div className="form-group">
              <div className="label-row">
                <label htmlFor="password">Password</label>
              </div>

              <input
                id="password"
                type="password"
                placeholder="Enter your password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
              />
            </div>

            <button
              className="auth-submit"
              type="submit"
              disabled={loading}
            >
              {loading ? "Signing in..." : "Sign in"}
              {!loading && <span>→</span>}
            </button>
          </form>

          <p className="auth-switch">
            New to PropMate? <Link to="/register">Create an account</Link>
          </p>

          <div className="auth-footer">
            <span>PROP<span>MATE</span></span>
            <p>Property management, enhanced by intelligence.</p>
          </div>
        </div>
      </section>
    </div>
  );
}

export default Login;