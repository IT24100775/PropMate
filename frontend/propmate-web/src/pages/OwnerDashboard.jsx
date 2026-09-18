import { useNavigate } from "react-router-dom";
import { useAuth } from "../context/authContext";

function OwnerDashboard() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  return (
    <div>
      <h1>Owner / Agent Dashboard</h1>
      <p>Logged in as: {user.email}</p>
      <p>Role: {user.role}</p>

      <button onClick={handleLogout}>
        Logout
      </button>
    </div>
  );
}

export default OwnerDashboard;