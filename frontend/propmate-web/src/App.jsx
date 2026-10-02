import { Navigate, Route, Routes } from "react-router-dom";

import Login from "./pages/Login";
import Register from "./pages/Register";

// Component 1 - Property Listing & Approval
import OwnerDashboard from "./pages/owner/OwnerDashboard";
import AdminDashboard from "./pages/AdminDashboard";
import Unauthorized from "./pages/Unauthorized";
import ProtectedRoute from "./components/ProtectedRoute";
import { useAuth } from "./context/authContext";
import CreateListing from "./features/property-listings/pages/owner/CreateListing";
import MyListings from "./features/property-listings/pages/owner/MyListings";
import EditListing from "./features/property-listings/pages/owner/EditListing";
import AdminReviewListing from "./features/property-listings/pages/admin/AdminReviewListing";

// Component 2 - Property Discovery & Viewing
import PropertyDiscovery from "./pages/discovery/PropertyDiscovery";
import PropertyDetails from "./pages/discovery/PropertyDetails";
import PropertyLocation from "./pages/discovery/PropertyLocation";
import ViewingBookings from "./pages/discovery/ViewingBookings";
import ViewingManagement from "./pages/owner/ViewingManagement";

// Component 3 - Applications, Offers & Transactions
import OwnerRentalApplications from "./features/transactions/pages/OwnerRentalApplications";
import OwnerPurchaseOffers from "./features/transactions/pages/OwnerPurchaseOffers";
import OwnerTransactionPage from "./features/transactions/pages/OwnerTransactionPage";
import AdminTransactions from "./features/transactions/pages/AdminTransactions";
import "./features/transactions/styles/component3.css";

// Component 4 - Maintenance Management
import StakeholderDashboard from "./pages/StakeholderDashboard";
import "./App.css";
import "./ai-workflow.css";
import "./technician-options.css";

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

  return <Navigate to="/discover" replace />;
}

function App() {
  return (
    <Routes>
      <Route path="/" element={<HomeRedirect />} />

      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<Register />} />

      {/* Property Discovery & Viewing */}
      <Route
        path="/discover"
        element={<PropertyDiscovery />}
      />

      <Route
        path="/discover/:id"
        element={<PropertyDetails />}
      />

      <Route
        path="/discover/:id/location"
        element={<PropertyLocation />}
      />

      <Route
        path="/discover/:id/viewing-slots"
        element={<ViewingBookings />}
      />

      {/* Owner - Property Listing & Approval */}
      <Route
        path="/owner"
        element={
          <ProtectedRoute allowedRoles={["OwnerAgent"]}>
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

      {/* Owner - Viewing Management */}
      <Route
        path="/owner/viewings"
        element={
          <ProtectedRoute allowedRoles={["OwnerAgent"]}>
            <ViewingManagement />
          </ProtectedRoute>
        }
      />

      {/* Owner - Applications, Offers & Transactions */}
      <Route
        path="/owner/component3/rentals"
        element={
          <ProtectedRoute allowedRoles={["OwnerAgent"]}>
            <OwnerRentalApplications />
          </ProtectedRoute>
        }
      />

      <Route
        path="/owner/component3/purchases"
        element={
          <ProtectedRoute allowedRoles={["OwnerAgent"]}>
            <OwnerPurchaseOffers />
          </ProtectedRoute>
        }
      />

      <Route
        path="/owner/component3/:type/:id"
        element={
          <ProtectedRoute allowedRoles={["OwnerAgent"]}>
            <OwnerTransactionPage />
          </ProtectedRoute>
        }
      />

      {/* Admin */}
      <Route
        path="/admin"
        element={
          <ProtectedRoute allowedRoles={["Admin"]}>
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

      {/* Admin - Applications, Offers & Transactions */}
      <Route
        path="/admin/component3"
        element={
          <ProtectedRoute allowedRoles={["Admin"]}>
            <AdminTransactions />
          </ProtectedRoute>
        }
      />

      <Route
        path="/unauthorized"
        element={<Unauthorized />}
      />

      {/* Component 4 - Maintenance Management */}
      <Route
        path="/maintenance"
        element={<StakeholderDashboard />}
      />

      <Route
        path="*"
        element={<Navigate to="/" replace />}
      />
    </Routes>
  );
}

export default App;