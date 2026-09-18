import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import OwnerSidebar from "../../components/owner/OwnerSidebar";
import {
  deleteListing,
  getOwnerListings,
  submitListing,
} from "../../services/propertyListingService";
import "./MyListings.css";

function formatStatus(status) {
  return status?.replace(/([a-z])([A-Z])/g, "$1 $2");
}

function formatPrice(price, purpose) {
  const formatted = new Intl.NumberFormat("en-LK", {
    style: "currency",
    currency: "LKR",
    maximumFractionDigits: 0,
  }).format(price);

  return purpose === "Rent" ? `${formatted} / month` : formatted;
}

function MyListings() {
  const [listings, setListings] = useState([]);
  const [filter, setFilter] = useState("All");
  const [search, setSearch] = useState("");
  const [loading, setLoading] = useState(true);
  const [actionId, setActionId] = useState(null);
  const [error, setError] = useState("");

  useEffect(() => {
  let cancelled = false;

  async function fetchListings() {
    try {
      const data = await getOwnerListings();

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
    return listings.filter((listing) => {
      const matchesSearch =
        listing.title.toLowerCase().includes(search.toLowerCase()) ||
        listing.city.toLowerCase().includes(search.toLowerCase());

      let matchesFilter = true;

      if (filter === "Draft") {
        matchesFilter = listing.status === "Draft";
      }

      if (filter === "In Review") {
        matchesFilter = ["Submitted", "UnderReview"].includes(
          listing.status
        );
      }

      if (filter === "Published") {
        matchesFilter = listing.status === "Published";
      }

      if (filter === "Revision") {
        matchesFilter = listing.status === "RevisionRequired";
      }

      if (filter === "Rejected") {
        matchesFilter = listing.status === "Rejected";
      }

      return matchesSearch && matchesFilter;
    });
  }, [listings, filter, search]);

  const getCount = (type) => {
    if (type === "All") return listings.length;

    if (type === "In Review") {
      return listings.filter((listing) =>
        ["Submitted", "UnderReview"].includes(listing.status)
      ).length;
    }

    if (type === "Revision") {
      return listings.filter(
        (listing) => listing.status === "RevisionRequired"
      ).length;
    }

    return listings.filter((listing) => listing.status === type).length;
  };

  const handleDelete = async (id) => {
    const confirmed = window.confirm(
      "Delete this draft listing? This action cannot be undone."
    );

    if (!confirmed) return;

    try {
      setActionId(id);
      setError("");

      await deleteListing(id);

      setListings((previous) =>
        previous.filter((listing) => listing.id !== id)
      );
    } catch (err) {
      setError(err.message);
    } finally {
      setActionId(null);
    }
  };

  const handleSubmit = async (id) => {
    const confirmed = window.confirm(
      "Submit this listing for verification? You will not be able to edit it while it is under review."
    );

    if (!confirmed) return;

    try {
      setActionId(id);
      setError("");

      const updatedListing = await submitListing(id);

      setListings((previous) =>
        previous.map((listing) =>
          listing.id === id ? updatedListing : listing
        )
      );
    } catch (err) {
      setError(err.message);
    } finally {
      setActionId(null);
    }
  };

  const filters = [
    "All",
    "Draft",
    "In Review",
    "Published",
    "Revision",
    "Rejected",
  ];

  return (
    <div className="owner-layout">
      <OwnerSidebar />

      <main className="owner-main my-listings-main">
        <header className="my-listings-header">
          <div>
            <p className="page-eyebrow">PROPERTY PORTFOLIO</p>
            <h1>My listings</h1>
            <p className="page-description">
              Create, manage and follow every property through the
              verification process.
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

        {error && <div className="dashboard-error">{error}</div>}

        <section className="listing-controls">
          <div className="listing-filters">
            {filters.map((item) => (
              <button
                key={item}
                type="button"
                className={filter === item ? "active" : ""}
                onClick={() => setFilter(item)}
              >
                {item}
                <span>{getCount(item)}</span>
              </button>
            ))}
          </div>

          <div className="listing-search">
            <span>⌕</span>
            <input
              type="text"
              placeholder="Search by property or city..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </div>
        </section>

        {loading ? (
          <div className="dashboard-empty">
            <p>Loading your properties...</p>
          </div>
        ) : filteredListings.length === 0 ? (
          <div className="dashboard-empty">
            <div className="empty-number">00</div>

            <h3>No properties found.</h3>

            <p>
              {listings.length === 0
                ? "Create your first property listing to get started."
                : "There are no properties matching this filter."}
            </p>

            {listings.length === 0 && (
              <Link to="/owner/listings/new">
                Create a listing →
              </Link>
            )}
          </div>
        ) : (
          <section className="property-list">
            {filteredListings.map((listing, index) => (
              <article
                className="property-list-card"
                key={listing.id}
              >
                <div className="listing-index">
                  {String(index + 1).padStart(2, "0")}
                </div>

                <div className="listing-image">
                  {listing.imageUrls?.length > 0 ? (
                    <img
                      src={listing.imageUrls[0]}
                      alt={listing.title}
                    />
                  ) : (
                    <div className="listing-no-image">
                      <span>P</span>
                      <small>NO IMAGE</small>
                    </div>
                  )}
                </div>

                <div className="listing-card-content">
                  <div className="listing-card-top">
                    <div>
                      <div className="listing-meta">
                        <span>{listing.propertyType}</span>
                        <i></i>
                        <span>
                          {listing.purpose === "Sale"
                            ? "FOR SALE"
                            : "FOR RENT"}
                        </span>
                      </div>

                      <h2>{listing.title}</h2>

                      <p className="listing-location">
                        {listing.address}, {listing.city}
                      </p>
                    </div>

                    <span
                      className={`status-chip status-${listing.status.toLowerCase()}`}
                    >
                      {formatStatus(listing.status)}
                    </span>
                  </div>

                  <div className="listing-card-details">
                    <div>
                      <span>PRICE</span>
                      <strong>
                        {formatPrice(
                          listing.price,
                          listing.purpose
                        )}
                      </strong>
                    </div>

                    <div>
                      <span>BEDROOMS</span>
                      <strong>{listing.bedrooms}</strong>
                    </div>

                    <div>
                      <span>BATHROOMS</span>
                      <strong>{listing.bathrooms}</strong>
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

                  <div className="listing-card-actions">
                    {(listing.status === "Draft" ||
                      listing.status === "RevisionRequired") && (
                      <Link
                        to={`/owner/listings/${listing.id}/edit`}
                        className="secondary-listing-action"
                      >
                        Edit
                      </Link>
                    )}

                    {listing.status === "Draft" && (
                      <button
                        type="button"
                        className="delete-listing-action"
                        disabled={actionId === listing.id}
                        onClick={() => handleDelete(listing.id)}
                      >
                        Delete
                      </button>
                    )}

                    {listing.status === "Draft" && (
                      <button
                        type="button"
                        className="submit-listing-action"
                        disabled={actionId === listing.id}
                        onClick={() => handleSubmit(listing.id)}
                      >
                        {actionId === listing.id
                          ? "Processing..."
                          : "Submit for verification"}
                        {actionId !== listing.id && <span>→</span>}
                      </button>
                    )}

                    {listing.status === "RevisionRequired" && (
                      <div className="revision-notice">
                        Revision requested — update the listing before
                        resubmitting.
                      </div>
                    )}

                    {["Submitted", "UnderReview"].includes(
                      listing.status
                    ) && (
                      <div className="review-notice">
                        <span></span>
                        Verification in progress
                      </div>
                    )}

                    {listing.status === "Published" && (
                      <div className="published-notice">
                        ✓ Live on PropMate
                      </div>
                    )}

                    {listing.status === "Rejected" && (
                      <div className="rejected-notice">
                        Listing rejected
                      </div>
                    )}
                  </div>
                </div>
              </article>
            ))}
          </section>
        )}
      </main>
    </div>
  );
}

export default MyListings;