import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import OwnerSidebar from "../../components/owner/OwnerSidebar";
import { getOwnerListings } from "../../services/propertyListingService";
import "./OwnerDashboard.css";

const statusMap = {
  0: "Draft",
  1: "Submitted",
  2: "UnderReview",
  3: "RevisionRequired",
  4: "Approved",
  5: "Rejected",
  6: "Published",
  7: "Unpublished",
};

const purposeMap = {
  0: "Sale",
  1: "Rent",
};

const propertyTypeMap = {
  0: "House",
  1: "Apartment",
  2: "Land",
  3: "Commercial",
};

function getEnumValue(value, map) {
  return typeof value === "number" ? map[value] : value;
}

function formatStatus(status) {
  return status
    ?.replace(/([a-z])([A-Z])/g, "$1 $2")
    .replace(/([A-Z])([A-Z][a-z])/g, "$1 $2");
}

function formatPrice(price) {
  return new Intl.NumberFormat("en-LK", {
    style: "currency",
    currency: "LKR",
    maximumFractionDigits: 0,
  }).format(price);
}

function OwnerDashboard() {
  const [listings, setListings] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    const loadListings = async () => {
      try {
        const data = await getOwnerListings();
        setListings(data);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };

    loadListings();
  }, []);

  const getStatus = (listing) =>
    getEnumValue(listing.status, statusMap);

  const totalListings = listings.length;

  const drafts = listings.filter(
    (listing) => getStatus(listing) === "Draft"
  ).length;

  const underReview = listings.filter((listing) =>
    ["Submitted", "UnderReview"].includes(getStatus(listing))
  ).length;

  const published = listings.filter(
    (listing) => getStatus(listing) === "Published"
  ).length;

  const recentListings = [...listings]
    .sort(
      (a, b) =>
        new Date(b.updatedAt) - new Date(a.updatedAt)
    )
    .slice(0, 4);

  return (
    <div className="owner-layout">
      <OwnerSidebar />

      <main className="owner-main">
        <header className="owner-header">
          <div>
            <p className="page-eyebrow">OWNER WORKSPACE</p>
            <h1>Property overview</h1>
            <p className="page-description">
              Manage your listings and follow their verification
              progress.
            </p>
          </div>

          <Link
            to="/owner/listings/new"
            className="new-listing-button"
          >
            <span>＋</span>
            New listing
          </Link>
        </header>

        {error && (
          <div className="dashboard-error">
            {error}
          </div>
        )}

        <section className="stats-grid">
          <div className="stat-card">
            <div className="stat-top">
              <span>ALL PROPERTIES</span>
              <span className="stat-mark">01</span>
            </div>

            <strong>
              {loading ? "—" : totalListings}
            </strong>

            <p>Total listings</p>
          </div>

          <div className="stat-card">
            <div className="stat-top">
              <span>DRAFTS</span>
              <span className="stat-mark">02</span>
            </div>

            <strong>{loading ? "—" : drafts}</strong>
            <p>Waiting for submission</p>
          </div>

          <div className="stat-card">
            <div className="stat-top">
              <span>IN REVIEW</span>
              <span className="stat-mark gold">03</span>
            </div>

            <strong>{loading ? "—" : underReview}</strong>
            <p>Verification in progress</p>
          </div>

          <div className="stat-card dark-stat">
            <div className="stat-top">
              <span>PUBLISHED</span>
              <span className="stat-mark">04</span>
            </div>

            <strong>{loading ? "—" : published}</strong>
            <p>Live properties</p>
          </div>
        </section>

        <section className="dashboard-section">
          <div className="section-heading">
            <div>
              <p className="page-eyebrow">RECENT ACTIVITY</p>
              <h2>Your listings</h2>
            </div>

            <Link to="/owner/listings">
              View all <span>→</span>
            </Link>
          </div>

          {loading ? (
            <div className="dashboard-empty">
              <p>Loading your properties...</p>
            </div>
          ) : recentListings.length === 0 ? (
            <div className="dashboard-empty">
              <div className="empty-number">01</div>

              <h3>Your property portfolio starts here.</h3>

              <p>
                Create your first listing and submit it for
                intelligent verification.
              </p>

              <Link to="/owner/listings/new">
                Create first listing →
              </Link>
            </div>
          ) : (
            <div className="listing-table-wrapper">
              <table className="listing-table">
                <thead>
                  <tr>
                    <th>PROPERTY</th>
                    <th>TYPE</th>
                    <th>PURPOSE</th>
                    <th>PRICE</th>
                    <th>STATUS</th>
                    <th></th>
                  </tr>
                </thead>

                <tbody>
                  {recentListings.map((listing) => {
                    const status = getStatus(listing);

                    return (
                      <tr key={listing.id}>
                        <td>
                          <div className="property-cell">
                            {listing.imageUrls?.length > 0 ? (
                              <img
                                src={listing.imageUrls[0]}
                                alt={listing.title}
                              />
                            ) : (
                              <div className="property-placeholder">
                                P
                              </div>
                            )}

                            <div>
                              <strong>{listing.title}</strong>
                              <span>{listing.city}</span>
                            </div>
                          </div>
                        </td>

                        <td>
                          {getEnumValue(
                            listing.propertyType,
                            propertyTypeMap
                          )}
                        </td>

                        <td>
                          {getEnumValue(
                            listing.purpose,
                            purposeMap
                          )}
                        </td>

                        <td className="price-cell">
                          {formatPrice(listing.price)}
                        </td>

                        <td>
                          <span
                            className={`status-chip status-${status?.toLowerCase()}`}
                          >
                            {formatStatus(status)}
                          </span>
                        </td>

                        <td>
                          <Link
                            className="table-action"
                            to={`/owner/listings/${listing.id}/edit`}
                          >
                            →
                          </Link>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          )}
        </section>
      </main>
    </div>
  );
}

export default OwnerDashboard;