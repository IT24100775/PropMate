import { useState } from "react";
import { useNavigate } from "react-router-dom";
import OwnerSidebar from "../../components/owner/OwnerSidebar";
import { createListing } from "../../services/propertyListingService";
import "./CreateListing.css";

function CreateListing() {
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

  const [imageUrl, setImageUrl] = useState("");
  const [imageUrls, setImageUrls] = useState([]);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const handleChange = (e) => {
    const { name, value } = e.target;

    setFormData((previous) => ({
      ...previous,
      [name]: value,
    }));
  };

  const handlePurposeChange = (purpose) => {
    setFormData((previous) => ({
      ...previous,
      purpose,
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
      previous.filter((_, currentIndex) => currentIndex !== index)
    );
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      const listingData = {
        ...formData,
        price: Number(formData.price),
        bedrooms: Number(formData.bedrooms),
        bathrooms: Number(formData.bathrooms),
        imageUrls,
      };

      await createListing(listingData);

      navigate("/owner/listings");
    } catch (err) {
      setError(err.message || "Unable to create property listing.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="owner-layout">
      <OwnerSidebar />

      <main className="owner-main create-listing-main">
        <header className="create-header">
          <div>
            <button
              type="button"
              className="back-link"
              onClick={() => navigate("/owner")}
            >
              ← Back to overview
            </button>

            <p className="page-eyebrow">NEW PROPERTY LISTING</p>
            <h1>Create a listing</h1>

            <p className="page-description">
              Add your property details and create a draft before
              submitting it for verification.
            </p>
          </div>

          <div className="draft-indicator">
            <span></span>
            DRAFT
          </div>
        </header>

        <form className="listing-form" onSubmit={handleSubmit}>
          {error && <div className="dashboard-error">{error}</div>}

          {/* SECTION 01 */}

          <section className="listing-form-section">
            <div className="form-section-heading">
              <span>01</span>

              <div>
                <p>LISTING TYPE</p>
                <h2>What's this property for?</h2>
              </div>
            </div>

            <div className="purpose-selector">
              <button
                type="button"
                className={formData.purpose === "Sale" ? "selected" : ""}
                onClick={() => handlePurposeChange("Sale")}
              >
                <span className="selector-number">01</span>
                <strong>For Sale</strong>
                <small>List this property for purchase</small>
              </button>

              <button
                type="button"
                className={formData.purpose === "Rent" ? "selected" : ""}
                onClick={() => handlePurposeChange("Rent")}
              >
                <span className="selector-number">02</span>
                <strong>For Rent</strong>
                <small>List this property for rental</small>
              </button>
            </div>
          </section>

          {/* SECTION 02 */}

          <section className="listing-form-section">
            <div className="form-section-heading">
              <span>02</span>

              <div>
                <p>PROPERTY INFORMATION</p>
                <h2>Tell us about the property.</h2>
              </div>
            </div>

            <div className="listing-fields">
              <div className="listing-field full-field">
                <label htmlFor="title">Listing title *</label>

                <input
                  id="title"
                  name="title"
                  type="text"
                  placeholder="e.g. Modern three-bedroom home in Colombo"
                  value={formData.title}
                  onChange={handleChange}
                  minLength={5}
                  maxLength={150}
                  required
                />

                <small>5–150 characters</small>
              </div>

              <div className="listing-field">
                <label htmlFor="propertyType">Property type *</label>

                <select
                  id="propertyType"
                  name="propertyType"
                  value={formData.propertyType}
                  onChange={handleChange}
                  required
                >
                  <option value="House">House</option>
                  <option value="Apartment">Apartment</option>
                  <option value="Land">Land</option>
                  <option value="Commercial">Commercial</option>
                </select>
              </div>

              <div className="listing-field">
                <label htmlFor="price">
                  {formData.purpose === "Rent"
                    ? "Monthly rent (LKR) *"
                    : "Sale price (LKR) *"}
                </label>

                <input
                  id="price"
                  name="price"
                  type="number"
                  min="1"
                  step="1"
                  placeholder="e.g. 25000000"
                  value={formData.price}
                  onChange={handleChange}
                  required
                />
              </div>

              <div className="listing-field full-field">
                <label htmlFor="description">Description *</label>

                <textarea
                  id="description"
                  name="description"
                  placeholder="Describe the property, its features and what makes it stand out..."
                  value={formData.description}
                  onChange={handleChange}
                  minLength={20}
                  maxLength={2000}
                  required
                />

                <div className="field-meta">
                  <small>Minimum 20 characters</small>
                  <small>{formData.description.length}/2000</small>
                </div>
              </div>
            </div>
          </section>

          {/* SECTION 03 */}

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
                <label htmlFor="address">Property address *</label>

                <input
                  id="address"
                  name="address"
                  type="text"
                  placeholder="Street address"
                  value={formData.address}
                  onChange={handleChange}
                  maxLength={250}
                  required
                />
              </div>

              <div className="listing-field">
                <label htmlFor="city">City *</label>

                <input
                  id="city"
                  name="city"
                  type="text"
                  placeholder="e.g. Colombo"
                  value={formData.city}
                  onChange={handleChange}
                  maxLength={100}
                  required
                />
              </div>
            </div>
          </section>

          {/* SECTION 04 */}

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
                <label htmlFor="bedrooms">Bedrooms</label>

                <input
                  id="bedrooms"
                  name="bedrooms"
                  type="number"
                  min="0"
                  max="100"
                  value={formData.bedrooms}
                  onChange={handleChange}
                />
              </div>

              <div className="listing-field">
                <label htmlFor="bathrooms">Bathrooms</label>

                <input
                  id="bathrooms"
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

          {/* SECTION 05 */}

          <section className="listing-form-section">
            <div className="form-section-heading">
              <span>05</span>

              <div>
                <p>PROPERTY IMAGES</p>
                <h2>Show the property.</h2>
              </div>
            </div>

            <div className="image-url-row">
              <div className="listing-field">
                <label htmlFor="imageUrl">Image URL</label>

                <input
                  id="imageUrl"
                  type="url"
                  placeholder="https://example.com/property.jpg"
                  value={imageUrl}
                  onChange={(e) => setImageUrl(e.target.value)}
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

            {imageUrls.length === 0 ? (
              <div className="image-empty">
                <span>+</span>
                <strong>No images added yet</strong>
                <p>Add image URLs to preview them here.</p>
              </div>
            ) : (
              <div className="image-preview-grid">
                {imageUrls.map((url, index) => (
                  <div className="image-preview" key={`${url}-${index}`}>
                    <img
                      src={url}
                      alt={`Property preview ${index + 1}`}
                    />

                    <div className="image-number">
                      {String(index + 1).padStart(2, "0")}
                    </div>

                    <button
                      type="button"
                      onClick={() => removeImage(index)}
                      aria-label="Remove image"
                    >
                      ×
                    </button>
                  </div>
                ))}
              </div>
            )}
          </section>

          <div className="listing-form-actions">
            <div>
              <strong>Ready to save?</strong>
              <span>
                Your property will remain a draft until you submit it
                for verification.
              </span>
            </div>

            <div className="action-buttons">
              <button
                type="button"
                className="cancel-listing-button"
                onClick={() => navigate("/owner")}
              >
                Cancel
              </button>

              <button
                type="submit"
                className="create-draft-button"
                disabled={loading}
              >
                {loading ? "Creating..." : "Create draft"}
                {!loading && <span>→</span>}
              </button>
            </div>
          </div>
        </form>
      </main>
    </div>
  );
}

export default CreateListing;