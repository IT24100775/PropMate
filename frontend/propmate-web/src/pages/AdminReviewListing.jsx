import { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import {
  approveListing,
  getVerificationReview,
  publishListing,
  rejectListing,
  requestListingRevision,
  unpublishListing,
} from "../services/propertyListingService";
import logoWhite from "../assets/logo-white.png";
import "./AdminReviewListing.css";

function formatPrice(price) {
  return new Intl.NumberFormat("en-LK", {
    style: "currency",
    currency: "LKR",
    maximumFractionDigits: 0,
  }).format(price);
}

function formatStatus(status) {
  return status?.replace(/([a-z])([A-Z])/g, "$1 $2");
}

function formatToolName(name) {
  if (!name) return "Verification Check";

  return name
    .replace(/_/g, " ")
    .replace(/\b\w/g, (letter) => letter.toUpperCase());
}

function AdminReviewListing() {
  const { id } = useParams();
  const navigate = useNavigate();

  const [review, setReview] = useState(null);
  const [reason, setReason] = useState("");
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState(false);
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");

  useEffect(() => {
    let cancelled = false;

    async function fetchReview() {
      try {
        const data = await getVerificationReview(id);

        if (!cancelled) {
          setReview(data);
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

    fetchReview();

    return () => {
      cancelled = true;
    };
  }, [id]);

  async function refreshReview() {
    const data = await getVerificationReview(id);
    setReview(data);
  }

  async function handleApprove() {
    try {
      setActionLoading(true);
      setError("");
      setMessage("");

      await approveListing(id, reason);

      setMessage("Property approved successfully.");
      setReason("");

      await refreshReview();
    } catch (err) {
      setError(err.message);
    } finally {
      setActionLoading(false);
    }
  }

  async function handleReject() {
    if (!reason.trim()) {
      setError("Please provide a reason before rejecting the listing.");
      return;
    }

    try {
      setActionLoading(true);
      setError("");
      setMessage("");

      await rejectListing(id, reason.trim());

      setMessage("Property rejected.");
      setReason("");

      await refreshReview();
    } catch (err) {
      setError(err.message);
    } finally {
      setActionLoading(false);
    }
  }

  async function handleRevision() {
    if (!reason.trim()) {
      setError(
        "Please explain what the owner needs to revise."
      );
      return;
    }

    try {
      setActionLoading(true);
      setError("");
      setMessage("");

      await requestListingRevision(id, reason.trim());

      setMessage("Revision requested from the property owner.");
      setReason("");

      await refreshReview();
    } catch (err) {
      setError(err.message);
    } finally {
      setActionLoading(false);
    }
  }

  async function handlePublish() {
    try {
      setActionLoading(true);
      setError("");
      setMessage("");

      await publishListing(id);

      setMessage("Property published successfully.");

      await refreshReview();
    } catch (err) {
      setError(err.message);
    } finally {
      setActionLoading(false);
    }
  }

  async function handleUnpublish() {
    try {
      setActionLoading(true);
      setError("");
      setMessage("");

      await unpublishListing(id);

      setMessage("Property unpublished successfully.");

      await refreshReview();
    } catch (err) {
      setError(err.message);
    } finally {
      setActionLoading(false);
    }
  }

  if (loading) {
    return (
      <div className="review-loading">
        Loading property verification...
      </div>
    );
  }

  if (error && !review) {
    return (
      <div className="review-loading">
        <p>{error}</p>

        <button
          type="button"
          onClick={() => navigate("/admin")}
        >
          Return to verification queue
        </button>
      </div>
    );
  }

  if (!review) {
    return null;
  }

  const confidence =
    review.confidence ?? 0;

  const confidencePercentage = Math.round(
    confidence * 100
  );

  const riskScore =
    review.riskScore ?? review.risk_score ?? 0;

  const riskPercentage = Math.round(
    riskScore * 100
  );

  return (
    <div className="admin-review-layout">
      <aside className="review-sidebar">
        <div>
          <img
            src={logoWhite}
            alt="PropMate"
            className="review-logo"
          />

          <p className="review-sidebar-label">
            ADMINISTRATION
          </p>

          <Link to="/admin" className="review-nav-link">
            <span>01</span>
            Verification Queue
          </Link>

          <div className="review-nav-active">
            <span>02</span>
            Property Review
          </div>
        </div>

        <div className="review-sidebar-footer">
          HUMAN REVIEW
          <span>Final decision required</span>
        </div>
      </aside>

      <main className="review-main">
        <div className="review-breadcrumb">
          <Link to="/admin">Verification Queue</Link>
          <span>/</span>
          <span>Property #{review.listingId}</span>
        </div>

        <header className="review-header">
          <div>
            <p className="review-eyebrow">
              PROPERTY VERIFICATION
            </p>

            <h1>{review.title}</h1>

            <p>
              {review.address}, {review.city}
            </p>
          </div>

          <span
            className={`review-status review-status-${review.status?.toLowerCase()}`}
          >
            {formatStatus(review.status)}
          </span>
        </header>

        <section className="review-property-overview">
          <div className="review-property-image">
            {review.imageUrls?.length ? (
              <img
                src={review.imageUrls[0]}
                alt={review.title}
              />
            ) : (
              <div className="review-image-placeholder">
                PROP
              </div>
            )}
          </div>

          <div className="review-property-content">
            <div className="review-property-label">
              LISTING #{review.listingId}
            </div>

            <h2>Property details</h2>

            <p className="review-description">
              {review.description}
            </p>

            <div className="review-property-grid">
              <div>
                <span>PRICE</span>
                <strong>{formatPrice(review.price)}</strong>
              </div>

              <div>
                <span>PURPOSE</span>
                <strong>{review.purpose}</strong>
              </div>

              <div>
                <span>PROPERTY TYPE</span>
                <strong>{review.propertyType}</strong>
              </div>

              <div>
                <span>OWNER ID</span>
                <strong>#{review.ownerId}</strong>
              </div>

              <div>
                <span>BEDROOMS</span>
                <strong>{review.bedrooms}</strong>
              </div>

              <div>
                <span>BATHROOMS</span>
                <strong>{review.bathrooms}</strong>
              </div>
            </div>
          </div>
        </section>

        <section className="ai-review-section">
          <div className="ai-review-heading">
            <div>
              <p className="review-eyebrow">
                AGENTIC AI ASSESSMENT
              </p>

              <h2>Verification intelligence</h2>

              <p>
                Automated findings are advisory. The final
                property decision remains with the
                administrator.
              </p>
            </div>

            <div className="ai-recommendation">
              <span>AI RECOMMENDATION</span>
              <strong>{review.recommendation}</strong>
            </div>
          </div>

          <div className="ai-metrics">
            <div>
              <span>01</span>
              <p>CONFIDENCE</p>
              <strong>{confidencePercentage}%</strong>

              <div className="metric-track">
                <div
                  style={{
                    width: `${Math.min(
                      confidencePercentage,
                      100
                    )}%`,
                  }}
                />
              </div>
            </div>

            <div>
              <span>02</span>
              <p>RISK SCORE</p>
              <strong>{riskPercentage}%</strong>

              <div className="metric-track">
                <div
                  style={{
                    width: `${Math.min(
                      riskPercentage,
                      100
                    )}%`,
                  }}
                />
              </div>
            </div>

            <div>
              <span>03</span>
              <p>HUMAN REVIEW</p>
              <strong>
                {review.requiresHumanReview
                  ? "Required"
                  : "Not Required"}
              </strong>
            </div>

            <div>
              <span>04</span>
              <p>VERIFIED</p>
              <strong>
                {review.verifiedAt
                  ? new Date(
                      review.verifiedAt
                    ).toLocaleDateString()
                  : "—"}
              </strong>
            </div>
          </div>

          <div className="ai-findings-grid">
            <div className="ai-reasons">
              <div className="finding-heading">
                <span>AI ANALYSIS</span>
                <h3>Reasons</h3>
              </div>

              {review.reasons?.length ? (
                review.reasons.map((item, index) => (
                  <div
                    className="reason-item"
                    key={`${item}-${index}`}
                  >
                    <span>
                      {String(index + 1).padStart(2, "0")}
                    </span>
                    <p>{item}</p>
                  </div>
                ))
              ) : (
                <p className="no-findings">
                  No reasons were returned.
                </p>
              )}
            </div>

            <div className="ai-evidence">
              <div className="finding-heading">
                <span>TOOL EXECUTION</span>
                <h3>Verification evidence</h3>
              </div>

              {review.evidence?.length ? (
                review.evidence.map((item, index) => {
                  const toolName =
                    item.toolName ??
                    item.tool_name ??
                    "Verification Check";

                  const evidenceRisk =
                    item.riskScore ??
                    item.risk_score ??
                    0;

                  return (
                    <div
                      className="evidence-item"
                      key={`${toolName}-${index}`}
                    >
                      <div className="evidence-top">
                        <div>
                          <span
                            className={
                              item.passed
                                ? "evidence-dot passed"
                                : "evidence-dot failed"
                            }
                          />

                          <strong>
                            {formatToolName(toolName)}
                          </strong>
                        </div>

                        <span>
                          Risk{" "}
                          {Math.round(evidenceRisk * 100)}%
                        </span>
                      </div>

                      <p>{item.message}</p>
                    </div>
                  );
                })
              ) : (
                <p className="no-findings">
                  No verification evidence was returned.
                </p>
              )}
            </div>
          </div>
        </section>

        <section className="admin-decision-section">
          <div className="decision-heading">
            <p className="review-eyebrow">
              HUMAN DECISION
            </p>

            <h2>Administrator review</h2>

            <p>
              Review the property and AI evidence before
              making the final decision.
            </p>
          </div>

          {message && (
            <div className="review-success">
              {message}
            </div>
          )}

          {error && (
            <div className="review-error">{error}</div>
          )}

          {review.status === "UnderReview" && (
            <>
              <label
                className="decision-label"
                htmlFor="adminReason"
              >
                REVIEW NOTE / DECISION REASON
              </label>

              <textarea
                id="adminReason"
                value={reason}
                onChange={(event) =>
                  setReason(event.target.value)
                }
                placeholder="Add an approval note, or explain why the listing should be revised or rejected..."
                rows="4"
              />

              <div className="decision-actions">
                <button
                  type="button"
                  className="decision-reject"
                  onClick={handleReject}
                  disabled={actionLoading}
                >
                  Reject
                </button>

                <button
                  type="button"
                  className="decision-revision"
                  onClick={handleRevision}
                  disabled={actionLoading}
                >
                  Request Revision
                </button>

                <button
                  type="button"
                  className="decision-approve"
                  onClick={handleApprove}
                  disabled={actionLoading}
                >
                  {actionLoading
                    ? "Processing..."
                    : "Approve Property"}
                </button>
              </div>
            </>
          )}

          {review.status === "Approved" && (
            <div className="publish-panel">
              <div>
                <strong>Property approved</strong>
                <p>
                  The listing has passed administrator
                  review and can now be published.
                </p>
              </div>

              <button
                type="button"
                onClick={handlePublish}
                disabled={actionLoading}
              >
                {actionLoading
                  ? "Publishing..."
                  : "Publish Listing →"}
              </button>
            </div>
          )}

          {review.status === "Published" && (
            <div className="publish-panel">
              <div>
                <strong>Property is live</strong>
                <p>
                  This listing is currently visible in the
                  public property marketplace.
                </p>
              </div>

              <button
                type="button"
                className="unpublish-button"
                onClick={handleUnpublish}
                disabled={actionLoading}
              >
                Unpublish
              </button>
            </div>
          )}

          {review.status === "RevisionRequired" && (
            <div className="decision-complete">
              Revision has been requested from the property
              owner.
            </div>
          )}

          {review.status === "Rejected" && (
            <div className="decision-complete">
              This property has been rejected.
            </div>
          )}

          {review.status === "Unpublished" && (
            <div className="decision-complete">
              This property is currently unpublished.
            </div>
          )}
        </section>
      </main>
    </div>
  );
}

export default AdminReviewListing;