import { Navigate, Route, Routes } from "react-router-dom";
import AdminDashboard from "./pages/AdminDashboard";
import AdminReviewListing from "./pages/AdminReviewListing";
import AdminTransactions from "./component3/pages/AdminTransactions";
import CreateListing from "./pages/owner/CreateListing";
import EditListing from "./pages/owner/EditListing";
import WorkspacePicker from "./pages/WorkspacePicker";
import MyListings from "./pages/owner/MyListings";
import OwnerDashboard from "./pages/owner/OwnerDashboard";
import OwnerPurchaseOffers from "./component3/pages/OwnerPurchaseOffers";
import OwnerRentalApplications from "./component3/pages/OwnerRentalApplications";
import OwnerTransactionPage from "./component3/pages/OwnerTransactionPage";
import StakeholderDashboard from "./pages/StakeholderDashboard";
import TenantDashboard from "./pages/TenantDashboard";
import "./App.css";
import "./ai-workflow.css";
import "./technician-options.css";
import "./component3/styles/component3.css";

function App() {
  return (
    <Routes>
      <Route path="/" element={<WorkspacePicker />} />
      <Route path="/workspaces" element={<WorkspacePicker />} />
      <Route path="/tenant" element={<TenantDashboard />} />
      <Route path="/maintenance" element={<StakeholderDashboard />} />
      <Route path="/property-management" element={<StakeholderDashboard />} />
      <Route path="/owner" element={<OwnerDashboard />} />
      <Route path="/owner/listings" element={<MyListings />} />
      <Route path="/owner/listings/new" element={<CreateListing />} />
      <Route path="/owner/listings/:id/edit" element={<EditListing />} />
      <Route path="/owner/component3/rentals" element={<OwnerRentalApplications />} />
      <Route path="/owner/component3/purchases" element={<OwnerPurchaseOffers />} />
      <Route path="/owner/component3/:type/:id" element={<OwnerTransactionPage />} />
      <Route path="/admin" element={<AdminDashboard />} />
      <Route path="/admin/listings/:id/review" element={<AdminReviewListing />} />
      <Route path="/admin/component3" element={<AdminTransactions />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

export default App;
