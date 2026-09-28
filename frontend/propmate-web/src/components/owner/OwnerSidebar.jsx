import { NavLink, useNavigate } from "react-router-dom";
import logoWhite from "../../assets/logo-white.png";

function OwnerSidebar() {
  const navigate = useNavigate();

  const handleWorkspaceChange = () => navigate("/workspaces");

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
            D
          </div>

          <div className="user-details">
            <strong>Owner / Agent</strong>
            <span>owner@propmate.demo</span>
          </div>
        </div>

        <button
          type="button"
          className="logout-button"
          onClick={handleWorkspaceChange}
        >
          Change workspace
          <span>→</span>
        </button>
      </div>
    </aside>
  );
}

export default OwnerSidebar;