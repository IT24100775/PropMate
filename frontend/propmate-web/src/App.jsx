import { Navigate, Route, Routes } from "react-router-dom";
import AdminDashboard from "./pages/AdminDashboard";
import AdminReviewListing from "./pages/AdminReviewListing";
import AdminTransactions from "./component3/pages/AdminTransactions";
import CreateListing from "./pages/owner/CreateListing";
import EditListing from "./pages/owner/EditListing";
import Login from "./pages/Login";
import MyListings from "./pages/owner/MyListings";
import OwnerDashboard from "./pages/owner/OwnerDashboard";
import OwnerPurchaseOffers from "./component3/pages/OwnerPurchaseOffers";
import OwnerRentalApplications from "./component3/pages/OwnerRentalApplications";
import OwnerTransactionPage from "./component3/pages/OwnerTransactionPage";
import ProtectedRoute from "./components/ProtectedRoute";
import Register from "./pages/Register";
import StakeholderDashboard from "./pages/StakeholderDashboard";
import TenantDashboard from "./pages/TenantDashboard";
import Unauthorized from "./pages/Unauthorized";
import { useAuth } from "./context/authContext";
import "./App.css";
import "./ai-workflow.css";
import "./technician-options.css";
import "./component3/styles/component3.css";

function HomeRedirect() {
  const { user } = useAuth();

  if (!user) return <Navigate to="/login" replace />;
  if (user.role === "Admin") return <Navigate to="/admin" replace />;
  if (user.role === "OwnerAgent") return <Navigate to="/owner" replace />;
  if (user.role === "PropertyManager") return <Navigate to="/maintenance" replace />;
  if (user.role === "BuyerRenter") return <Navigate to="/tenant" replace />;
  return <Navigate to="/unauthorized" replace />;
}

function App() {
  return (
    <Routes>
      <Route path="/" element={<HomeRedirect />} />
      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<Register />} />
      <Route path="/tenant" element={<ProtectedRoute allowedRoles={["BuyerRenter"]}><TenantDashboard /></ProtectedRoute>} />
      <Route path="/maintenance" element={<ProtectedRoute allowedRoles={["PropertyManager"]}><StakeholderDashboard /></ProtectedRoute>} />
      <Route path="/property-management" element={<ProtectedRoute allowedRoles={["PropertyManager"]}><StakeholderDashboard /></ProtectedRoute>} />
      <Route path="/owner" element={<ProtectedRoute allowedRoles={["OwnerAgent"]}><OwnerDashboard /></ProtectedRoute>} />
      <Route path="/owner/listings" element={<ProtectedRoute allowedRoles={["OwnerAgent"]}><MyListings /></ProtectedRoute>} />
      <Route path="/owner/listings/new" element={<ProtectedRoute allowedRoles={["OwnerAgent"]}><CreateListing /></ProtectedRoute>} />
      <Route path="/owner/listings/:id/edit" element={<ProtectedRoute allowedRoles={["OwnerAgent"]}><EditListing /></ProtectedRoute>} />
      <Route path="/owner/component3/rentals" element={<ProtectedRoute allowedRoles={["OwnerAgent"]}><OwnerRentalApplications /></ProtectedRoute>} />
      <Route path="/owner/component3/purchases" element={<ProtectedRoute allowedRoles={["OwnerAgent"]}><OwnerPurchaseOffers /></ProtectedRoute>} />
      <Route path="/owner/component3/:type/:id" element={<ProtectedRoute allowedRoles={["OwnerAgent"]}><OwnerTransactionPage /></ProtectedRoute>} />
      <Route path="/admin" element={<ProtectedRoute allowedRoles={["Admin"]}><AdminDashboard /></ProtectedRoute>} />
      <Route path="/admin/listings/:id/review" element={<ProtectedRoute allowedRoles={["Admin"]}><AdminReviewListing /></ProtectedRoute>} />
      <Route path="/admin/component3" element={<ProtectedRoute allowedRoles={["Admin"]}><AdminTransactions /></ProtectedRoute>} />
      <Route path="/unauthorized" element={<Unauthorized />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

export default App;
