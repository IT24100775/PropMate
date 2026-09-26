import { useCallback, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { c3Api } from "../component3/services/api";
import { useAuth } from "../context/authContext";
import { getPublishedListings } from "../services/propertyListingService";
import { maintenanceApi } from "../services/maintenanceApi";
import "./TenantDashboard.css";

const statusText = (value) =>
  typeof value === "string" ? value.replaceAll("_", " ") : "Pending";

function TenantDashboard() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const tenantId = Number(user?.userId);
  const [view, setView] = useState("properties");
  const [listings, setListings] = useState([]);
  const [rentals, setRentals] = useState([]);
  const [purchases, setPurchases] = useState([]);
  const [activeRentals, setActiveRentals] = useState([]);
  const [requests, setRequests] = useState([]);
  const [notifications, setNotifications] = useState([]);
  const [action, setAction] = useState(null);
  const [details, setDetails] = useState(null);
  const [rentalForm, setRentalForm] = useState({
    employment: "",
    monthlyIncome: "",
    occupants: "1",
    preferredMoveInDate: "",
    durationMonths: "12",
    message: "",
  });
  const [purchaseForm, setPurchaseForm] = useState({ offerAmount: "", conditions: "" });
  const [maintenanceForm, setMaintenanceForm] = useState({
    propertyId: "",
    description: "",
    category: "General",
    priority: "MEDIUM",
  });
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");

  const loadDashboard = useCallback(async () => {
    try {
      setLoading(true);
      setError("");
      const [propertyData, rentalData, purchaseData, requestData, notificationData] =
        await Promise.all([
          getPublishedListings(),
          c3Api.rentalMine(),
          c3Api.purchaseMine(),
          maintenanceApi.getForTenant(tenantId),
          maintenanceApi.getNotificationsForTenant(tenantId),
        ]);
      const agreements = await Promise.all(
        rentalData.map((rental) => c3Api.rentalAgreement(rental.id).catch(() => null)),
      );
      setListings(propertyData);
      setRentals(rentalData);
      setPurchases(purchaseData);
      setActiveRentals(
        rentalData.filter((_, index) => agreements[index]?.status === "Completed"),
      );
      setRequests(Array.isArray(requestData) ? requestData : requestData?.items || []);
      setNotifications(notificationData);
    } catch (loadError) {
      setError(loadError.message || "Could not load your tenant workspace.");
    } finally {
      setLoading(false);
    }
  }, [tenantId]);

  useEffect(() => {
    const timer = window.setTimeout(() => void loadDashboard(), 0);
    return () => window.clearTimeout(timer);
  }, [loadDashboard]);

  const submitAction = async (event) => {
    event.preventDefault();
    if (!action) return;
    try {
      setSaving(true);
      setError("");
      if (action.kind === "rent") {
        await c3Api.rentalCreate({
          propertyListingId: action.listing.id,
          employment: rentalForm.employment,
          monthlyIncome: Number(rentalForm.monthlyIncome),
          occupants: Number(rentalForm.occupants),
          preferredMoveInDate: rentalForm.preferredMoveInDate,
          durationMonths: Number(rentalForm.durationMonths),
          message: rentalForm.message || null,
        });
        setMessage("Rental application submitted.");
      } else {
        await c3Api.purchaseCreate({
          propertyListingId: action.listing.id,
          offerAmount: Number(purchaseForm.offerAmount),
          conditions: purchaseForm.conditions || null,
        });
        setMessage("Purchase offer submitted.");
      }
      setAction(null);
      await loadDashboard();
    } catch (submitError) {
      setError(submitError.message || "Could not submit your request.");
    } finally {
      setSaving(false);
    }
  };

  const submitMaintenanceRequest = async (event) => {
    event.preventDefault();
    try {
      setSaving(true);
      setError("");
      await maintenanceApi.createForTenant({
        propertyListingId: Number(maintenanceForm.propertyId),
        description: maintenanceForm.description.trim(),
        category: maintenanceForm.category,
        priority: maintenanceForm.priority,
      });
      setMaintenanceForm({ ...maintenanceForm, description: "" });
      setMessage("Maintenance request sent to the property manager.");
      await loadDashboard();
    } catch (submitError) {
      setError(submitError.message || "Could not send the maintenance request.");
    } finally {
      setSaving(false);
    }
  };

  const signOut = () => {
    logout();
    navigate("/login");
  };

  return (
    <main className="tenant-shell">
      <header className="tenant-header">
        <a className="tenant-brand" href="/tenant">PropMate</a>
        <nav aria-label="Tenant workspace">
          <button className={view === "properties" ? "active" : ""} onClick={() => setView("properties")}>Properties</button>
          <button className={view === "activity" ? "active" : ""} onClick={() => setView("activity")}>My activity</button>
          <button className={view === "maintenance" ? "active" : ""} onClick={() => setView("maintenance")}>Maintenance</button>
        </nav>
        <div className="tenant-account"><span>{user?.email}</span><button onClick={signOut}>Sign out</button></div>
      </header>

      <div className="tenant-content">
        <div className="tenant-heading">
          <div><p className="tenant-eyebrow">TENANT WORKSPACE</p><h1>{view === "properties" ? "Find a place to call home." : view === "activity" ? "Your applications and offers" : "Care for your home."}</h1></div>
          <button className="tenant-refresh" onClick={() => void loadDashboard()} disabled={loading}>Refresh</button>
        </div>
        {message && <div className="tenant-alert success">{message}<button onClick={() => setMessage("")}>×</button></div>}
        {error && <div className="tenant-alert error">{error}<button onClick={() => setError("")}>×</button></div>}

        {loading ? <p className="tenant-empty">Loading your workspace…</p> : null}

        {!loading && view === "properties" && <section className="tenant-property-grid">
          {listings.map((listing) => <article className="tenant-property" key={listing.id}>
            {listing.imageUrls?.[0] ? <img src={listing.imageUrls[0]} alt={listing.title} /> : <div className="tenant-image-placeholder">PropMate homes</div>}
            <div className="tenant-property-body">
              <div className="tenant-property-meta"><span>{listing.purpose}</span><span>{listing.city}</span></div>
              <h2>{listing.title}</h2><p>{listing.address}</p>
              <strong className="tenant-price">LKR {Number(listing.price).toLocaleString()}</strong>
              <div className="tenant-property-actions">
                <button className="tenant-secondary" onClick={() => setDetails(listing)}>Details</button>
                {listing.purpose === "Rent" ? <button className="tenant-primary" onClick={() => setAction({ listing, kind: "rent" })}>Apply to rent</button> : <button className="tenant-primary" onClick={() => { setPurchaseForm({ offerAmount: String(listing.price), conditions: "" }); setAction({ listing, kind: "purchase" }); }}>Make an offer</button>}
              </div>
            </div>
          </article>)}
          {!listings.length && <p className="tenant-empty">No published properties are available right now.</p>}
        </section>}

        {!loading && view === "activity" && <div className="tenant-activity-grid">
          <section><h2>Rental applications</h2>{rentals.map((item) => <article className="tenant-row" key={item.id}><div><strong>{item.propertyTitle}</strong><span>Move-in {item.preferredMoveInDate} · {item.durationMonths} months</span></div><b>{statusText(item.status)}</b></article>)}{!rentals.length && <p className="tenant-empty">No rental applications yet.</p>}</section>
          <section><h2>Purchase offers</h2>{purchases.map((item) => <article className="tenant-row" key={item.id}><div><strong>{item.propertyTitle}</strong><span>LKR {Number(item.offerAmount).toLocaleString()}</span></div><b>{statusText(item.status)}</b></article>)}{!purchases.length && <p className="tenant-empty">No purchase offers yet.</p>}</section>
        </div>}

        {!loading && view === "maintenance" && <div className="tenant-maintenance-grid">
          <section><h2>Submit a maintenance request</h2>
            {activeRentals.length ? <form className="tenant-form" onSubmit={submitMaintenanceRequest}>
              <label>Rented property<select required value={maintenanceForm.propertyId} onChange={(event) => setMaintenanceForm({ ...maintenanceForm, propertyId: event.target.value })}><option value="">Choose a property</option>{activeRentals.map((rental) => <option key={rental.id} value={rental.propertyListingId}>{rental.propertyTitle}</option>)}</select></label>
              <label>Issue description<textarea required value={maintenanceForm.description} onChange={(event) => setMaintenanceForm({ ...maintenanceForm, description: event.target.value })} /></label>
              <div className="tenant-form-row"><label>Category<select value={maintenanceForm.category} onChange={(event) => setMaintenanceForm({ ...maintenanceForm, category: event.target.value })}><option>General</option><option>Plumbing</option><option>Electrical</option><option>HVAC</option><option>Appliance</option></select></label><label>Priority<select value={maintenanceForm.priority} onChange={(event) => setMaintenanceForm({ ...maintenanceForm, priority: event.target.value })}><option>LOW</option><option>MEDIUM</option><option>HIGH</option></select></label></div>
              <button className="tenant-primary" disabled={saving}>{saving ? "Sending…" : "Send request"}</button>
            </form> : <p className="tenant-empty">Maintenance requests become available after both sides confirm your rental agreement.</p>}
          </section>
          <section><h2>Request status</h2>{requests.map((request) => <article className="tenant-row" key={request.id}><div><strong>Request #{request.id}</strong><span>{request.description}</span></div><b>{statusText(request.status)}</b></article>)}{!requests.length && <p className="tenant-empty">No maintenance requests yet.</p>}<h2 className="tenant-notification-title">Notifications</h2>{notifications.map((item) => <article className="tenant-notification" key={item.id}><strong>{item.title}</strong><p>{item.message}</p><small>{new Date(item.createdAt).toLocaleString()}</small></article>)}{!notifications.length && <p className="tenant-empty">No notifications yet.</p>}</section>
        </div>}
      </div>

      {action && <div className="tenant-overlay"><section className="tenant-dialog"><button className="tenant-close" onClick={() => setAction(null)} aria-label="Close">×</button><p className="tenant-eyebrow">{action.kind === "rent" ? "RENTAL APPLICATION" : "PURCHASE OFFER"}</p><h2>{action.listing.title}</h2><form className="tenant-form" onSubmit={submitAction}>
        {action.kind === "rent" ? <>
          <label>Employment<input required value={rentalForm.employment} onChange={(event) => setRentalForm({ ...rentalForm, employment: event.target.value })} /></label>
          <div className="tenant-form-row"><label>Monthly income<input type="number" min="1" required value={rentalForm.monthlyIncome} onChange={(event) => setRentalForm({ ...rentalForm, monthlyIncome: event.target.value })} /></label><label>Occupants<input type="number" min="1" required value={rentalForm.occupants} onChange={(event) => setRentalForm({ ...rentalForm, occupants: event.target.value })} /></label></div>
          <div className="tenant-form-row"><label>Preferred move-in<input type="date" required value={rentalForm.preferredMoveInDate} onChange={(event) => setRentalForm({ ...rentalForm, preferredMoveInDate: event.target.value })} /></label><label>Duration (months)<input type="number" min="1" required value={rentalForm.durationMonths} onChange={(event) => setRentalForm({ ...rentalForm, durationMonths: event.target.value })} /></label></div>
          <label>Message (optional)<textarea value={rentalForm.message} onChange={(event) => setRentalForm({ ...rentalForm, message: event.target.value })} /></label>
        </> : <><label>Offer amount (LKR)<input type="number" min="1" required value={purchaseForm.offerAmount} onChange={(event) => setPurchaseForm({ ...purchaseForm, offerAmount: event.target.value })} /></label><label>Conditions (optional)<textarea value={purchaseForm.conditions} onChange={(event) => setPurchaseForm({ ...purchaseForm, conditions: event.target.value })} /></label></>}
        <div className="tenant-dialog-actions"><button type="button" className="tenant-secondary" onClick={() => setAction(null)}>Cancel</button><button className="tenant-primary" disabled={saving}>{saving ? "Submitting…" : "Submit"}</button></div>
      </form></section></div>}
      {details && <div className="tenant-overlay"><section className="tenant-dialog"><button className="tenant-close" onClick={() => setDetails(null)} aria-label="Close">×</button><p className="tenant-eyebrow">{details.purpose} · {details.propertyType}</p><h2>{details.title}</h2><p>{details.address}, {details.city}</p><p>{details.description}</p><p>{details.bedrooms} bedrooms · {details.bathrooms} bathrooms</p><strong className="tenant-price">LKR {Number(details.price).toLocaleString()}</strong></section></div>}
    </main>
  );
}

export default TenantDashboard;