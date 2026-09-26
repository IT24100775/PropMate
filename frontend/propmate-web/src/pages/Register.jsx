import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../context/authContext";
import logo from "../assets/logo-full.png";
import logoIcon from "../assets/logo-icon.png";
import "./Auth.css";

function Register() {
  const [formData, setFormData] = useState({
    firstName: "",
    lastName: "",
    email: "",
    password: "",
  });

  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const { register } = useAuth();
  const navigate = useNavigate();

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      const user = await register(
        formData.firstName,
        formData.lastName,
        formData.email,
        formData.password
      );

      if (user.role === "Admin") {
        navigate("/admin");
      } else if (user.role === "OwnerAgent") {
        navigate("/owner");
      } else if (user.role === "PropertyManager") {
        navigate("/maintenance");
      } else if (user.role === "BuyerRenter") {
        navigate("/tenant");
      } else {
        navigate("/");
      }
    } catch (err) {
      setError(err.message || "Unable to create account.");
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
            List smarter.
            <br />
            <span>Move confidently.</span>
          </h1>

          <p className="brand-description">
            Bring your properties into one intelligent workspace,
            with structured listings, verification and transparent
            review.
          </p>
        </div>

        <div className="verification-badge">
          <div className="verification-symbol">✓</div>

          <div>
            <strong>Built for trusted listings</strong>
            <span>Verification before publication.</span>
          </div>
        </div>

        <span className="building-outline building-one"></span>
        <span className="building-outline building-two"></span>
        <span className="building-outline building-three"></span>
      </section>

      <section className="auth-form-panel">
        <div className="auth-form-wrapper">
          <img
            src={logo}
            alt="PropMate"
            className="auth-logo"
          />

          <div className="auth-heading">
            <p className="auth-kicker">GET STARTED</p>
            <h2>Create your account</h2>
            <p>
              Join PropMate and start managing your property journey.
            </p>
          </div>

          <form onSubmit={handleSubmit} className="auth-form">
            {error && (
              <div className="auth-error">
                {error}
              </div>
            )}

            <div className="name-fields">
              <div className="form-group">
                <label htmlFor="firstName">First name</label>
                <input
                  id="firstName"
                  name="firstName"
                  type="text"
                  placeholder="First name"
                  value={formData.firstName}
                  onChange={handleChange}
                  required
                />
              </div>

              <div className="form-group">
                <label htmlFor="lastName">Last name</label>
                <input
                  id="lastName"
                  name="lastName"
                  type="text"
                  placeholder="Last name"
                  value={formData.lastName}
                  onChange={handleChange}
                  required
                />
              </div>
            </div>

            <div className="form-group">
              <label htmlFor="email">Email address</label>
              <input
                id="email"
                name="email"
                type="email"
                placeholder="you@example.com"
                value={formData.email}
                onChange={handleChange}
                required
              />
            </div>

            <div className="form-group">
              <label htmlFor="password">Password</label>
              <input
                id="password"
                name="password"
                type="password"
                placeholder="Minimum 8 characters"
                value={formData.password}
                onChange={handleChange}
                minLength={8}
                required
              />
            </div>

            <button
              className="auth-submit"
              type="submit"
              disabled={loading}
            >
              {loading ? "Creating account..." : "Create account"}
              {!loading && <span>→</span>}
            </button>
          </form>

          <p className="auth-switch">
            Already have an account?{" "}
            <Link to="/login">Sign in</Link>
          </p>
        </div>
      </section>
    </div>
  );
}

export default Register;