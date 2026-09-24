import { Navigate, Route, Routes } from "react-router-dom";

import Login from "./pages/Login";
import Register from "./pages/Register";
import PropertyManagement from "./pages/staff/PropertyManagement";
import PropertyDiscovery from "./pages/discovery/PropertyDiscovery";
import PropertyDetails from "./pages/discovery/PropertyDetails";
import PropertyLocation from "./pages/discovery/PropertyLocation";
import ViewingBookings from "./pages/discovery/ViewingBookings";

function App() {
  return (
    <Routes>
      <Route
        path="/login"
        element={<Login />}
      />
    <Route
        path="/register"
        element={<Register />}
      />
      <Route
        path="/"
        element={<Navigate to="/discover" replace />}
      />

      <Route
        path="/staff/properties"
        element={<PropertyManagement />}
      />
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

      <Route
        path="*"
        element={<Navigate to="/discover" replace />}
      />
    </Routes>
  );
}

export default App;


