import { NavLink, useNavigate } from "react-router-dom";
import { useAuth } from "../../context/authContext";
import logoWhite from "../../assets/logo-white.png";

function OwnerSidebar() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  return (
    <aside className="owner-sidebar">
      <div>
        <img
          src={logoWhite}
          alt="PropMate"
          className="sidebar-logo"
        />

        <p className="sidebar-section-label">WORKSPACE</p>

        <nav className="sidebar-nav">
          <NavLink
            to="/owner"
            end
            className={({ isActive }) =>
              `sidebar-link ${isActive ? "active" : ""}`
            }
          >
            <span className="sidebar-icon">⌂</span>
            Overview
          </NavLink>

          <NavLink
            to="/owner/listings"
            className={({ isActive }) =>
              `sidebar-link ${isActive ? "active" : ""}`
            }
          >
            <span className="sidebar-icon">▤</span>
            My Listings
          </NavLink>

          <NavLink
            to="/owner/listings/new"
            className={({ isActive }) =>
              `sidebar-link ${isActive ? "active" : ""}`
            }
          >
            <span className="sidebar-icon">＋</span>
            Add Property
          </NavLink>
        </nav>
      </div>

      <div className="sidebar-bottom">
        <div className="sidebar-user">
          <div className="user-avatar">
            {user?.email?.charAt(0).toUpperCase()}
          </div>

          <div className="user-details">
            <strong>Owner / Agent</strong>
            <span>{user?.email}</span>
          </div>
        </div>

        <button
          type="button"
          className="logout-button"
          onClick={handleLogout}
        >
          Sign out
          <span>→</span>
        </button>
      </div>
    </aside>
  );
}

export default OwnerSidebar;