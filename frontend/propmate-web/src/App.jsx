import {
  Navigate,
  Route,
  Routes,
} from "react-router-dom";

import Login from "./pages/Login";
import Register from "./pages/Register";
import OwnerDashboard from "./pages/owner/OwnerDashboard";
import AdminDashboard from "./pages/AdminDashboard";
import Unauthorized from "./pages/Unauthorized";
import ProtectedRoute from "./components/ProtectedRoute";
import { useAuth } from "./context/authContext";
import CreateListing from "./pages/owner/CreateListing";
import MyListings from "./pages/owner/MyListings";
import EditListing from "./pages/owner/EditListing"; 
import AdminReviewListing from "./pages/AdminReviewListing";
import OwnerRentalApplications from "./component3/pages/OwnerRentalApplications";
import OwnerPurchaseOffers from "./component3/pages/OwnerPurchaseOffers";
import OwnerTransactionPage from "./component3/pages/OwnerTransactionPage";
import AdminTransactions from "./component3/pages/AdminTransactions";
import "./component3/styles/component3.css";

function HomeRedirect() {
  const { user } = useAuth();

  if (!user) {
    return <Navigate to="/login" replace />;
  }

  if (user.role === "Admin") {
    return <Navigate to="/admin" replace />;
  }

  if (user.role === "OwnerAgent") {
    return <Navigate to="/owner" replace />;
  }

  return <Navigate to="/unauthorized" replace />;
}

function App() {
  return (
    <Routes>
      <Route
        path="/"
        element={<HomeRedirect />}
      />

      <Route
        path="/login"
        element={<Login />}
      />

      <Route
        path="/register"
        element={<Register />}
      />

      <Route
        path="/owner"
        element={
          <ProtectedRoute
            allowedRoles={["OwnerAgent"]}
          >
            <OwnerDashboard />
          </ProtectedRoute>
        }
      />

      <Route
        path="/owner/listings"
        element={
          <ProtectedRoute allowedRoles={["OwnerAgent"]}>
            <MyListings />
          </ProtectedRoute>
        }
      />

      <Route
        path="/owner/listings/new"
        element={
          <ProtectedRoute allowedRoles={["OwnerAgent"]}>
            <CreateListing />
          </ProtectedRoute>
        }
      />

      <Route
        path="/owner/listings/:id/edit"
        element={
          <ProtectedRoute allowedRoles={["OwnerAgent"]}>
            <EditListing />
          </ProtectedRoute>
        }
      />

      <Route
        path="/admin"
        element={
          <ProtectedRoute
            allowedRoles={["Admin"]}
          >
            <AdminDashboard />
          </ProtectedRoute>
        }
      />

      <Route
        path="/admin/listings/:id/review"
        element={
          <ProtectedRoute allowedRoles={["Admin"]}>
            <AdminReviewListing />
          </ProtectedRoute>
        }
      />


      <Route path="/owner/component3/rentals" element={<ProtectedRoute allowedRoles={["OwnerAgent"]}><OwnerRentalApplications /></ProtectedRoute>} />
      <Route path="/owner/component3/purchases" element={<ProtectedRoute allowedRoles={["OwnerAgent"]}><OwnerPurchaseOffers /></ProtectedRoute>} />
      <Route path="/owner/component3/:type/:id" element={<ProtectedRoute allowedRoles={["OwnerAgent"]}><OwnerTransactionPage /></ProtectedRoute>} />

      <Route path="/admin/component3" element={<ProtectedRoute allowedRoles={["Admin"]}><AdminTransactions /></ProtectedRoute>} />

      <Route
        path="/unauthorized"
        element={<Unauthorized />}
      />

      <Route
        path="*"
        element={<Navigate to="/" replace />}
      />
    </Routes>
  );
}

export default App;