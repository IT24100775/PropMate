import { useState } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../../../context/authContext";
import TransactionInbox from "../components/TransactionInbox";
import logoWhite from "../../../assets/logo-white.png";
import "../../../pages/AdminDashboard.css";

export default function AdminTransactions() {
  const [type, setType] = useState("rental");
  const { user, logout } = useAuth();

  return (
    <div className="admin-layout">
      <aside className="admin-sidebar">
        <div>
          <img
            src={logoWhite}
            alt="PropMate"
            className="admin-logo"
          />

          <div className="admin-sidebar-label">
            ADMINISTRATION
          </div>

          <nav>
            <Link to="/admin">
              <span>01</span>
              Verification Queue
            </Link>

            <Link
              to="/admin/component3"
              className="admin-nav-active"
            >
              <span>02</span>
              Transactions
            </Link>
          </nav>
        </div>

        <div className="admin-profile">
          <div className="admin-profile-icon">A</div>

          <div>
            <small>ADMINISTRATOR</small>
            <p>{user?.email}</p>
          </div>

          <button type="button" onClick={logout}>
            ↗
          </button>
        </div>
      </aside>

      <main className="admin-main">
        <header className="admin-header">
          <div>
            <p className="admin-eyebrow">
              TRANSACTION MANAGEMENT
            </p>

            <h1>Transactions</h1>

            <p className="admin-description">
              Review rental applications and purchase offers
              across the PropMate platform.
            </p>
          </div>
        </header>

        <section className="admin-queue">
          <div className="admin-queue-heading">
            <div>
              <p className="admin-eyebrow">
                TRANSACTION RECORDS
              </p>

              <h2>
                {type === "rental"
                  ? "Rental transactions"
                  : "Purchase transactions"}
              </h2>
            </div>

            <div className="admin-filters">
              <button
                type="button"
                className={type === "rental" ? "active" : ""}
                onClick={() => setType("rental")}
              >
                Rental
              </button>

              <button
                type="button"
                className={type === "purchase" ? "active" : ""}
                onClick={() => setType("purchase")}
              >
                Purchase
              </button>
            </div>
          </div>

          <TransactionInbox type={type} admin />
        </section>
      </main>
    </div>
  );
}