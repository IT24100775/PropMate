import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../context/authContext";
import { getAdminListings } from "../services/propertyListingService";
import "./AdminDashboard.css";
import logoWhite from "../assets/logo-white.png";

function formatStatus(status) {
  return status?.replace(/([a-z])([A-Z])/g, "$1 $2");
}

function formatPrice(price) {
  return new Intl.NumberFormat("en-LK", {
    style: "currency",
    currency: "LKR",
    maximumFractionDigits: 0,
  }).format(price);
}

function AdminDashboard() {
  const { user, logout } = useAuth();

  const [listings, setListings] = useState([]);
  const [filter, setFilter] = useState("All");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    let cancelled = false;

    async function fetchListings() {
      try {
        const data = await getAdminListings();

        if (!cancelled) {
          setListings(data);
        }
      } catch (err) {
        if (!cancelled) {
          setError(err.message);
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    fetchListings();

    return () => {
      cancelled = true;
    };
  }, []);

  const filteredListings = useMemo(() => {
    if (filter === "All") {
      return listings;
    }

    if (filter === "Revision") {
      return listings.filter(
        (listing) => listing.status === "RevisionRequired"
      );
    }

    return listings.filter(
      (listing) => listing.status === filter
    );
  }, [listings, filter]);

  const getCount = (filterName) => {
    if (filterName === "All") {
      return listings.length;
    }

    if (filterName === "Revision") {
      return listings.filter(
        (listing) => listing.status === "RevisionRequired"
      ).length;
    }

    return listings.filter(
      (listing) => listing.status === filterName
    ).length;
  };

  const filters = [
    "All",
    "UnderReview",
    "Approved",
    "Revision",
    "Rejected",
    "Published",
  ];

  return (
    <div className="admin-layout">
      <aside className="admin-sidebar">
        <div>
          <img
            src={logoWhite}
            alt="PropMate"
            className="admin-logo"
          />

          <div className="admin-sidebar-label">
            ADMINISTRATION
          </div>

          <nav>
            <Link to="/admin" className="admin-nav-active">
              <span>01</span>
              Verification Queue
            </Link>

            <Link to="/admin/component3" className="admin-nav-active">
              <span>02</span>
              Component 3 Transactions
            </Link>
          </nav>
        </div>

        <div className="admin-profile">
          <div className="admin-profile-icon">A</div>

          <div>
            <small>ADMINISTRATOR</small>
            <p>{user?.email}</p>
          </div>

          <button type="button" onClick={logout}>
            ↗
          </button>
        </div>
      </aside>

      <main className="admin-main">
        <header className="admin-header">
          <div>
            <p className="admin-eyebrow">
              PROPERTY GOVERNANCE
            </p>

            <h1>Verification queue</h1>

            <p className="admin-description">
              Review submitted properties, inspect AI
              verification findings and make the final
              publishing decision.
            </p>
          </div>

          <div className="admin-review-count">
            <strong>{getCount("UnderReview")}</strong>
            <span>AWAITING REVIEW</span>
          </div>
        </header>

        <section className="admin-stats">
          <div>
            <span>01</span>
            <strong>{listings.length}</strong>
            <p>All Listings</p>
          </div>

          <div>
            <span>02</span>
            <strong>{getCount("UnderReview")}</strong>
            <p>Under Review</p>
          </div>

          <div>
            <span>03</span>
            <strong>{getCount("Approved")}</strong>
            <p>Approved</p>
          </div>

          <div>
            <span>04</span>
            <strong>{getCount("Published")}</strong>
            <p>Published</p>
          </div>
        </section>

        <section className="admin-queue">
          <div className="admin-queue-heading">
            <div>
              <p className="admin-eyebrow">
                LISTING MANAGEMENT
              </p>
              <h2>Property reviews</h2>
            </div>

            <div className="admin-filters">
              {filters.map((item) => (
                <button
                  key={item}
                  type="button"
                  className={filter === item ? "active" : ""}
                  onClick={() => setFilter(item)}
                >
                  {formatStatus(item)}
                  <span>{getCount(item)}</span>
                </button>
              ))}
            </div>
          </div>

          {error && (
            <div className="admin-error">
              {error}
            </div>
          )}

          {loading ? (
            <div className="admin-empty">
              Loading verification queue...
            </div>
          ) : filteredListings.length === 0 ? (
            <div className="admin-empty">
              <strong>Nothing here.</strong>
              <p>
                There are currently no listings in this
                category.
              </p>
            </div>
          ) : (
            <div className="admin-listings">
              {filteredListings.map((listing, index) => (
                <article
                  className="admin-listing-card"
                  key={listing.id}
                >
                  <div className="admin-listing-number">
                    {String(index + 1).padStart(2, "0")}
                  </div>

                  <div className="admin-listing-image">
                    {listing.imageUrls?.length ? (
                      <img
                        src={listing.imageUrls[0]}
                        alt={listing.title}
                      />
                    ) : (
                      <div className="admin-no-image">
                        P
                      </div>
                    )}
                  </div>

                  <div className="admin-listing-info">
                    <div className="admin-listing-top">
                      <div>
                        <div className="admin-listing-meta">
                          {listing.propertyType}
                          <span>•</span>
                          {listing.purpose === "Sale"
                            ? "FOR SALE"
                            : "FOR RENT"}
                        </div>

                        <h3>{listing.title}</h3>

                        <p>
                          {listing.address}, {listing.city}
                        </p>
                      </div>

                      <span
                        className={`admin-status admin-status-${listing.status.toLowerCase()}`}
                      >
                        {formatStatus(listing.status)}
                      </span>
                    </div>

                    <div className="admin-listing-bottom">
                      <div className="admin-property-data">
                        <div>
                          <span>PRICE</span>
                          <strong>
                            {formatPrice(listing.price)}
                          </strong>
                        </div>

                        <div>
                          <span>OWNER ID</span>
                          <strong>
                            #{listing.ownerId}
                          </strong>
                        </div>

                        <div>
                          <span>UPDATED</span>
                          <strong>
                            {new Date(
                              listing.updatedAt
                            ).toLocaleDateString()}
                          </strong>
                        </div>
                      </div>

                      {listing.status ===
                        "UnderReview" && (
                        <Link
                          to={`/admin/listings/${listing.id}/review`}
                          className="review-property-button"
                        >
                          Review property
                          <span>→</span>
                        </Link>
                      )}

                      {listing.status === "Approved" && (
                        <Link
                          to={`/admin/listings/${listing.id}/review`}
                          className="review-property-button"
                        >
                          Open & publish
                          <span>→</span>
                        </Link>
                      )}

                      {![
                        "UnderReview",
                        "Approved",
                      ].includes(listing.status) && (
                        <Link
                          to={`/admin/listings/${listing.id}/review`}
                          className="view-property-button"
                        >
                          View details →
                        </Link>
                      )}
                    </div>
                  </div>
                </article>
              ))}
            </div>
          )}
        </section>
      </main>
    </div>
  );
}

export default AdminDashboard;