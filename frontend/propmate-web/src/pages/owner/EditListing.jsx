import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import OwnerSidebar from "../../components/owner/OwnerSidebar";
import {
  getOwnerListingById,
  updateListing,
  submitListing,
} from "../../services/propertyListingService";
import "./CreateListing.css";

function EditListing() {
  const { id } = useParams();
  const navigate = useNavigate();

  const [formData, setFormData] = useState({
    title: "",
    description: "",
    purpose: "Sale",
    propertyType: "House",
    price: "",
    address: "",
    city: "",
    bedrooms: 0,
    bathrooms: 0,
  });

  const [status, setStatus] = useState("");
  const [imageUrls, setImageUrls] = useState([]);
  const [imageUrl, setImageUrl] = useState("");

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    let cancelled = false;

    async function fetchListing() {
      try {
        const listing = await getOwnerListingById(id);

        if (cancelled) return;

        if (
          listing.status !== "Draft" &&
          listing.status !== "RevisionRequired"
        ) {
          setError(
            "This listing cannot currently be edited."
          );
          return;
        }

        setFormData({
          title: listing.title,
          description: listing.description,
          purpose: listing.purpose,
          propertyType: listing.propertyType,
          price: listing.price,
          address: listing.address,
          city: listing.city,
          bedrooms: listing.bedrooms,
          bathrooms: listing.bathrooms,
        });

        setImageUrls(listing.imageUrls || []);
        setStatus(listing.status);
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

    fetchListing();

    return () => {
      cancelled = true;
    };
  }, [id]);

  const handleChange = (e) => {
    const { name, value } = e.target;

    setFormData((previous) => ({
      ...previous,
      [name]: value,
    }));
  };

  const addImage = () => {
    const url = imageUrl.trim();

    if (!url) return;

    if (imageUrls.includes(url)) {
      setError("This image URL has already been added.");
      return;
    }

    setImageUrls((previous) => [...previous, url]);
    setImageUrl("");
    setError("");
  };

  const removeImage = (index) => {
    setImageUrls((previous) =>
      previous.filter(
        (_, currentIndex) => currentIndex !== index
      )
    );
  };

  const buildPayload = () => ({
    ...formData,
    price: Number(formData.price),
    bedrooms: Number(formData.bedrooms),
    bathrooms: Number(formData.bathrooms),
    imageUrls,
  });

  const handleSave = async (e) => {
    e.preventDefault();

    try {
      setSaving(true);
      setError("");

      await updateListing(id, buildPayload());

      navigate("/owner/listings");
    } catch (err) {
      setError(err.message || "Unable to update listing.");
    } finally {
      setSaving(false);
    }
  };

  const handleSaveAndSubmit = async () => {
    try {
      setSaving(true);
      setError("");

      await updateListing(id, buildPayload());
      await submitListing(id);

      navigate("/owner/listings");
    } catch (err) {
      setError(
        err.message || "Unable to submit listing."
      );
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <div className="owner-layout">
        <OwnerSidebar />

        <main className="owner-main">
          <div className="dashboard-empty">
            <p>Loading property...</p>
          </div>
        </main>
      </div>
    );
  }

  return (
    <div className="owner-layout">
      <OwnerSidebar />

      <main className="owner-main create-listing-main">
        <header className="create-header">
          <div>
            <button
              type="button"
              className="back-link"
              onClick={() =>
                navigate("/owner/listings")
              }
            >
              ← Back to my listings
            </button>

            <p className="page-eyebrow">
              PROPERTY LISTING
            </p>

            <h1>Edit listing</h1>

            <p className="page-description">
              Update your property information before
              submitting it for verification.
            </p>
          </div>

          {status && (
            <div className="draft-indicator">
              <span></span>
              {status === "RevisionRequired"
                ? "REVISION REQUIRED"
                : "DRAFT"}
            </div>
          )}
        </header>

        {error && (
          <div className="dashboard-error">
            {error}
          </div>
        )}

        {(status === "Draft" ||
          status === "RevisionRequired") && (
          <form
            className="listing-form"
            onSubmit={handleSave}
          >
            <section className="listing-form-section">
              <div className="form-section-heading">
                <span>01</span>

                <div>
                  <p>LISTING TYPE</p>
                  <h2>What's this property for?</h2>
                </div>
              </div>

              <div className="purpose-selector">
                {["Sale", "Rent"].map((purpose) => (
                  <button
                    key={purpose}
                    type="button"
                    className={
                      formData.purpose === purpose
                        ? "selected"
                        : ""
                    }
                    onClick={() =>
                      setFormData((previous) => ({
                        ...previous,
                        purpose,
                      }))
                    }
                  >
                    <span className="selector-number">
                      {purpose === "Sale" ? "01" : "02"}
                    </span>

                    <strong>
                      For {purpose}
                    </strong>

                    <small>
                      {purpose === "Sale"
                        ? "List this property for purchase"
                        : "List this property for rental"}
                    </small>
                  </button>
                ))}
              </div>
            </section>

            <section className="listing-form-section">
              <div className="form-section-heading">
                <span>02</span>

                <div>
                  <p>PROPERTY INFORMATION</p>
                  <h2>Property details.</h2>
                </div>
              </div>

              <div className="listing-fields">
                <div className="listing-field full-field">
                  <label>Listing title *</label>

                  <input
                    name="title"
                    value={formData.title}
                    onChange={handleChange}
                    minLength={5}
                    maxLength={150}
                    required
                  />
                </div>

                <div className="listing-field">
                  <label>Property type *</label>

                  <select
                    name="propertyType"
                    value={formData.propertyType}
                    onChange={handleChange}
                  >
                    <option value="House">House</option>
                    <option value="Apartment">
                      Apartment
                    </option>
                    <option value="Land">Land</option>
                    <option value="Commercial">
                      Commercial
                    </option>
                  </select>
                </div>

                <div className="listing-field">
                  <label>
                    {formData.purpose === "Rent"
                      ? "Monthly rent (LKR) *"
                      : "Sale price (LKR) *"}
                  </label>

                  <input
                    name="price"
                    type="number"
                    min="1"
                    value={formData.price}
                    onChange={handleChange}
                    required
                  />
                </div>

                <div className="listing-field full-field">
                  <label>Description *</label>

                  <textarea
                    name="description"
                    value={formData.description}
                    onChange={handleChange}
                    minLength={20}
                    maxLength={2000}
                    required
                  />

                  <div className="field-meta">
                    <small>Minimum 20 characters</small>
                    <small>
                      {formData.description.length}/2000
                    </small>
                  </div>
                </div>
              </div>
            </section>

            <section className="listing-form-section">
              <div className="form-section-heading">
                <span>03</span>

                <div>
                  <p>LOCATION</p>
                  <h2>Where is it located?</h2>
                </div>
              </div>

              <div className="listing-fields">
                <div className="listing-field full-field">
                  <label>Property address *</label>

                  <input
                    name="address"
                    value={formData.address}
                    onChange={handleChange}
                    maxLength={250}
                    required
                  />
                </div>

                <div className="listing-field">
                  <label>City *</label>

                  <input
                    name="city"
                    value={formData.city}
                    onChange={handleChange}
                    maxLength={100}
                    required
                  />
                </div>
              </div>
            </section>

            <section className="listing-form-section">
              <div className="form-section-heading">
                <span>04</span>

                <div>
                  <p>PROPERTY DETAILS</p>
                  <h2>Add the essentials.</h2>
                </div>
              </div>

              <div className="listing-fields">
                <div className="listing-field">
                  <label>Bedrooms</label>

                  <input
                    name="bedrooms"
                    type="number"
                    min="0"
                    max="100"
                    value={formData.bedrooms}
                    onChange={handleChange}
                  />
                </div>

                <div className="listing-field">
                  <label>Bathrooms</label>

                  <input
                    name="bathrooms"
                    type="number"
                    min="0"
                    max="100"
                    value={formData.bathrooms}
                    onChange={handleChange}
                  />
                </div>
              </div>
            </section>

            <section className="listing-form-section">
              <div className="form-section-heading">
                <span>05</span>

                <div>
                  <p>PROPERTY IMAGES</p>
                  <h2>Manage images.</h2>
                </div>
              </div>

              <div>
                <div className="image-url-row">
                  <div className="listing-field">
                    <label>Image URL</label>

                    <input
                      type="url"
                      placeholder="https://example.com/property.jpg"
                      value={imageUrl}
                      onChange={(e) =>
                        setImageUrl(e.target.value)
                      }
                    />
                  </div>

                  <button
                    type="button"
                    className="add-image-button"
                    onClick={addImage}
                  >
                    + Add image
                  </button>
                </div>

                {imageUrls.length > 0 && (
                  <div className="image-preview-grid">
                    {imageUrls.map((url, index) => (
                      <div
                        className="image-preview"
                        key={`${url}-${index}`}
                      >
                        <img
                          src={url}
                          alt={`Property ${index + 1}`}
                        />

                        <div className="image-number">
                          {String(index + 1).padStart(
                            2,
                            "0"
                          )}
                        </div>

                        <button
                          type="button"
                          onClick={() =>
                            removeImage(index)
                          }
                        >
                          ×
                        </button>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </section>

            <div className="listing-form-actions">
              <div>
                <strong>
                  {status === "RevisionRequired"
                    ? "Finished your revision?"
                    : "Keep working or submit."}
                </strong>

                <span>
                  You can save your changes before
                  verification.
                </span>
              </div>

              <div className="action-buttons">
                <button
                  type="submit"
                  className="cancel-listing-button"
                  disabled={saving}
                >
                  {saving ? "Saving..." : "Save changes"}
                </button>

                <button
                  type="button"
                  className="create-draft-button"
                  disabled={saving}
                  onClick={handleSaveAndSubmit}
                >
                  {saving
                    ? "Processing..."
                    : "Save & submit"}
                  {!saving && <span>→</span>}
                </button>
              </div>
            </div>
          </form>
        )}
      </main>
    </div>
  );
}

export default EditListing;